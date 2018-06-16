using System;
using FPT.Parsing;

namespace FPT
{
    class Program
    {
        static void Main(string[] args)
        {
            Program run = new Program();

            run.Run();
        }

        public void Run()
        {
            while (true)
            {
                Console.WriteLine("Press 1 to test parser.");
                var input = Console.ReadKey();

                switch (input.KeyChar)
                {
                    case '1':
                        ITokenizer tokenizer = new Tokenizer();
                        break;
                    default: break;
                }
            }
        }
    }
}
