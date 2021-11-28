namespace EncFIleStorage
{
    internal interface IDataContainerInfo
    {
        ushort GetVersion();
        uint End { get; }
    }
}