using Microsoft.AspNetCore.Mvc.Rendering;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookify.Web.Core.ViewModels.BooksViewModels
{
    public class BookFormViewModel
    {
        public int Id { get; set; }

        [MaxLength(500,ErrorMessage =ErrorMessages.MaxLength), Display(Name ="Title")]
        [Remote("AllowItem", null!, AdditionalFields = "Id, AuthorId", ErrorMessage = ErrorMessages.DuplicatedBookTitleWithTheSameAuyhor)]
        public string Title { get; set; } = null!;

        [Display(Name = "Author")]
        [Remote("AllowItem", null!, AdditionalFields = "Id, Title", ErrorMessage = ErrorMessages.DuplicatedBookAuyhorWithTheSameTitle)]
        public int AuthorId { get; set; }
        public IEnumerable<SelectListItem>? Authors { get; set; }

        [MaxLength(200, ErrorMessage = ErrorMessages.MaxLength)]
        public string Publisher { get; set; } = null!;

        [Display(Name = "Publishing Date")]
        [AssertThat("PublishingDate <= Today()", ErrorMessage = ErrorMessages.NotAllowedDate)]
        public DateTime PublishingDate { get; set; } = DateTime.Now;

        public IFormFile? Image { get; set; }
        public string? ImageUrl { get; set; }
        public string? ThumbnailImageUrl { get; set; }

        [MaxLength(200, ErrorMessage = ErrorMessages.MaxLength)]
        public string Hall { get; set; } = null!;

        [Display(Name = "Is avilable for rental?")]
        public bool IsAvilableForRental { get; set; }
        public string Description { get; set; } = null!;

        [Display(Name = "Categories")]
        public IList<int> SelectedCategories { get; set; } = new List<int>();
        
        public IEnumerable<SelectListItem>? Categories { get; set; }

    }
}
