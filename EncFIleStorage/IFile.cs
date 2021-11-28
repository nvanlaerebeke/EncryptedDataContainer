using EncFIleStorage.Data;

namespace EncFIleStorage
{
    internal interface IFile
    {
        void Dispose();
        void Create();
        void Open();
        void OpenWrite();
        void Close();
        void Write(byte[] data, int offset);
        string ReadAllText();
        void Delete();
    }
}