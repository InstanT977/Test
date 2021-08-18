using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RwmSignatureGenerator.Interfaces
{
    public interface ILogger
    {
        void Info(string message);
        void Error(Exception exception);
        void Error(string message);
    }
}
