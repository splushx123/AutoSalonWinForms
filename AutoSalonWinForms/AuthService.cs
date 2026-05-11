using System.Configuration;

namespace AutoInsuranceWinForms
{
    public static class AuthService
    {
        public static UserAccount Authenticate(string email, string password)
        {
            email = (email ?? string.Empty).Trim().ToLowerInvariant();
            password = (password ?? string.Empty).Trim();
            if (email.Length == 0 || password.Length == 0) return null;

            if (email == Read("HeadEmail") && password == ReadRaw("HeadPassword"))
                return new UserAccount { Email = email, FullName = "Руководитель отдела продаж", Role = UserRole.DepartmentHead };
            if (email == Read("ManagerEmail") && password == ReadRaw("ManagerPassword"))
                return new UserAccount { Email = email, FullName = "Менеджер по продажам", Role = UserRole.Manager };
            if (email == Read("StockEmail") && password == ReadRaw("StockPassword"))
                return new UserAccount { Email = email, FullName = "Сотрудник склада", Role = UserRole.Stock };
            if (email == Read("ServiceEmail") && password == ReadRaw("ServicePassword"))
                return new UserAccount { Email = email, FullName = "Сервисный специалист", Role = UserRole.Service };
            if (email == Read("AdminEmail") && password == ReadRaw("AdminPassword"))
                return new UserAccount { Email = email, FullName = "Администратор", Role = UserRole.Admin };

            return null;
        }

        private static string Read(string key)
        {
            return (ConfigurationManager.AppSettings[key] ?? string.Empty).Trim().ToLowerInvariant();
        }

        private static string ReadRaw(string key)
        {
            return (ConfigurationManager.AppSettings[key] ?? string.Empty).Trim();
        }
    }
}
