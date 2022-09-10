using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BF1SuckerWV
{
    public static class Blaze_Inventory
    {
        public enum InventoryCommand
        {
            getItems = 1,
            talliedConsumablesReport = 3,
            activateTemporalConsumable = 4,
            consumeTalliedConsumable = 5,
            drainTemporalConsumable = 6,
            getTemplate = 7,
            getItemKeyDictionary = 8,
            talliedConsumableAdjustmentReport = 9,
            redeemPurchase = 40,
            consumeMultipleTalliedConsumable = 43,
        }

        public static byte[] MakeGetItemsPacket()
        {
            Blaze.Packet p = new Blaze.Packet(0, (ushort)Blaze.Component.Inventory, (ushort)InventoryCommand.getItems, Backend.sendCounter, 0, 0);
            p.payload.Add(new Blaze.TDFInteger("CFLG", 7));
            p.payload.Add(new Blaze.TDFInteger("HIST", 0));
            p.payload.Add(new Blaze.TDFInteger("RTYP", 1));
            p.payload.Add(new Blaze.TDFInteger("UID", 0xEA36C0DA6C));
            p.payload.Add(new Blaze.TDFInteger("VERS", 0));
            return p.MakePacket();
        }

        public static byte[] MakeGetTemplatePacket()
        {
            Blaze.Packet p = new Blaze.Packet(0, (ushort)Blaze.Component.Inventory, (ushort)InventoryCommand.getTemplate, Backend.sendCounter, 0, 0);
            p.payload.Add(new Blaze.TDFInteger("DLTA", 1));
            p.payload.Add(new Blaze.TDFInteger("FLAG", 3));
            return p.MakePacket();
        }
    }
}
