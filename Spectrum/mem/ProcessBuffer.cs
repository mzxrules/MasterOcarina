using System;
using System.Diagnostics;

namespace Spectrum
{
    partial class Zpr
    {
        class ProcessBuffer
        {
            const int BUFFER_SIZE = 0x10000;
            readonly byte[][] buffer = new byte[2][];
            readonly long[] addr = new long[3];

            public void Initialize(Process proc, IntPtr address)
            {
                addr[0] = (long)address + BUFFER_SIZE + BUFFER_SIZE;
                addr[1] = (long)address + BUFFER_SIZE;
                addr[2] = (long)address;
                buffer[0] = ReadProcess(proc, (IntPtr)addr[1], BUFFER_SIZE, out _);
                buffer[1] = ReadProcess(proc, (IntPtr)addr[2], BUFFER_SIZE, out _);
            }

            public int ReadInt32(Process proc, IntPtr address)
            {
                long a = (long)address;
                for (int i = 0; i < 2; i++)
                {
                    long start = addr[i + 1];
                    long end = addr[i];

                    if (start <= a && a < end)
                    {
                        return BitConverter.ToInt32(buffer[i], (int)(a - start));
                    }
                }
                if (addr[0] <= a && a <= addr[0] + BUFFER_SIZE)
                {
                    buffer[1] = buffer[0];
                    addr[2] = addr[1];
                    addr[1] = addr[0];
                    addr[0] += BUFFER_SIZE;
                    buffer[0] = ReadProcess(proc, (IntPtr)addr[1], BUFFER_SIZE, out _);
                    return ReadInt32(proc, address);
                }
                else
                {
                    Initialize(proc, address);
                    return ReadInt32(proc, address);
                }
            }
        }
    }
}