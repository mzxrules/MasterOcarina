using mzxrules.OcaLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum
{
    public delegate void LegacyCommandDelegate(Arguments args);

    public class Command
    {
        public string Id { get; private set; }
        public string Description { get; private set; }
        public LegacyCommandDelegate CommandAction { get; private set; }
        public Command(string id, LegacyCommandDelegate d, string description)
        {
            Id = id;
            Description = description;
            CommandAction = d;
        }
    }

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

        public string[] Legacy()
        {
            string readLine = Input;
            var args = readLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (args.Length == 0)
                args = new string[] { "" };
            return args;
        }
    }


    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    class SpectrumCommand : Attribute
    {
        public string Name;
        public Category Cat = Category.Unsorted;
        public string Description;
        public Supported Sup = Supported.OoT | Supported.MM;

        public enum Category
        {
            Help = -1,
            Spectrum = 0,
            Ram,
            Spawn,
            Actor,
            Gfx,
            Gbi,
            Gbi_bin,
            Framebuffer,
            Collision,
            Conversion,
            Write,
            Item,
            Proto,
            Unsorted = 0x1000
        }

        [Flags]
        public enum Supported
        {
            OoT = 1,
            MM = 2
        }
        public string PrintSupportedVersions()
        {
            string o = (Sup & Supported.OoT) == Supported.OoT ? "O" : " ";
            return o + ((Sup & Supported.MM) == Supported.MM ? "M" : " ");
        }

        public bool IsSupported(RomVersion version)
        {
            if (version.Game == Game.OcarinaOfTime)
                return (Sup & Supported.OoT) == Supported.OoT;
            if (version.Game == Game.MajorasMask)
                return (Sup & Supported.MM) == Supported.MM;
            return false;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    class SpectrumCommandSignature : Attribute
    {
        public Tokens[] Sig = new Tokens[0];
        public int SigId = 0;
        public string Help = null;
    }
}
