using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PowerControlsLibrary.MultiSlider
{
    public partial class SliderValueToolTip : Form
    {
        public delegate void SliderValueHandler(string name, int value);
        public event SliderValueHandler SliderValueChanged;

        public SliderValueToolTip(Color transparencyColor)
        {
            InitializeComponent();
            blinkTimer = new Timer();
            blinkTimer.Tick += OnTextInputBlink;
            blinkTimer.Interval = 750;
            ForeColor = Color.Red;
            DoubleBuffered = true;
            BackColor = TransparencyKey = transparencyColor;
        }

        public bool IsCurrentSelectedToolTip { get; set; }

        public string SliderName
        {
            get
            {
                return sliderName;
            }

            set
            {
                sliderName = value;
                int width = (int)CreateGraphics().MeasureString(value, Font).Width;
                if (valueWidth < width + 20)
                {
                    nameWidth = Width = width + 20;
                }
                Invalidate();
            }
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
                int width = (int)CreateGraphics().MeasureString(value.ToString(), Font).Width;
                if (nameWidth < width + 20)
                {
                    valueWidth = Width = width + 20;
                }
                Invalidate();
            }
        }

        public bool IsInputEntry
        {
            get => isInputEntry;
            set
            {
                isInputEntry = value;
                if (value)
                {
                    inputText = 0;
                    blinkTimer.Start();
                    Invalidate();
                }
            }

        }

        public Color SliderToolTipBackColor
        {
            get
            {
                return sliderToolTipBackColor;
            }

            set
            {
                sliderToolTipBackColor = value;
                Invalidate();
            }
        }

        public bool IsUpsideDown { get; set; } = false;

        private bool isInputEntry, isBlinking;
        private int value, radius = 20, valueWidth, nameWidth, inputText;
        private string sliderName;
        private Matrix lastGraphicsMatrix;
        private Color sliderToolTipBackColor;
        private Timer blinkTimer;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (IsUpsideDown)
            {
                e.Graphics.TranslateTransform(0, Height);
                e.Graphics.ScaleTransform(1, -1);
            }
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

            StringFormat sFormat = new StringFormat();
            sFormat.Alignment = StringAlignment.Center;
            sFormat.LineAlignment = StringAlignment.Center;

            Pen pen = new Pen(ForeColor, 3);
            SolidBrush foregroundBrush = new SolidBrush(ForeColor);
            SolidBrush backgroundBrush = new SolidBrush(SliderToolTipBackColor);
            int height = Height * 4 / 5, posX = 0, posY = 0;

            GraphicsPath graphPath = new GraphicsPath();
            graphPath.AddArc(new Rectangle(1, 2, radius, radius), 180, 90);
            graphPath.AddArc(new Rectangle(Width - radius, 2, radius - 2, radius), 270, 90);
            graphPath.AddArc(new Rectangle(Width - radius, height - radius, radius - 2, radius - 2), 0, 90);

            graphPath.AddLine(Width - radius, height - 2, Width * 3 / 5 + 2, height - 2);

            graphPath.AddLine(Width * 3 / 5, height, Width / 2, Height);
            graphPath.AddLine(Width / 2, Height, Width * 2 / 5, height);

            graphPath.AddLine(Width * 2 / 5 - 2, height - 2, radius + 2, height - 2);

            graphPath.AddArc(new Rectangle(1, height - radius, radius, radius - 2), 90, 90);
            graphPath.CloseFigure();
            e.Graphics.FillPath(backgroundBrush, graphPath);
            e.Graphics.DrawPath(pen, graphPath);

            Font headerFont = new Font(Font, FontStyle.Bold);
            if (IsUpsideDown)
            {
                e.Graphics.TranslateTransform(0, Height);
                e.Graphics.ScaleTransform(1, -1);
                posY = Height / 5;
            }
            e.Graphics.DrawString(sliderName, headerFont, foregroundBrush, new RectangleF(0, posY, Width, height / 2), sFormat);

            if (!IsInputEntry)
            {
                e.Graphics.DrawString(value.ToString(), Font, foregroundBrush, new RectangleF(0, posY + height / 2, Width, height / 2), sFormat);
            }
            else
                EnterInput(e.Graphics);

            sFormat?.Dispose();
            pen?.Dispose();
            foregroundBrush?.Dispose();
            backgroundBrush?.Dispose();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if ('0' <= e.KeyChar && e.KeyChar <= '9' && isInputEntry)
            {
                if (inputText * 10 + Convert.ToInt32(e.KeyChar) > 0 && inputText * 10 + Convert.ToInt32(e.KeyChar) > inputText)
                {
                    inputText = inputText * 10 + e.KeyChar - 48;
                    Invalidate();
                }
            }
            else if (e.KeyChar == '\b' && isInputEntry)
            {
                inputText = inputText / 10;
                Invalidate();
            }
            else if (e.KeyChar == '\r')
            {
                SliderValueChanged?.Invoke(sliderName, inputText);
                Hide();
            }
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);

            if (isInputEntry)
            {
                SliderValueChanged?.Invoke(sliderName, inputText);
            }

            blinkTimer.Stop();

            if (IsCurrentSelectedToolTip)
                Hide();
        }

        private void EnterInput(Graphics g)
        {
            int posY = 0;
            if (IsUpsideDown)
            {
                //g.Restore(lastGraphicsMatrix);
                posY = Height / 5;
            }
            Pen pen = new Pen(ForeColor, 2);
            SolidBrush brush = new SolidBrush(ForeColor);
            StringFormat sFormat = new StringFormat();
            sFormat.Alignment = StringAlignment.Center;
            sFormat.LineAlignment = StringAlignment.Center;
            int width = 0, height = Height * 4 / 5;

            if (inputText != 0)
            {
                width = (int)Math.Round(CreateGraphics().MeasureString(inputText.ToString(), Font).Width);
                g.DrawString(inputText.ToString(), Font, brush, new RectangleF(0, posY + height / 2, Width, height / 2), sFormat);
            }

            if (isBlinking)
            {
                int xPos = (Width + width) / 2;
                g.DrawLine(pen, xPos, posY + height / 2 + 3, xPos, posY + height - 6);
            }

            pen?.Dispose();
            brush?.Dispose();
        }

        private void OnTextInputBlink(object sender, EventArgs e)
        {
            isBlinking = !isBlinking;
            Invalidate();
        }
    }
}
