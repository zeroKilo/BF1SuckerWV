using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BF1SuckerWV
{
    public static class Blaze_LicenseManager
    {
        public enum LicenseManagerCommand
        {
            getLicenses = 1
        }

        public static byte[] MakeGetLicensesPacket()
        {
            Blaze.Packet p = new Blaze.Packet(0, (ushort)Blaze.Component.LicenseManager, (ushort)LicenseManagerCommand.getLicenses, Backend.sendCounter, 0, 0);
            p.payload.Add(new Blaze.TDFInteger("FCLR", 1));
            p.payload.Add(new Blaze.TDFInteger("PID", 0));
            return p.MakePacket();
        }
    }
}
