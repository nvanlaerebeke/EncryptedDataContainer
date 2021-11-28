namespace EncFIleStorage.FileIndex
{
    internal interface IIndex
    {
        IndexEntry GetFreeBlock(int size);
        IndexEntry GetBlock(int offset);
        void Flush();
        IndexEntry[] GetAll();
        void SetUsed(IndexEntry indexEntry, ulong offset, int dataLength);
    }
}