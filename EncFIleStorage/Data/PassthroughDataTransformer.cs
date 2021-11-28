namespace EncFIleStorage.Data
{
    internal class PassthroughDataTransformer : IDataTransformer
    {
        public byte[] In(byte[] data)
        {
            return data;
        }

        public byte[] Out(byte[] data)
        {
            return data;
        }
    }
}