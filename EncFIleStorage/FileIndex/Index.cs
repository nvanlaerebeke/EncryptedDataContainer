using System;
using System.Buffers.Binary;
using System.Dynamic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using EncFIleStorage.Container;

namespace EncFIleStorage.FileIndex
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    internal class Index : IIndex
    {
        private readonly IDataContainer _dataContainer;
        private IndexEntry[] _freeBlockList;
        private IndexEntry[] _index;

        public Index(IDataContainer dataContainer)
        {
            _dataContainer = dataContainer;
        }

        public IndexEntry GetFreeBlock(int size)
        {
            var freeBlocks = GetFreeBlocks();
            //ToDo: benchmark against none linq methods
            var entry = freeBlocks.FirstOrDefault(b => b.End - b.Start >= size);
            if (entry != null)
            {
                return entry;
            }

            var index = GetIndex();
            if (_index.Length > 0)
            {
                var lastBlock = index[^1];
                entry = IndexEntry.Create(lastBlock.SequenceNumber, (uint) lastBlock.End, (ushort) size);
            }
            else
            {
                entry = IndexEntry.Create(1, _dataContainer.DataContainerInfo.End + 4096, (ushort) size);
            }

            //ToDo: test efficiency of .Append, Array.Copy might be much better
            //      this depends on how .net has implemented the append
            _index = _index.Append(entry).ToArray();
            _freeBlockList = _freeBlockList.Append(entry).ToArray();

            return entry;
        }

        public IndexEntry GetBlock(int offset)
        {
            var blockIndex = offset / 4096;
            var index = GetIndex();
            return blockIndex >= 0 && index.Length > blockIndex ? index[blockIndex] : null;
        }

        public IndexEntry GetBlockAtIndex(int i)
        {
            var index = GetIndex();
            return index.Length > i ? index[i] : null;
        }
        
        [Benchmark]
        public void Flush()
        {
            
            var index = IndexMoveFirstBlockIfNeeded(GetIndex());
            index.ToList().RemoveAll(x => x == null);
            Array.Sort(index);
            _index = index;
            
            var stream = _dataContainer.GetStream(FileMode.Open, FileAccess.ReadWrite);
            var indexBytes = index.Length * IndexEntry.IndexEntryLength;
            if (indexBytes == 0)
            {
                stream.SetLength(0);
                return;
            }

            var indexEntries = index.Where(x => x != null).ToArray();
            var bytes = new byte[indexEntries.Length * IndexEntry.IndexEntryLength];

            for (long i = 0; i < indexEntries.Length; i++)
            {
                var entryBytes = indexEntries[i].GetBytes();
                Array.Copy(entryBytes, 0, bytes, i * IndexEntry.IndexEntryLength, entryBytes.Length);
            }

            //Write the index length
            stream.Position = _dataContainer.DataContainerInfo.End;
            var sizeBytes = new byte[2];
            BinaryPrimitives.WriteInt16LittleEndian(sizeBytes, (short) bytes.Length);
            stream.Write(sizeBytes);

            //Write the index to the file
            stream.Write(bytes);
        }

        public IndexEntry[] GetAll()
        {
            return GetIndex();
        }

        public void SetUsed(IndexEntry indexEntry, ulong offset, int dataLength)
        {
            var index = _freeBlockList.ToList().IndexOf(indexEntry);
            if (index > -1)
            {
                _freeBlockList[index] = null;
            }

            indexEntry.SetUsed(offset, dataLength);
        }

        private IndexEntry[] GetFreeBlocks()
        {
            //already loaded?
            if (_freeBlockList != null)
            {
                return _freeBlockList.Where(x => x is {Free: true}).ToArray();
            }

            var index = GetIndex();

            //ToDo: benchmark cpu/memory usage with none linq methods
            _freeBlockList = index.Where(entry => entry.Free).ToArray();

            return _freeBlockList;
        }

        /// <summary>
        ///     Loads the index for the current container
        /// </summary>
        /// <returns></returns>
        private IndexEntry[] GetIndex()
        {
            //Index already loaded?
            if (_index != null)
            {
                return _index;
            }

            var stream = _dataContainer.GetStream(FileMode.Open, FileAccess.ReadWrite);
            //if the file is empty, there is nothing
            if (stream.Length == 0)
            {
                _index = Array.Empty<IndexEntry>();
                return _index;
            }

            //Get the index size
            //The size is stored as a ushort in the first 2 bytes of the index
            stream.Position = _dataContainer.DataContainerInfo.End;
            var indexEnd = new byte[2];
            stream.Read(indexEnd, 0, 2);
            var indexEndByte = BitConverter.ToUInt16(indexEnd) + (ushort) _dataContainer.DataContainerInfo.End;

            //now read the index
            var index = new byte[indexEndByte - 2];
            stream.Read(index);

            //get the index entries 
            var endIndexLength = index.Length;
            _index = Array.Empty<IndexEntry>();

            var blockIndex = 0;
            while (true)
            {
                var start = blockIndex * IndexEntry.IndexEntryLength;
                var end = start + IndexEntry.IndexEntryLength;
                //ToDo: benchmark vs Array.Copy
                _index = _index.Append(new IndexEntry(index[start..end])).ToArray();
                blockIndex++;

                if (end >= endIndexLength)
                {
                    break;
                }
            }

            //Having the IndexEntries in sequence helps
            Array.Sort(_index);

            //Set the previous index entry end to the next block's starting point
            //This start/end is not always the same size(ex. 4k) when the blocks get encrypted
            //When blocks get re-used the file container gets fragmented
            //Keeping track of the real 'ending' helps keep fragmentation low when doing file updates
            for (long i = 1; i < _index.Length; i++)
            {
                _index[i - 1].SetBlockBoundary(_index[i].Start);
            }

            return _index;
        }

        private IndexEntry[] IndexMoveFirstBlockIfNeeded(IndexEntry[] index)
        {
            //Check if a data block needs moving
            var indexEnd = _dataContainer.DataContainerInfo.End + index.Length * IndexEntry.IndexEntryLength;
            if (indexEnd > index[^1].End)
            {
                do
                {
                    var oldEntry = index[0];
                    index[0] = null;

                    var freeBlock = GetFreeBlock(oldEntry.Length);
                    var data = _dataContainer.Read(oldEntry);
                    _dataContainer.Write(freeBlock, data);
                    freeBlock.SetUsed(oldEntry.SequenceNumber, data.Length);
                } while (indexEnd > index[^1].End);
            }

            return index;
        }
    }
}