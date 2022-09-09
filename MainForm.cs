

using Decoder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace rxdataEditor
{
    public partial class RxdataEditor : Form
    {
        public DecoderXml decoder;
        public XmlDocument xld;
        public List<XmlNode> Symbollist;
        public List<TreeNode> Instlist;
        public List<Object> result = null;
        public delegate void decodefile();
        public String savepath = "";
        public decodefile mydecode;
        public List<TreeNode> treeNodes = null;
        SynchronizationContext SyncContext = null;
        public bool isshowouterxml = false;
        byte[] restbytes;
        public RxdataEditor()
        {
            InitializeComponent();


        }


        /// <summary>
        /// 更新进度
        /// </summary>
        public void getdecodePogress()
        {

            toolStripprogress1.Value = decoder.Progress;
        }
        /// <summary>
        /// 完成回调
        /// </summary>
        /// <param name="iar"></param>
        void finishDecode(IAsyncResult iar)
        {
            try
            {
                decodefile decode_method = (decodefile)((AsyncResult)iar).AsyncDelegate;
                decode_method.EndInvoke(iar);
                if (result != null && result.Count > 2)
                    SyncContext.Post((re) =>
                    {

                        
                        addtoSymbolListView();
                        addtoInstListView();
                        toolStrip1.Enabled = true;
                        treeView1.Nodes.Clear();
                        treeView1.Nodes.Add((TreeNode)result[2]);
                        toolStripStatusLabel1.Text = "完成";
                        SToolStripButton.Enabled = true;
                        SAToolStripButton.Enabled = true;

                        MessageBox.Show("加载完成");
                    }, null);
                else
                {
                    toolStripStatusLabel1.Text = "失败";
                }

            }


            catch (Exception e)
            {
                SyncContext.Post((m) => { toolStrip1.Enabled = true; }, null);

                MessageBox.Show(e.Message);

            }
            return;
        }
        void finishDecodeXml(IAsyncResult iar)
        {
            Func<List<Object>> action = (Func<List<object>>)((AsyncResult)iar).AsyncDelegate;
            result = action.EndInvoke(iar);
            SyncContext.Post((re) =>
            {

                //richTextBox1.Text = (string)result[0];
                addtoSymbolListView();
                toolStrip1.Enabled = true;
                treeView1.Nodes.Clear();
                treeView1.Nodes.Add((TreeNode)result[2]);
                toolStripStatusLabel1.Text = "完成";
                SToolStripButton.Enabled = true;
                SAToolStripButton.Enabled = true;
                MessageBox.Show("加载完成");
            }, null);


            return;
        }
        /// <summary>
        /// 异步加载
        /// </summary>
        /// <param name="decoder"></param>
        /// <param name="xld"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public void DecodeFile()
        {
            String dump = "";
            try
            {
                dump = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" ;
                String temp=decoder.addTag("section", decoder.Parse(decoder.GetStream()));
                while (decoder.tryReadVer(decoder.GetStream()))
                {
                   
                    temp+=decoder.addTag("section", decoder.Parse(decoder.GetStream()));
                }
                dump += decoder.addTag("root",temp);
                result = new List<Object>();
                result.Add(dump);
                long restlen = decoder.GetStream().Length - decoder.GetStream().Position;
                restbytes = new byte[restlen];
                decoder.GetStream().Read(restbytes,0, (int)restlen);
                    decoder.GetStream().Close();
                xld.LoadXml(dump);
                dump = FormatXML(xld);
                xld.LoadXml(dump);
                TreeNode rootnode = new TreeNode(xld.DocumentElement.Name);
                rootnode.Tag = xld.DocumentElement;
                Instlist = new List<TreeNode>();

                Symbollist = new List<XmlNode>();
                getSymbolInstances(Symbollist, xld.DocumentElement);

                //result.Add(dump);
                result.Add(Symbollist);
                result.Add(rootnode);
                RecursionTreeControl(xld.DocumentElement, rootnode);
                getLinkInstances(rootnode);



                return;
            }
            catch (XmlException xml)
            {

                if (MessageBox.Show(xml.Message + "\r\n是否保存到xml?", xml.Source, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {

                    SyncContext.Post((m) =>
                    {
                        saveFileDialog1.Filter = "xml(*.xml)|*.xml";

                        if (openFileDialog1.FileName != "")
                        {
                            String dumpp = openFileDialog1.FileName + "dump.xml";
                            StreamWriter sw = new StreamWriter(dumpp, false, System.Text.Encoding.UTF8);

                            sw.Write(dump);
                            sw.Flush();
                            sw.Close();
                            MessageBox.Show("已保存到" + dumpp);
                        }
                    }, null);
                }
                SyncContext.Post((m) => { toolStrip1.Enabled = true; }, null);
                return;
            }
        }
        public List<Object> DecodeXml()
        {
            String dump = FormatXML(xld);
            result = new List<Object>();
            result.Add(dump);
            xld.LoadXml(dump);
            dump = FormatXML(xld);
            xld.LoadXml(dump);
            TreeNode rootnode = new TreeNode(xld.DocumentElement.Name);
            rootnode.Tag = xld.DocumentElement;
            Instlist = new List<TreeNode>();

            Symbollist = new List<XmlNode>();
            getSymbolInstances(Symbollist, xld.DocumentElement);

            //result.Add(dump);
            result.Add(Symbollist);
            result.Add(rootnode);
            RecursionTreeControl(xld.DocumentElement, rootnode);
            getLinkInstances(rootnode);
   

            return result;

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (decoder != null)
                decoder.GetStream().Close();
        }
        /// <summary>
        /// 遍历绑定到树
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <param name="parent"></param>
        /// <param name="path"></param>
        private void RecursionTreeControl(XmlNode xmlNode, TreeNode parent)
        {

            int i = 0;
            if (xmlNode.NodeType != XmlNodeType.Element)
                return;

            foreach (XmlNode node in xmlNode.ChildNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue;
                //XmlElement element = (XmlElement)node;
                string temp = node.Name;
                XmlNode xmlNode1 = null;
                switch (temp.ToLower().Trim())
                {
                   
                       
                    case "symbol":
                        temp = node.InnerText;
                        //continue;
                        break;
                    case "class":
                    //    break;
                    case "object":
                        temp = node.FirstChild.InnerText;


                        if (node.FirstChild.FirstChild.Name == "sym")
                        {
                            i = UInt16.Parse(temp, System.Globalization.NumberStyles.Number);
                            if (i < Symbollist.Count)
                                temp = Symbollist[i].InnerText;
                        }
                        //temp = element.GetAttribute("classname");
                        break;
                    case "b":
                        temp =node.InnerText;
                        break;
                    case "i":
                        xmlNode1 = node.FirstChild;
                        String Itype = xmlNode1.Name;
                        if (Itype == "String" || Itype == "Hash" || Itype == "Array" || Itype == "Regexp")
                            temp += Itype;
                        break;

                    case "sym":
                        i = int.Parse(node.InnerText);

                        if (i <= Symbollist.Count)
                            temp = Symbollist[i].InnerText;
                        break;
                    case "item":
                        xmlNode1 = node.FirstChild;
                        if (xmlNode1.Name == "Key")
                            xmlNode1 = xmlNode1.FirstChild;
                        if (xmlNode1.Name == "sym")
                        {
                            i = UInt16.Parse(xmlNode1.InnerText);
                            if (i < Symbollist.Count)
                                temp = Symbollist[i].InnerText;
                        }
                        else if (xmlNode1.Name == "Symbol")
                        {
                            temp = xmlNode1.InnerText;
                        }
                        else
                        {
                            temp = xmlNode1.InnerText;
                        }


                        break;
                    case "link":
                        temp = node.FirstChild.InnerText;
           
                        break;

                }
                if (temp != "section")
                {
                    TreeNode new_child = new TreeNode(temp);
                    new_child.Tag = node;
                    parent.Nodes.Add(new_child);
                    RecursionTreeControl(node, new_child);
                }
                else 
                {
                    
                 
                    RecursionTreeControl(node, parent);
                }

               

                 




            }


        }
        private void addItemnode(TreeNode item, XmlElement value)
        {
            TreeNode val = new TreeNode(value.Name);
            val.Tag = item.Tag + "/1";
            item.Nodes.Add(val);
            RecursionTreeControl(value, val);
        }
        /// <summary>
        /// 格式化xml
        /// </summary>
        /// <param name="XMLstring"></param>
        /// <returns></returns>
        public static string FormatXML(XmlDocument document)
        {

     
            MemoryStream memoryStream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(memoryStream, null)
            {
                Formatting = Formatting.Indented
            };
            document.Save(writer);
            StreamReader streamReader = new StreamReader(memoryStream);
            memoryStream.Position = 0;
            string xmlString = streamReader.ReadToEnd();
            streamReader.Close();
            memoryStream.Close();
            return xmlString;
        }

        public static string GetNodePath(XmlNode xmlNode)
        {
            string pathName = xmlNode.Name;
            XmlNode node = xmlNode;
            while (true)
            {
                if (node.ParentNode.Name != "#document")
                {

                    pathName = $"{node.ParentNode.Name}/{pathName}";
                }
                else
                {
                    return pathName;

                }
                node = node.ParentNode;
            }
        }
        public XmlNode getNodebyPos(String pos)
        {
            String[] list = pos.Split('/');
            XmlNode node = xld.DocumentElement;
            foreach (String l in list)
            {
                if (l == "")
                    continue;
                int p = int.Parse(l);
                if (node.ChildNodes.Count >= p + 1)
                    node = node.ChildNodes[p];

            }
            return node;

        }
        /// <summary>
        /// 获取所有实例对象@
        /// </summary>
        /// <!--Fixnum,-->
        /// <param name="list"></param>
        /// <param name="xmlele"></param>
        public void getLinkInstances(TreeNode treeNode)
        {

            //  nodelist = xld.SelectNodes("//*");
            foreach (TreeNode treenode in treeNode.Nodes)
            {
                XmlNode node = (XmlNode)treenode.Tag;
                String nodename = node.Name;
                if (node.NodeType == XmlNodeType.Element && !"link,key,name,fixnum,item,sym,symbol,members".Contains(nodename.ToLower()))
                {
                    Instlist.Add(treenode);

                }
                getLinkInstances(treenode);
            }
        }
        /// <summary>
        /// 获取符号实例
        /// </summary>
        /// <param name="list"></param>
        /// <param name="xmlNode"></param>
        public void getSymbolInstances(List<XmlNode> list, XmlNode xmlNode)
        {


            if (xmlNode.NodeType == XmlNodeType.Element)
            {
                XmlElement xmlele = (XmlElement)xmlNode;
                if (xmlele.Name == "Symbol")
                {
                    list.Add(xmlele);
                    return;
                }

                foreach (XmlNode node in xmlele.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Element)
                        getSymbolInstances(list, node);
                }

            }
            return;
        }
        public void getSymbolInstances(XmlDocument document)
        {
            Symbollist = new List<XmlNode>();

            foreach (XmlNode node in document.SelectNodes("//Symbol"))
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    Symbollist.Add(node);

                }
            }


        }

        public void searchTreeView(TreeNode node, String idn, ListBox listBox)
        {


            foreach (TreeNode nodex in node.Nodes)
            {
                String ntxt = node.Text;
                if (ntxt.Length >= idn.Length)
                {
                    if (ntxt.Contains(idn))
                    {

                        treeNodes.Add(node);
                        listBox.Items.Add(ntxt);



                    }
                }
                else
                {
                    if (idn.Contains(ntxt))
                    {

                        listBox.Items.Add(ntxt);
                        treeNodes.Add(node);

                    }
                }


                searchTreeView(nodex, idn, listBox);
            }
        }
        public void fastsearch(String idn, ListBox listBox)
        {
            if (treeView1.Nodes.Count > 0)
            {
                treeNodes = new List<TreeNode>();
                listBox2.Items.Clear();
                searchTreeView(treeView1.Nodes[0], idn, listBox);
                MessageBox.Show("搜索完成");
            }
        }

        /// <summary>
        /// 关闭当前文件
        /// </summary>
        public void closefile()
        {

            xld = null;
            result = null;
            decoder = null;
            Symbollist = null;
            Instlist = null;
            listBox2.Items.Clear();
            listView2.Items.Clear();
            savepath = "";
            listView1.Items.Clear();
            textBox1.Text = "";
            SToolStripButton.Enabled = false;
            SAToolStripButton.Enabled = false;
            treeView1.Nodes.Clear();
            GC.Collect();
            GC.WaitForPendingFinalizers();

        }
        public void addtoSymbolListView()
        {
            int i = 0;
            foreach (XmlNode sym in Symbollist)
            {

                ListViewItem item = listView1.Items.Add(i.ToString());
                item.SubItems.Add(sym.InnerText);

                i++;
            }
        }
        public void addtoInstListView()
        {
            int i = 0;
            foreach (TreeNode val in Instlist)
            {

                ListViewItem item = listView2.Items.Add(i.ToString());
                item.SubItems.Add(val.Name);
                item.SubItems.Add((val.Tag as XmlNode).InnerText);

                i++;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {

            openFileDialog1.Filter = "rxdata(*.rxdata)|*.rxdata|dat(*.dat)|*.dat|xml文本(*.xml)|*.xml|所有(*.*)|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                closefile();
                this.Text = "RXDATA EDITOR -" + openFileDialog1.SafeFileName;
                if (openFileDialog1.SafeFileName.EndsWith("xml") || openFileDialog1.SafeFileName.EndsWith("txt"))
                {
                   
                    toolStripStatusLabel1.Text = "加载中";
                    SyncContext = SynchronizationContext.Current;
                    String fpath = openFileDialog1.FileName;
                    xld = new XmlDocument();
     
                    toolStrip1.Enabled = false;
                    xld.Load(openFileDialog1.FileName);
                    Func<List<Object>> action = DecodeXml;
                    action.BeginInvoke(finishDecodeXml, action);
                }
                else
                {
                    toolStripStatusLabel1.Text = "加载中";
                    SyncContext = SynchronizationContext.Current;
                    String fpath = openFileDialog1.FileName;
                    xld = new XmlDocument();
                    decoder = new DecoderXml(fpath);
                    decoder.updateAction = getdecodePogress;
                    decoder.context = SynchronizationContext.Current;
                    ver.Text = decoder.Ver;
                    toolStrip1.Enabled = false;

                    mydecode = new decodefile(DecodeFile);

                    mydecode.BeginInvoke(new AsyncCallback(finishDecode), mydecode);
                }
            }

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (searchtextbox.Text != "")
            {
                fastsearch(searchtextbox.Text, listBox2);


            }

        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            splitContainer2.Panel2Collapsed = !splitContainer2.Panel2Collapsed;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (searchtextbox.Text != "")
            {
                fastsearch(searchtextbox.Text, listBox2);


            }
        }

        private void LToolStripButton_Click(object sender, EventArgs e)
        {
            
            AboutForm aboutForm = new AboutForm();

            aboutForm.ShowDialog(this.Location);
        }

        private void SToolStripButton_Click(object sender, EventArgs e)
        {
            if (savepath != "")
            {
                EncoderXml encoder = new EncoderXml(xld);
                savepath = saveFileDialog1.FileName;
                encoder.startEncode(saveFileDialog1.FileName);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                return;
            }
            saveFileDialog1.Filter = "rxdata(*.rxdata)|*.rxdata|dat(*.dat)|*.dat|所有(*.*)|*.*";
            saveFileDialog1.Title = "保存";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.Text = "RXDATA EDITOR -" + saveFileDialog1.FileName;
                EncoderXml encoder = new EncoderXml(xld);
                savepath = openFileDialog1.FileName;
                encoder.startEncode(saveFileDialog1.FileName);
                if (restbytes != null)
                { 
                    FileStream stream = new FileStream(saveFileDialog1.FileName, FileMode.Append, FileAccess.Write);
                    stream.Write(restbytes, 0,restbytes.Length);
                    stream.Close();
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }



        }

        private void SAToolStripButton_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "rxdata(*.rxdata)|*.rxdata|dat(*.dat)|*.dat|xml文本(*.xml)|*.xml|所有(*.*)|*.*";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {

                xld.Save(saveFileDialog1.FileName);

                if (saveFileDialog1.FileName.EndsWith(".xml"))
                {

                    StreamWriter sw = new StreamWriter(saveFileDialog1.FileName, false, System.Text.Encoding.UTF8);

                    sw.Write(result[0]);
                    sw.Flush();
                    sw.Close();
                }
                else

                {
                    savepath = saveFileDialog1.FileName;
                    this.Text = "RXDATA EDITOR -" + saveFileDialog1.FileName;
                    EncoderXml encoder = new EncoderXml(xld);
                    savepath = saveFileDialog1.FileName;
                    encoder.startEncode(saveFileDialog1.FileName);
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private void searchtextbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ((char)Keys.Enter))
            {
                fastsearch(searchtextbox.Text, listBox2);
            }
        }

        private void closetoolStripButton3_Click(object sender, EventArgs e)
        {
            closefile();

        }

        private void searchtextbox_Click(object sender, EventArgs e)
        {

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                XmlNode node = (XmlNode)e.Node.Tag;
                xmlcheckBox1.Enabled = true;
                checkBox1.Checked = false;
                if (xmlcheckBox1.Checked)
                { textBox1.Text = node.OuterXml.Replace("><", ">\r\n<");
                    
                }
                else
                {
                    textBox1.Enabled = false;
                    textBox1.Text = node.InnerXml.Replace("><", ">\r\n<");
                }
                propertyDisp1.node = node;
                propertyDisp1.updateView();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {

            var listView = (sender as ListView);
            if (listView.SelectedItems.Count > 0)
            {
                treeView1.SelectedNode = Instlist[listView.SelectedItems[0].Index];

            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Controls.Count > 1)
                return;
            Button save = new Button();
            save.Text = "保存";
            save.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            save.Dock = DockStyle.Bottom;
  
            textBox1.Controls.Add(save);
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            StringEditor stringEditor = new StringEditor(xld);
            stringEditor.ShowDialog();

        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (treeNodes.Count > listBox2.SelectedIndex)
                treeView1.SelectedNode = treeNodes[listBox2.SelectedIndex];
        }

        private void listView2_SelectedIndexChanged_1(object sender, EventArgs e)
        {

            if (listView2.SelectedIndices.Count > 0 && Instlist.Count > listView2.SelectedIndices[0])
                treeView1.SelectedNode = Instlist[listView2.SelectedIndices[0]];
        }

        private void xmlcheckBox1_CheckedChanged(object sender, EventArgs e)
        {     if (treeView1.SelectedNode == null)
                return;
            XmlNode node = (XmlNode)(treeView1.SelectedNode.Tag);
       
            if (xmlcheckBox1.Checked)
                textBox1.Text = node.OuterXml.Replace("><", ">\r\n<");
            else
                textBox1.Text = node.InnerXml.Replace("><", ">\r\n<");
        }

        private void savebtn_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked)//源码
            {
                ModifyXmlNode(treeView1.SelectedNode);
            }
            else
                propertyDisp1.saveNode();
        }
        private void ModifyXmlNode(TreeNode node)
        { XmlNode xmlnode = (XmlNode)node.Tag;
            node.Nodes.Clear();
            xmlnode.InnerXml = textBox1.Text;
            
            RecursionTreeControl(xmlnode, node);
        }
        private void BatchBtn_Click(object sender, EventArgs e)
        {
           

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            bool check = checkBox1.Checked;
            xmlcheckBox1.Checked = !check;
            xmlcheckBox1.Enabled = !check;
            propertyDisp1.Enabled = !check;
            textBox1.Enabled = check;
        }
    }


}
