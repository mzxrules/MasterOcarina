using mzxrules.OcaLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Experimental
{

    class FuncSelector
    {
        enum FuncType
        {
            NOPARAMS,
            STRLIST,
            EXPERIMENTFACE_STRING,
            EXPERIMENTFACE_ROMS,
            NOPARAMS_EXPERIMENTFACE
        }

        public string DisplayName { get; private set; }
        private readonly FuncType type;
        private Action FunctionA { get; set; }
        private Action<List<string>> FunctionB { get; set; }
        private Action<IExperimentFace, List<string>> FunctionC { get; set; }
        private Action<IExperimentFace> FunctionD { get; set; }
        private string[] FunctionParams { get; set; }
        private RomVersion[] RomParams { get; set; }
        public static IExperimentFace ExperimentFace;

        public FuncSelector(string name, Action f)
        {
            DisplayName = name;
            FunctionA = f;
            type = FuncType.NOPARAMS;
        }
        public FuncSelector(string name, Action<IExperimentFace> f)
        {
            DisplayName = name;
            FunctionD = f;
            type = FuncType.NOPARAMS_EXPERIMENTFACE;
        }
        //public FuncSelector(string name, Action<List<string>> f, params string[] args)
        //{
        //    DisplayName = name;
        //    FunctionB = f;
        //    type = FuncType.STRLIST;
        //    FunctionParams = args;
        //}

        public FuncSelector(string name, Action<IExperimentFace, List<string>> f, params string[] args)
        {
            DisplayName = name;
            FunctionC = f;
            type = FuncType.EXPERIMENTFACE_STRING;
            FunctionParams = args;
        }

        public FuncSelector(string name, Action<IExperimentFace, List<string>> f, params RomVersion[] args)
        {
            DisplayName = name;
            FunctionC = f;
            type = FuncType.EXPERIMENTFACE_ROMS;
            RomParams = args;
        }

        public void Function()
        {
            switch (type)
            {
                case FuncType.NOPARAMS: FunctionA(); break;
                case FuncType.STRLIST:
                    {
                        if (ExperimentFace.GetFileDialogs(out List<string> files, FunctionParams))
                        {
                            FunctionB(files);
                        }
                        break;
                    }
                case FuncType.EXPERIMENTFACE_STRING:
                    {
                        if (ExperimentFace.GetFileDialogs(out List<string> files, FunctionParams))
                        {
                            FunctionC(ExperimentFace, files);
                        }
                        break;
                    }
                case FuncType.EXPERIMENTFACE_ROMS:
                    {
                        if (ExperimentFace.GetFileDialogs(out List<string> files, RomParams))
                        {
                            FunctionC(ExperimentFace, files);
                        }
                        break;
                    }
                case FuncType.NOPARAMS_EXPERIMENTFACE:
                    {
                        FunctionD(ExperimentFace);
                    }
                    break;
            }
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }

}
