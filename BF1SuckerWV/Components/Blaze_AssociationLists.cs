using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BF1SuckerWV
{
    public static class Blaze_AssociationLists
    {
        public enum AssociationLists_Command
        {
            addUsersToList = 1,
            removeUsersFromList = 2,
            clearLists = 3,
            setUsersToList = 4,
            getListForUser = 5,
            getLists = 6,
            subscribeToLists = 7,
            unsubscribeFromLists = 8,
            getConfigListsInfo = 9,
            getMemberHash = 0xA,
            setUsersAttributesInList = 0xD,
        }

        public static byte[] MakeSetUsersToListPacket()
        {
            Blaze.Packet p = new Blaze.Packet(0, (ushort)Blaze.Component.AssociationLists, (ushort)AssociationLists_Command.setUsersToList, Backend.sendCounter, 0, 0);
            p.payload.Add(new Blaze.TDFInteger("MXRC", 0xFFFFFFFF));
            p.payload.Add(new Blaze.TDFInteger("OFRC", 0));
            return p.MakePacket();
        }

        public static byte[] MakeGetListsPacket()
        {
            Blaze.Packet p = new Blaze.Packet(0, (ushort)Blaze.Component.AssociationLists, (ushort)AssociationLists_Command.getLists, Backend.sendCounter, 0, 0);
            p.payload.Add(new Blaze.TDFInteger("MXRC", 0xFFFFFFFF));
            p.payload.Add(new Blaze.TDFInteger("OFRC", 0));
            return p.MakePacket();
        }

        public static byte[] MakeSubscribeToListsPacket()
        {
            Blaze.Packet p = new Blaze.Packet(0, (ushort)Blaze.Component.AssociationLists, (ushort)AssociationLists_Command.subscribeToLists, Backend.sendCounter, 0, 0);
            p.payload.Add(new Blaze.TDFInteger("BID", 0));
            List<Blaze.TDFStruct> LIDS = new List<Blaze.TDFStruct>();
            List<Blaze.TDF> unNamed = new List<Blaze.TDF>();
            unNamed.Add(new Blaze.TDFString("LNM", "friendList"));
            unNamed.Add(new Blaze.TDFInteger("TYPE", 1));
            LIDS.Add(new Blaze.TDFStruct("", unNamed));
            p.payload.Add(new Blaze.TDFList("LIDS", Blaze.TDF.TDFType.Struct, LIDS));
            return p.MakePacket();
        }

        public static byte[] MakeGetMemberHashPacket()
        {
            Blaze.Packet p = new Blaze.Packet(0, (ushort)Blaze.Component.AssociationLists, (ushort)AssociationLists_Command.getMemberHash, Backend.sendCounter, 0, 0);
            List<Blaze.TDF> LID = new List<Blaze.TDF>()
            {
                new Blaze.TDFString("LNM", "friendList"),
                new Blaze.TDFInteger("TYPE", 1),
            };
            p.payload.Add(new Blaze.TDFInteger("BID", 0));
            p.payload.Add(new Blaze.TDFStruct("LID", LID));
            return p.MakePacket();
        }
    }
}
