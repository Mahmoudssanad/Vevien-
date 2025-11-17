namespace Blog_Website.ViewModel.Account
{
    public record OtpResult(bool Success, string Message)
    {
        public static OtpResult Failed(string msg) => new(false, msg);
        public static OtpResult SuccessResponse(string msg) => new(true, msg);
    }
}
