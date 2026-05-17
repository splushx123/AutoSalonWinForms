using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Data.SqlClient;

namespace AutoInsuranceWinForms
{
    public class ReportsForm : Form
    {
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill };
        private readonly ComboBox _reports = Theme.CreateComboBox(360);
        private readonly DateTimePicker _dtFrom = Theme.CreateDatePicker(130);
        private readonly DateTimePicker _dtTo = Theme.CreateDatePicker(130);
        private readonly Label _reportTitle = new Label();
        private readonly Label _rowCount = new Label();
        private static readonly Encoding ExportEncoding = new UTF8Encoding(true);

        public ReportsForm()
        {
            Theme.StyleForm(this);
            Text = "Аналитика автосалона";
            Width = 1240; Height = 760; StartPosition = FormStartPosition.CenterParent;
            Theme.StyleGrid(_grid);

            Panel page = new Panel { Dock = DockStyle.Fill, Padding = new Padding(22), BackColor = Theme.AppBack };
            Controls.Add(page);

            Panel toolbar = new Panel { Dock = DockStyle.Top, Height = 76, BackColor = Theme.Card, Padding = new Padding(12, 12, 12, 12) };
            FlowLayoutPanel row = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
            row.Controls.Add(new Label { Text = "Отчет:", Width = 66, Height = 44, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Theme.Ink, Margin = new Padding(0, 0, 8, 0) });
            _reports.DropDownStyle = ComboBoxStyle.DropDownList;
            _reports.Width = 420;
            _reports.Margin = new Padding(0, 6, 12, 0);
            _reports.Items.AddRange(new object[] { "Автомобили по статусам", "Продажи по менеджерам", "Выручка по формам оплаты", "Тест-драйвы по результатам", "Выполненные услуги", "Доступные автомобили" });
            _reports.SelectedIndex = 0;
            _reports.SelectedIndexChanged += delegate { BuildReport(); };
            row.Controls.Add(_reports);
            row.Controls.Add(new Label { Text = "Период:", Width = 72, Height = 44, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Theme.Ink, Margin = new Padding(0, 0, 6, 0) });
            _dtFrom.Margin = new Padding(0, 8, 6, 0);
            _dtTo.Margin = new Padding(0, 8, 12, 0);
            _dtFrom.Value = new DateTime(DateTime.Today.Year, 1, 1);
            _dtTo.Value = DateTime.Today;
            _dtFrom.ValueChanged += delegate { BuildReport(); };
            _dtTo.ValueChanged += delegate { BuildReport(); };
            row.Controls.Add(_dtFrom);
            row.Controls.Add(new Label { Text = "—", Width = 18, Height = 44, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Theme.Muted, Margin = new Padding(0, 0, 6, 0) });
            row.Controls.Add(_dtTo);
            Button apply = Theme.CreateSecondaryButton("Показать", 110);
            apply.Margin = new Padding(0, 0, 10, 0);
            apply.Click += delegate { BuildReport(); };
            Button export = Theme.CreatePrimaryButton("Экспорт результата", 180);
            export.Margin = new Padding(0, 0, 0, 0);
            export.Click += delegate { Export(); };
            row.Controls.Add(apply);
            row.Controls.Add(export);
            toolbar.Controls.Add(row);

            RoundedPanel work = Theme.CreateCard(0);
            work.Dock = DockStyle.Fill;
            work.Padding = new Padding(1);
            page.Controls.Add(work);
            Panel header = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = Theme.Ink, Padding = new Padding(22, 12, 22, 12) };
            _reportTitle.Dock = DockStyle.Left;
            _reportTitle.Width = 480;
            _reportTitle.ForeColor = Color.White;
            _reportTitle.Font = new Font("Segoe UI Semibold", 17F, FontStyle.Bold);
            _reportTitle.TextAlign = ContentAlignment.MiddleLeft;
            _rowCount.Dock = DockStyle.Right;
            _rowCount.Width = 220;
            _rowCount.ForeColor = Color.FromArgb(196, 217, 238);
            _rowCount.TextAlign = ContentAlignment.MiddleRight;
            header.Controls.Add(_rowCount);
            header.Controls.Add(_reportTitle);
            Panel subHeader = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = Theme.CardAlt, Padding = new Padding(12, 7, 12, 7) };
            subHeader.Controls.Add(new Label { Text = "Результаты отчета", Dock = DockStyle.Left, Width = 220, ForeColor = Theme.Ink, Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft });
            work.Controls.Add(_grid);
            work.Controls.Add(subHeader);
            work.Controls.Add(header);
            page.Controls.Add(toolbar);
            Load += delegate { Theme.ApplyCurrentTheme(this); BuildReport(); };
        }

        private void BuildReport()
        {
            try
            {
                DateTime from = _dtFrom.Value.Date;
                DateTime to = _dtTo.Value.Date;
                if (from > to)
                {
                    MessageBox.Show("Дата начала периода не может быть позже даты окончания.");
                    return;
                }
                DateTime toExclusive = to.AddDays(1);
                _reportTitle.Text = _reports.SelectedItem == null ? "Отчет" : _reports.SelectedItem.ToString();
                switch (_reports.SelectedIndex)
                {
                    case 0:
                        _grid.DataSource = Db.Query(@"SELECT s.name AS [Статус], COUNT(*) AS [Количество автомобилей] FROM dbo.Car c JOIN dbo.Ref_CarStatus s ON s.status_id=c.status_id WHERE c.arrival_date>=@from AND c.arrival_date<@to GROUP BY s.name ORDER BY [Количество автомобилей] DESC",
                            new SqlParameter("@from", from), new SqlParameter("@to", toExclusive));
                        break;
                    case 1:
                        _grid.DataSource = Db.Query(@"SELECT e.last_name + N' ' + e.first_name AS [Менеджер], COUNT(*) AS [Сделок], SUM(d.final_price) AS [Сумма продаж] FROM dbo.Deal d JOIN dbo.Employee e ON e.employee_id=d.manager_id WHERE d.deal_date>=@from AND d.deal_date<@to GROUP BY e.last_name, e.first_name ORDER BY [Сумма продаж] DESC",
                            new SqlParameter("@from", from), new SqlParameter("@to", toExclusive));
                        break;
                    case 2:
                        _grid.DataSource = Db.Query(@"SELECT pt.name AS [Форма оплаты], COUNT(*) AS [Сделок], CAST(AVG(d.final_price) AS decimal(18,2)) AS [Средняя цена], SUM(d.final_price) AS [Выручка] FROM dbo.Deal d JOIN dbo.Ref_PaymentType pt ON pt.payment_type_id=d.payment_type_id WHERE d.deal_date>=@from AND d.deal_date<@to GROUP BY pt.name ORDER BY [Выручка] DESC",
                            new SqlParameter("@from", from), new SqlParameter("@to", toExclusive));
                        break;
                    case 3:
                        _grid.DataSource = Db.Query(@"SELECT ISNULL(r.name, N'Без результата') AS [Результат], COUNT(*) AS [Количество] FROM dbo.TestDrive td LEFT JOIN dbo.Ref_TestDriveResult r ON r.result_id=td.result_id WHERE td.planned_start>=@from AND td.planned_start<@to GROUP BY r.name ORDER BY [Количество] DESC",
                            new SqlParameter("@from", from), new SqlParameter("@to", toExclusive));
                        break;
                    case 4:
                        _grid.DataSource = Db.Query(@"SELECT sc.name AS [Услуга], COUNT(*) AS [Количество], SUM(so.cost) AS [Сумма] FROM dbo.ServiceOrder so JOIN dbo.ServiceCatalog sc ON sc.service_catalog_id=so.service_catalog_id WHERE so.created_at>=@from AND so.created_at<@to GROUP BY sc.name ORDER BY [Сумма] DESC",
                            new SqlParameter("@from", from), new SqlParameter("@to", toExclusive));
                        break;
                    default:
                        _grid.DataSource = Db.Query(@"SELECT c.vin AS [VIN], b.name AS [Марка], m.model_name AS [Модель], c.[year] AS [Год], c.exterior_color AS [Цвет], c.sale_price AS [Цена], s.name AS [Статус] FROM dbo.Car c JOIN dbo.CarModel m ON m.model_id=c.model_id JOIN dbo.Ref_Brand b ON b.brand_id=m.brand_id JOIN dbo.Ref_CarStatus s ON s.status_id=c.status_id WHERE s.name IN (N'В продаже', N'На складе') AND c.arrival_date>=@from AND c.arrival_date<@to ORDER BY b.name, m.model_name",
                            new SqlParameter("@from", from), new SqlParameter("@to", toExclusive));
                        break;
                }
                _rowCount.Text = _grid.Rows.Count.ToString() + " строк";
            }
            catch (Exception ex) { MessageBox.Show("Не удалось построить отчет.\n" + ex.Message); }
        }

        private void Export()
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV (*.csv)|*.csv|Text (*.txt)|*.txt|JSON (*.json)|*.json|TSV (*.tsv)|*.tsv|XML (*.xml)|*.xml";
                sfd.FileName = "autosalon_report.csv";
                if (sfd.ShowDialog(this) != DialogResult.OK) return;
                string ext = Path.GetExtension(sfd.FileName).ToLowerInvariant();
                if (ext == ".json") ExportJson(sfd.FileName);
                else if (ext == ".xml") ExportXml(sfd.FileName);
                else ExportDelimited(sfd.FileName, ext == ".tsv" ? "\t" : ext == ".txt" ? " | " : ";");
                MessageBox.Show("Экспорт завершен.");
            }
        }

        private void ExportDelimited(string fileName, string separator)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Join(separator, _grid.Columns.Cast<DataGridViewColumn>().Select(c => c.HeaderText)));
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.IsNewRow) continue;
                sb.AppendLine(string.Join(separator, row.Cells.Cast<DataGridViewCell>().Select(c => (c.Value ?? string.Empty).ToString().Replace(separator, " "))));
            }
            File.WriteAllText(fileName, sb.ToString(), ExportEncoding);
        }

        private void ExportJson(string fileName)
        {
            var rows = _grid.Rows.Cast<DataGridViewRow>().Where(r => !r.IsNewRow).ToList();
            StringBuilder sb = new StringBuilder(); sb.AppendLine("[");
            for (int i = 0; i < rows.Count; i++)
            {
                sb.Append("  {");
                for (int j = 0; j < _grid.Columns.Count; j++)
                {
                    sb.Append("\"" + EscapeJson(_grid.Columns[j].HeaderText) + "\":\"" + EscapeJson((rows[i].Cells[j].Value ?? string.Empty).ToString()) + "\"");
                    if (j < _grid.Columns.Count - 1) sb.Append(", ");
                }
                sb.Append("}"); if (i < rows.Count - 1) sb.Append(","); sb.AppendLine();
            }
            sb.AppendLine("]"); File.WriteAllText(fileName, sb.ToString(), ExportEncoding);
        }

        private void ExportXml(string fileName)
        {
            StringBuilder sb = new StringBuilder(); sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>"); sb.AppendLine("<report>");
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.IsNewRow) continue;
                sb.AppendLine("  <row>");
                for (int i = 0; i < _grid.Columns.Count; i++)
                {
                    string name = SafeXmlName(_grid.Columns[i].HeaderText);
                    string value = SecurityElement.Escape((row.Cells[i].Value ?? string.Empty).ToString()) ?? string.Empty;
                    sb.AppendLine("    <" + name + ">" + value + "</" + name + ">");
                }
                sb.AppendLine("  </row>");
            }
            sb.AppendLine("</report>"); File.WriteAllText(fileName, sb.ToString(), ExportEncoding);
        }

        private string EscapeJson(string text) { return (text ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\""); }
        private string SafeXmlName(string text)
        {
            string cleaned = new string((text ?? string.Empty).Select(ch => char.IsLetterOrDigit(ch) ? ch : '_').ToArray());
            if (cleaned.Length == 0) return "field";
            if (char.IsDigit(cleaned[0])) cleaned = "f_" + cleaned;
            return cleaned;
        }
    }
}
