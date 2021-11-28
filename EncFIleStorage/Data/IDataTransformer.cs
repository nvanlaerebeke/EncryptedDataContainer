namespace EncFIleStorage.Data
{
    internal interface IDataTransformer
    {
        byte[] In(byte[] data);
        byte[] Out(byte[] data);
    }
}
