using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rxdatadecoder
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    namespace rxdatadecoder
    {
        public class DecoderXml
        {

            private static FileStream stream;
            public String Ver { get; }
            public String log = "";
            public Boolean isparseTable = true;
            private int progress;
            public int Progress { get { return progress; } }
            public Action updateAction, finishAction;
            public long Length { get; }
            public SynchronizationContext context = null;
            public DecoderXml(String path)
            {

                stream = new FileStream(path, FileMode.Open, FileAccess.Read);

                Ver = BytetoHexString(GetVer());
                progress = (int)(stream.Position * 100 / stream.Length);

            }
            private void updateProgress()
            {
                this.progress = (int)(stream.Position * 100 / stream.Length);
                if (updateAction != null && context != null)
                    context.Post((m) => { updateAction(); }, null);

            }
            public Stream GetStream()
            {
                return stream;
            }
            private byte[] GetVer()
            {
                byte[] buff = new byte[2];
                if (stream != null)
                    stream.Read(buff, 0, 2);
                return buff;
            }

            public String BytetoHexString(byte[] bytes)
            {
                String re = "";
                for (int i = 0; i < bytes.Length; i++)
                {
                    re += String.Format("{0:X2}", bytes[i]);
                }
                return re;
            }
            public String startDecode(List<Object> decoderesut)
            {

                String rre = Parse(stream);
                return rre;
            }
            public String Parse(Stream stream)
            {
                String re = "";
                byte t = (byte)stream.ReadByte();
                switch (t)
                {
                    case 0x40:
                        re += decodeReference(stream);//@ 
                        break;
                    case 0x5b:
                        re += decodeArray(stream);//
                        break;
                    case 0x6c:
                        re += decodeBignum(stream);//  
                        break;
                    case 0x63:
                        re += decodeClass(stream);//c ?
                        break;
                    case 0x46:
                        re += decodeFalseClass(stream);//
                        break;
                    case 0x69:
                        re += decodeFixnum(stream);//
                        break;
                    case 0x66:
                        re += decodeFloat(stream);//
                        break;
                    case 0x7b:
                        re += decodeHash(stream);//
                        break;
                    case 0x7d:
                        re += decodeHash2(stream);//} hash 2 
                        break;
                    case 0x6d:
                        re += decodeModule(stream);//
                        break;
                    case 0x4d:
                        re += decodeNilClass(stream);//
                        break;
                    case 0x30:
                        re += decodeNilClass(stream);//
                        break;
                    case 0x2f:
                        re += decodeRegexp(stream);//

                        break;
                    case 0x22:
                        re += decodeString(stream);//
                        break;
                    case 0x3a:
                        re += decodeSymbol(stream);//
                        break;
                    case 0x3b:
                        re += decodeLinkSymbol(stream);//

                        break;
                    case 0x54:
                        re += decodeTrueClass(stream);//
                        break;
                    case 0x53:
                        re += decodeStruct(stream);//
                        break;
                    case 0x6f:
                        re += decodeObject(stream);
                        break;
                    case 0x65:
                        re += decodeExtend(stream);//e extend module
                        break;
                    case 0x43:
                        re += decodeClass2(stream);//C
                        break;
                    case 0x49:
                        re += decodeI(stream);//I
                        break;
                    case 0x75:
                        re += decodeUser_dump(stream);//u _dump
                        break;
                    case 0x55:
                        re += decodeUserMarshal_dump(stream);//U: marshal_dump
                        break;

                }
                updateProgress();

                return re;

            }
            public String Parse(Stream stream, int index)
            {
                String re = "";

                switch (index)
                {
                    case 0x40:
                        re += decodeReference(stream);

                        break;
                    case 0x5b:
                        re += decodeArray(stream);//
                        break;
                    case 0x6c:
                        re += decodeBignum(stream);//
                        break;
                    case 0x63://?
                        re += decodeClass(stream);
                        break;
                    case 0x46:
                        re += decodeFalseClass(stream);//
                        break;
                    case 0x69:
                        re += decodeFixnum(stream);//
                        break;
                    case 0x66://
                        re += decodeFloat(stream);
                        break;
                    case 0x7b:
                        re += decodeHash(stream);
                        break;
                    case 0x7d:
                        re += decodeHash2(stream);
                        break;
                    case 0x6d:
                        re += decodeModule(stream);
                        break;
                    case 0x4d:
                        re += decodeNilClass(stream);
                        break;
                    case 0x30:
                        re += decodeNilClass(stream);
                        break;
                    case 0x2f:
                        re += decodeRegexp(stream);

                        break;
                    case 0x22:
                        re += decodeString(stream);
                        break;
                    case 0x3a:
                        re += decodeSymbol(stream);
                        break;
                    case 0x3b:
                        re += decodeLinkSymbol(stream);

                        break;
                    case 0x54:
                        re += decodeTrueClass(stream);
                        break;
                    case 0x53:
                        re += decodeStruct(stream);
                        break;
                    case 0x6f:
                        re += decodeObject(stream);
                        break;
                    case 0x65://e
                        re += decodeExtend(stream);
                        break;
                    case 0x43://C
                        re += decodeClass2(stream);
                        break;
                    case 0x49://I
                        re += decodeI(stream);
                        break;
                    case 0x75://u: _dump
                        re += decodeUser_dump(stream);
                        break;
                    case 0x55://U: marshal_dump
                        re += decodeUserMarshal_dump(stream);
                        break;

                }
                updateProgress();
                return re;

            }

            public String decodeArray(Stream stream)
            {
                int len = readLength(stream);
                String re = "";
                for (int i = 0; i < len; i++)
                {
                    re += Parse(stream);
                }
                return addTag("Array").Replace("%c", re);

            }
            public String decodeHash(Stream stream)
            {
                int len = readLength(stream);
                String re = "";
                for (int i = 0; i < len; i++)
                {
                    String item = addTag("Item", addTag("Key", Parse(stream)) + addTag("Value", Parse(stream)));
                    re += item;
                }
                return addTag("Hash").Replace("%c", re);

            }
            public String decodeBignum(Stream stream)
            {
                int sign = stream.ReadByte();
                int len = (int)readLength(stream) + 5;
                if (len <= 0xff - 5)
                    len = 2 * (len - 0x5);

                byte[] bytes = new byte[len];
                String re = "";
                for (int i = len - 1; i >= 0; i--)
                {
                    bytes[i] = (byte)stream.ReadByte();

                }
                foreach (byte b in bytes)
                {
                    re += String.Format("{0:x2}", b);
                }

                return addProperty(addTag("Bignum", "0x" + re), "s", ((char)sign).ToString());
            }
            public String decodeFalseClass(Stream stream)
            {

                return "<F/>";
            }
            public String decodeFixnum(Stream stream)
            {
                sbyte b = (sbyte)stream.ReadByte();

                String re = "";
                if (b >= -128 && b <= -5)
                {
                    b = (sbyte)(b + 5);
                    re += String.Format("{0:d}", b);

                }
                else if (b >= 5 && b <= 127)
                {
                    re += String.Format("{0:d}", b - 5);

                }
                else if (b == 0)
                {
                    re += "0";
                    return addTag("Fixnum", re);
                }
                else if (b >= -4 && b <= -1)
                {
                    int len;
                    ///sign = "-";
                    len = Math.Abs(b);
                    byte[] bytes = new byte[4] { 0xff, 0xff, 0xff, 0xff };

                    for (int i = 0; i < len; i++)
                    {
                        bytes[i] = (byte)stream.ReadByte();

                    }
                    re = String.Format("{0:d}", BitConverter.ToInt32(bytes, 0));



                }

                else if (b >= 1 && b <= 4)
                {
                    int len;
                    ///sign = "+";
                    len = Math.Abs(b);
                    byte[] bytes = new byte[4] { 0, 0, 0, 0 };

                    for (int i = 0; i < len; i++)
                    {
                        bytes[i] = (byte)stream.ReadByte();

                    }
                    re = String.Format("{0:d}", BitConverter.ToInt32(bytes, 0));
                }



                return addTag("Fixnum", re);
            }
            public String decodeNilClass(Stream stream)
            { return "<N/> "; }
            public String decodeString(Stream stream)
            {

                int len = (int)readLength(stream);
                byte[] buff = new byte[len];
                stream.Read(buff, 0, len);
                String re = System.Text.Encoding.UTF8.GetString(buff).Replace(">", "\\x3E").Replace("<", "\\x3C").Replace("&", "\\x26");
                return addTag("String", re);
            }
            public String decodeSymbol(Stream stream)
            {
                int len = readLength(stream);
                byte[] buff = new byte[len];
                stream.Read(buff, 0, len);
                return addTag("Symbol", System.Text.Encoding.UTF8.GetString(buff));
            }
            public String decodeLinkSymbol(Stream stream)
            {
                String re = "";
                re += addTag("sym", readLength(stream).ToString());
                return re;
            }
            public String decodeLinkSymbolx(Stream stream, Boolean hastag)
            {
                String re = "";
                if (!hastag)
                    re += "sym:" + readLength(stream).ToString();
                else
                    re += addTag("sym", readLength(stream).ToString());
                return re;
            }
            public String decodeTrueClass(Stream stream)
            {
                //stream.ReadByte();
                return "<T/>";
            }
            public String decodeModule(Stream stream)
            {
                return addTag("Module", decodeObjectName(stream));

            }
            public String decodeFloat(Stream stream)
            {
                int len = (int)readLength(stream);
                byte[] buff = new byte[len];
                stream.Read(buff, 0, len);
                return addTag("Float", System.Text.Encoding.UTF8.GetString(buff));
            }
            public String decodeRegexp(Stream stream)
            {
                int len = (int)readLength(stream);
                byte[] buff = new byte[len];
                stream.Read(buff, 0, len);
                String options = String.Format(" options=\"{0:x2}\"", stream.ReadByte());
                return addTag("Regexp").Replace(">%c", options + ">%c").Replace("%c", System.Text.Encoding.UTF8.GetString(buff));
            }
            public String decodeReference(Stream stream)
            {

                int key = readLength(stream);
                return addTag("Link", key.ToString());
            }
            public String decodeHash2(Stream stream)
            {
                int b = readLength(stream);
                if (b == 0)
                {
                    return addTag("Hash", addTag("defaultValue", Parse(stream)));
                }
                else
                {
                    int len = readLength(stream);
                    String re = "";
                    for (int i = 0; i < len; i++)
                    {
                        String item = addTag("Item", addTag("Key", Parse(stream)) + addTag("Value", Parse(stream)));
                        re += item;
                    }
                    return addTag("Hash").Replace("%c", re + addTag("defaultValue", Parse(stream)));
                }

            }
            public String decodeStruct(Stream stream)
            {
                String re = "", name = "";
                int id = stream.ReadByte();//:;
                name = addTag("name", Parse(stream, id));
                re += name;
                int len = readLength(stream);
                String members = "";
                for (int i = 0; i < len; i++)
                {
                    int b = stream.ReadByte();//:
                    if (b == ':' || b == ';')
                        members += addTag("Item", Parse(stream, b) + Parse(stream));
                    else
                    {
                        MessageBox.Show("Error at decodeStruct");
                    }
                }
                re += addTag("members", members);
                return addTag("Struct", re);
            }
            public String decodeExtend(Stream stream)
            {
                String re = "", ex = "";
                int s = stream.ReadByte();
                if (s == ':' || s == ';')
                    ex = Parse(stream, s);
                else
                {


                    return "Error key";
                }
                re += Parse(stream);

                return addTag("Extend", ex + re);
            }

            //没有完全测试
            public String decodeObject(Stream stream)
            {
                int b = stream.ReadByte();
                String name = "", re = "", item = "";
                if (b == ':' || b == ';')
                {
                    name = Parse(stream, b);

                    //name = decodeLinkSymbol(stream, false);
                    int len = readLength(stream);
                    for (int i = 0; i < len; i++)
                    {
                        item += addTag("Item", Parse(stream) + Parse(stream));
                    }
                }
                else
                {
                    MessageBox.Show("Error at decodeObject");
                    re = "error";
                    name = "error";
                }

                return addTag("Object", addTag("name", name) + addTag("Members", item));

            }
            public String decodeI(Stream stream)
            {
                //C s u c
                String re = "";
                int index = stream.ReadByte();
                if (index == 'C')
                {
                    re += addTag("I", Parse(stream, index));//decodeClass


                    return re;
                }
                else
                {//不完全正确
                    re += Parse(stream, index);
                    int len = readLength(stream);
                    for (int i = 0; i < len; i++)
                    {
                        re += Parse(stream);
                    }
                    re += Parse(stream);
                    //if(index!='"')
                    //	MessageBox.Show(BytetoHexString(new byte[] { (byte)index }));
                    return addTag("I").Replace("%c", re);
                }
                /*else {

					MessageBox.Show("I");
					return "";
				}*/
            }
            public String decodeClass2(Stream stream)
            {
                int b = stream.ReadByte();
                String name = "", re = "", item = "";
                if (b == ':' || b == ';')
                {
                    name = Parse(stream, b);
                    re = Parse(stream);

                    int len = readLength(stream);
                    for (int i = 0; i < len; i++)
                    {
                        item += addTag("item", Parse(stream) + Parse(stream));
                    }

                }
                else
                {
                    //MessageBox.Show("key index"+BytetoHexString(new byte[] { (byte)b }));
                    re = "error";
                    name = "error";
                }

                return addTag("Class", re + addTag("members", item));

            }
            public String decodeClass(Stream stream)
            {
                MessageBox.Show("decodeClass hasn't completed");
                return "";
            }
            public String decodeUser_dump(Stream stream)
            {

                int b = stream.ReadByte();
                String name = "";
                if (b == ';' || b == ':')
                    name = Parse(stream, b);
                int len = readLength(stream);

                /*if (name == "Table")
				{
					
				}

				else if (name == "Tone")*/

                byte[] bytes = new byte[len];
                stream.Read(bytes, 0, len);
                return addTag("Userdump", name + addTag("data", BytetoHexString(bytes)));
            }
            public String decodeUserMarshal_dump(Stream stream)
            {

                MessageBox.Show("User MarshalDump");
                return "";
            }
            public String decodeTable(Stream stream, int len)
            {
                /*
					writeInt32(&buffer, dim);//01 00 00 00 ess map中有03 00 00 00
					writeInt32(&buffer, xs);
					writeInt32(&buffer, ys);
					writeInt32(&buffer, zs);
					writeInt32(&buffer, size); ==xs*ys*zs

					metadata=data[xs*ys*z + xs*y + x];==>数据从x到y到z  1,0,0=x,0,1,0=xs*y
					{[0,0,0],[1,0,0],[2,0,0].....[xs-1,ys-1,zs-1]}
				 */
                int dim = readInt32(stream);
                int xsize = readInt32(stream);
                int ysize = readInt32(stream);
                int zsize = readInt32(stream);
                int size = readInt32(stream);
                if (len - 20 != xsize * ysize * zsize * 2)
                {

                    MessageBox.Show("Format Error wehen decoding Table" + size.ToString() + "Len" + len);

                }
                byte[] buf = new byte[len - 20];
                stream.Read(buf, 0, len - 20);
                if (!isparseTable)
                {

                    return addProperty(addProperty(addProperty(addTag("Table", BytetoHexString(buf)), "xsize", xsize.ToString()), "ysize", ysize.ToString()), "zsize", zsize.ToString());
                }
                return addProperty(addProperty(addProperty(addTag("Table", parseTable(buf, xsize, ysize, zsize)), "xsize", xsize.ToString()), "ysize", ysize.ToString()), "zsize", zsize.ToString()); ;

            }
            public String decodeObjectName(Stream stream)
            {
                int len = readLength(stream);
                byte[] buff = new byte[Math.Abs(len)];
                stream.Read(buff, 0, len);
                return System.Text.Encoding.UTF8.GetString(buff);
            }
            public int readLength(Stream stream)
            {
                int temp = Math.Abs(stream.ReadByte());
                if (temp == 0)
                {
                    return temp;
                }
                if (temp <= 5)
                {
                    byte[] bytes = new byte[temp];
                    String hexnum = "";
                    for (int i = temp - 1; i >= 0; i--)
                    {
                        bytes[i] = (byte)stream.ReadByte();

                    }
                    foreach (byte b in bytes)
                    {
                        hexnum += String.Format("{0:x2}", b);
                    }

                    int len = 0; ;
                    len = int.Parse(hexnum, System.Globalization.NumberStyles.HexNumber);
                    return len;
                }
                return Math.Abs(temp - 5);

            }
            public String addTag(String tagName)
            {
                return "<" + tagName + ">%c" + "</" + tagName + ">";
            }
            public String addTag(String tagName, String content)
            {
                return "<" + tagName + ">" + content + "</" + tagName + ">";
            }
            public String addProperty(String tag, String name, String value)
            {

                return tag.Insert(tag.IndexOf('>'), " " + name + "=\"" + value + "\"");
            }
            public String addProperty(String tag, String name, String value, Boolean isappend)
            {
                if (tag.Contains(name))
                {
                    int ind = tag.IndexOf(name + "=\"") + name.Length + 2;
                    if (isappend)
                    {

                        return tag.Insert(ind, value + "|");
                    }


                }

                return tag.Insert(tag.IndexOf('>'), " " + name + "=\"" + value + "\"");
            }
            public Int32 readInt32(Stream stream)
            {

                byte[] bytes = new byte[4];

                for (int i = 0; i < 4; i++)
                {
                    bytes[i] = (byte)stream.ReadByte();

                }
                return BitConverter.ToInt32(bytes, 0);


            }
            public String addSubtag(String tag, String sub, String value)
            {

                return tag.Insert(tag.IndexOf('>') + 1, addTag(sub).Replace("%c", value));
            }
            public String parseTable(byte[] data, int xs, int ys, int zs)
            {
                String re = "", ystr = "";
                for (int z = 0; z < zs; z++)
                {
                    String zss = "";
                    for (int y = 0; y < ys; y++)
                    {
                        String yss = "";
                        for (int x = 0; x < xs; x++)
                        {
                            String xss;
                            xss = addTag("x", String.Format("{0:X2}", data[xs * ys * z + xs * y + x]));
                            yss += xss;
                        }
                        ystr += addTag("y", yss);
                    }
                    zss += addTag("z", ystr);
                    re += zss;
                }
                return re;

            }
        }
    }


}
