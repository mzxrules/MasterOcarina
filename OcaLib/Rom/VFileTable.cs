using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace mzxrules.OcaLib
{
    public class VFileTable : /*IDisposable,*/ IList<FileRecord>
    {
        protected DmaData dmadata;
        public RomVersion Version { get; protected set; }

        public int Count => dmadata.Table.Count;

        public bool IsReadOnly => false;

        public FileRecord this[int index] { get => dmadata.Table[index]; set => dmadata.Table[index] = value; }

        protected string RomLocation;

        byte[] CachedFile;
        FileAddress CachedFileAddress;
        public TableInfo Tables;
        
        protected VFileTable() { }

        public virtual RomFile GetSceneFile(int i)
        {
            throw new InvalidOperationException();
        }

        public FileAddress GetDmaDataStart()
        {
            return dmadata.Address.VRom;
        }

        #region GetFile

        /// <summary>
        /// Returns a stream pointed to the decompressed file at the given address
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public RomFile GetFile(FileAddress address)
        {
            if (dmadata.TryGetFileRecord(address.Start, out FileRecord record)
                && address.Size == record.VRom.Size)
                    return GetFile(record);
            
            return GetFile(new FileRecord(address, new FileAddress(address.Start, 0)));
        }

        /// <summary>
        /// Returns a stream pointed to the decompressed file at the given address
        /// </summary>
        /// <param name="vromStart"></param>
        /// <param name="recordCopy"></param>
        /// <returns></returns>
        public RomFile GetFile(int vromStart)
        {
            if (!dmadata.TryGetFileRecord(vromStart, out FileRecord record))
                throw new Exception();

            return GetFile(new FileRecord(record));
        }

        /// <summary>
        /// Returns a reference to a decompressed file
        /// </summary>
        /// <param name="record">The DMA address used to reference the file</param>
        /// <returns></returns>
        protected RomFile GetFile(FileRecord record)
        {
            MemoryStream ms;
            byte[] decompressedData;

            if (record.VRom == CachedFileAddress)
            {
                ms = new MemoryStream(CachedFile);
                return new RomFile(record, ms, Version);
            }

            using (FileStream fs = new FileStream(RomLocation, FileMode.Open, FileAccess.Read))
            {
                byte[] data = new byte[record.Data.Size];
                fs.Position = record.Data.Start;
                fs.Read(data, 0, record.Data.Size);

                if (record.IsCompressed)
                {
                    ms = new MemoryStream(data);
                    if (Version == ORom.Build.IQUEC || Version == ORom.Build.IQUET)
                    {
                        decompressedData = Deflate(ms, record.Data.Size);
                    }
                    else
                    {
                        decompressedData = Yaz.Decode(ms, record.Data.Size);
                    }
                }
                else
                {
                    decompressedData = data;
                }
            }
            CachedFile = decompressedData;
            ms = new MemoryStream(decompressedData);
            CachedFileAddress = record.VRom;
            return new RomFile(record, ms, Version);
        }

        private static byte[] Deflate(MemoryStream ms, int size)
        {
            using (DeflateStream deflate = new DeflateStream(ms, CompressionMode.Decompress))
            {
                using (MemoryStream decomp = new MemoryStream(size))
                {
                    deflate.CopyTo(decomp);
                    return decomp.ToArray();
                }
            }
        }


        /// <summary>
        /// Returns a stream pointed to the decompressed file at the given address
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public RomFile GetFile(RomFileToken file) => GetFile(Addresser.GetRom(file, Version, AddressToken.__Start));
        
        #endregion

        /// <summary>
        /// Returns a file without attempting to decompress it.  
        /// </summary>
        /// <param name="address">Address containing the location of the file</param>
        /// <returns>Returns a stream containing the file</returns>
        public Stream GetPhysicalFile(FileAddress address) => GetPhysicalFile(address.Start);
        
        /// <summary>
        /// Returns a file without attempting to decompress it.
        /// </summary>
        /// <param name="vromStart">Address containing the location of the file</param>
        /// <returns></returns>
        public Stream GetPhysicalFile(int vromStart)
        {
            byte[] data;
            
            if (!dmadata.TryGetFileRecord(vromStart, out FileRecord tableRecord))
                throw new Exception();

            using (FileStream fs = new FileStream(RomLocation, FileMode.Open, FileAccess.Read))
            {
                data = new byte[tableRecord.Data.Size];
                fs.Position = tableRecord.Data.Start;
                fs.Read(data, 0, tableRecord.Data.Size);

            }
            return new MemoryStream(data);
        }


        /// <summary>
        /// Returns the full FileRecord for a file that contains the given address
        /// </summary>
        /// <param name="vromStart"></param>
        /// <returns>The FileRecord of the containing file, or null if no file contains the given address</returns>
        public FileRecord GetFileStart(int vromStart)
        {
            return dmadata.Table.FirstOrDefault(x => x.VRom.Start <= vromStart
                && vromStart < x.VRom.End);
        }

        /// <summary>
        /// Tries to get the full FileRecord for a file with the given virtual FileAddress
        /// </summary>
        /// <param name="vrom">The virtual FileAddress of the record to return. Does not verify end address</param>
        /// <param name="record">Returns the full FileRecord for that file</param>
        /// <returns>True if operation is successful</returns>
        public bool TryGetFileRecord(FileAddress vrom, out FileRecord record)
        {
            return dmadata.TryGetFileRecord(vrom.Start, out record);
        }

        /// <summary>
        /// Tries to get the full FileRecord for a file with the given starting virtual address
        /// </summary>
        /// <param name="vrom">The virtual address of the record to return.</param>
        /// <param name="record">Returns the full FileRecord for that file</param>
        /// <returns>True if operation is successful</returns>
        public bool TryGetFileRecord(int vrom, out FileRecord record)
        {
            return dmadata.TryGetFileRecord(vrom, out record);
        }

        /// <summary>
        /// Tries to return the virtual FileAddress of a file with a given start address
        /// </summary>
        /// <param name="vromStart">The file's VROM address. Must match FileAddress's Vrom.Start</param>
        /// <param name="address">The returned FileAddress</param>
        /// <returns>True if the operation is successful</returns>
        public bool TryGetVirtualAddress(int vromStart, out FileAddress address)
        {
            bool getValue;

            getValue = dmadata.TryGetFileRecord(vromStart, out FileRecord record);
            if (getValue)
                address = record.VRom;
            else
                address = new FileAddress();
            return getValue;
        }

        /// <summary>
        /// Gets the virtual FileAddress of a file with a given start address
        /// </summary>
        /// <param name="vromStart">The file's VROM address. Must match FileAddress's Vrom.Start</param>
        /// <returns>The returned FileAddress</returns>
        public FileAddress GetVRomAddress(int vromStart)
        {
            if (!dmadata.TryGetFileRecord(vromStart, out FileRecord record))
                throw new FileNotFoundException();

            return record.VRom;
        }

        protected int ReadInt32(int addr)
        {
            FileRecord record = GetFileStart(addr);
            using (BinaryReader reader = new BinaryReader(GetFile(record)))
            {
                reader.BaseStream.Position = record.GetRelativeAddress(addr);
                return reader.ReadBigInt32();
            }
        }


        #region FetchAddresses

        public FileAddress GetSceneVirtualAddress(int scene)
        {
            return GetFileByTable(Tables.Scenes, scene);
        }

        public FileAddress GetActorVirtualAddress(int actor)
        {
            return GetFileByTable(Tables.Actors, actor);
        }

        public FileAddress GetObjectVirtualAddress(int obj)
        {
            //oot obj 0x00E3, 0x00FB malformed references
            return GetFileByTable(Tables.Objects, obj);
        }

        public FileAddress GetParticleEffectAddress(int index)
        {
            return GetFileByTable(Tables.Particles, index);
        }

        public FileAddress GetGameContextAddress(int i)
        {
            return GetFileByTable(Tables.GameOvls, i);
        }

        public FileAddress GetPlayPauseAddress(int i)
        {
            return GetFileByTable(Tables.PlayerPause , i);
        }

        public FileAddress GetFileByTable(TableInfo.Type type, int index)
        {
            var table = Tables.GetTable(type);
            return GetFileByTable(table, index);
        }

        protected FileAddress GetFileByTable(TableInfo.Table refTable, int index)
        {
            RomFileToken token = GetCodeFileToken();
            
            int size = refTable.Length;
            int offset = refTable.StartOff;
            int recordAddr, startAddr, endAddr;

            recordAddr = Addresser.GetRom(token, Version, refTable.Id);
            startAddr = recordAddr + (index * size) + offset;
            endAddr = startAddr + 4;
            startAddr = ReadInt32(startAddr);
            endAddr = ReadInt32(endAddr);

            FileAddress result;
            try
            {
                result = GetVRomAddress(startAddr);
            }
            catch
            {
                result = new FileAddress(startAddr, endAddr);
            }

            return result;
        }
        
        #endregion
        
        #region GetOverlayRecord
        public OverlayRecord GetOverlayRecord(int index, TableInfo.Type type)
        {
            RomFileToken token = GetCodeFileToken();
            RomFile code = GetFile(token);

            var table = Tables.GetTable(type);
            if (!Addresser.TryGetRom(token, Version, table.Id, out int addr))
            {
                return null;
            }
            code.Stream.Position = code.Record.GetRelativeAddress(addr + (index * table.Length));

            switch (type)
            {
                case TableInfo.Type.Actors: return new ActorOverlayRecord(index, new BinaryReader(code));
                case TableInfo.Type.GameOvls: return new GameStateRecord(index, new BinaryReader(code));
                case TableInfo.Type.Particles: return new ParticleOverlayRecord(index, new BinaryReader(code));
                case TableInfo.Type.PlayerPause: return new PlayPauseOverlayRecord(index, new BinaryReader(code));
                case TableInfo.Type.Transitions: return new TransitionOverlayRecord(index, new BinaryReader(code));
                case TableInfo.Type.MapMarkData: return new MapMarkDataOverlayRecord(new BinaryReader(code));
                default: return null;
            }
        }

        public ActorOverlayRecord GetActorOverlayRecord(int actor)
        {
            return (ActorOverlayRecord)GetOverlayRecord(actor, TableInfo.Type.Actors);
        }

        public GameStateRecord GetGameContextRecord(int index)
        {
            return (GameStateRecord)GetOverlayRecord(index, TableInfo.Type.GameOvls);
        }

        public ParticleOverlayRecord GetParticleOverlayRecord(int index)
        {
            return (ParticleOverlayRecord)GetOverlayRecord(index, TableInfo.Type.Particles);
        }

        public PlayPauseOverlayRecord GetPlayPauseOverlayRecord(int index)
        {
            return (PlayPauseOverlayRecord)GetOverlayRecord(index, TableInfo.Type.PlayerPause);
        }

        #endregion

        private RomFileToken GetCodeFileToken()
        {
            RomFileToken token = ORom.FileList.invalid;
            if (Version.Game == Game.OcarinaOfTime)
                token = ORom.FileList.code;
            if (Version.Game == Game.MajorasMask)
                token = MRom.FileList.code;
            return token;
        }


        public IEnumerator<FileRecord> GetEnumerator()
        {
            return dmadata.Table.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return dmadata.Table.GetEnumerator();
        }

        public int IndexOf(FileRecord item)
        {
            return dmadata.Table.IndexOf(item);
        }

        public void Insert(int index, FileRecord item)
        {
            dmadata.Table.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            dmadata.Table.RemoveAt(index);
        }

        public void Add(FileRecord item)
        {
            dmadata.Table.Add(item);
        }

        public void Clear()
        {
            dmadata.Table.Clear();
        }

        public bool Contains(FileRecord item)
        {
            return dmadata.Table.Contains(item);
        }

        public void CopyTo(FileRecord[] array, int arrayIndex)
        {
            dmadata.Table.CopyTo(array, arrayIndex);
        }

        public bool Remove(FileRecord item)
        {
            return dmadata.Table.Remove(item);
        }

    }
}
