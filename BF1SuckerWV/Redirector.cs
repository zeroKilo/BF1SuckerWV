using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace BF1SuckerWV
{
    public static class Redirector
    {
        public static readonly string Host = "winter15.gosredirector.ea.com";
        public static readonly ushort Port = 42230;
        public static string backend_hostname;
        public static string backend_port;
        public static string backend_ip;
        public static void GetBackendIP()
        {
            Console.WriteLine("[REDI] Connecting to " + Host + ":" + Port);
            TcpClient client = new TcpClient(Host, Port);
            SslStream sslStream = new SslStream(client.GetStream(), true, Helper.CertCallback);
            sslStream.ReadTimeout = 1000;
            Console.WriteLine("[REDI] Authenticate as client");
            sslStream.AuthenticateAsClient(Host);
            Console.WriteLine("[REDI] Sending request");
            string requestHeader = Res.RedirectorRequestHeader;
            string requestBody = Res.RedirectorRequestBody;
            if (!requestHeader.EndsWith("\r\n\r\n"))
                requestHeader += "\r\n\r\n";
            if (!requestBody.EndsWith("\n"))
                requestBody += "\n";
            requestHeader = requestHeader.Replace("XXXX", requestBody.Length.ToString());
            string request = requestHeader + requestBody;
            sslStream.Write(Encoding.UTF8.GetBytes(request));
            Console.WriteLine("[REDI] Reading response");
            string response = Helper.ReadStream(sslStream);
            Console.WriteLine("[REDI] Processing response");
            int start = response.IndexOf("<?");
            if(start != -1)
            {
                string xmlString = response.Substring(start);
                XDocument xml = XDocument.Parse(xmlString);
                backend_hostname = xml.XPathSelectElement("/serverinstanceinfo/address/valu/hostname").Value;
                backend_port = xml.XPathSelectElement("/serverinstanceinfo/address/valu/port").Value;
                backend_ip = xml.XPathSelectElement("/serverinstanceinfo/address/valu/ip").Value;
                uint ip_u = uint.Parse(backend_ip);
                byte[] ip_a = BitConverter.GetBytes(ip_u);
                backend_ip = ip_a[3] + "." + ip_a[2] + "." + ip_a[1] + "." + ip_a[0];
                Console.WriteLine("[REDI] Response : " + backend_hostname + ":" + backend_port + " (" + backend_ip + ")");
            }
            else
                Console.WriteLine("[REDI] Error: Response failed!");
            sslStream.Close();
            client.Close();
        }
    }
}
