using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using qint32 = System.Int32;
using QString = System.String;

namespace ConsoleApplication1
{
    public class GbxParser : System.IO.BinaryReader
    {
        public int position { get { return (int)BaseStream.Position; } set { BaseStream.Position = value; } }
        public GbxParser(Stream input)
            : base(input)
        {
        }
        public GbxParser(Stream input, Encoding encoding)
            : base(input, encoding)
        {
        }
    
        public void Skip(int P0)
        {
            BaseStream.Position += P0;            
        }
        public byte[] GetChars(int Start, int L)
        {
            BaseStream .Position = Start;
            return ReadBytes(L);
        }
        public long Length()
        {
            return BaseStream.Length;
        }
        public override QString ReadString()
        {
            qint32 len = ReadInt32();
            return Encoding.UTF8.GetString(ReadBytes(len));
        }
        public QString ReadStringLB()
        {
            unchecked
            {
                if (SeenLoopback == false)
                {
                    Debug.Log("-> Lookbackstring version " + this.ReadInt32());
                }
                SeenLoopback = true;
                qint32 inp = ReadInt32();
                if ((inp & 0xc0000000) != 0 && (inp & 0x3fffffff) == 0)
                {
                    QString str = ReadString();
                    StoredStrings.Add(str);
                    return str;
                }
                if (inp == 0)
                {
                    QString str = ReadString();

                    StoredStrings.Add(str);
                    return str;
                }
                if (inp == -1)
                    return "";
                if ((inp & 0x3fffffff) == inp)
                {
                    // The string could be in the predefined libary...
                    switch (inp)
                    {
                        case 11:
                            return "Valley";
                        case 12:
                            return "Canyon";
                        case 17:
                            return "TMCommon";
                        case 202:
                            return "Storm";
                        case 299:
                            return "SMCommon";
                        case 10003:
                            return "Common";
                    }
                }
                inp &= 0x3fffffff;
                if (inp - 1 >= StoredStrings.Count)
                {
                    Debug.Log("WARNING: String not found in list! Error: LBS " + inp + " in " + StoredStrings.Count + " not found");
                    return "{{{ Error: LBS " + inp + " in " + StoredStrings.Count + " not found }}}";
                }
                return StoredStrings[inp - 1];
            }
        }
        private List<QString> StoredStrings = new List<QString>();
        private bool SeenLoopback;
    
        public void PushStringLB(QString what)
        {
            StoredStrings.Add(what);
        }
        public void ResetStringLB()
        {
            StoredStrings.Clear();
            SeenLoopback = false;
        }

    }

}