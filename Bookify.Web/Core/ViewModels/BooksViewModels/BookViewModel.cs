﻿namespace Bookify.Web.Core.ViewModels.BooksViewModels
{
    public class BookViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public string Author { get; set; } = null!;

        public string Publisher { get; set; } = null!;

        public DateTime PublishingDate { get; set; }

        public string? ImageUrl { get; set; }

        public string? ThumbnailImageUrl { get; set; }

        public string Hall { get; set; } = null!;

        public bool IsAvilableForRental { get; set; }

        public string Description { get; set; } = null!;

        public IEnumerable<string> Categories { get; set; } = null!;

        public bool IsDeleted { get; set; }

        public DateTime CreatedOn { get; set; }

        public IEnumerable<BookCopyViewModel> Copies { get; set; } = null!;
    }
}