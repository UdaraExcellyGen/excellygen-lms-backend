using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ExcellyGenLMS.Application.DTOs.Learner
{
    public class AuthorDto // Stays the same, User.Id is string
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Avatar { get; set; }
    }

    public class ForumThreadDto
    {
        public int Id { get; set; } // CHANGED: string to int
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public AuthorDto? Author { get; set; } // Author.Id is string
        public int CommentsCount { get; set; }
        public bool IsCurrentUserAuthor { get; set; }
    }

    public class CreateForumThreadDto // Stays the same (no IDs)
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        [StringLength(512)]
        public string? ImageUrl { get; set; }
    }

    public class UpdateForumThreadDto // Stays the same (no IDs in DTO)
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        [StringLength(512)]
        public string? ImageUrl { get; set; }
    }

    public class ThreadCommentDto
    {
        public int Id { get; set; } // CHANGED: string to int
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public AuthorDto? Author { get; set; } // Author.Id is string
        public int ThreadId { get; set; } // CHANGED: string to int (FK to ForumThread.Id)
        public int RepliesCount { get; set; }
        public bool IsCurrentUserAuthor { get; set; }
    }

    public class CreateThreadCommentDto // Stays the same (no IDs)
    {
        [Required]
        public string Content { get; set; } = string.Empty;
    }

    public class UpdateThreadCommentDto // Stays the same (no IDs in DTO)
    {
        [Required]
        public string Content { get; set; } = string.Empty;
    }

    public class ThreadReplyDto
    {
        public int Id { get; set; } // CHANGED: string to int
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public AuthorDto? Author { get; set; } // Author.Id is string
        public int CommentId { get; set; } // CHANGED: string to int (FK to ThreadComment.Id)
        public bool IsCurrentUserAuthor { get; set; }
    }

    public class CreateThreadReplyDto // Stays the same (no IDs)
    {
        [Required]
        public string Content { get; set; } = string.Empty;
    }

    public class UpdateThreadReplyDto // Stays the same (no IDs in DTO)
    {
        [Required]
        public string Content { get; set; } = string.Empty;
    }

    public class ForumQueryParams // Stays the same
    {
        public string? SearchTerm { get; set; }
        public string? Category { get; set; }
        public bool MyThreads { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    // PagedResult should already be using generic T, so its definition is fine
    // but make sure it's correctly referenced/located as discussed previously
    // e.g., ExcellyGenLMS.Application.DTOs.Learner or Common
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}