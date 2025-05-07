using ExcellyGenLMS.Application.DTOs.Learner;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExcellyGenLMS.Application.Interfaces.Learner
{
    public interface IForumService
    {
        Task<PagedResult<ForumThreadDto>> GetThreadsAsync(ForumQueryParams queryParams); // Updated to PagedResult
        Task<ForumThreadDto?> GetThreadByIdAsync(string threadId);
        Task<ForumThreadDto> CreateThreadAsync(CreateForumThreadDto createDto);
        Task<ForumThreadDto?> UpdateThreadAsync(string threadId, UpdateForumThreadDto updateDto);
        Task<bool> DeleteThreadAsync(string threadId);

        Task<IEnumerable<ThreadCommentDto>> GetCommentsForThreadAsync(string threadId);
        Task<ThreadCommentDto?> GetCommentByIdAsync(string commentId);
        Task<ThreadCommentDto> CreateCommentAsync(string threadId, CreateThreadCommentDto createDto);
        Task<ThreadCommentDto?> UpdateCommentAsync(string commentId, UpdateThreadCommentDto updateDto);
        Task<bool> DeleteCommentAsync(string commentId);

        Task<IEnumerable<ThreadReplyDto>> GetRepliesForCommentAsync(string commentId);
        Task<ThreadReplyDto?> GetReplyByIdAsync(string replyId);
        Task<ThreadReplyDto> CreateReplyAsync(string commentId, CreateThreadReplyDto createDto);
        Task<ThreadReplyDto?> UpdateReplyAsync(string replyId, UpdateThreadReplyDto updateDto);
        Task<bool> DeleteReplyAsync(string replyId);
    }
}