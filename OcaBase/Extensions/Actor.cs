using System;

namespace OcaBase
{
    partial class Actor
    {
        public string GetDescription()
        {
            int startAddr = File.Address_DBG.StartAddr;
            int endAddr = File.Address_DBG.EndAddr;

            string filename;
            string description;
            
            switch(ID)
            {
                case 0x15:
                    filename = File.Filename + " (En_Item00)";
                    description = "Collectable Items";break;
                case 0x39:
                    filename = File.Filename + " (En_A_Obj)";
                    description = "Gameplay_keep items";break;
                default:
                    filename = File.Filename;
                    description = File.Description;
                    break;
            }

            return String.Format("F{0} Actor {1:X4}: {2}, {3}F{4} object {5:X4} ({6})",
                File.FileId,
                ID,
                filename,
                (!String.IsNullOrEmpty(description)) ? description + ", " : "",
                ObjectFile.FileId,
                Object,
                ObjectFile.Filename);
        }
    }
}
