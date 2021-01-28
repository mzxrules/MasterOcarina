using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spectrum
{
    public enum Tokens
    {
        INVALID,
        EXPRESSION_S,
        STRING,
        PATH,
        LITERAL,
        HEX_U8,
        HEX_S8,
        HEX_U16,
        HEX_S16,
        HEX_S32,
        HEX_FLOAT,
        U8,
        S8,
        S16,
        S32,
        FLOAT,
        COORDS_FLOAT,
        U16,
        U32,
    }

    public class Arguments
    {
        public string Input { get; private set; }
        public bool Valid { get; private set; }

        private List<object> argObjects = new List<object>();
        public int Length { get { return argObjects.Count; } }

        public object this[int i]
        {
            get
            {
                return argObjects[i];
            }
        }


        public Arguments(string arg, Tokens token) 
            : this(arg, tokens: new Tokens[] { token })
        {
        }

        public Arguments(string args, Tokens[] tokens)
        {
            Input = args;
            Valid = ParseSignature(args, tokens);
        }


        private bool ParseSignature(string argsStr, Tokens[] Tokens)
        {
            argObjects.Clear();
            int argsTotal = Tokens.Length;
            int cur = 0;

            foreach (var token in Tokens)
            {
                if (cur == argsStr.Length)
                    break;
                switch (token)
                {
                    case Spectrum.Tokens.COORDS_FLOAT:
                        if (!TryGetCOORDS_FLOAT(argsStr, ref cur))
                            return false;
                        break;
                    case Spectrum.Tokens.EXPRESSION_S:
                        if (!TryGetEXPRESSION(argsStr, ref cur))
                            return false;
                        break;
                    case Spectrum.Tokens.STRING:
                        if (!TryGetUnescapedString(argsStr, ref cur))
                            return false;
                        break;
                    case Spectrum.Tokens.PATH:
                        if (!TryGetPathString(argsStr, ref cur))
                            return false;
                        break;
                    case Spectrum.Tokens.LITERAL:
                        if (!TryGetLITERAL(argsStr, ref cur))
                            return false;
                        break;
                    case Spectrum.Tokens.HEX_U8:
                        if (!TryGetHEX_TYPE(argsStr, ref cur, typeof(byte)))
                            return false;
                        break;
                    case Spectrum.Tokens.HEX_S8:
                        if (!TryGetHEX_TYPE(argsStr, ref cur, typeof(sbyte)))
                            return false;
                        break;
                    case Spectrum.Tokens.HEX_S16:
                        if (!TryGetHEX_TYPE(argsStr, ref cur, typeof(short)))
                            return false;
                        break;
                    case Spectrum.Tokens.HEX_U16:
                        if (!TryGetHEX_TYPE(argsStr, ref cur, typeof(ushort)))
                            return false;
                        break;
                    case Spectrum.Tokens.HEX_S32:
                        if (!TryGetHEX_TYPE(argsStr, ref cur, typeof(int)))
                            return false;
                        break;
                    case Spectrum.Tokens.HEX_FLOAT:
                        if (!TryGetHEX_TYPE(argsStr, ref cur, typeof(float)))
                            return false;
                        break;
                    #region case Spectrum.Tokens.NUM_TYPE
                    case Spectrum.Tokens.S8:
                        if (!TryGetNUM_TYPE(argsStr, ref cur, typeof(sbyte)))
                            return false;
                        break;
                    case Spectrum.Tokens.S16:
                        if (!TryGetNUM_TYPE(argsStr, ref cur, typeof(short)))
                            return false;
                        break;
                    case Spectrum.Tokens.S32:
                        if (!TryGetNUM_TYPE(argsStr, ref cur, typeof(int)))
                            return false;
                        break;
                    case Spectrum.Tokens.U8:
                        if (!TryGetNUM_TYPE(argsStr, ref cur, typeof(byte)))
                            return false;
                        break;
                    case Spectrum.Tokens.U16:
                        if (!TryGetNUM_TYPE(argsStr, ref cur, typeof(ushort)))
                            return false;
                        break;
                    case Spectrum.Tokens.U32:
                        if (!TryGetNUM_TYPE(argsStr, ref cur, typeof(uint)))
                            return false;
                        break;
                    case Spectrum.Tokens.FLOAT:
                        if (!TryGetNUM_TYPE(argsStr, ref cur, typeof(float)))
                            return false;
                        break;
                    #endregion
                    default: throw new NotImplementedException();
                }
            }
            if (argsTotal != argObjects.Count)
                return false;
            if (cur != argsStr.Length)
                return false;
            return true;
        }

        private bool TryGetPathString(string argsStr, ref int cur)
        {
            int start, end;
            if (cur == argsStr.Length)
                return false;
            if (argsStr[cur] == '"')
            {
                cur++;
                start = cur;
                ParseSeekNextChar(argsStr, '"', ref cur);
                end = cur;
                if (cur == argsStr.Length)
                    return false;
                cur++;
            }
            else
            {
                start = cur;
                ParseSeekNextChar(argsStr, ' ', ref cur);
                end = cur;
            }
            if (start == end)
                return false;
            string path = argsStr.Substring(start, end - start);
            argObjects.Add(path);
            return true;
        }

        private bool TryGetLITERAL(string argsStr, ref int cur)
        {
            string literal = GetTokenWithNextSpace(argsStr, ref cur);
            if (string.IsNullOrWhiteSpace(literal))
                return false;
            argObjects.Add(literal);
            return true;
        }

        private bool TryGetNUM_TYPE(string argsStr, ref int cur, Type type)
        {
            string test = GetTokenWithNextSpace(argsStr, ref cur);

            if (type == typeof(sbyte)
                && sbyte.TryParse(test, out sbyte s8))
            {
                argObjects.Add(s8);
                return true;
            }
            else if (type == typeof(byte)
                && byte.TryParse(test, out byte u8))
            {
                argObjects.Add(u8);
                return true;
            } else
            if (type == typeof(short)
                && short.TryParse(test, out short s16))
            {
                argObjects.Add(s16);
                return true;
            }
            else if(type == typeof(ushort)
                && ushort.TryParse(test, out ushort u16))
            {
                argObjects.Add(u16);
                return true;
            }
            else if (type == typeof(int)
                && int.TryParse(test, out int s32))
            {
                argObjects.Add(s32);
                return true;
            }
            else if (type == typeof(uint)
                && uint.TryParse(test, out uint u32))
            {
                argObjects.Add(u32);
                return true;
            }
            else if (type == typeof(float)
                && float.TryParse(test, out float f))
            {
                argObjects.Add(f);
                return true;
            }


            return false;
        }

        private bool TryGetHEX_TYPE(string argsStr, ref int cur, Type type)
        {
            string test = GetTokenWithNextSpace(argsStr, ref cur);

            if (type == typeof(short))
            {
                if (short.TryParse(test, NumberStyles.HexNumber, new CultureInfo("en-US"), out short s16))
                {
                    argObjects.Add(s16);
                    return true;
                }
            }
            else if (type == typeof(ushort))
            {
                if (ushort.TryParse(test, NumberStyles.HexNumber, new CultureInfo("en-US"), out ushort u16))
                {
                    argObjects.Add(u16);
                    return true;
                }
            }
            else if (type == typeof(int))
            {
                if (int.TryParse(test, NumberStyles.HexNumber, new CultureInfo("en-US"), out int s32))
                {
                    argObjects.Add(s32);
                    return true;
                }
            }
            else if (type == typeof(float))
            {
                if (int.TryParse(test, NumberStyles.HexNumber, new CultureInfo("en-US"), out int val))
                {
                    var bytes = BitConverter.GetBytes(val);
                    float result = BitConverter.ToSingle(bytes, 0);
                    argObjects.Add(result);
                    return true;
                }
            }
            else if (type == typeof(byte))
            {
                if (byte.TryParse(test, NumberStyles.HexNumber, new CultureInfo("en-US"), out byte u8))
                {
                    argObjects.Add(u8);
                    return true;
                }
            }
            else if (type == typeof(sbyte))
            {
                if (sbyte.TryParse(test, NumberStyles.HexNumber, new CultureInfo("en-US"), out sbyte s8))
                {
                    argObjects.Add(s8);
                    return true;
                }
            }
            return false;
        }

        private string GetTokenWithNextSpace(string argsStr, ref int cur)
        {
            int start = cur;
            cur = argsStr.IndexOf(' ', start);
            if (cur == -1)
                cur = argsStr.Length;

            string test = argsStr.Substring(start, cur - start);
            ParseSeekNext(argsStr, ref cur);
            return test;
        }

        private bool TryGetEXPRESSION(string argsStr, ref int cur)
        {
            return TryGetUnescapedString(argsStr, ref cur);
        }

        private bool TryGetUnescapedString(string argsStr, ref int cur)
        {
            int start = cur;
            int end;
            cur = argsStr.IndexOfAny(new char[] { ',' }, start);
            if (cur == -1)
            {
                cur = argsStr.Length;
                end = cur;
            }
            else
            {
                end = cur;
                cur++;
            }

            string a = argsStr.Substring(start, end - start);
            ParseSeekNext(argsStr, ref cur);
            if (string.IsNullOrWhiteSpace(a))
                return false;

            argObjects.Add(a);
            return true;
        }

        private bool TryGetCOORDS_FLOAT(string argsStr, ref int cur)
        {
            string[] args = new string[]
            {
                GetNextNumber(argsStr, ref cur),
                GetNextNumber(argsStr, ref cur),
                GetNextNumber(argsStr, ref cur)
            };
            if (cur < argsStr.Length
                && argsStr[cur] == ')')
                cur++;
            ParseSeekNext(argsStr, ref cur);
            if ( TryParseVector3Float(args, out Vector3<float> a))
            {
                argObjects.Add(a);
                return true;
            }
            return false;

            string GetNextNumber(string arg, ref int c)
            {
                int start = c;

                while (c != arg.Length)
                {
                    int index = arg.IndexOfAny(new char[] { '(', ')', ' ', ',' }, start);

                    if (index == start)
                    {
                        start++;
                    }
                    else if (index == -1)
                    {
                        c = arg.Length;
                    }
                    else
                    {
                        c = index;
                        break;
                    }
                }
                return arg.Substring(start, c - start);
            }
        }

        private bool TryParseVector3Float(string[] args, out Vector3<float> xyz)
        {
            if (float.TryParse(args[0], out float x)
                && float.TryParse(args[1], out float y)
                && float.TryParse(args[2], out float z))
            {
                xyz = new Vector3<float>(x, y, z);
                return true;
            }
            xyz = null;
            return false;
        }

        private void ParseSeekNext(string argsStr, ref int cur)
        {
            while (cur != argsStr.Length)
            {
                if (argsStr[cur] != ' ')
                    return;
                cur++;
            }
        }

        private void ParseSeekNextChar(string argsStr, char chr, ref int cur)
        {
            while (cur != argsStr.Length)
            {
                if (argsStr[cur] == chr)
                    return;
                cur++;
            }    
        }

    }
}
