using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Admin_Kiosk_Program
{
    public partial class AddForm : Form
    {
        public Dictionary<string, object> NewRowData { get; private set; }
        private List<TextBox> inputTextBoxes = new List<TextBox>();

        public AddForm(DataTable tableSchema, string primaryKeyName)
        {
            this.Text = $"'{tableSchema.TableName}' 테이블에 새 항목 추가";
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            InitializeDynamicControls(tableSchema, primaryKeyName);
        }

        private void InitializeDynamicControls(DataTable tableSchema, string primaryKeyName)
        {
            FlowLayoutPanel mainPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(10),
                AutoScroll = true
            };

            foreach (DataColumn column in tableSchema.Columns)
            {
                if (column.ColumnName == primaryKeyName && column.AutoIncrement)
                {
                    continue;
                }

                Label lbl = new Label
                {
                    Text = column.ColumnName,
                    Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold),
                    Width = 350,
                    Margin = new Padding(0, 10, 0, 0)
                };

                TextBox txt = new TextBox
                {
                    Name = "txt" + column.ColumnName,
                    Width = 350,
                    Tag = column
                };

                inputTextBoxes.Add(txt);
                mainPanel.Controls.Add(lbl);
                mainPanel.Controls.Add(txt);
            }

            Button btnSave = new Button { Text = "저장", Width = 100 };
            btnSave.Click += BtnSave_Click;

            Button btnCancel = new Button { Text = "취소", Width = 100 };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            FlowLayoutPanel buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Height = 40 };
            buttonPanel.Controls.Add(btnCancel);
            buttonPanel.Controls.Add(btnSave);

            this.Controls.Add(mainPanel);
            this.Controls.Add(buttonPanel);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            NewRowData = new Dictionary<string, object>();
            foreach (var txt in inputTextBoxes)
            {
                var column = txt.Tag as DataColumn;
                NewRowData[column.ColumnName] = string.IsNullOrEmpty(txt.Text) ? null : txt.Text;
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}