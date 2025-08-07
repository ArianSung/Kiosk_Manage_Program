// Kiosk_Manage_Program/Product_Change_Form.cs

using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Admin_Kiosk_Program
{
    public partial class Product_Change_Form : Form
    {
        private int _productId;
        private Product _product;

        private TextBox txt_OptionName;
        private TextBox txt_OptionPrice;
        private Button btn_UpdateOption;
        private TreeView optionTreeView;

        public Product_Change_Form(int productId)
        {
            _productId = productId;
            this.Load += Product_Change_Form_Load;
        }

        private async void Product_Change_Form_Load(object sender, EventArgs e)
        {
            this.Text = "상품 정보 수정";
            this.Size = new Size(800, 1020);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            _product = await Task.Run(() => DatabaseManager.Instance.GetProductDetails(_productId));
            if (_product == null)
            {
                MessageBox.Show("상품 정보를 불러오는 데 실패했습니다.");
                this.Close();
                return;
            }
            InitializeDynamicControls();
        }

        private void InitializeDynamicControls()
        {
            this.Controls.Clear();

            FlowLayoutPanel mainPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, AutoScroll = true, Padding = new Padding(20), WrapContents = false };

            var productGroup = CreateGroupBox("상품 기본 정보", 740, 300);
            AddDetailRow(productGroup, "product_name", "상품 이름", _product.ProductName, 30);
            AddDetailRow(productGroup, "category_id", "카테고리 ID", _product.CategoryId, 70);
            AddDetailRow(productGroup, "product_image", "상품 이미지 URL", _product.ProductImageUrl, 110);
            AddDetailRow(productGroup, "description", "상품 설명", _product.ProductDescription, 150);
            AddDetailRow(productGroup, "base_price", "기본 가격", _product.BasePrice, 190);
            AddDetailRow(productGroup, "product_kcal", "칼로리", _product.ProductKcal, 230);

            var optionGroupGroup = CreateGroupBox("옵션 그룹 관리", 740, 220);
            Button btn_AddGroup = new Button { Text = "새 그룹 추가", Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(optionGroupGroup.Width - 115, 20), Width = 100, Font = new Font(this.Font, FontStyle.Regular) };
            btn_AddGroup.Click += Btn_AddGroup_Click;
            optionGroupGroup.Controls.Add(btn_AddGroup);
            var ogContainer = new Panel { Location = new Point(5, 50), Size = new Size(optionGroupGroup.Width - 10, optionGroupGroup.Height - 60), AutoScroll = true, BorderStyle = BorderStyle.Fixed3D };
            optionGroupGroup.Controls.Add(ogContainer);
            PopulateOptionGroups(ogContainer);

            // ▼▼▼▼▼ 그룹박스를 분리하여 레이아웃 문제를 해결했습니다 ▼▼▼▼▼

            // 1. TreeView만 포함하는 그룹박스
            var optionsGroup = CreateGroupBox("상세 옵션 목록", 740, 200);
            optionTreeView = new TreeView { Dock = DockStyle.Fill, Font = new Font("맑은 고딕", 10F) };
            optionTreeView.AfterSelect += OptionTreeView_AfterSelect;
            optionsGroup.Controls.Add(optionTreeView);
            PopulateOptionTree(optionTreeView);

            // 2. 추가/수정/삭제 컨트롤을 담는 별도의 그룹박스
            var editOptionsGroup = CreateGroupBox("선택 옵션 편집", 740, 120);

            Label lblAddPrompt = new Label { Text = "새 옵션을 추가하거나, 목록에서 삭제합니다.", Location = new Point(10, 30), AutoSize = true, Font = new Font(this.Font, FontStyle.Regular) };
            Button btn_AddOption = new Button { Text = "새 옵션 추가", Location = new Point(10, 60), Font = new Font(this.Font, FontStyle.Regular), Height = 30, Width = 90};
            Button btn_DeleteSelected = new Button { Text = "선택 항목 삭제", Location = new Point(110, 60), Font = new Font(this.Font, FontStyle.Regular), Height = 30, Width = 105 };

            Label lblEditPrompt = new Label { Text = "수정할 옵션:", Location = new Point(300, 30), AutoSize = true, Font = new Font(this.Font, FontStyle.Regular) };
            txt_OptionName = new TextBox { Location = new Point(300, 60), Width = 180, Visible = false };
            txt_OptionPrice = new TextBox { Location = new Point(490, 60), Width = 100, Visible = false };
            btn_UpdateOption = new Button { Text = "수정", Location = new Point(600, 59), Visible = false, Font = new Font(this.Font, FontStyle.Regular), Height = 30, Width = 80 };

            editOptionsGroup.Controls.AddRange(new Control[] { lblAddPrompt, btn_AddOption, btn_DeleteSelected, lblEditPrompt, txt_OptionName, txt_OptionPrice, btn_UpdateOption });

            btn_UpdateOption.Click += Btn_UpdateOption_Click;
            btn_AddOption.Click += Btn_AddOption_Click;
            btn_DeleteSelected.Click += Btn_DeleteSelected_Click;

            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

            var btnConfirm = new Button { Text = "닫기", Size = new Size(100, 40), DialogResult = DialogResult.OK };

            mainPanel.Controls.Add(productGroup);
            mainPanel.Controls.Add(optionGroupGroup);
            mainPanel.Controls.Add(optionsGroup);
            mainPanel.Controls.Add(editOptionsGroup);
            mainPanel.Controls.Add(btnConfirm);

            mainPanel.SetFlowBreak(editOptionsGroup, true);
            this.Controls.Add(mainPanel);
        }

        private GroupBox CreateGroupBox(string title, int width, int height)
        {
            return new GroupBox { Text = title, Font = new Font("맑은 고딕", 11F, FontStyle.Bold), Size = new Size(width, height), Margin = new Padding(0, 0, 0, 15) };
        }

        private void AddDetailRow(GroupBox parent, string dbKey, string displayName, object value, int yPos)
        {
            var lbl = new Label { Text = displayName, Location = new Point(20, yPos + 3), Font = new Font("맑은 고딕", 9F, FontStyle.Bold), AutoSize = true };
            var txt = new TextBox { Text = value?.ToString() ?? "", Location = new Point(150, yPos), Width = 450, Font = new Font("맑은 고딕", 9F) };
            var btn = new Button { Text = "수정", Location = new Point(610, yPos - 1), Tag = dbKey, Font = new Font(this.Font, FontStyle.Regular), Height = 30 };

            btn.Click += (s, e) => {
                DatabaseManager.Instance.UpdateCellValue("products", "product_id", _productId, dbKey, txt.Text);
                MessageBox.Show($"'{displayName}' 값이 수정되었습니다.");
            };
            parent.Controls.AddRange(new Control[] { lbl, txt, btn });
        }

        private void PopulateOptionGroups(Panel container)
        {
            container.Controls.Clear();
            int yPos = 10;

            var headerPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, Size = new Size(700, 30), Location = new Point(10, yPos) };
            headerPanel.Controls.Add(new Label { Text = "ID", Width = 40, TextAlign = ContentAlignment.MiddleCenter, Font = new Font(this.Font, FontStyle.Bold) });
            headerPanel.Controls.Add(new Label { Text = "그룹 이름", Width = 150, TextAlign = ContentAlignment.MiddleCenter, Font = new Font(this.Font, FontStyle.Bold) });
            headerPanel.Controls.Add(new Label { Text = "필수(1/0)", Width = 80, TextAlign = ContentAlignment.MiddleCenter, Font = new Font(this.Font, FontStyle.Bold) });
            headerPanel.Controls.Add(new Label { Text = "다중선택(1/0)", Width = 100, TextAlign = ContentAlignment.MiddleCenter, Font = new Font(this.Font, FontStyle.Bold) });
            container.Controls.Add(headerPanel);
            yPos += 35;

            foreach (var group in _product.OptionGroups)
            {
                var pnl = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, Size = new Size(720, 35), Location = new Point(10, yPos) };
                var lblId = new Label { Text = group.GroupId.ToString(), Width = 40, TextAlign = ContentAlignment.MiddleCenter };
                var txtName = new TextBox { Text = group.GroupName, Width = 150 };
                var txtRequired = new TextBox { Text = group.IsRequired ? "1" : "0", Width = 80 };
                var txtMultiple = new TextBox { Text = group.AllowMultiple ? "1" : "0", Width = 100 };
                var btnUpdate = new Button { Text = "수정", Tag = new Tuple<OptionGroup, TextBox, TextBox, TextBox>(group, txtName, txtRequired, txtMultiple), Font = new Font(this.Font, FontStyle.Regular), Height = 30 };
                var btnDelete = new Button { Text = "삭제", Tag = group, Font = new Font(this.Font, FontStyle.Regular), Height = 30, BackColor = Color.LightCoral };

                btnUpdate.Click += Btn_UpdateGroup_Click;
                btnDelete.Click += Btn_DeleteGroup_Click;

                pnl.Controls.AddRange(new Control[] { lblId, txtName, txtRequired, txtMultiple, btnUpdate, btnDelete });
                container.Controls.Add(pnl);
                yPos += 40;
            }
        }

        private void PopulateOptionTree(TreeView tv)
        {
            tv.Nodes.Clear();
            foreach (var group in _product.OptionGroups)
            {
                var groupNode = new TreeNode(group.GroupName) { Tag = group };
                tv.Nodes.Add(groupNode);
                foreach (var option in group.Options)
                {
                    var optionNode = new TreeNode($"{option.OptionName} (+{option.AdditionalPrice:N0}원)") { Tag = option };
                    groupNode.Nodes.Add(optionNode);
                }
            }
            tv.ExpandAll();
        }

        private void Btn_AddGroup_Click(object sender, EventArgs e)
        {
            string groupName = Product_Add_Form.ShowInputDialog("추가할 옵션 그룹의 이름을 입력하세요:");
            if (!string.IsNullOrWhiteSpace(groupName))
            {
                DatabaseManager.Instance.AddOptionGroup(_productId, groupName, false, false);
                MessageBox.Show("새 옵션 그룹이 추가되었습니다.");
                Product_Change_Form_Load(this, EventArgs.Empty);
            }
        }

        private void Btn_UpdateGroup_Click(object sender, EventArgs e)
        {
            var tag = (sender as Button)?.Tag as Tuple<OptionGroup, TextBox, TextBox, TextBox>;
            if (tag != null)
            {
                var group = tag.Item1;
                var txtName = tag.Item2;
                var txtRequired = tag.Item3;
                var txtMultiple = tag.Item4;

                DatabaseManager.Instance.UpdateCellValue("option_groups", "group_id", group.GroupId, "group_name", txtName.Text);
                DatabaseManager.Instance.UpdateCellValue("option_groups", "group_id", group.GroupId, "is_required", txtRequired.Text == "1");
                DatabaseManager.Instance.UpdateCellValue("option_groups", "group_id", group.GroupId, "allow_multiple", txtMultiple.Text == "1");

                MessageBox.Show($"그룹 '{txtName.Text}' 정보가 수정되었습니다.");
                Product_Change_Form_Load(this, EventArgs.Empty);
            }
        }

        private void Btn_DeleteGroup_Click(object sender, EventArgs e)
        {
            var group = (sender as Button)?.Tag as OptionGroup;
            if (group != null)
            {
                if (MessageBox.Show($"'{group.GroupName}' 그룹을 삭제하시겠습니까?\n그룹에 속한 모든 하위 옵션도 함께 삭제됩니다.", "삭제 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    DatabaseManager.Instance.DeleteOptionGroup(group.GroupId);
                    MessageBox.Show("옵션 그룹이 삭제되었습니다.");
                    Product_Change_Form_Load(this, EventArgs.Empty);
                }
            }
        }

        private void Btn_AddOption_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = optionTreeView.SelectedNode;
            if (selectedNode == null)
            {
                MessageBox.Show("옵션을 추가할 그룹을 TreeView에서 먼저 선택하세요.", "알림");
                return;
            }

            OptionGroup targetGroup = selectedNode.Tag as OptionGroup ?? (selectedNode.Parent?.Tag as OptionGroup);

            if (targetGroup != null)
            {
                string optionName = Product_Add_Form.ShowInputDialog("추가할 옵션의 이름을 입력하세요:");
                if (string.IsNullOrWhiteSpace(optionName)) return;

                string priceStr = Product_Add_Form.ShowInputDialog("추가 가격을 입력하세요 (숫자만):");
                if (decimal.TryParse(priceStr, out decimal price))
                {
                    DatabaseManager.Instance.AddOption(targetGroup.GroupId, optionName, price);
                    MessageBox.Show("새 옵션이 추가되었습니다.");
                    Product_Change_Form_Load(this, EventArgs.Empty);
                }
                else
                {
                    MessageBox.Show("유효한 가격을 입력해주세요.");
                }
            }
            else
            {
                MessageBox.Show("옵션을 추가하려면 TreeView에서 그룹을 선택해야 합니다.", "알림");
            }
        }

        private void Btn_DeleteSelected_Click(object sender, EventArgs e)
        {
            TreeNode selectedNode = optionTreeView.SelectedNode;
            if (selectedNode == null)
            {
                MessageBox.Show("삭제할 항목을 TreeView에서 먼저 선택하세요.", "알림");
                return;
            }

            if (selectedNode.Tag is Option selectedOption)
            {
                if (MessageBox.Show($"'{selectedOption.OptionName}' 옵션을 삭제하시겠습니까?", "삭제 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    DatabaseManager.Instance.DeleteOption(selectedOption.OptionId);
                    Product_Change_Form_Load(this, EventArgs.Empty);
                }
            }
            else if (selectedNode.Tag is OptionGroup selectedGroup)
            {
                if (MessageBox.Show($"'{selectedGroup.GroupName}' 그룹 전체를 삭제하시겠습니까?\n모든 하위 옵션이 영구적으로 삭제됩니다.", "그룹 삭제 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    DatabaseManager.Instance.DeleteOptionGroup(selectedGroup.GroupId);
                    Product_Change_Form_Load(this, EventArgs.Empty);
                }
            }
        }

        private void OptionTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is Option selectedOption)
            {
                txt_OptionName.Text = selectedOption.OptionName;
                txt_OptionPrice.Text = selectedOption.AdditionalPrice.ToString("N0");
                txt_OptionName.Tag = selectedOption.OptionId;

                txt_OptionName.Visible = true;
                txt_OptionPrice.Visible = true;
                btn_UpdateOption.Visible = true;
            }
            else
            {
                txt_OptionName.Visible = false;
                txt_OptionPrice.Visible = false;
                btn_UpdateOption.Visible = false;
            }
        }

        private void Btn_UpdateOption_Click(object sender, EventArgs e)
        {
            if (txt_OptionName.Tag is int optionId)
            {
                DatabaseManager.Instance.UpdateOptionValue(optionId, "option_name", txt_OptionName.Text);

                if (decimal.TryParse(txt_OptionPrice.Text.Replace(",", ""), out decimal newPrice))
                {
                    DatabaseManager.Instance.UpdateOptionValue(optionId, "additional_price", newPrice);
                }
                else
                {
                    MessageBox.Show("가격은 숫자만 입력 가능합니다.");
                    return;
                }

                MessageBox.Show("옵션이 성공적으로 수정되었습니다.");
                Product_Change_Form_Load(this, EventArgs.Empty);
            }
        }
    }
}