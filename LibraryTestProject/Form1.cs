﻿using System;
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
            InitializeDB();

            InitializeComponent();
            multiSlider1.ForeColor = Color.DeepSkyBlue;
            multiSlider1.BackColor = Color.AliceBlue;
            multiSlider1.SliderTransparentColor = BackColor;
            multiSlider1.Font = Font;
            multiSlider1.AddItem("Small Defect Size", Color.Red, 0, 0);
            multiSlider1.AddItem("Medium Defect Size", Color.Red, 1, 200);
            multiSlider1.AddItem("Large Defect Size", Color.Red, 2, 350);
            multiSlider1.AddItem("Huge Defect Size", Color.Red, 3, 600);

            UpdatePrinterStatus();
        }

        public void InitializeDB()
        {
            //DatabaseManager manager = new MySqlHandler(DatabaseConfig.Host, DatabaseConfig.User, DatabaseConfig.Password, DatabaseConfig.StaticDatabase)
            //{
            //    HostName = DatabaseConfig.Host,
            //    UserName = DatabaseConfig.User,
            //    Password = DatabaseConfig.Password,
            //    Database = DatabaseConfig.StaticDatabase,
            //    StorageEngine = DatabaseConfig.StorageEngine
            //};
            //manager
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
    }
}
