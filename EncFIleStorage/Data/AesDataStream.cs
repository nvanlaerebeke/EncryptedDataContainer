using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EncFIleStorage.Container;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsTCPIP;

namespace EncFIleStorage.Data
{
    internal class AesDataStream : Stream
    {
        private readonly IDataContainer _dataContainer;
        private readonly Stream _stream;

        public AesDataStream(IDataContainer dataContainer)
        {
            ;
            _dataContainer = dataContainer;
            _stream = _dataContainer.GetStream(FileMode.Open, FileAccess.ReadWrite);
        }

        public override bool CanRead => _stream.CanRead;
        public override bool CanSeek => _stream.CanSeek;
        public override bool CanTimeout => _stream.CanTimeout;
        public override bool CanWrite => _stream.CanWrite;
        public override long Length => _stream.Length;

        public override long Position
        {
            get => _stream.Position;
            set => _stream.Position = value;
        }

        public override int ReadTimeout => _stream.ReadTimeout;

        public override int WriteTimeout
        {
            get => _stream.WriteTimeout;
            set => _stream.WriteTimeout = value;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            return Task.Run(() =>
            {
                //Get the block we need to start reading from
                var blockNr = (long) Math.Floor((double) offset / 4096);
                var startByte = blockNr * 4096;

                //start reading from somewhere in the block
                var blockOffset = offset - startByte;
                var bytesRead = 0;
                do
                {
                    var block = _dataContainer.ReadBlock(blockNr);
                    var blockData = block.GetData();
                    if (blockOffset > 0 && bytesRead == 0)
                    {
                        Array.Copy(blockData, blockOffset, buffer, 0, blockData.Length - blockOffset);
                        bytesRead += (int) (blockData.Length - blockOffset);
                    }
                    else
                    {
                        if (bytesRead + blockData.Length > count)
                        {
                            Array.Copy(blockData, 0, buffer, bytesRead, count - bytesRead);
                        }
                        else
                        {
                            Array.Copy(blockData, 0, buffer, bytesRead, blockData.Length);
                        }

                        bytesRead += blockData.Length;
                    }

                    blockNr++;
                } while (bytesRead < count);
            });
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            throw new NotImplementedException();
            //return _stream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void Close()
        {
            throw new NotImplementedException();
            //_stream.Close();
        }

        public override void CopyTo(Stream destination, int bufferSize)
        {
            throw new NotImplementedException();
            //_stream.CopyTo(destination, bufferSize);
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
            //return _stream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        [Obsolete("CreateWaitHandle will be removed eventually.  Please use \"new ManualResetEvent(false)\" instead.")]
        protected override WaitHandle CreateWaitHandle()
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            //_stream.Dispose();
            throw new NotImplementedException();
        }

        public override ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
            //return _stream.DisposeAsync();
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
            //return _stream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
            //_stream.EndWrite(asyncResult);
        }

        public override void Flush()
        {
            throw new NotImplementedException();
            //_stream.Flush();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
            //return _stream.FlushAsync(cancellationToken);
        }

        [Obsolete("Do not call or override this method.")]
        protected override void ObjectInvariant()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
            //return _stream.Read(buffer, offset, count);
        }

        public override int Read(Span<byte> buffer)
        {
            throw new NotImplementedException();
            //return _stream.Read(buffer);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
            //return _stream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new())
        {
            throw new NotImplementedException();
            //return _stream.ReadAsync(buffer, cancellationToken);
        }

        public override int ReadByte()
        {
            throw new NotImplementedException();
            //return _stream.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
            //return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
            //_stream.SetLength(value);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            //Notes
            // 1. the offset given does not always begin at data block index 0
            // 2. data blocks are not always 4096 bytes!
            // 3. buffer end isn't always the end of the gotten block

            //ToDo: don't use a list
            var blocks = new List<DataBlock>(); //blocks to write
            var blockNr = (long) Math.Floor((double) offset / 4096); //first block where offset starts
            var blockOffset = offset - blockNr * 4096; //location that the data starts in the first block
            long bytesWritten = 0;

            do
            {
                var block = _dataContainer.ReadBlock(blockNr);
                var data = block.GetData();

                if (bytesWritten == 0 && blockOffset > 0)
                {
                    //not a full block
                    if (blockOffset + buffer.Length <= 4096)
                    {
                        var newBlock = new byte[blockOffset + buffer.Length];
                        Array.Copy(data, 0, newBlock, 0, blockOffset);
                        Array.Copy(buffer, 0, newBlock, blockOffset, newBlock.Length - blockOffset);
                        block.SetData(newBlock);
                        blocks.Add(block);
                        bytesWritten += newBlock.Length - blockOffset;
                        blockNr++;
                    }
                    //full block
                    else
                    {
                        var newBlock = new byte[4096];
                        Array.Copy(data, 0, newBlock, 0, blockOffset);
                        Array.Copy(buffer, 0, newBlock, blockOffset, 4096 - blockOffset);
                        block.SetData(newBlock);
                        blocks.Add(block);
                        bytesWritten += 4096 - blockOffset;
                        blockNr++;
                    }
                }
                else
                {
                    //full block?
                    if (bytesWritten + 4096 <= buffer.Length)
                    {
                        var newBlock = new byte[4096];
                        Array.Copy(buffer, bytesWritten, newBlock, 0, 4096);
                        block.SetData(newBlock);
                        blocks.Add(block);
                        bytesWritten += 4096;
                    }
                    //last/partial block
                    else
                    {
                        var newBlock = new byte[buffer.Length - bytesWritten];
                        Array.Copy(buffer, bytesWritten, newBlock, 0, buffer.Length - bytesWritten);
                        block.SetData(newBlock);
                        blocks.Add(block);
                        bytesWritten += buffer.Length - bytesWritten;
                    }
                }
            } while (bytesWritten < buffer.Length);
            _dataContainer.Write(blocks);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            throw new NotImplementedException();
            //_stream.Write(buffer);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
            //return _stream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = new())
        {
            throw new NotImplementedException();
            //return _stream.WriteAsync(buffer, cancellationToken);
        }

        public override void WriteByte(byte value)
        {
            throw new NotImplementedException();
            //_stream.WriteByte(value);
        }

        [Obsolete("This Remoting API is not supported and throws PlatformNotSupportedException.", DiagnosticId = "SYSLIB0010", UrlFormat = "https://aka.ms/dotnet-warnings/{0}")]
        public override object InitializeLifetimeService()
        {
            return _stream.InitializeLifetimeService();
        }

        public override bool Equals(object? obj)
        {
            return _stream.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _stream.GetHashCode();
        }

        public override string ToString()
        {
            return _stream.ToString();
        }
    }
}