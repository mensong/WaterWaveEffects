namespace WaveEffects
{
    partial class Main
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.waterTime = new System.Windows.Forms.Timer(this.components);
            this.dropsTime = new System.Windows.Forms.Timer(this.components);
            this.pbViewport = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbViewport)).BeginInit();
            this.SuspendLayout();
            // 
            // waterTime
            // 
            this.waterTime.Interval = 15;
            this.waterTime.Tick += new System.EventHandler(this.waterTime_Tick);
            // 
            // dropsTime
            // 
            this.dropsTime.Interval = 50000;
            this.dropsTime.Tick += new System.EventHandler(this.dropsTime_Tick);
            // 
            // pbViewport
            // 
            this.pbViewport.BackColor = System.Drawing.Color.Black;
            this.pbViewport.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pbViewport.Image = ((System.Drawing.Image)(resources.GetObject("pbViewport.Image")));
            this.pbViewport.Location = new System.Drawing.Point(-1, -2);
            this.pbViewport.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pbViewport.Name = "pbViewport";
            this.pbViewport.Size = new System.Drawing.Size(449, 197);
            this.pbViewport.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pbViewport.TabIndex = 0;
            this.pbViewport.TabStop = false;
            this.pbViewport.Visible = false;
            this.pbViewport.Click += new System.EventHandler(this.pbViewport_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(506, 249);
            this.ControlBox = false;
            this.Controls.Add(this.pbViewport);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.Main_Load);
            this.Click += new System.EventHandler(this.Main_Click);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Main_Paint);
            ((System.ComponentModel.ISupportInitialize)(this.pbViewport)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer waterTime;
        private System.Windows.Forms.PictureBox pbViewport;
        private System.Windows.Forms.Timer dropsTime;
    }
}