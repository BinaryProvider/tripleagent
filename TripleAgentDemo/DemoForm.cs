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
        public DemoForm()
        {
            InitializeComponent();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"  
                <animations>
                    <animation name=""Show"">
                        <startframe num=""824"" />
                        <endframe num=""854"" />
                    </animation>
                    <animation name=""Idle"" loopcount=""-1"" frameduration=""120"" loopdelay=""3000"">
                        <startframe num=""234"" />
                        <endframe num=""269"" />
                    </animation>
                    <animation name=""Ok"">
                        <startframe num=""1"" />
                        <endframe num=""23"" />
                    </animation>
                    <animation name=""PointRight"">
                        <startframe num=""512"" />
                        <endframe num=""536"" />
                    </animation>
                    <animation name=""KnockOnScreen"">
                        <startframe num=""200"" />
                        <endframe num=""218"" />
                    </animation>
                </animations>"
            );

            tripleAgentControl2.LoadAnimationData(doc);
        }
        private void DemoForm_Load(object sender, EventArgs e)
        {
            this.Activate();

            SpriteAnimation anim = tripleAgentControl2.SpriteAnimations[1];
            tripleAgentControl2.PlayAnimation(anim);

            //tripleAgentControl2.ShowTip(anim, new Point(0, 0), new Point(140, 20), new Size(400, 0), "Make sure you phrase the link as the continuation of the following statement: I want to...", 1000, 3000);

        }
    }
}
