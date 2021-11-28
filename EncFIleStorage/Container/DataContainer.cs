using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using EncFileStorage;
using EncFIleStorage.Container;

namespace EncFIleStorage
{
    internal class DataContainer
    {
        private readonly Stream _stream;

        public DataContainer(Stream stream)
        {
            _stream = stream;
            DataContainerInfo = new DataContainerInfo(this);
            Index = new Index(this);
        }

        public IDataContainerInfo DataContainerInfo { get; }

        public IIndex Index { get; }

        public Stream GetStream()
        {
            return _stream;
        }

        public byte[] Read()
        {
            return Array.Empty<byte>();
        }

        public byte[] Read(IndexEntry indexEntry)
        {
            _stream.Position = indexEntry.Start;
            var data = new byte[indexEntry.Length];
            _stream.Read(data, 0, indexEntry.Length);
            return data;
        }

        public void Write(byte[] data, int offset)
        {
            //ToDo: don't use a list
            var blocks = new List<byte[]>();
            if (data.Length <= 4096)
                blocks.Add(data);
            else
                do
                {
                    var start = blocks.Count * 4096;
                    var end = start + 4096;
                    if (end > data.Length) end = data.Length;
                    
                    blocks.Add(data[start..end]);

                    if (end >= data.Length) break;
                } while (true);

            foreach (var block in blocks)
            {
                WriteBlocks(blocks, offset);    
            }
            
            Index.Flush();
            _stream.Flush();
        }

        private void WriteBlocks(List<byte[]> dataList, int offset)
        {
            foreach (var data in dataList)
            {
                var indexEntry = Index.GetBlock(offset);
                if (indexEntry == null) indexEntry = Index.GetFreeBlock(data.Length);
                
                if (_stream.Length < indexEntry.End) _stream.SetLength(indexEntry.End);
                
                _stream.Position = indexEntry.Start;
                _stream.Write(data, 0, data.Length);
                indexEntry.SetUsed((ulong) offset / 4096, data.Length);

                offset += 4096;
            }
        }

        public void Write(IndexEntry indexEntry, byte[] data)
        {
            if (data.Length > indexEntry.GetAvailableSize()) throw new OverflowException();
            if (_stream.Length < indexEntry.End) _stream.SetLength(indexEntry.End);
            _stream.Position = indexEntry.Start;
            _stream.Write(data, 0, data.Length);
        }

        public byte[] ReadAll()
        {
            var entries = Index.GetAll();
            var data = Array.Empty<byte>();
            foreach (var entry in entries)
            {
                //ToDo: convert to array copy and resize
                //      needs to be benchmarked to know what is fastest/most memory efficient
                var entryData = new byte[entry.Length];
                _stream.Position = entry.Start;
                _stream.Read(entryData, 0, entry.Length);

                var start = data.Length;
                Array.Resize(ref data, data.Length + entryData.Length);
                Array.Copy(entryData, 0, data, start, entryData.Length);
            }

            return data;
        }
    }
}