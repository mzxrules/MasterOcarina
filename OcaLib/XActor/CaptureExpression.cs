using mzxrules.Helper;
using mzxrules.OcaLib;
using System;

namespace mzxrules.XActor
{
    public enum CaptureVar
    {
        INVALID, a, x, y, z, rx, ry, rz, v, d
    }
    

    public class CaptureExpression
    {
        //public Func<short[], ushort> GetValue { get; private set; }
        private string Value;
        public CaptureVar VarType;
        public int Mask;

        public CaptureExpression(string capture)
        {
            Value = capture;
            VarType = GetCaptureVarType(Value);
            Mask = GetCaptureMask(capture);

        }

        private static int GetCaptureMask(string capture)
        {
            return Convert.ToInt32(capture.Substring(capture.IndexOf("0x")), 16);
        }
        private static CaptureVar GetCaptureVarType(string capture)
        {
            string v = capture.Substring(0, capture.IndexOf('&')).Trim().ToLower();
            if (Enum.TryParse(v, out CaptureVar capvar))
            {
                return capvar;
            }
            return CaptureVar.INVALID;
        }

        public delegate Func<short[], ushort> GetValueDelegate(CaptureExpression capture);

        public static Func<short[], ushort> GetOcaActorValue(CaptureExpression capture)
        {
            return GetActorValue(Game.OcarinaOfTime, capture);
        }

        public static Func<short[], ushort> GetMMActorValue(CaptureExpression capture)
        {
            return GetActorValue(Game.MajorasMask, capture);
        }

        public static Func<short[], ushort> GetOcaTransActorValue(CaptureExpression capture)
        {
            return GetTransitionActorValue(Game.OcarinaOfTime, capture);
        }

        public static Func<short[], ushort> GetMMTransActorValue(CaptureExpression capture)
        {
            return GetTransitionActorValue(Game.MajorasMask, capture);
        }

        private static Func<short[], ushort> GetActorValue(Game game, CaptureExpression capture)
        {
            int shift = (game == Game.OcarinaOfTime) ? 0 : 7;
            switch (capture.VarType)
            {
                case CaptureVar.v:
                    return (x) => { return Shift.AsUInt16((ushort)x[7], (uint)capture.Mask); };
                case CaptureVar.rx:
                    return (x) => { return Shift.AsUInt16((ushort)x[4], (uint)capture.Mask << shift); };
                case CaptureVar.ry:
                    return (x) => { return Shift.AsUInt16((ushort)x[5], (uint)capture.Mask << shift); };
                case CaptureVar.rz:
                    return (x) => { return Shift.AsUInt16((ushort)x[6], (uint)capture.Mask << shift); };

                default:
                    throw new NotImplementedException();
            }
        }

        public static Func<short[], ushort> GetTransitionActorValue(Game game, CaptureExpression capture)
        {
            int shift = (game == Game.OcarinaOfTime) ? 0 : 7;
            switch (capture.VarType)
            {
                case CaptureVar.v:
                    return (x) => { return Shift.AsUInt16((ushort)x[7], (uint)capture.Mask); };
                case CaptureVar.ry:
                    return (x) => { return Shift.AsUInt16((ushort)x[6], (uint)capture.Mask << shift); };
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
