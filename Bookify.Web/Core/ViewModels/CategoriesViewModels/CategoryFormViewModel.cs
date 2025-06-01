namespace Bookify.Web.Core.ViewModels.CategoriesViewModels
{
    public class CategoryFormViewModel
    {
        public int Id { get; set; }

        [MaxLength(100, ErrorMessage = ErrorMessages.MaxLength), Display(Name = "Category")]
        [Remote("AllowItem", null!, AdditionalFields = "Id", ErrorMessage = ErrorMessages.Duplicated)]
        public string Name { get; set; }

    }
}
