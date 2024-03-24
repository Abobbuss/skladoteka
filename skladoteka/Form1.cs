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
            List<string> allPeople = _myDBContext.GetAllPeople();
            List<string> allItem = _myDBContext.GetAllItems();

            addDateToCombox(comboBox1, allPeople);
            addDateToCombox(comboBox2, allItem);
            addDateToCombox(comboBox4, allPeople);
            addDateToCombox(comboBox5, allItem);

            comboBox1.Tag = allPeople;
            comboBox2.Tag = allItem;
            comboBox4.Tag = allPeople;
            comboBox5.Tag = allItem;
            comboBox10.Tag = allItem;
            comboBox11.Tag = allPeople;

            dataGridView1.DataSource = _myDBContext.GetInventoryRecords();
            dataGridView1.UserDeletingRow += DataGridView_UserDeletingRow;
            dataGridView1.CellClick += DataGridViewCellClicked;

            comboBox1.TextChanged += combobox_TextChanged;
            comboBox2.TextChanged += combobox_TextChanged;
            comboBox4.TextChanged += combobox_TextChanged;
            comboBox5.TextChanged += combobox_TextChanged;
            comboBox10.TextChanged += combobox_TextChanged;
            comboBox11.TextChanged += combobox_TextChanged;

            textBox2.Text = "1";

            EnableDisableChangeInfoToInventory(false);
        }

        private void addDateToCombox(ComboBox comboBox, List<string> list)
        {
            comboBox.Items.Clear();
            
            foreach (string item in list)
            {
                comboBox.Items.Add(item);
            }
        }

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
        }

        private void combobox_TextChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;

            if (comboBox != null)
            {
                string searchText = comboBox.Text.ToLower();
                List<string> allItems = comboBox.Tag as List<string>;

                if (allItems != null)
                {
                    int selectionStart = comboBox.SelectionStart;
                    int selectionLength = comboBox.SelectionLength;

                    List<string> filteredItems = allItems.Where(item => item.ToLower().Contains(searchText)).ToList();

                    if (comboBox.SelectedItem == null)
                        addDateToCombox(comboBox, filteredItems);

                    comboBox.Select(selectionStart, selectionLength);
                }
            }
        }

        private void DataGridView_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            int id = Convert.ToInt32(e.Row.Cells["ID"].Value);

            _myDBContext.DeleteInventoryRecord(id);
        }


        private void button2_Click(object sender, EventArgs e)
        {
            string person = comboBox4.SelectedItem?.ToString();
            string city = comboBox3.SelectedItem?.ToString();
            string item = comboBox5.SelectedItem?.ToString();

            int? personId = string.IsNullOrEmpty(person) ? (int?)null : _myDBContext.GetPersonIdByName(person);
            int? itemId = string.IsNullOrEmpty(item) ? (int?)null : _myDBContext.GetItemIdByName(item);

            dataGridView1.DataSource = _myDBContext.GetInventoryRecords(personId, itemId);
        }

        private void DataGridViewCellClicked(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
                ChangeInfoToInventory(e.RowIndex + 1);
        }

        private void ChangeInfoToInventory(int id)
        {
            EnableDisableChangeInfoToInventory(true);
            addDateToCombox(comboBox11, _myDBContext.GetAllPeople());
            addDateToCombox(comboBox10, _myDBContext.GetAllItems());

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
    }
}