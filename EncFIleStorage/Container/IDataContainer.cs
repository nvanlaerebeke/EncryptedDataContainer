using System.IO;
using EncFIleStorage.FileIndex;

namespace EncFIleStorage.Container
{
    internal interface IDataContainer
    {
        IDataContainerInfo DataContainerInfo { get; }
        IIndex Index { get; }
        Stream GetStream();
        byte[] Read();
        byte[] Read(IndexEntry indexEntry);
        void Write(byte[] data, int offset);
        void Write(IndexEntry indexEntry, byte[] data);
        byte[] ReadAll();
    }
}