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

            dataGridView1.DataSource = _myDBContext.GetInventoryRecords();
        }

        private void addDateToCombox(ComboBox comboBox, List<string> list)
        {
            comboBox.Items.Clear();
            comboBox.Tag = list;

            foreach (string item in list)
            {
                comboBox.Items.Add(item);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string selectedPerson = comboBox1.SelectedItem.ToString();
            string selectedItem = comboBox2.SelectedItem.ToString();
            string serialNumber = textBox1.Text.ToString();

            int personId = _myDBContext.GetPersonIdByName(selectedPerson);
            int itemId = _myDBContext.GetItemIdByName(selectedItem);

            _myDBContext.AddRecordToInventory(personId, itemId, serialNumber);

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

        private void button2_Click(object sender, EventArgs e)
        {
            string person = comboBox4.SelectedItem?.ToString();
            string city = comboBox3.SelectedItem?.ToString();
            string item = comboBox5.SelectedItem?.ToString();

            int? personId = string.IsNullOrEmpty(person) ? (int?)null : _myDBContext.GetPersonIdByName(person);
            int? itemId = string.IsNullOrEmpty(item) ? (int?)null : _myDBContext.GetItemIdByName(item);

            dataGridView1.DataSource = _myDBContext.GetInventoryRecords(personId, itemId);
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
    }
}