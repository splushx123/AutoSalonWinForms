using System.Data;
using System.Windows.Forms;

namespace AutoInsuranceWinForms
{
    public static class LookupService
    {
        public static void Fill(ComboBox combo, string sql, string valueMember, string displayMember)
        {
            var table = Db.Query(sql);
            combo.DataSource = table;
            combo.ValueMember = valueMember;
            combo.DisplayMember = displayMember;
            if (combo.Items.Count > 0) combo.SelectedIndex = 0;
        }
    }
}
