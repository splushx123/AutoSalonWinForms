using System;
using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
    public class FormBase : Form
    {
        protected FlowLayoutPanel CreateTopPanel()
        {
            return new FlowLayoutPanel { Dock = DockStyle.Top, Height = 68, Padding = new Padding(12), WrapContents = false, BackColor = Theme.Card };
        }

        protected object SelectedKey(DataGridView grid)
        {
            if (grid.CurrentRow == null || grid.CurrentRow.Cells.Count == 0) return null;
            return grid.CurrentRow.Cells[0].Value;
        }

        protected int? SelectedIntId(DataGridView grid)
        {
            var key = SelectedKey(grid);
            if (key == null || key == DBNull.Value) return null;
            return Convert.ToInt32(key);
        }
    }
}
