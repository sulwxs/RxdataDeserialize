using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace mycontrol
{
    
    public partial class PropertyDisp : UserControl
    {
        public delegate void DataEventHandler(Object sender, ChangeEventArgs args);
        public event DataEventHandler onDataChange;
        public enum CtrlType
        {
        EDIT=0,
        LABEL=1,
        COMB=2
        }
        public XmlNode node=null;
        public List<Control> controls=null;
        private List<Object> result;
        public PropertyDisp()
        {
            InitializeComponent();
        }
        
        public void updateView() 
        {
            if (node != null)
            {
                this.label4.Text = node.Name;
                analysisNode();
            }
        }
        public void addItem(String name,String value) 
        {

            Label namelabel = new Label();
            namelabel.Text = name;
            namelabel.Dock = DockStyle.Top;
            namelabel.Height = 40;
            namelabel.TextAlign = ContentAlignment.MiddleCenter;
            tableLayoutPanel1.Controls.Add(namelabel);
            Label vallabel = new Label();
            vallabel.Text = value;
            vallabel.Dock = DockStyle.Top;
            vallabel.Height = 40;

            vallabel.TextAlign = ContentAlignment.MiddleCenter;
            tableLayoutPanel1.Controls.Add(vallabel);
        }
        public void removeItem() 
        {

            while (tableLayoutPanel1.Controls.Count>4)
            {
                tableLayoutPanel1.Controls.RemoveAt(4);
            }
        }
        private void PropertyDisp_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnChangeData(label4.Text,"");
        }

        private void OnChangeData(String data, String type) 
        {
           
                if (onDataChange != null)
                {
                    ChangeEventArgs e = new ChangeEventArgs();
                    e.Type =type;
                    e.Data = data;
                    onDataChange.Invoke(this, e);
                }
            
        }
       public class ChangeEventArgs : EventArgs
        {
            public ChangeEventArgs() : base() { }
        public String Type;
        public String Data;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        public void dispNode() 
        {
        
        }
        public void analysisNode() 
        {
            removeItem();
            if (this.node != null)
            {

                if (node.ChildNodes.Count ==1&&node.FirstChild.NodeType==XmlNodeType.Text)
                {
                    textBox1.Enabled = true;
                    textBox1.Text = node.InnerText;
                }
                else {
                    textBox1.Enabled = false;
                    textBox1.Clear();
                    switch (node.Name) 
                    {
                        case "Members":
                            addItem("成员数",node.ChildNodes.Count.ToString());
                            break;
                        case "Hash":
                            addItem("长度", node.ChildNodes.Count.ToString());
                            break;
                        case "Array":
                            addItem("长度", node.ChildNodes.Count.ToString());
                            break;

                    }
                }
            }
        }
        public void saveNode() 
        {
            if (node != null && textBox1.Enabled) 
            {
                node.InnerText = textBox1.Text;
            }
        }
    }
}
