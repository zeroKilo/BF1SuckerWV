using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;
namespace BF1SuckerWV
{
    public static class LSX
    {
        class CRandom
        {
            public void Seed(uint seed)
            {
                this.randSeed = seed;
            }

            public int Rand()
            {
                this.randSeed = this.randSeed * 214013U + 2531011U;
                return (int)(this.randSeed >> 16 & 65535U);
            }

            private uint randSeed;

            public static byte[] GetLSXKey(ushort seed)
            {
                CRandom crandom = new CRandom();
                crandom.Seed(7U);
                crandom.Seed((uint)(crandom.Rand() + seed));
                byte[] result = new byte[16];
                for (int i = 0; i < 16; i++)
                    result[i] = (byte)crandom.Rand();
                return result;
            }
        }
        public static ushort LSXport = 3216;
        public static string TokenRequest = "<LSX><Request recipient=\"Utility\" id=\"2\"><GetAuthCode ClientId=\"GOS-BlazeServer-BFTUN-PC\" Scope=\"\" version=\"3\"/></Request></LSX>";
        public static string ChallengeResponse = "<LSX><Request recipient=\"EALS\" id=\"1\"><ChallengeResponse response=\"##RESPONSE##\" key=\"##KEY##\" version=\"3\"><ContentId>Origin.OFR.50.0000557</ContentId><Title/><MultiplayerId/><Language>en_US</Language><Version>10.6.1.1</Version></ChallengeResponse></Request></LSX>";
        public static byte[] ChallengeKey;
        public static ushort Seed;
        public static string GetAuthToken()
        {
            Aes aes = GetSimpleAes();
            TcpClient client = new TcpClient("127.0.0.1", LSXport);
            NetworkStream s = client.GetStream();
            string response = ReadString(s);
            int start = response.IndexOf("key=") + 5;
            ChallengeKey = HexStringToArray(response.Substring(start, 32));
            byte[] data = Encrypt(aes, Encoding.UTF8.GetBytes(ArrayToHexString(ChallengeKey)));
            string challengeResData = ArrayToHexString(data);
            string request = ChallengeResponse.Replace("##RESPONSE##", challengeResData).Replace("##KEY##", ArrayToHexString(ChallengeKey));
            WriteString(s, request);
            ReadString(s);
            Seed = (ushort)(challengeResData[0] << 8 | challengeResData[1]);
            aes = GetLSXAes(Seed);
            data = Encrypt(aes, Encoding.UTF8.GetBytes(TokenRequest));
            request = ArrayToHexString(data);
            WriteString(s, request);
            data = HexStringToArray(ReadString(s));
            client.Close();
            response = Encoding.UTF8.GetString(Decrypt(aes, data));
            start = response.IndexOf("value=") + 7;
            int end = response.IndexOf('\"', start);
            return response.Substring(start, end - start);
        }

        public static void WriteString(Stream s, string str)
        {
            byte[] data = Encoding.UTF8.GetBytes(str + '\0');
            s.Write(data, 0, data.Length);
        }

        public static string ReadString(Stream s)
        {
            StringBuilder sb = new StringBuilder();
            int b;
            while ((b = s.ReadByte()) != -1 && b != 0)
                sb.Append((char)b);
            return sb.ToString();
        }

        public static string ArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in data)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        public static byte[] HexStringToArray(string s)
        {
            MemoryStream m = new MemoryStream();
            for (int i = 0; i < s.Length; i += 2)
                m.WriteByte(Convert.ToByte(s.Substring(i, 2), 16));
            return m.ToArray();
        }

        public static byte[] Decrypt(Aes aes, byte[] data)
        {
            ICryptoTransform transform = aes.CreateDecryptor();
            MemoryStream memoryStream = new MemoryStream();
            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
            {
                cryptoStream.Write(data, 0, data.Length);
                cryptoStream.FlushFinalBlock();
                return memoryStream.ToArray();
            }
        }

        public static byte[] Encrypt(Aes aes, byte[] data)
        {
            ICryptoTransform transform = aes.CreateEncryptor();
            MemoryStream memoryStream = new MemoryStream();
            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
            {
                cryptoStream.Write(data, 0, data.Length);
                cryptoStream.FlushFinalBlock();
                return memoryStream.ToArray();
            }
        }

        public static Aes GetSimpleAes()
        {
            byte[] key = new byte[16];
            for (byte i = 0; i < 16; i++)
                key[i] = i;
            Aes aes = Aes.Create();
            aes.Key = key;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            return aes;
        }
        public static Aes GetLSXAes(ushort seed)
        {
            Aes aes = Aes.Create();
            aes.Key = CRandom.GetLSXKey(seed);
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            return aes;
        }
    }
}
