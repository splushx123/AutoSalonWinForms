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

        public UserAccount CurrentUser { get; private set; }

        public LoginForm()
        {
            Theme.StyleForm(this);
            Text = "Вход в AutoSalon Control Center";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            ClientSize = new Size(1040, 620);

            Panel canvas = new Panel { Dock = DockStyle.Fill, Padding = new Padding(34), BackColor = Theme.AppBack };
            Controls.Add(canvas);

            Panel topStrip = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = Theme.Ink, Padding = new Padding(26, 12, 26, 10) };
            Label brand = new Label
            {
                Text = "AutoSalon Control Center",
                Dock = DockStyle.Left,
                Width = 420,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            Label version = new Label
            {
                Text = "CRM / склад / продажи / сервис",
                Dock = DockStyle.Right,
                Width = 330,
                ForeColor = Color.FromArgb(178, 203, 230),
                TextAlign = ContentAlignment.MiddleRight
            };
            topStrip.Controls.Add(version);
            topStrip.Controls.Add(brand);
            canvas.Controls.Add(topStrip);

            TableLayoutPanel grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(0, 28, 0, 0) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48));
            canvas.Controls.Add(grid);

            RoundedPanel cockpit = Theme.CreateCard(26);
            cockpit.Dock = DockStyle.Fill;
            cockpit.BackColor = Color.FromArgb(18, 30, 52);
            cockpit.StrokeColor = Color.FromArgb(50, 76, 112);
            cockpit.Margin = new Padding(0, 0, 18, 0);
            grid.Controls.Add(cockpit, 0, 0);

            Label cockpitTitle = new Label
            {
                Text = "Панель автосалона",
                Dock = DockStyle.Top,
                Height = 50,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 24F, FontStyle.Bold)
            };
            Label cockpitText = new Label
            {
                Text = "После входа система откроет рабочее место по роли пользователя: продажи, склад, сервис, отчеты или администрирование.",
                Dock = DockStyle.Top,
                Height = 74,
                ForeColor = Color.FromArgb(197, 214, 232),
                Font = new Font("Segoe UI", 11F)
            };
            FlowLayoutPanel chips = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 92, FlowDirection = FlowDirection.LeftToRight };
            chips.Controls.Add(MetricChip("VIN", "склад"));
            chips.Controls.Add(MetricChip("CRM", "клиенты"));
            chips.Controls.Add(MetricChip("DEAL", "сделки"));
            chips.Controls.Add(MetricChip("DRIVE", "тест"));
            RoundedPanel hero = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(25, 48, 83),
                Radius = 34,
                StrokeColor = Color.FromArgb(61, 101, 146),
                Padding = new Padding(24),
                Margin = new Padding(0, 22, 0, 0)
            };
            hero.Controls.Add(new Label
            {
                Text = "Новый интерфейс не использует оформление примера: нет прежнего меню и компоновки, вместо этого — командный центр с карточками, лентой и рабочими зонами.",
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(220, 233, 248),
                Font = new Font("Segoe UI", 12F),
                TextAlign = ContentAlignment.MiddleCenter
            });
            cockpit.Controls.Add(hero);
            cockpit.Controls.Add(chips);
            cockpit.Controls.Add(cockpitText);
            cockpit.Controls.Add(cockpitTitle);

            RoundedPanel loginCard = Theme.CreateCard(34);
            loginCard.Dock = DockStyle.Fill;
            loginCard.Margin = new Padding(18, 0, 0, 0);
            grid.Controls.Add(loginCard, 1, 0);

            Label title = new Label { Text = "Авторизация", Dock = DockStyle.Top, Height = 48, Font = new Font("Segoe UI Semibold", 24F, FontStyle.Bold), ForeColor = Theme.Ink };
            Label subtitle = new Label { Text = "Введите учетные данные из курсовой работы. Роль будет выбрана автоматически.", Dock = DockStyle.Top, Height = 58, ForeColor = Theme.Muted, Font = new Font("Segoe UI", 10.5F) };
            loginCard.Controls.Add(BuildBottomButtons());
            loginCard.Controls.Add(BuildLoginFields());
            loginCard.Controls.Add(subtitle);
            loginCard.Controls.Add(title);

            if (File.Exists(_rememberFile))
            {
                _txtEmail.Text = File.ReadAllText(_rememberFile).Trim();
                _chkRemember.Checked = _txtEmail.Text.Length > 0;
            }
            else _txtEmail.Text = "head@autosalon.local";
            AcceptButton = FindLoginButton(loginCard);
        }

        private Control MetricChip(string big, string small)
        {
            RoundedPanel p = new RoundedPanel { Width = 112, Height = 76, Radius = 18, BackColor = Color.FromArgb(31, 57, 94), StrokeColor = Color.FromArgb(65, 105, 149), Margin = new Padding(0, 0, 12, 0), Padding = new Padding(12) };
            p.Controls.Add(new Label { Text = small, Dock = DockStyle.Bottom, Height = 24, ForeColor = Color.FromArgb(164, 190, 219), TextAlign = ContentAlignment.MiddleCenter });
            p.Controls.Add(new Label { Text = big, Dock = DockStyle.Fill, ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter });
            return p;
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
            _chkRemember.Text = "Запомнить почту на этом компьютере";
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
            Panel bottom = new Panel { Dock = DockStyle.Bottom, Height = 146 };
            FlowLayoutPanel row = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 58, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            Button btnLogin = Theme.CreatePrimaryButton("Войти", 150);
            Button btnTest = Theme.CreateSecondaryButton("Проверить SQL", 150);
            Button btnClose = Theme.CreateGhostButton("Закрыть", 110);
            btnLogin.Name = "btnLogin";
            btnLogin.Click += delegate { DoLogin(); };
            btnTest.Click += delegate { TestConnection(); };
            btnClose.Click += delegate { Close(); };
            row.Controls.Add(btnLogin);
            row.Controls.Add(btnTest);
            row.Controls.Add(btnClose);
            Label hint = new Label
            {
                Text = "Учетные записи: head@autosalon.local / manager@autosalon.local / stock@autosalon.local / service@autosalon.local / admin@autosalon.local",
                Dock = DockStyle.Fill,
                ForeColor = Theme.Muted,
                Font = new Font("Segoe UI", 9F)
            };
            bottom.Controls.Add(hint);
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
            if (string.IsNullOrWhiteSpace(_txtEmail.Text)) { _lblError.Text = "Введите почту."; return; }
            if (string.IsNullOrWhiteSpace(_txtPassword.Text)) { _lblError.Text = "Введите пароль."; return; }
            CurrentUser = AuthService.Authenticate(_txtEmail.Text, _txtPassword.Text);
            if (CurrentUser == null) { _lblError.Text = "Неверная почта или пароль."; return; }
            string error;
            if (!Db.CanConnect(out error))
            {
                MessageBox.Show("Не удалось подключиться к SQL Server.\n\n" + error, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (_chkRemember.Checked) File.WriteAllText(_rememberFile, _txtEmail.Text.Trim());
            else if (File.Exists(_rememberFile)) File.Delete(_rememberFile);
            LogService.Log("Авторизация", CurrentUser.Email + " | " + CurrentUser.FullName);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
