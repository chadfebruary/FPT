using System;
using System.Collections.Generic;
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
                        //ITokenizer tokenizer = new Tokenizer();
                        Parser parser = new Parser();
                        List<FPTToken> tokens = new List<FPTToken>();
                        tokens.Add(new FPTToken(TokenType.Match, "fuck knows"));
                        tokens.Add(new FPTToken(TokenType.Add, "+"));
                        tokens.Add(new FPTToken(TokenType.Subtract, "-"));
                        parser.Parse(tokens);
                        break;
                    default: break;
                }
            }
        }
    }
}
