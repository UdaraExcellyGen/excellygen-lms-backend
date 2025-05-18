// ExcellyGenLMS.Application/Services/Learner/ForumService.cs
using ExcellyGenLMS.Core.Entities.Learner;
using ExcellyGenLMS.Core.Interfaces.Repositories.Learner;
using ExcellyGenLMS.Core.Interfaces.Repositories.Auth;
using ExcellyGenLMS.Application.Interfaces.Learner;
using ExcellyGenLMS.Application.DTOs.Learner;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using ExcellyGenLMS.Core.Entities.Auth;
using System.Linq.Expressions; // Required for Expression, Func, etc.
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.Extensions.Logging;

namespace ExcellyGenLMS.Application.Services.Learner
{
    public class ForumService : IForumService
    {
        private readonly IForumThreadRepository _threadRepository;
        private readonly IThreadCommentRepository _commentRepository;
        private readonly IThreadComReplyRepository _replyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ForumService> _logger;
        private readonly string? _currentUserId;

        public ForumService(
            IForumThreadRepository threadRepository,
            IThreadCommentRepository commentRepository,
            IThreadComReplyRepository replyRepository,
            IUserRepository userRepository,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ForumService> logger)
        {
            _threadRepository = threadRepository;
            _commentRepository = commentRepository;
            _replyRepository = replyRepository;
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private AuthorDto MapUserToAuthorDto(User? user)
        {
            if (user == null) return new AuthorDto { Id = "unknown", Name = "Unknown User", Avatar = null };
            return new AuthorDto { Id = user.Id, Name = user.Name, Avatar = user.Avatar };
        }

        private ForumThreadDto MapEntityToForumThreadDto(ForumThread t)
        {
            var createdAtUtc = DateTime.SpecifyKind(t.CreatedAt, DateTimeKind.Utc);
            return new ForumThreadDto
            {
                Id = t.Id,
                Title = t.Title,
                Content = t.Content?.Length > 150 ? t.Content.Substring(0, 150) + "..." : t.Content ?? "",
                Category = t.Category,
                ImageUrl = t.ImageUrl,
                CreatedAt = createdAtUtc,
                Author = MapUserToAuthorDto(t.Creator),
                CommentsCount = t.Comments?.Count ?? 0,
                IsCurrentUserAuthor = t.CreatorId == _currentUserId
            };
        }

        private ForumThreadDto MapEntityToFullForumThreadDto(ForumThread t)
        {
            var createdAtUtc = DateTime.SpecifyKind(t.CreatedAt, DateTimeKind.Utc);
            return new ForumThreadDto
            {
                Id = t.Id,
                Title = t.Title,
                Content = t.Content,
                Category = t.Category,
                ImageUrl = t.ImageUrl,
                CreatedAt = createdAtUtc,
                Author = MapUserToAuthorDto(t.Creator),
                CommentsCount = t.Comments?.Count ?? 0,
                IsCurrentUserAuthor = t.CreatorId == _currentUserId
            };
        }

        private ThreadCommentDto MapEntityToThreadCommentDto(ThreadComment c)
        {
            var createdAtUtc = DateTime.SpecifyKind(c.CreatedAt, DateTimeKind.Utc);
            return new ThreadCommentDto
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = createdAtUtc,
                Author = MapUserToAuthorDto(c.Commentor),
                ThreadId = c.ThreadId,
                RepliesCount = c.Replies?.Count ?? 0,
                IsCurrentUserAuthor = c.CommentorId == _currentUserId
            };
        }

        private ThreadReplyDto MapEntityToThreadReplyDto(ThreadComReply r)
        {
            var createdAtUtc = DateTime.SpecifyKind(r.CreatedAt, DateTimeKind.Utc);
            return new ThreadReplyDto
            {
                Id = r.Id,
                Content = r.Content,
                CreatedAt = createdAtUtc,
                Author = MapUserToAuthorDto(r.Commentor),
                CommentId = r.CommentId,
                IsCurrentUserAuthor = r.CommentorId == _currentUserId
            };
        }

        public async Task<PagedResult<ForumThreadDto>> GetThreadsAsync(ForumQueryParams queryParams)
        {
            Expression<Func<ForumThread, bool>> filter = t => true; // Start with a predicate that's always true

            if (queryParams.MyThreads && !string.IsNullOrEmpty(_currentUserId))
            {
                // Note: The variable 'filter' needs to be assigned the result of .And()
                filter = filter.And(t => t.CreatorId == _currentUserId); // This line needs PredicateBuilder
            }
            else if (queryParams.MyThreads)
            {
                return new PagedResult<ForumThreadDto> { /* empty */ };
            }

            if (!string.IsNullOrEmpty(queryParams.SearchTerm))
            {
                string term = queryParams.SearchTerm.ToLower();
                // Define the search predicate separately for clarity
                Expression<Func<ForumThread, bool>> searchFilter = t =>
                    (t.Title != null && t.Title.ToLower().Contains(term)) ||
                    (t.Content != null && t.Content.ToLower().Contains(term));
                filter = filter.And(searchFilter); // This line needs PredicateBuilder
            }
            if (!string.IsNullOrEmpty(queryParams.Category) && queryParams.Category.ToLower() != "all")
            {
                // Define the category predicate separately
                string categoryLower = queryParams.Category.ToLower();
                Expression<Func<ForumThread, bool>> categoryFilter = t =>
                    t.Category != null && t.Category.ToLower() == categoryLower;
                filter = filter.And(categoryFilter); // This line needs PredicateBuilder
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

        // GetThreadByIdAsync, CreateThreadAsync, UpdateThreadAsync, DeleteThreadAsync,
        // GetCommentsForThreadAsync, GetCommentByIdAsync, CreateCommentAsync, UpdateCommentAsync, DeleteCommentAsync,
        // GetRepliesForCommentAsync, GetReplyByIdAsync, CreateReplyAsync, UpdateReplyAsync, DeleteReplyAsync
        // remain the same as in modal_15 (the version with int parsing and using updated mapping functions)
        public async Task<ForumThreadDto?> GetThreadByIdAsync(string threadIdString)
        {
            if (!int.TryParse(threadIdString, out int threadId)) return null;
            var thread = await _threadRepository.GetByIdAsync(threadId, "Creator,Comments");
            if (thread == null) return null;
            return MapEntityToFullForumThreadDto(thread);
        }

        public async Task<ForumThreadDto> CreateThreadAsync(CreateForumThreadDto createDto)
        {
            if (string.IsNullOrEmpty(_currentUserId)) throw new UnauthorizedAccessException("User not authenticated.");
            var thread = new ForumThread { Title = createDto.Title, Content = createDto.Content, Category = createDto.Category, ImageUrl = createDto.ImageUrl, CreatorId = _currentUserId, CreatedAt = DateTime.UtcNow };
            await _threadRepository.AddAsync(thread);
            thread.Creator = await _userRepository.GetUserByIdAsync(_currentUserId);
            return MapEntityToFullForumThreadDto(thread);
        }

        public async Task<ForumThreadDto?> UpdateThreadAsync(string threadIdString, UpdateForumThreadDto updateDto)
        {
            if (!int.TryParse(threadIdString, out int threadId)) return null;
            var thread = await _threadRepository.GetByIdAsync(threadId, "Creator");
            if (thread == null) throw new KeyNotFoundException("Thread not found.");
            if (thread.CreatorId != _currentUserId) throw new UnauthorizedAccessException("Only the author can edit this thread.");
            thread.Title = updateDto.Title; thread.Content = updateDto.Content; thread.Category = updateDto.Category; thread.ImageUrl = updateDto.ImageUrl;
            await _threadRepository.UpdateAsync(thread);
            return MapEntityToFullForumThreadDto(thread);
        }

        public async Task<bool> DeleteThreadAsync(string threadIdString)
        {
            if (!int.TryParse(threadIdString, out int threadId)) return false;
            var thread = await _threadRepository.GetByIdAsync(threadId);
            if (thread == null) throw new KeyNotFoundException("Thread not found.");
            if (thread.CreatorId != _currentUserId) throw new UnauthorizedAccessException("Only the author can delete this thread.");
            await _threadRepository.DeleteAsync(thread);
            return true;
        }

        public async Task<IEnumerable<ThreadCommentDto>> GetCommentsForThreadAsync(string threadIdString)
        {
            if (!int.TryParse(threadIdString, out int threadId)) return Enumerable.Empty<ThreadCommentDto>();
            var comments = await _commentRepository.GetAllAsync(filter: c => c.ThreadId == threadId, orderBy: q => q.OrderByDescending(c => c.CreatedAt), includeProperties: "Commentor,Replies");
            return comments.Select(MapEntityToThreadCommentDto);
        }

        public async Task<ThreadCommentDto?> GetCommentByIdAsync(string commentIdString)
        {
            if (!int.TryParse(commentIdString, out int commentId)) return null;
            var comment = await _commentRepository.GetByIdAsync(commentId, "Commentor,Replies");
            if (comment == null) return null;
            return MapEntityToThreadCommentDto(comment);
        }

        public async Task<ThreadCommentDto> CreateCommentAsync(string threadIdString, CreateThreadCommentDto createDto)
        {
            if (!int.TryParse(threadIdString, out int threadIdValue)) throw new ArgumentException("Invalid thread ID format.", nameof(threadIdString));
            if (string.IsNullOrEmpty(_currentUserId)) throw new UnauthorizedAccessException("User not authenticated.");
            if (!await _threadRepository.ExistsAsync(threadIdValue)) throw new KeyNotFoundException("Thread not found.");
            var comment = new ThreadComment { Content = createDto.Content, ThreadId = threadIdValue, CommentorId = _currentUserId, CreatedAt = DateTime.UtcNow };
            await _commentRepository.AddAsync(comment);
            comment.Commentor = await _userRepository.GetUserByIdAsync(_currentUserId);
            return MapEntityToThreadCommentDto(comment);
        }

        public async Task<ThreadCommentDto?> UpdateCommentAsync(string commentIdString, UpdateThreadCommentDto updateDto)
        {
            if (!int.TryParse(commentIdString, out int commentId)) return null;
            var comment = await _commentRepository.GetByIdAsync(commentId, "Commentor");
            if (comment == null) throw new KeyNotFoundException("Comment not found.");
            if (comment.CommentorId != _currentUserId) throw new UnauthorizedAccessException("Only the author can edit this comment.");
            comment.Content = updateDto.Content;
            await _commentRepository.UpdateAsync(comment);
            return MapEntityToThreadCommentDto(comment);
        }

        public async Task<bool> DeleteCommentAsync(string commentIdString)
        {
            if (!int.TryParse(commentIdString, out int commentId)) return false;
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null) throw new KeyNotFoundException("Comment not found.");
            if (comment.CommentorId != _currentUserId) throw new UnauthorizedAccessException("Only the author can delete this comment.");
            await _commentRepository.DeleteAsync(comment);
            return true;
        }

        public async Task<IEnumerable<ThreadReplyDto>> GetRepliesForCommentAsync(string commentIdString)
        {
            if (!int.TryParse(commentIdString, out int commentId)) return Enumerable.Empty<ThreadReplyDto>();
            var replies = await _replyRepository.GetAllAsync(filter: r => r.CommentId == commentId, orderBy: q => q.OrderBy(r => r.CreatedAt), includeProperties: "Commentor");
            return replies.Select(MapEntityToThreadReplyDto);
        }

        public async Task<ThreadReplyDto?> GetReplyByIdAsync(string replyIdString)
        {
            if (!int.TryParse(replyIdString, out int replyId)) return null;
            var reply = await _replyRepository.GetByIdAsync(replyId, "Commentor");
            if (reply == null) return null;
            return MapEntityToThreadReplyDto(reply);
        }

        public async Task<ThreadReplyDto> CreateReplyAsync(string commentIdString, CreateThreadReplyDto createDto)
        {
            if (!int.TryParse(commentIdString, out int commentIdValue)) throw new ArgumentException("Invalid comment ID format.", nameof(commentIdString));
            if (string.IsNullOrEmpty(_currentUserId)) throw new UnauthorizedAccessException("User not authenticated.");
            if (!await _commentRepository.ExistsAsync(commentIdValue)) throw new KeyNotFoundException("Comment not found.");
            var reply = new ThreadComReply { Content = createDto.Content, CommentId = commentIdValue, CommentorId = _currentUserId, CreatedAt = DateTime.UtcNow };
            await _replyRepository.AddAsync(reply);
            reply.Commentor = await _userRepository.GetUserByIdAsync(_currentUserId);
            return MapEntityToThreadReplyDto(reply);
        }

        public async Task<ThreadReplyDto?> UpdateReplyAsync(string replyIdString, UpdateThreadReplyDto updateDto)
        {
            if (!int.TryParse(replyIdString, out int replyId)) return null;
            var reply = await _replyRepository.GetByIdAsync(replyId, "Commentor");
            if (reply == null) throw new KeyNotFoundException("Reply not found.");
            if (reply.CommentorId != _currentUserId) throw new UnauthorizedAccessException("Only the author can edit this reply.");
            reply.Content = updateDto.Content;
            await _replyRepository.UpdateAsync(reply);
            return MapEntityToThreadReplyDto(reply);
        }

        public async Task<bool> DeleteReplyAsync(string replyIdString)
        {
            if (!int.TryParse(replyIdString, out int replyId)) return false;
            var reply = await _replyRepository.GetByIdAsync(replyId);
            if (reply == null) throw new KeyNotFoundException("Reply not found.");
            if (reply.CommentorId != _currentUserId) throw new UnauthorizedAccessException("Only the author can delete this reply.");
            await _replyRepository.DeleteAsync(reply);
            return true;
        }
    }

    // ----- INCLUDE PREDICATEBUILDER HERE -----
    // (Or move to a shared Utilities/Extensions folder and add a 'using' statement)
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
                // ReSharper disable once RedundantAssignment
                if (_map.TryGetValue(p, out var replacement))
                {
                    p = replacement!; // Use null-forgiving operator if necessary after TryGetValue
                }
                return base.VisitParameter(p);
            }
        }
    }
}