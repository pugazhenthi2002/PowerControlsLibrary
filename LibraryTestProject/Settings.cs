using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibraryTestProject
{
    class Settings : Control
    {
        float actualSize = 1920f;
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float scalingFactor = Width / actualSize;

            GraphicsPath path = new GraphicsPath();

            path.StartFigure(); 
            path.AddLine(568.13f * scalingFactor, 392f * scalingFactor, 176.142f * scalingFactor, 783.864f * scalingFactor);
            path.AddLine(176.142f * scalingFactor, 783.864f * scalingFactor, 392f * scalingFactor, 1743.87f * scalingFactor); 
            path.AddLine(392f * scalingFactor, 1743.87f * scalingFactor, 568.13f * scalingFactor, 1920f * scalingFactor); 
            path.AddLine(568.13f * scalingFactor, 1920f * scalingFactor, 960.118f * scalingFactor, 959.87f * scalingFactor);
            path.CloseFigure(); 

            g.DrawPath(Pens.Black, path);
            g.FillPath(Brushes.Red, path);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Width = Height;
        }
    }
}
