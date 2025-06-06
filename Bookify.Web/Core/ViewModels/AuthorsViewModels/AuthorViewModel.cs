﻿namespace Bookify.Web.Core.ViewModels.AuthorsViewModels
{
    public class AuthorViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastUpdatedOn { get; set; }

    }
}
