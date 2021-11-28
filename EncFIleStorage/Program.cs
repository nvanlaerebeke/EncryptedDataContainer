using System;
using System.Text;

namespace EncFIleStorage
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var sampleInputFile = "/home/nvanlaerebeke/sample.txt";
            var testOutputFile = "/home/nvanlaerebeke/test";
            
            var f = new File(testOutputFile);
            Write(f, sampleInputFile);
            //Read(f);
            _ = Console.ReadKey();
        }

        private static void Read(File file)
        {
            file.Open();
            var str = file.ReadAllText();
            Console.WriteLine(str);
        }

        private static void Write(File f, string inputFile)
        {
            f.Delete();
            f.Create();
            f.OpenWrite();
            var content = System.IO.File.ReadAllText(inputFile);
            f.Write(Encoding.UTF8.GetBytes(content), 0);
        }
    }
}