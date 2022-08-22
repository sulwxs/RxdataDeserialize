using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace rxdataencoder
{
    public class encoderXml
    {
        public int TYPE_PATH = 0, TYPE_XMLSTRING = 1;
        public XmlDocument xmlDocument;
        private FileStream stream;
        private List<byte> list;
        public encoderXml(String arg, int type)
        {
            xmlDocument = new XmlDocument();
            if (type == TYPE_XMLSTRING)
            {
                xmlDocument.LoadXml(arg);
            }
            else
            {
                xmlDocument.Load(arg);
            }
        }
        public encoderXml(XmlDocument xml)
        {
            xmlDocument = xml;
        }
        public void startEncode(String fileoutput)
        {
            //list = new Filelist(fileoutput,FileMode.OpenOrCreate,FileAccess.Write);
            //list.Seek(0,SeekOrigin.Begin);
            list = new List<byte>();
            list.Add(0x04);
            list.Add(0x08);
            encode(list, xmlDocument.DocumentElement);
            FileStream stream = new FileStream(fileoutput, FileMode.Create, FileAccess.Write);
            stream.Write(list.ToArray(), 0, list.Count);
            stream.Close();

        }
        public void encode(List<byte> list, XmlNode node)
        {
            switch (node.Name)
            {
                case "Link"://@
                    encodeReference(list, node);
                    break;
                case "Array":
                    encodeArray(list, node);//
                    break;
                case "Hash":
                    encodeHash(list, node);//
                    break;
                case "Bignum":
                    encodeBignum(list, node);//
                    break;

                case "Class":
                    encodeClass(list, node);//c ?
                    break;
                case "F":
                    encodeFalseClass(list);//
                    break;
                case "Fixnum":
                    encodeFixnum(list, node);//
                    break;
                case "Float":
                    encodeFloat(list, node);//
                    break;

                case "sym"://;
                    encodeLinkSymbol(list, node);
                    break;

                case "Module":
                    encodeModule(list, node);//
                    break;
                case "N":
                    encodeNilClass(list);//
                    break;
                case "NilClass2":
                    encodeNilClass(list);//??
                    break;
                case "Regexp":
                    encodeRegexp(list, node);//

                    break;
                case "String":
                    encodeString(list, node);//
                    break;
                case "Symbol":
                    encodeSymbol(list, node);//?
                    break;
                case "LinkSymbol":
                    encodeLinkSymbol(list, node);//

                    break;
                case "T":
                    encodeTrueClass(list);//
                    break;
                case "Struct":
                    encodeStruct(list, node);//
                    break;
                case "Object":
                    encodeObject(list, node);//
                    break;
                case "Extend":
                    encodeExtend(list, node);//
                    break;
                case "Class2":
                    encodeClass2(list, node);//C
                    break;
                case "I":
                    encodeI(list, node);//I
                    break;
                case "Userdump":
                    encodeUser_dump(list, node);//u: _dump
                    break;
                case "UserMarshal_dump":
                    encodeUserMarshal_dump(list);//U: marshal_dump
                    break;


            }
        }
        public void encodeArray(List<byte> list, XmlNode node)
        {
            list.Add(0x5b);//[
            XmlNodeList nlist = node.ChildNodes;//item 
            int len = nlist.Count;
            encodeLen(list, len);
            foreach (XmlNode xmlNode in nlist)
            {
                encode(list, xmlNode);
            }

        }
        public void encodeLen(List<byte> list, int len)
        {
            if (len == 0)
            {
                list.Add(0x00);
            }
            else if (len <= 122)
            {
                list.Add((byte)(len + 5));
            }
            else
            {
                byte[] bytes = BitConverter.GetBytes(len);
                List<byte> numl = new List<byte>();
                for (int i = bytes.Length - 1; i >= 0; i--)
                {
                    byte b = bytes[i];
                    if (b != 0x00)
                    {
                        numl.AddRange(bytes.Take(i + 1));
                        break;
                    }

                }
                if (numl.Count == 0)
                    numl.Add(bytes[0]);

                list.Add((byte)numl.Count);
                list.AddRange(numl);

            }

        }

        public void encodeHash(List<byte> list, XmlNode node)
        {
            if (node.SelectNodes("defaultValue").Count > 0)
            {
                list.Add(0x7d);//}

                XmlNodeList nlist = node.SelectNodes("Item");//item 
                int len = nlist.Count;
                encodeLen(list, len);
                foreach (XmlNode xmlNode in nlist)
                {
                    encode(list, xmlNode.FirstChild.FirstChild);//key/v
                    encode(list, xmlNode.LastChild.FirstChild);//value/v
                }
                XmlNode defaultv = node.SelectSingleNode("defaultValue").FirstChild;
                encode(list, defaultv);

            }
            else
            {
                list.Add(0x7b);//{
                XmlNodeList nlist = node.ChildNodes;//item 
                int len = nlist.Count;
                encodeLen(list, len);
                foreach (XmlNode xmlNode in nlist)
                {
                    encode(list, xmlNode.FirstChild.FirstChild);
                    encode(list, xmlNode.LastChild.FirstChild);
                }

            }



        }
        public void encodeFixnum(List<byte> list, XmlNode node)
        {
            String nums = node.InnerText;
            list.Add(0x69);
            int num = int.Parse(node.InnerText);

            if (num >= 1 && num <= 122)
            {
                list.Add((byte)(num + 5));
                return;
            }
            else if (num == 0)
            {
                list.Add(0x00);
                return;
            }
            else if (num <= -1 && num >= -123)
            {
                list.Add((byte)(num - 5));
                return;
            }
            byte[] bytes = BitConverter.GetBytes(num);
            List<byte> numl = new List<byte>();
            for (int i = 0; i < 4; i++)
            {
                byte b = bytes[i];
                if (b != 0xff && b != 0x00)
                    numl.Add(b);
                if (i == 3 && numl.Count < 1)
                    numl.Add(b);

            }
            if (num < -123)
            {
                sbyte len = (sbyte)(numl.Count * (-1));
                list.Add((byte)len);
                list.AddRange(numl);
            }
            else if (num > 122)
            {
                list.Add((byte)numl.Count);
                list.AddRange(numl);
            }





        }


        public void encodeFalseClass(List<byte> list)
        {
            list.Add((byte)'F');
        }
        public void encodeBignum(List<byte> list, XmlNode node)
        {
            list.Add(0x6c);
            String nums = node.InnerText.Replace("0x", "");
            byte s = (byte)((node as XmlElement).GetAttribute("s").Contains("-") ? '-' : '+');
            list.Add(s);
            int len = nums.Length;
            if (len % 2 != 0)//奇数
            {
                len++;

                nums.Insert(0, "0");

            }
            if ((len / 2) % 2 != 0)
            {
                len += 2;
                nums.Insert(0, "00");

            }
            if (len / 2 < 0xff - 5)
                len = len / 4;
            encodeLen(list, len);
            list.AddRange(getStringNumBytes(nums));


        }
        public void encodeNilClass(List<byte> list)
        {
            list.Add((byte)'0');
        }
        public void encodeString(List<byte> list, XmlNode node)
        {

            list.Add(0x22);
            byte[] bytes = Encoding.UTF8.GetBytes(node.InnerText.Replace("\\x3C", "<").Replace("\\x3e", ">").Replace("\\x26", "&"));
            int len = bytes.Length;
            encodeLen(list, len);
            list.AddRange(bytes);
        }
        public void encodeSymbol(List<byte> list, XmlNode node)
        {
            list.Add(0x3a);
            byte[] bytes = Encoding.UTF8.GetBytes(node.InnerText);
            int len = bytes.Length;
            encodeLen(list, len);
            list.AddRange(bytes);
        }
        public void encodeLinkSymbol(List<byte> list, XmlNode node)
        {
            list.Add(0x3b);
            String s = node.InnerText;
            if (s.StartsWith("0x"))
            {
                s = s.Remove(0, 2);
                encodeLen(list, int.Parse(s, System.Globalization.NumberStyles.HexNumber));
            }
            else
            { encodeLen(list, int.Parse(s)); }

        }
        public void encodeTrueClass(List<byte> list)
        {
            list.Add((byte)'T');
            //list.ReadByte();

        }
        public void encodeModule(List<byte> list, XmlNode node)
        {

            list.Add(0x6d);
            byte[] bytes = Encoding.UTF8.GetBytes(node.InnerText);
            int len = bytes.Length;
            encodeLen(list, len);
            list.AddRange(bytes);

        }
        public void encodeFloat(List<byte> list, XmlNode node)
        {
            list.Add(0x66);

            byte[] bytes = Encoding.UTF8.GetBytes(node.InnerText);
            int len = bytes.Length;
            encodeLen(list, len);
            list.AddRange(bytes);

        }
        public void encodeRegexp(List<byte> list, XmlNode node)
        {
            list.Add(0x2f);
            byte[] bytes = Encoding.UTF8.GetBytes(node.InnerText);
            int len = bytes.Length;
            encodeLen(list, len);
            list.AddRange(bytes);

            XmlElement el = (XmlElement)node;
            String opt = el.GetAttribute("options");
            if (opt == "")
            { MessageBox.Show("error"); }
            list.Add((byte)(byte.Parse(opt, System.Globalization.NumberStyles.HexNumber)));
        }
        public void encodeReference(List<byte> list, XmlNode node)
        {
            list.Add(0x40);
            String s = node.InnerText;
            if (s.StartsWith("0x"))
            {
                s = s.Remove(0, 2);
                encodeLen(list, int.Parse(s, System.Globalization.NumberStyles.HexNumber));
            }
            else { encodeLen(list, int.Parse(s)); }

        }

        public void encodeStruct(List<byte> list, XmlNode node)
        {

            list.Add(0x53);
            XmlNode name = node.FirstChild.FirstChild;
            XmlNode members = node.LastChild;

            encode(list, name);
            encodeLen(list, members.ChildNodes.Count);
            int len = members.ChildNodes.Count;

            for (int i = 0; i < len; i++)
            {
                encode(list, members.ChildNodes[i].FirstChild);
                encode(list, members.ChildNodes[i].LastChild);

            }
        }
        public void encodeExtend(List<byte> list, XmlNode node)
        {
            list.Add(0x65);
            if (node.ChildNodes.Count > 2)
                MessageBox.Show("Eror");
            encode(list, node.FirstChild);
            encode(list, node.LastChild);


        }

        public void encodeObject(List<byte> list, XmlNode node)
        {

            list.Add(0x6f);
            XmlNode namenode = node.FirstChild.FirstChild;
            encode(list, namenode);
            XmlNode meembs = node.LastChild;
            int len = meembs.ChildNodes.Count;
            encodeLen(list, len);
            for (int i = 0; i < len; i++)
            {
                XmlNode itemn = meembs.ChildNodes[i].FirstChild;
                XmlNode itemv = meembs.ChildNodes[i].LastChild;


                encode(list, itemn);
                encode(list, itemv);
            }
        }


        public void encodeI(List<byte> list, XmlNode node)
        {
            //C s u c
            list.Add(0x49);
            XmlNode child = node.FirstChild;
            if (child.Name == "Class")
            {
                encodeClass2(list, child);//decodeClass
            }
            else
            {//不完全正确
                encode(list, node.FirstChild);
                int len = node.ChildNodes.Count - 2;
                encodeLen(list, len);
                for (int i = 0; i < len; i++)
                {
                    encode(list, node.ChildNodes[i + 1]);
                }
                encode(list, node.LastChild);

            }

        }
        public void encodeClass2(List<byte> list, XmlNode node)
        {
            list.Add(0x43);
            XmlNode namenode = node.FirstChild.FirstChild;
            encode(list, namenode);
            XmlNode meembs = node.LastChild;
            int len = meembs.ChildNodes.Count;
            encodeLen(list, len);
            for (int i = 0; i < len; i++)
            {
                XmlNode itemn = meembs.ChildNodes[i].FirstChild;
                XmlNode itemv = meembs.ChildNodes[i].LastChild;
                encode(list, itemn);
                encode(list, itemv);
            }

        }
        public void encodeClass(List<byte> list, XmlNode node)
        {
            list.Add(0x6);
            XmlNode namenode = node.FirstChild.FirstChild;
            encode(list, namenode);
        }
        public void encodeUser_dump(List<byte> list, XmlNode node)
        {
            list.Add(0x75);

            encode(list, node.FirstChild);
            //MessageBox.Show("len:"+len.ToString());


            String hex = node.LastChild.InnerText;
            byte[] hexbs = getStringHexBytes(hex);
            encodeLen(list, hexbs.Length);
            list.AddRange(hexbs);

        }
        public void encodeUserMarshal_dump(List<byte> list)
        {


        }
        public void encodeTable(List<byte> list, int len)
        {

        }
        public void encodeObjectName(List<byte> list, String name)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(name);
            int len = bytes.Length;
            encodeLen(list, len);
            list.AddRange(bytes);
        }
        public void encodeTable(List<byte> list, XmlNode node)
        {


        }
        public byte[] getStringNumBytes(String numstr)
        {
            int len = numstr.Length / 2;
            byte[] bs = new byte[len];

            for (int i = 0; i < len; i++)
            {
                bs[len - i - 1] = byte.Parse(numstr.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return bs;
        }
        public byte[] getStringHexBytes(String hexstr)
        {

            if (hexstr.Length % 2 != 0)
                hexstr.Insert(0, "0");
            int len = hexstr.Length / 2;
            byte[] bs = new byte[len];

            for (int i = 0; i < len; i++)
            {
                bs[i] = byte.Parse(hexstr.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return bs;
        }
    }
}
