using System;
using System.Drawing;
using System.Windows.Forms;

namespace Admin_Kiosk_Program
{
    public partial class Product_Change_Form : Form
    {
        private int _productId;
        private Product _product;

        // UI 컨트롤들을 담을 Panel
        private FlowLayoutPanel mainPanel;

        public Product_Change_Form(int productId)
        {
            _productId = productId;

            this.Text = "상품 정보 수정";
            this.Size = new Size(800, 1020);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Load += Product_Change_Form_Load;
        }

        private async void Product_Change_Form_Load(object sender, EventArgs e)
        {
            // DB에서 모든 데이터를 비동기적으로 로드
            _product = await Task.Run(() => DatabaseManager.Instance.GetProductDetails(_productId));
            if (_product == null)
            {
                MessageBox.Show("상품 정보를 불러오는데 실패했습니다.");
                this.Close();
                return;
            }
            // 로드된 데이터로 UI를 생성
            InitializeDynamicControls();
        }

        private void InitializeDynamicControls()
        {
            this.Controls.Clear();

            mainPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, AutoScroll = true, Padding = new Padding(10) };

            // --- 1. 상품 기본 정보 섹션 ---
            var productGroup = CreateGroupBox("상품 기본 정보", 720, 280);
            mainPanel.Controls.Add(productGroup);
            CreateDetailRow(productGroup.Controls, "product_name", _product.ProductName, 1);
            CreateDetailRow(productGroup.Controls, "category_id", _product.CategoryId, 2);
            CreateDetailRow(productGroup.Controls, "product_image", "[이미지 데이터]", 3, true); // 이미지 행
            CreateDetailRow(productGroup.Controls, "description", _product.ProductDescription, 4);
            CreateDetailRow(productGroup.Controls, "base_price", _product.BasePrice, 5);
            CreateDetailRow(productGroup.Controls, "product_kcal", _product.ProductKcal, 6); // DB에 컬럼 추가 후 사용

            // --- 2. 옵션 그룹 정보 섹션 ---
            var optionGroupGroup = CreateGroupBox("옵션 그룹 (Option Groups)", 720, 250);
            mainPanel.Controls.Add(optionGroupGroup);
            foreach (var group in _product.OptionGroups)
            {
                CreateOptionGroupRow(optionGroupGroup.Controls, group);
            }

            // --- 3. 상세 옵션 정보 섹션 ---
            var optionsGroup = CreateGroupBox("상세 옵션 (Options)", 720, 300);
            mainPanel.Controls.Add(optionsGroup);
            var optionTreeView = new TreeView { Dock = DockStyle.Fill, Font = new Font("맑은 고딕", 10F), Margin = new Padding(10, 20, 10, 10) };
            optionsGroup.Controls.Add(optionTreeView);
            PopulateOptionTree(optionTreeView);

            // --- 확인 버튼 ---
            var btnConfirm = new Button { Text = "확인", Size = new Size(100, 40), Anchor = AnchorStyles.Right, Margin = new Padding(0, 10, 10, 10) };
            btnConfirm.Click += (s, e) => this.Close();
            mainPanel.Controls.Add(btnConfirm);

            this.Controls.Add(mainPanel);
        }

        // 각 섹션을 만들기 위한 GroupBox 생성 헬퍼 메서드
        private GroupBox CreateGroupBox(string title, int width, int height)
        {
            return new GroupBox
            {
                Text = title,
                Font = new Font("맑은 고딕", 12F, FontStyle.Bold),
                Size = new Size(width, height),
                Margin = new Padding(0, 0, 0, 15)
            };
        }

        // 상품 기본 정보 행을 만드는 헬퍼 메서드
        private void CreateDetailRow(Control.ControlCollection parent, string key, object value, int rowIndex, bool isImageRow = false)
        {
            int yPos = 25 + (rowIndex * 40);
            var lbl = new Label { Text = key, Location = new Point(20, yPos + 3), AutoSize = true };
            var txt = new TextBox { Text = value.ToString(), Location = new Point(150, yPos), Width = 400 };
            var btn = new Button { Text = "수정", Location = new Point(560, yPos - 1), Tag = key };

            if (isImageRow)
            {
                // 이미지 행은 이제 이미지 URL을 보여주고 수정합니다.
                txt.Text = _product.ProductImageUrl ?? ""; // DB에서 읽어온 URL 표시
                btn.Text = "URL 수정";
                btn.Click += (s, e) => {
                    DatabaseManager.Instance.UpdateCellValue("products", "product_id", _productId, "product_image", txt.Text);
                    MessageBox.Show("이미지 URL이 수정되었습니다.");
                };
            }
            else
            {
                btn.Click += (s, e) => {
                    DatabaseManager.Instance.UpdateCellValue("products", "product_id", _productId, key, txt.Text);
                    MessageBox.Show($"{key} 값이 수정되었습니다.");
                };
            }
            parent.AddRange(new Control[] { lbl, txt, btn });
        }

        // 옵션 그룹 행을 만드는 헬퍼 메서드
        private void CreateOptionGroupRow(Control.ControlCollection parent, OptionGroup group)
        {
            var pnl = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, Size = new Size(700, 35), Margin = new Padding(20, 0, 0, 0) };
            var lblId = new Label { Text = group.GroupId.ToString(), Width = 80, TextAlign = ContentAlignment.MiddleCenter };
            var txtName = new TextBox { Text = group.GroupName, Width = 150 };
            var txtRequired = new TextBox { Text = group.IsRequired.ToString(), Width = 150 };
            var txtMultiple = new TextBox { Text = group.AllowMultiple.ToString(), Width = 150 };
            var btnUpdate = new Button { Text = "수정", Tag = group };
            btnUpdate.Click += (s, e) => {
                // TODO: Option Group 수정 로직 구현
                MessageBox.Show($"Group ID {group.GroupId} 수정 로직 필요");
            };
            pnl.Controls.AddRange(new Control[] { lblId, txtName, txtRequired, txtMultiple, btnUpdate });
            parent.Add(pnl);
        }

        // 상세 옵션 TreeView를 채우는 헬퍼 메서드
        private void PopulateOptionTree(TreeView tv)
        {
            foreach (var group in _product.OptionGroups)
            {
                var groupNode = new TreeNode(group.GroupName);
                tv.Nodes.Add(groupNode);
                foreach (var option in group.Options)
                {
                    var optionNode = new TreeNode($"{option.OptionName} (ID:{option.OptionId}, +{option.AdditionalPrice:N0})");
                    groupNode.Nodes.Add(optionNode);
                }
            }
        }
    }
}