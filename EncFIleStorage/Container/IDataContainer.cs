using System;
using System.Collections.Generic;
using System.IO;
using EncFIleStorage.Data;
using EncFIleStorage.FileIndex;

namespace EncFIleStorage.Container
{
    internal interface IDataContainer : IDisposable
    {
        IDataTransformer Transformer { get; }
        IDataContainerInfo DataContainerInfo { get; }
        IIndex Index { get; }

        DataBlock ReadBlock(long index);
        //byte[] Read();
        byte[] Read(IndexEntry indexEntry);
        //void Write(byte[] data, int offset);
        void Write(IndexEntry indexEntry, byte[] data);
        //byte[] ReadAll();
        Stream GetStream(FileMode mode, FileAccess access);
        //DataBlock ReadBlock(long blockNr);
        void Write(List<DataBlock> blocks);
    }
}