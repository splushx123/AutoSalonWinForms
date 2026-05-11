using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
    public class RoundedPanel : Panel
    {
        public int Radius { get; set; }
        public Color StrokeColor { get; set; }
        public int StrokeWidth { get; set; }

        public RoundedPanel()
        {
            Radius = 22;
            StrokeColor = Color.FromArgb(218, 225, 236);
            StrokeWidth = 1;
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = RoundRect(new Rectangle(0, 0, Width - 1, Height - 1), Radius))
            using (SolidBrush brush = new SolidBrush(BackColor))
            using (Pen pen = new Pen(StrokeColor, StrokeWidth))
            {
                e.Graphics.FillPath(brush, path);
                if (StrokeWidth > 0) e.Graphics.DrawPath(pen, path);
            }
        }

        private static GraphicsPath RoundRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    public static class Theme
    {
        public static readonly Color AppBack = Color.FromArgb(236, 240, 246);
        public static readonly Color Surface = Color.FromArgb(246, 248, 252);
        public static readonly Color Card = Color.FromArgb(255, 255, 255);
        public static readonly Color CardAlt = Color.FromArgb(242, 246, 252);
        public static readonly Color Ink = Color.FromArgb(16, 25, 43);
        public static readonly Color Muted = Color.FromArgb(99, 113, 133);
        public static readonly Color Border = Color.FromArgb(218, 225, 236);
        public static readonly Color Primary = Color.FromArgb(18, 116, 196);
        public static readonly Color PrimaryDark = Color.FromArgb(10, 75, 140);
        public static readonly Color Accent = Color.FromArgb(0, 180, 216);
        public static readonly Color Danger = Color.FromArgb(205, 67, 67);
        public static readonly Color Success = Color.FromArgb(17, 153, 104);
        public static readonly Color Warning = Color.FromArgb(232, 153, 58);
        public static readonly Color Violet = Color.FromArgb(111, 89, 220);

        public static void StyleForm(Form form)
        {
            form.BackColor = AppBack;
            form.Font = new Font("Segoe UI", 10F);
            form.ForeColor = Ink;
        }

        public static RoundedPanel CreateCard(int padding)
        {
            return new RoundedPanel
            {
                BackColor = Card,
                Padding = new Padding(padding),
                Margin = new Padding(10),
                Radius = 24,
                StrokeColor = Border,
                StrokeWidth = 1
            };
        }

        public static Label Label(string text, float size, FontStyle style, Color color)
        {
            return new Label { Text = text, Font = new Font("Segoe UI", size, style), ForeColor = color, AutoEllipsis = true };
        }

        public static TextBox CreateTextBox(int width)
        {
            return new TextBox
            {
                Width = width,
                Height = 32,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10F),
                BackColor = Color.White,
                ForeColor = Ink
            };
        }

        public static ComboBox CreateComboBox(int width)
        {
            return new ComboBox
            {
                Width = width,
                Height = 32,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F),
                BackColor = Color.White,
                ForeColor = Ink
            };
        }

        public static DateTimePicker CreateDatePicker(int width)
        {
            return new DateTimePicker
            {
                Width = width,
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 10F)
            };
        }

        public static NumericUpDown CreateNumeric(int width, decimal max, int decimals)
        {
            return new NumericUpDown
            {
                Width = width,
                Maximum = max,
                DecimalPlaces = decimals,
                ThousandsSeparator = true,
                Font = new Font("Segoe UI", 10F)
            };
        }

        public static Button CreatePrimaryButton(string text, int width, bool filled)
        {
            return BuildButton(text, width, filled ? Primary : Color.White, filled ? Color.White : Primary, filled ? Primary : Border, filled ? PrimaryDark : CardAlt);
        }

        public static Button CreatePrimaryButton(string text, int width)
        {
            return CreatePrimaryButton(text, width, true);
        }

        public static Button CreateSecondaryButton(string text, int width)
        {
            return BuildButton(text, width, Color.White, Ink, Border, CardAlt);
        }

        public static Button CreateGhostButton(string text, int width)
        {
            return BuildButton(text, width, Color.Transparent, Primary, Color.Transparent, Color.FromArgb(222, 238, 252));
        }

        private static Button BuildButton(string text, int width, Color back, Color fore, Color border, Color hover)
        {
            Button b = new Button
            {
                Text = text,
                Width = width,
                Height = 42,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                BackColor = back,
                ForeColor = fore,
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            bool hasBorder = border != Color.Transparent;
            b.FlatAppearance.BorderSize = hasBorder ? 1 : 0;
            if (hasBorder)
            {
                b.FlatAppearance.BorderColor = border;
            }
            b.MouseEnter += delegate { b.BackColor = hover; };
            b.MouseLeave += delegate { b.BackColor = back; };
            return b;
        }

        public static void StyleGrid(DataGridView grid)
        {
            grid.BackgroundColor = Card;
            grid.BorderStyle = BorderStyle.None;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.RowHeadersVisible = false;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersHeight = 44;
            grid.RowTemplate.Height = 36;
            grid.GridColor = Color.FromArgb(229, 235, 244);
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(235, 241, 249);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Ink;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            grid.DefaultCellStyle.BackColor = Color.White;
            grid.DefaultCellStyle.ForeColor = Ink;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 239, 255);
            grid.DefaultCellStyle.SelectionForeColor = Ink;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 251, 254);
        }
    }
}
