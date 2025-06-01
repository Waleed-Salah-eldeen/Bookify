
namespace Bookify.Web.Core.ViewModels.BookCopiesViewModels
{
    public class BookCopyFormViewModel
    {
        public int Id { get; set; }
        public int BookId { get; set; }

        [Display(Name = "Is available for rental?")]
        public bool IsAvilableForRental { get; set; }

        [Display(Name = "Edition Number"),
            Range(1, 1000, ErrorMessage = ErrorMessages.InvalidRange)]
        public int EditionNumber { get; set; }

        public bool ShowRentalInput { get; set; }
    }
}
