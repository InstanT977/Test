using RwmSignatureGenerator.Data.Common;
using RwmSignatureGenerator.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RwmSignatureGenerator.Data
{
    internal class FileReader
    {
        internal event EventHandler<FileDataBlock> FileBlockRecieved;
        internal event EventHandler ReadingEnded;
        internal long FileBlocksCount { private set; get; }

        private FileInfo _fileInfo;
        private int _blockSize;
        private ILogger _logger;
        internal FileReader(string filePath, int blockSize, ILogger logger)
        {
            _logger = logger;
            _blockSize = blockSize;
            _fileInfo = new FileInfo(filePath);
            if (!_fileInfo.Exists)
            {
                throw new FileNotFoundException($"File for reading not found: {filePath}");
            }
            if(_blockSize > _fileInfo.Length)
            {
                throw new Exception("Too high block size for selected file!");
            }
            FileBlocksCount = (long)Math.Floor(_fileInfo.Length / (double)_blockSize);
        }



        internal void Read()
        {
            byte[] buffer = null;
            int blockNumber = 0;
            var blocks = FileBlocksCount;
            int lastBytesCount = (int)(_fileInfo.Length - (blocks * _blockSize));
            long offsetFile = 0;
            using (FileStream fs = new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
                buffer = new byte[_blockSize];
                using (BinaryReader br = new BinaryReader(fs))
                {
                    while (blocks > 0)
                    {
                        blocks -= 1;
                        fs.Seek(offsetFile, SeekOrigin.Begin);
                        buffer = br.ReadBytes(_blockSize);
                        FileBlockRecieved?.Invoke(this, new FileDataBlock(buffer, blockNumber));
                        offsetFile += _blockSize;
                        blockNumber++;
                    }
                    if (lastBytesCount > 0)
                    {
                        Array.Resize(ref buffer, lastBytesCount);
                        fs.Seek(offsetFile, SeekOrigin.Begin);
                        buffer = br.ReadBytes(lastBytesCount);
                        FileBlockRecieved?.Invoke(this, new FileDataBlock(buffer, blockNumber));
                    }
                    buffer = null;
                }
            }

        }
    }
}
