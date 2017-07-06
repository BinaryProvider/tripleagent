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
            this.button1 = new System.Windows.Forms.Button();
            this.tripleAgentControl2 = new TripleAgent.TripleAgent();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(260, 221);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // tripleAgentControl2
            // 
            this.tripleAgentControl2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tripleAgentControl2.BackColor = System.Drawing.SystemColors.Control;
            this.tripleAgentControl2.Location = new System.Drawing.Point(12, 12);
            this.tripleAgentControl2.Name = "tripleAgentControl2";
            this.tripleAgentControl2.Size = new System.Drawing.Size(1026, 441);
            this.tripleAgentControl2.SpriteHiddenFromStart = false;
            this.tripleAgentControl2.SpriteSheet = global::TripleAgentDemo.Properties.Resources.clippy_spritesheet;
            this.tripleAgentControl2.SpriteSize = new System.Drawing.Size(124, 93);
            this.tripleAgentControl2.SpriteStartFrame = 1;
            this.tripleAgentControl2.SpriteStartLocation = new System.Drawing.Point(0, 0);
            this.tripleAgentControl2.TabIndex = 0;
            // 
            // DemoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1050, 465);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.tripleAgentControl2);
            this.Name = "DemoForm";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.DemoForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private TripleAgent.TripleAgent tripleAgentControl2;
        private System.Windows.Forms.Button button1;
    }
}

