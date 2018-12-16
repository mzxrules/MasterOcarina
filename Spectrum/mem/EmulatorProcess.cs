using System.Collections.Generic;
using System.Diagnostics;

namespace Spectrum
{
    class EmulatorProcess
    {
        public Emulator Stats { get; private set; }
        public Process Process { get; private set; }
        public List<ProcessModule> WatchedModules = new List<ProcessModule>();

        public EmulatorProcess(Emulator stat, Process p)
        {
            Stats = stat;
            Process = p;
        }
    }
}