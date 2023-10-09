using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using NetworkSelector.Models;

namespace NetworkSelector.Datas
{
    internal class SQLiteHelper
    {
        private string connectionString = "Data Source=ns.db";

        public SQLiteHelper()
        {
            CreateTableIfNotExists();
        }
        // 建表
        public void CreateTableIfNotExists()
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var createTableCommand = connection.CreateCommand();
                createTableCommand.CommandText = "CREATE TABLE IF NOT EXISTS NSTable (Id INTEGER PRIMARY KEY, Name TEXT, Netinterface Text, IPAddr TEXT, Mask TEXT, Gateway TEXT, DNS1 TEXT, DNS2 TEXT)";
                createTableCommand.ExecuteNonQuery();
            }
        }
        // 删表
        public void DropTable()
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var dropTableCommand = connection.CreateCommand();
                dropTableCommand.CommandText = $"DROP TABLE IF EXISTS NSTable;";
                dropTableCommand.ExecuteNonQuery();
            }
        }
        // 插入数据
        public void InsertData(NSModel model)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var insertCommand = connection.CreateCommand();
                insertCommand.CommandText = "INSERT INTO NSTable (Name, Netinterface, IPAddr, Mask, Gateway, DNS1, DNS2) VALUES (@Name, @Netinterface, @IPAddr, @Mask, @Gateway, @DNS1, @DNS2)";
                insertCommand.Parameters.AddWithValue("@Name", model.Name ?? (object)DBNull.Value);
                insertCommand.Parameters.AddWithValue("@Netinterface", model.Netinterface ?? (object)DBNull.Value);
                insertCommand.Parameters.AddWithValue("@IPAddr", model.IPAddr ?? (object)DBNull.Value);
                insertCommand.Parameters.AddWithValue("@Mask", model.Mask ?? (object)DBNull.Value);
                insertCommand.Parameters.AddWithValue("@Gateway", model.Gateway ?? (object)DBNull.Value);
                insertCommand.Parameters.AddWithValue("@DNS1", model.DNS1 ?? (object)DBNull.Value);
                insertCommand.Parameters.AddWithValue("@DNS2", model.DNS2 ?? (object)DBNull.Value);

                insertCommand.ExecuteNonQuery();
            }
        }

        // 更新数据
        public void UpdateData(NSModel model)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var updateCommand = connection.CreateCommand();
                updateCommand.CommandText = "UPDATE NSTable SET Name = @Name, Netinterface = @Netinterface, IPAddr = @IPAddr, Mask = @Mask, Gateway = @Gateway, DNS1 = @DNS1, DNS2 = @DNS2 WHERE Id = @Id";
                updateCommand.Parameters.AddWithValue("@Id", model.Id);
                updateCommand.Parameters.AddWithValue("@Name", model.Name ?? (object)DBNull.Value);
                updateCommand.Parameters.AddWithValue("@Netinterface", model.Netinterface ?? (object)DBNull.Value);
                updateCommand.Parameters.AddWithValue("@IPAddr", model.IPAddr ?? (object)DBNull.Value);
                updateCommand.Parameters.AddWithValue("@Mask", model.Mask ?? (object)DBNull.Value);
                updateCommand.Parameters.AddWithValue("@Gateway", model.Gateway ?? (object)DBNull.Value);
                updateCommand.Parameters.AddWithValue("@DNS1", model.DNS1 ?? (object)DBNull.Value);
                updateCommand.Parameters.AddWithValue("@DNS2", model.DNS2 ?? (object)DBNull.Value);

                updateCommand.ExecuteNonQuery();
            }
        }

        // 删除数据
        public void DeleteData(NSModel model)
        {
            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var deleteCommand = connection.CreateCommand();
                deleteCommand.CommandText = "DELETE FROM NSTable WHERE Id = @Id";
                deleteCommand.Parameters.AddWithValue("@Id", model.Id);

                deleteCommand.ExecuteNonQuery();
            }
        }

        // 查询数据
        public List<NSModel> QueryData()
        {
            List<NSModel> entries = new List<NSModel>();

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var queryCommand = connection.CreateCommand();
                queryCommand.CommandText = "SELECT * FROM NSTable";

                using (SqliteDataReader reader = queryCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        NSModel entry = new NSModel
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                            Netinterface = reader.IsDBNull(2) ? null : reader.GetString(2),
                            IPAddr = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Mask = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Gateway = reader.IsDBNull(5) ? null : reader.GetString(5),
                            DNS1 = reader.IsDBNull(6) ? null : reader.GetString(6),
                            DNS2 = reader.IsDBNull(7) ? null : reader.GetString(7)
                        };
                        entries.Add(entry);
                    }
                }
            }

            return entries;
        }
    }
}
