using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
    public enum FieldKind { Text, Date, DateTime, Number, Lookup }

    public sealed class FieldDef
    {
        public string Column;
        public string Label;
        public FieldKind Kind;
        public bool Required;
        public bool ReadOnlyOnEdit;
        public decimal Max = 1000000000;
        public int Decimals = 0;
        public string LookupSql;
        public string ValueMember;
        public string DisplayMember;
    }

    public sealed class EntityConfig
    {
        public string Title;
        public string TableName;
        public string KeyColumn;
        public bool KeyIsIdentity = true;
        public string ListSql;
        public string SearchSql;
        public string DeleteSql;
        public List<FieldDef> Fields = new List<FieldDef>();
    }

    public static class EntityConfigs
    {
        public static EntityConfig Clients()
        {
            return new EntityConfig
            {
                Title = "Клиенты",
                TableName = "Client",
                KeyColumn = "client_id",
                ListSql = @"SELECT client_id AS [Код], last_name AS [Фамилия], first_name AS [Имя], middle_name AS [Отчество], birth_date AS [Дата рождения], inn AS [ИНН] FROM dbo.Client ORDER BY last_name, first_name",
                SearchSql = @"SELECT client_id AS [Код], last_name AS [Фамилия], first_name AS [Имя], middle_name AS [Отчество], birth_date AS [Дата рождения], inn AS [ИНН] FROM dbo.Client WHERE last_name LIKE @search OR first_name LIKE @search OR ISNULL(middle_name,'') LIKE @search OR ISNULL(inn,'') LIKE @search ORDER BY last_name, first_name",
                Fields =
                {
                    Text("last_name", "Фамилия", true), Text("first_name", "Имя", true), Text("middle_name", "Отчество", false),
                    Date("birth_date", "Дата рождения", false), Text("inn", "ИНН", false), Lookup("source_id", "Источник", false, "SELECT source_id, name FROM dbo.Ref_Source ORDER BY name", "source_id", "name")
                }
            };
        }

        public static EntityConfig Cars()
        {
            return new EntityConfig
            {
                Title = "Автомобили",
                TableName = "Car",
                KeyColumn = "vin",
                KeyIsIdentity = false,
                ListSql = @"SELECT c.vin AS [VIN], b.name AS [Марка], m.model_name AS [Модель], c.exterior_color AS [Цвет], c.[year] AS [Год], c.mileage AS [Пробег], c.sale_price AS [Цена], s.name AS [Статус] FROM dbo.Car c JOIN dbo.CarModel m ON m.model_id=c.model_id JOIN dbo.Ref_Brand b ON b.brand_id=m.brand_id JOIN dbo.Ref_CarStatus s ON s.status_id=c.status_id ORDER BY b.name, m.model_name, c.[year] DESC",
                SearchSql = @"SELECT c.vin AS [VIN], b.name AS [Марка], m.model_name AS [Модель], c.exterior_color AS [Цвет], c.[year] AS [Год], c.mileage AS [Пробег], c.sale_price AS [Цена], s.name AS [Статус] FROM dbo.Car c JOIN dbo.CarModel m ON m.model_id=c.model_id JOIN dbo.Ref_Brand b ON b.brand_id=m.brand_id JOIN dbo.Ref_CarStatus s ON s.status_id=c.status_id WHERE c.vin LIKE @search OR b.name LIKE @search OR m.model_name LIKE @search OR s.name LIKE @search ORDER BY b.name, m.model_name, c.[year] DESC",
                Fields =
                {
                    Text("vin", "VIN", true, true), Lookup("model_id", "Модель", true, "SELECT model_id, CAST(b.name + N' ' + model_name + ISNULL(N' ' + generation, N'') AS nvarchar(160)) AS title FROM dbo.CarModel m JOIN dbo.Ref_Brand b ON b.brand_id=m.brand_id ORDER BY b.name, model_name", "model_id", "title"),
                    Text("exterior_color", "Цвет кузова", true), Text("interior_color", "Цвет салона", false), Number("year", "Год выпуска", true, 2100, 0), Number("mileage", "Пробег", true, 10000000, 0),
                    Number("engine_volume_l", "Объем двигателя", false, 20, 1), Number("power_hp", "Мощность, л.с.", false, 5000, 0),
                    Lookup("transmission_type_id", "Коробка", false, "SELECT transmission_type_id, name FROM dbo.Ref_TransmissionType ORDER BY name", "transmission_type_id", "name"),
                    Lookup("drive_type_id", "Привод", false, "SELECT drive_type_id, name FROM dbo.Ref_DriveType ORDER BY name", "drive_type_id", "name"),
                    Lookup("fuel_type_id", "Топливо", false, "SELECT fuel_type_id, name FROM dbo.Ref_FuelType ORDER BY name", "fuel_type_id", "name"),
                    Lookup("trim_id", "Комплектация", false, "SELECT trim_id, name FROM dbo.Ref_Trim ORDER BY name", "trim_id", "name"),
                    Number("purchase_price", "Цена закупки", false, 1000000000, 2), Number("msrp_price", "Рекоменд. цена", false, 1000000000, 2), Number("sale_price", "Цена продажи", true, 1000000000, 2), Number("discount", "Скидка", false, 1000000000, 2),
                    Date("arrival_date", "Дата поступления", false), Lookup("status_id", "Статус", true, "SELECT status_id, name FROM dbo.Ref_CarStatus ORDER BY name", "status_id", "name")
                }
            };
        }

        public static EntityConfig Deals()
        {
            return new EntityConfig
            {
                Title = "Сделки",
                TableName = "Deal",
                KeyColumn = "deal_id",
                ListSql = @"SELECT d.deal_id AS [Код], d.contract_no AS [Договор], cl.last_name + N' ' + cl.first_name AS [Клиент], d.vin AS [VIN], e.last_name + N' ' + e.first_name AS [Менеджер], d.deal_date AS [Дата], pt.name AS [Оплата], d.final_price AS [Итоговая цена] FROM dbo.Deal d JOIN dbo.Client cl ON cl.client_id=d.client_id JOIN dbo.Employee e ON e.employee_id=d.manager_id JOIN dbo.Ref_PaymentType pt ON pt.payment_type_id=d.payment_type_id ORDER BY d.deal_date DESC, d.deal_id DESC",
                SearchSql = @"SELECT d.deal_id AS [Код], d.contract_no AS [Договор], cl.last_name + N' ' + cl.first_name AS [Клиент], d.vin AS [VIN], e.last_name + N' ' + e.first_name AS [Менеджер], d.deal_date AS [Дата], pt.name AS [Оплата], d.final_price AS [Итоговая цена] FROM dbo.Deal d JOIN dbo.Client cl ON cl.client_id=d.client_id JOIN dbo.Employee e ON e.employee_id=d.manager_id JOIN dbo.Ref_PaymentType pt ON pt.payment_type_id=d.payment_type_id WHERE d.contract_no LIKE @search OR d.vin LIKE @search OR cl.last_name LIKE @search OR cl.first_name LIKE @search ORDER BY d.deal_date DESC, d.deal_id DESC",
                Fields =
                {
                    Text("contract_no", "Номер договора", true), Lookup("client_id", "Клиент", true, "SELECT client_id, last_name + N' ' + first_name + ISNULL(N' ' + middle_name, N'') AS title FROM dbo.Client ORDER BY last_name, first_name", "client_id", "title"),
                    Lookup("vin", "Автомобиль", true, "SELECT vin, vin AS title FROM dbo.Car ORDER BY vin", "vin", "title"), Lookup("manager_id", "Менеджер", true, "SELECT employee_id, last_name + N' ' + first_name AS title FROM dbo.Employee ORDER BY last_name, first_name", "employee_id", "title"),
                    Date("deal_date", "Дата договора", true), Date("transfer_date", "Дата передачи", false), Lookup("payment_type_id", "Форма оплаты", true, "SELECT payment_type_id, name FROM dbo.Ref_PaymentType ORDER BY name", "payment_type_id", "name"),
                    Number("down_payment", "Первоначальный взнос", false, 1000000000, 2), Number("credit_amount", "Сумма кредита/лизинга", false, 1000000000, 2), Lookup("bank_id", "Банк-партнер", false, "SELECT bank_id, name FROM dbo.Ref_BankPartner ORDER BY name", "bank_id", "name"), Number("final_price", "Итоговая цена", true, 1000000000, 2)
                }
            };
        }

        public static EntityConfig TestDrives()
        {
            return new EntityConfig
            {
                Title = "Тест-драйвы",
                TableName = "TestDrive",
                KeyColumn = "test_drive_id",
                ListSql = @"SELECT td.test_drive_id AS [Код], cl.last_name + N' ' + cl.first_name AS [Клиент], td.vin AS [VIN], e.last_name + N' ' + e.first_name AS [Сопровождающий], td.planned_start AS [Плановое начало], td.planned_duration_min AS [Минут], r.name AS [Результат] FROM dbo.TestDrive td JOIN dbo.Client cl ON cl.client_id=td.client_id JOIN dbo.Employee e ON e.employee_id=td.employee_id LEFT JOIN dbo.Ref_TestDriveResult r ON r.result_id=td.result_id ORDER BY td.planned_start DESC",
                SearchSql = @"SELECT td.test_drive_id AS [Код], cl.last_name + N' ' + cl.first_name AS [Клиент], td.vin AS [VIN], e.last_name + N' ' + e.first_name AS [Сопровождающий], td.planned_start AS [Плановое начало], td.planned_duration_min AS [Минут], r.name AS [Результат] FROM dbo.TestDrive td JOIN dbo.Client cl ON cl.client_id=td.client_id JOIN dbo.Employee e ON e.employee_id=td.employee_id LEFT JOIN dbo.Ref_TestDriveResult r ON r.result_id=td.result_id WHERE td.vin LIKE @search OR cl.last_name LIKE @search OR cl.first_name LIKE @search ORDER BY td.planned_start DESC",
                Fields =
                {
                    Lookup("client_id", "Клиент", true, "SELECT client_id, last_name + N' ' + first_name AS title FROM dbo.Client ORDER BY last_name, first_name", "client_id", "title"), Lookup("vin", "Автомобиль", true, "SELECT vin, vin AS title FROM dbo.Car ORDER BY vin", "vin", "title"), Lookup("employee_id", "Сопровождающий", true, "SELECT employee_id, last_name + N' ' + first_name AS title FROM dbo.Employee ORDER BY last_name, first_name", "employee_id", "title"), DateTime("planned_start", "Плановое начало", true), Number("planned_duration_min", "Длительность, мин", true, 240, 0), DateTime("actual_start", "Факт. начало", false), DateTime("actual_end", "Факт. окончание", false), Lookup("result_id", "Результат", false, "SELECT result_id, name FROM dbo.Ref_TestDriveResult ORDER BY name", "result_id", "name"), Text("notes", "Заметки", false)
                }
            };
        }

        public static EntityConfig Services()
        {
            return new EntityConfig
            {
                Title = "Услуги",
                TableName = "ServiceOrder",
                KeyColumn = "service_order_id",
                ListSql = @"SELECT so.service_order_id AS [Код], sc.name AS [Услуга], so.vin AS [VIN], e.last_name + N' ' + e.first_name AS [Исполнитель], so.created_at AS [Создана], so.done_date AS [Выполнена], so.cost AS [Стоимость] FROM dbo.ServiceOrder so JOIN dbo.ServiceCatalog sc ON sc.service_catalog_id=so.service_catalog_id JOIN dbo.Employee e ON e.employee_id=so.employee_id ORDER BY so.created_at DESC, so.service_order_id DESC",
                SearchSql = @"SELECT so.service_order_id AS [Код], sc.name AS [Услуга], so.vin AS [VIN], e.last_name + N' ' + e.first_name AS [Исполнитель], so.created_at AS [Создана], so.done_date AS [Выполнена], so.cost AS [Стоимость] FROM dbo.ServiceOrder so JOIN dbo.ServiceCatalog sc ON sc.service_catalog_id=so.service_catalog_id JOIN dbo.Employee e ON e.employee_id=so.employee_id WHERE sc.name LIKE @search OR so.vin LIKE @search OR e.last_name LIKE @search ORDER BY so.created_at DESC, so.service_order_id DESC",
                Fields =
                {
                    Lookup("service_catalog_id", "Услуга", true, "SELECT service_catalog_id, name FROM dbo.ServiceCatalog ORDER BY name", "service_catalog_id", "name"), Lookup("vin", "Автомобиль", true, "SELECT vin, vin AS title FROM dbo.Car ORDER BY vin", "vin", "title"), Lookup("employee_id", "Исполнитель", true, "SELECT employee_id, last_name + N' ' + first_name AS title FROM dbo.Employee ORDER BY last_name, first_name", "employee_id", "title"), Lookup("deal_id", "Сделка", false, "SELECT deal_id, contract_no AS title FROM dbo.Deal ORDER BY deal_date DESC", "deal_id", "title"), Date("created_at", "Дата создания", true), Date("planned_date", "Плановая дата", false), Date("done_date", "Дата выполнения", false), Number("cost", "Стоимость", true, 1000000000, 2), Text("notes", "Заметки", false)
                }
            };
        }

        public static EntityConfig Employees()
        {
            return new EntityConfig
            {
                Title = "Сотрудники",
                TableName = "Employee",
                KeyColumn = "employee_id",
                ListSql = @"SELECT e.employee_id AS [Код], e.last_name AS [Фамилия], e.first_name AS [Имя], e.middle_name AS [Отчество], p.name AS [Должность], e.phone AS [Телефон], e.email AS [Почта], e.hire_date AS [Дата приема] FROM dbo.Employee e JOIN dbo.Ref_Position p ON p.position_id=e.position_id ORDER BY e.last_name, e.first_name",
                SearchSql = @"SELECT e.employee_id AS [Код], e.last_name AS [Фамилия], e.first_name AS [Имя], e.middle_name AS [Отчество], p.name AS [Должность], e.phone AS [Телефон], e.email AS [Почта], e.hire_date AS [Дата приема] FROM dbo.Employee e JOIN dbo.Ref_Position p ON p.position_id=e.position_id WHERE e.last_name LIKE @search OR e.first_name LIKE @search OR ISNULL(e.email,'') LIKE @search OR p.name LIKE @search ORDER BY e.last_name, e.first_name",
                Fields =
                {
                    Text("last_name", "Фамилия", true), Text("first_name", "Имя", true), Text("middle_name", "Отчество", false), Lookup("position_id", "Должность", true, "SELECT position_id, name FROM dbo.Ref_Position ORDER BY name", "position_id", "name"), Text("phone", "Телефон", false), Text("email", "Почта", false), Date("hire_date", "Дата приема", true), Date("fire_date", "Дата увольнения", false)
                }
            };
        }

        public static FieldDef Text(string col, string label, bool required, bool readOnlyOnEdit = false) { return new FieldDef { Column = col, Label = label, Kind = FieldKind.Text, Required = required, ReadOnlyOnEdit = readOnlyOnEdit }; }
        public static FieldDef Date(string col, string label, bool required) { return new FieldDef { Column = col, Label = label, Kind = FieldKind.Date, Required = required }; }
        public static FieldDef DateTime(string col, string label, bool required) { return new FieldDef { Column = col, Label = label, Kind = FieldKind.DateTime, Required = required }; }
        public static FieldDef Number(string col, string label, bool required, decimal max, int decimals) { return new FieldDef { Column = col, Label = label, Kind = FieldKind.Number, Required = required, Max = max, Decimals = decimals }; }
        public static FieldDef Lookup(string col, string label, bool required, string sql, string value, string display) { return new FieldDef { Column = col, Label = label, Kind = FieldKind.Lookup, Required = required, LookupSql = sql, ValueMember = value, DisplayMember = display }; }
    }

    public class EntityListForm : FormBase
    {
        private readonly UserAccount _user;
        private readonly EntityConfig _config;
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill };
        private readonly TextBox _txtSearch = Theme.CreateTextBox(260);
        private readonly Label _lblStatus = new Label();

        public EntityListForm(UserAccount user, EntityConfig config)
        {
            _user = user;
            _config = config;
            Theme.StyleForm(this);
            Text = config.Title;
            Width = 1240;
            Height = 760;
            StartPosition = FormStartPosition.CenterParent;
            Theme.StyleGrid(_grid);

            Panel page = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0), BackColor = Theme.AppBack };
            Controls.Add(page);

            Panel toolbar = new Panel { Dock = DockStyle.Top, Height = 76, BackColor = Theme.Card, Padding = new Padding(10, 12, 10, 12) };
            FlowLayoutPanel row = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = false };
            row.Controls.Add(new Label { Text = "Поиск:", Width = 60, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Theme.Ink, Padding = new Padding(0, 7, 0, 0) });
            _txtSearch.Width = 240;
            _txtSearch.Margin = new Padding(0, 0, 8, 0);
            _txtSearch.TextChanged += delegate { LoadData(); };
            row.Controls.Add(_txtSearch);
            Button add = Theme.CreatePrimaryButton("Добавить", 110, false);
            Button edit = Theme.CreateSecondaryButton("Изменить", 110);
            Button del = Theme.CreateSecondaryButton("Удалить", 110);
            add.Margin = new Padding(0, 0, 8, 0);
            edit.Margin = new Padding(0, 0, 8, 0);
            del.Margin = new Padding(0, 0, 8, 0);
            add.Click += delegate { if (!CanWrite()) { ShowAccessDenied(); return; } OpenEditor(null); };
            edit.Click += delegate { if (!CanWrite()) { ShowAccessDenied(); return; } object key = SelectedKey(_grid); if (key != null) OpenEditor(key); };
            del.Click += delegate { if (!CanWrite()) { ShowAccessDenied(); return; } DeleteSelected(); };
            row.Controls.Add(add); row.Controls.Add(edit); row.Controls.Add(del);
            toolbar.Controls.Add(row);

            Panel tableHeader = new Panel { Dock = DockStyle.Top, Height = 38, BackColor = Theme.CardAlt, Padding = new Padding(8, 6, 8, 6) };
            tableHeader.Controls.Add(new Label { Text = config.Title, Dock = DockStyle.Left, Width = 260, ForeColor = Theme.Ink, Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft });
            _lblStatus.Dock = DockStyle.Right;
            _lblStatus.Width = 260;
            _lblStatus.ForeColor = Theme.Muted;
            _lblStatus.TextAlign = ContentAlignment.MiddleRight;
            tableHeader.Controls.Add(_lblStatus);
            page.Controls.Add(_grid);
            page.Controls.Add(tableHeader);
            page.Controls.Add(toolbar);

            Load += delegate { Theme.ApplyCurrentTheme(this); LoadData(); };
        }

        private bool CanWrite()
        {
            return true;
        }

        private void ShowAccessDenied()
        {
            MessageBox.Show("У вас нет прав на изменение данных в этом разделе.\nДоступен только просмотр.", "Недостаточно прав", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void ShowAccessDenied()
        {
            MessageBox.Show("У вас нет прав на изменение данных в этом разделе.\nДоступен только просмотр.", "Недостаточно прав", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void LoadData()
        {
            try
            {
                string text = _txtSearch.Text.Trim();
                if (text.Length == 0) _grid.DataSource = Db.Query(_config.ListSql);
                else _grid.DataSource = Db.Query(_config.SearchSql, new SqlParameter("@search", "%" + text + "%"));
                if (_grid.Columns.Count > 0) _grid.Columns[0].Visible = false;
                int rows = _grid.Rows.Count;
                _lblStatus.Text = rows.ToString() + " записей";
            }
            catch (Exception ex) { MessageBox.Show("Ошибка загрузки данных.\n" + ex.Message); }
        }

        private void OpenEditor(object key)
        {
            using (EntityEditForm f = new EntityEditForm(_config, key))
                if (f.ShowDialog(this) == DialogResult.OK) LoadData();
        }

        private void DeleteSelected()
        {
            object key = SelectedKey(_grid); if (key == null) return;
            if (MessageBox.Show("Удалить выбранную запись?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                Db.Execute("DELETE FROM dbo." + _config.TableName + " WHERE " + _config.KeyColumn + "=@id", new SqlParameter("@id", key));
                LoadData();
            }
            catch (Exception ex) { MessageBox.Show("Не удалось удалить запись.\n" + ex.Message); }
        }
    }

    public class EntityEditForm : Form
    {
        private readonly EntityConfig _config;
        private readonly object _key;
        private readonly Dictionary<string, Control> _controls = new Dictionary<string, Control>();
        private CheckBox _chkCreateTestDrive;
        private DateTimePicker _dtTestDrive;

        public EntityEditForm(EntityConfig config, object key)
        {
            _config = config; _key = key;
            Theme.StyleForm(this);
            Text = key == null ? "Новая карточка: " + config.Title : "Карточка записи: " + config.Title;
            Width = 860; Height = 720; StartPosition = FormStartPosition.CenterParent;

            Panel page = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24), BackColor = Theme.AppBack };
            Controls.Add(page);

            RoundedPanel shell = Theme.CreateCard(0);
            shell.Dock = DockStyle.Fill;
            shell.Padding = new Padding(0);
            page.Controls.Add(shell);

            Panel header = new Panel { Dock = DockStyle.Top, Height = 92, BackColor = Theme.Ink, Padding = new Padding(26, 14, 26, 14) };
            header.Controls.Add(new Label { Text = key == null ? "Создание записи" : "Редактирование записи", Dock = DockStyle.Top, Height = 34, ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold) });
            header.Controls.Add(new Label { Text = config.Title + " / карточка данных автосалона", Dock = DockStyle.Bottom, Height = 28, ForeColor = Color.FromArgb(191, 214, 238) });
            shell.Controls.Add(header);

            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, Padding = new Padding(24), AutoScroll = true, BackColor = Theme.Card };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 26));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 26));

            int index = 0;
            foreach (var field in config.Fields)
            {
                var control = CreateControl(field);
                control.Dock = DockStyle.Fill;
                _controls[field.Column] = control;
                AddField(table, field.Label + (field.Required ? " *" : string.Empty), control, index++);
            }
            if (_config.TableName == "Deal") AddDealAssistBlock(table);

            Load += delegate { Theme.ApplyCurrentTheme(this); };

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 76, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(18), BackColor = Theme.CardAlt, WrapContents = false };
            var save = Theme.CreatePrimaryButton("Сохранить карточку", 170);
            var cancel = Theme.CreateSecondaryButton("Отмена", 130);
            save.Click += delegate { SaveData(); };
            cancel.Click += delegate { DialogResult = DialogResult.Cancel; Close(); };
            buttons.Controls.Add(save); buttons.Controls.Add(cancel);
            shell.Controls.Add(table); shell.Controls.Add(buttons);
            if (key != null) LoadData();
        }

        private Control CreateControl(FieldDef f)
        {
            if (f.Kind == FieldKind.Text)
            {
                var tb = Theme.CreateTextBox(330);
                if (_key != null && f.ReadOnlyOnEdit) tb.ReadOnly = true;
                return tb;
            }
            if (f.Kind == FieldKind.Date || f.Kind == FieldKind.DateTime)
            {
                var dt = Theme.CreateDatePicker(220);
                if (f.Kind == FieldKind.DateTime) dt.Format = DateTimePickerFormat.Custom;
                if (f.Kind == FieldKind.DateTime) dt.CustomFormat = "dd.MM.yyyy HH:mm";
                dt.ShowCheckBox = !f.Required;
                dt.Checked = f.Required;
                return dt;
            }
            if (f.Kind == FieldKind.Number)
            {
                var n = Theme.CreateNumeric(220, f.Max, f.Decimals);
                return n;
            }
            var cb = Theme.CreateComboBox(330);
            cb.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cb.AutoCompleteSource = AutoCompleteSource.ListItems;
            if (!f.Required)
            {
                var source = Db.Query(f.LookupSql);
                var dt = source.Clone();
                foreach (DataColumn col in dt.Columns) col.AllowDBNull = true;
                var row = dt.NewRow();
                row[f.ValueMember] = DBNull.Value;
                row[f.DisplayMember] = "-- не выбрано --";
                dt.Rows.Add(row);
                foreach (DataRow sourceRow in source.Rows) dt.ImportRow(sourceRow);
                cb.DataSource = dt;
                cb.ValueMember = f.ValueMember;
                cb.DisplayMember = f.DisplayMember;
            }
            else LookupService.Fill(cb, f.LookupSql, f.ValueMember, f.DisplayMember);
            return cb;
        }

        private void AddField(TableLayoutPanel t, string name, Control control, int index)
        {
            int row = index / 2;
            int col = (index % 2) * 2;
            while (t.RowCount <= row)
            {
                t.RowCount++;
                t.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            }
            Label label = new Label { Text = name, Dock = DockStyle.Fill, Padding = new Padding(0, 10, 10, 0), ForeColor = Theme.Muted, TextAlign = ContentAlignment.TopLeft };
            Panel inputWrap = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 6, 18, 6) };
            inputWrap.Controls.Add(control);
            t.Controls.Add(label, col, row);
            t.Controls.Add(inputWrap, col + 1, row);
        }

        private void LoadData()
        {
            var dt = Db.Query("SELECT * FROM dbo." + _config.TableName + " WHERE " + _config.KeyColumn + "=@id", new SqlParameter("@id", _key));
            if (dt.Rows.Count == 0) return;
            var row = dt.Rows[0];
            foreach (var f in _config.Fields)
            {
                if (!dt.Columns.Contains(f.Column)) continue;
                SetValue(_controls[f.Column], f, row[f.Column]);
            }
        }

        private void SetValue(Control c, FieldDef f, object value)
        {
            if (value == DBNull.Value)
            {
                var dp = c as DateTimePicker;
                if (dp != null) { dp.Checked = false; return; }
                var cb = c as ComboBox;
                if (cb != null) { cb.SelectedIndex = 0; return; }
                c.Text = string.Empty;
                return;
            }
            if (f.Kind == FieldKind.Text) c.Text = value.ToString();
            else if (f.Kind == FieldKind.Number) ((NumericUpDown)c).Value = Convert.ToDecimal(value);
            else if (f.Kind == FieldKind.Date || f.Kind == FieldKind.DateTime)
            {
                var dp = (DateTimePicker)c; dp.Checked = true; dp.Value = Convert.ToDateTime(value);
            }
            else ((ComboBox)c).SelectedValue = value;
        }

        private object GetValue(FieldDef f)
        {
            var c = _controls[f.Column];
            if (f.Kind == FieldKind.Text)
            {
                var text = c.Text.Trim();
                return text.Length == 0 ? (object)DBNull.Value : text;
            }
            if (f.Kind == FieldKind.Number) return ((NumericUpDown)c).Value;
            if (f.Kind == FieldKind.Date || f.Kind == FieldKind.DateTime)
            {
                var dp = (DateTimePicker)c;
                if (!f.Required && !dp.Checked) return DBNull.Value;
                return f.Kind == FieldKind.Date ? (object)dp.Value.Date : dp.Value;
            }
            var cb = (ComboBox)c;
            if (cb.SelectedValue == null || cb.SelectedValue == DBNull.Value) return DBNull.Value;
            return cb.SelectedValue;
        }

        private void SaveData()
        {
            try
            {
                if (!ValidateFields()) return;
                var fields = _config.Fields.Where(f => !(_key != null && f.Column == _config.KeyColumn && !_config.KeyIsIdentity)).ToList();
                var parameters = new List<SqlParameter>();
                foreach (var f in fields) parameters.Add(new SqlParameter("@" + f.Column, GetValue(f)));

                if (_key == null)
                {
                    var insertFields = _config.KeyIsIdentity ? fields : _config.Fields;
                    parameters.Clear();
                    foreach (var f in insertFields) parameters.Add(new SqlParameter("@" + f.Column, GetValue(f)));
                    var cols = string.Join(",", insertFields.Select(f => f.Column));
                    var vals = string.Join(",", insertFields.Select(f => "@" + f.Column));
                    Db.Execute("INSERT INTO dbo." + _config.TableName + "(" + cols + ") VALUES(" + vals + ")", parameters.ToArray());
                    if (_config.TableName == "Deal") TryCreateRelatedTestDrive();
                }
                else
                {
                    var set = string.Join(",", fields.Select(f => f.Column + "=@" + f.Column));
                    parameters.Add(new SqlParameter("@id", _key));
                    Db.Execute("UPDATE dbo." + _config.TableName + " SET " + set + " WHERE " + _config.KeyColumn + "=@id", parameters.ToArray());
                }
                DialogResult = DialogResult.OK; Close();
            }
            catch (SqlException ex) { MessageBox.Show("Ошибка базы данных.\n" + BuildSqlHint(ex) + "\n\n" + ex.Message); }
            catch (Exception ex) { MessageBox.Show("Ошибка сохранения записи.\nПроверьте обязательные поля и корректность значений.\n\n" + ex.Message); }
        }

        private bool ValidateFields()
        {
            foreach (var f in _config.Fields)
            {
                var val = GetValue(f);
                if (f.Required && (val == DBNull.Value || string.IsNullOrWhiteSpace(val.ToString())))
                {
                    MessageBox.Show("Заполните поле: " + f.Label); return false;
                }
            }
            if (_config.TableName == "Client")
            {
                var inn = GetText("inn");
                if (inn.Length > 0 && !ValidationRules.IsInn12(inn)) { MessageBox.Show("ИНН клиента должен содержать 12 цифр."); return false; }
                var bd = GetDate("birth_date");
                if (bd.HasValue && bd.Value.Date > DateTime.Today) { MessageBox.Show("Дата рождения не может быть в будущем."); return false; }
            }
            if (_config.TableName == "Employee")
            {
                var phone = GetText("phone"); string normalized;
                if (phone.Length > 0 && !ValidationRules.TryNormalizePhoneRu(phone, out normalized)) { MessageBox.Show("Телефон должен быть в формате +7(XXX)XXX-XX-XX."); return false; }
                var email = GetText("email");
                if (email.Length > 0 && !ValidationRules.IsEmail(email)) { MessageBox.Show("Введите корректную почту сотрудника."); return false; }
                var hire = GetDate("hire_date"); var fire = GetDate("fire_date");
                if (hire.HasValue && fire.HasValue && fire.Value.Date < hire.Value.Date) { MessageBox.Show("Дата увольнения не может быть раньше даты приема."); return false; }
            }
            if (_config.TableName == "Car")
            {
                var vin = GetText("vin").ToUpperInvariant();
                if (!ValidationRules.IsVin(vin)) { MessageBox.Show("VIN должен содержать 17 латинских букв и цифр без I, O, Q."); return false; }
                var year = Convert.ToInt32(GetValue(_config.Fields.First(f => f.Column == "year")));
                if (year < 1980 || year > DateTime.Today.Year + 1) { MessageBox.Show("Год выпуска указан некорректно."); return false; }
                var arrival = GetDate("arrival_date");
                if (arrival.HasValue && arrival.Value.Date > DateTime.Today) { MessageBox.Show("Дата поступления не может быть позже текущей даты."); return false; }
            }
            if (_config.TableName == "Deal")
            {
                var deal = GetDate("deal_date"); var transfer = GetDate("transfer_date");
                if (deal.HasValue && deal.Value.Date > DateTime.Today) { MessageBox.Show("Дата договора не может быть позже текущей даты."); return false; }
                if (deal.HasValue && transfer.HasValue && transfer.Value.Date < deal.Value.Date) { MessageBox.Show("Дата передачи не может быть раньше даты договора."); return false; }
                var finalPrice = Convert.ToDecimal(GetValue(_config.Fields.First(f => f.Column == "final_price")));
                if (finalPrice <= 0) { MessageBox.Show("Итоговая цена сделки должна быть больше нуля."); return false; }
            }
            if (_config.TableName == "TestDrive")
            {
                var duration = Convert.ToInt32(GetValue(_config.Fields.First(f => f.Column == "planned_duration_min")));
                if (duration < 10 || duration > 240) { MessageBox.Show("Длительность тест-драйва должна быть от 10 до 240 минут."); return false; }
                var start = GetDate("actual_start"); var end = GetDate("actual_end");
                if (start.HasValue && end.HasValue && end.Value < start.Value) { MessageBox.Show("Фактическое окончание не может быть раньше начала."); return false; }
            }
            if (_config.TableName == "ServiceOrder")
            {
                var planned = GetDate("planned_date"); var done = GetDate("done_date");
                if (planned.HasValue && done.HasValue && done.Value.Date < planned.Value.Date) { MessageBox.Show("Дата выполнения услуги не может быть раньше плановой даты."); return false; }
            }
            return true;
        }

        private string GetText(string column)
        {
            Control c;
            return _controls.TryGetValue(column, out c) ? c.Text.Trim() : string.Empty;
        }

        private DateTime? GetDate(string column)
        {
            Control c;
            if (!_controls.TryGetValue(column, out c)) return null;
            var dp = c as DateTimePicker;
            if (dp == null || (!dp.Checked && dp.ShowCheckBox)) return null;
            return dp.Value;
        }

        private void AddDealAssistBlock(TableLayoutPanel t)
        {
            _chkCreateTestDrive = new CheckBox { Text = "Клиент хочет тест-драйв (создать сразу)", AutoSize = true, ForeColor = Theme.Muted };
            _dtTestDrive = Theme.CreateDatePicker(220);
            _dtTestDrive.Format = DateTimePickerFormat.Custom;
            _dtTestDrive.CustomFormat = "dd.MM.yyyy HH:mm";
            _dtTestDrive.Value = DateTime.Now.AddHours(2);
            AddField(t, "Тест-драйв", _chkCreateTestDrive, t.RowCount);
            AddField(t, "Плановое начало", _dtTestDrive, t.RowCount);
        }

        private void TryCreateRelatedTestDrive()
        {
            if (_chkCreateTestDrive == null || !_chkCreateTestDrive.Checked) return;
            if (_dtTestDrive.Value < DateTime.Now.AddMinutes(-1))
            {
                MessageBox.Show("Тест-драйв не создан: плановое время не может быть в прошлом.");
                return;
            }
            object client = GetValue(_config.Fields.First(f => f.Column == "client_id"));
            object vin = GetValue(_config.Fields.First(f => f.Column == "vin"));
            object manager = GetValue(_config.Fields.First(f => f.Column == "manager_id"));
            if (client == DBNull.Value || vin == DBNull.Value || manager == DBNull.Value)
            {
                MessageBox.Show("Тест-драйв не создан: укажите клиента, автомобиль и менеджера в сделке.");
                return;
            }
            Db.Execute(
                "INSERT INTO dbo.TestDrive(client_id, vin, employee_id, planned_start, planned_duration_min) VALUES(@c,@v,@e,@s,@d)",
                new SqlParameter("@c", client),
                new SqlParameter("@v", vin),
                new SqlParameter("@e", manager),
                new SqlParameter("@s", _dtTestDrive.Value),
                new SqlParameter("@d", 30));
        }

        private string BuildSqlHint(SqlException ex)
        {
            if (ex.Number == 2627 || ex.Number == 2601) return "Дублирующее значение. Проверьте уникальные поля (например VIN, номер договора, ИНН).";
            if (ex.Number == 547) return "Нарушены связи данных. Проверьте, что выбранные клиент/автомобиль/сотрудник существуют.";
            if (ex.Number == 515) return "Есть незаполненные обязательные поля.";
            return "Проверьте введенные данные и повторите.";
        }
    }
}
