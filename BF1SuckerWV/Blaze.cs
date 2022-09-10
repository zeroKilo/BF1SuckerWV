using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BF1SuckerWV
{
    public static class Blaze
    {
        public enum Component
        {
            Authentication      = 0x0001,
            GameManager         = 0x0004,
            Redirector          = 0x0005,
            Stats               = 0x0007,
            Util                = 0x0009,
            Messaging           = 0x000F,
            AssociationLists    = 0x0019,
            GameReporting       = 0x001C,
            Inventory           = 0x0803,
            LicenseManager      = 0x0806,
            UserSession         = 0x7802,
        }

        public class Packet
        {
            public ushort Unknown1;
            public ushort Component;
            public ushort Command;
            public uint ID;
            public byte Unknown2;
            public ushort QType;
            public List<TDF> payload = new List<TDF>();
            public Packet(Stream s)
            {
                uint payloadSize = Helper.ReadU32(s);
                Unknown1 = Helper.ReadU16(s);
                Component = Helper.ReadU16(s);
                Command = Helper.ReadU16(s);
                uint combine = Helper.ReadU32(s);
                ID = combine >> 8;
                Unknown2 = (byte)combine;
                QType = Helper.ReadU16(s);
                long start = s.Position;
                while(s.Position < start + payloadSize && s.Position < s.Length)
                    payload.Add(TDF.ReadTDF(s));
            }

            public Packet(ushort unk1, ushort comp, ushort cmd, uint id, byte unk2, ushort qtype)
            {
                Unknown1 = unk1;
                Component = comp;
                Command = cmd;
                ID = id;
                Unknown2 = unk2;
                QType = qtype;
            }

            public string Print()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Unk1         = 0x" + Unknown1.ToString("X4"));
                sb.AppendLine("Component    = " + (Component)Component);
                sb.AppendLine("Command      = 0x" + Command.ToString("X4"));
                sb.AppendLine("Unk2         = 0x" + Unknown2.ToString("X4"));
                sb.AppendLine("ID           = 0x" + ID.ToString("X4"));
                sb.AppendLine("QType        = 0x" + QType.ToString("X4"));
                foreach (TDF tdf in payload)
                    sb.Append(tdf.Print(1));
                return sb.ToString();
            }

            public byte[] MakePacket()
            {
                MemoryStream m = new MemoryStream();
                foreach (TDF tdf in payload)
                    tdf.Write(m);
                byte[] data = m.ToArray();
                MemoryStream result = new MemoryStream();
                Helper.WriteU32(result, (uint)data.Length);
                Helper.WriteU16(result, Unknown1);
                Helper.WriteU16(result, Component);
                Helper.WriteU16(result, Command);
                uint combine = (ID << 8) | Unknown2;
                Helper.WriteU32(result, combine);
                Helper.WriteU16(result, QType);
                result.Write(data, 0, data.Length);
                return result.ToArray();
            }
        }

        public abstract class TDF
        {
            public enum TDFType
            {
                Integer = 0,
                String = 1,
                Blob = 2,
                Struct = 3,
                List = 4,
                Dictionary = 5,
                Union = 6,
                IntegerArray = 7,
                Tuple2 = 8,
                Tuple3 = 9,
            }
            public class TDFHeader
            {
                public string TAG;
                public TDFType Type;

                public TDFHeader(string tag, TDFType t)
                {
                    TAG = tag;
                    Type = t;
                }
                public TDFHeader(Stream s)
                {
                    uint header = Helper.ReadU32(s);
                    uint tag = header & 0xFFFFFF00;
                    TAG = Helper.TagToLabel(tag);
                    Type = (TDFType)(header & 0xFF);
                }

                public void Write(Stream s)
                {
                    byte[] tag = Helper.Label2Tag(TAG);
                    s.Write(tag, 0, 3);
                    s.WriteByte((byte)Type);
                }
            }

            public TDFHeader header;
            public object data;
            public static TDF ReadTDF(Stream s)
            {
                TDFHeader h = new TDFHeader(s);
                switch(h.Type)
                {
                    case TDFType.Integer:
                        return new TDFInteger(h, s);
                    case TDFType.String:
                        return new TDFString(h, s);
                    case TDFType.Blob:
                        return new TDFBlob(h, s);
                    case TDFType.Struct:
                        return new TDFStruct(h, s);
                    case TDFType.List:
                        return new TDFList(h, s);
                    case TDFType.Dictionary:
                        return new TDFDictionary(h, s);
                    case TDFType.Union:
                        return new TDFUnion(h, s);
                    case TDFType.IntegerArray:
                        return new TDFIntegerArray(h, s);
                    case TDFType.Tuple2:
                        return new TDF2Tuple(h, s);
                    case TDFType.Tuple3:
                        return new TDF3Tuple(h, s);
                    default:
                        throw new NotImplementedException();
                }
            }

            public abstract void Write(Stream s);

            public abstract string Print(int tabs);
        }

        public class TDFInteger : TDF
        {
            public TDFInteger(string tag, long value)
            {
                header = new TDFHeader(tag, TDFType.Integer);
                data = value;
            }
            public TDFInteger(TDFHeader h, Stream s)
            {
                header = h;
                data = Helper.DecompressInteger(s);
            }

            public override void Write(Stream s)
            {
                header.Write(s);
                Helper.CompressInteger((long)data, s);
            }

            public override string Print(int tabs)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(Helper.MakeTabs(tabs) + header.TAG + " = " + (long)data + " (0x" + ((long)data).ToString("X") + ")");
                return sb.ToString();
            }
        }

        public class TDFString : TDF
        {
            public TDFString(string tag, string value)
            {
                header = new TDFHeader(tag, TDFType.String);
                data = value;
            }
            public TDFString(TDFHeader h, Stream s)
            {
                header = h;
                data = Helper.ReadBlazeString(s);
            }
            public override void Write(Stream s)
            {
                header.Write(s);
                Helper.WriteBlazeString(s, (string)data);
            }

            public override string Print(int tabs)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(Helper.MakeTabs(tabs) + header.TAG + " = " + data);
                return sb.ToString();
            }
        }

        public class TDFBlob : TDF
        {
            public TDFBlob(TDFHeader h, Stream s)
            {
                header = h;
                data = Helper.ReadBlazeBlob(s);
            }
            public override void Write(Stream s)
            {
                throw new NotImplementedException();
            }

            public override string Print(int tabs)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(Helper.MakeTabs(tabs) + header.TAG + " = { ");
                foreach (byte b in (byte[])data)
                    sb.Append(b.ToString("X2") + " ");
                sb.AppendLine("}");
                return sb.ToString();
            }
        }

        public class TDFStruct : TDF
        {
            public bool startswith2;
            public TDFStruct(string tag, List<TDF> list)
            {
                header = new TDFHeader(tag, TDFType.Struct);
                data = list;
            }
            public TDFStruct(TDFHeader h, Stream s)
            {
                header = h;
                data = new List<TDF>();
                while (true)
                {
                    byte test = (byte)s.ReadByte();
                    if (test == 0)
                        break;
                    if (test == 2)
                    {
                        startswith2 = true;
                        continue;
                    }
                    s.Seek(-1, SeekOrigin.Current);
                    ((List<TDF>)data).Add(ReadTDF(s));
                }
            }
            public override void Write(Stream s)
            {
                header.Write(s);
                if (startswith2)
                    s.WriteByte(2);
                foreach (TDF tdf in (List<TDF>)data)
                    tdf.Write(s);
                s.WriteByte(0);
            }

            public override string Print(int tabs)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(Helper.MakeTabs(tabs) + header.TAG + (startswith2 ? "*2 " : "") + "{");
                foreach (TDF tdf in (List<TDF>)data)
                    sb.Append(tdf.Print(tabs + 1));
                sb.AppendLine(Helper.MakeTabs(tabs) + "}");
                return sb.ToString();
            }
        }

        public class TDFList : TDF
        {
            public TDFType subType;

            public TDFList(string TAG, TDFType subT, object list)
            {
                header = new TDFHeader(TAG, TDFType.List);
                subType = subT;
                data = list;
            }
            public TDFList(TDFHeader h, Stream s)
            {
                header = h;
                subType = (TDFType)s.ReadByte();
                switch (subType)
                {
                    case TDFType.Integer:
                        data = new List<long>();
                        break;
                    case TDFType.String:
                        data = new List<string>();
                        break;
                    case TDFType.Struct:
                        data = new List<TDFStruct>();
                        break;
                    case TDFType.Tuple3:
                        data = new List<TDF3Tuple>();
                        break;
                    default:
                        throw new NotImplementedException();
                }
                long count = Helper.DecompressInteger(s);
                for (int i = 0; i < count; i++)
                    switch(subType)
                    {
                        case TDFType.Integer:
                            ((List<long>)data).Add(Helper.DecompressInteger(s));
                            break;
                        case TDFType.String:
                            ((List<string>)data).Add(Helper.ReadBlazeString(s));
                            break;
                        case TDFType.Struct:
                            ((List<TDFStruct>)data).Add(new TDFStruct(new TDFHeader("", TDFType.Struct), s));
                            break;
                        case TDFType.Tuple3:
                            ((List<TDF3Tuple>)data).Add(new TDF3Tuple(new TDFHeader("", TDFType.Tuple3), s));
                            break;
                        default:
                            throw new NotImplementedException();
                    }
            }
            public override void Write(Stream s)
            {
                header.Write(s);
                s.WriteByte((byte)subType);
                long count;
                switch (subType)
                {
                    case TDFType.Integer:
                        count = ((List<long>)data).Count;
                        break;
                    case TDFType.String:
                        count = ((List<string>)data).Count;
                        break;
                    case TDFType.Struct:
                        count = ((List<TDFStruct>)data).Count;
                        break;
                    case TDFType.Tuple3:
                        count = ((List<TDF3Tuple>)data).Count;
                        break;
                    default:
                        throw new NotImplementedException();
                }
                Helper.CompressInteger(count, s);
                for (int i = 0; i < count; i++)
                    switch (subType)
                    {
                        case TDFType.Integer:
                            Helper.CompressInteger(((List<long>)data)[i], s);
                            break;
                        case TDFType.String:
                            Helper.WriteBlazeString(s, ((List<string>)data)[i]);
                            break;
                        case TDFType.Struct:
                            TDFStruct st = ((List<TDFStruct>)data)[i];
                            foreach (TDF tdf in ((List<TDF>)st.data))
                                tdf.Write(s);
                            s.WriteByte(0);
                            break;
                        case TDFType.Tuple3:
                            ((List<TDF3Tuple>)data)[i].Write(s);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
            }

            public override string Print(int tabs)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(Helper.MakeTabs(tabs) + header.TAG + " <" + subType + "> {");
                switch (subType)
                {
                    case TDFType.Integer:
                        foreach (long l in (List<long>)data)
                            sb.AppendLine(Helper.MakeTabs(tabs + 1) + l + " (0x" + l.ToString("X") + "),");
                        break;
                    case TDFType.String:
                        foreach (string s in (List<string>)data)
                            sb.AppendLine(Helper.MakeTabs(tabs + 1) + s + ",");
                        break;
                    case TDFType.Struct:
                        foreach(TDFStruct str in (List<TDFStruct>)data)
                            sb.AppendLine(str.Print(tabs + 2));
                        break;
                    case TDFType.Tuple3:
                        foreach (TDF3Tuple t in (List<TDF3Tuple>)data)
                            sb.AppendLine(t.Print(tabs + 2));
                        break;
                    default:
                        throw new NotImplementedException();
                }
                sb.AppendLine(Helper.MakeTabs(tabs) + "}");
                return sb.ToString();
            }
        }

        public class TDFDictionary : TDF
        {
            public TDFType subType1;
            public TDFType subType2;

            public TDFDictionary(TDFHeader h, TDFType t1, TDFType t2)
            {
                header = h;
                subType1 = t1;
                subType2 = t2;
            }            
            public TDFDictionary(TDFHeader h, Stream s)
            {
                header = h;
                subType1 = (TDFType)s.ReadByte();
                subType2 = (TDFType)s.ReadByte();
                long count = Helper.DecompressInteger(s);
                data = new Dictionary<object, object>();
                for (int i = 0; i < count; i++)
                {
                    object key, value;
                    switch(subType1)
                    {
                        case TDFType.Integer:
                            key = Helper.DecompressInteger(s);
                            break;
                        case TDFType.String:
                            key = Helper.ReadBlazeString(s);
                            break;
                        case TDFType.Struct:
                            key = new TDFStruct(new TDFHeader("", TDFType.Struct), s);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    switch (subType2)
                    {
                        case TDFType.Integer:
                            value = Helper.DecompressInteger(s);
                            break;
                        case TDFType.String:
                            value = Helper.ReadBlazeString(s);
                            break;
                        case TDFType.Struct:
                            value = new TDFStruct(new TDFHeader("", TDFType.Struct), s);
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                    ((Dictionary<object, object>)data).Add(key, value);
                }
            }
            public override void Write(Stream s)
            {
                header.Write(s);
                s.WriteByte((byte)subType1);
                s.WriteByte((byte)subType2);
                Helper.CompressInteger(((Dictionary<object, object>)data).Count, s);
                foreach(KeyValuePair<object,object> pair in (Dictionary<object, object>)data)
                {
                    switch (subType1)
                    {
                        case TDFType.Integer:
                            Helper.CompressInteger(Convert.ToInt64(pair.Key.ToString()), s);
                            break;
                        case TDFType.String:
                            Helper.WriteBlazeString(s, (string)pair.Key);
                            break;
                        case TDFType.Struct:
                            foreach (TDF tdf in (List<TDF>)((TDFStruct)pair.Key).data)
                                tdf.Write(s);
                            s.WriteByte(0);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    switch (subType2)
                    {
                        case TDFType.Integer:
                            Helper.CompressInteger(Convert.ToInt64(pair.Value.ToString()), s);
                            break;
                        case TDFType.String:
                            Helper.WriteBlazeString(s, (string)pair.Value);
                            break;
                        case TDFType.Struct:
                            foreach (TDF tdf in (List<TDF>)((TDFStruct)pair.Value).data)
                                tdf.Write(s);
                            s.WriteByte(0);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }

            public override string Print(int tabs)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(Helper.MakeTabs(tabs) + header.TAG + " <" + subType1 + "," + subType2 + ">{");
                foreach (KeyValuePair<object, object> pair in ((Dictionary<object, object>)data))
                {
                    sb.Append(Helper.MakeTabs(tabs + 1));
                    switch (subType1)
                    {
                        case TDFType.Integer:
                            sb.Append(((long)pair.Key).ToString());
                            break;
                        case TDFType.String:
                            sb.Append((pair.Key).ToString());
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    sb.Append(" = ");
                    switch (subType2)
                    {
                        case TDFType.Integer:
                            sb.AppendLine(((long)pair.Value).ToString());
                            break;
                        case TDFType.String:
                            sb.AppendLine(pair.Value.ToString());
                            break;
                        case TDFType.Struct:
                            sb.AppendLine("\n" + ((TDFStruct)pair.Value).Print(tabs + 2));
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
                sb.AppendLine(Helper.MakeTabs(tabs) + "}");
                return sb.ToString();
            }
        }

        public class TDFUnion : TDF
        {
            public byte Unknown;
            public TDFUnion(string tag, TDF value, byte unk = 0x7F)
            {
                header = new TDFHeader(tag, TDFType.Union);
                data = value;
                Unknown = unk;
            }
            public TDFUnion(TDFHeader h, Stream s)
            {
                header = h;
                Unknown = (byte)s.ReadByte();
                data = ReadTDF(s);
            }

            public override void Write(Stream s)
            {
                header.Write(s);
                s.WriteByte(Unknown);
                ((TDF)data).Write(s);
            }

            public override string Print(int tabs)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(Helper.MakeTabs(tabs) + header.TAG + " = Union (0x" + Unknown.ToString("X2") + "){");
                sb.Append(((TDF)data).Print(tabs + 1));
                sb.AppendLine(Helper.MakeTabs(tabs) + "}");
                return sb.ToString();
            }
        }
        
        public class TDFIntegerArray : TDF
        {
            public byte UnionType;
            public TDFIntegerArray(string tag, long[] value)
            {
                header = new TDFHeader(tag, TDFType.IntegerArray);
                data = value;
            }
            public TDFIntegerArray(TDFHeader h, Stream s)
            {
                header = h;
                long count = Helper.DecompressInteger(s);
                data = new long[count];
                for (int i = 0; i < count; i++)
                    ((long[])data)[i] = Helper.DecompressInteger(s);
            }

            public override void Write(Stream s)
            {
                header.Write(s);
                long count = ((long[])data).Length;
                Helper.CompressInteger(count, s);
                foreach (long l in ((long[])data))
                    Helper.CompressInteger(l, s);
            }

            public override string Print(int tabs)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(Helper.MakeTabs(tabs) + header.TAG + " = [");
                foreach (long l in ((long[])data))
                    sb.AppendLine(Helper.MakeTabs(tabs + 1) + l + "(0x" + l.ToString("X") + "),");
                sb.AppendLine(Helper.MakeTabs(tabs) + "]");
                return sb.ToString();
            }
        }

        public class TDF2Tuple : TDF
        {
            public TDF2Tuple(string tag, long[] value)
            {
                header = new TDFHeader(tag, TDFType.Tuple2);
                data = value;
            }
            public TDF2Tuple(TDFHeader h, Stream s)
            {
                header = h;
                data = new long[] { Helper.DecompressInteger(s), Helper.DecompressInteger(s) };
            }

            public override void Write(Stream s)
            {
                header.Write(s);
                for (int i = 0; i < 2; i++)
                    Helper.CompressInteger(((long[])data)[i], s);
            }

            public override string Print(int tabs)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(Helper.MakeTabs(tabs) + header.TAG + " = (");
                for (int i = 0; i < 2; i++)
                    sb.Append(((long[])data)[i] + " ");
                sb.AppendLine(")");
                return sb.ToString();
            }
        }

        public class TDF3Tuple : TDF
        {
            public TDF3Tuple(string tag, long[] value)
            {
                header = new TDFHeader(tag, TDFType.Tuple3);
                data = value;
            }
            public TDF3Tuple(TDFHeader h, Stream s)
            {
                header = h;
                data = new long[] { Helper.DecompressInteger(s), Helper.DecompressInteger(s), Helper.DecompressInteger(s) };
            }

            public override void Write(Stream s)
            {
                header.Write(s);
                for (int i = 0; i < 3; i++)
                    Helper.CompressInteger(((long[])data)[i], s);
            }

            public override string Print(int tabs)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(Helper.MakeTabs(tabs) + header.TAG + " = (");
                for (int i = 0; i < 3; i++)
                    sb.Append(((long[])data)[i] + " ");
                sb.AppendLine(")");
                return sb.ToString();
            }
        }
    }
}
