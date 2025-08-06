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

        // 옵션 편집용 컨트롤들을 클래스 필드로 선언
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
            this.Size = new Size(800, 950);
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

            // --- 1. 상품 기본 정보 섹션 ---
            var productGroup = CreateGroupBox("상품 기본 정보", 740, 300);
            AddDetailRow(productGroup, "product_name", "상품 이름", _product.ProductName, 30);
            AddDetailRow(productGroup, "category_id", "카테고리 ID", _product.CategoryId, 70);
            AddDetailRow(productGroup, "product_image", "상품 이미지 URL", _product.ProductImageUrl, 110);
            AddDetailRow(productGroup, "description", "상품 설명", _product.ProductDescription, 150);
            AddDetailRow(productGroup, "base_price", "기본 가격", _product.BasePrice, 190);
            AddDetailRow(productGroup, "product_kcal", "칼로리", _product.ProductKcal, 230);

            // --- 2. 옵션 그룹 정보 섹션 ---
            var optionGroupGroup = CreateGroupBox("옵션 그룹", 740, 220);
            var ogContainer = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(10) };
            optionGroupGroup.Controls.Add(ogContainer);
            PopulateOptionGroups(ogContainer);

            // --- 3. 상세 옵션 정보 섹션 ---
            var optionsGroup = CreateGroupBox("상세 옵션", 740, 300);
            optionTreeView = new TreeView { Dock = DockStyle.Fill, Font = new Font("맑은 고딕", 10F), Margin = new Padding(10, 50, 10, 10), BorderStyle = BorderStyle.None };
            optionTreeView.AfterSelect += OptionTreeView_AfterSelect;
            optionsGroup.Controls.Add(optionTreeView);
            PopulateOptionTree(optionTreeView);

            txt_OptionName = new TextBox { Location = new Point(200, 25), Width = 200, Visible = false };
            txt_OptionPrice = new TextBox { Location = new Point(430, 25), Width = 100, Visible = false };
            btn_UpdateOption = new Button { Text = "옵션 수정", Location = new Point(570, 24), Visible = false, Font = new Font(this.Font, FontStyle.Regular), Height = 30, Width = 80 };
            btn_UpdateOption.Click += Btn_UpdateOption_Click;
            optionsGroup.Controls.AddRange(new Control[] { txt_OptionName, txt_OptionPrice, btn_UpdateOption });

            txt_OptionName.BringToFront();
            txt_OptionPrice.BringToFront();
            btn_UpdateOption.BringToFront();

            var btnConfirm = new Button { Text = "확인", Size = new Size(100, 40), DialogResult = DialogResult.OK };
            mainPanel.SetFlowBreak(optionsGroup, true);

            mainPanel.Controls.Add(productGroup);
            mainPanel.Controls.Add(optionGroupGroup);
            mainPanel.Controls.Add(optionsGroup);
            mainPanel.Controls.Add(btnConfirm);
            this.Controls.Add(mainPanel);
        }

        private GroupBox CreateGroupBox(string title, int width, int height)
        {
            return new GroupBox { Text = title, Font = new Font("맑은 고딕", 11F, FontStyle.Bold), Size = new Size(width, height), Margin = new Padding(0, 0, 0, 15) };
        }

        private void AddDetailRow(GroupBox parent, string dbKey, string displayName, object value, int yPos, bool isImageRow = false)
        {
            var lbl = new Label { Text = dbKey, Location = new Point(20, yPos + 3), Font = new Font(this.Font, FontStyle.Bold), AutoSize = true };
            var txt = new TextBox { Text = value?.ToString() ?? "", Location = new Point(150, yPos), Width = 450 };
            var btn = new Button { Text = "수정", Location = new Point(610, yPos - 1), Tag = dbKey, Font = new Font(this.Font, FontStyle.Regular), Height = 30 };

            if (isImageRow)
            {
                btn.Text = "URL 수정";
            }

            btn.Click += (s, e) => {
                DatabaseManager.Instance.UpdateCellValue("products", "product_id", _productId, dbKey, txt.Text);
                MessageBox.Show($"'{displayName}' 값이 수정되었습니다.");
            };
            parent.Controls.AddRange(new Control[] { lbl, txt, btn });
        }

        private void PopulateOptionGroups(Panel container)
        {
            int yPos = 10;
            var headerPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, Size = new Size(700, 30), Location = new Point(10, yPos) };
            headerPanel.Controls.Add(new Label { Text = "group_id", Width = 80, TextAlign = ContentAlignment.MiddleCenter, Font = new Font(this.Font, FontStyle.Bold) });
            headerPanel.Controls.Add(new Label { Text = "group_name", Width = 150, TextAlign = ContentAlignment.MiddleCenter, Font = new Font(this.Font, FontStyle.Bold) });
            headerPanel.Controls.Add(new Label { Text = "is_required", Width = 150, TextAlign = ContentAlignment.MiddleCenter, Font = new Font(this.Font, FontStyle.Bold) });
            headerPanel.Controls.Add(new Label { Text = "allow_multiple", Width = 150, TextAlign = ContentAlignment.MiddleCenter, Font = new Font(this.Font, FontStyle.Bold) });
            container.Controls.Add(headerPanel);
            yPos += 35;

            foreach (var group in _product.OptionGroups)
            {
                var pnl = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, Size = new Size(700, 35), Location = new Point(10, yPos) };
                var lblId = new Label { Text = group.GroupId.ToString(), Width = 80, TextAlign = ContentAlignment.MiddleCenter };
                var txtName = new TextBox { Text = group.GroupName, Width = 150 };
                var txtRequired = new TextBox { Text = group.IsRequired ? "1" : "0", Width = 150 };
                var txtMultiple = new TextBox { Text = group.AllowMultiple ? "1" : "0", Width = 150 };
                var btnUpdate = new Button { Text = "수정", Tag = group, Font = new Font(this.Font, FontStyle.Regular), Height = 30 };

                btnUpdate.Click += (s, e) => {
                    MessageBox.Show($"Group ID {group.GroupId} 수정 로직 필요");
                };

                pnl.Controls.AddRange(new Control[] { lblId, txtName, txtRequired, txtMultiple, btnUpdate });
                container.Controls.Add(pnl);
                yPos += 40;
            }
        }

        private void PopulateOptionTree(TreeView tv)
        {
            foreach (var group in _product.OptionGroups)
            {
                var groupNode = new TreeNode(group.GroupName) { Tag = group };
                tv.Nodes.Add(groupNode);
                foreach (var option in group.Options)
                {
                    var optionNode = new TreeNode($"{option.OptionName} (+{option.AdditionalPrice:N0})") { Tag = option };
                    groupNode.Nodes.Add(optionNode);
                }
            }
            tv.ExpandAll();
        }

        private void OptionTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is Option selectedOption)
            {
                txt_OptionName.Text = selectedOption.OptionName;
                txt_OptionPrice.Text = selectedOption.AdditionalPrice.ToString();
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

                if (decimal.TryParse(txt_OptionPrice.Text, out decimal newPrice))
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