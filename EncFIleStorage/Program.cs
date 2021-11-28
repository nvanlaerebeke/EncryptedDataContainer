using System;
using System.Text;
using EncFIleStorage.Data;

namespace EncFIleStorage
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            var sampleInputFile = "/home/nvanlaerebeke/sample_large.txt";
            //var sampleInputFile = "/home/nvanlaerebeke/sample_small.txt";
            var testOutputFile = "/home/nvanlaerebeke/test";

            using (var f = new File<AesDataTransformer>(testOutputFile))
            {
                Write(f, sampleInputFile);
            }

            using (var f = new File<AesDataTransformer>(testOutputFile))
            {
                Read(f);
            }
            //_ = Console.ReadKey();
        }

        private static void Read(IFile file)
        {
            file.Open();
            var str = file.ReadAllText();
            Console.WriteLine(str);
        }

        private static void Write(IFile f, string inputFile)
        {
            f.Delete();
            f.Create();
            f.OpenWrite();
            var content = System.IO.File.ReadAllText(inputFile);
            f.Write(Encoding.UTF8.GetBytes(content), 0);
        }
    }
}