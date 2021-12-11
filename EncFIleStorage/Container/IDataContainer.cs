using System;
using System.IO;
using EncFIleStorage.FileIndex;

namespace EncFIleStorage.Container
{
    internal interface IDataContainer : IDisposable
    {
        IDataContainerInfo DataContainerInfo { get; }
        IIndex Index { get; }
        byte[] Read();
        byte[] Read(IndexEntry indexEntry);
        void Write(byte[] data, int offset);
        void Write(IndexEntry indexEntry, byte[] data);
        byte[] ReadAll();
        Stream GetStream(FileMode mode, FileAccess access);
        byte[] ReadBlock(long blockNr);
    }
}