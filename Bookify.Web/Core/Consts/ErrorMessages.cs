namespace Bookify.Web.Core.Consts
{
    public class ErrorMessages
    {
        public const string MaxLength = "Length can not be more then {1} charachters !";
        public const string Duplicated = "{0} with the same name already exists !";
        public const string DuplicatedBookTitleWithTheSameAuyhor = "{0} with the same author already exists !";
        public const string DuplicatedBookAuyhorWithTheSameTitle = "{0} with the same title already exists !";
        public const string NotAllowedExtention = "Only .jpg, .jpeg, .png files are allowed !";
        public const string MaxSize = "The file can not be more than 2 MB !";
        public const string NotAllowedDate = "Date on the future are not allowed !";
        public const string InvalidRange = "{0} should be between {1} and {2}!";
        public const string RequiredField = "Required field";
        public const string MaxMinLength = "The {0} must be at least {2} and at max {1} characters long.";
        public const string ConfirmPasswordNotMatch = "The password and confirmation password do not match.";
        public const string WeakPassword = "Passwords contain an uppercase character, lowercase character, a digit, and a non-alphanumeric character. Passwords must be at least 8 characters long";
        public const string InvalidUsername = "Username can only contain letters or digits.";
        public const string OnlyEnglishLetters = "Only English letters are allowed.";
        public const string OnlyArabicLetters = "Only Arabic letters are allowed.";
        public const string OnlyNumbersAndLetters = "Only Arabic/English letters or digits are allowed.";
        public const string DenySpecialCharacters = "Special characters are not allowed.";
		public const string InvalidMobileNumber = "Invalid mobile number.";
        public const string InvalidNationalId = "Invalid national ID.";
        public const string EmptyImage = "Please add an image.";
        public const string InvalidSerialNumber = "Invalid serial number.";
        public const string NotAvilableRental = "This book/copy is not available for rental.";
        public const string BlackListedSubscriber = "This subscriber is blacklisted.";
        public const string InactiveSubscriber = "This subscriber is inactive.";
        public const string MaxCopiesReached = "This subscriber has reached the max number for rentals.";
        public const string CopyIsInRental = "This copy is already rentaled.";
        public const string RentalNotAllowedForBlacklisted = "Rental cannot be extended for blacklisted subscribers.";
        public const string RentalNotAllowedForInactive = "Rental cannot be extended for this subscriber before renwal.";
        public const string ExtendNotAllowed = "Rental cannot be extended.";
        public const string PenaltyShouldBePaid = "Penalty should be paid.";
    }
}
