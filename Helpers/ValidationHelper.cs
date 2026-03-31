using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SecureBankingApp.Helpers
{
    public static class ValidationHelper
    {
        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Validates a basic email address format.
        /// </summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return EmailRegex.IsMatch(email);
        }

        /// <summary>
        /// Validates password strength:
        /// - 8+ characters
        /// - 1+ Uppercase
        /// - 1+ Lowercase
        /// - 1+ Digit
        /// - 1+ Special Character
        /// </summary>
        public static bool IsValidPassword(string password, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                errorMessage = "Password cannot be empty.";
                return false;
            }

            if (password.Length < 8)
            {
                errorMessage = "Password must be at least 8 characters long.";
                return false;
            }

            if (!password.Any(char.IsUpper))
            {
                errorMessage = "Password must contain at least one uppercase letter.";
                return false;
            }

            if (!password.Any(char.IsLower))
            {
                errorMessage = "Password must contain at least one lowercase letter.";
                return false;
            }

            if (!password.Any(char.IsDigit))
            {
                errorMessage = "Password must contain at least one digit.";
                return false;
            }

            if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                errorMessage = "Password must contain at least one special character.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }
    }
}
