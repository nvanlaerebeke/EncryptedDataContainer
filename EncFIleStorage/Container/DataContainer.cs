using System;
using System.Collections.Generic;
using System.IO;
using EncFIleStorage.Data;
using EncFIleStorage.FileIndex;
using Microsoft.Diagnostics.Runtime.ICorDebug;
using Index = EncFIleStorage.FileIndex.Index;

namespace EncFIleStorage.Container
{
    internal class DataContainer<T> : IDisposable, IDataContainer where T: IDataTransformer, new()
    {
        private readonly string _path;
        private Stream _stream;
        private readonly DataContainerWriter<T> _dataContainerWriter;
        private readonly DataContainerReader<T> _dataContainerReader;

        public DataContainer(string path)
        {
            _path = path;
            
            DataContainerInfo = new DataContainerInfo(this);
            Index = new Index(this);
            
            var dataTransformer = new T();
            _dataContainerWriter = new DataContainerWriter<T>(this, dataTransformer);
            _dataContainerReader = new DataContainerReader<T>(this, dataTransformer);
        }

        public IDataContainerInfo DataContainerInfo { get; }

        public IIndex Index { get; }

        public byte[] ReadBlock(long index)
        {
            return _dataContainerReader.ReadBlock(index);
        }
        /*public byte[] Read()
        {
            return _dataContainerReader.Read();
        }

        public byte[] Read(IndexEntry indexEntry)
        {
            return _dataContainerReader.Read(indexEntry);
        }

        public void Write(byte[] data, int offset)
        {
            _dataContainerWriter.Write(data, offset);
            Index.Flush();
        }

        public void Write(IndexEntry indexEntry, byte[] data)
        {
            _dataContainerWriter.Write(indexEntry, data);
            Index.Flush();
        }

        public byte[] ReadAll()
        {
            return _dataContainerReader.ReadAll();
        }*/

        public Stream GetStream(FileMode mode, FileAccess access)
        {
            return _stream ??= new FileInfo(_path).Open(mode, access);
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}