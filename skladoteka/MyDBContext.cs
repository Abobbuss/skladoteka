using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.IO;

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

                if (!citiesTableExists || !peopleTableExists || !itemsTableExists || !inventoryTableExists)
                {
                    string createCitiesTableQuery = "CREATE TABLE IF NOT EXISTS Cities (Id INTEGER PRIMARY KEY, Name TEXT)";
                    string createPeopleTableQuery = "CREATE TABLE IF NOT EXISTS People (Id INTEGER PRIMARY KEY, FullName TEXT, CityId INTEGER, FOREIGN KEY(CityId) REFERENCES Cities(Id))";
                    string createItemsTableQuery = "CREATE TABLE IF NOT EXISTS Items (Id INTEGER PRIMARY KEY, Name TEXT)";
                    string createInventoryTableQuery = "CREATE TABLE IF NOT EXISTS Inventory (Id INTEGER PRIMARY KEY, PersonId INTEGER, ItemId INTEGER, SerialNumber TEXT, DateAdded DATETIME, FOREIGN KEY(PersonId) REFERENCES People(Id), FOREIGN KEY(ItemId) REFERENCES Items(Id))";

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

        public List<string> GetAllItems()
        {
            string query = "SELECT Name FROM Items";
            return GetAllData(query, "Name");
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

        public void AddRecordToInventory(int personId, int itemId, string serialNumber)
        {
            if (!IsIdValid("People", personId))
            {
                return;
            }

            if (!IsIdValid("Items", itemId))
            {
                return;
            }

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

                    cmd.ExecuteNonQuery();
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

        public DataTable GetInventoryRecords(int? personId = null, int? itemId = null)
        {
            DataTable dataTable = new DataTable();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT People.FullName AS PersonName, Items.Name AS ItemName, " +
                               "Inventory.SerialNumber, Inventory.DateAdded " +
                               "FROM Inventory " +
                               "INNER JOIN People ON Inventory.PersonId = People.Id " +
                               "INNER JOIN Items ON Inventory.ItemId = Items.Id";

                // Если передан параметр personId, добавляем условие WHERE для фильтрации по этому параметру
                if (personId != null)
                {
                    query += $" WHERE Inventory.PersonId = {personId}";
                }

                // Если передан параметр itemId, добавляем условие WHERE для фильтрации по этому параметру
                if (itemId != null)
                {
                    // Если уже есть условие WHERE, то добавляем AND
                    if (personId != null)
                    {
                        query += $" AND Inventory.ItemId = {itemId}";
                    }
                    else // Иначе начинаем новое условие WHERE
                    {
                        query += $" WHERE Inventory.ItemId = {itemId}";
                    }
                }

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    using (var adapter = new SQLiteDataAdapter(cmd))
                    {
                        adapter.Fill(dataTable);
                    }
                }
            }

            return dataTable;
        }
    
    
    }
}
