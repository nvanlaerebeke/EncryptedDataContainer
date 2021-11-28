using System;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace EncFIleStorage.FileIndex
{
    /// <summary>
    /// Object that represents an index item
    /// </summary>
    internal class IndexEntry : IComparer<IndexEntry>, IComparable
    {
        /// <summary>
        /// For convenience sake, this is the number of bytes an index entry takes up in the index
        /// </summary>
        public const byte IndexEntryLength = 15;

        /// <summary>
        /// Denotes if this index entry was changed or not
        /// ToDo: use this to not update/dump the entire Index every time
        /// </summary>
        private bool _changed = false;
        
        /// <summary>
        /// Is this block in use or not?
        /// </summary>
        public bool Free { get; private set; }
        
        public ulong SequenceNumber { get; private set; }
        
        /// <summary>
        /// Start byte in the file for this block
        /// </summary>
        public uint Start { get; }
        
        /// <summary>
        /// Size (number of bytes) the current block represents
        /// </summary>
        public ushort Length { get; private set; }
        
        /// <summary>
        /// Byte in the file where the next block starts
        ///
        /// Note: that while loading the index this is set to the block Length
        ///       during index loading this is then changed to the next block
        ///       when that block info is loaded
        /// </summary>
        public long End { get; private set; }

        public IndexEntry(byte[] indexEntry)
        {
            //0 if not in use
            Free = indexEntry[0] == 0;
            //sequence number
            SequenceNumber = BinaryPrimitives.ReadUInt64LittleEndian(indexEntry.AsSpan()[1..9]);
            //DataBlock start position in the stream 
            Start = BinaryPrimitives.ReadUInt32LittleEndian(indexEntry.AsSpan()[9..13]);
            //Length of the DataBlock
            Length = BinaryPrimitives.ReadUInt16LittleEndian(indexEntry.AsSpan()[13..15]);
            
            //Default end point of this DataBlock
            //This can be different then the length if the data inside the block is not
            //the same size every time, example with encryption
            End = Start + Length;
        }
        
        /// <summary>
        /// Sets the boundary of the block
        ///
        /// This means that the next data block starts at this location
        /// So this block may grow to 'end'
        /// </summary>
        /// <param name="end">Next block starting position</param>
        public void SetBlockBoundary(long end)
        {
            End = end;
        }

        /// <summary>
        /// Gets the number of bytes available for writing
        /// </summary>
        /// <returns></returns>
        public ushort GetAvailableSize()
        {
            if (!Free) return 0;
            if(End != 0) return (ushort)(End - Start);
            return Length;
        }

        public void SetUsed(ulong sequenceNumber, int length)
        {
            if (Free || length != Length)
            {
                Free = false;
                Length = (ushort)length;
                SequenceNumber = sequenceNumber;
            }
        }

        public void Clear()
        {
            if (!Free)
            {
                Free = true;
                _changed = true;
                if (End != 0)
                {
                    Length = GetAvailableSize();
                }
            }
        }

        public byte[] GetBytes()
        {
            var entryBytes = new byte[IndexEntryLength];
            if (Free)
            {
                entryBytes[0] = 0;
            }
            else
            {
                entryBytes[0] = 1;
            }

            var sequenceBytes = new byte[8]; //sequence number in the file
            var startBytes = new byte[4];  //block start
            var lengthBytes = new byte[2]; //block length
            
            //Get the byte representation of the values and add them to the entryBytes array
            BinaryPrimitives.WriteUInt64LittleEndian(sequenceBytes, SequenceNumber);
            BinaryPrimitives.WriteUInt32LittleEndian(startBytes, Start);
            BinaryPrimitives.WriteUInt16LittleEndian(lengthBytes, Length);

            Array.Copy(sequenceBytes, 0, entryBytes, 1, 8);
            Array.Copy(startBytes, 0, entryBytes, 9, 4);
            Array.Copy(lengthBytes, 0, entryBytes, 13, 2);
            return entryBytes;
        }

        /// <summary>
        /// Writes a data block
        /// </summary>
        /// <param name="data"></param>
        /// <param name="stream"></param>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /*public void Write(byte[] data, Stream stream)
        {
            if (!Free) throw new UnauthorizedAccessException();
            var available = GetAvailableSize(); 
            if (available < data.Length) throw new ArgumentOutOfRangeException();

            var startBytes = new byte[4];
            var lengthBytes = new byte[2];
            BinaryPrimitives.WriteUInt32LittleEndian(startBytes, Start);
            BinaryPrimitives.WriteUInt32LittleEndian(lengthBytes, (uint)data.Length);

            //stream.Position = Start;
            var block = new byte[available + _indexLength];
            block[0] = 1;
            Array.Copy(startBytes, 0, block, 1, 5); //add start = 4bytes            
            Array.Copy(lengthBytes, 0, block, 5, 2); //add length = 2 bytes
            Array.Copy(data, 0, block, 7, data.Length); //add data 
            
            stream.Position = Start;
            stream.Write(block);
        }*/

        /// <summary>
        /// Reads the data for this block
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /*public byte[] Read(Stream stream)
        {
            var buff = new byte[Length];
            stream.Position = Start;
            stream.Read(buff, (int)Start + _indexLength, Length);
            return buff;
        }*/

        public static IndexEntry Create(ulong sequenceNumber, uint startByte, ushort size)
        {
            var entryBytes = new byte[IndexEntryLength];
            entryBytes[0] = 0; //block not in use
            var sequenceBytes = new byte[8]; //sequence number in the file
            var startBytes = new byte[4];  //block start
            var lengthBytes = new byte[2]; //block length
            
            //Get the byte representation of the values and add them to the entryBytes array
            BinaryPrimitives.WriteUInt64LittleEndian(sequenceBytes, sequenceNumber);
            BinaryPrimitives.WriteUInt32LittleEndian(startBytes, startByte);
            BinaryPrimitives.WriteUInt16LittleEndian(lengthBytes, size);

            Array.Copy(sequenceBytes, 0, entryBytes, 1, 8);
            Array.Copy(startBytes, 0, entryBytes, 9, 4);
            Array.Copy(lengthBytes, 0, entryBytes, 13, 2);
            return new IndexEntry(entryBytes);
        }

        public int Compare(IndexEntry? x, IndexEntry? y)
        {
            if (x == null) return -1;
            if (y == null) return 1;
            return x.SequenceNumber.CompareTo(y.SequenceNumber);
        }

        public int CompareTo(object? obj)
        {
            return this.Compare(this, obj as IndexEntry);
        }
    }
}