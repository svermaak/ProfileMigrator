namespace ProfileMigrator
{
    partial class formMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(formMain));
            this.textUserName = new System.Windows.Forms.TextBox();
            this.labelUserName = new System.Windows.Forms.Label();
            this.buttonMigrate = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // textUserName
            // 
            this.textUserName.Location = new System.Drawing.Point(194, 361);
            this.textUserName.Name = "textUserName";
            this.textUserName.Size = new System.Drawing.Size(152, 20);
            this.textUserName.TabIndex = 0;
            this.textUserName.UseWaitCursor = true;
            // 
            // labelUserName
            // 
            this.labelUserName.AutoSize = true;
            this.labelUserName.Location = new System.Drawing.Point(128, 364);
            this.labelUserName.Name = "labelUserName";
            this.labelUserName.Size = new System.Drawing.Size(60, 13);
            this.labelUserName.TabIndex = 1;
            this.labelUserName.Text = "User Name";
            this.labelUserName.UseWaitCursor = true;
            // 
            // buttonMigrate
            // 
            this.buttonMigrate.Location = new System.Drawing.Point(194, 387);
            this.buttonMigrate.Name = "buttonMigrate";
            this.buttonMigrate.Size = new System.Drawing.Size(75, 23);
            this.buttonMigrate.TabIndex = 2;
            this.buttonMigrate.Text = "Migrate";
            this.buttonMigrate.UseVisualStyleBackColor = true;
            this.buttonMigrate.UseWaitCursor = true;
            this.buttonMigrate.Click += new System.EventHandler(this.buttonMigrate_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(120)))), ((int)(((byte)(180)))));
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(52, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(524, 309);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 8;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.UseWaitCursor = true;
            // 
            // formMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(66)))), ((int)(((byte)(120)))), ((int)(((byte)(180)))));
            this.ClientSize = new System.Drawing.Size(628, 424);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.buttonMigrate);
            this.Controls.Add(this.labelUserName);
            this.Controls.Add(this.textUserName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "formMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Profile Migrator";
            this.UseWaitCursor = true;
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textUserName;
        private System.Windows.Forms.Label labelUserName;
        private System.Windows.Forms.Button buttonMigrate;
        private System.Windows.Forms.PictureBox pictureBox1;


    }
}

