
namespace Bookify.Web.Core.ViewModels.AuthorsViewModels
{
    public class AuthorFormViewModel
    {
        public int Id { get; set; }

        [MaxLength(100, ErrorMessage = ErrorMessages.MaxLength), Display(Name = "Author")]
        [Remote("AllowItem", null!, AdditionalFields = "Id", ErrorMessage = ErrorMessages.Duplicated)]
        public string Name { get; set; }

    }
}
