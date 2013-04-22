using System.ComponentModel.DataAnnotations;

namespace TeamMentor.CoreLib.TM_AppCode.Schemas
{
    public class Password
    {
        public Password(string password)
        {
            PasswordText = password;
        }

        [Required]
        [StringLength(30, MinimumLength = 8)]
        [RegularExpression(ValidationRegex.Password, ErrorMessage = ValidationRegex.PasswordErrorMessage)]
        public string PasswordText { get; set; }
    }
}