using System;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Collections.Generic;

namespace BF1SuckerWV
{
    public static class Helper
    {
        public static bool CertCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
        public static string ReadStream(Stream stream)
        {
            return Encoding.UTF8.GetString(ReadStreamRaw(stream));
        }
        public static byte[] ReadStreamRaw(Stream stream)
        {
            byte[] resultBuffer = new byte[2048];
            MemoryStream m = new MemoryStream();
            do
            {
                try
                {
                    int read = stream.Read(resultBuffer, 0, resultBuffer.Length);
                    m.Write(resultBuffer, 0, read);
                    if (read < resultBuffer.Length)
                        break;
                }
                catch { break; }
            } while (true);
            return m.ToArray();
        }
        public static byte[] ReadBlazePacket(Stream stream)
        {
            MemoryStream m = new MemoryStream();
            uint len = ReadU32(stream);
            ushort more = ReadU16(stream);
            WriteU32(m, len);
            WriteU16(m, more);
            len = len + more + 10;
            byte[] buff = new byte[len];
            int read = 0;
            while (read < len)
                read += stream.Read(buff, read, buff.Length - read);
            m.Write(buff, 0, buff.Length);
            return m.ToArray();
        }


        public static string TagToLabel(uint Tag)
        {
            string s = "";
            List<byte> buff = new List<byte>(BitConverter.GetBytes(Tag));
            buff.Reverse();
            byte[] res = new byte[4];
            res[0] |= (byte)((buff[0] & 0x80) >> 1);
            res[0] |= (byte)((buff[0] & 0x40) >> 2);
            res[0] |= (byte)((buff[0] & 0x30) >> 2);
            res[0] |= (byte)((buff[0] & 0x0C) >> 2);

            res[1] |= (byte)((buff[0] & 0x02) << 5);
            res[1] |= (byte)((buff[0] & 0x01) << 4);
            res[1] |= (byte)((buff[1] & 0xF0) >> 4);

            res[2] |= (byte)((buff[1] & 0x08) << 3);
            res[2] |= (byte)((buff[1] & 0x04) << 2);
            res[2] |= (byte)((buff[1] & 0x03) << 2);
            res[2] |= (byte)((buff[2] & 0xC0) >> 6);

            res[3] |= (byte)((buff[2] & 0x20) << 1);
            res[3] |= (byte)((buff[2] & 0x1F));

            for (int i = 0; i < 4; i++)
            {
                if (res[i] < 0x20)
                    res[i] += 0x20;
                s += (char)res[i];
            }
            return s;
        }
        public static byte[] Label2Tag(string Label)
        {
            byte[] res = new byte[3];
            while (Label.Length < 4)
                Label += '\0';
            if (Label.Length > 4)
                Label = Label.Substring(0, 4);
            byte[] buff = new byte[4];
            for (int i = 0; i < 4; i++)
                buff[i] = (byte)Label[i];
            res[0] |= (byte)((buff[0] & 0x40) << 1);
            res[0] |= (byte)((buff[0] & 0x10) << 2);
            res[0] |= (byte)((buff[0] & 0x0F) << 2);
            res[0] |= (byte)((buff[1] & 0x40) >> 5);
            res[0] |= (byte)((buff[1] & 0x10) >> 4);

            res[1] |= (byte)((buff[1] & 0x0F) << 4);
            res[1] |= (byte)((buff[2] & 0x40) >> 3);
            res[1] |= (byte)((buff[2] & 0x10) >> 2);
            res[1] |= (byte)((buff[2] & 0x0C) >> 2);

            res[2] |= (byte)((buff[2] & 0x03) << 6);
            res[2] |= (byte)((buff[3] & 0x40) >> 1);
            res[2] |= (byte)((buff[3] & 0x1F));
            return res;
        }
        public static long DecompressInteger(Stream s)
        {
            List<byte> tmp = new List<byte>();
            byte b;
            while ((b = (byte)s.ReadByte()) >= 0x80)
                tmp.Add(b);
            tmp.Add(b);
            byte[] buff = tmp.ToArray();
            int currshift = 6;
            ulong result = (ulong)(buff[0] & 0x3F);
            for (int i = 1; i < buff.Length; i++)
            {
                byte curbyte = buff[i];
                ulong l = (ulong)(curbyte & 0x7F) << currshift;
                result |= l;
                currshift += 7;
            }
            return (long)result;
        }
        public static void CompressInteger(long l, Stream s)
        {
            List<byte> result = new List<byte>();
            if (l < 0x40)
            {
                result.Add((byte)(l & 0xFF));
            }
            else
            {
                byte curbyte = (byte)((l & 0x3F) | 0x80);
                result.Add(curbyte);
                long currshift = l >> 6;
                while (currshift >= 0x80)
                {
                    curbyte = (byte)((currshift & 0x7F) | 0x80);
                    currshift >>= 7;
                    result.Add(curbyte);
                }
                result.Add((byte)currshift);
            }
            foreach (byte b in result)
                s.WriteByte(b);
        }

        public static string ReadBlazeString(Stream s)
        {
            int len = (int)DecompressInteger(s);
            string res = "";
            for (int i = 0; i < len - 1; i++)
                res += (char)s.ReadByte();
            s.ReadByte();
            return res;
        }
        public static byte[] ReadBlazeBlob(Stream s)
        {
            int len = (int)DecompressInteger(s);
            byte[] result = new byte[len];
            s.Read(result, 0, len);
            return result;
        }

        public static uint ReadU32(Stream s, bool isLE = true)
        {
            byte[] data = new byte[4];
            s.Read(data, 0, data.Length);
            List<byte> result = new List<byte>(data);
            if (isLE)
                result.Reverse();
            return BitConverter.ToUInt32(result.ToArray(), 0);
        }
        public static ushort ReadU16(Stream s, bool isLE = true)
        {
            byte[] data = new byte[2];
            s.Read(data, 0, data.Length);
            List<byte> result = new List<byte>(data);
            if (isLE)
                result.Reverse();
            return BitConverter.ToUInt16(result.ToArray(), 0);
        }
        public static void WriteBlazeString(Stream s, string str)
        {
            CompressInteger(str.Length + 1, s);
            foreach (char c in str)
                s.WriteByte((byte)c);
            s.WriteByte(0);
        }

        public static void WriteU32(Stream s, uint u, bool isLE = true)
        {
            List<byte> data = new List<byte>(BitConverter.GetBytes(u));
            if (isLE)
                data.Reverse();
            foreach (byte b in data)
                s.WriteByte(b);
        }
        public static void WriteU16(Stream s, ushort u, bool isLE = true)
        {
            List<byte> data = new List<byte>(BitConverter.GetBytes(u));
            if (isLE)
                data.Reverse();
            foreach (byte b in data)
                s.WriteByte(b);
        }

        public static string MakeTabs(int tabs)
        {
            string s = "";
            for (int i = 0; i < tabs; i++)
                s += "\t";
            return s;
        }
    }
}
