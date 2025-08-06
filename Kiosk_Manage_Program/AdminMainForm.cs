using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic; // Dictionary ����� ���� �߰�
using System.Linq; // LINQ ����� ���� �߰�

namespace Admin_Kiosk_Program
{
    public partial class AdminMainForm : Form
    {
        private SplitContainer mainContainer;
        private FlowLayoutPanel buttonPanel;
        private Button btn_Categories; // ī�װ� ��ư
        private Button btn_Products;
        private Button btn_Advertisements;
        private Button btn_Payments;
        private Button btn_SystemColors;
        private Button btn_SystemImages;
        private Button btn_WebBannerImage;
        private Button btn_Exit;
        private Button btn_Add;
        private Button btn_Change;
        private Button btn_Update;
        private Button btn_Delete;
        private Button btn_ChangeImage;
        private Button btn_Add_Product;
        private Button btn_Delete_Product;

        private Panel editPanel;
        private TreeView dataTreeView;
        private TextBox txt_EditValue;
        private Label lbl_SelectedNodeInfo;

        private DataTable currentDataTable;
        private string currentTableName;
        private string currentPrimaryKeyName;

        public AdminMainForm()
        {
            this.Text = "Ű����ũ ���� ���� ���α׷�";
            this.Size = new Size(1300, 700);
            this.Load += AdminMainForm_Load;
            InitializeCustomComponents();
        }

        private void AdminMainForm_Load(object sender, EventArgs e)
        {
            btn_Categories.PerformClick();
        }

        private void InitializeCustomComponents()
        {
            mainContainer = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 220, FixedPanel = FixedPanel.Panel1, BorderStyle = BorderStyle.Fixed3D };
            var leftPanel = mainContainer.Panel1;
            btn_Exit = new Button { Text = "������", Font = new Font("���� ���", 10F, FontStyle.Bold), Dock = DockStyle.Bottom, Height = 50 };
            buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(10) };
            leftPanel.Controls.Add(buttonPanel);
            leftPanel.Controls.Add(btn_Exit);

            editPanel = new Panel { Dock = DockStyle.Fill };
            lbl_SelectedNodeInfo = new Label { Location = new Point(10, 15), AutoSize = true, Font = new Font("���� ���", 10F) };
            txt_EditValue = new TextBox { Location = new Point(10, 40), Size = new Size(400, 23) };
            btn_Update = new Button { Location = new Point(420, 38), Size = new Size(80, 25), Text = "����" };
            btn_Delete = new Button { Location = new Point(510, 38), Size = new Size(80, 25), Text = "����" };
            btn_Add = new Button { Location = new Point(600, 38), Size = new Size(80, 25), Text = "�߰�" };
            btn_ChangeImage = new Button { Location = new Point(10, 38), Size = new Size(120, 25), Text = "�̹��� ����...", Visible = false };
            btn_Change = new Button { Location = new Point(10, 38), Size = new Size(120, 25), Text = "���� ��ǰ ����", Visible = false };
            btn_Add_Product = new Button { Location = new Point(140, 38), Size = new Size(100, 25), Text = "�� ��ǰ �߰�", Visible = false };
            btn_Delete_Product = new Button { Location = new Point(250, 38), Size = new Size(120, 25), Text = "���� ��ǰ ����", Visible = false };

            dataTreeView = new TreeView { Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, Location = new Point(10, 80), Font = new Font("���� ���", 11F) };
            dataTreeView.Size = new Size(editPanel.ClientSize.Width - 20, editPanel.ClientSize.Height - 100);

            editPanel.Controls.AddRange(new Control[] { dataTreeView, lbl_SelectedNodeInfo, txt_EditValue, btn_Update, btn_Delete, btn_Add, btn_ChangeImage, btn_Change, btn_Add_Product, btn_Delete_Product });
            mainContainer.Panel2.Controls.Add(editPanel);

            // ������ ī�װ� ���� ��ư�� ���� �߰��մϴ� ������
            btn_Categories = CreateSimpleMenuButton("ī�װ� ����", "categories", "category_id");
            btn_Products = new Button { Text = "��ǰ ����", Size = new Size(200, 50), Font = new Font("���� ���", 10F, FontStyle.Bold) };
            btn_Advertisements = CreateSimpleMenuButton("���� ����", "advertisements", "id");
            btn_Payments = CreateSimpleMenuButton("�������� ����", "payments", "payment_id");
            btn_SystemColors = CreateSimpleMenuButton("�׸� ���� ����", "system_colors", "color_key");
            btn_SystemImages = CreateSimpleMenuButton("�ý��� �̹��� ����", "system_images", "image_key");
            btn_WebBannerImage = CreateSimpleMenuButton("�� ��� ����", "webbanner_image", "id");

            // ��ư �гο� ��Ʈ�ѵ��� �߰��մϴ�.
            buttonPanel.Controls.AddRange(new Control[] { btn_Categories, btn_Products, btn_Advertisements, btn_Payments, btn_SystemColors, btn_SystemImages, btn_WebBannerImage });
            // �������������������������������������������

            // �̺�Ʈ �ڵ鷯 ����
            dataTreeView.AfterSelect += DataTreeView_AfterSelect;
            btn_Update.Click += Btn_Update_Click;
            btn_Delete.Click += Btn_Delete_Click;
            btn_Add.Click += Btn_Add_Click;
            btn_ChangeImage.Click += Btn_ChangeImage_Click;
            btn_Products.Click += Btn_Products_Click;
            btn_Change.Click += Btn_Change_Click;
            btn_Add_Product.Click += Btn_Add_Product_Click;
            btn_Delete_Product.Click += Btn_Delete_Product_Click;
            btn_Exit.Click += (s, e) => { if (MessageBox.Show("���α׷��� �����Ͻðڽ��ϱ�?", "���� Ȯ��", MessageBoxButtons.YesNo) == DialogResult.Yes) Application.Exit(); };

            this.Controls.Add(mainContainer);
        }

        private Button CreateSimpleMenuButton(string text, string tableName, string pkName)
        {
            var btn = new Button { Text = text, Tag = new Tuple<string, string>(tableName, pkName), Size = new Size(200, 40), Font = new Font("���� ���", 9F, FontStyle.Bold) };
            btn.Click += TableButton_Click;
            return btn;
        }

        private void TableButton_Click(object sender, EventArgs e)
        {
            var tag = (sender as Button).Tag as Tuple<string, string>;
            currentTableName = tag.Item1;
            currentPrimaryKeyName = tag.Item2;
            ShowSimpleEditor();
            LoadDataToTreeViewSimple();
        }

        private void Btn_Products_Click(object sender, EventArgs e)
        {
            ShowProductEditor();
            LoadProductsToTreeView();
        }

        private void ShowSimpleEditor()
        {
            txt_EditValue.Visible = true;
            btn_Update.Visible = true;
            btn_Delete.Visible = true;
            btn_Add.Visible = true;
            btn_Change.Visible = false;
            btn_Add_Product.Visible = false;
            btn_Delete_Product.Visible = false;
        }

        private void ShowProductEditor()
        {
            txt_EditValue.Visible = false;
            btn_Update.Visible = false;
            btn_Delete.Visible = false;
            btn_Add.Visible = false;
            btn_Change.Visible = true;
            btn_Add_Product.Visible = true;
            btn_Delete_Product.Visible = true;
            btn_Change.Enabled = false;
            btn_Delete_Product.Enabled = false;
        }

        private void LoadDataToTreeViewSimple()
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

        private void LoadProductsToTreeView()
        {
            dataTreeView.Nodes.Clear();
            List<Category> categoriesWithProducts = DatabaseManager.Instance.GetCategoriesWithProducts();
            var rootNode = new TreeNode("ī�װ�/��ǰ ���");
            dataTreeView.Nodes.Add(rootNode);

            foreach (var category in categoriesWithProducts)
            {
                var categoryNode = new TreeNode(category.CategoryName) { Tag = category };
                rootNode.Nodes.Add(categoryNode);
                foreach (var product in category.Products)
                {
                    var productNode = new TreeNode(product.ProductName) { Tag = product };
                    categoryNode.Nodes.Add(productNode);
                }
            }
            rootNode.ExpandAll();
            lbl_SelectedNodeInfo.Text = "��ǰ�� �����ϸ� '����' �Ǵ� '����' ��ư�� Ȱ��ȭ�˴ϴ�.";
        }

        private void DataTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (btn_Change.Visible) // ��ǰ ���� ���
            {
                bool isProductSelected = e.Node?.Tag is Product;
                btn_Change.Enabled = isProductSelected;
                btn_Delete_Product.Enabled = isProductSelected;
                lbl_SelectedNodeInfo.Text = isProductSelected ? $"���õ� ��ǰ: {e.Node.Text}" : "��ǰ�� �������ּ���.";
            }
            else // �ܼ� ���̺� ���� ���
            {
                txt_EditValue.Visible = true;
                btn_Update.Visible = true;
                btn_ChangeImage.Visible = false;
                txt_EditValue.Enabled = false;
                btn_Update.Enabled = false;

                if (e.Node?.Parent?.Tag is DataRow row)
                {
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
                        lbl_SelectedNodeInfo.Text = $"���õ� �׸�: {colName} (PK�� ���� �Ұ�)";
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
                LoadDataToTreeViewSimple();
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
                    LoadDataToTreeViewSimple();
                }
            }
            else { MessageBox.Show("������ �׸�(Row)�� �������ּ���."); }
        }

        private void Btn_Add_Click(object sender, EventArgs e)
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
                    LoadDataToTreeViewSimple();
                }
            }
        }

        private void Btn_ChangeImage_Click(object sender, EventArgs e)
        {
            DataRow row = dataTreeView.SelectedNode?.Tag as DataRow ?? dataTreeView.SelectedNode?.Parent?.Tag as DataRow;
            if (row == null) { MessageBox.Show("�̹����� ������ �׸��� �������ּ���."); return; }

            string imageColumnName = "image_data";
            if (dataTreeView.SelectedNode.Tag is string colName && colName.Contains("image"))
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
                        LoadDataToTreeViewSimple();
                    }
                    catch (Exception ex) { MessageBox.Show($"�̹��� ���� ����: {ex.Message}"); }
                }
            }
        }

        private void Btn_Change_Click(object sender, EventArgs e)
        {
            if (dataTreeView.SelectedNode?.Tag is Product selectedProduct)
            {
                using (var productForm = new Product_Change_Form(selectedProduct.ProductId))
                {
                    productForm.ShowDialog();
                    LoadProductsToTreeView();
                }
            }
        }

        private void Btn_Add_Product_Click(object sender, EventArgs e)
        {
            using (var addForm = new Product_Add_Form())
            {
                if (addForm.ShowDialog() == DialogResult.OK)
                {
                    // 1. Product_Add_Form���� null�� �ƴ� NewProductData�� �����ɴϴ�.
                    Dictionary<string, object> productData = addForm.NewProductData;

                    // 2. DatabaseManager�� ���� ��ǰ�� ���� �߰��ϰ� ���� ������ ID�� �޽��ϴ�.
                    int newProductId = DatabaseManager.Instance.AddProductAndGetId(productData);

                    if (newProductId != -1)
                    {
                        // TODO: ������ ������ �ɼ� �׷�� �� �ɼǵ��� DB�� �߰��ϴ� ����
                        // List<OptionGroup> newOptionGroups = addForm.NewOptionGroups;
                        // foreach(var group in newOptionGroups) { ... }

                        MessageBox.Show("�� ��ǰ�� �߰��Ǿ����ϴ�.");
                        LoadProductsToTreeView(); // TreeView ���ΰ�ħ
                    }
                    else
                    {
                        MessageBox.Show("��ǰ �߰��� �����߽��ϴ�.");
                    }
                }
            }
        }

        private void Btn_Delete_Product_Click(object sender, EventArgs e)
        {
            if (dataTreeView.SelectedNode?.Tag is Product selectedProduct)
            {
                if (MessageBox.Show($"'{selectedProduct.ProductName}' ��ǰ�� ������ �����Ͻðڽ��ϱ�?\n�� ��ǰ�� ���õ� ��� �ɼ� ������ �Բ� �����˴ϴ�.", "���� Ȯ��", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    DatabaseManager.Instance.DeleteProduct(selectedProduct.ProductId);
                    MessageBox.Show("��ǰ�� �����Ǿ����ϴ�.");
                    LoadProductsToTreeView();
                }
            }
        }
    }
}