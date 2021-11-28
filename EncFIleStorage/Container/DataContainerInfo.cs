using System.Buffers.Binary;

namespace EncFIleStorage.Container
{
    /// <summary>
    /// DataContainerInfo represents the information about the current data container
    /// This includes the version so that different storage solutions can be supported
    /// </summary>
    internal class DataContainerInfo : IDataContainerInfo
    {
        
        private readonly DataContainer _dataContainer;
        private ushort? _version;
        
        public DataContainerInfo(DataContainer dataContainer)
        {
            _dataContainer = dataContainer;
        }

        public ushort GetVersion()
        {
            //Already loaded?
            if (_version.HasValue) return _version.Value;
            
            //read the version from the stream
            var stream = _dataContainer.GetStream();

            stream.Position = 0;
            var versionBytes = new byte[2];
            stream.Read(versionBytes, 0, 2);
            _version = BinaryPrimitives.ReadUInt16LittleEndian(versionBytes);

            return _version.Value;
        }

        public uint End => 2;
    }
}