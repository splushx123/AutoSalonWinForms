using System;
using System.Drawing;
using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
    public class MainForm : Form
    {
        private readonly UserAccount _user;
        private readonly FlowLayoutPanel _statsPanel = new FlowLayoutPanel();
        private readonly FlowLayoutPanel _modulesPanel = new FlowLayoutPanel();
        private readonly DateTimePicker _dtFrom = Theme.CreateDatePicker(120);
        private readonly DateTimePicker _dtTo = Theme.CreateDatePicker(120);
        private readonly Label _periodHint = new Label();
        public bool ReturnToLogin { get; private set; }

        public MainForm(UserAccount user)
        {
            _user = user;
            Theme.StyleForm(this);
            Text = "AutoSalon Control Center";
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterScreen;

            Panel shell = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), BackColor = Theme.AppBack };
            Controls.Add(shell);

            RoundedPanel dashboard = Theme.CreateCard(20);
            dashboard.Dock = DockStyle.Fill;
            dashboard.Padding = new Padding(18);
            shell.Controls.Add(dashboard);

            Label title = new Label { Text = BuildRoleTitle(), Dock = DockStyle.Top, Height = 42, Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold), ForeColor = Theme.Ink };
            Label sub = new Label { Text = BuildAccessText(), Dock = DockStyle.Top, Height = 36, ForeColor = Theme.Muted, Font = new Font("Segoe UI", 10F) };

            Panel topStatsArea = new Panel { Dock = DockStyle.Top, Height = 170, Padding = new Padding(0, 8, 0, 12) };
            Label sideTitle = new Label { Text = "Статистика за период", Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold), ForeColor = Theme.Ink };
            Panel periodPanel = BuildPeriodPanel();
            _statsPanel.Dock = DockStyle.Fill;
            _statsPanel.FlowDirection = FlowDirection.LeftToRight;
            _statsPanel.WrapContents = false;
            _statsPanel.AutoScroll = true;
            topStatsArea.Controls.Add(_statsPanel);
            topStatsArea.Controls.Add(periodPanel);
            topStatsArea.Controls.Add(sideTitle);

            FlowLayoutPanel bottomActions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 52, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Padding = new Padding(0, 6, 0, 0) };
            Button exitButton = Theme.CreatePrimaryButton("Выйти", 120);
            exitButton.Click += delegate { ReturnToLogin = false; Close(); };
            bottomActions.Controls.Add(exitButton);

                        _modulesPanel.Dock = DockStyle.Fill;
            _modulesPanel.AutoScroll = true;
            _modulesPanel.WrapContents = true;
            _modulesPanel.Padding = new Padding(0, 10, 0, 0);

            dashboard.Controls.Add(_modulesPanel);
            dashboard.Controls.Add(bottomActions);
            dashboard.Controls.Add(topStatsArea);
            dashboard.Controls.Add(sub);
            dashboard.Controls.Add(title);

            Panel ribbon = BuildRibbon();
            shell.Controls.Add(ribbon);

            Load += delegate { FillModules(); ApplyPeriodAndRefreshStats(); };
        }

        private Panel BuildRibbon()
        {
            Panel ribbon = new Panel { Dock = DockStyle.Top, Height = 86, BackColor = Theme.Card, Padding = new Padding(20, 12, 20, 12) };

            Label logo = new Label
            {
                Text = "AutoSalon",
                Dock = DockStyle.Left,
                Width = 170,
                ForeColor = Theme.Ink,
                Font = new Font("Segoe UI Semibold", 20F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            Label user = new Label
            {
                Text = _user.FullName + "\n" + _user.Email,
                Dock = DockStyle.Left,
                Width = 480,
                ForeColor = Theme.Muted,
                Font = new Font("Segoe UI", 10F),
                TextAlign = ContentAlignment.MiddleLeft
            };

            Panel divider = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Theme.Border };

            ribbon.Controls.Add(user);
            ribbon.Controls.Add(logo);
            ribbon.Controls.Add(divider);
            return ribbon;
        }

        private Panel BuildPeriodPanel()
        {
            Panel panel = new Panel { Dock = DockStyle.Top, Height = 58, Padding = new Padding(0, 0, 0, 4) };

            _dtFrom.Value = DateTime.Today.AddMonths(-1);
            _dtTo.Value = DateTime.Today;

            FlowLayoutPanel row = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 32, WrapContents = false, FlowDirection = FlowDirection.LeftToRight };
            row.Controls.Add(new Label { Text = "Период:", Width = 56, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Theme.Muted, Padding = new Padding(0, 8, 0, 0) });
            row.Controls.Add(_dtFrom);
            row.Controls.Add(new Label { Text = "по", Width = 24, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Theme.Muted, Padding = new Padding(0, 8, 0, 0) });
            row.Controls.Add(_dtTo);

            Button apply = Theme.CreateSecondaryButton("Показать", 96);
            apply.Height = 32;
            apply.Click += delegate { ApplyPeriodAndRefreshStats(); };
            row.Controls.Add(apply);

            _periodHint.Dock = DockStyle.Top;
            _periodHint.Height = 20;
            _periodHint.ForeColor = Theme.Muted;
            _periodHint.Font = new Font("Segoe UI", 8.5F);

            panel.Controls.Add(_periodHint);
            panel.Controls.Add(row);
            return panel;
        }

        private void ApplyPeriodAndRefreshStats()
        {
            if (_dtTo.Value.Date < _dtFrom.Value.Date)
            {
                MessageBox.Show("Дата окончания периода не может быть раньше даты начала.", "Период", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _periodHint.Text = "Сделки/выручка/тест-драйвы за период: " + _dtFrom.Value.ToString("dd.MM.yyyy") + " — " + _dtTo.Value.ToString("dd.MM.yyyy");
            FillStats();
        }

        private string BuildRoleTitle()
        {
            if (_user.Role == UserRole.DepartmentHead) return "Командный центр руководителя отдела продаж";
            if (_user.Role == UserRole.Manager) return "Рабочее пространство менеджера по продажам";
            if (_user.Role == UserRole.Stock) return "Складской контур автосалона";
            if (_user.Role == UserRole.Service) return "Сервисный контур предпродажной подготовки";
            return "Административная панель автосалона";
        }

        private string BuildAccessText()
        {
            if (_user.Role == UserRole.DepartmentHead) return "Просмотр ключевых разделов, анализ продаж, автомобилей, услуг и сотрудников.";
            if (_user.Role == UserRole.Manager) return "Клиентская база, автомобили, сделки, тест-драйвы, услуги и отчеты.";
            if (_user.Role == UserRole.Stock) return "Учет автомобилей на складе, VIN, цены, поступления и статусы.";
            if (_user.Role == UserRole.Service) return "Фиксация работ, исполнителей, стоимости и даты выполнения услуг.";
            return "Полный доступ к данным, справочникам, сотрудникам и отчетности.";
        }

        private void FillStats()
        {
            _statsPanel.Controls.Clear();
            string from = _dtFrom.Value.Date.ToString("yyyy-MM-dd");
            string to = _dtTo.Value.Date.AddDays(1).ToString("yyyy-MM-dd");
            AddGauge("Клиенты", SafeCount("SELECT COUNT(*) FROM dbo.Client").ToString(), Theme.Primary);
            AddGauge("Автомобили", SafeCount("SELECT COUNT(*) FROM dbo.Car").ToString(), Theme.Success);
            AddGauge("Сделки", SafeCount($"SELECT COUNT(*) FROM dbo.Deal WHERE deal_date >= '{from}' AND deal_date < '{to}'").ToString(), Theme.Warning);
            AddGauge("Выручка", SafeMoney($"SELECT ISNULL(SUM(final_price),0) FROM dbo.Deal WHERE deal_date >= '{from}' AND deal_date < '{to}'") + " руб.", Theme.Violet);
            AddGauge("Тест-драйвы", SafeCount($"SELECT COUNT(*) FROM dbo.TestDrive WHERE planned_start >= '{from}' AND planned_start < '{to}'").ToString(), Theme.Accent);
        }

        private int SafeCount(string sql) { try { return Db.Count(sql); } catch { return 0; } }
        private string SafeMoney(string sql) { try { return Math.Round(Convert.ToDecimal(Db.Scalar(sql)), 0).ToString("0"); } catch { return "0"; } }

        private void FillModules()
        {
            _modulesPanel.Controls.Clear();
            bool head = _user.Role == UserRole.DepartmentHead;
            bool manager = _user.Role == UserRole.Manager;
            bool stock = _user.Role == UserRole.Stock;
            bool service = _user.Role == UserRole.Service;
            bool admin = _user.Role == UserRole.Admin;
            AddModule("01", "Клиенты", "CRM-карточки, ИНН, источники обращения", delegate { OpenModule("Клиенты", new EntityListForm(_user, EntityConfigs.Clients())); }, head || manager || admin, Theme.Primary);
            AddModule("02", "Автомобили", "VIN, модель, комплектация, цена, статус", delegate { OpenModule("Автомобили", new EntityListForm(_user, EntityConfigs.Cars())); }, head || manager || stock || admin, Theme.Success);
            AddModule("03", "Сделки", "Договоры купли-продажи и формы оплаты", delegate { OpenModule("Сделки", new EntityListForm(_user, EntityConfigs.Deals())); }, head || manager || admin, Theme.Warning);
            AddModule("04", "Тест-драйвы", "Запись клиента, время, результат поездки", delegate { OpenModule("Тест-драйвы", new EntityListForm(_user, EntityConfigs.TestDrives())); }, head || manager || admin, Theme.Accent);
            AddModule("05", "Услуги", "Подготовка, доп. оборудование, стоимость", delegate { OpenModule("Услуги", new EntityListForm(_user, EntityConfigs.Services())); }, head || manager || service || admin, Theme.Violet);
            AddModule("06", "Сотрудники", "ФИО, должности, контакты, даты работы", delegate { OpenModule("Сотрудники", new EntityListForm(_user, EntityConfigs.Employees())); }, head || admin, Color.FromArgb(65, 91, 130));
            AddModule("07", "Отчеты", "Продажи, склад, оплаты, услуги", delegate { OpenModule("Отчеты", new ReportsForm()); }, head || manager || admin, Color.FromArgb(0, 132, 160));
        }

        private void AddModule(string num, string title, string description, Action action, bool visible, Color color)
        {
            if (!visible) return;
            RoundedPanel card = Theme.CreateCard(18);
            card.Width = 320;
            card.Height = 188;
            card.Margin = new Padding(0, 0, 18, 18);
            Panel mark = new Panel { Dock = DockStyle.Left, Width = 6, BackColor = color };
            Label no = new Label { Text = "Модуль " + num, Dock = DockStyle.Top, Height = 22, ForeColor = Theme.Muted, Font = new Font("Segoe UI", 9.5F), TextAlign = ContentAlignment.MiddleLeft };
            Label h = new Label { Text = title, Dock = DockStyle.Top, Height = 34, ForeColor = Theme.Ink, Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold) };
            Label d = new Label { Text = description, Dock = DockStyle.Fill, ForeColor = Theme.Muted, Font = new Font("Segoe UI", 10F), AutoEllipsis = true };
            Button open = Theme.CreatePrimaryButton("Открыть модуль", 160, false);
            open.Anchor = AnchorStyles.Left;
            open.Click += delegate { action(); };

            TableLayoutPanel inner = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18, 14, 14, 14), ColumnCount = 1, RowCount = 4 };
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            inner.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
            inner.Controls.Add(no, 0, 0);
            inner.Controls.Add(h, 0, 1);
            inner.Controls.Add(d, 0, 2);
            inner.Controls.Add(open, 0, 3);
            card.Controls.Add(inner);
            card.Controls.Add(mark);
            _modulesPanel.Controls.Add(card);
        }

        private void AddGauge(string title, string value, Color color)
        {
            RoundedPanel item = new RoundedPanel { Width = 250, Height = 96, BackColor = Theme.CardAlt, Radius = 16, StrokeColor = Theme.Border, Margin = new Padding(0, 0, 10, 0), Padding = new Padding(12) };
            Panel dot = new Panel { Width = 6, Dock = DockStyle.Left, BackColor = color };
            Label v = new Label { Text = value, Dock = DockStyle.Bottom, Height = 38, ForeColor = Theme.Ink, Font = new Font("Segoe UI Semibold", value.Length > 10 ? 14F : 20F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
            Label t = new Label { Text = title, Dock = DockStyle.Top, Height = 28, ForeColor = Theme.Muted, Font = new Font("Segoe UI", 10F), TextAlign = ContentAlignment.MiddleLeft };
            item.Controls.Add(v);
            item.Controls.Add(t);
            item.Controls.Add(dot);
            _statsPanel.Controls.Add(item);
        }

        private void OpenModule(string name, Form form)
        {
            LogService.Log("Открытие модуля", name);
            using (form) form.ShowDialog(this);
            ApplyPeriodAndRefreshStats();
        }
    }
}
