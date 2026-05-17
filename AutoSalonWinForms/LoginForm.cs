using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
    public class LoginForm : Form
    {
        private readonly TextBox _txtEmail = Theme.CreateTextBox(360);
        private readonly TextBox _txtPassword = Theme.CreateTextBox(360);
        private readonly CheckBox _chkRemember = new CheckBox();
        private readonly Label _lblError = new Label();
        private readonly string _rememberFile = Path.Combine(Application.StartupPath, "last_email.txt");
        private readonly string _rememberPasswordFile = Path.Combine(Application.StartupPath, "last_password.txt");

        public UserAccount CurrentUser { get; private set; }

        public LoginForm()
        {
            Theme.StyleForm(this);
            Text = "Вход в AutoSalon Control Center";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            ClientSize = new Size(620, 460);

            Panel canvas = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24), BackColor = Theme.AppBack };
            Controls.Add(canvas);

            RoundedPanel loginCard = Theme.CreateCard(30);
            loginCard.Size = new Size(520, 400);
            loginCard.Location = new Point((ClientSize.Width - loginCard.Width) / 2, (ClientSize.Height - loginCard.Height) / 2);
            canvas.Controls.Add(loginCard);

            Label title = new Label { Text = "Авторизация", Dock = DockStyle.Top, Height = 56, Font = new Font("Segoe UI Semibold", 24F, FontStyle.Bold), ForeColor = Theme.Ink, TextAlign = ContentAlignment.MiddleLeft };
            loginCard.Controls.Add(BuildBottomButtons());
            loginCard.Controls.Add(BuildLoginFields());
            loginCard.Controls.Add(title);

            if (File.Exists(_rememberFile))
            {
                _txtEmail.Text = File.ReadAllText(_rememberFile).Trim();
                if (File.Exists(_rememberPasswordFile))
                {
                    _txtPassword.Text = File.ReadAllText(_rememberPasswordFile);
                    _chkRemember.Checked = _txtPassword.Text.Length > 0;
                }
            }
            else _txtEmail.Text = "head@autosalon.local";
            AcceptButton = FindLoginButton(loginCard);
            Load += delegate { Theme.ApplyCurrentTheme(this); };
        }

        private Control BuildLoginFields()
        {
            Panel zone = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 24, 0, 0) };
            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Top, Height = 230, ColumnCount = 1 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            _txtEmail.Dock = DockStyle.Top;
            _txtPassword.Dock = DockStyle.Top;
            _txtPassword.UseSystemPasswordChar = true;
            _chkRemember.Text = "Запомнить пароль";
            _chkRemember.ForeColor = Theme.Muted;
            _chkRemember.AutoSize = true;
            _lblError.ForeColor = Theme.Danger;
            _lblError.Dock = DockStyle.Fill;
            layout.Controls.Add(new Label { Text = "Почта", ForeColor = Theme.Muted, Dock = DockStyle.Fill }, 0, 0);
            layout.Controls.Add(_txtEmail, 0, 1);
            layout.Controls.Add(new Label { Text = "Пароль", ForeColor = Theme.Muted, Dock = DockStyle.Fill }, 0, 2);
            layout.Controls.Add(_txtPassword, 0, 3);
            layout.Controls.Add(_chkRemember, 0, 4);
            layout.Controls.Add(_lblError, 0, 5);
            zone.Controls.Add(layout);
            return zone;
        }

        private Control BuildBottomButtons()
        {
            Panel bottom = new Panel { Dock = DockStyle.Bottom, Height = 74 };
            FlowLayoutPanel row = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 58, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            Button btnLogin = Theme.CreatePrimaryButton("Войти", 170);
            Button btnTest = Theme.CreateSecondaryButton("Проверить SQL", 170);
            btnLogin.Name = "btnLogin";
            btnLogin.Click += delegate { DoLogin(); };
            btnTest.Click += delegate { TestConnection(); };
            row.Controls.Add(btnLogin);
            row.Controls.Add(btnTest);
            bottom.Controls.Add(row);
            return bottom;
        }

        private IButtonControl FindLoginButton(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c.Name == "btnLogin") return (IButtonControl)c;
                IButtonControl child = FindLoginButton(c);
                if (child != null) return child;
            }
            return null;
        }

        private void TestConnection()
        {
            string error;
            bool ok = Db.CanConnect(out error);
            MessageBox.Show(ok ? "Подключение к SQL Server выполнено успешно." : "Не удалось подключиться к базе данных.\n\n" + error,
                "Проверка подключения", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Error);
        }

        private void DoLogin()
        {
            _lblError.Text = string.Empty;
            string email = (_txtEmail.Text ?? string.Empty).Trim();
            string password = (_txtPassword.Text ?? string.Empty).Trim();
            _txtEmail.Text = email;
            _txtPassword.Text = password;

            if (email.Length == 0)
            {
                _lblError.Text = "Введите почту.";
                MessageBox.Show("Поле «Почта» не заполнено.", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtEmail.Focus();
                return;
            }
            if (!ValidationRules.IsEmail(email))
            {
                _lblError.Text = "Неверный формат почты.";
                MessageBox.Show("Введите корректную почту в формате name@example.com.", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtEmail.Focus();
                return;
            }
            if (password.Length == 0)
            {
                _lblError.Text = "Введите пароль.";
                MessageBox.Show("Поле «Пароль» не заполнено.", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtPassword.Focus();
                return;
            }
            if (password.Length < 4)
            {
                _lblError.Text = "Пароль слишком короткий.";
                MessageBox.Show("Пароль должен содержать минимум 4 символа.", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtPassword.Focus();
                return;
            }

            CurrentUser = AuthService.Authenticate(email, password);
            if (CurrentUser == null)
            {
                _lblError.Text = "Неверная почта или пароль.";
                MessageBox.Show("Неверная почта или пароль. Проверьте введенные данные.", "Ошибка авторизации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string error;
            if (!Db.CanConnect(out error))
            {
                MessageBox.Show("Не удалось подключиться к SQL Server.\n\n" + error, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (_chkRemember.Checked)
            {
                File.WriteAllText(_rememberFile, _txtEmail.Text.Trim());
                File.WriteAllText(_rememberPasswordFile, _txtPassword.Text);
            }
            else
            {
                if (File.Exists(_rememberFile)) File.Delete(_rememberFile);
                if (File.Exists(_rememberPasswordFile)) File.Delete(_rememberPasswordFile);
            }
            LogService.Log("Авторизация", CurrentUser.Email + " | " + CurrentUser.FullName);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
