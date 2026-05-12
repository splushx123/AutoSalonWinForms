using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += delegate(object sender, System.Threading.ThreadExceptionEventArgs e)
            {
                LogService.Log("Ошибка интерфейса", e.Exception.Message);
                MessageBox.Show("Во время работы приложения возникла ошибка.\n\n" + e.Exception.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
            {
                var ex = e.ExceptionObject as Exception;
                var message = ex == null ? "Неизвестная критическая ошибка." : ex.Message;
                LogService.Log("Критическая ошибка", message);
                MessageBox.Show("Критическая ошибка приложения.\n\n" + message, "Критическая ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            TaskScheduler.UnobservedTaskException += delegate(object sender, UnobservedTaskExceptionEventArgs e)
            {
                LogService.Log("Ошибка фоновой задачи", e.Exception.Message);
                e.SetObserved();
            };

            while (true)
            {
                using (var login = new LoginForm())
                {
                    if (login.ShowDialog() != DialogResult.OK || login.CurrentUser == null) break;
                    var main = new MainForm(login.CurrentUser);
                    Application.Run(main);
                    if (!main.ReturnToLogin) break;
                }
            }
        }
    }
}
