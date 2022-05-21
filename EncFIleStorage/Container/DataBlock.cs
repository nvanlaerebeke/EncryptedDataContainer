using System;
using System.Buffers.Binary;
using System.IO;
using EncFIleStorage.FileIndex;

namespace EncFIleStorage.Container
{
    internal class DataBlock
    {
        private readonly IDataContainer _dataContainer;
        private byte[] _newData;

        public DataBlock(IndexEntry entry, IDataContainer dataContainer)
        {
            IndexEntry = entry;
            _dataContainer = dataContainer;
        }

        public IndexEntry IndexEntry { get; }

        public byte[] GetData()
        {
            var buffer = new byte[IndexEntry.Length];
            var stream = _dataContainer.GetStream(FileMode.Open, FileAccess.ReadWrite);
            stream.Read(buffer, (int) IndexEntry.Start, IndexEntry.Length);
            return _dataContainer.Transformer.Out(buffer);
        }

        public void SetData(byte[] newData)
        {
            _newData = newData;
        }

        //Apply the changes to the container
        public void Push()
        {
            var encrypted = _dataContainer.Transformer.In(_newData);
            if (IndexEntry.GetAvailableSize() < encrypted.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            var stream = _dataContainer.GetStream(FileMode.Append, FileAccess.ReadWrite);
            var startBytes = new byte[4];
            var lengthBytes = new byte[2];
            BinaryPrimitives.WriteUInt32LittleEndian(startBytes, IndexEntry.Start);
            BinaryPrimitives.WriteUInt32LittleEndian(lengthBytes, (uint) _newData.Length);

            var block = new byte[_newData.Length + 1 + startBytes.Length + lengthBytes.Length];
            block[0] = 1;
            Array.Copy(startBytes, 0, block, 1, 5); //add start = 4bytes            
            Array.Copy(lengthBytes, 0, block, 5, 2); //add length = 2 bytes
            Array.Copy(_newData, 0, block, 7, _newData.Length); //add data 

            stream.Position = IndexEntry.Start;
            stream.Write(block);
        }
    }
}