using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Admin_Kiosk_Program
{
    public partial class AdminMainForm : Form
    {
        private SplitContainer mainContainer;
        private FlowLayoutPanel buttonPanel;
        private Button btn_Advertisements;
        private Button btn_Payments;
        private Button btn_SystemColors;
        private Button btn_SystemImages;
        private Button btn_WebBannerImage;
        private Button btn_Exit;
        private Button btn_Add;

        private Panel editPanel;
        private TreeView dataTreeView;
        private TextBox txt_EditValue;
        private Button btn_Update;
        private Button btn_Delete;
        private Button btn_ChangeImage;
        private Label lbl_SelectedNodeInfo;

        private DataTable currentDataTable;
        private string currentTableName;
        private string currentPrimaryKeyName;



        public AdminMainForm()
        {
            this.Text = "Ű����ũ ���� ���α׷�";
            this.Size = new Size(1300, 700);
            this.Load += AdminMainForm_Load;
            InitializeCustomComponents();
        }

        private void AdminMainForm_Load(object sender, EventArgs e)
        {
            btn_Advertisements.PerformClick();
        }

        private void InitializeCustomComponents()
        {
            mainContainer = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 220, FixedPanel = FixedPanel.Panel1, BorderStyle = BorderStyle.Fixed3D };

            // ���� �г� (Panel1)
            var leftPanel = mainContainer.Panel1;
            btn_Exit = new Button { Text = "������", Font = new Font("���� ���", 10F, FontStyle.Bold), Dock = DockStyle.Bottom, Height = 50 };
            btn_Exit.Click += (s, e) => { if (MessageBox.Show("���α׷��� �����Ͻðڽ��ϱ�?", "���� Ȯ��", MessageBoxButtons.YesNo) == DialogResult.Yes) Application.Exit(); };

            buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(10) };

            leftPanel.Controls.Add(buttonPanel);
            leftPanel.Controls.Add(btn_Exit);

            // ������ �г� (Panel2)
            editPanel = new Panel { Dock = DockStyle.Fill };

            lbl_SelectedNodeInfo = new Label { Location = new Point(10, 15), AutoSize = true, Font = new Font("���� ���", 10F) };
            txt_EditValue = new TextBox { Location = new Point(10, 40), Size = new Size(400, 23) };
            btn_Update = new Button { Location = new Point(420, 38), Size = new Size(80, 25), Text = "����" };
            btn_Delete = new Button { Location = new Point(510, 38), Size = new Size(80, 25), Text = "����" };
            btn_ChangeImage = new Button { Location = new Point(10, 38), Size = new Size(120, 25), Text = "�̹��� ����...", Visible = false };
            btn_Add = new Button();
            btn_Add.Location = new Point(600, 38);
            btn_Add.Size = new Size(80, 25);
            btn_Add.Text = "�߰�";
            btn_Add.Click += Btn_Add_Click;

            dataTreeView = new TreeView { Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, Location = new Point(10, 80), Font = new Font("���� ���", 11F) };
            dataTreeView.Size = new Size(editPanel.ClientSize.Width - 20, editPanel.ClientSize.Height - 100);

            editPanel.Controls.AddRange(new Control[] { dataTreeView, lbl_SelectedNodeInfo, txt_EditValue, btn_Update, btn_Delete, btn_ChangeImage, btn_Add });
            mainContainer.Panel2.Controls.Add(editPanel);

            // ��ư ���� �� �̺�Ʈ ����
            btn_Advertisements = CreateMenuButton("���� ���� (advertisements)", "advertisements", "id");
            btn_Payments = CreateMenuButton("�������� ���� (payments)", "payments", "payment_id");
            btn_SystemColors = CreateMenuButton("�׸� ���� ���� (system_colors)", "system_colors", "color_key");
            btn_SystemImages = CreateMenuButton("�ý��� �̹��� ���� (system_images)", "system_images", "image_key");
            btn_WebBannerImage = CreateMenuButton("�� ��� ���� (webbanner_image)", "webbanner_image", "id");
            buttonPanel.Controls.AddRange(new Control[] { btn_Advertisements, btn_Payments, btn_SystemColors, btn_SystemImages, btn_WebBannerImage });

            dataTreeView.AfterSelect += DataTreeView_AfterSelect;
            btn_Update.Click += Btn_Update_Click;
            btn_Delete.Click += Btn_Delete_Click;
            btn_ChangeImage.Click += Btn_ChangeImage_Click;
            

            this.Controls.Add(mainContainer);
        }

        private void Btn_Add_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentTableName))
            {
                MessageBox.Show("���̺��� ���� �������ּ���.");
                return;
            }

            using (var addForm = new AddForm(currentDataTable, currentPrimaryKeyName))
            {
                if (addForm.ShowDialog() == DialogResult.OK)
                {
                    DatabaseManager.Instance.AddNewRow(currentTableName, addForm.NewRowData);
                    MessageBox.Show("�� �׸��� �߰��Ǿ����ϴ�.");
                    LoadDataToTreeView(); // TreeView ���ΰ�ħ
                }
            }
        }

        private Button CreateMenuButton(string text, string tableName, string pkName)
        {
            var btn = new Button
            {
                Text = text,
                Tag = new Tuple<string, string>(tableName, pkName),
                Size = new Size(200, 50),
                Font = new Font("���� ���", 10F, FontStyle.Bold)
            };
            btn.Click += TableButton_Click;
            return btn;
        }

        private void TableButton_Click(object sender, EventArgs e)
        {
            var tag = (sender as Button).Tag as Tuple<string, string>;
            currentTableName = tag.Item1;
            currentPrimaryKeyName = tag.Item2;
            LoadDataToTreeView();
        }

        private void LoadDataToTreeView()
        {
            dataTreeView.Nodes.Clear();
            currentDataTable = DatabaseManager.Instance.GetTableData(currentTableName);
            var rootNode = new TreeNode(currentTableName);
            dataTreeView.Nodes.Add(rootNode);

            foreach (DataRow row in currentDataTable.Rows)
            {
                var rowNode = new TreeNode($"�׸� (PK: {row[currentPrimaryKeyName]})") { Tag = row };
                rootNode.Nodes.Add(rowNode);
                foreach (DataColumn col in currentDataTable.Columns)
                {
                    var cellNode = new TreeNode($"{col.ColumnName}: {(row[col] is byte[]? "[�̹��� ������]" : row[col])}") { Tag = col.ColumnName };
                    rowNode.Nodes.Add(cellNode);
                }
            }
            rootNode.Expand();
            lbl_SelectedNodeInfo.Text = $"'{currentTableName}' ���̺��� �ε�Ǿ����ϴ�.";
            txt_EditValue.Text = "";
        }

        private void DataTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            txt_EditValue.Visible = true;
            btn_Update.Visible = true;
            btn_ChangeImage.Visible = false;
            txt_EditValue.Enabled = false;
            btn_Update.Enabled = false;

            if (e.Node?.Tag is DataRow)
            {
                lbl_SelectedNodeInfo.Text = $"���õ� �׸�: {e.Node.Text}";
            }
            else if (e.Node?.Parent?.Tag is DataRow)
            {
                var row = e.Node.Parent.Tag as DataRow;
                var colName = e.Node.Tag as string;

                if (currentDataTable.Columns[colName].DataType == typeof(byte[]))
                {
                    txt_EditValue.Visible = false;
                    btn_Update.Visible = false;
                    btn_ChangeImage.Visible = true;
                    lbl_SelectedNodeInfo.Text = "���õ� �׸�: �̹��� ������";
                }
                else if (colName == currentPrimaryKeyName)
                {
                    lbl_SelectedNodeInfo.Text = $"���õ� �׸�: {colName} (�⺻ Ű�� ���� �Ұ�)";
                    txt_EditValue.Text = row[colName].ToString();
                }
                else
                {
                    txt_EditValue.Enabled = true;
                    btn_Update.Enabled = true;
                    lbl_SelectedNodeInfo.Text = $"���õ� �׸�: {colName}";
                    txt_EditValue.Text = row[colName].ToString();
                }
            }
        }

        private void Btn_Update_Click(object sender, EventArgs e)
        {
            var selectedNode = dataTreeView.SelectedNode;
            if (selectedNode?.Parent?.Tag is DataRow row)
            {
                var pkValue = row[currentPrimaryKeyName];
                var colName = selectedNode.Tag as string;
                DatabaseManager.Instance.UpdateCellValue(currentTableName, currentPrimaryKeyName, pkValue, colName, txt_EditValue.Text);
                MessageBox.Show("�����Ǿ����ϴ�.");
                LoadDataToTreeView();
            }
        }

        private void Btn_Delete_Click(object sender, EventArgs e)
        {
            var selectedNode = dataTreeView.SelectedNode;
            if (selectedNode?.Tag is DataRow row)
            {
                if (MessageBox.Show("������ �� �׸��� �����Ͻðڽ��ϱ�?", "���� Ȯ��", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    var pkValue = row[currentPrimaryKeyName];
                    DatabaseManager.Instance.DeleteRow(currentTableName, currentPrimaryKeyName, pkValue);
                    MessageBox.Show("�����Ǿ����ϴ�.");
                    LoadDataToTreeView();
                }
            }
            else { MessageBox.Show("������ �׸�(Row)�� �������ּ���."); }
        }

        private void Btn_ChangeImage_Click(object sender, EventArgs e)
        {
            var selectedNode = dataTreeView.SelectedNode;
            DataRow row = selectedNode?.Tag as DataRow ?? selectedNode?.Parent?.Tag as DataRow;
            if (row == null) { MessageBox.Show("�̹����� ������ �׸��� �������ּ���."); return; }

            string imageColumnName = "image_data"; // �⺻��
            if (selectedNode.Tag is string colName && colName.Contains("image"))
            {
                imageColumnName = colName;
            }

            using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        byte[] newImageData = File.ReadAllBytes(ofd.FileName);
                        var pkValue = row[currentPrimaryKeyName];
                        DatabaseManager.Instance.UpdateCellValue(currentTableName, currentPrimaryKeyName, pkValue, imageColumnName, newImageData);
                        MessageBox.Show("�̹����� ���������� ����Ǿ����ϴ�.");
                        LoadDataToTreeView();
                    }
                    catch (Exception ex) { MessageBox.Show($"�̹��� ���� ����: {ex.Message}"); }
                }
            }
        }
    }
}