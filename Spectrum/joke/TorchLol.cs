using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Spectrum
{
    static class TorchLol
    {
        const short TORCH_TIMER = 0x264;
        const int TORCH_TIMER_OFFSET = 0x1D4;
        const int TORCHES_LIT_OFFSET = 0xC30;
        static Random Rand = new Random();

        static Timer t;

        class Test
        {
            public int Inc { get; set; }
            public int RandTick { get; set; }

            public Test()
            {
                Inc = 0;
                RandTick = -1;
            }
            
        }

        public static void End()
        {
            if (t != null)
                t.Dispose();
        }

        public static void Method()
        {
            Test obj = new Test();
            if (t != null)
                t.Dispose();

            t = new Timer(callback, obj, new TimeSpan(0), new TimeSpan(500000));
        }

        private static void callback(object state)
        {
            Test obj = (Test)state;

            
            if (obj.RandTick < 0)
            {
                obj.RandTick = Rand.Next(8, 26);
            }
            else if (obj.RandTick == 0)
            {
                List<ActorInstance> unlitTorches = new List<ActorInstance>();
                var map = ActorMemoryMapper.FetchFilesAndInstances();

                var allTorches = map.Instances
                    .Where(x => x.Actor == 0x5E).ToList();


                foreach(var item in allTorches)
                {
                    Ptr localTorch = SPtr.New(item.Ram.Start);
                    var time = localTorch.ReadInt16(TORCH_TIMER_OFFSET);
                    if (time <= 0)
                        unlitTorches.Add(item);
                }

                if (unlitTorches.Count > 0)
                {
                    var torchOvl = map.Files.Where(x => x.Actor == 0x5E).SingleOrDefault();
                    var select = Rand.Next(0, unlitTorches.Count);

                    Ptr torch = SPtr.New(unlitTorches[select].Ram.Start);

                    torch.Write(TORCH_TIMER_OFFSET, TORCH_TIMER);

                    if (torchOvl != null)
                    {
                        Ptr ovl = SPtr.New(torchOvl.Ram.Start);
                        int lit = ovl.ReadInt32(TORCHES_LIT_OFFSET);
                        lit++;
                        ovl.Write(TORCHES_LIT_OFFSET, lit);
                    }
                    //Console.WriteLine(string.Format("Light! {0:X6}", (int)torch));
                }
            }
            obj.RandTick--;
        }
    }
}
