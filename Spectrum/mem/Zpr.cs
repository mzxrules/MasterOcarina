using System;
using System.Collections.Generic;
using System.Diagnostics;
using mzxrules.Helper;

namespace Spectrum
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
                Console.WriteLine($"{i:D2}) {item.Stats.ProcessName} {item.Process.MainWindowTitle}");
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
                    //if (baseAddress.
                    ramStartExpression = ramStartExpression.Replace
                        (moduleName, baseAddress.ToString("X"));
                    emu.WatchedModules.Add(item);
                }
            }
            Func<long,long> ReadProc;
            if (emu.Stats.ProcessType == 64)
            {
                ReadProc = ReadProcess64;
            }
            else
                ReadProc = ReadProcess32;

            ExpressionEvalulator = new ExpressTest.ExpressionEvaluator(ReadProc);
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
        
        public static long ReadProcess32(long x)
        {
            int result = ReadProcessInt32(Emulator.Process, new IntPtr(x), out int bytesRead);
            return result;
        }
        public static long ReadProcess64(long x)
        {
            return ReadProcessInt64(Emulator.Process, new IntPtr(x), out int bytesRead);
        }
        
        public static long GetEmulatedAddress(N64Ptr addr)
        {
            return (long)IntPtr.Add(RamPointer, (int)(addr & 0xFFFFFF));
        }
        
        /// <summary>
        /// Returns a block of RDRAM in Big Endian order
        /// </summary>
        /// <param name="addr">N64 Pointer to the start of the block of ram</param>
        /// <param name="numOfBytes">Minimum number of bytes to return</param>
        /// <returns></returns>
        public static byte[] ReadRam(N64Ptr addr, int numOfBytes)
        {
            return ReadN64Rdram(addr, numOfBytes, out readBytes, true);
        }

        public static Int64 ReadRamInt64(N64Ptr addr)
        {
            ulong left = ((ulong)ReadRamInt32(addr)) << 32;
            uint right = (uint)ReadRamInt32(addr + 4);

            return (long)(left + right);
        }

        public static Int32 ReadRamInt32(N64Ptr addr)
        {
            var v = ReadN64Rdram(addr, sizeof(Int32), out readBytes, false);
            return BitConverter.ToInt32(v, 0);
        }

        public static float ReadRamFloat(N64Ptr addr)
        {
            var v = ReadN64Rdram(addr, sizeof(float), out readBytes, false);
            return System.BitConverter.ToSingle(v, 0);
        }

        public static Int16 ReadRamInt16(N64Ptr addr)
        {
            var b = ReadN64Rdram(addr, sizeof(Int16), out readBytes, true);
            var v = BitConverter.ToInt16(b, 0);
            return Endian.ConvertInt16(v);
        }

        public static byte ReadRamByte(N64Ptr addr)
        {
            return ReadN64Rdram(addr, sizeof(byte), out readBytes, true)[0];
        }

        public static void WriteRam32(N64Ptr addr, int v)
        {
            byte[] val = BitConverter.GetBytes(Endian.ConvertInt32(v));
            WriteN64RDRAM(addr, val);
        }

        public static void WriteRam32(N64Ptr addr, float v)
        {
            byte[] val = BitConverter.GetBytes(v);
            val.Reverse32();
            WriteN64RDRAM(addr, val);
        }

        public static void WriteRam16(N64Ptr addr, short v)
        {
            WriteN64RDRAM(addr, BitConverter.GetBytes(Endian.ConvertInt16(v)));
        }

        public static void WriteRam8(N64Ptr addr, byte v)
        {
            WriteN64RDRAM(addr, new byte[] { v });
        }

        private static int End16(int addr)
        {
            if (addr % 4 == 0)
                return addr + 2;
            else if (addr % 4 == 2)
                return addr - 2;
            else throw new ArgumentOutOfRangeException("End16 must end in 0 or 2");
        }

        private static int End8(int addr)
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

        /// <summary>
        /// Reads a 4 byte aligned block of emulated ram
        /// </summary>
        /// <param name="process"></param>
        /// <param name="address"></param>
        /// <param name="bytes"></param>
        /// <param name="bytesRead"></param>
        /// <param name="BigEndian"></param>
        /// <returns></returns>
        private static byte[] ReadN64Rdram(N64Ptr pointer, int bytes, out int bytesRead, bool BigEndian)
        {
            Process process = Emulator.Process;
            IntPtr address = IntPtr.Add(RamPointer, (int)(pointer & 0xFFFFFF));

            var addrStart = address.ToInt64();
            int off = (int)(addrStart & 0x3);
            addrStart >>= 2;
            addrStart <<= 2;
            int numOfBytes = (off + bytes + 3) & (unchecked((int)0xFFFFFFFC));
            IntPtr hProc = NativeMethods.OpenProcess(ProcessAccessFlags.All, false, process.Id);

            byte[] buffer = new byte[numOfBytes];
            NativeMethods.ReadProcessMemory(hProc, (IntPtr)addrStart, buffer, (IntPtr)numOfBytes, out bytesRead);
            NativeMethods.CloseHandle(hProc);

            var bigEndian = Emulator.Stats.BigEndian > 0;

            if (bigEndian != BigEndian)
            {
                buffer.Reverse32();
            }
            if (off != 0)
            {
                Array.Copy(buffer, off, buffer, 0, bytes);
            }
            return buffer;
        }

        private static byte[] ReadProcess(Process process, IntPtr address, int bytes, out int bytesRead)
        {
            IntPtr hProc = NativeMethods.OpenProcess(ProcessAccessFlags.All, false, process.Id);

            byte[] buffer = new byte[bytes];
            NativeMethods.ReadProcessMemory(hProc, address, buffer, (IntPtr)bytes, out bytesRead);
            NativeMethods.CloseHandle(hProc);
            
            return buffer;
        }

        private static Int32 ReadProcessInt32(Process process, IntPtr address, out int bytesRead)
        {
            return BitConverter.ToInt32(ReadProcess(process, address, sizeof(Int32), out bytesRead), 0);
        }

        private static Int64 ReadProcessInt64(Process process, IntPtr address, out int bytesRead)
        {
            return BitConverter.ToInt64(ReadProcess(process, address, sizeof(Int64), out bytesRead), 0);
        }
        

        private static bool WriteProcess()
        {
            return false;
            Process process; IntPtr address; long value;
            IntPtr hProc = NativeMethods.OpenProcess(ProcessAccessFlags.All, false, process.Id);

            byte[] val = BitConverter.GetBytes(value);

            bool worked = NativeMethods.WriteProcessMemory(hProc, address, val, (IntPtr)val.LongLength, out int bytesWritten);

            NativeMethods.CloseHandle(hProc);

            return worked;
        }

        /// <summary>
        /// Writes sequence of bytes to emulated RDRAM
        /// </summary>
        /// <param name="pointer">N64 Pointer</param>
        /// <param name="data">Data. Must be in Big Endian form</param>
        /// <returns></returns>
        private static bool WriteN64RDRAM(N64Ptr pointer, byte[] data)
        {
            Process process = Emulator.Process;
            int ptr = (pointer & 0xFFFFFF);

            if (Emulator.Stats.BigEndian > 0)
            {
                IntPtr address = IntPtr.Add(RamPointer, ptr);
                IntPtr hProc = NativeMethods.OpenProcess(ProcessAccessFlags.All, false, process.Id);
                bool worked = NativeMethods.WriteProcessMemory(hProc, address, data, (IntPtr)data.LongLength, out int bytesWritten);
                NativeMethods.CloseHandle(hProc);
                return worked;
            }
            else //little32 aligned
            {
                int offStart = ptr & 0x3;
                int offEnd = (int)((ptr + data.LongLength) & 3);
                
                if (offStart != 0 || offEnd != 0)
                {

                    //get write lengths
                    int w_startLen = (offStart == 0) ? 0 : Math.Min(data.Length, 4 - offStart);
                    int w_midLen = Math.Max(0, data.Length - w_startLen - offEnd);
                    int w_endLen = Math.Max(0, data.Length - w_startLen - w_midLen);
                    
                    byte[] startData = new byte[w_startLen];
                    byte[] midData = new byte[w_midLen];
                    byte[] endData = new byte[w_endLen];
                    byte[] temp = new byte[4];

                    //copy and write data

                    IntPtr hProc = NativeMethods.OpenProcess(ProcessAccessFlags.All, false, process.Id);
                    bool worked = true;
                    
                    if (w_startLen > 0)
                    {
                        int startOffset = End8(offStart) + 1 - w_startLen;
                        //set first write address
                        IntPtr firstWrite = IntPtr.Add(RamPointer, (ptr & 0xFFFFFC) + startOffset);

                        Array.Copy(data, 0, temp, offStart, w_startLen);
                        temp.Reverse32();
                        Array.Copy(temp, startOffset, startData, 0, w_startLen);

                        worked &= NativeMethods.WriteProcessMemory(hProc, firstWrite, startData, (IntPtr)w_startLen, out int bytesWritten);
                    }
                    if (w_midLen > 0)
                    {
                        IntPtr midWrite = IntPtr.Add(RamPointer, ((ptr + w_startLen) & 0xFFFFFC));
                        Array.Copy(data, w_startLen, midData, 0, w_midLen);
                        midData.Reverse32();

                        worked &= NativeMethods.WriteProcessMemory(hProc, midWrite, midData, (IntPtr)w_midLen, out int bytesWritten);
                    }
                    if (w_endLen > 0)
                    {
                        IntPtr endWrite = IntPtr.Add(RamPointer, ((ptr + w_startLen) & 0xFFFFFC) + w_midLen + 4 - w_endLen);
                        Array.Copy(data, w_startLen + w_midLen, temp, 0, w_endLen);
                        temp.Reverse32();
                        Array.Copy(temp, 4 - offEnd, endData, 0, w_endLen);

                        worked &= NativeMethods.WriteProcessMemory(hProc, endWrite, endData, (IntPtr)w_endLen, out int bytesWritten);
                    }

                    NativeMethods.CloseHandle(hProc);
                    return worked;
                }
                else
                {
                    IntPtr address = IntPtr.Add(RamPointer, (int)(pointer & 0xFFFFFF));
                    data.Reverse32();
                    IntPtr hProc = NativeMethods.OpenProcess(ProcessAccessFlags.All, false, process.Id);
                    bool worked = NativeMethods.WriteProcessMemory(hProc, address, data, (IntPtr)data.LongLength, out int bytesWritten);
                    NativeMethods.CloseHandle(hProc);
                    return worked;
                }
            }
        }

        private static bool WriteProcess(Process process, IntPtr address, long value, out int bytesWritten)
        {
            IntPtr hProc = NativeMethods.OpenProcess(ProcessAccessFlags.All, false, process.Id);

            byte[] val = BitConverter.GetBytes(value);

            bool worked = NativeMethods.WriteProcessMemory(hProc, address, val, (IntPtr)val.LongLength, out bytesWritten);

            NativeMethods.CloseHandle(hProc);

            return worked;
        }

        private static bool WriteProcess(Process process, IntPtr address, int value, out int bytesWritten)
        {
            IntPtr hProc = NativeMethods.OpenProcess(ProcessAccessFlags.All, false, process.Id);

            byte[] val = BitConverter.GetBytes(value);

            bool worked = NativeMethods.WriteProcessMemory(hProc, address, val, (IntPtr)val.LongLength, out bytesWritten);

            NativeMethods.CloseHandle(hProc);

            return worked;
        }
        
        private static bool WriteProcess(Process process, IntPtr address, float value, out int bytesWritten)
        {
            IntPtr hProc = NativeMethods.OpenProcess(ProcessAccessFlags.All, false, process.Id);

            byte[] val = BitConverter.GetBytes(value);

            bool worked = NativeMethods.WriteProcessMemory(hProc, address, val, (IntPtr)val.LongLength, out bytesWritten);

            NativeMethods.CloseHandle(hProc);

            return worked;
        }
        
        private static bool WriteProcess(Process process, IntPtr address, short value, out int bytesWritten)
        {
            IntPtr hProc = NativeMethods.OpenProcess(ProcessAccessFlags.All, false, process.Id);

            byte[] val = BitConverter.GetBytes(value);

            bool worked = NativeMethods.WriteProcessMemory(hProc, address, val, (IntPtr)val.LongLength, out bytesWritten);

            NativeMethods.CloseHandle(hProc);

            return worked;
        }

        private static bool WriteProcess(Process process, IntPtr address, byte value, out int bytesWritten)
        {
            IntPtr hProc = NativeMethods.OpenProcess(ProcessAccessFlags.All, false, process.Id);

            bool worked = NativeMethods.WriteProcessMemory(hProc, address, new byte[] { value }, new IntPtr(1), out bytesWritten);

            NativeMethods.CloseHandle(hProc);

            return worked;
        }
        #endregion
    }
}
