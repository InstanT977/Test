using RwmSignatureGenerator.Data;
using RwmSignatureGenerator.Data.Common;
using RwmSignatureGenerator.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RwmSignatureGenerator
{
    class FileSignatureGenerator
    {
        FileReader _reader;
        IHashGenerator _hashGenerator;
        ILogger _logger;
        ConcurrentQueue<FileDataBlock> _fileBlocksQueue = new ConcurrentQueue<FileDataBlock>();

        internal FileSignatureGenerator(IHashGenerator hashGenerator,ILogger logger)
        {
            _hashGenerator = hashGenerator;
            _logger = logger;
        }

        public void GenerateSignature(string filePath,int signatureBlockSize)
        {
            try
            {
                _reader = new FileReader(filePath, signatureBlockSize,_logger);
                _reader.FileBlockRecieved += _reader_FileBlockRecieved;
                long signPerspectiveCount = _reader.FileBlocksCount;
                RThreadPool.Instance.Run(ComputeHash, ref signPerspectiveCount);
                if (signPerspectiveCount > 0)
                {
                    _reader.Read();
                    RThreadPool.Instance.WaitWorksFinished();
                }
                else
                {
                    _logger.Error($"Cannot start generate signature for file {filePath}! Too many effective generations launched!");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                RThreadPool.Instance.StopWork();
                RThreadPool.Instance.Check((int a) => { return a; });
            }
        }

        private void ComputeHash()
        {
            while (true)
            {
                try
                {
                    if (RThreadPool.Instance.NeedStopWork())
                    {
                        return;
                    }
                    FileDataBlock currentblock = null;
                    _fileBlocksQueue.TryDequeue(out currentblock);
                    if (currentblock == null)
                    {
                        if(RThreadPool.Instance.IsWorkCanBeFinished)
                        {
                            return;
                        }
                        Thread.Sleep(10);
                        continue;
                    }

                    var hash = _hashGenerator.GenerateHash(currentblock.Data);
                    _logger.Info($"#{currentblock.Number} Hash: {hash}");
                }
                catch(Exception ex)
                {
                    _logger.Error(ex);
                }
            }
        }

        private void _reader_FileBlockRecieved(object sender, FileDataBlock e)
        {
            _fileBlocksQueue.Enqueue(e);
        }
    }
}
