using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BF1SuckerWV
{
    public static class Blaze_Stats
    {
        public enum StatsCommand
        {
            getStatDescs = 1,
            getStats = 2,
            getStatGroupList = 3,
            getStatGroup = 4,
            getStatsByGroup = 5,
            getDateRange = 6,
            getEntityCount = 7,
            getLeaderboardGroup = 0xA,
            getLeaderboardFolderGroup = 0xB,
            getLeaderboard = 0xC,
            getCenteredLeaderboard = 0xD,
            getFilteredLeaderboard = 0xE,
            getKeyScopesMap = 0xF,
            getStatsByGroupAsync = 0x10,
            getLeaderboardTreeAsync = 0x11,
            getLeaderboardEntityCount = 0x12,
            getStatCategoryList = 0x13,
            getPeriodIds = 0x14,
            getLeaderboardRaw = 0x15,
            getCenteredLeaderboardRaw = 0x16,
            getFilteredLeaderboardRaw = 0x17,
            changeKeyscopeValue = 0x18,
            getEntityRank = 0x19,
        }

        public static byte[] MakeGetStatGroupPacket()
        {
            Blaze.Packet p = new Blaze.Packet(0, (ushort)Blaze.Component.Stats, (ushort)StatsCommand.getStatGroup, Backend.sendCounter, 0, 0);
            p.payload.Add(new Blaze.TDFString("NAME", "player_FrontEndStats"));
            return p.MakePacket();
        }

        public static byte[] MakeGetStatsByGroupAsyncPacket()
        {
            Blaze.Packet p = new Blaze.Packet(0, (ushort)Blaze.Component.Stats, (ushort)StatsCommand.getStatsByGroupAsync, Backend.sendCounter, 0, 0);
            List<long> EID = new List<long>()
            {
                0xEA36C0DA6C
            };
            p.payload.Add(new Blaze.TDFList("EID", Blaze.TDF.TDFType.Integer, EID));
            p.payload.Add(new Blaze.TDFString("NAME", "player_FrontEndStats"));
            p.payload.Add(new Blaze.TDFInteger("PCTR", 1));
            p.payload.Add(new Blaze.TDFInteger("POFF", 0));
            p.payload.Add(new Blaze.TDFInteger("PRID", 0));
            p.payload.Add(new Blaze.TDFInteger("PTYP", 0));
            p.payload.Add(new Blaze.TDFInteger("TIME", 0));
            p.payload.Add(new Blaze.TDFInteger("VID", 1));
            return p.MakePacket();
        }
    }
}
