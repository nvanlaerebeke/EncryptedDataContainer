using System.Buffers.Binary;
using System.IO;

namespace EncFIleStorage.Container
{
    /// <summary>
    /// DataContainerInfo represents the information about the current data container
    /// This includes the version so that different storage solutions can be supported
    /// </summary>
    internal class DataContainerInfo : IDataContainerInfo
    {
        
        private readonly IDataContainer _dataContainer;
        private ushort? _version;
        
        public DataContainerInfo(IDataContainer dataContainer)
        {
            _dataContainer = dataContainer;
        }

        public ushort GetVersion()
        {
            //Already loaded?
            if (_version.HasValue) return _version.Value;
            
            //read the version from the stream
            var stream = _dataContainer.GetStream(FileMode.Open, FileAccess.ReadWrite);

            stream.Position = 0;
            var versionBytes = new byte[2];
            stream.Read(versionBytes, 0, 2);
            _version = BinaryPrimitives.ReadUInt16LittleEndian(versionBytes);

            return _version.Value;
        }

        public uint End => 2;
    }
}