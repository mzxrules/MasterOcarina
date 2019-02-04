using mzxrules.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace mzxrules.OcaLib
{
    public class VFileTable : /*IDisposable,*/ IEnumerable<FileRecord>
    {
        protected DmaData dmadata;
        public RomVersion Version { get; protected set; }

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
            return dmadata.Address.VirtualAddress;
        }

        private void ResetFileCache()
        {
            CachedFile = null;
            CachedFileAddress = new FileAddress();
        }

        #region GetFile

        /// <summary>
        /// Returns a stream pointed to the decompressed file at the given address
        /// </summary>
        /// <param name="vAddr"></param>
        /// <returns></returns>
        public RomFile GetFile(FileAddress vAddr)
        {
            if (dmadata.TryGetFileRecord(vAddr.Start, out FileRecord record)
                && vAddr.Size == record.VirtualAddress.Size)
                    return GetFile(record);
            
            return GetFile(new FileRecord(vAddr, new FileAddress(vAddr.Start, 0), -1));
        }

        /// <summary>
        /// Returns a stream pointed to the decompressed file at the given address
        /// </summary>
        /// <param name="virtualAddress"></param>
        /// <param name="recordCopy"></param>
        /// <returns></returns>
        public RomFile GetFile(long virtualAddress)
        {
            if (!dmadata.TryGetFileRecord(virtualAddress, out FileRecord record))
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
            byte[] data;
            byte[] decompressedData;

            if (record.VirtualAddress == CachedFileAddress)
            {
                ms = new MemoryStream(CachedFile);
                return new RomFile(record, ms, Version);
            }

            using (FileStream fs = new FileStream(RomLocation, FileMode.Open, FileAccess.Read))
            {
                data = new byte[record.DataAddress.Size];
                fs.Position = record.DataAddress.Start;
                fs.Read(data, 0, record.DataAddress.Size);

                if (record.IsCompressed)
                {
                    ms = new MemoryStream(data);
                    decompressedData = Yaz.Decode(ms, record.DataAddress.Size);
                }
                else
                {
                    decompressedData = data;
                }
            }
            CachedFile = decompressedData;
            ms = new MemoryStream(decompressedData);
            CachedFileAddress = record.VirtualAddress;
            return new RomFile(record, ms, Version);
        }


        /// <summary>
        /// Returns a stream pointed to the decompressed file at the given address
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public RomFile GetFile(RomFileToken file) => GetFile(Addresser.GetRom(file, Version, "__Start"));
        
        #endregion

        /// <summary>
        /// Returns a file without attempting to decompress it.  
        /// </summary>
        /// <param name="virtualAddress">Address containing the location of the file</param>
        /// <returns>Returns a stream containing the file</returns>
        public Stream GetPhysicalFile(FileAddress virtualAddress) => GetPhysicalFile(virtualAddress.Start);
        
        /// <summary>
        /// Returns a file without attempting to decompress it.
        /// </summary>
        /// <param name="virtualAddress">Address containing the location of the file</param>
        /// <returns></returns>
        public Stream GetPhysicalFile(long virtualAddress)
        {
            MemoryStream ms;
            byte[] data;
            
            if (!dmadata.TryGetFileRecord(virtualAddress, out FileRecord tableRecord))
                throw new Exception();

            using (FileStream fs = new FileStream(RomLocation, FileMode.Open, FileAccess.Read))
            {
                data = new byte[tableRecord.DataAddress.Size];
                fs.Position = tableRecord.DataAddress.Start;
                fs.Read(data, 0, tableRecord.DataAddress.Size);

            }
            return ms = new MemoryStream(data);
        }


        /// <summary>
        /// Returns the full FileRecord for a file that contains the given address
        /// </summary>
        /// <param name="virtualAddress"></param>
        /// <returns>The FileRecord of the containing file, or null if no file contains the given address</returns>
        public FileRecord GetFileStart(long virtualAddress)
        {
            return dmadata.Table.FirstOrDefault(x => x.VirtualAddress.Start <= virtualAddress
                && virtualAddress < x.VirtualAddress.End);
        }

        /// <summary>
        /// Tries to get the full FileRecord for a file with the given virtual FileAddress
        /// </summary>
        /// <param name="virtualAddress">The virtual FileAddress of the record to return. Does not verify end address</param>
        /// <param name="record">Returns the full FileRecord for that file</param>
        /// <returns>True if operation is successful</returns>
        public bool TryGetFileRecord(FileAddress virtualAddress, out FileRecord record)
        {
            return dmadata.TryGetFileRecord(virtualAddress.Start, out record);
        }

        /// <summary>
        /// Tries to get the full FileRecord for a file with the given starting virtual address
        /// </summary>
        /// <param name="virtualAddress">The virtual address of the record to return.</param>
        /// <param name="record">Returns the full FileRecord for that file</param>
        /// <returns>True if operation is successful</returns>
        public bool TryGetFileRecord(long virtualAddress, out FileRecord record)
        {
            return dmadata.TryGetFileRecord(virtualAddress, out record);
        }

        /// <summary>
        /// Tries to return the virtual FileAddress of a file with a given start address
        /// </summary>
        /// <param name="address">The file's VROM address. Must match FileAddress's Vrom.Start</param>
        /// <param name="resultAddress">The returned FileAddress</param>
        /// <returns>True if the operation is successful</returns>
        public bool TryGetVirtualAddress(long address, out FileAddress resultAddress)
        {
            bool getValue;

            getValue = dmadata.TryGetFileRecord(address, out FileRecord record);
            if (getValue)
                resultAddress = record.VirtualAddress;
            else
                resultAddress = new FileAddress();
            return getValue;
        }

        /// <summary>
        /// Gets the virtual FileAddress of a file with a given start address
        /// </summary>
        /// <param name="address">The file's VROM address. Must match FileAddress's Vrom.Start</param>
        /// <returns>The returned FileAddress</returns>
        public FileAddress GetVRomAddress(long address)
        {
            if (!dmadata.TryGetFileRecord(address, out FileRecord record))
                throw new FileNotFoundException();

            return record.VirtualAddress;
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


        public void Dispose()
        {
        }
        

        public IEnumerator<FileRecord> GetEnumerator()
        {
            return dmadata.Table.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return dmadata.Table.GetEnumerator();
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
            RomFileToken token = ORom.FileList.invalid;
            if (Version.Game == Game.OcarinaOfTime)
                token = ORom.FileList.code;
            if (Version.Game == Game.MajorasMask)
                token = MRom.FileList.code;
            
            int size = refTable.Length;
            int offset = refTable.StartOff;
            int recordAddr, startAddr, endAddr;

            recordAddr = Addresser.GetRom(token, Version, refTable.Id);
            startAddr = recordAddr + (index * size) + offset;
            endAddr = startAddr + 4;
            startAddr = ReadInt32(startAddr);
            endAddr = ReadInt32(endAddr);
            FileAddress result = new FileAddress();
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
                default: return null;
            }
        }

        public ActorOverlayRecord GetActorOverlayRecord(int actor)
        {
            RomFileToken token = GetCodeFileToken();
            RomFile code = GetFile(token);
            if (!Addresser.TryGetRom(token, Version, Tables.Actors.Id, out int addr))
            {
                return null;
            }
            code.Stream.Position = code.Record.GetRelativeAddress(addr + (actor * 0x20));
            return new ActorOverlayRecord(actor, new BinaryReader(code));
        }

        public GameStateRecord GetGameContextRecord(int index)
        {
            RomFileToken token = GetCodeFileToken();
            RomFile code = GetFile(token);
            if (!Addresser.TryGetRom(token, Version, Tables.GameOvls.Id, out int addr))
            {
                return null;
            }
            code.Stream.Position = code.Record.GetRelativeAddress(addr + (index * 0x30));
            return new GameStateRecord(index, new BinaryReader(code));
        }

        public ParticleOverlayRecord GetParticleOverlayRecord(int index)
        {
            RomFileToken token = GetCodeFileToken();
            RomFile code = GetFile(token);
            if (!Addresser.TryGetRom(token, Version, Tables.Particles.Id, out int addr))
            {
                return null;
            }
            code.Stream.Position = code.Record.GetRelativeAddress(addr + (index * 0x1C));
            return new ParticleOverlayRecord(index, new BinaryReader(code));
        }

        public PlayPauseOverlayRecord GetPlayPauseOverlayRecord(int index)
        {
            RomFileToken token = GetCodeFileToken();
            RomFile code = GetFile(token);
            if (!Addresser.TryGetRom(token, Version, Tables.PlayerPause.Id, out int addr))
            {
                return null;
            }
            code.Stream.Position = code.Record.GetRelativeAddress(addr + (index * 0x1C));
            return new PlayPauseOverlayRecord(index, new BinaryReader(code));
        }

        private RomFileToken GetCodeFileToken()
        {
            RomFileToken token = ORom.FileList.invalid;
            if (Version.Game == Game.OcarinaOfTime)
                token = ORom.FileList.code;
            if (Version.Game == Game.MajorasMask)
                token = MRom.FileList.code;
            return token;
        }

        #endregion

    }
}
