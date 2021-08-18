using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RwmSignatureGenerator.Data.Common
{
    internal class FileDataBlock
    {
        internal byte[] Data { get; private set; }
        internal int Number { get; private set; }
        internal FileDataBlock(byte[] data, int number)
        {
            Data = data;
            Number = number;
        }
    }
}
