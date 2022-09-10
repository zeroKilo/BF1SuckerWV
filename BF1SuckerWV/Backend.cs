using System;
using System.Text;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;

namespace BF1SuckerWV
{
    public static class Backend
    {

        public static uint sendCounter;

        public static bool DUMP_PACKETS = false;

        public static void Start()
        {
            sendCounter = 0;
            Console.WriteLine("[MAIN] Connecting to " + Redirector.backend_hostname + ":" + Redirector.backend_port);
            TcpClient client = new TcpClient(Redirector.backend_hostname, ushort.Parse(Redirector.backend_port));
            SslStream sslStream = new SslStream(client.GetStream(), true, Helper.CertCallback);
            sslStream.ReadTimeout = 1000;
            Console.WriteLine("[MAIN] Authenticate as client");
            sslStream.AuthenticateAsClient(Redirector.backend_hostname);
            Console.WriteLine("[MAIN] Sending requests...");
            SendCommand(sslStream, Blaze.Component.Util, (uint)Blaze_Util.UtilCommand.preAuth);
            SendCommand(sslStream, Blaze.Component.Util, (uint)Blaze_Util.UtilCommand.ping);
            SendCommand(sslStream, Blaze.Component.Util, (uint)Blaze_Util.UtilCommand.fetchClientConfig);
            SendCommand(sslStream, Blaze.Component.Authentication, (uint)Blaze_Authentication.Authentication_Command.login);
            SendCommand(sslStream, Blaze.Component.Util, (uint)Blaze_Util.UtilCommand.postAuth);
            SendCommand(sslStream, Blaze.Component.AssociationLists, (uint)Blaze_AssociationLists.AssociationLists_Command.getLists);
            SendCommand(sslStream, Blaze.Component.UserSession, (uint)Blaze_UserSessions.UserSession_Command.updateNetworkInfo);
            SendCommand(sslStream, Blaze.Component.Inventory, (uint)Blaze_Inventory.InventoryCommand.getTemplate);
            SendCommand(sslStream, Blaze.Component.LicenseManager, (uint)Blaze_LicenseManager.LicenseManagerCommand.getLicenses);
            SendCommand(sslStream, Blaze.Component.Authentication, (uint)Blaze_Authentication.Authentication_Command.listUserEntitlements2);
            SendCommand(sslStream, Blaze.Component.AssociationLists, (uint)Blaze_AssociationLists.AssociationLists_Command.subscribeToLists);
            SendCommand(sslStream, Blaze.Component.Util, (uint)Blaze_Util.UtilCommand.setClientState);
            SendCommand(sslStream, Blaze.Component.Util, (uint)Blaze_Util.UtilCommand.userSettingsLoadAll);
            SendCommand(sslStream, Blaze.Component.Stats, (uint)Blaze_Stats.StatsCommand.getStatGroup);
            SendCommand(sslStream, Blaze.Component.UserSession, (uint)Blaze_UserSessions.UserSession_Command.lookupUsersByPersonaNames);
            SendCommand(sslStream, Blaze.Component.Inventory, (uint)Blaze_Inventory.InventoryCommand.getItems);
            SendCommand(sslStream, Blaze.Component.Stats, (uint)Blaze_Stats.StatsCommand.getStatsByGroupAsync);
            SendCommand(sslStream, Blaze.Component.AssociationLists, (uint)Blaze_AssociationLists.AssociationLists_Command.getMemberHash);
            sslStream.Close();
            client.Close();
        }

        public static void SendCommand(Stream s, Blaze.Component comp, uint cmd)
        {
            byte[] request;
            switch(comp)
            {
                case Blaze.Component.Authentication:
                    request = MakeAuthenticationCmd((Blaze_Authentication.Authentication_Command)cmd);
                    break;
                case Blaze.Component.Stats:
                    request = MakeStatsCmd((Blaze_Stats.StatsCommand)cmd);
                    break;
                case Blaze.Component.Util:
                    request = MakeUtilCmd((Blaze_Util.UtilCommand)cmd);
                    break;
                case Blaze.Component.AssociationLists:
                    request = MakeAssociationsListCmd((Blaze_AssociationLists.AssociationLists_Command)cmd);
                    break;
                case Blaze.Component.Inventory:
                    request = MakeInventoryCmd((Blaze_Inventory.InventoryCommand)cmd);
                    break;
                case Blaze.Component.LicenseManager:
                    request = MakeLicenseManagerCmd((Blaze_LicenseManager.LicenseManagerCommand)cmd);
                    break;
                case Blaze.Component.UserSession:
                    request = MakeUserSessionCmd((Blaze_UserSessions.UserSession_Command)cmd);
                    break;
                default:
                    throw new NotImplementedException();
            }
            Blaze.Packet p = new Blaze.Packet(new MemoryStream(request));
            ushort reqCmp = p.Component;
            ushort reqCmd = p.Command;
            Console.WriteLine(p.Print());
            s.Write(request, 0, request.Length);
            if(DUMP_PACKETS)
                File.WriteAllBytes(sendCounter.ToString("D4") + "Request.bin", request);
            Console.WriteLine("[MAIN] Reading response");
            int subcounter = 0;
            while (true)
            {
                Thread.Sleep(100);
                byte[] response = Helper.ReadBlazePacket(s);
                if (DUMP_PACKETS)
                    File.WriteAllBytes(sendCounter.ToString("D4") + "Response_" + subcounter++ + ".bin", response);
                Console.WriteLine("[MAIN] Response : ");
                p = new Blaze.Packet(new MemoryStream(response));
                Console.WriteLine(p.Print());
                if (p.Component == reqCmp && p.Command == reqCmd)
                    break;
            }
            sendCounter++;
        }

        public static byte[] MakeAuthenticationCmd(Blaze_Authentication.Authentication_Command cmd)
        {
            byte[] request;
            Console.WriteLine("[MAIN] Sending Authentication command " + cmd);
            switch (cmd)
            {
                case Blaze_Authentication.Authentication_Command.login:
                    request = Blaze_Authentication.MakeLoginPacket();
                    break;
                case Blaze_Authentication.Authentication_Command.listUserEntitlements2:
                    request = Blaze_Authentication.MakeListUserEntitlements2Packet();
                    break;
                default:
                    throw new NotImplementedException();
            }
            return request;
        }

        public static byte[] MakeStatsCmd(Blaze_Stats.StatsCommand cmd)
        {
            byte[] request;
            Console.WriteLine("[MAIN] Sending Util command " + cmd);
            switch (cmd)
            {
                case Blaze_Stats.StatsCommand.getStatGroup:
                    request = Blaze_Stats.MakeGetStatGroupPacket();
                    break;
                case Blaze_Stats.StatsCommand.getStatsByGroupAsync:
                    request = Blaze_Stats.MakeGetStatsByGroupAsyncPacket();
                    break;
                default:
                    throw new NotImplementedException();
            }
            return request;
        }

        public static byte[] MakeUtilCmd(Blaze_Util.UtilCommand cmd)
        {
            byte[] request;
            Console.WriteLine("[MAIN] Sending Util command " + cmd);
            switch(cmd)
            {
                case Blaze_Util.UtilCommand.fetchClientConfig:
                    request = Blaze_Util.MakeFetchClientConfigPacket();
                    break;
                case Blaze_Util.UtilCommand.ping:
                    request = Blaze_Util.MakePingPacket();
                    break;
                case Blaze_Util.UtilCommand.preAuth:
                    request = Blaze_Util.MakePreAuthPacket();
                    break;
                case Blaze_Util.UtilCommand.postAuth:
                    request = Blaze_Util.MakePostAuthPacket();
                    break;
                case Blaze_Util.UtilCommand.userSettingsLoadAll:
                    request = Blaze_Util.MakeUserSettingsLoadAllPacket();
                    break;
                case Blaze_Util.UtilCommand.setClientState:
                    request = Blaze_Util.MakeSetClientStatePacket();
                    break;
                default:
                    throw new NotImplementedException();
            }
            return request;
        }

        public static byte[] MakeAssociationsListCmd(Blaze_AssociationLists.AssociationLists_Command cmd)
        {
            byte[] request;
            Console.WriteLine("[MAIN] Sending Associations List command " + cmd);
            switch (cmd)
            {
                case Blaze_AssociationLists.AssociationLists_Command.getLists:
                    request = Blaze_AssociationLists.MakeGetListsPacket();
                    break;
                case Blaze_AssociationLists.AssociationLists_Command.subscribeToLists:
                    request = Blaze_AssociationLists.MakeSubscribeToListsPacket();
                    break;
                case Blaze_AssociationLists.AssociationLists_Command.getMemberHash:
                    request = Blaze_AssociationLists.MakeGetMemberHashPacket();
                    break;
                default:
                    throw new NotImplementedException();
            }
            return request;
        }

        public static byte[] MakeInventoryCmd(Blaze_Inventory.InventoryCommand cmd)
        {
            byte[] request;
            Console.WriteLine("[MAIN] Sending User Sessions command " + cmd);
            switch (cmd)
            {
                case Blaze_Inventory.InventoryCommand.getItems:
                    request = Blaze_Inventory.MakeGetItemsPacket();
                    break;
                case Blaze_Inventory.InventoryCommand.getTemplate:
                    request = Blaze_Inventory.MakeGetTemplatePacket();
                    break;
                default:
                    throw new NotImplementedException();
            }
            return request;
        }

        public static byte[] MakeLicenseManagerCmd(Blaze_LicenseManager.LicenseManagerCommand cmd)
        {
            byte[] request;
            Console.WriteLine("[MAIN] Sending User Sessions command " + cmd);
            switch (cmd)
            {
                case Blaze_LicenseManager.LicenseManagerCommand.getLicenses:
                    request = Blaze_LicenseManager.MakeGetLicensesPacket();
                    break;
                default:
                    throw new NotImplementedException();
            }
            return request;
        }

        public static byte[] MakeUserSessionCmd(Blaze_UserSessions.UserSession_Command cmd)
        {
            byte[] request;
            Console.WriteLine("[MAIN] Sending User Sessions command " + cmd);
            switch (cmd)
            {
                case Blaze_UserSessions.UserSession_Command.updateNetworkInfo:
                    request = Blaze_UserSessions.MakeUpdateNetworkInfoPacket();
                    break;
                case Blaze_UserSessions.UserSession_Command.lookupUsersByPersonaNames:
                    request = Blaze_UserSessions.MakeLookupUsersByPersonaNamesPacket();
                    break;
                default:
                    throw new NotImplementedException();
            }
            return request;
        }
    }
}
