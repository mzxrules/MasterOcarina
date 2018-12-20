using System;
using System.Collections.Generic;
using System.IO;

namespace mzxrules.OcaLib.Cutscenes
{
    public abstract class CutsceneCommand 
    {
        public Int32 Command { get; protected set; }
        public int Length { get { return GetLength(); } }

        protected CutsceneCommand(int command, BinaryReader br)
        {
            Command = command;
        }
        protected CutsceneCommand() { }

        public virtual string ReadCommand()
        {
            throw new InvalidOperationException();
        }

        protected virtual int GetLength()
        {
            throw new InvalidOperationException();
        }

        public virtual void Save(BinaryWriter bw)
        {
            throw new InvalidOperationException();
        }
    }
}
