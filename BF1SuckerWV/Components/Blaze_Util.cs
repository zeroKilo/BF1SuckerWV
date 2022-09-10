using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BF1SuckerWV
{
    public static class Blaze_Util
    {
        public enum UtilCommand
        {
            fetchClientConfig = 1,
            ping = 2,
            setClientData = 3,
            localizeStrings = 4,
            getTelemetryServer = 5,
            getTickerServer = 6,
            preAuth = 7,
            postAuth = 8,
            userSettingsLoad = 0xA,
            userSettingsSave = 0xB,
            userSettingsLoadAll = 0xC,
            deleteUserSettings = 0xE,
            userSettingsLoadMultiple = 0xF,
            filterForProfanity = 0x14,
            fetchQosConfig = 0x15,
            setClientMetrics = 0x16,
            setConnectionState = 0x17,
            getUserOptions = 0x19,
            setUserOptions = 0x1A,
            suspendUserPing = 0x1B,
            setClientState = 0x1C,
        }

        public static byte[] MakeFetchClientConfigPacket()
        {
            Blaze.Packet p = new Blaze.Packet(0, (ushort)Blaze.Component.Util, (ushort)UtilCommand.fetchClientConfig, Backend.sendCounter, 0, 0);
            p.payload.Add(new Blaze.TDFString("CFID", "IdentityParams"));
            return p.MakePacket();
        }

        public static byte[] MakePingPacket()
        {
            Blaze.Packet p = new Blaze.Packet(0, (ushort)Blaze.Component.Util, (ushort)UtilCommand.ping, Backend.sendCounter, 0, 0);
            return p.MakePacket();
        }

        public static byte[] MakePreAuthPacket()
        {
            Blaze.Packet p = new Blaze.Packet(0, (ushort)Blaze.Component.Util, (ushort)UtilCommand.preAuth, Backend.sendCounter, 0, 0);
            List<Blaze.TDF> CDAT = new List<Blaze.TDF>();
            CDAT.Add(new Blaze.TDFInteger("IITO", 0));
            CDAT.Add(new Blaze.TDFInteger("LANG", 0x656E5553));
            CDAT.Add(new Blaze.TDFString("SVCN", "battlefield - 1 - pc"));
            CDAT.Add(new Blaze.TDFInteger("TYPE", 0));
            p.payload.Add(new Blaze.TDFStruct("CDAT", CDAT));
            List<Blaze.TDF> CINF = new List<Blaze.TDF>();
            CINF.Add(new Blaze.TDFString("BSDK", "15.1.1.3.0"));
            CINF.Add(new Blaze.TDFString("BTIM", "Jul 13 2018 21:54:21"));
            CINF.Add(new Blaze.TDFString("CLNT", "tunguska client"));
            CINF.Add(new Blaze.TDFInteger("CPFT", 4));
            CINF.Add(new Blaze.TDFString("CSKU", "pc"));
            CINF.Add(new Blaze.TDFString("CVER", "XPack23779779retail-x64-1.0-3779779"));
            CINF.Add(new Blaze.TDFString("DSDK", "15.1.2.1.0"));
            CINF.Add(new Blaze.TDFString("ENV", "prod"));
            CINF.Add(new Blaze.TDFInteger("LOC", 0x656E5553));
            CINF.Add(new Blaze.TDFString("PTVR", "1.1"));
            p.payload.Add(new Blaze.TDFStruct("CINF", CINF));
            List<Blaze.TDF> FCCR = new List<Blaze.TDF>();
            FCCR.Add(new Blaze.TDFString("CFID", "BlazeSDK"));
            p.payload.Add(new Blaze.TDFStruct("FCCR", FCCR));
            p.payload.Add(new Blaze.TDFInteger("LADD", 0x6400A8C0));
            return p.MakePacket();
        }

        public static byte[] MakePostAuthPacket()
        {
            Blaze.Packet p = new Blaze.Packet(0, (ushort)Blaze.Component.Util, (ushort)UtilCommand.postAuth, Backend.sendCounter, 0, 0);
            p.payload.Add(new Blaze.TDFInteger("DSUI", 0));
            p.payload.Add(new Blaze.TDFString("UDID", ""));
            return p.MakePacket();
        }

        public static byte[] MakeUserSettingsLoadAllPacket()
        {
            Blaze.Packet p = new Blaze.Packet(0, (ushort)Blaze.Component.Util, (ushort)UtilCommand.userSettingsLoadAll, Backend.sendCounter, 0, 0);
            return p.MakePacket();
        }

        public static byte[] MakeSetClientStatePacket()
        {
            Blaze.Packet p = new Blaze.Packet(0, (ushort)Blaze.Component.Util, (ushort)UtilCommand.setClientState, Backend.sendCounter, 0, 0);
            p.payload.Add(new Blaze.TDFInteger("MODE", 1));
            p.payload.Add(new Blaze.TDFInteger("STAT", 0));
            return p.MakePacket();
        }
    }
}
