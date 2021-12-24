using mzxrules.OcaLib;
using System;

namespace Spectrum
{
    public class CommandRequest
    {
        public string CommandName { get; private set; } = "";
        public string Arguments { get; private set; } = "";
        string Input;

        public CommandRequest(string args)
        {
            int CommandArgsIndex;
            Input = args.Trim();

            //set command name
            if (Input.StartsWith("="))
            {
                CommandName = "=";
                CommandArgsIndex = 1;
            }
            else
            {
                var index = Input.IndexOf(' ');
                if (index < 0)
                {
                    CommandName = Input.ToLower();
                    CommandArgsIndex = Input.Length;
                }
                else
                {
                    CommandName = Input.Substring(0, index).ToLower();
                    CommandArgsIndex = index + 1;
                }
            }

            Arguments = Input.Substring(CommandArgsIndex).TrimStart();
        }
    }


    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    class SpectrumCommand : Attribute
    {
        public string Name;
        public Category Cat = Category.Unsorted;
        public string Description;
        public Supported Sup = Supported.All;

        public enum Category
        {
            Help = -1,
            Spectrum = 0,
            Ram,
            Spawn,
            Actor,
            VerboseOcarina,
            Gfx,
            Gbi,
            Gbi_bin,
            Framebuffer,
            Collision,
            Conversion,
            Write,
            Item,
            Proto,
            Deprecated,
            Unsorted = 0x1000
        }

        [Flags]
        public enum Supported
        {
            None = 0,
            All = 1,
            OoT = 2,
            MM = 4,
        }
        public string PrintSupportedVersions()
        {
            string o = (Sup & (Supported.OoT | Supported.All)) != Supported.None ? "O" : " ";
            return o + ((Sup & (Supported.MM | Supported.All)) != Supported.None ? "M" : " ");
        }

        public bool IsSupported(RomVersion version)
        {
            if ((Sup & Supported.All) == Supported.All)
            {
                return true;
            }
            if (version.Game == Game.OcarinaOfTime)
            {
                return (Sup & Supported.OoT) == Supported.OoT;
            }
            if (version.Game == Game.MajorasMask)
            {
                return (Sup & Supported.MM) == Supported.MM;
            }
            return false;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    class SpectrumCommandSignature : Attribute
    {
        public Tokens[] Sig = Array.Empty<Tokens>();
        public int SigId = 0;
        public string Help;
    }
}
