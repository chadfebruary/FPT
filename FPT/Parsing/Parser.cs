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

        private FPTQueryModel _queryModel;
        private MatchCondition _currentMatchCondition;

        private const string expectedObjectErrorText = "Expected =, !=, LIKE, NOT LIKE, IN or NOT IN but found: ";

        public FPTQueryModel Parse(List<FPTToken> tokens)
        {
            LoadSequenceStack(tokens);
            PrepareLookAheads();
            _queryModel = new FPTQueryModel();

            Match();

            DiscardToken(TokenType.SequenceTerminator);

            return _queryModel;
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
                //else if (lookAheadSecond.tokenType == TokenType.In)
                //{
                //    InCondition();
                //}
                //else if (lookAheadSecond.tokenType == TokenType.NotIn)
                //{
                //    NotInCondition();
                //}
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
            _currentMatchCondition.Object = GetObject(lookAheadFirst);
            DiscardToken();
            _currentMatchCondition.Operator = GetOperator(lookAheadFirst);
            DiscardToken();
            _currentMatchCondition.Value = lookAheadFirst.value;
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
                case TokenType.Like:
                    return FPTOperator.Like;
                case TokenType.NotLike:
                    return FPTOperator.NotLike;
                case TokenType.In:
                    return FPTOperator.In;
                case TokenType.NotIn:
                    return FPTOperator.NotIn;
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
            _currentMatchCondition.Values.Add(ReadToken(TokenType.StringValue).value);
            DiscardToken(TokenType.StringValue);
            StringLiteralListNext();
        }

        private void StringLiteralListNext()
        {
            if (lookAheadFirst.tokenType == TokenType.Comma)
            {
                DiscardToken(TokenType.Comma);
                _currentMatchCondition.Values.Add(ReadToken(TokenType.StringValue).value);
                DiscardToken(TokenType.StringValue);
                StringLiteralListNext();
            }
        }

        private void MatchConditionNext()
        {
            if (lookAheadFirst.tokenType == TokenType.And)
                AndMatchCondition();
            else if (lookAheadFirst.tokenType == TokenType.Or)
                OrMatchCondition();
            //else if (_lookAheadFirst.tokenType == TokenType.Between)
            //    DateCondition();
            else
                throw new System.Exception();
        }

        private void AndMatchCondition()
        {
            _currentMatchCondition.LogOpToNextCondition = FPTLogicalOperator.And;
            DiscardToken(TokenType.And);
            MatchCondition();
        }

        private void OrMatchCondition()
        {
            _currentMatchCondition.LogOpToNextCondition = FPTLogicalOperator.Or;
            DiscardToken(TokenType.Or);
            MatchCondition();
        }

        private void Limit()
        {
            DiscardToken(TokenType.Limit);
            int limit = 0;
            bool success = int.TryParse(ReadToken(TokenType.Number).value, out limit);

            if (success)
                _queryModel.limit = limit;
            else
                throw new Exception();

            DiscardToken(TokenType.Number);
        }

        //private bool IsObject(FPTToken token)
        //{
        //    return token.tokenType == TokenType.Application
        //        || token.tokenType == TokenType.ExceptionType
        //        || token.tokenType == TokenType.FingerPrint
        //        || token.tokenType == TokenType.Message
        //        || token.tokenType == TokenType.StackFrame;
        //}

        private bool IsEqualityOperator(FPTToken token)
        {
            return token.tokenType == TokenType.Equals
                || token.tokenType == TokenType.NotEquals
                || token.tokenType == TokenType.Like
                || token.tokenType == TokenType.NotLike;
        }

        private void CreateNewMatchCondition()
        {
            _currentMatchCondition = new DataRepresentation.MatchCondition();
            _queryModel.matchConditions.Add(_currentMatchCondition);
        }
    }

    public enum FPTOperator
    {
        Equals,
        NotEquals,
        Like,
        NotLike,
        In,
        NotIn
    }

    public enum FPTLogicalOperator
    {
        And,
        Or
    }
}
