using GenshinGrinderHelper.Managers;
using NLog;

namespace GenshinGrinderHelper.Forms
{
    public partial class KeyBindingForm : Form
    {
        private DataGridView dgvHotKeys;
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        public KeyBindingForm()
        {
            logger.Info("Initalizing");
            InitializeComponent();
            InitializeGrid();
            logger.Info("Successfully initalized");
        }

        private void InitializeGrid()
        {
            dgvHotKeys = new DataGridView
            {
                Font = new Font("Microsoft YaHei UI", 9f),
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowDrop = false,
                AllowUserToResizeColumns = false,
                AllowUserToOrderColumns = false,
                MultiSelect = false,
                ShowCellToolTips = true,
                TabStop = false,
                BackgroundColor = SystemColors.Control,
                EditMode = DataGridViewEditMode.EditOnKeystroke,
                ImeMode = ImeMode.Disable,
            };

            // 动作列不可编辑
            var colAction = new DataGridViewTextBoxColumn
            {
                Name = "Action",
                HeaderText = "动作",
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
            dgvHotKeys.Columns.Add(colAction);

            var colKey = new DataGridViewTextBoxColumn
            {
                Name = "Key",
                HeaderText = "按键",
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
            dgvHotKeys.Columns.Add(colKey);

            foreach (var kvp in Config.Instance.HotKeys.KeyBindings)
            {
                dgvHotKeys.Rows.Add(kvp.Key.ToString(), kvp.Value.ToString());
            }
            dgvHotKeys.PreviewKeyDown += DgvHotKeys_PreviewKeyDown;
            dgvHotKeys.EditingControlShowing += DgvHotKeys_EditingControlShowing;
            dgvHotKeys.KeyDown += DgvHotKeys_KeyDown;
            dgvHotKeys.CellEnter += DgvHotKeys_CellEnter;
            Controls.Add(dgvHotKeys);
        }

        private void DgvHotKeys_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            e.IsInputKey = true;
        }

        private void DgvHotKeys_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (dgvHotKeys.CurrentCell.ColumnIndex == dgvHotKeys.Columns["Key"].Index)
            {
                e.Control.KeyDown -= EditingControl_KeyDown;
                e.Control.KeyDown += EditingControl_KeyDown;
            }
        }

        private void EditingControl_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;

            if (e.KeyCode == Keys.Escape)
            {
                dgvHotKeys.CancelEdit();
                return;
            }
            if (e.KeyCode == Keys.Back)
            {
                dgvHotKeys.CurrentCell.Value = "无";
            }
            else
            {
                dgvHotKeys.CurrentCell.Value = e.KeyCode.ToString();
            }
            dgvHotKeys.EndEdit();

            var actionName = dgvHotKeys.Rows[dgvHotKeys.CurrentCell.RowIndex].Cells["Action"].Value.ToString();
            if (Enum.TryParse(actionName, out HotkeyManager.HotKeyActions action))
            {
                Config.Instance.HotKeys.KeyBindings[action] = e.KeyCode == Keys.Back ? Keys.None : e.KeyCode;
            }
        }

        private void DgvHotKeys_KeyDown(object sender, KeyEventArgs e)
        {
            if (dgvHotKeys.CurrentCell != null
                && dgvHotKeys.CurrentCell.ColumnIndex == dgvHotKeys.Columns["Key"].Index)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void DgvHotKeys_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != dgvHotKeys.Columns["Key"].Index) return;
            dgvHotKeys.BeginEdit(false);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Config.Instance.SaveConfig();
            HotkeyManager.Instance.ReloadConfig();
            base.OnFormClosed(e);
        }
    }
}
