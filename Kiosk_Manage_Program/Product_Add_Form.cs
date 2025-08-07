using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Admin_Kiosk_Program
{
    public partial class Product_Add_Form : Form
    {
        // AdminMainForm이 이 속성을 통해 입력된 데이터를 가져갑니다.
        public Dictionary<string, object> NewProductData { get; private set; }
        private List<OptionGroup> optionGroups = new List<OptionGroup>();

        // UI 컨트롤들
        private TextBox txtProductName, txtBasePrice, txtDescription, txtProductKcal, txtProductImageUrl;
        private ComboBox cmbCategoryId;
        private TreeView optionsTreeView;

        public Product_Add_Form()
        {
            this.Text = "새 상품 추가 및 옵션 관리";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            InitializeCustomComponents();
            LoadCategories();
        }

        private void InitializeCustomComponents()
        {
            SplitContainer mainContainer = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 450, IsSplitterFixed = true, BorderStyle = BorderStyle.Fixed3D };
            FlowLayoutPanel leftPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(20), AutoScroll = true };
            txtProductName = AddLabeledControl<TextBox>(leftPanel, "product_name (상품 이름):");
            cmbCategoryId = AddLabeledControl<ComboBox>(leftPanel, "category_id (카테고리 ID):");
            txtBasePrice = AddLabeledControl<TextBox>(leftPanel, "base_price (기본 가격):");
            txtDescription = AddLabeledControl<TextBox>(leftPanel, "description (상품 설명):");
            txtProductKcal = AddLabeledControl<TextBox>(leftPanel, "product_kcal (칼로리):");
            txtProductImageUrl = AddLabeledControl<TextBox>(leftPanel, "product_image (이미지 URL):");

            Panel rightPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            optionsTreeView = new TreeView { Dock = DockStyle.Fill, Font = new Font("맑은 고딕", 10F) };
            Panel buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 40 };
            Button btnAddGroup = new Button { Text = "그룹 추가", Left = 0 };
            Button btnAddOption = new Button { Text = "옵션 추가", Left = 110 };
            Button btnDelete = new Button { Text = "선택 삭제", Left = 220 };
            buttonPanel.Controls.AddRange(new Control[] { btnAddGroup, btnAddOption, btnDelete });
            rightPanel.Controls.Add(optionsTreeView);
            rightPanel.Controls.Add(buttonPanel);

            Button btnSave = new Button { Text = "최종 저장", Width = 100 };
            Button btnCancel = new Button { Text = "취소", Width = 100 };
            FlowLayoutPanel bottomPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Height = 50, Padding = new Padding(10) };
            bottomPanel.Controls.Add(btnCancel);
            bottomPanel.Controls.Add(btnSave);

            mainContainer.Panel1.Controls.Add(leftPanel);
            mainContainer.Panel2.Controls.Add(rightPanel);
            this.Controls.Add(mainContainer);
            this.Controls.Add(bottomPanel);

            btnAddGroup.Click += BtnAddGroup_Click;
            btnAddOption.Click += BtnAddOption_Click;
            btnDelete.Click += BtnDelete_Click;
            btnSave.Click += BtnSave_Click; // SaveAllData 대신 직접 연결
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
        }

        private void LoadCategories()
        {
            DataTable categories = DatabaseManager.Instance.GetTableData("categories");
            cmbCategoryId.DataSource = categories;
            cmbCategoryId.DisplayMember = "category_name";
            cmbCategoryId.ValueMember = "category_id";
        }

        private T AddLabeledControl<T>(FlowLayoutPanel panel, string labelText) where T : Control, new()
        {
            panel.Controls.Add(new Label { Text = labelText, Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 10, 0, 0) });
            T control = new T { Width = 400 };
            panel.Controls.Add(control);
            return control;
        }

        private void RefreshTreeView()
        {
            optionsTreeView.Nodes.Clear();
            foreach (var group in optionGroups)
            {
                var groupNode = new TreeNode(group.GroupName) { Tag = group };
                optionsTreeView.Nodes.Add(groupNode);
                foreach (var option in group.Options)
                {
                    var optionNode = new TreeNode($"{option.OptionName} (+{option.AdditionalPrice:N0})") { Tag = option };
                    groupNode.Nodes.Add(optionNode);
                }
            }
            optionsTreeView.ExpandAll();
        }

        private void BtnAddGroup_Click(object sender, EventArgs e)
        {
            string groupName = ShowInputDialog("새 옵션 그룹 이름을 입력하세요:");
            if (!string.IsNullOrEmpty(groupName))
            {
                optionGroups.Add(new OptionGroup { GroupName = groupName, IsRequired = false, AllowMultiple = false });
                RefreshTreeView();
            }
        }

        private void BtnAddOption_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = optionsTreeView.SelectedNode;
            if (selectedNode == null) { MessageBox.Show("옵션을 추가할 그룹을 선택하세요."); return; }

            OptionGroup targetGroup = selectedNode.Tag as OptionGroup ?? (selectedNode.Parent?.Tag as OptionGroup);
            if (targetGroup == null) { MessageBox.Show("옵션 그룹 노드를 선택해야 합니다."); return; }

            string optionName = ShowInputDialog("새 옵션 이름을 입력하세요:");
            string priceStr = ShowInputDialog("추가 가격을 입력하세요 (숫자만):");
            if (!string.IsNullOrEmpty(optionName) && decimal.TryParse(priceStr, out decimal price))
            {
                targetGroup.Options.Add(new Option { OptionName = optionName, AdditionalPrice = price });
                RefreshTreeView();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = optionsTreeView.SelectedNode;
            if (selectedNode == null) { MessageBox.Show("삭제할 항목을 선택하세요."); return; }

            if (selectedNode.Tag is OptionGroup group) { optionGroups.Remove(group); }
            else if (selectedNode.Tag is Option option) { (selectedNode.Parent.Tag as OptionGroup)?.Options.Remove(option); }

            RefreshTreeView();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // 유효성 검사
            if (cmbCategoryId.SelectedValue == null || string.IsNullOrWhiteSpace(txtProductName.Text) || !decimal.TryParse(txtBasePrice.Text, out _) || !int.TryParse(txtProductKcal.Text, out _))
            {
                MessageBox.Show("필수 항목을 올바르게 입력해주세요.", "입력 오류");
                return;
            }

            // NewProductData 속성에 데이터 할당
            NewProductData = new Dictionary<string, object>
            {
                { "category_id", cmbCategoryId.SelectedValue },
                { "product_name", txtProductName.Text },
                { "base_price", decimal.Parse(txtBasePrice.Text) },
                { "description", txtDescription.Text },
                { "product_kcal", int.Parse(txtProductKcal.Text) },
                { "product_image", string.IsNullOrEmpty(txtProductImageUrl.Text) ? null : txtProductImageUrl.Text }
            };

            // TODO: 옵션 데이터도 함께 전달하는 로직 필요
            // (예: public List<OptionGroup> NewOptionGroups { get { return optionGroups; } } 속성 추가)

            this.DialogResult = DialogResult.OK; // 성공 시에만 OK 설정
            this.Close();
        }

        public static string ShowInputDialog(string text)
        {
            Form prompt = new Form() { Width = 500, Height = 150, FormBorderStyle = FormBorderStyle.FixedDialog, Text = text, StartPosition = FormStartPosition.CenterScreen };
            Label textLabel = new Label() { Left = 50, Top = 20, Text = text, AutoSize = true };
            TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 400 };
            Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 80, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;
            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
    }
}