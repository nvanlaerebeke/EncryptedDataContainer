namespace EncFIleStorage.Container
{
    internal interface IDataContainerInfo
    {
        ushort GetVersion();
        uint End { get; }
    }
}