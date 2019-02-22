using FPT.DataRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using static FPT.Parsing.OperatorFactory;

namespace FPT.Parsing
{
    public class Parser
    {
        /*private Stack<FPTToken> tokenSequence;
        private FPTToken lookAheadFirst;
        private FPTToken lookAheadSecond;

        private FPTQueryModel queryModel;
        private MatchCondition currentMatchCondition;*/

        private const string expectedObjectErrorText = "Expected =, !=, LIKE, NOT LIKE, IN or NOT IN but found: ";

        private Dictionary<string, int> mathOperators = new Dictionary<string, int>() { { "+", 1 }, { "-", 2 }, { "*", 3 }, { "/", 4 } };
        private Dictionary<string, int> invalidPreceedingOperators = new Dictionary<string, int>() { { "%", 9 }, { "|", 6 }, { "&", 8 }, { ">>", 7 }, { "<<", 6 }, { "+", 4 }, { "/", 3 }, { "*", 2 }, { "^", 1 } };
        protected IDictionary<string, decimal> symbols;
        private UniLeafFactory uniLeafFact = new UniLeafFactory();
        private OperatorFactory operFact = new OperatorFactory();
        public List<string> Variables { get; set; }

        //public FPTQueryModel Parse(List<FPTToken> tokens)
        //{
        //    /*LoadSequenceStack(tokens);
        //    PrepareLookAheads();
        //    queryModel = new FPTQueryModel();

        //    Match();

        //    DiscardToken(TokenType.SequenceTerminator);*/

        //    return null;
        //}
        public IMathNode Parse(string expression)
        {
            this.Variables = new List<string>();
            return MathParse(expression);
        }

        public IMathNode MathParse(string code)
        {
            if (string.IsNullOrEmpty(code))
                throw new Exception("No code found.");

            code.Trim();

            code = code.Replace("plus", "+");
            code = code.Replace("kunye", "+");
            code = code.Replace("minus", "-");
            code = code.Replace("nciphisa", "-");
            code = code.Replace("multiply", "*");
            code = code.Replace("phinda", "*");
            code = code.Replace("divide", "/");
            code = code.Replace("ukwahlula", "/");

            int startPosition = 0;
            int parenthesis = 0;

            if (code[0] == '-')
                startPosition++;

            int lastValidChar = -1;

            int operatorValue;
            int lowestOperator = -1;
            int operatorPosition = -1;
            int operatorLength = 0;
            string symbol = string.Empty;

            for (int i = code.Length - 1; i >= startPosition; i--)
            {
                char temp = code[i];

                if (code[i] == ')')
                {
                    lastValidChar = i;
                    parenthesis++;
                }
                else if (code[i] == '(')
                {
                    lastValidChar = i;
                    parenthesis--;
                }

                if (parenthesis < 0)
                {
                    throw new Exception("Format error.");
                }

                string searchItem = code[i].ToString();

                //Change to <
                if (searchItem == "<" || searchItem == ">")
                {
                    searchItem = string.Concat(code[i - 1].ToString(), code[i].ToString());
                    i--;
                }

                if (!mathOperators.ContainsKey(searchItem))
                {
                    if (!Char.IsWhiteSpace(code[i]))
                    {
                        lastValidChar = i;
                    }
                    continue;
                }

                if (invalidPreceedingOperators.ContainsKey(searchItem) && mathOperators.ContainsKey(code[i - 1].ToString()))
                    throw new Exception("Invalid operator.");

                operatorValue = mathOperators[searchItem];

                if ((parenthesis != 0 || lowestOperator >= operatorValue) && code[i + 1] != '-')
                    continue;

                lowestOperator = operatorValue;
                operatorPosition = i;
                operatorLength = searchItem.Length - 1;
                symbol = searchItem;
            }

            if (parenthesis != 0)
                throw new Exception("Invalid expression.");

            if (operatorPosition == -1)
            {
                decimal d;

                if ((code[0] != '0' || code.Length == 1 || code[1] == '0' || code[1] == '.') && decimal.TryParse(code.Trim(), out d))
                {
                    return new NumericMathNode(d);
                }
                else
                {
                    var node = uniLeafFact.CreateUniLeafNode(code.Trim(), this);
                    if (node == null)
                    {
                        decimal value;

                        if (symbols.TryGetValue(code.Trim(), out value))
                        {
                            node = new NumericMathNode(value);
                        }
                        else
                        {
                            UnitFactory factory = new UnitFactory();
                            var result = factory.TryParse(code, this);

                            if (result != null)
                            {
                                return result;
                            }

                            if (code[0] == '-')
                            {
                                return new MultiplicationBiLeafMathNode(new NumericMathNode(-1), this.Parse(code.Substring(1)));
                            }
                            else if (code[0] == '~')
                            {
                                return new NegateUniLeafMathNode(this.Parse(code.Substring(1)));
                            }
                            else if (code[0] == '"' && code.Last() == '"')
                            {
                                return new StringMathNode(code.Substring(1, code.Length - 2));
                            }

                            throw new Exception("");
                        }
                    }

                    return node;
                }
            }
            else
            {
                string left = String.Empty;

                if (operatorPosition - 1 > 0)
                {
                    left = code.Substring(0, operatorPosition - operatorLength);
                }
                else
                {
                    left = code[0].ToString();
                }

                string right = code.Substring(operatorPosition + 1 + operatorLength, code.Length - operatorPosition - 1 - operatorLength);

                return operFact.CreateOperatorNode(symbol, this.Parse(left), this.Parse(right));
            }
        }
    }
}