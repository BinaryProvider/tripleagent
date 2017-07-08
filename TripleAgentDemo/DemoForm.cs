using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using TripleAgent;

namespace TripleAgentDemo
{
    public partial class DemoForm : Form
    {
        TripleAgentControl agent;

        bool isWriting = false;

        public DemoForm()
        {
            InitializeComponent();

            XmlDocument animationFrames = new XmlDocument();
            animationFrames.LoadXml(@"  
                <animations>
                    <animation name=""Walk"" loopcount=""3"">
                        <startframe num=""131"" />
                        <endframe num=""139"" />
                    </animation>
                    <animation name=""Dance"" loopcount=""-1"" loopdelay=""0"">
                        <startframe num=""183"" />
                        <endframe num=""188"" />
                    </animation>
                    <animation name=""Shoot"">
                        <startframe num=""248"" />
                        <endframe num=""260"" />
                    </animation>
                </animations>"
            );

            Size spriteSize = new Size(64, 64);
            Point spriteLocation = new Point(0, 20);
            int spriteStartFrame = 131;

            agent = new TripleAgentControl(Properties.Resources.demo_spritesheet, spriteSize, spriteStartFrame, spriteLocation, animationFrames);

            agent.Location = new Point(0, 0);
            agent.Width = this.Width;
            agent.Height = this.Height;
            agent.BackColor = Form.DefaultBackColor;

            Controls.Add(agent);
        }
        private void DemoForm_Load(object sender, EventArgs e)
        {
            agent.ShowTip(agent.SpriteAnimations[0], "Hi! I'm a TripleAgent! Use me to let users know what to do in your application!", ContentAlignment.MiddleRight, labelDelay: 500);
        }

        private void someTextbox_TextChanged(object sender, EventArgs e)
        {
            if (!isWriting)
            {
                isWriting = true;
                agent.ShowTip(agent.SpriteAnimations[1], "I sense that you are writing something. Well done I like it!", ContentAlignment.MiddleRight);
            }
        }

        private void someTextbox_Leave(object sender, EventArgs e)
        {
            isWriting = false;
        }

        private void someButton1_Click(object sender, EventArgs e)
        {
            agent.ShowTip(agent.SpriteAnimations[2], "Yeah, press that button!", ContentAlignment.MiddleRight);
        }
    }
}
