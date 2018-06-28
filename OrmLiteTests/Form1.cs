using Commons;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OrmLiteTests
{
    public partial class Form1 : Form
    {
        OrmLiteDbHandler ormLiteDbHandler;
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
                foreach (Customer c in customers)
                {
                    ormLiteDbHandler.AddRow(c);
                }
                sw.Stop();
                Log($"CREATE took\t{sw.ElapsedMilliseconds} ms");
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
                Customer c = new Customer();
                c.ContactDetails = null;

                if (radioButton2.Checked)
                {
                    PopulatePrimitives(c);
                }
                else if (radioButton3.Checked)
                {
                    PopulatePrimitives(c);
                    c._CustomerSettings = new CustomerSettings(); // 
                }
                else if (radioButton4.Checked)
                {
                    PopulatePrimitives(c);
                    c._CustomerSettings = new CustomerSettings();
                    c.ContactDetails = new ContactDetails();
                    c.ContactDetails.ContactItemList.Add(new AlfaOnline(RandomString(3), RandomString(5), RandomString(8)));
                    c.CurrentContactItem = new AlfaOnline(RandomString(5), RandomString(5), RandomString(5));
                }
                customers.Add(c);
            }
        }

        private void PopulatePrimitives(Customer c)
        {
            c.Username = RandomString(4);
            c.Century = 19;
            c.COAdress = RandomString(10);
            c.CustomerNumber = RandomString(5);
            c.ZipCode = RandomString(5);
            c.County = RandomString(3);
            c.DeceasedDate = DateTime.Now;
            c.HomeAddress = RandomString(20);
            c.Firstname = "Firstname " + RandomString(2);
            c.Firstname = "Lastname " + RandomString(2);
            c._AvailableCustomers = new List<uint>();
            c._AvailableCustomers.Add(31313);
            c.Classified = true;
            c.GroupCustomer = true;
            c.HomeAddressObj = 321231;
            c.Muni = RandomString(9);
            c.HomeCity = RandomString(7);
            c.PopulationRegistryNumber = RandomString(12);
            c.UpdateFromRegAutomatically = true;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;

            Task.Run(() =>
            {
                customers.Clear();

                sw.Reset();
                sw.Start();

                customers = ormLiteDbHandler.FetchAll<Customer>();

                sw.Stop();
                Log($"LOAD took\t{sw.ElapsedMilliseconds} ms, loaded {customers.Count} customers");
                sw.Reset();
                Invoke((MethodInvoker)delegate
                {
                    button2.Enabled = true;
                });
            });
        }
    }
}
