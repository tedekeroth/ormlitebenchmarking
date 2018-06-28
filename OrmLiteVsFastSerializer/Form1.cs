using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using tWorks.Alfa.AlfaCommons.Actors;
using tWorks.Alfa.AlfaCommons.Actors.ContactItems;
using tWorks.Core.CoreCommons.Actors;

namespace OrmLiteVsFastSerializer
{
    public partial class Form1 : Form
    {
        OrmLiteDbHandler ormLiteDbHandler;
        DbHandler dbHandler;
        RelationalDbHandler relDbHandler;
        DapperDbHandler dapperDbHandler;
        Stopwatch sw;
       
        public Form1()
        {
            InitializeComponent();
            sw = new Stopwatch();
            this.HandleCreated += Form1_HandleCreated;
        }

        private void Form1_HandleCreated(object sender, EventArgs e)
        {
            ormLiteDbHandler = new OrmLiteDbHandler();
            ormLiteDbHandler.OnLogEvent += OrmLiteDbHandler_OnLogEvent;
            ormLiteDbHandler.Start();

            dbHandler = new DbHandler();
            dbHandler.OnLogEvent += DbHandler_OnLogEvent;
            dbHandler.Start();

            relDbHandler = new RelationalDbHandler();
            relDbHandler.OnLogEvent += RelDbHandler_OnLogEvent;
            relDbHandler.Start();
        }

        private void RelDbHandler_OnLogEvent(object sender, string e)
        {
            Log(e);
        }

        private void DbHandler_OnLogEvent(object sender, string e)
        {
            Log(e);
        }

        private void OrmLiteDbHandler_OnLogEvent(object sender, string e)
        {
            Log(e);
        }

        private void Log(string log)
        {
            Invoke((MethodInvoker)delegate
            {
                textBox1.Text += $"{DateTime.Now.ToString("G")}\t{log}{Environment.NewLine}";
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;

            Task.Run(() =>
            {
                CreateCustomers();
                sw.Reset();
                sw.Start();
                uint counter = dbHandler.GetAutoIncrementValue<Customer>() + 1;
                string name = "";
                foreach (Customer c in customers)
                {
                    if (radioButton1.Checked)
                    {
                        ormLiteDbHandler.MyTestMethod(c);
                        name = "OrmLite";
                    }
                    else if (radioButton2.Checked)
                    {
                        dbHandler.AddCoreObjectTabelledToDatabase(counter, typeof(Customer), c, c.Serialize());
                        name = "Alfa";
                    }
                    else if (radioButton3.Checked)
                    {
                        relDbHandler.Save(c);
                        name = "Relational";
                    }
                    else if (radioButton4.Checked)
                    {
                        dapperDbHandler.Insert(c);
                        name = "Dapper";
                    }
                    counter++;
                }
                sw.Stop();
                Log($"CREATE {name} took\t{sw.ElapsedMilliseconds} ms");
                sw.Reset();
                Invoke((MethodInvoker)delegate
                {
                   button1.Enabled = true;
                });
            });
        }

        private Random random = new Random();
        public string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private List<Customer> customers = new List<Customer>();

        private void CreateCustomers()
        {
            customers.Clear();

            for (int i = 0; i < 10000; i++)
            {
                Customer c = new Customer(RandomString(10), RandomString(5), RandomString(4));
                c.Username = RandomString(4);
                c.ContactDetails = new ContactDetails();
                c.ContactDetails.ContactItemList.Add(new AlfaOnline(RandomString(10), RandomString(5), RandomString(6)));
                c._CustomerSettings = new CustomerSettings();
                c._CustomerSettings.CustomerConnect_NotifCancelled = true;
                c._CustomerSettings.CustomerConnect_NotifNoShowed = false;
                c.Century = 19;
                c.CustomerNumber = RandomString(5);
                c.ZipCode = RandomString(5);
                customers.Add(c);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;

            Task.Run(() =>
            {
                customers.Clear();

                sw.Reset();
                sw.Start();

                string name = radioButton1.Checked ? "OrmLite" : "Alfa";
                if (radioButton1.Checked)
                {
                    customers = ormLiteDbHandler.FetchAll<Customer>();
                }
                else if (radioButton2.Checked)
                {
                    customers = dbHandler.ReadObjectsTabelled<Customer>();
                }
                else if (radioButton3.Checked)
                {
                    customers = relDbHandler.FetchAll();
                }

                sw.Stop();
                Log($"LOAD {name} took\t{sw.ElapsedMilliseconds} ms, loaded {customers.Count} customers");
                sw.Reset();
                Invoke((MethodInvoker)delegate
                {
                    button2.Enabled = true;
                });
            });
        }
    }
}
