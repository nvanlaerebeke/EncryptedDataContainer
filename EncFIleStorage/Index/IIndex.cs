using EncFileStorage;

namespace EncFIleStorage
{
    internal interface IIndex
    {
        IndexEntry GetFreeBlock(int size);
        IndexEntry GetBlock(int offset);
        void Flush();
        IndexEntry[] GetAll();
    }
}