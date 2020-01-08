namespace LifeSimulator
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
            this.components = new System.ComponentModel.Container();
            this.picViewport = new System.Windows.Forms.PictureBox();
            this.tmrSim = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.picViewport)).BeginInit();
            this.SuspendLayout();
            // 
            // picViewport
            // 
            this.picViewport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picViewport.Location = new System.Drawing.Point(0, 0);
            this.picViewport.Name = "picViewport";
            this.picViewport.Size = new System.Drawing.Size(545, 473);
            this.picViewport.TabIndex = 0;
            this.picViewport.TabStop = false;
            this.picViewport.Click += new System.EventHandler(this.picViewport_Click);
            this.picViewport.Paint += new System.Windows.Forms.PaintEventHandler(this.picViewport_Paint);
            // 
            // tmrSim
            // 
            this.tmrSim.Interval = 25;
            this.tmrSim.Tick += new System.EventHandler(this.tmrSim_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(545, 473);
            this.Controls.Add(this.picViewport);
            this.Name = "Form1";
            this.Text = "LifeSimulator";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picViewport)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox picViewport;
        private System.Windows.Forms.Timer tmrSim;
    }
}

