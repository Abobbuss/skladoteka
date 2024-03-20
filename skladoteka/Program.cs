using System;
using System.Windows.Forms;

namespace skladoteka
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string dbPath = "skladoteka.db";
            MyDBContext dbContext = MyDBContext.GetInstance(dbPath);

            dbContext.InitializeDatabase();

            Application.Run(new Form1());
        }
    }
}
