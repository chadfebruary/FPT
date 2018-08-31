using FPT.DataRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPT.Parsing
{
    public class Parser
    {
        private Stack<FPTToken> tokenSequence;
        private FPTToken lookAheadFirst;
        private FPTToken lookAheadSecond;

        private FPTQueryModel queryModel;
        private MatchCondition currentMatchCondition;

        private const string expectedObjectErrorText = "Expected =, !=, LIKE, NOT LIKE, IN or NOT IN but found: ";

        private Dictionary<string, int> mathOperators = new Dictionary<string, int>() { { "+", 1 }, { "-", 2 }, { "*", 3 }, { "/", 4 } };
        private Dictionary<string, int> invalidPreceedingOperators = new Dictionary<string, int>() { { "%", 9 }, { "|", 6 }, { "&", 8 }, { ">>", 7 }, { "<<", 6 }, { "+", 4 }, { "/", 3 }, { "*", 2 }, { "^", 1 } };

        public FPTQueryModel Parse(List<FPTToken> tokens)
        {
            LoadSequenceStack(tokens);
            PrepareLookAheads();
            queryModel = new FPTQueryModel();

            Match();

            DiscardToken(TokenType.SequenceTerminator);

            return queryModel;
        }

        public string MathParse(string code)
        {
            if (string.IsNullOrEmpty(code))
                return "No code found.";

            code.Trim();

            int startPosition = 0;
            int parenthesis = 0;

            if (code[0] == '-')
                startPosition++;

            int lastValidChar = -1;

            int operatorValue;
            int lowestOperator = -1;
            int operatorPosition = -1;
            int operatorLength = 0;
            string symbol;

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
                    return "Format error.";
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
                    return "Invalid operator.";

                operatorValue = mathOperators[searchItem];

                if ((parenthesis != 0 || lowestOperator >= operatorValue) && code[i + 1] != '-')
                    continue;

                lowestOperator = operatorValue;
                operatorPosition = i;
                operatorLength = searchItem.Length - 1;
                symbol = searchItem;
            }

            if (parenthesis != 0)
                return "Invalid expression.";

            if (operatorPosition == -1)
            {
                decimal d;

                if ((code[0] != '0' || code.Length == 1 || code[1] == '0' || code[1] == '.') && decimal.TryParse(code.Trim(), out d))
                {

                }
            }

            return "";
        }

        private void LoadSequenceStack(List<FPTToken> tokens)
        {
            tokenSequence = new Stack<FPTToken>();
            int count = tokens.Count;

            for (int i = count - 1; i >= 0; i--)
            {
                tokenSequence.Push(tokens[i]);
            }
        }

        private void PrepareLookAheads()
        {
            lookAheadFirst = tokenSequence.Pop();
            lookAheadSecond = tokenSequence.Pop();
        }

        private FPTToken ReadToken(TokenType tokenType)
        {
            if (lookAheadFirst.tokenType != tokenType)
                throw new System.Exception();

            return lookAheadFirst;
        }

        private void DiscardToken()
        {
            lookAheadFirst = lookAheadSecond.Clone();

            if (tokenSequence.Any())
                lookAheadSecond = tokenSequence.Pop();
            else
                lookAheadSecond = new FPTToken(TokenType.SequenceTerminator, string.Empty);
        }

        private void DiscardToken(TokenType tokenType)
        {
            if (lookAheadFirst.tokenType != tokenType)
                throw new System.Exception();

            DiscardToken();
        }

        private void Match()
        {
            DiscardToken(TokenType.Match);
            MatchCondition();
        }

        private void Match(TokenType x)
        {
            DiscardToken(TokenType.Match);
            MatchCondition();

            if (lookAheadFirst.tokenType == x)
                Consume();
            else
                throw new Exception("Expected: " + lookAheadFirst.value + " Found: " + x.ToString());
        }

        public void Consume()
        {
            lookAheadFirst = tokenSequence.Pop();
        }

        private void MatchCondition()
        {
            CreateNewMatchCondition();

            //if (IsObject(_lookAheadFirst))
            //{
                if (IsEqualityOperator(lookAheadSecond))
                {
                    EqualityMatchCondition();
                }
                else if (IsMathOperator(lookAheadSecond))
                {
                    //InCondition();
                }
                else
                {
                    throw new System.Exception();
                }

                MatchConditionNext();
            //}
            //else
            //{
            //    throw new System.Exception();
            //}
        }

        private void EqualityMatchCondition()
        {
            currentMatchCondition.Object = GetObject(lookAheadFirst);
            DiscardToken();
            currentMatchCondition.Operator = GetOperator(lookAheadFirst);
            DiscardToken();
            currentMatchCondition.Value = lookAheadFirst.value;
            DiscardToken();
        }

        private FPTObject GetObject(FPTToken token)
        {
            switch (token.tokenType)
            {
                case TokenType.Application:
                    return FPTObject.Application;
                case TokenType.ExceptionType:
                    return FPTObject.ExeptionType;
                case TokenType.FingerPrint:
                    return FPTObject.FingerPrint;
                case TokenType.Message:
                    return FPTObject.Message;
                case TokenType.StackFrame:
                    return FPTObject.StackFrame;
                default:
                    throw new System.Exception();
            }
        }

        private FPTOperator GetOperator(FPTToken token)
        {
            switch (token.tokenType)
            {
                case TokenType.Equals:
                    return FPTOperator.Equals;
                case TokenType.NotEquals:
                    return FPTOperator.NotEquals;
                case TokenType.Add:
                    return FPTOperator.Add;
                case TokenType.Subtract:
                    return FPTOperator.Subtract;
                case TokenType.Multiply:
                    return FPTOperator.Multiply;
                case TokenType.Divide:
                    return FPTOperator.Divide;
                //case TokenType.Like:
                //    return FPTOperator.Like;
                //case TokenType.NotLike:
                //    return FPTOperator.NotLike;
                //case TokenType.In:
                //    return FPTOperator.In;
                //case TokenType.NotIn:
                //    return FPTOperator.NotIn;
                default:
                    throw new System.Exception();
            }
        }

        //private void NotInCondition()
        //{
        //    ParseInCondition(FPTOperator.NotIn);
        //}

        //private void InCondition()
        //{
        //    ParseInCondition(FPTOperator.In);
        //}

        //private void ParseInCondition(FPTOperator inOperator)
        //{
        //    _currentMatchCondition.Operator = inOperator;
        //    _currentMatchCondition.Values = new List<string>();
        //    _currentMatchCondition.Object = GetObject(lookAheadFirst);
        //    DiscardToken();

        //    if (inOperator == FPTOperator.In)
        //        DiscardToken(TokenType.In);
        //    else if (inOperator == FPTOperator.NotIn)
        //        DiscardToken(TokenType.NotIn);

        //    DiscardToken(TokenType.OpenParenthesis);
        //    StringLiteralList();
        //    DiscardToken(TokenType.CloseParenthesis);
        //}

        private void StringLiteralList()
        {
            currentMatchCondition.Values.Add(ReadToken(TokenType.StringValue).value);
            DiscardToken(TokenType.StringValue);
            StringLiteralListNext();
        }

        private void StringLiteralListNext()
        {
            if (lookAheadFirst.tokenType == TokenType.Comma)
            {
                DiscardToken(TokenType.Comma);
                currentMatchCondition.Values.Add(ReadToken(TokenType.StringValue).value);
                DiscardToken(TokenType.StringValue);
                StringLiteralListNext();
            }
        }

        private void MatchConditionNext()
        {
            if (lookAheadFirst.tokenType == TokenType.Add)
                AddMatchCondition();
            else if (lookAheadFirst.tokenType == TokenType.Subtract)
                SubtractMatchCondition();
            //else if (_lookAheadFirst.tokenType == TokenType.Between)
            //    DateCondition();
            else
                throw new System.Exception();
        }

        private void AddMatchCondition()
        {
            currentMatchCondition.LogOpToNextCondition = FPTLogicalOperator.Add;
            DiscardToken(TokenType.Add);
            MatchCondition();
        }

        private void SubtractMatchCondition()
        {
            currentMatchCondition.LogOpToNextCondition = FPTLogicalOperator.Subtract;
            DiscardToken(TokenType.Subtract);
            MatchCondition();
        }

        //private void Limit()
        //{
        //    DiscardToken(TokenType.Limit);
        //    int limit = 0;
        //    bool success = int.TryParse(ReadToken(TokenType.Number).value, out limit);

        //    if (success)
        //        queryModel.limit = limit;
        //    else
        //        throw new Exception();

        //    DiscardToken(TokenType.Number);
        //}

        private bool IsObject(FPTToken token)
        {
            return token.tokenType == TokenType.Application
                || token.tokenType == TokenType.ExceptionType
                || token.tokenType == TokenType.FingerPrint
                || token.tokenType == TokenType.Message
                || token.tokenType == TokenType.StackFrame;
        }

        private bool IsEqualityOperator(FPTToken token)
        {
            return token.tokenType == TokenType.Equals
                || token.tokenType == TokenType.NotEquals;
        }

        private bool IsMathOperator(FPTToken token)
        {
            return token.tokenType == TokenType.Add
                || token.tokenType == TokenType.Subtract
                || token.tokenType == TokenType.Multiply
                || token.tokenType == TokenType.Divide;
        }

        private void CreateNewMatchCondition()
        {
            currentMatchCondition = new DataRepresentation.MatchCondition();
            queryModel.matchConditions.Add(currentMatchCondition);
        }
    }

    public enum FPTOperator
    {
        Equals,
        NotEquals,
        Add,
        Subtract,
        Multiply,
        Divide
    }

    public enum FPTLogicalOperator
    {
        And,
        Or,
        Add,
        Subtract
    }
}
