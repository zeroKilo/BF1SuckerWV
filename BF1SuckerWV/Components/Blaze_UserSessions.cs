using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BF1SuckerWV
{
    public class Blaze_UserSessions
    {
        public enum UserSession_Command
        {
            validateSessionKey = 1,
            fetchExtendedData = 3,
            updateExtendedDataAttribute = 5,
            updateHardwareFlags = 8,
            lookupUser = 0xC,
            lookupUsers = 0xD,
            lookupUsersByPrefix = 0xE,
            lookupUsersIdentification = 0xF,
            updateNetworkInfo = 0x14,
            lookupUserGeoIPData = 0x17,
            overrideUserGeoIPData = 0x18,
            updateUserSessionClientData = 0x19,
            setUserInfoAttribute = 0x1A,
            resetUserGeoIPData = 0x1B,
            lookupUserSessionId = 0x20,
            fetchLastLocaleUsedAndAuthError = 0x21,
            fetchUserFirstLastAuthTime = 0x22,
            setUserGeoOptIn = 0x25,
            enableUserAuditLogging = 0x29,
            disableUserAuditLogging = 0x2A,
            getUniqueDeviceId = 0x2D,
            lookupUsersByPrefixMultiNamespace = 0x2F,
            lookupUsersByPersonaNameMultiNamespace = 0x30,
            lookupUsersByPersonaNamesMultiNamespace = 0x31,
            lookupUsersByPersonaNames = 0x32,
            forceOwnSessionLogout = 0x36,
            updateLocalUserGroup = 0x37,
        }

        public static byte[] MakeUpdateNetworkInfoPacket()
        {
            Blaze.Packet p = new Blaze.Packet(0, (ushort)Blaze.Component.UserSession, (ushort)UserSession_Command.updateNetworkInfo, Backend.sendCounter, 0, 0);
            List<Blaze.TDF> INFO = new List<Blaze.TDF>();
            List<Blaze.TDF> VALU = new List<Blaze.TDF>();
            List<Blaze.TDF> EXIP = new List<Blaze.TDF>();
            List<Blaze.TDF> INIP = new List<Blaze.TDF>();
            EXIP.Add(new Blaze.TDFInteger("IP", 0));
            EXIP.Add(new Blaze.TDFInteger("MACI", 0));
            EXIP.Add(new Blaze.TDFInteger("PORT", 0));
            INIP.Add(new Blaze.TDFInteger("IP", 0));
            INIP.Add(new Blaze.TDFInteger("MACI", 0));
            INIP.Add(new Blaze.TDFInteger("PORT", 0));
            VALU.Add(new Blaze.TDFStruct("EXIP", EXIP));
            VALU.Add(new Blaze.TDFStruct("INIP", INIP));
            VALU.Add(new Blaze.TDFInteger("MACI", 0));
            INFO.Add(new Blaze.TDFUnion("ADDR", new Blaze.TDFStruct("VALU", VALU), 2));
            Blaze.TDFDictionary NLMP = new Blaze.TDFDictionary(new Blaze.TDF.TDFHeader("NLMP", Blaze.TDF.TDFType.Dictionary), Blaze.TDF.TDFType.String, Blaze.TDF.TDFType.Integer);
            Dictionary<object, object> dic = new Dictionary<object, object>()
            {
                {"brz", 1601961986 },
                {"dub" , 40},
                {"dxb" , 144},
                {"fra" , 14},
                {"hkg" , 1601961986},
                {"iad" , 112},
                {"jnb" , 186},
                {"nrt" , 1601961986},
                {"sjc" , 172},
                {"syd" , 1601961986},
            };
            NLMP.data = dic;
            INFO.Add(NLMP);
            List<Blaze.TDF> NQOS = new List<Blaze.TDF>();
            NQOS.Add(new Blaze.TDFInteger("BWHR", 0));
            NQOS.Add(new Blaze.TDFInteger("DBPS", 0));
            NQOS.Add(new Blaze.TDFInteger("NAHR", 0));
            NQOS.Add(new Blaze.TDFInteger("NATT", 5));
            NQOS.Add(new Blaze.TDFInteger("UBPS", 0));
            INFO.Add(new Blaze.TDFStruct("NQOS", NQOS));
            p.payload.Add(new Blaze.TDFStruct("INFO", INFO));
            p.payload.Add(new Blaze.TDFInteger("OPTS", 1));
            return p.MakePacket();
        }

        public static byte[] MakeLookupUsersByPersonaNamesPacket()
        {
            Blaze.Packet p = new Blaze.Packet(0, (ushort)Blaze.Component.UserSession, (ushort)UserSession_Command.lookupUsersByPersonaNames, Backend.sendCounter, 0, 0);
            p.payload.Add(new Blaze.TDFString("NASP", ""));
            List<string> PLST = new List<string>()
            {
                "ABN-DARA",
                "AL--WAWWY--",
                "CS_Senpai",
                "Yaser_Revive_YOU",
                "MickeyMouse0",
                "Chappie1820",
                "wuhufeixingyuann",
                "AL--WAWWY--l",
            };
            p.payload.Add(new Blaze.TDFList("PLST", Blaze.TDF.TDFType.String, PLST));
            return p.MakePacket();
        }
    }
}
