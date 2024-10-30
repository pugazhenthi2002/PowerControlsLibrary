using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PowerControlsLibrary.MultiSlider
{
    public class MultiSlider : UserControl
    {
        public delegate void SliderHandler(string name, int value);
        public event SliderHandler SliderValueUpdated;

        public MultiSlider()
        {
            InitializeMultiSlider();
        }

        #region Properties

        public bool ItemDisplayOnHover { get; set; }

        public int MinimumValue
        {
            get
            {
                return minimumValue;
            }

            set
            {
                if (value >= 0 && value < maximumValue)
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
                selectedToolTip = new SliderToolTip(value);
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

        #endregion

        #region Fields

        private bool isDragging, isInputEntering, isHoverToolTipShown, isOtherToolTipShown, isDelayOver;
        private int minimumValue = 0, maximumValue = 100, legendInterval = 1, sliderSize = 36, penWidth = 7;
        private int selectedSliderMinPos = 0, selectedSliderMaxPos = 0;
        private int selectedSliderMinValue = 0, selectedSliderMaxValue = 0;
        private string selectedSlider, hoverSlider;
        private Color lineColor = Color.AliceBlue, sliderColor = Color.DeepSkyBlue, sliderTransparentColor;
        private Point offSetPoint;
        private List<KeyValuePair<string, int>> itemCollection;
        private Dictionary<string, Rectangle> sliderCollection;
        private Dictionary<string, Color> colorCollection;
        private Dictionary<string, SliderToolTip> toolTipCollection;
        private SliderToolTip selectedToolTip, hoverToolTip;
        private Timer doubleClickDelayTimer;

        #endregion

        public bool AddItem(string itemName, Color color, int pos = -1, int value = -1)
        {
            if (itemCollection.FindIndex(item => item.Key == itemName) != -1)
            {
                return false;
            }

            if (value < MinimumValue || value > MaximumValue)
                return false;

            if (pos == -1)
            {
                pos = itemCollection.Count;
                value = value == -1 ? GetMidValue(itemCollection.Count) : IsValidValueOrUpdate(itemCollection.Count, value);

                if (value == -1)
                    return false;

                AddSlider(itemName, value, pos);
                itemCollection.Add(new KeyValuePair<string, int>(itemName, value));
            }
            else
            {
                pos = itemCollection.Count < pos ? itemCollection.Count : pos;
                value = IsValidValueOrUpdate(pos, value);

                if (value == -1)
                    return false;

                AddSlider(itemName, value, pos);
                itemCollection.Insert(pos, new KeyValuePair<string, int>(itemName, value));
            }
            if (color != null)
                colorCollection.Add(itemName, color);

            Invalidate();
            return true;
        }

        public new void Dispose()
        {
            itemCollection?.Clear();
            sliderCollection?.Clear();
            colorCollection?.Clear();

            if (toolTipCollection != null)
            {
                foreach (var Iter in toolTipCollection)
                {
                    Iter.Value.Close();
                }
            }

            selectedToolTip?.Dispose();
            hoverToolTip?.Dispose();
            doubleClickDelayTimer?.Stop();
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

        #region Drag Drop Mechanism

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            var leftItemCollection = new List<KeyValuePair<string, double>>();
            var rightItemCollection = new List<KeyValuePair<string, double>>();

            foreach (var Iter in itemCollection)
            {
                if (Iter.Value <= (MinimumValue + MaximumValue) / 2)
                {
                    leftItemCollection.Add(new KeyValuePair<string, double>(Iter.Key, Iter.Value));
                }
                else
                {
                    rightItemCollection.Add(new KeyValuePair<string, double>(Iter.Key, Iter.Value));
                }
            }

            leftItemCollection.Sort((item1, item2) => item2.Value.CompareTo(item1.Value));
            rightItemCollection.Sort((item1, item2) => item1.Value.CompareTo(item2.Value));

            foreach (var Iter in leftItemCollection)
            {
                Rectangle slider = sliderCollection[Iter.Key];
                if (slider.Contains(e.X, e.Y))
                {
                    SelectSliderOnMouseDown(e, slider, Iter.Key, (int)Iter.Value);
                    break;
                }
            }

            foreach (var Iter in rightItemCollection)
            {
                Rectangle slider = sliderCollection[Iter.Key];
                if (slider.Contains(e.X, e.Y))
                {
                    SelectSliderOnMouseDown(e, slider, Iter.Key, (int)Iter.Value);
                    break;
                }
            }

            leftItemCollection.Clear();
            rightItemCollection.Clear();

        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            isDragging = false;
            selectedSlider = "";
            offSetPoint = new Point();
            CloseOtherToolTips();
            isOtherToolTipShown = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (isDragging)
            {
                Rectangle rect = sliderCollection[selectedSlider];

                if (selectedSliderMinPos < (e.X - offSetPoint.X) && (e.X - offSetPoint.X) < selectedSliderMaxPos - sliderSize)
                {
                    sliderCollection[selectedSlider] = new Rectangle(e.X - offSetPoint.X, rect.Y, rect.Width, rect.Height);
                    int pos = itemCollection.FindIndex(item => item.Key == selectedSlider);
                    itemCollection[pos] = new KeyValuePair<string, int>(selectedSlider, UpdateSelectedSliderValue(sliderCollection[selectedSlider].X, sliderCollection[selectedSlider].Width));

                    Point toolTipLocation = PointToScreen(sliderCollection[selectedSlider].Location);
                    isInputEntering = selectedToolTip.IsInputEntry = false;
                    selectedToolTip.Value = (int)itemCollection[pos].Value;
                    selectedToolTip.Location = new Point(toolTipLocation.X - (selectedToolTip.Width - rect.Width) / 2, toolTipLocation.Y - 60);

                    if (!isOtherToolTipShown && isDelayOver)
                    {
                        ShowOtherToolTips();
                        isOtherToolTipShown = true;
                    }

                    if (isDelayOver)
                        selectedToolTip.Show();

                    SliderValueUpdated?.Invoke(selectedSlider, (int)itemCollection[pos].Value);
                }

                Invalidate();
                return;
            }

            var sliderCollectionCopy = GetSliderCollectionCopy();

            bool isHovering = false;
            foreach (var Iter in sliderCollectionCopy)
            {
                if (Iter.Value.Contains(e.X, e.Y))
                {
                    Cursor = Cursors.Hand;
                    hoverSlider = Iter.Key;

                    if (ItemDisplayOnHover && !isDragging)
                    {

                        if (!isHoverToolTipShown)
                        {
                            hoverToolTip = new SliderToolTip(sliderTransparentColor);

                            SetToolTipValue(hoverToolTip, Iter.Key, 0, lineColor, DisplayType.Name, true, false, false);

                            isHoverToolTipShown = true;
                            hoverToolTip.Show();
                        }
                    }

                    isHovering = true;
                }
            }

            sliderCollectionCopy.Clear();


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

        #endregion

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnDoubleClick(e);
            for (int ctr = itemCollection.Count - 1; ctr >= 0; ctr--)
            {
                Rectangle slider = sliderCollection[itemCollection[ctr].Key];
                if (slider.Contains(e.X, e.Y))
                {
                    SetToolTipValue(selectedToolTip, itemCollection[ctr].Key, itemCollection[ctr].Value, lineColor, DisplayType.NameAndValue, true, false, true);
                    selectedToolTip.Show();

                    return;
                }
            }
            if (itemCollection.Count > 1)
            {
                itemCollection.Sort((item1, item2) => item1.Value.CompareTo(item2.Value));
                Rectangle sliderRec = sliderCollection[itemCollection[0].Key];
                Rectangle rec = new Rectangle(Padding.Left, sliderRec.Y, sliderRec.X, sliderRec.Height);

                if (rec.Contains(e.X, e.Y))
                {
                    SetToolTipValue(selectedToolTip, itemCollection[0].Key, itemCollection[0].Value, lineColor, DisplayType.NameAndValue, true, false, true);
                    isInputEntering = true;

                    selectedToolTip.Show();
                }
            }
        }

        #region Paint

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
            SliderAllButtonPaint(e);
            LegendPaint(e);

            brush?.Dispose();
            path?.Dispose();
            pen?.Dispose();
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

                if (ctr < itemCollection.Count)
                    sliderBarPosX = sliderCollection[itemCollection[ctr].Key].X + sliderSize / 2;

                path?.Dispose();
                brush?.Dispose();
            }
        }

        private void SliderAllButtonPaint(PaintEventArgs e)
        {
            var leftItemCollection = new List<KeyValuePair<string, int>>();
            var rightItemCollection = new List<KeyValuePair<string, int>>();

            foreach (var Iter in itemCollection)
            {
                if (Iter.Value <= (MinimumValue + MaximumValue) / 2)
                {
                    leftItemCollection.Add(new KeyValuePair<string, int>(Iter.Key, Iter.Value));
                }
                else
                {
                    rightItemCollection.Add(new KeyValuePair<string, int>(Iter.Key, Iter.Value));
                }
            }

            leftItemCollection.Sort((item1, item2) => item1.Value.CompareTo(item2.Value));
            rightItemCollection.Sort((item1, item2) => item2.Value.CompareTo(item1.Value));

            foreach (var Iter in leftItemCollection)
            {
                SliderButtonPaint(e, colorCollection[Iter.Key], Iter.Key, Iter.Value);
            }

            foreach (var Iter in rightItemCollection)
            {
                SliderButtonPaint(e, colorCollection[Iter.Key], Iter.Key, Iter.Value);
            }

            leftItemCollection.Clear();
            rightItemCollection.Clear();
        }

        private void SliderButtonPaint(PaintEventArgs e, Color color, string name, int value)
        {
            SolidBrush brush = new SolidBrush(color);
            SolidBrush innerEllipseBrush = new SolidBrush(lineColor);
            StringFormat sFormat = new StringFormat();
            sFormat.LineAlignment = sFormat.Alignment = StringAlignment.Center;

            var slider = sliderCollection[name];
            e.Graphics.FillEllipse(brush, slider.X, slider.Y, slider.Width, slider.Height);

            if (hoverSlider == name)
                e.Graphics.FillEllipse(innerEllipseBrush, slider.X + 5, slider.Y + 5, slider.Width - 10, slider.Height - 10);
            else
                e.Graphics.FillEllipse(innerEllipseBrush, slider.X + 3, slider.Y + 3, slider.Width - 6, slider.Height - 6);

            Font font = new Font(Font.FontFamily, GetFittingFontSize(e.Graphics, value.ToString(), new Font(Font.FontFamily, 20, Font.Style), new Rectangle(slider.X + 5, slider.Y + 5, slider.Width - 10, slider.Height - 10)));
            e.Graphics.DrawString(value.ToString(), font, brush, slider, sFormat);

            sFormat?.Dispose();
            innerEllipseBrush?.Dispose();
            brush?.Dispose();
        }

        private void MilestonePaint(PaintEventArgs e)
        {
            Pen pen = new Pen(GetDarkerColor(lineColor), 1.75f);
            SolidBrush brush = new SolidBrush(GetDarkerColor(lineColor));

            float size = Height / 10, linePosY = Height * 15 / 20, stepInterval = (maximumValue - minimumValue) / (float)(legendInterval + 1);
            int xPos = 0, ellipseYPos, stringYPos, totalWidth = Width - Padding.Left - Padding.Right;

            ellipseYPos = Height * 7 / 10;
            stringYPos = Height * 11 / 20;


            for (int ctr = 0; ctr < legendInterval + 2; ctr++)
            {
                xPos = (int)Math.Round(totalWidth * ctr / (double)(legendInterval + 1));
                xPos = xPos + Padding.Left;
                string measureString = ((int)Math.Round(ctr * stepInterval) + minimumValue).ToString();
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

            if (itemCollection.Count > 0)
            {
                pen = new Pen(colorCollection[itemCollection[itemCollection.Count - 1].Key]);
                e.Graphics.DrawEllipse(pen, xPos, circleYPos, Height / 10, Height / 10);
            }

            sFormat?.Dispose();
        }

        #endregion

        private void OnSliderValueChanged(string name, int value)
        {
            int pos = itemCollection.FindIndex(item => item.Key == name);
            int minValue = (int)GetMinAndMaxValueForPosition(pos).Key;
            int maxValue = (int)GetMinAndMaxValueForPosition(pos + 1).Value;

            if ((pos == 0 && value == minValue) || (pos == itemCollection.Count - 1 && value == maxValue) || (minValue < value && value < maxValue))
            {
                itemCollection[pos] = new KeyValuePair<string, int>(name, value);
                sliderCollection[name] = UpdateSliderPosition(value);
                Invalidate();
                SliderValueUpdated?.Invoke(name, value);
            }
        }

        private void AddSlider(string name, double value, int pos)
        {
            sliderCollection.Add(name, UpdateSliderPosition((int)value));
            Invalidate();
        }

        private void DoubleClickDelayTick(object sender, EventArgs e)
        {
            isDelayOver = true;
            doubleClickDelayTimer.Stop();
        }

        #region Calculation

        private int UpdateSelectedSliderValue(int xPos, int width)
        {
            double totalWidth = Width - Padding.Left - Padding.Right;
            double percentage = (xPos + (width / (double)2) - Padding.Left) * 100 / totalWidth;
            double value = Math.Round((maximumValue - minimumValue) * percentage / (double)100) + minimumValue;

            return (int)value;
        }

        private int GetMidValue(int pos)
        {
            var minMaxPairValue = GetMinAndMaxValueForPosition(pos);
            int minValue = minMaxPairValue.Key, maxValue = minMaxPairValue.Value;

            return (minValue + maxValue) / 2;
        }

        private int IsValidValueOrUpdate(int pos, int value)
        {
            var minMaxPairValue = GetMinAndMaxValueForPosition(pos);
            int minValue = minMaxPairValue.Key, maxValue = minMaxPairValue.Value;

            if (minValue == maxValue)
                return -1;

            //if (minValue == MinimumValue && pos == 0)
            //    return minValue;

            if (minValue < value && value == MaximumValue && pos == itemCollection.Count)
                return value;

            return minValue < value && value < maxValue ? value : minValue + 1;
        }

        private KeyValuePair<int, int> GetMinAndMaxValueForPosition(int pos)
        {
            int minValue, maxValue;
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

            return new KeyValuePair<int, int>(minValue, maxValue);
        }

        private Rectangle UpdateSliderPosition(int value)
        {
            int totalWidth = Width - Padding.Left - Padding.Right, startPosY;
            double percentage = (value - MinimumValue) * 100 / (double)(MaximumValue - MinimumValue);
            startPosY = Height * 4 / 20;
            Point location = new Point((int)(percentage * totalWidth / 100) - sliderSize / 2 + Padding.Left, (startPosY - (sliderSize - Height / 10) / 2));
            return new Rectangle(location, new Size(sliderSize, sliderSize));
        }

        private float GetFittingFontSize(Graphics g, string text, Font font, Rectangle rect)
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

        #endregion

        #region OtherToolTip Mechanism

        private void ShowOtherToolTips()
        {
            int currentItemIdx = itemCollection.FindIndex(item => item.Key == selectedSlider), flagIdx = 0;
            CloseOtherToolTips();
            toolTipCollection = new Dictionary<string, SliderToolTip>();
            SliderToolTip otherToolTip;

            for (int ctr = currentItemIdx - 1; ctr >= 0; ctr--)
            {
                otherToolTip = new SliderToolTip(sliderTransparentColor);
                bool isUpsideDown = TooltipDisplayMode == TooltipDisplayMode.Alternate && flagIdx % 2 == 0 ? true : true;
                SetToolTipValue(otherToolTip, itemCollection[ctr].Key, itemCollection[ctr].Value, lineColor, DisplayType.Name, false, isUpsideDown, false);

                otherToolTip.Show();
                toolTipCollection.Add(itemCollection[ctr].Key, otherToolTip);
                flagIdx++;
            }

            flagIdx = 0;

            for (int ctr = currentItemIdx + 1; ctr < itemCollection.Count; ctr++)
            {
                otherToolTip = new SliderToolTip(sliderTransparentColor);
                bool isUpsideDown = TooltipDisplayMode == TooltipDisplayMode.Alternate && flagIdx % 2 == 0 ? true : true;
                SetToolTipValue(otherToolTip, itemCollection[ctr].Key, itemCollection[ctr].Value, lineColor, DisplayType.Name, false, isUpsideDown, false);

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

        #endregion

        private Dictionary<string, Rectangle> GetSliderCollectionCopy()
        {
            var copyCollection = new Dictionary<string, Rectangle>();

            foreach (var Iter in sliderCollection)
            {
                copyCollection.Add(Iter.Key, new Rectangle()
                {
                    X = Iter.Value.X,
                    Y = Iter.Value.Y,
                    Width = Iter.Value.Width,
                    Height = Iter.Value.Height,
                });
            }

            return copyCollection;
        }

        private void SelectSliderOnMouseDown(MouseEventArgs e, Rectangle slider, string itemName, int value)
        {
            isDragging = true; isDelayOver = false;
            offSetPoint = new Point(e.Location.X - slider.X, e.Location.Y - slider.Y);
            selectedSlider = itemName;

            int pos = itemCollection.FindIndex(item => item.Key == itemName);
            int totalWidth = Width - Padding.Right - Padding.Left;

            selectedSliderMinValue = GetMinAndMaxValueForPosition(pos).Key;
            selectedSliderMaxValue = GetMinAndMaxValueForPosition(pos + 1).Value;

            double percentage = (selectedSliderMinValue - MinimumValue) * 100 / (MaximumValue - MinimumValue);
            Point location = new Point((int)(Math.Round(percentage * totalWidth / 100) + Padding.Left + 1), (int)(Math.Round((Height - 20) / 2.0)));
            selectedSliderMinPos = location.X;

            percentage = (selectedSliderMaxValue - MinimumValue) * 100 / (MaximumValue - MinimumValue);
            location = new Point((int)(Math.Round(percentage * totalWidth / 100) + Padding.Left - 1), (int)(Math.Round((Height - 20) / 2.0)));
            selectedSliderMaxPos = location.X;

            Point toolTipLocation = PointToScreen(slider.Location);

            SetToolTipValue(selectedToolTip, itemName, value, lineColor, DisplayType.NameAndValue, true, false, true);

            doubleClickDelayTimer.Start();
        }

        private Color GetDarkerColor(Color color)
        {
            int r = color.R - 50 < 0 ? 0 : color.R - 50;
            int g = color.G - 50 < 0 ? 0 : color.G - 50;
            int b = color.B - 50 < 0 ? 0 : color.B - 50;

            return Color.FromArgb(r, g, b);
        }

        private void SetToolTipValue(SliderToolTip toolTip, string name, int value, Color sliderBackColor, DisplayType type, bool isCurrentToolTip, bool isUpsideDown, bool isInputEntry)
        {
            Point toolTipLocation = PointToScreen(sliderCollection[name].Location);

            toolTip.SliderToolTipBackColor = sliderBackColor;
            toolTip.Font = Font;
            toolTip.ForeColor = colorCollection[name];
            toolTip.SliderName = name;
            toolTip.Value = value;
            toolTip.ToolTipDisplayType = type;
            toolTip.IsUpsideDown = isUpsideDown;
            toolTip.Location = toolTip.IsUpsideDown ? new Point(toolTipLocation.X + sliderCollection[name].Width / 2 - toolTip.Width / 2, toolTipLocation.Y + 40) : new Point(toolTipLocation.X + sliderCollection[name].Width / 2 - toolTip.Width / 2, toolTipLocation.Y - 60);
            toolTip.IsCurrentSelectedToolTip = isCurrentToolTip;
            toolTip.IsInputEntry = isInputEntry;
        }

        private void InitializeMultiSlider()
        {
            DoubleBuffered = true;
            itemCollection = new List<KeyValuePair<string, int>>();
            sliderCollection = new Dictionary<string, Rectangle>();
            colorCollection = new Dictionary<string, Color>();
            doubleClickDelayTimer = new Timer();
            doubleClickDelayTimer.Interval = 100;
            doubleClickDelayTimer.Tick += DoubleClickDelayTick;

            if (selectedToolTip != null)
                selectedToolTip.SliderValueChanged -= OnSliderValueChanged;

            selectedToolTip = new SliderToolTip(SliderTransparentColor)
            {
                SliderToolTipBackColor = lineColor,
                ForeColor = sliderColor,
                IsUpsideDown = true,
                IsCurrentSelectedToolTip = true,
                ToolTipDisplayType = DisplayType.NameAndValue
            };
            selectedToolTip.SliderValueChanged += OnSliderValueChanged;
        }
    }

    public enum TooltipDisplayMode
    {
        Down,
        Alternate
    }
}
