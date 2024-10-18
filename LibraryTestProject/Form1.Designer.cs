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
            this.multiSlider1.LegendInterval = 1;
            this.multiSlider1.Location = new System.Drawing.Point(49, 145);
            this.multiSlider1.MaximumValue = 0;
            this.multiSlider1.MinimumValue = 0;
            this.multiSlider1.Name = "multiSlider1";
            this.multiSlider1.Size = new System.Drawing.Size(800, 75);
            this.multiSlider1.TabIndex = 0;
            this.multiSlider1.Text = "multiSlider1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(909, 450);
            this.Controls.Add(this.multiSlider1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private PowerControlsLibrary.MultiSlider.MultiSlider multiSlider1;
    }
}

