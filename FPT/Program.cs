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
                        tokens.Add(new FPTToken(TokenType.Match, "1"));
                        tokens.Add(new FPTToken(TokenType.Add, "+"));
                        tokens.Add(new FPTToken(TokenType.Subtract, "1"));

                        Console.WriteLine("Enter code homie:");
                        string userInput = Console.ReadLine();
                        //string[] userInputList = userInput.Split(null);

                        //parser.Parse(tokens);
                        parser.MathParse(userInput);
                        break;
                    default: break;
                }
            }
        }
    }
}
