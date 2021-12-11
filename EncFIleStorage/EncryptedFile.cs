using System;
using System.IO;
using System.Text;
using EncFIleStorage.Container;
using EncFIleStorage.Data;

namespace EncFIleStorage
{
    public class EncryptedFile : IDisposable, IFile
    {
        private readonly string _path;
        private IDataContainer _dataContainer;

        public EncryptedFile(string path)
        {
            _dataContainer = new DataContainer<AesDataTransformer>(path);
        }
        
        public Stream GetStream(FileMode mode, FileAccess access)
        {
            return _dataContainer.GetStream(mode, access);
        }

        public Stream GetStream(FileMode mode)
        {
            throw new NotImplementedException();
        }

        public Stream GetStream(FileMode mode, FileAccess access, FileShare share)
        {
            throw new NotImplementedException();
        }

        public Stream GetStream(FileMode mode, FileAccess access, FileShare share, int bufferSize)
        {
            throw new NotImplementedException();
        }

        public Stream GetStream(FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options)
        {
            throw new NotImplementedException();
        }

        public Stream GetStream(FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _dataContainer?.Dispose();
        }
    }
}