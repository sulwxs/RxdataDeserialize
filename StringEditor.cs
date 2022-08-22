using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace rxdatadecoder
{
    public partial class StringEditor : Form
    {
        public XmlDocument xld;
        public XmlNodeList xmlNodeList = null;
        public ListViewItem selitem;
        private bool hasload = false;
        public StringEditor(XmlDocument document)
        {
            InitializeComponent();
            if (document == null)
                return;
            this.xld = document;
            xmlNodeList=  xld.SelectNodes("//String");
            foreach (XmlNode node in xmlNodeList) 
            {
                listView1.Items.Add(node.InnerText);
            
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                StreamWriter writer = new StreamWriter(saveFileDialog1.FileName,false,Encoding.UTF8);       
               foreach(XmlNode  node in xmlNodeList)
                    writer.WriteLine(node.InnerText);
                writer.Close();
                MessageBox.Show("已导出到"+saveFileDialog1.FileName);
            }
        
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                StreamReader reader = new StreamReader(openFileDialog1.FileName,Encoding.UTF8);
                int i = 0;
                while (reader.Peek() != -1&&i<listView1.Items.Count)
                {
                    String newline = reader.ReadLine();
                        ListViewItem item = listView1.Items[i];
                    if(item.SubItems.Count<2)
                    item.SubItems.Add( newline);
                    else
                        item.SubItems[1].Text=newline;

                    i++;
                }
                hasload = true;
                if (i < xmlNodeList.Count)
                {
                    hasload = false;
                    MessageBox.Show("行数小于原文本,无法保存！","错误",MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
                reader.Close();
                
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            var listview=(sender as ListView);

            if (listview.SelectedItems.Count > 0)
            {
                 selitem = listview.SelectedItems[0];
             if(text==null)   
                text = new TextBox();
                text.Location =selitem.Position;
                text.Focus();
                text.Visible = true;
                text.Top -= 2;
                text.Text = selitem.Text;
                text.Width = columnHeader1.Width;
                text.LostFocus += new EventHandler((send,obj) =>
                {
                    text.Clear();
                    text.Visible = false;
                    selitem = null;
                   // text.Dispose();
                }
                );
                listview.Controls.Add(text);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (xmlNodeList != null)
            { int i = 0;
                if (!hasload)
                    foreach (XmlNode node in xmlNodeList)
                    {
                   
                        node.InnerText = listView1.Items[i].Text;
                        i++;
                    }
                else 
                {
                    foreach (XmlNode node in xmlNodeList)
                    {
                       
                        node.InnerText = listView1.Items[i].SubItems[1].Text;
                        i++;
                    }
                }
            }
        }

        private void text_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ((char)Keys.Enter)&&selitem!=null) 
            {
                selitem.Text = text.Text;
                text.Visible = false;
            }
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            text.Visible = false;
        }

        private void text_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
