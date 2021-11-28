using System;
using System.IO;
using System.Text;
using EncFIleStorage.Container;
using EncFIleStorage.Data;

namespace EncFIleStorage
{
    internal class File<T> : IDisposable, IFile where T : IDataTransformer, new()
    {
        private readonly FileInfo _info;
        private IDataContainer _dataContainer;
        private FileStream _stream;

        public File(string path)
        {
            _info = new FileInfo(path);
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }

        public void Create()
        {
            if (!_info.Exists)
            {
                _info.Create().Close();
            }
        }

        public void Open()
        {
            if (_stream == null)
            {
                _stream = _info.OpenRead();
            }

            _dataContainer = new DataContainer<T>(_stream);
        }

        public void OpenWrite()
        {
            if (_stream != null)
            {
                Close();
            }

            _stream = _info.OpenWrite();
            _dataContainer = new DataContainer<T>(_stream);
        }

        public void Close()
        {
            _stream?.Flush();
            _stream?.Close();
            _stream = null;
            _dataContainer = null;
        }

        public void Write(byte[] data, int offset)
        {
            _dataContainer.Write(data, offset);
        }

        public string ReadAllText()
        {
            var data = _dataContainer.ReadAll();
            return Encoding.UTF8.GetString(data);
        }

        public void Delete()
        {
            if (!_info.Exists)
            {
                return;
            }

            Close();
            _info.Delete();
        }
    }
}