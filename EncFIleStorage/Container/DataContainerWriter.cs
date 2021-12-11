using System;
using System.Collections.Generic;
using System.IO;
using EncFIleStorage.Data;
using EncFIleStorage.FileIndex;

namespace EncFIleStorage.Container
{
    internal class DataContainerWriter<T> where T : IDataTransformer, new()
    {
        private DataContainer<T> _dataContainer;
        private readonly IDataTransformer _dataTransformer;

        public DataContainerWriter(DataContainer<T> dataContainer, IDataTransformer dataTransformer)
        {
            _dataContainer = dataContainer;
            _dataTransformer = dataTransformer;
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
        }

        public void Write(IndexEntry indexEntry, byte[] data)
        {
            if (data.Length > indexEntry.GetAvailableSize())
            {
                throw new OverflowException();
            }


            var stream = _dataContainer.GetStream(FileMode.Open, FileAccess.ReadWrite);
            if (stream.Length < indexEntry.End)
            {
                stream.SetLength(indexEntry.End);
            }

            stream.Position = indexEntry.Start;
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        private void WriteBlocks(List<byte[]> dataList, int offset)
        {
            foreach (var data in dataList)
            {
                var transformedData = _dataTransformer.In(data);
                var indexEntry = _dataContainer.Index.GetBlock(offset);
                if (indexEntry == null)
                {
                    indexEntry = _dataContainer.Index.GetFreeBlock(transformedData.Length);
                }

                var stream = _dataContainer.GetStream(FileMode.Open, FileAccess.ReadWrite);
                if (stream.Length < indexEntry.End)
                {
                    stream.SetLength(indexEntry.End);
                }

                stream.Position = indexEntry.Start;
                stream.Write(transformedData, 0, transformedData.Length);
                //Switch to events
                _dataContainer.Index.SetUsed(indexEntry, (ulong) offset / 4096, transformedData.Length);
                offset += 4096;
            }
        }
    }
}