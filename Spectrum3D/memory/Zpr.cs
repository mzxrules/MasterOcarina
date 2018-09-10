using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace Spectrum3D.memory
{
    class EmulatorProcess
    {
        public Emulator Stats { get; private set; }
        public Process Process { get; private set; }
        public List<ProcessModule> WatchedModules = new List<ProcessModule>();

        public EmulatorProcess (Emulator stat, Process p)
        {
            Stats = stat;
            Process = p;
        }
    }
    class Zpr
    {
        static EmulatorProcess Emulator;
        //static Process Emulator;
        static IntPtr RamPointer;
        static int readBytes;

        public static bool IsEmulatorSet { get { return Emulator != null; } }

        public static bool TryMountEmulator(List<Emulator> emulators)
        {
            bool result;
            EmulatorProcess emulator = SelectEmulator(emulators);
            if (emulator == null)
                return false;

            result = MountEmulator(emulator);
            if (result)
                Console.WriteLine(emulator.Stats.ProcessDescription);
            return result;
        }

        private static EmulatorProcess SelectEmulator(List<Emulator> emulators)
        {
            List<EmulatorProcess> foundEmulators = new List<EmulatorProcess>();
            foreach (var item in emulators)
            {
                foreach (var p in Process.GetProcessesByName(item.ProcessName))
                {
                    var t = new EmulatorProcess(item, p);
                    foundEmulators.Add(t);
                }
            }

            if (foundEmulators.Count == 0)
            {
                return null;
            }
            if (foundEmulators.Count == 1)
            {
                return foundEmulators[0];
            }

            Console.Clear();
            int i = 1;
            foreach (var item in foundEmulators)
            {
                Console.WriteLine(string.Format("{0:D2}) {1} {2}", i, item.Stats.ProcessName, item.Process.MainWindowTitle));
                i++;
            }
            Console.Write("Select Emulator: ");
            var input = Console.ReadLine();

            if (!int.TryParse(input, out i))
                return null;

            i--;
            if (i < 0 || i >= foundEmulators.Count)
                return null;

            return foundEmulators[i];
        }

        private static bool MountEmulator(EmulatorProcess emu)
        {
            ExpressTest.ExpressionEvaluator ExpressionEvalulator;
            string ramStartExpression = emu.Stats.RamStart;
            Emulator = emu;//.Process;

            if (Emulator == null)
                return false;

            foreach (ProcessModule item in Emulator.Process.Modules)
            {
                IntPtr baseAddress = item.BaseAddress;
                string moduleName = "`" + item.ModuleName.ToLowerInvariant() + "`";
                if (ramStartExpression.Contains(moduleName))
                {
                    ramStartExpression = ramStartExpression.Replace
                        (moduleName, baseAddress.ToString("X"));
                    emu.WatchedModules.Add(item);
                }
            }

            ExpressionEvalulator = new ExpressTest.ExpressionEvaluator(ReadProcess);
            RamPointer = new IntPtr(ExpressionEvalulator.Evaluate(ramStartExpression));

            if (RamPointer == new IntPtr(0))
                return false;

            return true;
        }

        public static void GetContainingModule(IntPtr address)
        {
            if (Emulator == null)
            {
                Console.WriteLine("No Emulator!");
                return;
            }
            foreach(ProcessModule item in Emulator.Process.Modules)
            {
                IntPtr baseAddress = item.BaseAddress;
                int size = item.ModuleMemorySize;

                long base64 = baseAddress.ToInt64();
                long addr64 = address.ToInt64();

                if (base64 <= addr64
                    && (addr64 < base64 + size))
                {
                    Console.WriteLine($"{item.ModuleName} - ADDR: {base64:X8} SIZE: {size:X8} OFF: {addr64-base64:X8}");
                }
            }
        }
        
        public static long ReadProcess(long x)
        {
            int bytesRead;
            int result = ReadMemoryInt32(Emulator.Process, new IntPtr(x), out bytesRead);
            return result;
        }
        
        public static long GetEmulatedAddress(int addr)
        {
            return (long)(ResolveAddr(addr));
        }

        public static byte[] ReadMem(IntPtr addr, int numOfBytes)
        {
            return ReadMemory(Emulator.Process, addr, numOfBytes, out readBytes);
        }

        /// <summary>
        /// Resolves address for Citra 5
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        private static IntPtr ResolveAddrOld(int addr)
        {
            var pagingPtr = RamPointer + 0x1B3558;
            var pagingTbl = new IntPtr(BitConverter.ToInt64(ReadMem(pagingPtr, 8), 0));

            pagingTbl += ((addr >> 12) * 8);
            var pagePtr = new IntPtr(BitConverter.ToInt64(ReadMem(pagingTbl, 8), 0));
            return pagePtr + (addr & 0xFFF);
        }
        private static IntPtr ResolveAddr(int addr)
        {
            var pagingTbl = RamPointer + 0x309490;

            pagingTbl += ((addr >> 12) * 8);
            var pagePtr = new IntPtr(BitConverter.ToInt64(ReadMem(pagingTbl, 8), 0));
            return pagePtr + (addr & 0xFFF);
        }

        public static byte[] ReadRam(int addr, int numOfBytes)
        {
            IntPtr address = ResolveAddr(addr);
            return ReadMemory(Emulator.Process, address, numOfBytes, out readBytes);
        }


        public static Int32 ReadRamInt32(int addr)
        {
            IntPtr address = ResolveAddr(addr);
            return ReadMemoryInt32(Emulator.Process, address, out readBytes);
        }

        public static Int16 ReadRamInt16(int addr)
        {
            IntPtr address = ResolveAddr(addr);
            return ReadMemoryInt16(Emulator.Process, address, out readBytes);
        }

        public static Byte ReadRamByte(int addr)
        {
            IntPtr address = ResolveAddr(addr);
            return ReadMemoryByte(Emulator.Process, address, out readBytes);
        }

        public static void WriteRam32(int addr, int v)
        {
            int n;
            IntPtr address = ResolveAddr(addr);
            WriteMemory(Emulator.Process, address, v, out n);
        }

        public static void WriteRam32(int addr, float v)
        {
            int n;
            IntPtr address = ResolveAddr(addr);
            WriteMemory(Emulator.Process, address, v, out n);
        }

        public static void WriteRam16(int addr, short v)
        {
            int n;
            IntPtr address = ResolveAddr(addr);
            WriteMemory(Emulator.Process, address, v, out n);
        }

        public static void WriteRam8(int addr, byte v)
        {
            int n;
            IntPtr address = ResolveAddr(addr);
            WriteMemory(Emulator.Process, address, v, out n);
        }

        public static int End16(int addr)
        {
            if (addr % 4 == 0)
                return addr + 2;
            else if (addr % 4 == 2)
                return addr - 2;
            else throw new ArgumentOutOfRangeException("End16 must end in 0 or 2");
        }

        public static int End8(int addr)
        {
            switch (addr % 4)
            {
                case 0: addr += 3; break;
                case 1: addr += 1; break;
                case 2: addr -= 1; break;
                case 3: addr -= 3; break;
            }
            return addr;
        }

        
        #region K32 Functions
        
        private static byte[] ReadMemory(Process process, IntPtr address, int numOfBytes, out int bytesRead)
        {
            IntPtr hProc = NativeMethods.OpenProcess(ProcessAccessFlags.All, false, process.Id);

            byte[] buffer = new byte[numOfBytes];

            NativeMethods.ReadProcessMemory(hProc, address, buffer, (IntPtr)numOfBytes, out bytesRead);
            NativeMethods.CloseHandle(hProc);
            return buffer;
        }

        private static Int32 ReadMemoryInt32(Process process, IntPtr address, out int bytesRead)
        {
            return BitConverter.ToInt32(ReadMemory(process, address, sizeof(Int32), out bytesRead), 0);
        }
        private static Int16 ReadMemoryInt16(Process process, IntPtr address, out int bytesRead)
        {
            return BitConverter.ToInt16(ReadMemory(process, address, sizeof(Int16), out bytesRead), 0);
        }
        private static Byte ReadMemoryByte(Process process, IntPtr address, out int bytesRead)
        {
            return ReadMemory(process, address, sizeof(byte), out bytesRead)[0];
        }


        private static bool WriteMemory(Process process, IntPtr address, long value, out int bytesWritten)
        {
            IntPtr hProc = NativeMethods.OpenProcess(ProcessAccessFlags.All, false, process.Id);

            byte[] val = BitConverter.GetBytes(value);

            bool worked = NativeMethods.WriteProcessMemory(hProc, address, val, (IntPtr)val.LongLength, out bytesWritten);

            NativeMethods.CloseHandle(hProc);

            return worked;
        }

        private static bool WriteMemory(Process process, IntPtr address, int value, out int bytesWritten)
        {
            IntPtr hProc = NativeMethods.OpenProcess(ProcessAccessFlags.All, false, process.Id);

            byte[] val = BitConverter.GetBytes(value);

            bool worked = NativeMethods.WriteProcessMemory(hProc, address, val, (IntPtr)val.LongLength, out bytesWritten);

            NativeMethods.CloseHandle(hProc);

            return worked;
        }
        
        private static bool WriteMemory(Process process, IntPtr address, float value, out int bytesWritten)
        {
            IntPtr hProc = NativeMethods.OpenProcess(ProcessAccessFlags.All, false, process.Id);

            byte[] val = BitConverter.GetBytes(value);

            bool worked = NativeMethods.WriteProcessMemory(hProc, address, val, (IntPtr)val.LongLength, out bytesWritten);

            NativeMethods.CloseHandle(hProc);

            return worked;
        }


        private static bool WriteMemory(Process process, IntPtr address, short value, out int bytesWritten)
        {
            IntPtr hProc = NativeMethods.OpenProcess(ProcessAccessFlags.All, false, process.Id);

            byte[] val = BitConverter.GetBytes(value);

            bool worked = NativeMethods.WriteProcessMemory(hProc, address, val, (IntPtr)val.LongLength, out bytesWritten);

            NativeMethods.CloseHandle(hProc);

            return worked;
        }

        private static bool WriteMemory(Process process, IntPtr address, byte value, out int bytesWritten)
        {
            IntPtr hProc = NativeMethods.OpenProcess(ProcessAccessFlags.All, false, process.Id);

            bool worked = NativeMethods.WriteProcessMemory(hProc, address, new byte[] { value }, new IntPtr(1), out bytesWritten);

            NativeMethods.CloseHandle(hProc);

            return worked;
        }
        #endregion
    }
}
