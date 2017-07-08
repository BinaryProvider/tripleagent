namespace TripleAgentDemo
{
    partial class DemoForm
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
            this.someTextbox1 = new System.Windows.Forms.TextBox();
            this.someLabel1 = new System.Windows.Forms.Label();
            this.someLabel2 = new System.Windows.Forms.Label();
            this.someTextbox2 = new System.Windows.Forms.TextBox();
            this.someButton1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // someTextbox1
            // 
            this.someTextbox1.Location = new System.Drawing.Point(12, 122);
            this.someTextbox1.Name = "someTextbox1";
            this.someTextbox1.Size = new System.Drawing.Size(239, 20);
            this.someTextbox1.TabIndex = 0;
            this.someTextbox1.TextChanged += new System.EventHandler(this.someTextbox_TextChanged);
            this.someTextbox1.Leave += new System.EventHandler(this.someTextbox_Leave);
            // 
            // someLabel1
            // 
            this.someLabel1.AutoSize = true;
            this.someLabel1.Location = new System.Drawing.Point(9, 106);
            this.someLabel1.Name = "someLabel1";
            this.someLabel1.Size = new System.Drawing.Size(56, 13);
            this.someLabel1.TabIndex = 1;
            this.someLabel1.Text = "Some field";
            // 
            // someLabel2
            // 
            this.someLabel2.AutoSize = true;
            this.someLabel2.Location = new System.Drawing.Point(9, 154);
            this.someLabel2.Name = "someLabel2";
            this.someLabel2.Size = new System.Drawing.Size(83, 13);
            this.someLabel2.TabIndex = 3;
            this.someLabel2.Text = "Some other field";
            // 
            // someTextbox2
            // 
            this.someTextbox2.Location = new System.Drawing.Point(12, 170);
            this.someTextbox2.Name = "someTextbox2";
            this.someTextbox2.Size = new System.Drawing.Size(239, 20);
            this.someTextbox2.TabIndex = 1;
            this.someTextbox2.TextChanged += new System.EventHandler(this.someTextbox_TextChanged);
            this.someTextbox2.Leave += new System.EventHandler(this.someTextbox_Leave);
            // 
            // someButton1
            // 
            this.someButton1.Location = new System.Drawing.Point(12, 205);
            this.someButton1.Name = "someButton1";
            this.someButton1.Size = new System.Drawing.Size(239, 23);
            this.someButton1.TabIndex = 2;
            this.someButton1.Text = "Some button";
            this.someButton1.UseVisualStyleBackColor = true;
            this.someButton1.Click += new System.EventHandler(this.someButton1_Click);
            // 
            // DemoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(263, 251);
            this.Controls.Add(this.someButton1);
            this.Controls.Add(this.someLabel2);
            this.Controls.Add(this.someTextbox2);
            this.Controls.Add(this.someLabel1);
            this.Controls.Add(this.someTextbox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DemoForm";
            this.Text = "TripleAgentDemo";
            this.Load += new System.EventHandler(this.DemoForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox someTextbox1;
        private System.Windows.Forms.Label someLabel1;
        private System.Windows.Forms.Label someLabel2;
        private System.Windows.Forms.TextBox someTextbox2;
        private System.Windows.Forms.Button someButton1;
    }
}

