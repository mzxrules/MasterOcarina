using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spectrum
{
    enum FlagOperations
    {
        On,
        Off,
        Toggle,
        AllOn,
        AllOff
    }

    enum OFlags
    {
        event_chk_inf,
        item_get_inf,
        inf_table,
        scene_switch,
        scene_chest,
        scene_clear,
        scene_collect,
        scene_rooms,
        scene_floors
    }

    public static class OFlagsOperation
    {
        public static void Process(string flagGroup, string flagOperation, int? flagId = null)
        {
            bool parsedGroup = Enum.TryParse(flagGroup, true, out OFlags flagType);
            bool parsedOperation = Enum.TryParse(flagOperation, true, out FlagOperations flagOp);

            if (!parsedGroup || !parsedOperation)
            {
                if (!parsedGroup)
                {
                    Console.WriteLine("Invalid flag set name, expected the following:");
                    foreach (var item in Enum.GetValues(typeof(OFlags)).Cast<OFlags>())
                    {
                        Console.WriteLine(item.ToString());
                    }
                }
                if (!parsedOperation)
                {
                    Console.WriteLine("Invalid operation, expected the following:");
                    foreach (var item in Enum.GetValues(typeof(FlagOperations)).Cast<FlagOperations>())
                    {
                        Console.WriteLine(item.ToString());
                    }
                }
                return;
            }
            if (flagOp == FlagOperations.On || flagOp == FlagOperations.Off || flagOp == FlagOperations.Toggle)
            {
                if (flagId == null) //&& flagType != OFlags.scene_clear)
                {
                    Console.WriteLine("No flag id specified.");
                    return;
                }
                var fId = flagId ?? 0;
                switch (flagType)
                {
                    case OFlags.event_chk_inf: SetEventChkInf(flagOp, SpectrumVariables.SaveContext, fId); break;
                    case OFlags.scene_switch: SetSceneFlag(flagOp, SpectrumVariables.GlobalContext.RelOff(0x1D28), fId); break;
                    case OFlags.scene_chest: SetSceneFlag(flagOp, SpectrumVariables.GlobalContext.RelOff(0x1D30), fId); break;
                    case OFlags.scene_clear: SetSceneFlag(flagOp, SpectrumVariables.GlobalContext.RelOff(0x1D3C), fId); break;
                    case OFlags.scene_collect: SetSceneFlag(flagOp, SpectrumVariables.GlobalContext.RelOff(0x1D44), fId); break;
                }
                return;
            }
            else
            {
                Console.WriteLine("Not Implemented");
            }
        }

        private static void SetSceneFlag(FlagOperations op, Ptr baseAddr, int id)
        {
            if (id < 0 || id > 0x3F)
            {
                Console.WriteLine("Flag Id is not between 0x00 and 0x3F");
                return;
            }
            int value = baseAddr.ReadInt32(0);
            if (id >= 0x20)
            {
                baseAddr = baseAddr.RelOff(4);
                id -= 0x20;
            }
            if (op == FlagOperations.Off)
            {
                int mask = ~(1 << id);
                value &= mask;
            }
            else if (op == FlagOperations.On)
            {
                value |= 1 << id;
            }
            else if (op == FlagOperations.Toggle)
            {
                value ^= 1 << id;
            }
            baseAddr.Write(0, value);
            Console.WriteLine($"{baseAddr}: {value:X8}");
        }

        private static void SetEventChkInf(FlagOperations op, Ptr saveCtx, int id)
        {
            if (id < 0 || id >= 0xE0)
            {
                Console.WriteLine("Flag ID is not between 0x00 and 0xDF");
                return;
            }
            Ptr off = saveCtx.RelOff(id / 0x10 * 2 + 0xED4);
            ushort value = off.ReadUInt16(0);
            ushort shift = (ushort)(1 << (id % 0x10));
            if (op == FlagOperations.Off)
            {
                ushort mask = (ushort)~shift;
                value &= mask;
            }
            else if (op == FlagOperations.On)
            {
                value |= shift;
            }
            else if (op == FlagOperations.Toggle)
            {
                value ^= shift;
            }
            off.Write(0, value);
            Console.WriteLine($"{off}: {value:X4}");
        }
    }
}