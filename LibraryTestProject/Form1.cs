using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibraryTestProject
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            multiSlider1.MinimumValue = 0;
            multiSlider1.MaximumValue = 1000;
            multiSlider1.AddItem("Small Defect Size", Color.Red, 0, 100);
            multiSlider1.AddItem("Small Defect Size1", Color.Blue);
            multiSlider1.AddItem("Small Defect Size2", Color.Green, 1, value: 300);
            multiSlider1.AddItem("Small Defect Size3", Color.Orange, 2, value: 500);
        }
    }
}
