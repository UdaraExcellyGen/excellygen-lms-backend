using ExcellyGenLMS.Core.Entities.Learner;
using ExcellyGenLMS.Core.Interfaces.Repositories.Learner;
using ExcellyGenLMS.Core.Interfaces.Repositories.Auth;
using ExcellyGenLMS.Application.Interfaces.Learner;
using ExcellyGenLMS.Application.DTOs.Learner; // Assuming ForumDtos.cs (and thus all DTOs) are in this namespace
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using ExcellyGenLMS.Core.Entities.Auth;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Services.Learner
{
    public class ForumService : IForumService
    {
        private readonly IForumThreadRepository _threadRepository;
        private readonly IThreadCommentRepository _commentRepository;
        private readonly IThreadComReplyRepository _replyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string? _currentUserId; // This will be the User.Id (string) from claims

        public ForumService(
            IForumThreadRepository threadRepository,
            IThreadCommentRepository commentRepository,
            IThreadComReplyRepository replyRepository,
            IUserRepository userRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _threadRepository = threadRepository;
            _commentRepository = commentRepository;
            _replyRepository = replyRepository;
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
            _currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private AuthorDto MapUserToAuthorDto(User? user)
        {
            if (user == null) return new AuthorDto { Id = "unknown", Name = "Unknown User", Avatar = null };
            return new AuthorDto
            {
                Id = user.Id, // User.Id is string
                Name = user.Name,
                Avatar = user.Avatar
            };
        }

        private ForumThreadDto MapEntityToForumThreadDto(ForumThread t)
        {
            return new ForumThreadDto
            {
                Id = t.Id,
                Title = t.Title,
                Content = t.Content?.Length > 150 ? t.Content.Substring(0, 150) + "..." : t.Content ?? "",
                Category = t.Category,
                ImageUrl = t.ImageUrl,
                CreatedAt = t.CreatedAt,
                Author = MapUserToAuthorDto(t.Creator),
                CommentsCount = t.Comments?.Count ?? 0,
                IsCurrentUserAuthor = t.CreatorId == _currentUserId
            };
        }

        private ForumThreadDto MapEntityToFullForumThreadDto(ForumThread t)
        {
            return new ForumThreadDto
            {
                Id = t.Id,
                Title = t.Title,
                Content = t.Content,
                Category = t.Category,
                ImageUrl = t.ImageUrl,
                CreatedAt = t.CreatedAt,
                Author = MapUserToAuthorDto(t.Creator),
                CommentsCount = t.Comments?.Count ?? 0,
                IsCurrentUserAuthor = t.CreatorId == _currentUserId
            };
        }

        public async Task<PagedResult<ForumThreadDto>> GetThreadsAsync(ForumQueryParams queryParams)
        {
            Expression<Func<ForumThread, bool>> filter = t => true;

            if (queryParams.MyThreads && !string.IsNullOrEmpty(_currentUserId))
            {
                filter = filter.And(t => t.CreatorId == _currentUserId);
            }
            else if (queryParams.MyThreads)
            {
                return new PagedResult<ForumThreadDto>
                {
                    Items = Enumerable.Empty<ForumThreadDto>(),
                    PageNumber = queryParams.PageNumber,
                    PageSize = queryParams.PageSize,
                    TotalCount = 0
                };
            }

            if (!string.IsNullOrEmpty(queryParams.SearchTerm))
            {
                string term = queryParams.SearchTerm.ToLower();
                Expression<Func<ForumThread, bool>> searchFilter = t =>
                    (t.Title != null && t.Title.ToLower().Contains(term)) ||
                    (t.Content != null && t.Content.ToLower().Contains(term));
                filter = filter.And(searchFilter);
            }

            if (!string.IsNullOrEmpty(queryParams.Category) && queryParams.Category.ToLower() != "all")
            {
                Expression<Func<ForumThread, bool>> categoryFilter = t => t.Category != null && t.Category.ToLower() == queryParams.Category.ToLower();
                filter = filter.And(categoryFilter);
            }

            var totalCount = await _threadRepository.CountThreadsAsync(filter);

            var threadEntities = await _threadRepository.GetThreadsAsync(
                filter,
                orderBy: q => q.OrderByDescending(t => t.CreatedAt),
                skip: (queryParams.PageNumber - 1) * queryParams.PageSize,
                take: queryParams.PageSize,
                includeProperties: "Creator,Comments"
            );

            var threadDtos = threadEntities.Select(MapEntityToForumThreadDto).ToList();

            return new PagedResult<ForumThreadDto>
            {
                Items = threadDtos,
                PageNumber = queryParams.PageNumber,
                PageSize = queryParams.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<ForumThreadDto?> GetThreadByIdAsync(string threadIdString)
        {
            if (!int.TryParse(threadIdString, out int threadId))
            {
                return null;
            }
            var thread = await _threadRepository.GetByIdAsync(threadId, "Creator,Comments"); // Pass int
            if (thread == null) return null;
            return MapEntityToFullForumThreadDto(thread);
        }

        public async Task<ForumThreadDto> CreateThreadAsync(CreateForumThreadDto createDto)
        {
            if (string.IsNullOrEmpty(_currentUserId)) throw new UnauthorizedAccessException("User not authenticated.");

            var thread = new ForumThread
            {
                Title = createDto.Title,
                Content = createDto.Content,
                Category = createDto.Category,
                ImageUrl = createDto.ImageUrl,
                CreatorId = _currentUserId,
                CreatedAt = DateTime.UtcNow
            };

            await _threadRepository.AddAsync(thread);
            thread.Creator = await _userRepository.GetUserByIdAsync(_currentUserId);
            return MapEntityToFullForumThreadDto(thread);
        }

        public async Task<ForumThreadDto?> UpdateThreadAsync(string threadIdString, UpdateForumThreadDto updateDto)
        {
            if (!int.TryParse(threadIdString, out int threadId))
            {
                return null;
            }
            var thread = await _threadRepository.GetByIdAsync(threadId, "Creator"); // Pass int
            if (thread == null) throw new KeyNotFoundException("Thread not found.");
            if (thread.CreatorId != _currentUserId) throw new UnauthorizedAccessException("Only the author can edit this thread.");

            thread.Title = updateDto.Title;
            thread.Content = updateDto.Content;
            thread.Category = updateDto.Category;
            thread.ImageUrl = updateDto.ImageUrl;

            await _threadRepository.UpdateAsync(thread);
            return MapEntityToFullForumThreadDto(thread);
        }

        public async Task<bool> DeleteThreadAsync(string threadIdString)
        {
            if (!int.TryParse(threadIdString, out int threadId))
            {
                return false;
            }
            var thread = await _threadRepository.GetByIdAsync(threadId); // Pass int
            if (thread == null) throw new KeyNotFoundException("Thread not found.");
            if (thread.CreatorId != _currentUserId) throw new UnauthorizedAccessException("Only the author can delete this thread.");

            await _threadRepository.DeleteAsync(thread);
            return true;
        }

        private ThreadCommentDto MapEntityToThreadCommentDto(ThreadComment c)
        {
            return new ThreadCommentDto
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                Author = MapUserToAuthorDto(c.Commentor),
                ThreadId = c.ThreadId,
                RepliesCount = c.Replies?.Count ?? 0,
                IsCurrentUserAuthor = c.CommentorId == _currentUserId
            };
        }

        public async Task<IEnumerable<ThreadCommentDto>> GetCommentsForThreadAsync(string threadIdString)
        {
            if (!int.TryParse(threadIdString, out int threadId))
            {
                return Enumerable.Empty<ThreadCommentDto>();
            }
            var comments = await _commentRepository.GetAllAsync(
                filter: c => c.ThreadId == threadId, // Compare int to int
                orderBy: q => q.OrderByDescending(c => c.CreatedAt),
                includeProperties: "Commentor,Replies"
            );
            return comments.Select(MapEntityToThreadCommentDto);
        }

        public async Task<ThreadCommentDto?> GetCommentByIdAsync(string commentIdString)
        {
            if (!int.TryParse(commentIdString, out int commentId))
            {
                return null;
            }
            var comment = await _commentRepository.GetByIdAsync(commentId, "Commentor,Replies"); // Pass int
            if (comment == null) return null;
            return MapEntityToThreadCommentDto(comment);
        }

        public async Task<ThreadCommentDto> CreateCommentAsync(string threadIdString, CreateThreadCommentDto createDto)
        {
            if (!int.TryParse(threadIdString, out int threadIdValue))
            {
                throw new ArgumentException("Invalid thread ID format.", nameof(threadIdString));
            }
            if (string.IsNullOrEmpty(_currentUserId)) throw new UnauthorizedAccessException("User not authenticated.");
            if (!await _threadRepository.ExistsAsync(threadIdValue)) throw new KeyNotFoundException("Thread not found."); // Pass int

            var comment = new ThreadComment
            {
                Content = createDto.Content,
                ThreadId = threadIdValue,
                CommentorId = _currentUserId,
                CreatedAt = DateTime.UtcNow
            };
            await _commentRepository.AddAsync(comment);
            comment.Commentor = await _userRepository.GetUserByIdAsync(_currentUserId);
            return MapEntityToThreadCommentDto(comment);
        }

        public async Task<ThreadCommentDto?> UpdateCommentAsync(string commentIdString, UpdateThreadCommentDto updateDto)
        {
            if (!int.TryParse(commentIdString, out int commentId))
            {
                return null;
            }
            var comment = await _commentRepository.GetByIdAsync(commentId, "Commentor"); // Pass int
            if (comment == null) throw new KeyNotFoundException("Comment not found.");
            if (comment.CommentorId != _currentUserId) throw new UnauthorizedAccessException("Only the author can edit this comment.");

            comment.Content = updateDto.Content;
            await _commentRepository.UpdateAsync(comment);
            return MapEntityToThreadCommentDto(comment);
        }

        public async Task<bool> DeleteCommentAsync(string commentIdString)
        {
            if (!int.TryParse(commentIdString, out int commentId))
            {
                return false;
            }
            var comment = await _commentRepository.GetByIdAsync(commentId); // Pass int
            if (comment == null) throw new KeyNotFoundException("Comment not found.");
            if (comment.CommentorId != _currentUserId) throw new UnauthorizedAccessException("Only the author can delete this comment.");

            await _commentRepository.DeleteAsync(comment);
            return true;
        }

        private ThreadReplyDto MapEntityToThreadReplyDto(ThreadComReply r)
        {
            return new ThreadReplyDto
            {
                Id = r.Id,
                Content = r.Content,
                CreatedAt = r.CreatedAt,
                Author = MapUserToAuthorDto(r.Commentor),
                CommentId = r.CommentId,
                IsCurrentUserAuthor = r.CommentorId == _currentUserId
            };
        }

        public async Task<IEnumerable<ThreadReplyDto>> GetRepliesForCommentAsync(string commentIdString)
        {
            if (!int.TryParse(commentIdString, out int commentId))
            {
                return Enumerable.Empty<ThreadReplyDto>();
            }
            var replies = await _replyRepository.GetAllAsync(
                filter: r => r.CommentId == commentId, // Compare int to int
                orderBy: q => q.OrderBy(r => r.CreatedAt),
                includeProperties: "Commentor"
            );
            return replies.Select(MapEntityToThreadReplyDto);
        }

        public async Task<ThreadReplyDto?> GetReplyByIdAsync(string replyIdString)
        {
            if (!int.TryParse(replyIdString, out int replyId))
            {
                return null;
            }
            var reply = await _replyRepository.GetByIdAsync(replyId, "Commentor"); // Pass int
            if (reply == null) return null;
            return MapEntityToThreadReplyDto(reply);
        }

        public async Task<ThreadReplyDto> CreateReplyAsync(string commentIdString, CreateThreadReplyDto createDto)
        {
            if (!int.TryParse(commentIdString, out int commentIdValue))
            {
                throw new ArgumentException("Invalid comment ID format.", nameof(commentIdString));
            }
            if (string.IsNullOrEmpty(_currentUserId)) throw new UnauthorizedAccessException("User not authenticated.");
            if (!await _commentRepository.ExistsAsync(commentIdValue)) throw new KeyNotFoundException("Comment not found."); // Pass int

            var reply = new ThreadComReply
            {
                Content = createDto.Content,
                CommentId = commentIdValue,
                CommentorId = _currentUserId,
                CreatedAt = DateTime.UtcNow
            };
            await _replyRepository.AddAsync(reply);
            reply.Commentor = await _userRepository.GetUserByIdAsync(_currentUserId);
            return MapEntityToThreadReplyDto(reply);
        }

        public async Task<ThreadReplyDto?> UpdateReplyAsync(string replyIdString, UpdateThreadReplyDto updateDto)
        {
            if (!int.TryParse(replyIdString, out int replyId))
            {
                return null;
            }
            var reply = await _replyRepository.GetByIdAsync(replyId, "Commentor"); // Pass int
            if (reply == null) throw new KeyNotFoundException("Reply not found.");
            if (reply.CommentorId != _currentUserId) throw new UnauthorizedAccessException("Only the author can edit this reply.");

            reply.Content = updateDto.Content;
            await _replyRepository.UpdateAsync(reply);
            return MapEntityToThreadReplyDto(reply);
        }

        public async Task<bool> DeleteReplyAsync(string replyIdString)
        {
            if (!int.TryParse(replyIdString, out int replyId))
            {
                return false;
            }
            var reply = await _replyRepository.GetByIdAsync(replyId); // Pass int
            if (reply == null) throw new KeyNotFoundException("Reply not found.");
            if (reply.CommentorId != _currentUserId) throw new UnauthorizedAccessException("Only the author can delete this reply.");

            await _replyRepository.DeleteAsync(reply);
            return true;
        }
    }

    public static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.AndAlso);
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            return first.Compose(second, Expression.OrElse);
        }

        private static Expression<Func<T, bool>> Compose<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second, Func<Expression, Expression, Expression> merge)
        {
            var map = first.Parameters.Select((f, i) => new { f, s = second.Parameters[i] }).ToDictionary(p => p.s, p => p.f);
            var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);
            return Expression.Lambda<Func<T, bool>>(merge(first.Body, secondBody), first.Parameters);
        }

        private class ParameterRebinder : ExpressionVisitor
        {
            private readonly Dictionary<ParameterExpression, ParameterExpression> _map;

            private ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
            {
                _map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
            }

            public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
            {
                return new ParameterRebinder(map).Visit(exp);
            }

            protected override Expression VisitParameter(ParameterExpression p)
            {
                if (_map.TryGetValue(p, out var replacement))
                {
                    p = replacement;
                }
                return base.VisitParameter(p);
            }
        }
    }
}