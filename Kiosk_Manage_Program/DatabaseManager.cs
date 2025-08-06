using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace Admin_Kiosk_Program // 관리자 프로그램용 네임스페이스
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

        // 특정 테이블의 모든 데이터를 가져오는 범용 메서드
        public DataTable GetTableData(string tableName)
        {
            DataTable dt = new DataTable();
            string query = $"SELECT * FROM {tableName}";
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

        // 특정 셀의 데이터를 업데이트하는 범용 메서드
        public void UpdateCellValue(string tableName, string primaryKeyColumn, object primaryKeyValue, string targetColumn, object newValue)
        {
            // SQL Injection 방지를 위해 컬럼 이름은 직접 검증하거나 화이트리스트 방식을 사용하는 것이 안전합니다.
            // 여기서는 전달된 값을 신뢰한다고 가정합니다.
            string query = $"UPDATE `{tableName}` SET `{targetColumn}` = @newValue WHERE `{primaryKeyColumn}` = @pkValue";
            ExecuteNonQuery(query,
                new MySqlParameter("@newValue", newValue),
                new MySqlParameter("@pkValue", primaryKeyValue));
        }

        // 특정 행을 삭제하는 범용 메서드
        public void DeleteRow(string tableName, string primaryKeyColumn, object primaryKeyValue)
        {
            string query = $"DELETE FROM `{tableName}` WHERE `{primaryKeyColumn}` = @pkValue";
            ExecuteNonQuery(query, new MySqlParameter("@pkValue", primaryKeyValue));
        }

        // C(Create), U(Update), D(Delete) 작업을 위한 공용 실행 메서드
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
                catch (Exception ex)
                {
                    MessageBox.Show($"DB 작업 오류: {ex.Message}");
                }
            }
        }

        public void AddNewRow(string tableName, Dictionary<string, object> data)
        {
            // Dictionary의 Key를 컬럼 이름으로, Value를 추가할 값으로 사용
            string columns = string.Join(", ", data.Keys.Select(k => $"`{k}`"));
            string values = string.Join(", ", data.Keys.Select(k => $"@{k}"));
            string query = $"INSERT INTO `{tableName}` ({columns}) VALUES ({values})";

            var parameters = data.Select(kvp => new MySqlParameter("@" + kvp.Key, kvp.Value ?? DBNull.Value)).ToArray();
            ExecuteNonQuery(query, parameters);
        }
    }
}