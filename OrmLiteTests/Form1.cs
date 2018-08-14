using Commons;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OrmLiteTests
{
    public partial class Form1 : Form
    {
        OrmLiteDbHandler ormLiteDbHandler;
        VeolcityDbHandler veloDbHandler;
        Stopwatch sw;

        public Form1()
        {
            InitializeComponent();
            sw = new Stopwatch();
            this.HandleCreated += Form1_HandleCreated;
        }

        private void Form1_HandleCreated(object sender, EventArgs e)
        {
            veloDbHandler = new VeolcityDbHandler();
            veloDbHandler.ClearAll<Customer>();
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
                    CoreObject co = (CoreObject)c;
                    ormLiteDbHandler.AddRow(co);
                }
                sw.Stop();
                Log($"CREATE took\t{sw.ElapsedMilliseconds} ms");
                sw.Reset();

                sw.Start();
                veloDbHandler.InsertList(customers);
                sw.Stop();
                Log($"CREATE VeloDb InsertList took\t{sw.ElapsedMilliseconds} ms");
                sw.Reset();

                sw.Start();
                int i = 0;
                foreach (Customer c in customers)
                {
                    CoreObject co = (CoreObject)c;
                    veloDbHandler.Insert(co);
                    i++;
                    if (i % 50 == 0)
                    {
                        double speed = (i + 0.0) / (sw.ElapsedMilliseconds / 1000.0);
                        Log($"VeloSpeed: {speed} rec/s ({sw.ElapsedMilliseconds / 1000.0} seconds, {i} rec)");
                    }
                }
                sw.Stop();
                Log($"CREATE VelocityDB took\t{sw.ElapsedMilliseconds} ms");

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
                    c.ContactDetails.ContactItemList = new List<ContactItem>();
                    c.ContactDetails.ContactItemList.Add(new AlfaOnline(RandomString(3), RandomString(5), RandomString(8)));
                    c.CurrentContactItem = new AlfaOnline(RandomString(5), RandomString(5), RandomString(5));

                    c._CustomProperties.Add("MyBoolTest", true);
                    c._CustomProperties.Add("MyOtherBoolTest", false);
                    c._CustomProperties.Add("MyDateTimeTest", DateTime.Now);
                    c._SetCustomProperty("Settings", new CustomerSettings() { CustomerConnect_NotifCancelled = true, NotifFinished = true });
                    c._SetCustomProperty("MyList", new List<uint> { 1, 2, 3, 4 });
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
                //sw.Start();
                //List<CoreObject> list = ormLiteDbHandler.FetchAll(typeof(Customer));
                //sw.Stop();
                //Log($"REFLECTION LOAD took\t{sw.ElapsedMilliseconds} ms, loaded {list.Count} customers");
                //sw.Reset();
                //Console.WriteLine($"MyBoolTest={list[10]._CustomProperties["MyBoolTest"]}");
                //Console.WriteLine($"MyOtherBoolTest={list[10]._CustomProperties["MyOtherBoolTest"]}");
                //Console.WriteLine($"MyDateTimeTest={list[10]._CustomProperties["MyDateTimeTest"]}");
                sw.Start();
                customers = ormLiteDbHandler.FetchAll<Customer>();
                sw.Stop();
                Log($"LOAD took\t{sw.ElapsedMilliseconds} ms, loaded {customers.Count} customers");

                customers.Clear();
                sw.Reset();
                customers = veloDbHandler.ReadAll<Customer>();

                Invoke((MethodInvoker)delegate
                {
                    button2.Enabled = true;
                });
            });
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //uint id = ormLiteDbHandler.GetHighestCoreObjectId();

            //CoreActionSettings cas = new CoreActionSettings();
            //cas.Id = 3;
            //cas._AvailableActions = new List<ActionInfo>();
            //cas._AvailableActions.Add(new ActionInfo("Test", "desc", new List<ParameterInfo>()));
            //cas._AvailableActions.Add(new ActionInfo("Test2", "desc2", new List<ParameterInfo>()));
            //cas._AvailableActions.Add(new ActionInfo("Test4", "desc4", new List<ParameterInfo>()));
            //cas._AvailableActions.Add(new ActionInfo("Test4asd", "asddesc4", new List<ParameterInfo>()));
            //cas._AvailableActions.Add(new ActionInfo("Test", "desc", new List<ParameterInfo>()));
            //cas._AvailableActions.Add(new ActionInfo("Test2", "desc2", new List<ParameterInfo>()));
            //cas._AvailableActions.Add(new ActionInfo("Test4", "desc4", new List<ParameterInfo>()));
            //cas._AvailableActions.Add(new ActionInfo("Test4asd", "asddesc4", new List<ParameterInfo>()));
            //cas._AvailableActions.Add(new ActionInfo("Test", "desc", new List<ParameterInfo>()));
            //cas._AvailableActions.Add(new ActionInfo("Test2", "desc2", new List<ParameterInfo>()));
            //cas._AvailableActions.Add(new ActionInfo("Test4", "desc4", new List<ParameterInfo>()));
            //cas._AvailableActions.Add(new ActionInfo("Test4asd", "asddesc4", new List<ParameterInfo>()));
            //cas._AvailablePoints = new List<string>();
            //cas._AvailablePoints.Add("Str1");
            //cas._AvailablePoints.Add("Str2");
            //ormLiteDbHandler.AddRow<CoreActionSettings>(cas);

            //ModuleController mc = new ModuleController("test", new List<Type>() { typeof(Customer) });
            ////mc.ContactTypes.Add(null);
            //mc.ContactDetails.ContactItemList.Add(new CoreClient());
            //ormLiteDbHandler.AddRow<ModuleController>(mc);

            ormLiteDbHandler.TestStringSerializers();
        }

        private R GetPoolAndRun<R>(Func<OrmLiteDbHandler, R> theFunction)
        {
            R result = default(R);

            int poolCounter = 0;
            while (poolCounter < 3)
            {
                try
                {
                    result = theFunction(ormLiteDbHandler);
                    return result;
                }
                catch(Exception e)
                {
                    // Something is wrong, we will try the next pool
                    Console.WriteLine(e);
                }
                poolCounter++;
            }
            return default(R);
        }

        private void test()
        {
            ormLiteDbHandler.CustomSelect("");
            CoreObject co = ormLiteDbHandler.Get<Customer>(3);
            ormLiteDbHandler.Delete(co);
        }


        private void button4_Click(object sender, EventArgs e)
        {

            //Dictionary<ushort, Dictionary<tWorks.Alfa.AlfaCommons.Geo.Geography.Ranges, HashSet<uint>>> latDictionary;
            //latDictionary = new Dictionary<ushort, Dictionary<tWorks.Alfa.AlfaCommons.Geo.Geography.Ranges, HashSet<uint>>>();
            //latDictionary.Add(1, new Dictionary<tWorks.Alfa.AlfaCommons.Geo.Geography.Ranges, HashSet<uint>>());
            //latDictionary[1].Add(tWorks.Alfa.AlfaCommons.Geo.Geography.Ranges.Km3, new HashSet<uint>());
            //latDictionary[1][tWorks.Alfa.AlfaCommons.Geo.Geography.Ranges.Km3].Add(1);
            //latDictionary[1][tWorks.Alfa.AlfaCommons.Geo.Geography.Ranges.Km3].Add(2);
            //latDictionary[1][tWorks.Alfa.AlfaCommons.Geo.Geography.Ranges.Km3].Add(3);
            //latDictionary[1][tWorks.Alfa.AlfaCommons.Geo.Geography.Ranges.Km3].Add(4);
            //latDictionary[1][tWorks.Alfa.AlfaCommons.Geo.Geography.Ranges.Km3].Add(5);

            //latDictionary[1].Add(tWorks.Alfa.AlfaCommons.Geo.Geography.Ranges.Km9, new HashSet<uint>());
            //latDictionary[1][tWorks.Alfa.AlfaCommons.Geo.Geography.Ranges.Km3].Add(1);
            //latDictionary[1][tWorks.Alfa.AlfaCommons.Geo.Geography.Ranges.Km3].Add(2);
            //latDictionary[1][tWorks.Alfa.AlfaCommons.Geo.Geography.Ranges.Km3].Add(3);
            //latDictionary[1][tWorks.Alfa.AlfaCommons.Geo.Geography.Ranges.Km3].Add(4);
            //latDictionary[1][tWorks.Alfa.AlfaCommons.Geo.Geography.Ranges.Km3].Add(5);

            //latDictionary[1].Add(tWorks.Alfa.AlfaCommons.Geo.Geography.Ranges.Km27, new HashSet<uint>());
            //latDictionary[1][tWorks.Alfa.AlfaCommons.Geo.Geography.Ranges.Km3].Add(1);
            //latDictionary[1][tWorks.Alfa.AlfaCommons.Geo.Geography.Ranges.Km3].Add(2);
            //latDictionary[1][tWorks.Alfa.AlfaCommons.Geo.Geography.Ranges.Km3].Add(3);
            //latDictionary[1][tWorks.Alfa.AlfaCommons.Geo.Geography.Ranges.Km3].Add(4);
            //latDictionary[1][tWorks.Alfa.AlfaCommons.Geo.Geography.Ranges.Km3].Add(5);

            //latDictionary.Add(2, new Dictionary<tWorks.Alfa.AlfaCommons.Geo.Geography.Ranges, HashSet<uint>>());
            //latDictionary[2].Add(tWorks.Alfa.AlfaCommons.Geo.Geography.Ranges.Km3, new HashSet<uint>());
            //latDictionary[2][tWorks.Alfa.AlfaCommons.Geo.Geography.Ranges.Km3].Add(1);
            //latDictionary[2][tWorks.Alfa.AlfaCommons.Geo.Geography.Ranges.Km3].Add(2);
            //latDictionary[2][tWorks.Alfa.AlfaCommons.Geo.Geography.Ranges.Km3].Add(3);
            //latDictionary[2][tWorks.Alfa.AlfaCommons.Geo.Geography.Ranges.Km3].Add(4);
            //latDictionary[2][tWorks.Alfa.AlfaCommons.Geo.Geography.Ranges.Km3].Add(5);

            //string json = JsonConvert.SerializeObject(latDictionary, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });

            string json = File.ReadAllText("TextFile1.txt");
            ormLiteDbHandler.Test2(json, typeof(Dictionary<ushort, Dictionary<tWorks.Alfa.AlfaCommons.Geo.Geography.Ranges, HashSet<uint>>>));
            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ormLiteDbHandler.Test3();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            VeolcityDbHandler dbHandler = new VeolcityDbHandler();
            radioButton4.Checked = true;
            CreateCustomers();
            ulong id = dbHandler.Insert(customers[0]);
            Console.WriteLine($"Velo: {id}");
        }
    }
}
