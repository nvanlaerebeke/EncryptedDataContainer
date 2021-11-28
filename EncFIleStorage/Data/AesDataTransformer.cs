using System;
using CrazyFS.Encryption;
using Microsoft.Diagnostics.Runtime.Interop;

namespace EncFIleStorage.Data
{
    internal class AesDataTransformer : IDataTransformer
    {
        private string _salt = "2JSn0nhJPVrjKGgZeRQkqYHGXUWPzHRnU9blxhe7qdWJkELHB+vCxgGTUbG/GJ4vp4hg6muGlzVQL5xCI1CFSq1rPJSxWKqdoz7KFFKNsW0xFOVwQTOxVMAVeN3gLQd0HQm93CoEVLtPpFIqGIaioKyZY/Y8uFVYaod5pe7yWsDPcfOPVawyo8MFDzG1zBYJEoPsbglz7sDs1KpKWEWJRomBlghRc1/x6oaraCaMoGE97jLvT8c+ZfRLQLqaBglOj5K0ELRWlKXgbOqPXdAbUBwlDT7WJ+yiAKuC6gUK4JIk/o4l54QbcsKihsy5JszHMtpb78epesB+57e0AMOqUw==";
        private string _password = "myPassword";
            
        public ByteCrypto _crypto;

        public AesDataTransformer()
        {
            _crypto = new ByteCrypto(_password, Convert.FromBase64String(_salt));
        }
        public byte[] In(byte[] data)
        {
            return _crypto.Encrypt(data);
        }

        public byte[] Out(byte[] data)
        {
            return _crypto.Decrypt(data);
        }
    }
}