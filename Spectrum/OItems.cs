﻿using System;
using System.Collections.Generic;
using mzxrules.Helper;

namespace Spectrum
{
    static class OItems
    {
        public enum Item
        {
            Sticks = 0x00,
            Nuts = 0x01,
            Bombs = 0x02,
            Bow = 0x03,
            FireArrow = 0x04,
            DinsFire = 0x05,
            Slingshot = 0x06,
            Ocarina = 0x08,
            Bombchu = 0x09,
            Hookshot = 0x0A,
            Longshot = 0x0B,
            IceArrow = 0x0C,
            FaroresWind = 0x0D,
            Boomerang = 0x0E,
            Lens = 0x0F,
            Beans = 0x10,
            Hammer = 0x11,
            LightArrow = 0x12,
            NayrusLove = 0x13,

            Bottle = 0x14,
            RedPotion = 0x15,
            GreenPotion = 0x16,
            BluePotion = 0x17,
            Fairy = 0x18,
            Fish = 0x19,
            Milk = 0x1A,
            RutosLetter = 0x1B,
            BlueFire = 0x1C,
            Bugs = 0x1D,
            BigPoe = 0x1E,
            HalfMilk = 0x1F,
            Poe = 0x20,

            WeirdEgg = 0x21,
            Chicken = 0x22,
            ZeldasLetter = 0x23,
            KeatonMask = 0x24,
            SkullMask = 0x25,
            SpookyMask = 0x26,
            BunnyHood = 0x27,
            GoronMask = 0x28,
            ZoraMask = 0x29,
            GerudoMask = 0x2A,
            MaskOfTruth = 0x2B,
            SoldOut = 0x2C,

            PocketEgg = 0x2D,
            PocketCucco = 0x2E,
            OddMushroom = 0x30,
            OddPotion = 0x31,
            PoachersSaw = 0x32,
            BrokenSword = 0x33,
            Prescription = 0x34,
            EyeballFrog = 0x35,
            EyeDrops = 0x36,
            ClaimCheck = 0x37,

            FireBow = 0x38,
            IceBow = 0x39,
            LightBow = 0x3A,
            
            KokiriSword = 0x3B,
            MasterSword = 0x3C,
            BiggoronsSword = 0x3D,

            DekuShield = 0x3E,
            HylianShield = 0x3F,
            MirrorShield = 0x40,

            KokiriTunic = 0x41,
            GoronTunic = 0x42,
            ZoraTunic = 0x43,

            KokiriBoots = 0x44,
            IronBoots = 0x45,
            HoverBoots = 0x46,

            Minuet = 0x5A,
            Bolero = 0x5B,
            Serenade = 0x5C,
            Requiem = 0x5D,
            Nocturne = 0x5E,
            Prelude = 0x5F,
            Lullaby = 0x60,
            EponasSong = 0x61,
            SariasSong = 0x62,
            SunsSong = 0x63,
            SongOfTime = 0x64,
            SongOfStorms = 0x65,
            ForestMedallion = 0x66,
            FireMedallion = 0x67,
            WaterMedallion = 0x68,
            SpiritMedallion = 0x69,
            ShadowMedallion = 0x6A,
            LightMedallion = 0x6B,
            Emerald = 0x6C,
            Ruby = 0x6D,
            Sapphire = 0x6E,
            StoneOfAgony = 0x6F,

            None = 0xFF,

            //Pseudo items
            Magic,
            DoubleMagic,
            Wallet,
            Strength,
            Scale,
        }

        const int MASK_NUTS         = 0x00700000;
        const int MASK_STICKS       = 0x000E0000;
        const int MASK_BULLET_BAG   = 0x0001C000;
        const int MASK_WALLET       = 0x00003000;
        const int MASK_SCALE        = 0x00000E00;
        const int MASK_STRENGTH     = 0x000001C0;
        const int MASK_BOMBS        = 0x00000038;
        const int MASK_QUIVER       = 0x00000007;

        const int INVENTORY_SLOT_TOTAL = 24;
        const int ADULT_TRADE_SLOT = 22;
        const int CHILD_TRADE_SLOT = 23;


        struct InventoryInfo
        {
            public int Slot;
            public bool Ammo;
            public Item LeftEquipment;

            public InventoryInfo(int slot, bool ammo = false, Item leftEquipment = Item.None)
            {
                Slot = slot;
                Ammo = ammo;
                LeftEquipment = leftEquipment;
            }

            public static implicit operator InventoryInfo(int slot)
            {
                return new InventoryInfo(slot);
            }
        }
        static readonly Dictionary<Item, InventoryInfo> InventorySlot = new()
        {
            { Item.Sticks, new InventoryInfo(0, true, Item.Sticks) },
            { Item.Nuts, new InventoryInfo(1, true, Item.Nuts) },
            { Item.Bombs, new InventoryInfo(2, true, Item.Bombs) },
            { Item.Bow, new InventoryInfo(3, true, Item.Bow) },
            { Item.FireArrow, 4 },
            { Item.DinsFire, 5 },
            { Item.Slingshot, new InventoryInfo(6, true, Item.Slingshot) },
            { Item.Ocarina, 7 },
            { Item.Bombchu, new InventoryInfo(8, true) },
            { Item.Hookshot, 9 },
            { Item.Longshot, 9 },
            { Item.IceArrow, 10 },
            { Item.FaroresWind, 11 },
            { Item.Boomerang, 12 },
            { Item.Lens, 13 },
            { Item.Beans, new InventoryInfo(14, true) },
            { Item.Hammer, 15 },
            { Item.LightArrow, 16 },
            { Item.NayrusLove, 17 },

            { Item.WeirdEgg, CHILD_TRADE_SLOT },
            { Item.Chicken, CHILD_TRADE_SLOT },
            { Item.ZeldasLetter, CHILD_TRADE_SLOT },
            { Item.KeatonMask, CHILD_TRADE_SLOT },
            { Item.SkullMask, CHILD_TRADE_SLOT },
            { Item.SpookyMask, CHILD_TRADE_SLOT },
            { Item.BunnyHood, CHILD_TRADE_SLOT },
            { Item.GoronMask, CHILD_TRADE_SLOT },
            { Item.ZoraMask, CHILD_TRADE_SLOT },
            { Item.GerudoMask, CHILD_TRADE_SLOT },
            { Item.MaskOfTruth, CHILD_TRADE_SLOT },
            { Item.SoldOut, CHILD_TRADE_SLOT },

            { Item.PocketEgg, ADULT_TRADE_SLOT },
            { Item.PocketCucco, ADULT_TRADE_SLOT },
            { Item.OddMushroom, ADULT_TRADE_SLOT },
            { Item.OddPotion, ADULT_TRADE_SLOT },
            { Item.PoachersSaw, ADULT_TRADE_SLOT },
            { Item.BrokenSword, ADULT_TRADE_SLOT },
            { Item.Prescription, ADULT_TRADE_SLOT },
            { Item.EyeballFrog, ADULT_TRADE_SLOT },
            { Item.EyeDrops, ADULT_TRADE_SLOT },
            { Item.ClaimCheck, ADULT_TRADE_SLOT },
        };

        public static void GiveItem(Item item, Ptr saveCtx)
        {
            if (item <= Item.ClaimCheck)
            {
                SetInventoryItem(item, saveCtx);
            }
            else if (item <= Item.HoverBoots)
            {
                short equip = saveCtx.ReadInt16(0x9C);
                SetEquipment(item, true, ref equip);
                saveCtx.Write(0x9C, equip);
            }
            else if (item <= Item.StoneOfAgony)
            {
                int quest = saveCtx.ReadInt32(0xA4);
                SetQuestItem(item, true, ref quest);
                saveCtx.Write(0xA4, quest);
            }
            // Set Magic
            else if (item is Item.Magic or Item.DoubleMagic)
            {
                byte magicNext = (byte)(item == Item.Magic ? 0x30 : 0x60);
                byte magicNow = saveCtx.ReadByte(0x33);
                magicNow = Math.Min(magicNow, magicNext);

                saveCtx.Write(
                    0x32, (byte)0,
                    0x33, (byte)magicNow,
                    0x13F6, (short)magicNext,
                    0x3A, (byte)1,
                    0x3C, (byte)(item == Item.Magic ? 0 : 1));
            }
        }

        public static void SetInventoryItem(Item item, Ptr saveCtx)
        {
            Ptr inventPtr = saveCtx.RelOff(0x74);

            if (item is >= Item.Bottle and <= Item.Poe)
            {
                inventPtr.Write(18, (byte)item);
            }
            else if (InventorySlot.ContainsKey(item))
            {
                InventoryInfo info = InventorySlot[item];

                inventPtr.Write(info.Slot, (byte)item);

                SetInventoryItemAmmo(item, 50, saveCtx);

                if (info.Ammo && info.LeftEquipment != Item.None)
                {
                    int equipment = saveCtx.ReadInt32(0xA0);
                    SetLeftEquipmentItem(item, 1, ref equipment);
                    saveCtx.Write(0xA0, equipment);
                }
            }
        }

        public static void SetInventoryItemAmmo(Item item, byte amount, Ptr saveCtx)
        {
            Ptr ammoPtr = saveCtx.RelOff(0x8C);
            var info = InventorySlot[item];

            if (info.Ammo)
            {
                ammoPtr.Write(info.Slot, amount);
            }
        }

        struct LeftEquipmentInfo
        {
            public int Mask;
            public int Max;

            public LeftEquipmentInfo(int mask, int max)
            {
                Mask = mask;
                Max = max;
            }
        }

        static readonly Dictionary<Item, LeftEquipmentInfo> LeftEquipment = new()
        {
            { Item.Bombs, new LeftEquipmentInfo(MASK_BOMBS, 3) },
            { Item.Bow, new LeftEquipmentInfo(MASK_QUIVER, 3) },
            { Item.Nuts, new LeftEquipmentInfo(MASK_NUTS, 3) },
            { Item.Slingshot, new LeftEquipmentInfo(MASK_BULLET_BAG, 3) },
            { Item.Sticks, new LeftEquipmentInfo(MASK_STICKS, 3) },
            { Item.Strength, new LeftEquipmentInfo(MASK_STRENGTH, 3) },
            { Item.Scale, new LeftEquipmentInfo(MASK_SCALE, 2) },
            { Item.Wallet, new LeftEquipmentInfo(MASK_WALLET, 3) },
        };

        public static bool SetLeftEquipmentItemMax(Item item, ref int var)
        {
            return SetLeftEquipmentItem(item, int.MaxValue, ref var);
        }

        public static void SetLeftEquipmentMax(ref int var)
        {
            foreach (var item in LeftEquipment.Keys)
            {
                SetLeftEquipmentItem(item, int.MaxValue, ref var);
            }
        }

        public static void SetLeftEquipmentNone(ref int var)
        {
            foreach (var item in LeftEquipment.Keys)
            {
                SetLeftEquipmentItem(item, 0, ref var);
            }
        }

        public static bool SetLeftEquipmentItem(Item item, int value, ref int var)
        {
            if (!LeftEquipment.ContainsKey(item))
                return false;

            var info = LeftEquipment[item];

            if (value == int.MaxValue)
            {
                value = info.Max;
            }

            int leftshift = Shift.GetRight((uint)info.Mask);
            int write = value << leftshift;
            var &= -1 ^ info.Mask; 
            var |= write;
            return true;
        }

        struct EquipmentInfo
        {
            public int Mask;
            public int Value;

            public EquipmentInfo(int mask, int value)
            {
                Mask = mask;
                Value = value;
            }
        }

        static readonly Dictionary<Item, EquipmentInfo> Equipment = new()
        {
            {  Item.KokiriSword, new EquipmentInfo(0x0001, 1) },
            {  Item.MasterSword, new EquipmentInfo(0x0002, 2) },
            {  Item.BiggoronsSword, new EquipmentInfo(0x0004, 3) },

            {  Item.DekuShield, new EquipmentInfo(0x0010, 1) },
            {  Item.HylianShield, new EquipmentInfo(0x0020, 2) },
            {  Item.MirrorShield, new EquipmentInfo(0x0040, 3) },

            {  Item.KokiriTunic, new EquipmentInfo(0x0100, 1) },
            {  Item.GoronTunic, new EquipmentInfo(0x0200, 2) },
            {  Item.ZoraTunic, new EquipmentInfo(0x0400, 3) },

            {  Item.KokiriBoots, new EquipmentInfo(0x1000, 1) },
            {  Item.IronBoots, new EquipmentInfo(0x2000, 2) },
            {  Item.HoverBoots, new EquipmentInfo(0x4000, 3) },
        };

        public static bool SetEquipment(Item item, bool value, ref short var)
        {
            if (!Equipment.ContainsKey(item))
            {
                return false;
            }
            var info = Equipment[item];
            int v = (value) ? 1 : 0;
            int leftshift = Shift.GetRight((uint)info.Mask);
            short write = (short)(v << leftshift);

            var &= (short)(-1 ^ info.Mask);
            var |= write;
            return true;
        }

        struct QuestInfo
        {
            public int Mask;

            public QuestInfo(int mask)
            {
                Mask = mask;
            }
        }

        static readonly Dictionary<Item, QuestInfo> QuestItems = new()
        {
            { Item.ForestMedallion, new QuestInfo(0x00_0001) },
            { Item.FireMedallion,   new QuestInfo(0x00_0002) },
            { Item.WaterMedallion, new QuestInfo(0x00_0004) },
            { Item.SpiritMedallion, new QuestInfo(0x00_0008) },
            { Item.ShadowMedallion, new QuestInfo(0x00_0010) },
            { Item.LightMedallion, new QuestInfo(0x00_0020) },
            { Item.Minuet, new QuestInfo(0x00_0040) },
            { Item.Bolero, new QuestInfo(0x00_0080) },

            { Item.Serenade, new QuestInfo(0x00_0100) },
            { Item.Requiem, new QuestInfo(0x00_0200) },
            { Item.Nocturne, new QuestInfo(0x00_0400) },
            { Item.Prelude, new QuestInfo(0x00_0800) },
            { Item.Lullaby, new QuestInfo(0x00_1000) },
            { Item.EponasSong, new QuestInfo(0x00_2000) },
            { Item.SariasSong, new QuestInfo(0x00_4000) },
            { Item.SunsSong, new QuestInfo(0x00_8000) },

            { Item.SongOfTime, new QuestInfo(0x01_0000) },
            { Item.SongOfStorms, new QuestInfo(0x02_0000) },
            { Item.Emerald, new QuestInfo(0x04_0000) },
            { Item.Ruby, new QuestInfo(0x08_0000) },
            { Item.Sapphire, new QuestInfo(0x10_0000) },
            { Item.StoneOfAgony, new QuestInfo(0x20_0000) },
            //{ Item.GerudoCard, new QuestInfo(0x40_0000) },
            //{ Item.GoldSkulltula, new QuestInfo(0x80_0000) },

        };

        public static bool SetQuestItem(Item item, bool setOn, ref int var)
        {
            if (!QuestItems.ContainsKey(item))
            {
                return false;
            }
            var info = QuestItems[item];

            if (setOn == true)
            {
                var |= info.Mask;
            }
            else
            {
                int mask = -1 ^ info.Mask;
                var &= mask;
            }
            return true;
        }
    }
}
