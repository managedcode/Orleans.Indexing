using System.Diagnostics;
using Lucene.Net.Store;
using ManagedCode.Storage.Core;
using Directory = Lucene.Net.Store.Directory;

namespace Orleans.Index.Lucene.Temp
{
    /// <summary>
    /// Implements IndexOutput semantics for a write/append straight to blob storage
    /// </summary>
    public class AzureIndexOutput : IndexOutput
    {
        private AzureDirectory _azureDirectory;
        private IStorage _storage;
        private string _name;
        private IndexOutput _indexOutput;
        private Mutex _fileMutex;

        public AzureIndexOutput(AzureDirectory azureDirectory, string name, IStorage storage)
        {
            _name = name;
            _fileMutex = BlobMutexManager.GrabMutex(_name);
            _fileMutex.WaitOne();
            try
            {
                _azureDirectory = azureDirectory;
                _storage = storage;

                // create the local cache one we will operate against...
                _indexOutput = CacheDirectory.CreateOutput(_name, IOContext.DEFAULT);
            }
            finally
            {
                _fileMutex.ReleaseMutex();
            }
        }

        public Directory CacheDirectory => _azureDirectory.CacheDirectory;

        public override void Flush()
        {
            _indexOutput?.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            _fileMutex.WaitOne();
            try
            {
                // make sure it's all written out
                _indexOutput.Flush();

                long originalLength = _indexOutput.Length;
                _indexOutput.Dispose();

                using (var blobStream = new StreamInput(CacheDirectory.OpenInput(_name, IOContext.DEFAULT)))
                {
                    // push the blobStream up to the cloud
                    _storage.UploadStreamAsync(_name, blobStream);

                    // set the metadata with the original index file properties
                    // _blob.SetMetadata();

                    Debug.WriteLine($"{_azureDirectory.Name} PUT {_name} bytes to {blobStream.Length} in cloud");
                }

#if FULLDEBUG
                Debug.WriteLine($"{_azureDirectory.Name} CLOSED WRITESTREAM {_name}");
#endif
                // clean up
                _indexOutput = null;
                GC.SuppressFinalize(this);
            }
            finally
            {
                _fileMutex.ReleaseMutex();
            }
        }

        public override long Length => _indexOutput.Length;

        public override void WriteByte(byte b)
        {
            _indexOutput.WriteByte(b);
        }

        public override void WriteBytes(byte[] b, int length)
        {
            _indexOutput.WriteBytes(b, length);
        }

        public override void WriteBytes(byte[] b, int offset, int length)
        {
            _indexOutput.WriteBytes(b, offset, length);
        }

        public override long GetFilePointer()
        {
            return _indexOutput.GetFilePointer();
        }

        public override void Seek(long pos)
        {
            //_indexOutput.Seek(pos);
        }

        public override long Checksum => _indexOutput.Checksum;
    }
}