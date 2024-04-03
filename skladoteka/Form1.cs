using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;

namespace skladoteka
{
    public partial class Form1 : Form
    {
        private MyDBContext _myDBContext;

        public Form1()
        {
            string dbPath = "skladoteka.db";
            _myDBContext = MyDBContext.GetInstance(dbPath);

            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateDateToCombox();

            dataGridView1.DataSource = _myDBContext.GetInventoryRecords();
            SetHideColumn("InventoryId", dataGridView1);
            dataGridView1.UserDeletingRow += DataGridView_UserDeletingRow;
            dataGridView1.CellClick += DataGridViewCellClicked;

            comboBox1.TextChanged += combobox_TextChanged;
            comboBox2.TextChanged += combobox_TextChanged;
            comboBox4.TextChanged += combobox_TextChanged;
            comboBox5.TextChanged += combobox_TextChanged;
            comboBox6.TextChanged += combobox_TextChanged;
            comboBox8.TextChanged += combobox_TextChanged;
            comboBox10.TextChanged += combobox_TextChanged;
            comboBox11.TextChanged += combobox_TextChanged;

            textBox2.Text = "1";

            EnableDisableChangeInfoToInventory(false);
        }

        // General

        private void AddDateToCombox(ComboBox comboBox, List<string> list)
        {
            comboBox.Items.Clear();
            comboBox.Items.Add("");
            comboBox.Items.AddRange(list.ToArray());
        }

        private void UpdateDateToCombox()
        {
            List<string> allPeople = _myDBContext.GetAllPeople();
            List<string> allItem = _myDBContext.GetAllItems();
            List<string> allCities = _myDBContext.GetAllCities();
            List<string> allSerialNumbers = _myDBContext.GetAllSerialNumbers();

            AddDateToCombox(comboBox1, allPeople);
            AddDateToCombox(comboBox2, allItem);
            AddDateToCombox(comboBox3, allCities);
            AddDateToCombox(comboBox4, allPeople);
            AddDateToCombox(comboBox5, allItem);
            AddDateToCombox(comboBox6, allSerialNumbers);
            AddDateToCombox(comboBox8, allCities);

            comboBox1.Tag = allPeople;
            comboBox2.Tag = allItem;
            comboBox3.Tag = allCities;
            comboBox4.Tag = allPeople;
            comboBox5.Tag = allItem;
            comboBox6.Tag = allSerialNumbers;
            comboBox8.Tag = allCities;
            comboBox10.Tag = allItem;
            comboBox11.Tag = allPeople;
        }

        private void combobox_TextChanged(object sender, EventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                string searchText = comboBox.Text.ToLower();
                List<string> allItems = comboBox.Tag as List<string>;

                if (allItems != null)
                {
                    int selectionStart = comboBox.SelectionStart;
                    int selectionLength = comboBox.SelectionLength;

                    List<string> filteredItems = allItems.Where(item => item.ToLower().Contains(searchText)).ToList();

                    if (comboBox.SelectedItem == null)
                        AddDateToCombox(comboBox, filteredItems);

                    comboBox.Select(selectionStart, selectionLength);
                }
            }
        }

        private void SetHideColumn(string columName, DataGridView dataGridView) => dataGridView.Columns[columName].Visible = false;

        // Insert

        private void button1_Click(object sender, EventArgs e)
        {
            string selectedPerson = comboBox1.SelectedItem?.ToString();
            string selectedItem = comboBox2.SelectedItem?.ToString();
            string serialNumber = textBox1.Text?.ToString();
            string quantity = textBox2.Text?.ToString();

            int personId = _myDBContext.GetPersonIdByName(selectedPerson);
            int itemId = _myDBContext.GetItemIdByName(selectedItem);
            int quantityInt;

            int.TryParse(quantity, out quantityInt);

            _myDBContext.AddRecordToInventory(personId, itemId, serialNumber, quantityInt);

            comboBox1.Text = null;
            comboBox2.Text = null;
            textBox1.Text = null;
            textBox2.Text = "1";

            UpdateDateToCombox();
        }

        // Inventory

        private void button2_Click(object sender, EventArgs e)
        {
            string person = comboBox4.SelectedItem?.ToString();
            string city = comboBox3.SelectedItem?.ToString();
            string item = comboBox5.SelectedItem?.ToString();
            string serialNumber = comboBox6.SelectedItem?.ToString();

            int? personId = string.IsNullOrEmpty(person) ? (int?)null : _myDBContext.GetPersonIdByName(person);
            int? itemId = string.IsNullOrEmpty(item) ? (int?)null : _myDBContext.GetItemIdByName(item);
            int? cityId = string.IsNullOrEmpty(city) ? (int?)null : _myDBContext.GetCityIdByName(city);

            dataGridView1.DataSource = _myDBContext.GetInventoryRecords(personId, itemId, cityId, serialNumber);
            SetHideColumn("InventoryId", dataGridView1);
        }
        
        private void DataGridViewCellClicked(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow selectedRow = dataGridView1.Rows[e.RowIndex];
                int id = Convert.ToInt32(selectedRow.Cells["InventoryId"].Value);

                ChangeInfoToInventory(id);
            }
        }

        private void DataGridView_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            int id = Convert.ToInt32(e.Row.Cells["InventoryId"].Value);

            _myDBContext.DeleteInventoryRecord(id);
        }

        // InfoToInventory

        private void ChangeInfoToInventory(int id)
        {
            EnableDisableChangeInfoToInventory(true);
            AddDateToCombox(comboBox11, _myDBContext.GetAllPeople());
            AddDateToCombox(comboBox10, _myDBContext.GetAllItems());

            Dictionary<string, object> recordValues = _myDBContext.GetInventoryRecordById(id);

            if (recordValues.ContainsKey("PersonId"))
            {
                comboBox11.SelectedItem = recordValues["PersonId"].ToString();
            }

            if (recordValues.ContainsKey("ItemId"))
            {
                comboBox10.SelectedItem = recordValues["ItemId"].ToString();
            }

            if (recordValues.ContainsKey("SerialNumber"))
            {
                textBox3.Text = recordValues["SerialNumber"].ToString();
            }

            if (recordValues.ContainsKey("DateAdded"))
            {
                textBox4.Text = recordValues["DateAdded"].ToString();
            }
        }
        
        private void EnableDisableChangeInfoToInventory(bool flag)
        {
            label13.Visible = flag;
            label14.Visible = flag;
            label15.Visible = flag;
            label16.Visible = flag;
            label17.Visible = flag;
            textBox4.Visible = flag;
            textBox3.Visible = flag;
            comboBox10.Visible = flag;
            comboBox11.Visible = flag;
            button3.Visible = flag;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int selectedRowIndex = GetSelectedRowIndex();

            if (selectedRowIndex == -1)
            {
                MessageBox.Show("Пожалуйста, выберите строку в таблице.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int recordId = selectedRowIndex + 1;

            string selectedPerson = comboBox11.SelectedItem.ToString();
            string selectedItem = comboBox10.SelectedItem.ToString();

            int selectedPersonId = _myDBContext.GetPersonIdByName(selectedPerson);
            int selectedItemId = _myDBContext.GetItemIdByName(selectedItem);

            _myDBContext.UpdateInventory("PersonId", selectedPersonId, recordId);
            _myDBContext.UpdateInventory("ItemId", selectedItemId, recordId);

            MessageBox.Show("Запись успешно обновлена.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private int GetSelectedRowIndex()
        {
            if (dataGridView1.SelectedCells.Count > 0)
            {
                int selectedRow = dataGridView1.SelectedCells[0].RowIndex;
                return selectedRow;
            }

            return -1;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string fullname = textBox5.Text;
            string city = comboBox8.Text;

            int cityId = _myDBContext.GetCityIdByName(city);

            _myDBContext.AddPerson(fullname, cityId);

            textBox5.Text = null;
            comboBox8.Text = null;
            
            UpdateDateToCombox();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string itemName = textBox6.Text;

            _myDBContext.AddItem(itemName);

            textBox6.Text = null;

            UpdateDateToCombox();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }


        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void label15_Click(object sender, EventArgs e)
        {

        }

        private void label17_Click(object sender, EventArgs e)
        {

        }

        private void label14_Click(object sender, EventArgs e)
        {

        }

        private void comboBox11_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox10_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox9_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox8_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox8_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}