using mzxrules.Helper;
using mzxrules.OcaLib;
using System.Collections.Generic;

namespace Spectrum
{
    static class MItems
    {
        public enum Item
        {
            Ocarina,
            Bow,
            FireArrow,
            IceArrow,
            LightArrow,
            Bombs,
            Bombchu,
            Sticks,
            Nuts,
            Beans,
            PowderKeg,
            Pictograph,
            Lens,
            Longshot,
            GreatFairysSword,

            // Bottle Items
            Bottle,
            RedPotion,
            GreenPotion,
            BluePotion,
            Fairy,
            DekuPrincess,
            Milk,
            HalfMilk,
            Fish,
            Bugs,
            BlueFire,
            Poe,
            BigPoe,
            SpringWater,
            HotSpringWater,
            ZoraEgg,
            GoldDust,
            MagicMushroom,
            Seahorse,
            ChateauRomani,

            MoonsTear,
            LandDeed,
            SwampDeed,
            MountainDeed,
            OceanDeed,
            RoomKey,
            LetterMama,
            LetterKafei,
            PendentOfMemories,
            DekuMask,
            GoronMask,
            ZoraMask,
            FDMask,
            MaskOfTruth,
            KafeisMask,
            AllNightMask,
            BunnyHood,
            KeatonMask,
            GarosMask,
            RomanisMask,
            CircusLeadersMask,
            PostmansHat,
            CouplesMask,
            GreatFairyMask,
            GibdoMask,
            DonGerosMask,
            KamarosMask,
            CaptainsHat,
            StoneMask,
            BremenMask,
            BlastMask,
            MaskOfScents,
            GiantsMask,
            KokiriSword,
            RazorSword,
            GildedSword,
            HeroShield,
            MirrorShield,
            Quiver,
            LargeQuiver,
            BiggestQuiver,
            BombBag,
            BigBombBag,
            BiggestBombBag,
            OdalwaRemains,
            GohtRemains,
            GyorgRemains,
            TwinmoldRemains,
            SonataOfAwakening,
            GoronLullaby,
            NewWaveBossaNova,
            ElegyOfEmptiness,
            OathToOrder,
            SongOfTime,
            SongOfHealing,
            EponasSong,
            SongOfSoaring,
            SongOfStorms,
            BombersNotebook,
            LullabyIntro,
            BossKey,
            Compass,
            DungeonMap,
            StrayFairy,
            None,
            //Non-standard items
            Magic,
            DoubleMagic
        }

        public static Dictionary<Item, (int international, int jpn)> ItemIds = new()
        {
            { Item.Ocarina, (0x00, 0x00) },
            { Item.Bow, (0x01, 0x01) },
            { Item.FireArrow, (0x02, 0x02) },
            { Item.IceArrow, (0x03, 0x03) },
            { Item.LightArrow, (0x04, 0x04) },
            { Item.Bombs, (0x06, 0x06) },
            { Item.Bombchu, (0x07, 0x07) },
            { Item.Sticks, (0x08, 0x08) },
            { Item.Nuts, (0x09, 0x09) },
            { Item.Beans, (0x0A, 0x0A) },
            { Item.PowderKeg, (0x0C, 0x0C) },
            { Item.Pictograph, (0x0D, 0x0D) },
            { Item.Lens, (0x0E, 0x0E) },
            { Item.Longshot, (0x0F, 0x0F) },
            { Item.GreatFairysSword, (0x10, 0x10) },
            { Item.Bottle, (0x12, 0x12) },
            { Item.RedPotion, (0x13, 0x13) },
            { Item.GreenPotion, (0x14, 0x14) },
            { Item.BluePotion, (0x15, 0x15) },
            { Item.Fairy, (0x16, 0x16) },
            { Item.DekuPrincess, (0x17, 0x17) },
            { Item.Milk, (0x18, 0x18) },
            { Item.HalfMilk, (0x19, 0x19) },
            { Item.Fish, (0x1A, 0x1A) },
            { Item.Bugs, (0x1B, 0x1B) },
            { Item.BlueFire, (0x1C, 0x1C) },
            { Item.Poe, (0x1D, 0x1D) },
            { Item.BigPoe, (0x1E, 0x1E) },
            { Item.SpringWater, (0x1F, 0x1F) },
            { Item.HotSpringWater, (0x20, 0x20) },
            { Item.ZoraEgg, (0x21, 0x21) },
            { Item.GoldDust, (0x22, 0x22) },
            { Item.MagicMushroom, (0x23, 0x23) },
            { Item.Seahorse, (0x24, 0x26) },
            { Item.ChateauRomani, (0x25, 0x27) },
            { Item.MoonsTear, (0x28, 0x30) },
            { Item.LandDeed, (0x29, 0x31) },
            { Item.SwampDeed, (0x2A, 0x32) },
            { Item.MountainDeed, (0x2B, 0x33) },
            { Item.OceanDeed, (0x2C, 0x34) },
            { Item.RoomKey, (0x2D, 0x3A) },
            { Item.LetterMama, (0x2E, 0x3B) },
            { Item.LetterKafei, (0x2F, 0x44) },
            { Item.PendentOfMemories, (0x30, 0x45) },
            { Item.DekuMask, (0x32, 0x4E) },
            { Item.GoronMask, (0x33, 0x4F) },
            { Item.ZoraMask, (0x34, 0x50) },
            { Item.FDMask, (0x35, 0x51) },
            { Item.MaskOfTruth, (0x36, 0x52) },
            { Item.KafeisMask, (0x37, 0x53) },
            { Item.AllNightMask, (0x38, 0x54) },
            { Item.BunnyHood, (0x39, 0x55) },
            { Item.KeatonMask, (0x3A, 0x56) },
            { Item.GarosMask, (0x3B, 0x57) },
            { Item.RomanisMask, (0x3C, 0x58) },
            { Item.CircusLeadersMask, (0x3D, 0x59) },
            { Item.PostmansHat, (0x3E, 0x5A) },
            { Item.CouplesMask, (0x3F, 0x5B) },
            { Item.GreatFairyMask, (0x40, 0x5C) },
            { Item.GibdoMask, (0x41, 0x5D) },
            { Item.DonGerosMask, (0x42, 0x5E) },
            { Item.KamarosMask, (0x43, 0x5F) },
            { Item.CaptainsHat, (0x44, 0x60) },
            { Item.StoneMask, (0x45, 0x61) },
            { Item.BremenMask, (0x46, 0x62) },
            { Item.BlastMask, (0x47, 0x63) },
            { Item.MaskOfScents, (0x48, 0x64) },
            { Item.GiantsMask, (0x49, 0x65) },
            { Item.KokiriSword, (0x4D, 0x6C) },
            { Item.RazorSword, (0x4E, 0x6D) },
            { Item.GildedSword, (0x4F, 0x6E) },
            { Item.HeroShield, (0x51, 0x70) },
            { Item.MirrorShield, (0x52, 0x71) },
            { Item.Quiver, (0x53, 0x73) },
            { Item.LargeQuiver, (0x54, 0x74) },
            { Item.BiggestQuiver, (0x55, 0x75) },
            { Item.BombBag, (0x56, 0x76) },
            { Item.BigBombBag, (0x57, 0x77) },
            { Item.BiggestBombBag, (0x58, 0x78) },
            { Item.OdalwaRemains, (0x5D, 0x7D) },
            { Item.GohtRemains, (0x5E, 0x7E) },
            { Item.GyorgRemains, (0x5F, 0x7F) },
            { Item.TwinmoldRemains, (0x60, 0x80) },
            { Item.SonataOfAwakening, (0x61, 0x81) },
            { Item.GoronLullaby, (0x62, 0x82) },
            { Item.NewWaveBossaNova, (0x63, 0x83) },
            { Item.ElegyOfEmptiness, (0x64, 0x84) },
            { Item.OathToOrder, (0x65, 0x85) },
            { Item.SongOfTime, (0x67, 0x87) },
            { Item.SongOfHealing, (0x68, 0x88) },
            { Item.EponasSong, (0x69, 0x89) },
            { Item.SongOfSoaring, (0x6A, 0x8A) },
            { Item.SongOfStorms, (0x6B, 0x8B) },
            { Item.BombersNotebook, (0x6D, 0x8D) },
            { Item.LullabyIntro, (0x73, 0x93) },
            { Item.BossKey, (0x74, 0x94) },
            { Item.Compass, (0x75, 0x95) },
            { Item.DungeonMap, (0x76, 0x96) },
            { Item.StrayFairy, (0x77, 0x97) },
            { Item.None, (0xFF, 0xFF) },
            //Non-standard items
            { Item.Magic, (0xFF, 0xFF) },
            { Item.DoubleMagic, (0xFF, 0xFF) },
        };

        const int MASK_NUTS         = 0x00700000;
        const int MASK_STICKS       = 0x000E0000;
        const int MASK_BULLET_BAG   = 0x0001C000;
        const int MASK_WALLET       = 0x00003000;
        const int MASK_SCALE        = 0x00000E00;
        const int MASK_STRENGTH     = 0x000001C0;
        const int MASK_BOMBS        = 0x00000038;
        const int MASK_QUIVER       = 0x00000007;

        struct InventoryInfo
        {
            public int Slot;
            public bool Ammo;
            public Item LeftEquipment;

            public InventoryInfo(int slot, bool ammo = false, Item cItem = Item.None)
            {
                Slot = slot;
                Ammo = ammo;
                LeftEquipment = cItem;
            }

            public static implicit operator InventoryInfo(int slot)
            {
                return new InventoryInfo(slot);
            }
        }

        static readonly Dictionary<Item, InventoryInfo> InventorySlot = new()
        {
            { Item.Ocarina, 0 },
            { Item.Bow, new InventoryInfo(1, true, Item.Bow) },
            { Item.FireArrow, 2 },
            { Item.IceArrow, 3 },
            { Item.LightArrow, 4 },
            { Item.Bombs, new InventoryInfo(6, true, Item.Bombs) },
            { Item.Bombchu, new InventoryInfo(7, true, Item.Bombchu) },
            { Item.Sticks, new InventoryInfo(8, true, Item.Sticks) },
            { Item.Nuts, new InventoryInfo(9, true, Item.Nuts) },
            { Item.Beans, new InventoryInfo(10, true, Item.Beans) },
            { Item.PowderKeg, new InventoryInfo(12, true, Item.PowderKeg) },
            { Item.Pictograph, 13 },
            { Item.Lens, 14 },
            { Item.Longshot, 15 },
            { Item.GreatFairysSword, 16 },
            { Item.PostmansHat, 24 },
            { Item.AllNightMask, 25 },
            { Item.BlastMask, 26 },
            { Item.StoneMask, 27 },
            { Item.GreatFairyMask, 28 },
            { Item.DekuMask, 29 },
            { Item.KeatonMask, 30 },
            { Item.BremenMask, 31 },
            { Item.BunnyHood, 32 },
            { Item.DonGerosMask, 33 },
            { Item.MaskOfScents, 34 },
            { Item.GoronMask, 35 },
            { Item.RomanisMask, 36},
            { Item.CircusLeadersMask, 37 },
            { Item.KafeisMask, 38 },
            { Item.CouplesMask, 39 },
            { Item.MaskOfTruth, 40 },
            { Item.ZoraMask, 41 },
            { Item.KamarosMask, 42 },
            { Item.GibdoMask, 43 },
            { Item.GarosMask, 44 },
            { Item.CaptainsHat, 45 },
            { Item.GiantsMask, 46 },
            { Item.FDMask, 47 }
        };

        public static int GetItemId(RomVersion ver, Item item)
        {
            var idGroup = ItemIds[item];

            if (ver == MRom.Build.J0 || ver == MRom.Build.J1)
            {
                return idGroup.jpn;
            }
            else
            {
                return idGroup.international;
            }
        }

        public static void SetInventoryItem(RomVersion ver, Item itemKey, Ptr saveCtx)
        {
            Ptr inventPtr = saveCtx.RelOff(0x70);
            int itemId = GetItemId(ver, itemKey);

            // Set Bottle
            if (itemKey is >= Item.Bottle and <= Item.ChateauRomani)
            {
                inventPtr.Write(18, (byte)itemId);
            }
            // Set Magic
            else if (itemKey is Item.Magic or Item.DoubleMagic)
            {
                saveCtx.Write(
                    0x38, (byte)0,
                    0x39, (byte)(itemKey == Item.Magic ? 0x30 : 0x60),
                    0x40, (byte)1,
                    0x41, (byte)(itemKey == Item.Magic ? 0 : 1));
            }
            // Set Inventory Item
            else if (InventorySlot.ContainsKey(itemKey))
            {
                InventoryInfo info = InventorySlot[itemKey];

                inventPtr.Write(info.Slot, (byte)itemId);

                SetInventoryItemAmmo(itemKey, 50, saveCtx);

                if (info.Ammo && info.LeftEquipment != Item.None)
                {
                    int equipment = saveCtx.ReadInt32(0xB8);
                    SetLeftEquipmentItem(itemKey, 1, ref equipment);
                    saveCtx.Write(0xB8, equipment);
                }
            }
        }

        public static void SetInventoryItemAmmo(Item item, byte amount, Ptr saveCtx)
        {
            Ptr ammoPtr = saveCtx.RelOff(0xA0);
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
            //{ Item.Slingshot, new LeftEquipmentInfo(MASK_BULLET_BAG, 3) },
            { Item.Sticks, new LeftEquipmentInfo(MASK_STICKS, 3) },
            //{ Item.Strength, new LeftEquipmentInfo(MASK_STRENGTH, 3) },
            //{ Item.Scale, new LeftEquipmentInfo(MASK_SCALE, 2) },
            //{ Item.Wallet, new LeftEquipmentInfo(MASK_WALLET, 3) },
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

    }
}
