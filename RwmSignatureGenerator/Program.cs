using RwmSignatureGenerator.Data.HashGenerators;
using RwmSignatureGenerator.Data.Loggers;
using RwmSignatureGenerator.Interfaces;
using System;

namespace RwmSignatureGenerator
{
    class Program
    {
        static ILogger logger = new ConsoleLogger();
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;
            if (args.Length < 2)
            {
                throw new Exception("Incorrect arguments count to launch application!");
            }

            var filePath = args[0];//@"C:\Users\RWHite\Downloads";
            int blockSize;
            var isCorrectBlockSize = Int32.TryParse(args[1], out blockSize);
            if(!isCorrectBlockSize)
            {
                throw new Exception("Incorrect block size argument!");
            }
            var sGen = new FileSignatureGenerator(new Sha56HashGenerator(), logger);
            sGen.GenerateSignature(filePath, blockSize);
            WaitForExit();
        }

        static void WaitForExit()
        {
            Console.WriteLine("Please press any key for exit...");
            Console.ReadKey();
        }

        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            logger?.Error(e.ExceptionObject as Exception);
            WaitForExit();
            Environment.Exit(1);
        }
    }
}
