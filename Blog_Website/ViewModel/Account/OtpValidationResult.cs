namespace Blog_Website.ViewModel.Account
{
    public record OtpValidationResult(bool IsValid, string Message)
    {
        public static OtpValidationResult Invalid(string msg) => new(false, msg);
        public static OtpValidationResult Valid() => new(true, "");
    }
}
