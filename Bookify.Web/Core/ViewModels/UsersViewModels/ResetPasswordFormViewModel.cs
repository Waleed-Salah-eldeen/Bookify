namespace Bookify.Web.Core.ViewModels.UsersViewModels
{
    public class ResetPasswordFormViewModel
    {
        public string Id { get; set; } = null!;
        [DataType(DataType.Password),
            StringLength(100, ErrorMessage = ErrorMessages.MaxMinLength, MinimumLength = 8),
            RegularExpression(RegexPatterns.Password, ErrorMessage = ErrorMessages.WeakPassword)]
        public string Password { get; set; } = null!; 

        [DataType(DataType.Password), Display(Name = "Confirm password"),
            Compare("Password", ErrorMessage = ErrorMessages.ConfirmPasswordNotMatch)]
        public string ConfirmPassword { get; set; } = null!;
    }
}
