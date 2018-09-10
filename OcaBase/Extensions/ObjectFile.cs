using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OcaBase
{
    partial class ObjectFile
    {
        public string GetDescription()
        {
            return String.Format("F{0}: Object {1:X4}: {2}{3}",
                File.FileId,
                ID,
                Filename,
                (!String.IsNullOrEmpty(File.Description)) ? ", " + File.Description : "");
        }
    }
}
