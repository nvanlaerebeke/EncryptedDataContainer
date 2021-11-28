using System;
using System.Collections.Generic;
using System.IO;
using EncFIleStorage.Data;
using EncFIleStorage.FileIndex;
using Index = EncFIleStorage.FileIndex.Index;

namespace EncFIleStorage.Container
{
    internal class DataContainer<T> : IDataContainer where T: IDataTransformer, new()
    {
        private readonly Stream _stream;
        private readonly IDataTransformer _dataTransformer;
        public DataContainer(Stream stream)
        {
            _stream = stream;
            _dataTransformer = new T();
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
            return _dataTransformer.Out(data);
        }

        public void Write(byte[] data, int offset)
        {
            //ToDo: don't use a list
            var blocks = new List<byte[]>();
            if (data.Length <= 4096)
            {
                blocks.Add(data);
            }
            else
            {
                do
                {
                    var start = blocks.Count * 4096;
                    var end = start + 4096;
                    if (end > data.Length)
                    {
                        end = data.Length;
                    }

                    blocks.Add(data[start..end]);

                    if (end >= data.Length)
                    {
                        break;
                    }
                } while (true);
            }

            WriteBlocks(blocks, offset);

            Index.Flush();
            _stream.Flush();
        }

        private void WriteBlocks(List<byte[]> dataList, int offset)
        {
            foreach (var data in dataList)
            {
                var transformedData = _dataTransformer.In(data);
                var indexEntry = Index.GetBlock(offset);
                if (indexEntry == null)
                {
                    indexEntry = Index.GetFreeBlock(transformedData.Length);
                }

                if (_stream.Length < indexEntry.End)
                {
                    _stream.SetLength(indexEntry.End);
                }

                _stream.Position = indexEntry.Start;
                _stream.Write(transformedData, 0, transformedData.Length);
                Index.SetUsed(indexEntry, (ulong) offset / 4096, transformedData.Length);
                offset += 4096;
            }
        }

        public void Write(IndexEntry indexEntry, byte[] data)
        {
            if (data.Length > indexEntry.GetAvailableSize())
            {
                throw new OverflowException();
            }

            if (_stream.Length < indexEntry.End)
            {
                _stream.SetLength(indexEntry.End);
            }

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

                var transformedData = _dataTransformer.Out(entryData);
                var start = data.Length;
                Array.Resize(ref data, data.Length + transformedData.Length);
                Array.Copy(transformedData, 0, data, start, transformedData.Length);
            }

            return data;
        }
    }
}