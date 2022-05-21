using System;
using System.IO;
using EncFIleStorage.Data;
using EncFIleStorage.FileIndex;

namespace EncFIleStorage.Container
{
    internal class DataContainerReader<T> where T : IDataTransformer, new()
    {
        private DataContainer<T> _dataContainer;
        private readonly IDataTransformer _dataTransformer;

        public DataContainerReader(DataContainer<T> dataContainer, IDataTransformer dataTransformer)
        {
            _dataContainer = dataContainer;
            _dataTransformer = dataTransformer;
        }
        
        public byte[] Read(IndexEntry indexEntry)
        {
            var stream = _dataContainer.GetStream(FileMode.Open, FileAccess.ReadWrite);
            stream.Position = indexEntry.Start;
            var data = new byte[indexEntry.Length];
            stream.Read(data, 0, indexEntry.Length);
            return _dataTransformer.Out(data);
        }

        public byte[] ReadAll()
        {
            var stream = _dataContainer.GetStream(FileMode.Open, FileAccess.ReadWrite);
            var entries = _dataContainer.Index.GetAll();
            var data = Array.Empty<byte>();
            foreach (var entry in entries)
            {
                //ToDo: convert to array copy and resize
                //      needs to be benchmarked to know what is fastest/most memory efficient
                var entryData = new byte[entry.Length];
                stream.Position = entry.Start;
                stream.Read(entryData, 0, entry.Length);

                var transformedData = _dataTransformer.Out(entryData);
                var start = data.Length;
                Array.Resize(ref data, data.Length + transformedData.Length);
                Array.Copy(transformedData, 0, data, start, transformedData.Length);
            }

            return data;
        }

        public DataBlock ReadBlock(long index)
        {
            var entry = _dataContainer.Index.GetBlockAtIndex(index);
            return new DataBlock(entry, _dataContainer);
        }
    }
}