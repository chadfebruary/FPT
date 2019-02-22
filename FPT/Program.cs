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
                //ITokenizer tokenizer = new Tokenizer();
                Parser parser = new Parser();
                List<FPTToken> tokens = new List<FPTToken>();

                Console.WriteLine("Enter code homie:");
                string userInput = Console.ReadLine();
                var found = userInput.Contains("plus");



                IMathNode result = parser.MathParse(userInput);
                Console.WriteLine(result.Evaluate());

            }
        }
    }
}
