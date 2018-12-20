using System;
using System.IO;
using System.Collections.Generic;
using mzxrules.Helper;
using System.Text;
using System.Linq;

namespace mzxrules.OcaLib.SceneRoom
{
    public class CollisionMesh
    {
        public const int HEADER_SIZE = 0x2C;
        const int WATER_BOX_RECORD_SIZE = 0x10;
        /* 0x00 */ Vector3<short> BoundsMin = new Vector3<short>();
        /* 0x06 */ Vector3<short> BoundsMax = new Vector3<short>();
        /* 0x0C */ public short Vertices;
        /* 0x10 */ public SegmentAddress VertexArray;
        /* 0x14 */ public short Polys;
        /* 0x18 */ public SegmentAddress PolyArray;
        /* 0x1C */ public SegmentAddress PolyTypeArray;
        /* 0x20 */ public SegmentAddress CameraDataArray;
        /* 0x24 */ public short WaterBoxes;
        /* 0x28 */ public SegmentAddress WaterBoxesArray;

        public List<CollisionPolygon> PolyList = new List<CollisionPolygon>();
        public List<CollisionPolyType> PolyTypeList = new List<CollisionPolyType>();
        public List<Vector3<short>> VertexList = new List<Vector3<short>>();
        public List<CollisionCameraData> CameraDataList = new List<CollisionCameraData>();
        public List<CollisionWaterBox> WaterBoxList = new List<CollisionWaterBox>();

        
        SegmentAddress HeaderOffset;
        SegmentAddress CameraDataMin = 0;
        

        int PolyTypes = 0;
        int CameraDatas = 0;

        bool HasCameraData => CameraDataArray.Segment == 0 && CameraDataArray.Offset == 0;

        bool HasWaterBoxesData => WaterBoxes > 0;

        public CollisionMesh(SegmentAddress headerOffset)
        {
            HeaderOffset = headerOffset;
        }

        public void Initialize(BinaryReader br)
        {
            long seekBack;
            byte[] data;
            
            //initialize header
            data = new byte[HEADER_SIZE];
            seekBack = br.Seek(HeaderOffset.Offset);
            br.Read(data, 0, HEADER_SIZE);
            InitializeHeader(data);

            //initialize polygon list
            data = new byte[CollisionPolygon.SIZE];
            br.Seek(PolyArray.Offset);
            for (int i = 0; i < Polys; i++)
            {
                br.Read(data, 0, CollisionPolygon.SIZE);
                PolyList.Add(new CollisionPolygon(data));
            }

            //initialize polygon types
            data = new byte[CollisionPolyType.SIZE];
            br.Seek(PolyTypeArray.Offset);
            PolyTypes = GetPolygonTypeListLength();

            for (int i = 0; i < PolyTypes; i++)
            {
                br.Read(data,0, CollisionPolyType.SIZE);
                int hi = Endian.ConvertInt32(data, 0);
                int low = Endian.ConvertInt32(data, 4);
                PolyTypeList.Add(new CollisionPolyType(hi, low));
            }

            //initialize vertex list
            br.Seek(VertexArray.Offset);
            
            for ( int i = 0; i < Vertices; i++)
            {
                VertexList.Add(new Vector3<short>(br.ReadBigInt16(), br.ReadBigInt16(), br.ReadBigInt16()));
            }


            //initialize camera data
            if (CameraDataArray != 0)
            {
                br.Seek(CameraDataArray.Offset);
                CameraDatas = GetCameraDataListLength();

                for (int i = 0; i < CameraDatas; i++)
                {
                    var camData = new CollisionCameraData(br);
                    CameraDataList.Add(camData);
                    if (CameraDataMin == 0
                        || (camData.PositionAddress != 0
                        && camData.PositionAddress.Offset < CameraDataMin.Offset))
                    {
                        CameraDataMin = camData.PositionAddress;
                    }
                }
            }

            //initialize water box data
            if (WaterBoxes > 0)
            {
                br.Seek(WaterBoxesArray.Offset);
                for (int i = 0; i < WaterBoxes; i++)
                {
                    data = br.ReadBytes(CollisionWaterBox.SIZE);
                    WaterBoxList.Add(new CollisionWaterBox(data));
                }
            }

            br.Seek(seekBack);
        }

        public string PrintAll()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(Print());

            sb.AppendLine("Vertices:");
            PrintList(VertexList.Select(x => PrintCoords(x)).ToList());

            sb.AppendLine("Polys:");
            PrintList(PolyList);

            sb.AppendLine("PolyTypes:");
            PrintList(PolyTypeList);


            sb.AppendLine("CameraData:");
            PrintList(CameraDataList);

            sb.AppendLine("WaterBox:");
            PrintList(WaterBoxList);


            return sb.ToString();


            void PrintList<T>(List<T> list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    sb.AppendLine($"{i:X4} {item.ToString()}");
                }
            }
            string PrintCoords(Vector3<short> v)
            {
                return $"({v.x}, {v.y}, {v.z})";
            }
        }

        public string Print()
        {
            string result;

            result = $"Vertex array (Count: {Vertices}): {VertexArray:X8}" + Environment.NewLine
                + $"Polygon array (Count: {Polys}): {PolyArray:X8}" + Environment.NewLine
                + $"Polygon type defs (Count: {PolyTypeList.Count}): {PolyTypeArray:X8}" + Environment.NewLine
                + $"Camera data (Count: {CameraDataList.Count}): {CameraDataArray:X8}" + Environment.NewLine
                + $"Water boxes (Count: {WaterBoxes}): {WaterBoxesArray:X8}" + Environment.NewLine
                + $"Size: 0x{GetFileSize():X4}";

            return result;
        }

        private void InitializeHeader(byte[] data)
        {
            Endian.Convert(out BoundsMin, data, 0x00);
            Endian.Convert(out BoundsMax, data, 0x06);
            
            Endian.Convert(out Vertices, data, 0x0C);
            Endian.Convert(out int temp, data, 0x10);
            VertexArray = new SegmentAddress(temp);

            Endian.Convert(out Polys, data, 0x14);
            Endian.Convert(out temp, data, 0x18);
            PolyArray = new SegmentAddress(temp);
            Endian.Convert(out temp, data, 0x1C);
            PolyTypeArray = new SegmentAddress(temp);

            Endian.Convert(out temp, data, 0x20);
            CameraDataArray = new SegmentAddress(temp);

            Endian.Convert(out WaterBoxes, data, 0x24);
            Endian.Convert(out temp, data, 0x28);
            WaterBoxesArray = new SegmentAddress(temp);
        }

        public int GetPolygonTypeListLength()
        {
            return (PolyArray.Offset - PolyTypeArray.Offset) / CollisionPolyType.SIZE;
        }

        private int GetCameraDataListLength()
        {
            return (PolyTypeArray.Offset - CameraDataArray.Offset) / CollisionCameraData.SIZE;
        }

        public int GetFileSize()
        {
            int end = HeaderOffset.Offset + HEADER_SIZE;
            if (CameraDataArray != 0)
                if (CameraDataMin == 0)
                    return end - CameraDataArray.Offset;
                else
                    return end - CameraDataMin.Offset;
            else
                return end - VertexArray.Offset;
        }
    }
}
