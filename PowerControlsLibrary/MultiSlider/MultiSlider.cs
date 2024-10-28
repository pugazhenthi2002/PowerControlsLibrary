﻿using System;
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

        public SliderMilestoneMode MilestoneMode
        {
            get => milestoneMode;
            set
            {
                milestoneMode = value;
                Invalidate();
            }
        }

        public SliderLegendMode LegendMode
        {
            get => legendMode;
            set
            {
                legendMode = value;
                Invalidate();
            }
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

        public Color SliderTransparentColor
        {
            get => sliderTransparentColor;
            set
            {
                sliderTransparentColor = value;

                toolTip.SliderValueChanged -= OnSliderValueChanged;
                toolTip = new SliderValueToolTip(value);
                toolTip.IsCurrentSelectedToolTip = true;
                toolTip.SliderValueChanged += OnSliderValueChanged;
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

        private SliderMilestoneMode milestoneMode;
        private SliderLegendMode legendMode;
        private bool isDragging, isInputEntering;
        private int minimumValue = 0, maximumValue = 100, legendInterval = 1, sliderSize = 20;
        private int selectedSliderMinPos = 0, selectedSliderMaxPos = 0;
        private double selectedSliderMinValue = 0, selectedSliderMaxValue = 0;
        private string selectedSlider, hoverSlider;
        private Color lineColor = Color.AliceBlue, sliderColor = Color.DeepSkyBlue, sliderTransparentColor;
        private Point offSetPoint;
        private List<KeyValuePair<string, double>> itemCollection;
        private Dictionary<string, Rectangle> sliderCollection;
        private Dictionary<string, Color> colorCollection;
        private Dictionary<string, SliderValueToolTip> toolTipCollection;
        private SliderValueToolTip toolTip;

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
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            SolidBrush brush = new SolidBrush(lineColor);
            Pen pen = new Pen(GetDarkerColor(lineColor), 2);
            Rectangle leftArc = new Rectangle(Padding.Left, Height * 3 / 10, Height / 10, Height / 10);
            Rectangle rightArc = new Rectangle(Width - Height / 10 - Padding.Right, Height * 3 / 10, Height / 10 - 1, Height / 10);

            leftArc.Y = milestoneMode == SliderMilestoneMode.Ruler ? Height * 4 / 10 : Height * 3 / 10;
            rightArc.Y = milestoneMode == SliderMilestoneMode.Ruler ? Height * 4 / 10 : Height * 3 / 10;

            GraphicsPath path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(leftArc, 90, 180);
            path.AddArc(rightArc, 270, 180);
            path.CloseAllFigures();

            e.Graphics.FillPath(brush, path);
            SliderBarPaint(e);
            e.Graphics.DrawPath(pen, path);
            MilestonePaint(e);
            SliderPaint(e);

            if (legendMode == SliderLegendMode.UpDownMarker && !isDragging)
                SliderLegendPaint(e);

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

            Height = 80;
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
                    toolTip.SliderToolTipBackColor = lineColor;
                    toolTip.Font = Font;
                    toolTip.ForeColor = sliderColor;
                    toolTip.SliderName = itemCollection[ctr].Key;
                    toolTip.Value = (int)itemCollection[ctr].Value;
                    toolTip.Location = new Point(toolTipLocation.X - (toolTip.Width - slider.Width) / 2, toolTipLocation.Y - 60);
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
                    isInputEntering = toolTip.IsInputEntry = false;
                    toolTip.Value = (int)itemCollection[pos].Value;
                    toolTip.Location = new Point(toolTipLocation.X - (toolTip.Width - rect.Width) / 2, toolTipLocation.Y - 60);
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
                    toolTip.IsInputEntry = true;
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

        private void SliderLegendPaint(PaintEventArgs e)
        {
            int sliderBarPosY = milestoneMode == SliderMilestoneMode.Funnel ? Height * 3 / 10 : Height * 4 / 10, legendPosX = Padding.Left;
            Brush brush;
            Rectangle rectangle;
            StringFormat sFormat = new StringFormat();
            sFormat.Alignment = StringAlignment.Center;
            sFormat.LineAlignment = StringAlignment.Near;

            for (int ctr = 0; ctr < itemCollection.Count; ctr++)
            {
                rectangle = new Rectangle(legendPosX, 0, sliderCollection[itemCollection[ctr].Key].X - legendPosX, sliderBarPosY);
                brush = new SolidBrush(colorCollection[itemCollection[ctr].Key]);
                e.Graphics.DrawString(itemCollection[ctr].Key + ", " + itemCollection[ctr].Value.ToString(), Font, brush, rectangle, sFormat);
                brush?.Dispose();
                legendPosX = sliderCollection[itemCollection[ctr].Key].X;
            }
        }

        private void SliderBarPaint(PaintEventArgs e)
        {
            int sliderBarPosX = Padding.Left, sliderBarPosY = milestoneMode == SliderMilestoneMode.Funnel ? Height * 3 / 10 : Height * 4 / 10;
            Brush brush;

            for (int ctr = 0; ctr < itemCollection.Count + 1; ctr++)
            {
                Rectangle leftArc = new Rectangle(sliderBarPosX, sliderBarPosY, Height / 10, Height / 10);
                Rectangle rightArc;

                if (ctr == itemCollection.Count)
                    rightArc = new Rectangle(Width - Height / 10 - Padding.Right, sliderBarPosY, Height / 10, Height / 10);
                else
                    rightArc = new Rectangle(sliderCollection[itemCollection[ctr].Key].X + sliderSize / 2, sliderBarPosY, Height / 10, Height / 10);

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

        private void SliderPaint(PaintEventArgs e)
        {
            SolidBrush outerEllipseBrush = new SolidBrush(sliderColor);
            SolidBrush innerEllipseBrush = new SolidBrush(Color.White);

            foreach (var Iter in itemCollection)
            {
                var slider = sliderCollection[Iter.Key];
                e.Graphics.FillEllipse(outerEllipseBrush, slider.X, slider.Y, slider.Width, slider.Height);

                if (hoverSlider == Iter.Key)
                    e.Graphics.FillEllipse(innerEllipseBrush, slider.X + 5, slider.Y + 5, slider.Width - 10, slider.Height - 10);
                else
                    e.Graphics.FillEllipse(innerEllipseBrush, slider.X + 3, slider.Y + 3, slider.Width - 6, slider.Height - 6);

            }
            innerEllipseBrush?.Dispose();
            outerEllipseBrush?.Dispose();
        }

        private void MilestonePaint(PaintEventArgs e)
        {
            Pen pen = new Pen(sliderColor, 1.75f);
            SolidBrush brush = new SolidBrush(sliderColor);

            float size = Height / 10, linePosY = Height * 15 / 20, stepInterval = MaximumValue / (float)(legendInterval + 1);
            int xPos = 0, ellipseYPos, stringYPos, totalWidth = Width - Padding.Left - Padding.Right;

            ellipseYPos = milestoneMode == SliderMilestoneMode.Funnel ? Height * 7 / 10 : Height * 6 / 10;
            stringYPos = milestoneMode == SliderMilestoneMode.Funnel ? Height * 17 / 20 : Height * 6 / 10;

            if (milestoneMode == SliderMilestoneMode.Funnel)
                e.Graphics.DrawLine(pen, Padding.Left, linePosY, Width - Padding.Right, linePosY);
            for (int ctr = 0; ctr < legendInterval + 2; ctr++)
            {
                xPos = (int)Math.Round(totalWidth * ctr / (double)(legendInterval + 1));
                xPos = xPos + Padding.Left;
                string measureString = ((int)Math.Round(ctr * stepInterval)).ToString();
                float width = e.Graphics.MeasureString(measureString, base.Font).Width;

                if (ctr == 0)
                {
                    if (milestoneMode == SliderMilestoneMode.Funnel)
                        e.Graphics.FillEllipse(brush, new RectangleF(Padding.Left, ellipseYPos, size, size));

                    e.Graphics.DrawString(measureString, base.Font, brush, Padding.Left, stringYPos);
                }
                else if (ctr == legendInterval + 1)
                {
                    if (milestoneMode == SliderMilestoneMode.Funnel)
                        e.Graphics.FillEllipse(brush, new RectangleF(Width - (size) - Padding.Right, ellipseYPos, size, size));

                    e.Graphics.DrawString(measureString, base.Font, brush, Width - width - Padding.Right, stringYPos);
                }
                else
                {
                    if (milestoneMode == SliderMilestoneMode.Ruler)
                        e.Graphics.DrawLine(pen, xPos, Height * 5 / 10, xPos, Height * 6 / 10);

                    xPos = xPos - (int)(size / 2);

                    if (milestoneMode == SliderMilestoneMode.Funnel)
                        e.Graphics.FillEllipse(brush, new RectangleF(xPos, ellipseYPos, size, size));

                    e.Graphics.DrawString(measureString, base.Font, brush, xPos - (width / 2), stringYPos);
                }
            }

            brush?.Dispose();
            pen?.Dispose();
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

            if (toolTip != null)
                toolTip.SliderValueChanged -= OnSliderValueChanged;

            toolTip = new SliderValueToolTip(SliderTransparentColor)
            {
                SliderToolTipBackColor = lineColor,
                ForeColor = sliderColor,
                IsUpsideDown = true,
                IsCurrentSelectedToolTip = true
            };
            toolTip.SliderValueChanged += OnSliderValueChanged;
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
            startPosY = milestoneMode == SliderMilestoneMode.Funnel ? Height * 3 / 10 : Height * 4 / 10;
            Point location = new Point((int)(percentage * totalWidth / 100) - sliderSize / 2 + Padding.Left, (startPosY - (sliderSize - Height / 10) / 2));
            return new Rectangle(location, new Size(sliderSize, sliderSize));
        }

        public void ShowOtherToolTips()
        {
            int currentItemIdx = itemCollection.FindIndex(item => item.Key == selectedSlider), flagIdx = 0;
            toolTipCollection = new Dictionary<string, SliderValueToolTip>();
            SliderValueToolTip otherToolTip;
            for (int ctr = currentItemIdx - 1; ctr >= 0; ctr--)
            {
                otherToolTip = new SliderValueToolTip(sliderTransparentColor)
                {
                    SliderToolTipBackColor = lineColor,
                    ForeColor = sliderColor,
                    Font = Font,
                    SliderName = itemCollection[ctr].Key,
                    Value = (int)itemCollection[ctr].Value
                };

                otherToolTip.IsUpsideDown = flagIdx % 2 == 0 ? true : false;

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
                    ForeColor = sliderColor,
                    Font = Font,
                    SliderName = itemCollection[ctr].Key,
                    Value = (int)itemCollection[ctr].Value
                };

                otherToolTip.IsUpsideDown = flagIdx % 2 == 0 ? true : false;

                Rectangle slider = sliderCollection[itemCollection[ctr].Key];
                Point toolTipLocation = PointToScreen(slider.Location);

                otherToolTip.Location = otherToolTip.IsUpsideDown ? new Point(toolTipLocation.X + slider.Width / 2 - otherToolTip.Width / 2, toolTipLocation.Y + 30) : new Point(toolTipLocation.X + slider.Width / 2 - otherToolTip.Width / 2, toolTipLocation.Y - 60);
                otherToolTip.Show();
                toolTipCollection.Add(itemCollection[ctr].Key, otherToolTip);
                flagIdx++;
            }
        }

        public void CloseOtherToolTips()
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

    public enum SliderMilestoneMode
    {
        Ruler,
        Funnel
    }

    public enum SliderLegendMode
    {
        Form,
        UpDownMarker
    }
}
