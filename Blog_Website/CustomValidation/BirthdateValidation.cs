using System.ComponentModel.DataAnnotations;

namespace Blog_Website.CustomValidation
{
    public class BirthdateValidation : ValidationAttribute
    {
        private readonly DateOnly _birthdate;

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // عشان اتاكد انها جايالي بالنوع اللي انا عاوزه value لل cast بنعمل 
            if (value is not DateOnly birthdate)
            {
                return new ValidationResult("Invaild Date syntax");
            }
            var today = DateTime.Today;

            var result = today.Year - birthdate.Year;

            if(result < 14)
            {
                return new ValidationResult("age must be greater than 14 year");
            }

            return ValidationResult.Success;
        }
    }
}
