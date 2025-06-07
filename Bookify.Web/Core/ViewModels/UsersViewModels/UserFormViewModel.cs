using CloudinaryDotNet.Actions;
using UoN.ExpressiveAnnotations.NetCore.Attributes;

namespace Bookify.Web.Core.ViewModels.UsersViewModels
{
    public class UserFormViewModel
    {
        public string? Id { get; set; }

        [MaxLength(100, ErrorMessage = ErrorMessages.MaxLength), Display(Name = "Full Name"),
            RegularExpression(RegexPatterns.CharactersOnly_Eng, ErrorMessage = ErrorMessages.OnlyEnglishLetters)]
        public string FullName { get; set; } = null!;

        [MaxLength(20, ErrorMessage = ErrorMessages.MaxLength), Display(Name = "Username"),
            RegularExpression(RegexPatterns.Username, ErrorMessage = ErrorMessages.InvalidUsername)]
        [Remote("AllowUserName", null!, AdditionalFields = "Id", ErrorMessage = ErrorMessages.Duplicated)]
        public string UserName { get; set; } = null!;

        [MaxLength(200, ErrorMessage = ErrorMessages.MaxLength), EmailAddress]
        [Remote("AllowEmail", null!, AdditionalFields = "Id", ErrorMessage = ErrorMessages.Duplicated)]
        public string Email { get; set; } = null!;

        [DataType(DataType.Password),
        StringLength(100, ErrorMessage = ErrorMessages.MaxMinLength, MinimumLength = 8),
            RegularExpression(RegexPatterns.Password, ErrorMessage = ErrorMessages.WeakPassword)]
        [RequiredIf("Id == null", ErrorMessage = ErrorMessages.RequiredField)]
        public string? Password { get; set; } = null!;

        [DataType(DataType.Password), Display(Name = "Confirm password"),
            Compare("Password", ErrorMessage = ErrorMessages.ConfirmPasswordNotMatch)]
        [RequiredIf("Id == null", ErrorMessage = ErrorMessages.RequiredField)]
        public string? ConfirmPassword { get; set; } = null!;

        [Display(Name = "Roles")]
        public IList<string> SelectedRoles { get; set; } = new List<string>();

        public IEnumerable<SelectListItem>? Roles { get; set; }
    }
}
