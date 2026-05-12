using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace AutoInsuranceWinForms
{
    public class ReportsForm : Form
    {
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill };
        private readonly ListBox _reports = new ListBox();
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

            RoundedPanel menu = Theme.CreateCard(18);
            menu.Dock = DockStyle.Left;
            menu.Width = 330;
            menu.Margin = new Padding(0, 0, 18, 0);
            page.Controls.Add(menu);

            Label title = new Label { Text = "Отчеты", Dock = DockStyle.Top, Height = 44, Font = new Font("Segoe UI Semibold", 22F, FontStyle.Bold), ForeColor = Theme.Ink };
            Label hint = new Label { Text = "Выберите отчет слева. Результат появится в рабочей области справа.", Dock = DockStyle.Top, Height = 58, ForeColor = Theme.Muted };
            _reports.Dock = DockStyle.Fill;
            _reports.BorderStyle = BorderStyle.None;
            _reports.Font = new Font("Segoe UI", 11F);
            _reports.ItemHeight = 32;
            _reports.BackColor = Theme.CardAlt;
            _reports.Items.AddRange(new object[] { "Автомобили по статусам", "Продажи по менеджерам", "Выручка по формам оплаты", "Тест-драйвы по результатам", "Выполненные услуги", "Доступные автомобили" });
            _reports.SelectedIndex = 0;
            _reports.SelectedIndexChanged += delegate { BuildReport(); };
            Button export = Theme.CreatePrimaryButton("Экспорт результата", 260);
            export.Dock = DockStyle.Bottom;
            export.Click += delegate { Export(); };
            menu.Controls.Add(_reports);
            menu.Controls.Add(hint);
            menu.Controls.Add(title);
            menu.Controls.Add(export);

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
            work.Controls.Add(_grid);
            work.Controls.Add(header);
            Load += delegate { Theme.ApplyCurrentTheme(this); BuildReport(); };
        }

        private void BuildReport()
        {
            try
            {
                _reportTitle.Text = _reports.SelectedItem == null ? "Отчет" : _reports.SelectedItem.ToString();
                switch (_reports.SelectedIndex)
                {
                    case 0:
                        _grid.DataSource = Db.Query(@"SELECT s.name AS [Статус], COUNT(*) AS [Количество автомобилей] FROM dbo.Car c JOIN dbo.Ref_CarStatus s ON s.status_id=c.status_id GROUP BY s.name ORDER BY [Количество автомобилей] DESC");
                        break;
                    case 1:
                        _grid.DataSource = Db.Query(@"SELECT e.last_name + N' ' + e.first_name AS [Менеджер], COUNT(*) AS [Сделок], SUM(d.final_price) AS [Сумма продаж] FROM dbo.Deal d JOIN dbo.Employee e ON e.employee_id=d.manager_id GROUP BY e.last_name, e.first_name ORDER BY [Сумма продаж] DESC");
                        break;
                    case 2:
                        _grid.DataSource = Db.Query(@"SELECT pt.name AS [Форма оплаты], COUNT(*) AS [Сделок], CAST(AVG(d.final_price) AS decimal(18,2)) AS [Средняя цена], SUM(d.final_price) AS [Выручка] FROM dbo.Deal d JOIN dbo.Ref_PaymentType pt ON pt.payment_type_id=d.payment_type_id GROUP BY pt.name ORDER BY [Выручка] DESC");
                        break;
                    case 3:
                        _grid.DataSource = Db.Query(@"SELECT ISNULL(r.name, N'Без результата') AS [Результат], COUNT(*) AS [Количество] FROM dbo.TestDrive td LEFT JOIN dbo.Ref_TestDriveResult r ON r.result_id=td.result_id GROUP BY r.name ORDER BY [Количество] DESC");
                        break;
                    case 4:
                        _grid.DataSource = Db.Query(@"SELECT sc.name AS [Услуга], COUNT(*) AS [Количество], SUM(so.cost) AS [Сумма] FROM dbo.ServiceOrder so JOIN dbo.ServiceCatalog sc ON sc.service_catalog_id=so.service_catalog_id GROUP BY sc.name ORDER BY [Сумма] DESC");
                        break;
                    default:
                        _grid.DataSource = Db.Query(@"SELECT c.vin AS [VIN], b.name AS [Марка], m.model_name AS [Модель], c.[year] AS [Год], c.exterior_color AS [Цвет], c.sale_price AS [Цена], s.name AS [Статус] FROM dbo.Car c JOIN dbo.CarModel m ON m.model_id=c.model_id JOIN dbo.Ref_Brand b ON b.brand_id=m.brand_id JOIN dbo.Ref_CarStatus s ON s.status_id=c.status_id WHERE s.name IN (N'В продаже', N'На складе') ORDER BY b.name, m.model_name");
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
