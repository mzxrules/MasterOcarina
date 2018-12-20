using mzxrules.Helper;
using System;

namespace mzxrules.OcaLib.SceneRoom
{
    public class SceneWord
    {
        public const int LENGTH = 8; 
        byte[] command;
        public byte Code
        {
            get { return command[0]; }
            set { command[0] = value; }
        }
        public byte Data1
        { 
            get { return command[1]; }
            set { command[1] = value; }
        }
        public UInt32 Data2
        {
            get { return Endian.ConvertUInt32(command, 4); }
            set 
            {
                var arr = BitConverter.GetBytes(Endian.ConvertUInt32(value));
                command[4] = arr[0];
                command[5] = arr[1];
                command[6] = arr[2];
                command[7] = arr[3];
            }
        }

        //This indexer is very simple, and just returns or sets 
        //the corresponding element from the internal array.
        public byte this[int i]
        {
            get { return command[i]; }
            set { command[i] = value; }
        }

        public SceneWord()
        {
            command = new byte[8];
        }

        private SceneWord(byte[] arr)
        {
            command = arr;
        }

        public static implicit operator byte[](SceneWord cmd)
        {
            return cmd.command;
        }
        public static implicit operator SceneWord(byte[] arr)
        {
            return new SceneWord(arr);
        }
        public override string ToString()
        {
            return $"{Code:X2} {Data1:X2} {Data2:X8}";
        }
    }
}