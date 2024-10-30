using System;
using GoLibrary;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DatabaseLibrary;


namespace LibraryTestProject
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            multiSlider1.AddItem("Small Defect Size", Color.Red, 0, 100);
            multiSlider1.AddItem("Medium Defect Size", Color.Green, 1, 300);
            multiSlider1.AddItem("Large Defect Size", Color.Blue, 2, 450);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        public static void UpdatePrinterStatus()
        {
            try
            {

                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer"))
                {
                    string defaultPrinterName = null;

                    foreach (var printer in searcher.Get())
                    {
                        defaultPrinterName = printer["Name"]?.ToString();
                        string filePath = $"{defaultPrinterName}.txt";
                        StreamWriter writer = new StreamWriter(filePath, false);

                        foreach (PropertyData property in printer.Properties)
                        {
                            //writer.WriteLine($"{property.Name}: {property.Value}");
                            Console.WriteLine($"{property.Name}: {property.Value}");
                        }

                        Console.WriteLine("------------------------------------------------------------------------------------");
                        //break; // Only need the first one
                    }

                    if (string.IsNullOrEmpty(defaultPrinterName))
                    {
                        return;
                    }

                    // Escape single quotes in printer name to avoid WMI query issues
                    string printerQuery = $"SELECT * FROM Win32_Printer WHERE Name='{defaultPrinterName.Replace("'", "''")}'";


                }
            }
            catch(Exception ex) { }

               
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //multiSliderControl1.Dispose();
        }
    }
}
