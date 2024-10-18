namespace PowerControlsLibrary
{
    partial class TestControl
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

        #region Component Designer generated code

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
            this.multiSlider1.Location = new System.Drawing.Point(54, 136);
            this.multiSlider1.MaximumValue = 0;
            this.multiSlider1.MinimumValue = 0;
            this.multiSlider1.Name = "multiSlider1";
            this.multiSlider1.Size = new System.Drawing.Size(677, 50);
            this.multiSlider1.TabIndex = 0;
            this.multiSlider1.Text = "multiSlider1";
            // 
            // TestControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.multiSlider1);
            this.Name = "TestControl";
            this.Size = new System.Drawing.Size(800, 450);
            this.ResumeLayout(false);

        }

        #endregion

        private MultiSlider.MultiSlider multiSlider1;
    }
}
