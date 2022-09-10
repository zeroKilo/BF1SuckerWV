using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BF1SuckerWV
{
    public static class Blaze_Authentication
    {
        public enum Authentication_Command
        {
            login = 0xA,
            trustedLogin = 0xB,
            updateAccount = 0x14,
            upgradeAccount = 0x15,
            listUserEntitlements2 = 0x1D,
            getAccount = 0x1E,
            grantEntitlement = 0x1F,
            listEntitlements = 0x20,
            getUseCount = 0x22,
            decrementUseCount = 0x23,
            getAuthToken = 0x24,
            getPasswordRules = 0x26,
            grantEntitlement2 = 0x27,
            modifyEntitlement2 = 0x2B,
            consumecode = 0x2C,
            passwordForgot = 0x2D,
            getPrivacyPolicyContent = 0x2F,
            listPersonaEntitlements2 = 0x30,
            checkAgeReq = 0x33,
            getOptIn = 0x34,
            enableOptIn = 0x35,
            disableOptIn = 0x36,
            expressLogin = 0x3C,
            logout = 0x46,
            getPersona = 0x5A,
            listPersonas = 0x64,
            expressCreateAccount = 0x65,
            createWalUserSession = 0xE6,
            acceptLegalDocs = 0xF1,
            getEmailOptInSettings = 0xF2,
            getTermsOfServiceContent = 0xF6,
            getOriginPersona = 0x104,
            checkEmail = 0x10E,
            getPersonaNameSuggestions = 0x118,
            guestLogin = 0x122
        }

        public static byte[] MakeLoginPacket()
        {
            Blaze.Packet p = new Blaze.Packet(0, (ushort)Blaze.Component.Authentication, (ushort)Authentication_Command.login, Backend.sendCounter, 0, 0);
            p.payload.Add(new Blaze.TDFString("AUTH", LSX.GetAuthToken()));
            p.payload.Add(new Blaze.TDFStruct("EXTB", new List<Blaze.TDF>()));
            p.payload.Add(new Blaze.TDFInteger("EXTI", 0));
            return p.MakePacket();
        }
        public static byte[] MakeListUserEntitlements2Packet()
        {
            Blaze.Packet p = new Blaze.Packet(0, (ushort)Blaze.Component.Authentication, (ushort)Authentication_Command.listUserEntitlements2, Backend.sendCounter, 0, 0);
            p.payload.Add(new Blaze.TDFInteger("BUID", 0xEA36C0DA6C));
            p.payload.Add(new Blaze.TDFString("EGDA", ""));
            p.payload.Add(new Blaze.TDFInteger("EPSN", 1));
            p.payload.Add(new Blaze.TDFInteger("EPSZ", 100));
            p.payload.Add(new Blaze.TDFString("ETAG", ""));
            p.payload.Add(new Blaze.TDFString("ETDA", ""));
            List<string> GNLS = new List<string>()
            {
                "Battlefield1",
                "BF1PC",
                "Battlefield",
            };
            p.payload.Add(new Blaze.TDFList("GNLS", Blaze.TDF.TDFType.String, GNLS));
            p.payload.Add(new Blaze.TDFInteger("HAUP", 0));
            p.payload.Add(new Blaze.TDFString("PJID", ""));
            p.payload.Add(new Blaze.TDFString("PRID", ""));
            p.payload.Add(new Blaze.TDFInteger("RECU", 0));
            p.payload.Add(new Blaze.TDFString("SGDA", ""));
            p.payload.Add(new Blaze.TDFInteger("STAT", 0));
            p.payload.Add(new Blaze.TDFString("STDA", ""));
            p.payload.Add(new Blaze.TDFInteger("TYPE", 0));
            return p.MakePacket();
        }
    }
}
