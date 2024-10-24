namespace LibraryTestProject
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.multiSlider1 = new PowerControlsLibrary.MultiSlider.MultiSlider();
            this.SuspendLayout();
            // 
            // multiSlider1
            // 
            this.multiSlider1.Dock = System.Windows.Forms.DockStyle.Top;
            this.multiSlider1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.multiSlider1.LegendInterval = 5;
            this.multiSlider1.LegendMode = PowerControlsLibrary.MultiSlider.SliderLegendMode.Milestone;
            this.multiSlider1.Location = new System.Drawing.Point(12, 115);
            this.multiSlider1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.multiSlider1.MaximumValue = 1000;
            this.multiSlider1.MinimumValue = 0;
            this.multiSlider1.Name = "multiSlider1";
            this.multiSlider1.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.multiSlider1.Size = new System.Drawing.Size(1167, 80);
            this.multiSlider1.SliderTransparentColor = System.Drawing.Color.Black;
            this.multiSlider1.TabIndex = 2;
            this.multiSlider1.Text = "multiSlider1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1191, 519);
            this.Controls.Add(this.multiSlider1);
            this.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "Form1";
            this.Padding = new System.Windows.Forms.Padding(12, 115, 12, 0);
            this.Text = "Form1";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ResumeLayout(false);

        }

        #endregion

        private PowerControlsLibrary.MultiSlider.MultiSlider multiSlider1;
    }
}

