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
            this.tripleAgentControl2 = new TripleAgent.TripleAgentControl();
            this.SuspendLayout();
            // 
            // tripleAgentControl2
            // 
            this.tripleAgentControl2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tripleAgentControl2.BackColor = System.Drawing.Color.Transparent;
            this.tripleAgentControl2.Location = new System.Drawing.Point(12, 12);
            this.tripleAgentControl2.Name = "tripleAgentControl2";
            this.tripleAgentControl2.Size = new System.Drawing.Size(905, 528);
            this.tripleAgentControl2.SpriteHiddenFromStart = false;
            this.tripleAgentControl2.SpriteSheet = global::TripleAgentDemo.Properties.Resources.clippy_spritesheet;
            this.tripleAgentControl2.SpriteSize = new System.Drawing.Size(124, 93);
            this.tripleAgentControl2.SpriteStartFrame = 1;
            this.tripleAgentControl2.SpriteStartLocation = System.Drawing.ContentAlignment.MiddleRight;
            this.tripleAgentControl2.TabIndex = 0;
            // 
            // DemoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(929, 552);
            this.Controls.Add(this.tripleAgentControl2);
            this.Name = "DemoForm";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private TripleAgent.TripleAgentControl tripleAgentControl2;
    }
}

