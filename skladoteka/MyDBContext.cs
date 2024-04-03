using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace skladoteka
{
    class MyDBContext
    {
        private static MyDBContext _instance;
        private string _connectionString;
        private string _dbPath;

        private MyDBContext(string dbPath)
        {
            _dbPath = dbPath;
            _connectionString = $"Data Source={dbPath};Version=3;";
        }

        public void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                bool citiesTableExists = false;
                bool peopleTableExists = false;
                bool itemsTableExists = false;
                bool inventoryTableExists = false;
                bool historyTableExists = false; // Добавляем переменную для проверки существования таблицы "История"

                using (var cmd = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name='Cities'", connection))
                using (var reader = cmd.ExecuteReader())
                {
                    citiesTableExists = reader.Read();
                }

                using (var cmd = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name='People'", connection))
                using (var reader = cmd.ExecuteReader())
                {
                    peopleTableExists = reader.Read();
                }

                using (var cmd = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name='Items'", connection))
                using (var reader = cmd.ExecuteReader())
                {
                    itemsTableExists = reader.Read();
                }

                using (var cmd = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name='Inventory'", connection))
                using (var reader = cmd.ExecuteReader())
                {
                    inventoryTableExists = reader.Read();
                }

                using (var cmd = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name='History'", connection))
                using (var reader = cmd.ExecuteReader())
                {
                    historyTableExists = reader.Read();
                }

                if (!citiesTableExists || !peopleTableExists || !itemsTableExists || !inventoryTableExists || !historyTableExists) // Обновляем условие для проверки существования таблицы "История"
                {
                    string createCitiesTableQuery = "CREATE TABLE IF NOT EXISTS Cities (Id INTEGER PRIMARY KEY, Name TEXT)";
                    string createPeopleTableQuery = "CREATE TABLE IF NOT EXISTS People (Id INTEGER PRIMARY KEY, FullName TEXT, CityId INTEGER, FOREIGN KEY(CityId) REFERENCES Cities(Id))";
                    string createItemsTableQuery = "CREATE TABLE IF NOT EXISTS Items (Id INTEGER PRIMARY KEY, Name TEXT)";
                    string createInventoryTableQuery = "CREATE TABLE IF NOT EXISTS Inventory (Id INTEGER PRIMARY KEY, PersonId INTEGER, ItemId INTEGER, SerialNumber TEXT, DateAdded DATETIME, FOREIGN KEY(PersonId) REFERENCES People(Id), FOREIGN KEY(ItemId) REFERENCES Items(Id))";
                    string createHistoryTableQuery = "CREATE TABLE IF NOT EXISTS History (Id INTEGER PRIMARY KEY, PersonId INTEGER, ItemId INTEGER, SerialNumber TEXT, DateAdded DATETIME, Comment TEXT, FOREIGN KEY(PersonId) REFERENCES People(Id), FOREIGN KEY(ItemId) REFERENCES Items(Id))";

                    using (var cmd = new SQLiteCommand(createCitiesTableQuery, connection))
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("Table 'Cities' created successfully.");
                    }
                    using (var cmd = new SQLiteCommand(createPeopleTableQuery, connection))
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("Table 'People' created successfully.");
                    }
                    using (var cmd = new SQLiteCommand(createItemsTableQuery, connection))
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("Table 'Items' created successfully.");
                    }
                    using (var cmd = new SQLiteCommand(createInventoryTableQuery, connection))
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("Table 'Inventory' created successfully.");
                    }
                    
                    using (var cmd = new SQLiteCommand(createHistoryTableQuery, connection))
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("Table 'History' created successfully.");
                    }
                }
                else
                {
                    Console.WriteLine("Database and tables already exist.");
                }
            }
        }


        public static MyDBContext GetInstance(string dbPath)
        {
            if (_instance == null)
                _instance = new MyDBContext(dbPath);

            return _instance;
        }

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(_connectionString);
        }

        public List<string> GetAllPeople()
        {
            string query = "SELECT FullName FROM People";
            return GetAllData(query, "FullName");
        }

        public List<string> GetAllCities()
        {
            string query = "SELECT Name FROM Cities";
            return GetAllData(query, "Name");
        }

        public List<string> GetAllItems()
        {
            string query = "SELECT Name FROM Items";
            return GetAllData(query, "Name");
        }

        public List<string> GetAllSerialNumbers()
        {
            List<string> serialNumbers = new List<string>();

            try
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    string query = "SELECT DISTINCT SerialNumber FROM Inventory";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string serialNumber = reader["SerialNumber"].ToString();
                                serialNumbers.Add(serialNumber);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при получении серийных номеров: " + ex.Message);
            }

            return serialNumbers;
        }

        public List<string> GetAllData(string query, string columnName)
        {
            List<string> dataList = new List<string>();

            using (var connection = GetConnection())
            {
                connection.Open();

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string data = reader[columnName].ToString();
                            dataList.Add(data);
                        }
                    }
                }
            }

            return dataList;
        }

        public void AddRecordToInventory(int personId, int itemId, string serialNumber, int quantity = 1)
        {
            if (!IsIdValid("People", personId))
                return;

            if (!IsIdValid("Items", itemId))
                return;

            DateTime currentDate = DateTime.Now;

            using (var connection = GetConnection())
            {
                connection.Open();

                string query = "INSERT INTO Inventory (PersonId, ItemId, SerialNumber,DateAdded) VALUES (@PersonId, @ItemId, @SerialNumber, @DateAdded)";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@PersonId", personId);
                    cmd.Parameters.AddWithValue("@ItemId", itemId);
                    cmd.Parameters.AddWithValue("@SerialNumber", serialNumber);
                    cmd.Parameters.AddWithValue("@DateAdded", currentDate);

                    for (int i = 0; i < quantity; i++)
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private bool IsIdValid(string tableName, int id)
        {
            using (var connection = GetConnection())
            {
                connection.Open();

                string query = $"SELECT COUNT(*) FROM {tableName} WHERE Id = @Id";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());

                    return count > 0;
                }
            }
        }

        public int GetPersonIdByName(string fullName) => GetIdByParameter("People", "FullName", fullName);
        public int GetCityIdByName(string cityName) => GetIdByParameter("Cities", "Name", cityName);
        public int GetItemIdByName(string itemName) => GetIdByParameter("Items", "Name", itemName);

        private int GetIdByParameter(string tableName, string columnName, string parameterValue)
        {
            int id = -1;

            using (var connection = GetConnection())
            {
                connection.Open();

                string query = $"SELECT Id FROM {tableName} WHERE {columnName} = @ParameterValue";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@ParameterValue", parameterValue);

                    var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        id = Convert.ToInt32(result);
                    }
                }
            }

            return id;
        }

        public void AddPerson(string fullName, int cityId)
        {
            try
            {
                bool cityExists = IsIdValid("Cities", cityId);
                if (!cityExists)
                {
                    Console.WriteLine($"Города с ID {cityId} не существует.");
                    return;
                }

                using (var connection = GetConnection())
                {
                    connection.Open();

                    string query = "INSERT INTO People (FullName, CityId) VALUES (@FullName, @CityId)";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@FullName", fullName);
                        cmd.Parameters.AddWithValue("@CityId", cityId);

                        cmd.ExecuteNonQuery();
                    }
                }

                Console.WriteLine("Человек успешно добавлен.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при добавлении человека: " + ex.Message);
            }
        }

        public void AddItem(string itemName)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();

                    string query = "INSERT INTO Items (Name) VALUES (@Name)";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Name", itemName);

                        cmd.ExecuteNonQuery();
                    }
                }

                Console.WriteLine("Вещь успешно добавлена.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при добавлении вещи: " + ex.Message);
            }
        }

        public DataTable GetInventoryRecords(int? personId = null, int? itemId = null, int? cityId = null, string serialNumber = null)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();

                    string query = "SELECT Inventory.Id AS InventoryId, People.FullName AS PersonId, Items.Name AS ItemId, " +
                                   "Inventory.SerialNumber, Inventory.DateAdded " +
                                   "FROM Inventory " +
                                   "INNER JOIN People ON Inventory.PersonId = People.Id " +
                                   "INNER JOIN Items ON Inventory.ItemId = Items.Id " +
                                   "WHERE 1 = 1";

                    if (personId != null)
                        query += $" AND Inventory.PersonId = {personId}";

                    if (itemId != null)
                        query += $" AND Inventory.ItemId = {itemId}";

                    if (cityId != null)
                    {
                        string subQuery = $"SELECT Id FROM People WHERE CityId = {cityId}";
                        query += $" AND Inventory.PersonId IN ({subQuery})";
                    }

                    if (!string.IsNullOrEmpty(serialNumber))
                        query += $" AND Inventory.SerialNumber LIKE '{serialNumber}%'";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        using (var adapter = new SQLiteDataAdapter(cmd))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при получении записей инвентаря: " + ex.Message);
            }

            AddCounterColumn(dataTable);

            return dataTable;
        }

        private void AddCounterColumn(DataTable dataTable)
        {
            DataColumn counterColumn = new DataColumn("ID", typeof(int));

            dataTable.Columns.Add(counterColumn);

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                dataTable.Rows[i]["ID"] = i + 1;
            }

            dataTable.Columns["ID"].SetOrdinal(0);
        }

        public Dictionary<string, object> GetInventoryRecordById(int recordId)
        {
            Dictionary<string, object> recordValues = new Dictionary<string, object>();

            using (var connection = GetConnection())
            {
                connection.Open();

                string query = "SELECT People.FullName AS PersonId, Items.Name AS ItemId, " +
                               "Inventory.SerialNumber, Inventory.DateAdded " +
                               "FROM Inventory " +
                               "INNER JOIN People ON Inventory.PersonId = People.Id " +
                               "INNER JOIN Items ON Inventory.ItemId = Items.Id " +
                               $"WHERE Inventory.Id = {recordId}";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string columnName = reader.GetName(i);
                                object columnValue = reader.GetValue(i);
                                recordValues.Add(columnName, columnValue);
                            }
                        }
                    }
                }
            }

            return recordValues;
        }

        public void UpdateInventory(string columnName, int newValue, int id)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();

                    string query = $"UPDATE Inventory SET {columnName} = @NewValue WHERE ID = @ID";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@NewValue", newValue);
                        cmd.Parameters.AddWithValue("@ID", id);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при обновлении записи: " + ex.Message);
                return;
            }
        }

        public void DeleteInventoryRecord(int id)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();

                    string query = "DELETE FROM Inventory WHERE ID = @ID";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@ID", id);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при удалении записи: " + ex.Message);
            }
        }

    }
}
