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
            this.button1 = new System.Windows.Forms.Button();
            this.multiSlider1 = new PowerControlsLibrary.MultiSlider.MultiSlider();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(595, 373);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(123, 73);
            this.button1.TabIndex = 1;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // multiSlider1
            // 
            this.multiSlider1.Dock = System.Windows.Forms.DockStyle.Top;
            this.multiSlider1.ItemDisplayOnHover = false;
            this.multiSlider1.LegendInterval = 1;
            this.multiSlider1.Location = new System.Drawing.Point(12, 115);
            this.multiSlider1.MaximumValue = 600;
            this.multiSlider1.MinimumValue = 50;
            this.multiSlider1.Name = "multiSlider1";
            this.multiSlider1.Padding = new System.Windows.Forms.Padding(20, 0, 20, 0);
            this.multiSlider1.Size = new System.Drawing.Size(1167, 100);
            this.multiSlider1.SliderTransparentColor = System.Drawing.Color.Empty;
            this.multiSlider1.TabIndex = 2;
            this.multiSlider1.TooltipDisplayMode = PowerControlsLibrary.MultiSlider.TooltipDisplayMode.Down;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1191, 519);
            this.Controls.Add(this.multiSlider1);
            this.Controls.Add(this.button1);
            this.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "Form1";
            this.Padding = new System.Windows.Forms.Padding(12, 115, 12, 0);
            this.Text = "Form1";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button button1;
        private PowerControlsLibrary.MultiSlider.MultiSlider multiSlider1;
    }
}

