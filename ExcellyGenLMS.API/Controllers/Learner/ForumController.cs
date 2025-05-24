using ExcellyGenLMS.Application.DTOs.Learner;
using ExcellyGenLMS.Application.Interfaces.Learner;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace ExcellyGenLMS.API.Controllers.Learner
{
    [Route("api/forum")]
    [ApiController]
    [Authorize] // Most actions require user to be logged in
    public class ForumController : ControllerBase
    {
        private readonly IForumService _forumService;
        private readonly ILogger<ForumController> _logger;

        public ForumController(IForumService forumService, ILogger<ForumController> logger)
        {
            _forumService = forumService;
            _logger = logger;
        }

        [HttpGet("threads")]
        public async Task<ActionResult<PagedResult<ForumThreadDto>>> GetThreads([FromQuery] ForumQueryParams queryParams)
        {
            try
            {
                var threads = await _forumService.GetThreadsAsync(queryParams);
                return Ok(threads);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting threads.");
                return StatusCode(500, new { message = "An error occurred while fetching threads." });
            }
        }

        [HttpGet("threads/{id}")]
        public async Task<ActionResult<ForumThreadDto>> GetThread(string id)
        {
            try
            {
                var thread = await _forumService.GetThreadByIdAsync(id);
                if (thread == null) return NotFound(new { message = "Thread not found." });
                return Ok(thread);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting thread with id {id}.");
                return StatusCode(500, new { message = "An error occurred while fetching the thread." });
            }
        }

        [HttpPost("threads")]
        public async Task<ActionResult<ForumThreadDto>> CreateThread([FromBody] CreateForumThreadDto createDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var createdThread = await _forumService.CreateThreadAsync(createDto);
                return CreatedAtAction(nameof(GetThread), new { id = createdThread.Id }, createdThread);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized thread creation: {ex.Message}");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating thread.");
                return StatusCode(500, new { message = "An error occurred while creating the thread." });
            }
        }

        [HttpPut("threads/{id}")]
        public async Task<ActionResult<ForumThreadDto>> UpdateThread(string id, [FromBody] UpdateForumThreadDto updateDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var updatedThread = await _forumService.UpdateThreadAsync(id, updateDto);
                // Service throws KeyNotFoundException if thread not found, or UnauthorizedAccessException
                return Ok(updatedThread);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"UpdateThread: Thread not found (ID: {id}) - {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized attempt to update thread {id}: {ex.Message}");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating thread with id {id}.");
                return StatusCode(500, new { message = "An error occurred while updating the thread." });
            }
        }

        [HttpDelete("threads/{id}")]
        public async Task<IActionResult> DeleteThread(string id)
        {
            try
            {
                await _forumService.DeleteThreadAsync(id);
                // Service throws KeyNotFoundException or UnauthorizedAccessException
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"DeleteThread: Thread not found (ID: {id}) - {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized attempt to delete thread {id}: {ex.Message}");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting thread with id {id}.");
                return StatusCode(500, new { message = "An error occurred while deleting the thread." });
            }
        }

        // --- Comment Endpoints ---
        [HttpGet("threads/{threadId}/comments")]
        public async Task<ActionResult<IEnumerable<ThreadCommentDto>>> GetComments(string threadId)
        {
            try
            {
                // Verify thread exists before fetching comments (optional, service might do it)
                if (await _forumService.GetThreadByIdAsync(threadId) == null)
                {
                    return NotFound(new { message = $"Thread with ID {threadId} not found." });
                }
                var comments = await _forumService.GetCommentsForThreadAsync(threadId);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting comments for thread {threadId}.");
                return StatusCode(500, new { message = "An error occurred while fetching comments." });
            }
        }

        [HttpGet("comments/{commentId}")]
        public async Task<ActionResult<ThreadCommentDto>> GetComment(string commentId)
        {
            try
            {
                var comment = await _forumService.GetCommentByIdAsync(commentId);
                if (comment == null) return NotFound(new { message = "Comment not found." });
                return Ok(comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting comment {commentId}.");
                return StatusCode(500, new { message = "An error occurred while fetching the comment." });
            }
        }

        [HttpPost("threads/{threadId}/comments")]
        public async Task<ActionResult<ThreadCommentDto>> CreateComment(string threadId, [FromBody] CreateThreadCommentDto createDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var createdComment = await _forumService.CreateCommentAsync(threadId, createDto);
                return CreatedAtAction(nameof(GetComment), new { commentId = createdComment.Id }, createdComment);
            }
            catch (KeyNotFoundException ex) // From service if thread not found
            {
                _logger.LogWarning($"CreateComment on non-existent thread {threadId}: {ex.Message}");
                return NotFound(new { message = ex.Message }); // e.g. "Thread not found"
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized comment creation: {ex.Message}");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating comment for thread {threadId}.");
                return StatusCode(500, new { message = "An error occurred while creating the comment." });
            }
        }

        [HttpPut("comments/{commentId}")]
        public async Task<ActionResult<ThreadCommentDto>> UpdateComment(string commentId, [FromBody] UpdateThreadCommentDto updateDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var updatedComment = await _forumService.UpdateCommentAsync(commentId, updateDto);
                return Ok(updatedComment);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"UpdateComment: Comment not found (ID: {commentId}) - {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized attempt to update comment {commentId}: {ex.Message}");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating comment {commentId}.");
                return StatusCode(500, new { message = "An error occurred while updating the comment." });
            }
        }

        [HttpDelete("comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(string commentId)
        {
            try
            {
                await _forumService.DeleteCommentAsync(commentId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"DeleteComment: Comment not found (ID: {commentId}) - {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized attempt to delete comment {commentId}: {ex.Message}");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting comment {commentId}.");
                return StatusCode(500, new { message = "An error occurred while deleting the comment." });
            }
        }

        // --- Reply Endpoints ---
        [HttpGet("comments/{commentId}/replies")]
        public async Task<ActionResult<IEnumerable<ThreadReplyDto>>> GetReplies(string commentId)
        {
            try
            {
                // Verify comment exists before fetching replies (optional)
                if (await _forumService.GetCommentByIdAsync(commentId) == null)
                {
                    return NotFound(new { message = $"Comment with ID {commentId} not found." });
                }
                var replies = await _forumService.GetRepliesForCommentAsync(commentId);
                return Ok(replies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting replies for comment {commentId}.");
                return StatusCode(500, new { message = "An error occurred while fetching replies." });
            }
        }

        [HttpGet("replies/{replyId}")]
        public async Task<ActionResult<ThreadReplyDto>> GetReply(string replyId)
        {
            try
            {
                var reply = await _forumService.GetReplyByIdAsync(replyId);
                if (reply == null) return NotFound(new { message = "Reply not found." });
                return Ok(reply);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting reply {replyId}.");
                return StatusCode(500, new { message = "An error occurred while fetching the reply." });
            }
        }

        [HttpPost("comments/{commentId}/replies")]
        public async Task<ActionResult<ThreadReplyDto>> CreateReply(string commentId, [FromBody] CreateThreadReplyDto createDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var createdReply = await _forumService.CreateReplyAsync(commentId, createDto);
                return CreatedAtAction(nameof(GetReply), new { replyId = createdReply.Id }, createdReply);
            }
            catch (KeyNotFoundException ex) // From service if comment not found
            {
                _logger.LogWarning($"CreateReply on non-existent comment {commentId}: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized reply creation: {ex.Message}");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating reply for comment {commentId}.");
                return StatusCode(500, new { message = "An error occurred while creating the reply." });
            }
        }

        [HttpPut("replies/{replyId}")]
        public async Task<ActionResult<ThreadReplyDto>> UpdateReply(string replyId, [FromBody] UpdateThreadReplyDto updateDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var updatedReply = await _forumService.UpdateReplyAsync(replyId, updateDto);
                return Ok(updatedReply);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"UpdateReply: Reply not found (ID: {replyId}) - {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized attempt to update reply {replyId}: {ex.Message}");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating reply {replyId}.");
                return StatusCode(500, new { message = "An error occurred while updating the reply." });
            }
        }

        [HttpDelete("replies/{replyId}")]
        public async Task<IActionResult> DeleteReply(string replyId)
        {
            try
            {
                await _forumService.DeleteReplyAsync(replyId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"DeleteReply: Reply not found (ID: {replyId}) - {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Unauthorized attempt to delete reply {replyId}: {ex.Message}");
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting reply {replyId}.");
                return StatusCode(500, new { message = "An error occurred while deleting the reply." });
            }
        }
    }
}