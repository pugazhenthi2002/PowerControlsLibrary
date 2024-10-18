using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PowerControlsLibrary.MultiSlider
{
    public partial class SliderValueToolTip : Form
    {
        public SliderValueToolTip()
        {
            InitializeComponent();
        }

        public int Value
        {
            get
            {
                return value;
            }

            set
            {
                this.value = value;
                Invalidate();
            }
        }

        public new Color ForeColor { get; set; }

        private int value;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            StringFormat sFormat = new StringFormat();
            sFormat.Alignment = StringAlignment.Center;
            sFormat.LineAlignment = StringAlignment.Center;

            SolidBrush brush = new SolidBrush(ForeColor);
            e.Graphics.DrawString(value.ToString(), Font, brush, new RectangleF(0, 0, Width, Height), sFormat);

            sFormat.Dispose();
            brush.Dispose();
        }
    }
}
