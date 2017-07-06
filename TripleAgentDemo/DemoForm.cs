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
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;


namespace TripleAgentDemo
{
    public partial class DemoForm : Form
    {
        public DemoForm()
        {
            InitializeComponent();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(@"  <animations>
                                <animation name=""Show"">
                                    <startframe num=""824"" />
                                    <endframe num=""854"" />
                                </animation>
                            </animations>"
                        );

            tripleAgentControl2.AddAnimationData(doc);     
        }

        private void DemoForm_Load(object sender, EventArgs e)
        {
            //tripleAgentControl2.PlayAnimation(1, 1, true);
        }
    }
}
