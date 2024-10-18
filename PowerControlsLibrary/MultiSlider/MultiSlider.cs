using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PowerControlsLibrary.MultiSlider
{
    public class MultiSlider : Control
    {
        public MultiSlider()
        {
            InitializeMultiSlider();
        }

        public int MinimumValue
        {
            get
            {
                return minimumValue;
            }

            set
            {
                minimumValue = value;
                Invalidate();
            }
        }

        public int MaximumValue
        {
            get
            {
                return maximumValue;
            }

            set
            {
                maximumValue = value;
                Invalidate();
            }
        }

        public int LegendInterval
        {
            get
            {
                return legendInterval;
            }

            set
            {
                legendInterval = value;
                Invalidate();
            }
        }

        public int SliderCount { get; }

        public new Color BackColor
        {
            get => lineColor;
            set
            {
                lineColor = value;
                Invalidate();
            }
        }

        public new Color ForeColor
        {
            get => sliderColor;
            set
            {
                sliderColor = value;
                Invalidate();
            }
        }

        private bool isDragging;
        private int minimumValue = 0, maximumValue = 0, legendInterval = 0;
        private int selectedSliderMinPos = 0, selectedSliderMaxPos = 0;
        private double selectedSliderMinValue = 0, selectedSliderMaxValue = 0;
        private string selectedSlider, hoverSlider;
        private Color lineColor = Color.AliceBlue, sliderColor = Color.DeepSkyBlue;
        private Point offSetPoint;
        private List<KeyValuePair<string, double>> itemCollection;
        private Dictionary<string, Rectangle> sliderCollection;
        private Dictionary<string, Color> colorCollection;
        private SliderValueToolTip toolTip = new SliderValueToolTip();

        public void AddItem(string itemName, Color color, int pos = -1, double value = -1)
        {
            if (pos == -1)
            {
                pos = itemCollection.Count;
                value = value == -1 ? GetMidValue(itemCollection.Count) : IsValidValueOrUpdate(itemCollection.Count, value);
                AddControl(itemName, value, pos);
                itemCollection.Add(new KeyValuePair<string, double>(itemName, value));
            }
            else
            {
                pos = itemCollection.Count < pos ? itemCollection.Count : pos;
                value = IsValidValueOrUpdate(pos, value);
                AddControl(itemName, value, pos);
                itemCollection.Insert(pos, new KeyValuePair<string, double>(itemName, value));
            }
            colorCollection.Add(itemName, color);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            SolidBrush brush = new SolidBrush(lineColor);
            Pen pen = new Pen(Color.LightBlue, 1);
            Rectangle leftArc = new Rectangle(0, Height * 3 / 10, Height / 10, Height / 10);
            Rectangle rightArc = new Rectangle(Width - Height / 10, Height * 3 / 10, Height / 10 - 1, Height / 10);
            GraphicsPath path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(leftArc, 90, 180);
            path.AddArc(rightArc, 270, 180);
            path.CloseAllFigures();

            e.Graphics.FillPath(brush, path);
            e.Graphics.DrawPath(pen, path);

            SliderPaint(e);
            LegendMarkPaint(e);


            brush.Dispose();
            path.Dispose();
            pen.Dispose();
        }

        private void SliderPaint(PaintEventArgs e)
        {
            SolidBrush sliderBrush = new SolidBrush(Color.LightGreen);

            foreach (var Iter in itemCollection)
            {
                var slider = sliderCollection[Iter.Key];
                Pen sliderBorder = new Pen(colorCollection[Iter.Key]);
                Pen sliderBorder1 = new Pen(colorCollection[Iter.Key], 2);
                e.Graphics.FillEllipse(sliderBrush, slider.X, slider.Y, slider.Width, slider.Height);

                if (hoverSlider == Iter.Key)
                    e.Graphics.DrawEllipse(sliderBorder1, slider.X, slider.Y, slider.Width, slider.Height);
                else
                    e.Graphics.DrawEllipse(sliderBorder, slider.X, slider.Y, slider.Width, slider.Height);

                sliderBorder.Dispose();
                sliderBorder1.Dispose();
            }
            sliderBrush.Dispose();
        }

        private void LegendMarkPaint(PaintEventArgs e)
        {
            Pen pen = new Pen(Color.DeepSkyBlue);
            int xPos = 0, stepLineWidth = (Width - (Height * (legendInterval + 2) / 10)) / (legendInterval + 1);
            for (int ctr = 0; ctr < legendInterval + 2; ctr++)
            {
                e.Graphics.DrawEllipse(pen, new Rectangle(xPos, Height * 7 / 10, Height / 10, Height / 10));

                if (ctr != legendInterval + 1)
                    e.Graphics.DrawLine(pen, xPos + Height / 10, Height * 15 / 20, xPos + Height / 10 + stepLineWidth, Height * 15 / 20);
                xPos = xPos + Height / 10 + stepLineWidth;
            }
            pen.Dispose();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Height = 75;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            for (int ctr = itemCollection.Count - 1; ctr >= 0; ctr--)
            {
                Rectangle slider = sliderCollection[itemCollection[ctr].Key];
                if (slider.Contains(e.X, e.Y))
                {
                    isDragging = true;
                    offSetPoint = new Point(e.Location.X - slider.X, e.Location.Y - slider.Y);
                    selectedSlider = itemCollection[ctr].Key;

                    int pos = itemCollection.FindIndex(item => item.Key == itemCollection[ctr].Key);

                    selectedSliderMinValue = GetMinAndMaxValueForPosition(pos).Key;
                    selectedSliderMaxValue = GetMinAndMaxValueForPosition(pos + 1).Value;

                    double percentage = (selectedSliderMinValue - MinimumValue) * 100 / (MaximumValue - MinimumValue);
                    Point location = new Point((int)(percentage * Width / 100) - 10, (Height - 20) / 2);
                    selectedSliderMinPos = location.X;

                    percentage = (selectedSliderMaxValue - MinimumValue) * 100 / (MaximumValue - MinimumValue);
                    location = new Point((int)(Math.Round(percentage * Width / 100) - 10), (int)(Math.Round((Height - 20) / 2.0)));
                    selectedSliderMaxPos = location.X;
                    Point toolTipLocation = PointToScreen(slider.Location);
                    toolTip.Width = 136;
                    toolTip.Location = new Point(toolTipLocation.X - (toolTip.Width - slider.Width) / 2, toolTipLocation.Y - 50);
                    toolTip.Show();
                    break;
                }
            }

        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            isDragging = false;
            selectedSlider = "";
            offSetPoint = new Point();
            toolTip.Hide();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (isDragging)
            {
                Rectangle rect = sliderCollection[selectedSlider];

                if (selectedSliderMinPos + 10 < (e.X - offSetPoint.X) && (e.X - offSetPoint.X) < selectedSliderMaxPos - 10)
                {
                    sliderCollection[selectedSlider] = new Rectangle(e.X - offSetPoint.X, rect.Y, rect.Width, rect.Height);
                    int pos = itemCollection.FindIndex(item => item.Key == selectedSlider);
                    itemCollection[pos] = new KeyValuePair<string, double>(selectedSlider, UpdateSelectedSliderValue(sliderCollection[selectedSlider].X, sliderCollection[selectedSlider].Width));
                    Point toolTipLocation = PointToScreen(sliderCollection[selectedSlider].Location);
                    toolTip.Width = 136;
                    toolTip.Location = new Point(toolTipLocation.X - (toolTip.Width - rect.Width) / 2, toolTipLocation.Y - 50);

                }

                Invalidate();
                return;
            }

            bool isHovering = false;
            foreach (var Iter in sliderCollection)
            {
                if (Iter.Value.Contains(e.X, e.Y))
                {
                    Cursor = Cursors.Hand;
                    isHovering = true;
                    hoverSlider = Iter.Key;
                }
            }

            if (!isHovering)
            {
                hoverSlider = "";
                Cursor = Cursors.Default;
            }
            Invalidate();
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnDoubleClick(e);
            for (int ctr = itemCollection.Count - 1; ctr >= 0; ctr--)
            {
                Rectangle slider = sliderCollection[itemCollection[ctr].Key];
                if (slider.Contains(e.X, e.Y))
                {
                    break;
                }
            }
        }

        private void AddControl(string name, double value, int pos)
        {
            double percentage = (value - MinimumValue) * 100 / (MaximumValue - MinimumValue);
            Point location = new Point((int)(percentage * Width / 100) - 10, Height * 3 / 10 - (Height / 10));
            sliderCollection.Add(name, new Rectangle(location, new Size(20, 20)));
            Invalidate();
        }

        private void InitializeMultiSlider()
        {
            DoubleBuffered = true;
            itemCollection = new List<KeyValuePair<string, double>>();
            sliderCollection = new Dictionary<string, Rectangle>();
            colorCollection = new Dictionary<string, Color>();
        }

        private int UpdateSelectedSliderValue(int xPos, int width)
        {
            double percentage = (xPos + (width / 2)) * 100 / Width;
            double value = (MinimumValue + MaximumValue) * percentage / 100;

            return (int)value;
        }

        private double GetMidValue(int pos)
        {
            var minMaxPairValue = GetMinAndMaxValueForPosition(pos);
            double minValue = minMaxPairValue.Key, maxValue = minMaxPairValue.Value;

            return (minValue + maxValue) / 2;
        }

        private double IsValidValueOrUpdate(int pos, double value)
        {
            var minMaxPairValue = GetMinAndMaxValueForPosition(pos);
            double minValue = minMaxPairValue.Key, maxValue = minMaxPairValue.Value;

            return minValue < value && value < maxValue ? value : minValue + 1;
        }

        private KeyValuePair<double, double> GetMinAndMaxValueForPosition(int pos)
        {
            double minValue, maxValue;
            if (pos == 0)
            {
                minValue = MinimumValue;
                maxValue = itemCollection.Count == 0 ? MaximumValue : itemCollection[pos].Value;
            }
            else if (pos == itemCollection.Count)
            {
                minValue = itemCollection[pos - 1].Value;
                maxValue = MaximumValue;
            }
            else
            {
                minValue = itemCollection[pos - 1].Value;
                maxValue = itemCollection[pos].Value;
            }

            return new KeyValuePair<double, double>(minValue, maxValue);
        }

    }
}
