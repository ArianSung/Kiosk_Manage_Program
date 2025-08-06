using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Admin_Kiosk_Program
{
    public class DatabaseManager
    {
        private static readonly DatabaseManager instance = new DatabaseManager();
        public static DatabaseManager Instance => instance;
        private readonly string connectionString;

        private DatabaseManager()
        {
            // 키오스크 DB와 동일한 DB에 연결합니다.
            string server = "192.168.0.81";
            string database = "kiosk_project";
            string uid = "kiosk_user";
            string password = "123456";
            connectionString = $"SERVER={server};DATABASE={database};UID={uid};PASSWORD={password};";
        }

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

        // --- 데이터 조회 (Read) ---

        public DataTable GetTableData(string tableName)
        {
            DataTable dt = new DataTable();
            string query = $"SELECT * FROM `{tableName}`";
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    using (var adapter = new MySqlDataAdapter(query, conn))
                    {
                        adapter.Fill(dt);
                    }
                }
                catch (Exception ex) { MessageBox.Show($"{tableName} 테이블 로딩 오류: {ex.Message}"); }
            }
            return dt;
        }

        public List<Category> GetCategoriesWithProducts()
        {
            var categories = new List<Category>();
            string query = @"
                SELECT c.category_id, c.category_name, p.product_id, p.product_name
                FROM categories c
                LEFT JOIN products p ON c.category_id = p.category_id
                ORDER BY c.category_id, p.product_id;";
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int categoryId = reader.GetInt32("category_id");
                            if (!categories.Any(c => c.CategoryId == categoryId))
                            {
                                categories.Add(new Category { CategoryId = categoryId, CategoryName = reader.GetString("category_name") });
                            }

                            if (!reader.IsDBNull(reader.GetOrdinal("product_id")))
                            {
                                categories.First(c => c.CategoryId == categoryId).Products.Add(new Product
                                {
                                    ProductId = reader.GetInt32("product_id"),
                                    ProductName = reader.GetString("product_name")
                                });
                            }
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show($"카테고리/상품 목록 로딩 오류: {ex.Message}"); }
            }
            return categories;
        }

        public Product GetProductDetails(int productId)
        {
            Product product = null;
            string query = "SELECT * FROM products WHERE product_id = @id";
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", productId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                product = new Product
                                {
                                    ProductId = reader.GetInt32("product_id"),
                                    CategoryId = reader.GetInt32("category_id"),
                                    ProductName = reader.GetString("product_name"),
                                    BasePrice = reader.GetDecimal("base_price"),
                                    ProductDescription = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString("description"),
                                    ProductKcal = reader.IsDBNull(reader.GetOrdinal("product_kcal")) ? 0 : reader.GetInt32("product_kcal"),
                                    // ▼▼▼▼▼ BLOB이 아닌 VARCHAR(URL)을 읽도록 수정 ▼▼▼▼▼
                                    ProductImageUrl = reader.IsDBNull(reader.GetOrdinal("product_image")) ? null : reader.GetString("product_image")
                                };
                            }
                        }
                    }
                    if (product != null)
                    {
                        product.OptionGroups = GetOptionsForProduct(product.ProductId);
                    }
                }
                catch (Exception ex) { MessageBox.Show($"상품 상세 정보 로딩 오류: {ex.Message}"); }
            }
            return product;
        }

        public List<OptionGroup> GetOptionsForProduct(int productId)
        {
            var optionGroups = new List<OptionGroup>();
            string groupQuery = "SELECT group_id, group_name, is_required, allow_multiple FROM option_groups WHERE product_id = @product_id ORDER BY display_order";
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var groupCmd = new MySqlCommand(groupQuery, conn))
                {
                    groupCmd.Parameters.AddWithValue("@product_id", productId);
                    using (var groupReader = groupCmd.ExecuteReader())
                    {
                        while (groupReader.Read())
                        {
                            optionGroups.Add(new OptionGroup
                            {
                                GroupId = groupReader.GetInt32("group_id"),
                                GroupName = groupReader.GetString("group_name"),
                                IsRequired = groupReader.GetBoolean("is_required"),
                                AllowMultiple = groupReader.GetBoolean("allow_multiple"),
                                ProductId = productId
                            });
                        }
                    }
                }

                foreach (var group in optionGroups)
                {
                    string optionQuery = "SELECT option_id, option_name, additional_price FROM options WHERE group_id = @group_id ORDER BY display_order";
                    using (var optionCmd = new MySqlCommand(optionQuery, conn))
                    {
                        optionCmd.Parameters.AddWithValue("@group_id", group.GroupId);
                        using (var optionReader = optionCmd.ExecuteReader())
                        {
                            while (optionReader.Read())
                            {
                                group.Options.Add(new Option
                                {
                                    OptionId = optionReader.GetInt32("option_id"),
                                    OptionName = optionReader.GetString("option_name"),
                                    AdditionalPrice = optionReader.GetDecimal("additional_price"),
                                    GroupId = group.GroupId
                                });
                            }
                        }
                    }
                }
            }
            return optionGroups;
        }

        // --- 데이터 수정 (Update) ---
        public void UpdateCellValue(string tableName, string primaryKeyColumn, object primaryKeyValue, string targetColumn, object newValue)
        {
            string query = $"UPDATE `{tableName}` SET `{targetColumn}` = @newValue WHERE `{primaryKeyColumn}` = @pkValue";
            ExecuteNonQuery(query,
                new MySqlParameter("@newValue", newValue ?? DBNull.Value),
                new MySqlParameter("@pkValue", primaryKeyValue));
        }

        public void UpdateOptionValue(int optionId, string columnName, object value)
        {
            UpdateCellValue("options", "option_id", optionId, columnName, value);
        }

        // --- 데이터 삭제 (Delete) ---
        public void DeleteRow(string tableName, string primaryKeyColumn, object primaryKeyValue)
        {
            string query = $"DELETE FROM `{tableName}` WHERE `{primaryKeyColumn}` = @pkValue";
            ExecuteNonQuery(query, new MySqlParameter("@pkValue", primaryKeyValue));
        }

        public void AddNewRow(string tableName, Dictionary<string, object> data)
        {
            string columns = string.Join(", ", data.Keys.Select(k => $"`{k}`"));
            string values = string.Join(", ", data.Keys.Select(k => $"@{k}"));
            string query = $"INSERT INTO `{tableName}` ({columns}) VALUES ({values})";
            var parameters = data.Select(kvp => new MySqlParameter("@" + kvp.Key, kvp.Value ?? DBNull.Value)).ToArray();
            ExecuteNonQuery(query, parameters);
        }

        // ▼▼▼▼▼ 여기에 누락되었던 두 메서드를 추가합니다 ▼▼▼▼▼
        public void AddProduct(Dictionary<string, object> data)
        {
            AddNewRow("products", data);
        }

        public void DeleteProduct(int productId)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                try
                {
                    var groupIds = new List<int>();
                    string getGroupsQuery = "SELECT group_id FROM option_groups WHERE product_id = @productId";
                    using (var cmd = new MySqlCommand(getGroupsQuery, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@productId", productId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read()) { groupIds.Add(reader.GetInt32(0)); }
                        }
                    }

                    if (groupIds.Count > 0)
                    {
                        string deleteOptionsQuery = $"DELETE FROM options WHERE group_id IN ({string.Join(",", groupIds)})";
                        using (var cmd = new MySqlCommand(deleteOptionsQuery, conn, transaction)) { cmd.ExecuteNonQuery(); }
                    }

                    string deleteGroupsQuery = "DELETE FROM option_groups WHERE product_id = @productId";
                    using (var cmd = new MySqlCommand(deleteGroupsQuery, conn, transaction)) { cmd.Parameters.AddWithValue("@productId", productId); cmd.ExecuteNonQuery(); }

                    string deleteProductQuery = "DELETE FROM products WHERE product_id = @productId";
                    using (var cmd = new MySqlCommand(deleteProductQuery, conn, transaction)) { cmd.Parameters.AddWithValue("@productId", productId); cmd.ExecuteNonQuery(); }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"상품 삭제 오류: {ex.Message}");
                }
            }
        }

        public int AddProductAndGetId(Dictionary<string, object> data)
        {
            string columns = string.Join(", ", data.Keys.Select(k => $"`{k}`"));
            string values = string.Join(", ", data.Keys.Select(k => $"@{k}"));
            string query = $"INSERT INTO `products` ({columns}) VALUES ({values}); SELECT LAST_INSERT_ID();";
            var parameters = data.Select(kvp => new MySqlParameter("@" + kvp.Key, kvp.Value ?? DBNull.Value)).ToArray();
            return ExecuteScalar(query, parameters);
        }

        public int AddOptionGroup(int productId, string groupName, bool isRequired, bool allowMultiple)
        {
            string query = "INSERT INTO option_groups (product_id, group_name, is_required, allow_multiple) VALUES (@pId, @name, @req, @multi); SELECT LAST_INSERT_ID();";
            return ExecuteScalar(query,
                new MySqlParameter("@pId", productId),
                new MySqlParameter("@name", groupName),
                new MySqlParameter("@req", isRequired),
                new MySqlParameter("@multi", allowMultiple));
        }

        public int AddOption(int groupId, string optionName, decimal additionalPrice)
        {
            string query = "INSERT INTO options (group_id, option_name, additional_price) VALUES (@gId, @name, @price); SELECT LAST_INSERT_ID();";
            return ExecuteScalar(query,
                new MySqlParameter("@gId", groupId),
                new MySqlParameter("@name", optionName),
                new MySqlParameter("@price", additionalPrice));
        }

        // --- 범용 실행 메서드 ---
        private void ExecuteNonQuery(string query, params MySqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddRange(parameters);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex) { MessageBox.Show($"DB 작업 오류: {ex.Message}"); }
            }
        }

        private int ExecuteScalar(string query, params MySqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddRange(parameters);
                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"DB 작업(Scalar) 오류: {ex.Message}");
                    return -1;
                }
            }
        }
    }
}