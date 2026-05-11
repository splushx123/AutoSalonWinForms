using System;
using System.Text.RegularExpressions;

namespace AutoInsuranceWinForms
{
    public static class ValidationRules
    {
        public static bool IsEmail(string value)
        {
            return Regex.IsMatch(value ?? string.Empty, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        public static bool IsInn12(string value)
        {
            return Regex.IsMatch(value ?? string.Empty, @"^\d{12}$");
        }

        public static bool IsVin(string value)
        {
            return Regex.IsMatch((value ?? string.Empty).Trim().ToUpperInvariant(), @"^[A-HJ-NPR-Z0-9]{17}$");
        }

        public static bool TryNormalizePhoneRu(string input, out string phone)
        {
            var digits = Regex.Replace(input ?? string.Empty, @"\D", string.Empty);
            if (digits.Length == 10) digits = "7" + digits;
            if (digits.Length == 11 && digits.StartsWith("8")) digits = "7" + digits.Substring(1);
            if (digits.Length != 11 || !digits.StartsWith("7"))
            {
                phone = string.Empty;
                return false;
            }
            phone = string.Format("+7({0}){1}-{2}-{3}", digits.Substring(1, 3), digits.Substring(4, 3), digits.Substring(7, 2), digits.Substring(9, 2));
            return true;
        }
    }
}
