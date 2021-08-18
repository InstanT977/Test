using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RwmSignatureGenerator.Interfaces
{
    public interface IHashGenerator
    {
        string GenerateHash(byte[] data);
    }
}
