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

        public bool ItemDisplayOnHover { get; set; }

        public int MinimumValue
        {
            get
            {
                return minimumValue;
            }

            set
            {
                if (value > 0 && value < maximumValue)
                {
                    minimumValue = value;
                    Invalidate();
                }
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
                if (value > 0 && value > minimumValue)
                {
                    maximumValue = value;
                    Invalidate();
                }
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
                if (value > 0)
                {
                    legendInterval = value;
                    Invalidate();
                }
            }
        }

        public int SliderCount { get; }

        public TooltipDisplayMode TooltipDisplayMode { get; set; }

        public Color SliderTransparentColor
        {
            get => sliderTransparentColor;
            set
            {
                sliderTransparentColor = value;

                selectedToolTip.SliderValueChanged -= OnSliderValueChanged;
                selectedToolTip = new SliderValueToolTip(value);
                selectedToolTip.IsCurrentSelectedToolTip = true;
                selectedToolTip.SliderValueChanged += OnSliderValueChanged;
            }
        }

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

        private bool isDragging, isInputEntering, isHoverToolTipShown;
        private int minimumValue = 0, maximumValue = 100, legendInterval = 1, sliderSize = 36, penWidth = 7;
        private int selectedSliderMinPos = 0, selectedSliderMaxPos = 0;
        private double selectedSliderMinValue = 0, selectedSliderMaxValue = 0;
        private string selectedSlider, hoverSlider;
        private Color lineColor = Color.AliceBlue, sliderColor = Color.DeepSkyBlue, sliderTransparentColor;
        private Point offSetPoint;
        private List<KeyValuePair<string, double>> itemCollection;
        private Dictionary<string, Rectangle> sliderCollection;
        private Dictionary<string, Color> colorCollection;
        private Dictionary<string, SliderValueToolTip> toolTipCollection;
        private SliderValueToolTip selectedToolTip, hoverToolTip;

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
            if (color != null)
                colorCollection.Add(itemName, color);

            //AddToolTip(itemName, color);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            SolidBrush brush = new SolidBrush(lineColor);
            Pen pen = new Pen(GetDarkerColor(lineColor), penWidth);
            Rectangle leftArc = new Rectangle(Padding.Left, Height * 3 / 20, Height / 5, Height / 5);
            Rectangle rightArc = new Rectangle(Width - Height / 10 - Padding.Right - penWidth, Height * 3 / 20, Height / 5 - 1, Height / 5);

            GraphicsPath path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(leftArc, 90, 180);
            path.AddArc(rightArc, 270, 180);
            path.CloseAllFigures();

            e.Graphics.FillPath(brush, path);
            SliderLinePaint(e);
            e.Graphics.DrawPath(pen, path);
            MilestonePaint(e);
            SliderButtonPaint(e);
            LegendPaint(e);

            brush?.Dispose();
            path?.Dispose();
            pen?.Dispose();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            foreach (var Iter in itemCollection)
            {
                sliderCollection[Iter.Key] = UpdateSliderPosition((int)Iter.Value);
                Invalidate();
            }

            Height = 100;
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
                    int toalWidth = Width - Padding.Right - Padding.Left;

                    selectedSliderMinValue = GetMinAndMaxValueForPosition(pos).Key;
                    selectedSliderMaxValue = GetMinAndMaxValueForPosition(pos + 1).Value;

                    double percentage = (selectedSliderMinValue - MinimumValue) * 100 / (MaximumValue - MinimumValue);
                    Point location = new Point((int)(percentage * toalWidth / 100) - 10, (Height - 20) / 2);
                    selectedSliderMinPos = location.X;

                    percentage = (selectedSliderMaxValue - MinimumValue) * 100 / (MaximumValue - MinimumValue);
                    location = new Point((int)(Math.Round(percentage * toalWidth / 100) - 10), (int)(Math.Round((Height - 20) / 2.0)));
                    selectedSliderMaxPos = location.X;

                    ShowOtherToolTips();

                    Point toolTipLocation = PointToScreen(slider.Location);
                    selectedToolTip.SliderToolTipBackColor = lineColor;
                    selectedToolTip.Font = Font;
                    selectedToolTip.ForeColor = colorCollection[itemCollection[ctr].Key];
                    selectedToolTip.SliderName = itemCollection[ctr].Key;
                    selectedToolTip.Value = (int)itemCollection[ctr].Value;
                    selectedToolTip.Location = new Point(toolTipLocation.X - (selectedToolTip.Width - slider.Width) / 2, toolTipLocation.Y - 60);
                    selectedToolTip.Show();
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
            CloseOtherToolTips();
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
                    isInputEntering = selectedToolTip.IsInputEntry = false;
                    selectedToolTip.Value = (int)itemCollection[pos].Value;
                    selectedToolTip.Location = new Point(toolTipLocation.X - (selectedToolTip.Width - rect.Width) / 2, toolTipLocation.Y - 60);
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
                    hoverSlider = Iter.Key;

                    if (ItemDisplayOnHover && !isDragging)
                    {

                        if (!isHoverToolTipShown)
                        {
                            hoverToolTip = new SliderValueToolTip(sliderTransparentColor)
                            {
                                SliderToolTipBackColor = lineColor,
                                ForeColor = colorCollection[Iter.Key],
                                IsUpsideDown = false,
                                Font = Font,
                                ToolTipDisplayType = DisplayType.Name,
                                IsCurrentSelectedToolTip = true,
                                SliderName = Iter.Key
                            };

                            Point toolTipLocation = PointToScreen(Iter.Value.Location);
                            hoverToolTip.Location = new Point(toolTipLocation.X - (hoverToolTip.Width - Iter.Value.Width) / 2, toolTipLocation.Y - 60);
                            isHoverToolTipShown = true;
                            hoverToolTip.Show();
                        }
                    }

                    isHovering = true;
                }
            }


            if (!isHovering)
            {
                hoverSlider = "";
                Cursor = Cursors.Default;
                isHoverToolTipShown = false;

                if (hoverToolTip != null)
                    hoverToolTip.Close();
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
                    selectedToolTip.IsInputEntry = true;
                    isInputEntering = true;
                    break;
                }
            }
        }

        private void OnSliderValueChanged(string name, int value)
        {
            int pos = itemCollection.FindIndex(item => item.Key == name);
            int minValue = (int)GetMinAndMaxValueForPosition(pos).Key;
            int maxValue = (int)GetMinAndMaxValueForPosition(pos + 1).Value;

            if (minValue < value && value < maxValue)
            {
                itemCollection[pos] = new KeyValuePair<string, double>(name, value);
                sliderCollection[name] = UpdateSliderPosition(value);
                Invalidate();
            }
        }

        private Color GetDarkerColor(Color color)
        {
            int r = color.R - 50 < 0 ? 0 : color.R - 50;
            int g = color.G - 50 < 0 ? 0 : color.G - 50;
            int b = color.B - 50 < 0 ? 0 : color.B - 50;

            return Color.FromArgb(r, g, b);
        }

        private void SliderLinePaint(PaintEventArgs e)
        {
            int sliderBarPosX = Padding.Left, sliderBarPosY = Height * 3 / 20;
            Brush brush;

            for (int ctr = 0; ctr < itemCollection.Count + 1; ctr++)
            {
                Rectangle leftArc = new Rectangle(sliderBarPosX, sliderBarPosY, Height / 5, Height / 5);
                Rectangle rightArc;

                if (ctr == itemCollection.Count)
                    rightArc = new Rectangle(Width - Height / 10 - Padding.Right - penWidth, sliderBarPosY, Height / 5, Height / 5);
                else
                    rightArc = new Rectangle(sliderCollection[itemCollection[ctr].Key].X + sliderSize / 2, sliderBarPosY, Height / 5, Height / 5);

                GraphicsPath path = new GraphicsPath();
                path.StartFigure();
                path.AddArc(leftArc, 90, 180);
                path.AddArc(rightArc, 270, 180);
                path.CloseAllFigures();

                brush = ctr < itemCollection.Count && colorCollection.ContainsKey(itemCollection[ctr].Key) ? new SolidBrush(colorCollection[itemCollection[ctr].Key]) : new SolidBrush(lineColor);

                e.Graphics.FillPath(brush, path);
                brush?.Dispose();

                if (ctr < itemCollection.Count)
                    sliderBarPosX = sliderCollection[itemCollection[ctr].Key].X + sliderSize / 2;
            }
        }

        private void SliderButtonPaint(PaintEventArgs e)
        {
            SolidBrush outerEllipseBrush = new SolidBrush(sliderColor);
            SolidBrush innerEllipseBrush = new SolidBrush(lineColor);
            StringFormat sFormat = new StringFormat();
            sFormat.LineAlignment = sFormat.Alignment = StringAlignment.Center;

            foreach (var Iter in itemCollection)
            {
                SolidBrush brush = new SolidBrush(colorCollection[Iter.Key]);
                var slider = sliderCollection[Iter.Key];
                e.Graphics.FillEllipse(brush, slider.X, slider.Y, slider.Width, slider.Height);

                if (hoverSlider == Iter.Key)
                    e.Graphics.FillEllipse(innerEllipseBrush, slider.X + 5, slider.Y + 5, slider.Width - 10, slider.Height - 10);
                else
                    e.Graphics.FillEllipse(innerEllipseBrush, slider.X + 3, slider.Y + 3, slider.Width - 6, slider.Height - 6);

                Font font = new Font(Font.FontFamily, GetFittingFontSize(e.Graphics, ((int)Iter.Value).ToString(), new Font(Font.FontFamily, 20, Font.Style), new Rectangle(slider.X + 5, slider.Y + 5, slider.Width - 10, slider.Height - 10)));
                e.Graphics.DrawString(((int)Iter.Value).ToString(), font, brush, slider, sFormat);
                brush.Dispose();

            }
            innerEllipseBrush?.Dispose();
            outerEllipseBrush?.Dispose();
        }

        private void MilestonePaint(PaintEventArgs e)
        {
            Pen pen = new Pen(GetDarkerColor(lineColor), 1.75f);
            SolidBrush brush = new SolidBrush(GetDarkerColor(lineColor));

            float size = Height / 10, linePosY = Height * 15 / 20, stepInterval = MaximumValue / (float)(legendInterval + 1);
            int xPos = 0, ellipseYPos, stringYPos, totalWidth = Width - Padding.Left - Padding.Right;

            ellipseYPos = Height * 7 / 10;
            stringYPos = Height * 11 / 20;


            for (int ctr = 0; ctr < legendInterval + 2; ctr++)
            {
                xPos = (int)Math.Round(totalWidth * ctr / (double)(legendInterval + 1));
                xPos = xPos + Padding.Left;
                string measureString = ((int)Math.Round(ctr * stepInterval)).ToString();
                float width = e.Graphics.MeasureString(measureString, base.Font).Width;

                if (ctr == 0)
                {
                    e.Graphics.DrawString(measureString, base.Font, brush, Padding.Left, stringYPos);
                }
                else if (ctr == legendInterval + 1)
                {
                    e.Graphics.DrawString(measureString, base.Font, brush, Width - width - Padding.Right, stringYPos);
                }
                else
                {
                    e.Graphics.DrawLine(pen, xPos, Height * 9 / 20, xPos, Height * 11 / 20);
                    xPos = xPos - (int)(size / 2);
                    e.Graphics.DrawString(measureString, base.Font, brush, xPos - (width / 2), stringYPos);
                }
            }

            brush?.Dispose();
            pen?.Dispose();
        }

        private void LegendPaint(PaintEventArgs e)
        {
            int circleYPos = Height * 15 / 20, xPos = Padding.Left, totalWidth = Width - Padding.Left - Padding.Right, lineYPos = circleYPos + Height / 10 / 2;
            int totalCircleWidth = (itemCollection.Count + 1) * Height / 10, totalLineWidth = totalWidth - totalCircleWidth;
            float singleLineWidth = totalLineWidth / (float)itemCollection.Count;

            StringFormat sFormat = new StringFormat();
            sFormat.Alignment = sFormat.LineAlignment = StringAlignment.Center;

            Brush brush;
            Pen pen;
            for (int ctr = 0; ctr < itemCollection.Count; ctr++)
            {
                pen = new Pen(colorCollection[itemCollection[ctr].Key]);
                brush = new SolidBrush(colorCollection[itemCollection[ctr].Key]);

                e.Graphics.DrawEllipse(pen, xPos, circleYPos, Height / 10, Height / 10);
                e.Graphics.DrawLine(pen, xPos + Height / 10, lineYPos, xPos + Height / 10 + singleLineWidth, lineYPos);
                Console.WriteLine(Height / 10 + (int)Math.Round(singleLineWidth));
                Rectangle rectangle = new Rectangle(xPos, circleYPos, Height / 10 + (int)Math.Round(singleLineWidth), Height - circleYPos + Height / 10);

                e.Graphics.DrawString(itemCollection[ctr].Key, Font, brush, rectangle, sFormat);

                pen?.Dispose();
                brush?.Dispose();

                xPos = xPos + Height / 10 + (int)Math.Round(singleLineWidth);
            }
            pen = new Pen(colorCollection[itemCollection[itemCollection.Count - 1].Key]);
            e.Graphics.DrawEllipse(pen, xPos, circleYPos, Height / 10, Height / 10);
        }

        private void AddControl(string name, double value, int pos)
        {
            sliderCollection.Add(name, UpdateSliderPosition((int)value));
            Invalidate();
        }

        private void InitializeMultiSlider()
        {
            DoubleBuffered = true;
            itemCollection = new List<KeyValuePair<string, double>>();
            sliderCollection = new Dictionary<string, Rectangle>();
            colorCollection = new Dictionary<string, Color>();

            if (selectedToolTip != null)
                selectedToolTip.SliderValueChanged -= OnSliderValueChanged;

            selectedToolTip = new SliderValueToolTip(SliderTransparentColor)
            {
                SliderToolTipBackColor = lineColor,
                ForeColor = sliderColor,
                IsUpsideDown = true,
                IsCurrentSelectedToolTip = true,
                ToolTipDisplayType = DisplayType.NameAndValue
            };
            selectedToolTip.SliderValueChanged += OnSliderValueChanged;
        }

        private int UpdateSelectedSliderValue(int xPos, int width)
        {
            double totalWidth = Width - Padding.Left - Padding.Right;
            double percentage = (xPos + (width / (double)2) - Padding.Left) * 100 / totalWidth;
            double value = Math.Round((MinimumValue + MaximumValue) * percentage / (double)100);

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

        private Rectangle UpdateSliderPosition(int value)
        {
            int totalWidth = Width - Padding.Left - Padding.Right, startPosY;
            double percentage = (value - MinimumValue) * 100 / (double)(MaximumValue - MinimumValue);
            startPosY = Height * 4 / 20;
            Point location = new Point((int)(percentage * totalWidth / 100) - sliderSize / 2 + Padding.Left, (startPosY - (sliderSize - Height / 10) / 2));
            return new Rectangle(location, new Size(sliderSize, sliderSize));
        }

        public float GetFittingFontSize(Graphics g, string text, Font font, Rectangle rect)
        {
            float maxFontSize = font.Size;
            float minFontSize = 1f;

            while (maxFontSize - minFontSize > 0.5f)
            {
                float testSize = (maxFontSize + minFontSize) / 2;
                using (Font testFont = new Font(font.FontFamily, testSize, font.Style))
                {
                    SizeF textSize = g.MeasureString(text, testFont);

                    if (textSize.Width <= rect.Width && textSize.Height <= rect.Height)
                    {
                        minFontSize = testSize;
                    }
                    else
                    {
                        maxFontSize = testSize;
                    }
                }
            }
            return minFontSize;
        }

        private void ShowOtherToolTips()
        {
            int currentItemIdx = itemCollection.FindIndex(item => item.Key == selectedSlider), flagIdx = 0;
            toolTipCollection = new Dictionary<string, SliderValueToolTip>();
            SliderValueToolTip otherToolTip;
            for (int ctr = currentItemIdx - 1; ctr >= 0; ctr--)
            {
                otherToolTip = new SliderValueToolTip(sliderTransparentColor)
                {
                    SliderToolTipBackColor = lineColor,
                    ForeColor = colorCollection[itemCollection[ctr].Key],
                    Font = Font,
                    SliderName = itemCollection[ctr].Key,
                    Value = (int)itemCollection[ctr].Value,
                    ToolTipDisplayType = DisplayType.Name
                };

                otherToolTip.IsUpsideDown = TooltipDisplayMode == TooltipDisplayMode.Alternate && flagIdx % 2 == 0 ? true : false;

                Rectangle slider = sliderCollection[itemCollection[ctr].Key];
                Point toolTipLocation = PointToScreen(slider.Location);

                otherToolTip.Location = otherToolTip.IsUpsideDown ? new Point(toolTipLocation.X + slider.Width / 2 - otherToolTip.Width / 2, toolTipLocation.Y + 30) : new Point(toolTipLocation.X + slider.Width / 2 - otherToolTip.Width / 2, toolTipLocation.Y - 60);

                otherToolTip.Show();
                toolTipCollection.Add(itemCollection[ctr].Key, otherToolTip);
                flagIdx++;
            }

            flagIdx = 0;

            for (int ctr = currentItemIdx + 1; ctr < itemCollection.Count; ctr++)
            {
                otherToolTip = new SliderValueToolTip(sliderTransparentColor)
                {
                    SliderToolTipBackColor = lineColor,
                    ForeColor = colorCollection[itemCollection[ctr].Key],
                    Font = Font,
                    SliderName = itemCollection[ctr].Key,
                    Value = (int)itemCollection[ctr].Value,
                    ToolTipDisplayType = DisplayType.Name
                };

                otherToolTip.IsUpsideDown = TooltipDisplayMode == TooltipDisplayMode.Alternate && flagIdx % 2 == 0 ? true : false;

                Rectangle slider = sliderCollection[itemCollection[ctr].Key];
                Point toolTipLocation = PointToScreen(slider.Location);

                otherToolTip.Location = otherToolTip.IsUpsideDown ? new Point(toolTipLocation.X + slider.Width / 2 - otherToolTip.Width / 2, toolTipLocation.Y + 30) : new Point(toolTipLocation.X + slider.Width / 2 - otherToolTip.Width / 2, toolTipLocation.Y - 60);
                otherToolTip.Show();
                toolTipCollection.Add(itemCollection[ctr].Key, otherToolTip);
                flagIdx++;
            }
        }

        private void CloseOtherToolTips()
        {
            if (toolTipCollection == null)
                return;

            for (int ctr = 0; ctr < itemCollection.Count; ctr++)
            {
                if (toolTipCollection.ContainsKey(itemCollection[ctr].Key))
                {
                    toolTipCollection[itemCollection[ctr].Key].Close();
                    toolTipCollection.Remove(itemCollection[ctr].Key);
                }
            }
            toolTipCollection.Clear();
        }
    }

    public enum TooltipDisplayMode
    {
        Down,
        Alternate
    }
}
