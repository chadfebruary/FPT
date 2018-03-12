using FPT.DataRepresentation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPT.Parsing
{
    public class Parser
    {
        private Stack<FPTToken> _tokenSequence;
        private FPTToken _lookAheadFirst;
        private FPTToken _lookAheadSecond;

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
            _tokenSequence = new Stack<FPTToken>();
            int count = tokens.Count;

            for (int i = count - 1; i >= 0; i--)
            {
                _tokenSequence.Push(tokens[i]);
            }
        }

        private void PrepareLookAheads()
        {
            _lookAheadFirst = _tokenSequence.Pop();
            _lookAheadSecond = _tokenSequence.Pop();
        }

        private FPTToken ReadToken(TokenType tokenType)
        {
            if (_lookAheadFirst.tokenType != tokenType)
                throw new System.Exception();

            return _lookAheadFirst;
        }

        private void DiscardToken()
        {
            _lookAheadFirst = _lookAheadSecond.Clone();

            if (_tokenSequence.Any())
                _lookAheadSecond = _tokenSequence.Pop();
            else
                _lookAheadSecond = new FPTToken(TokenType.SequenceTerminator, string.Empty);
        }

        private void DiscardToken(TokenType tokenType)
        {
            if (_lookAheadFirst.tokenType != tokenType)
                throw new System.Exception();

            DiscardToken();
        }

        private void Match()
        {
            DiscardToken(TokenType.Match);
            MatchCondition();
        }

        private void MatchCondition()
        {
            CreateNewMatchCondition();

            //if (IsObject(_lookAheadFirst))
            //{
                if (IsEqualityOperator(_lookAheadSecond))
                {
                    EqualityMatchCondition();
                }
                else if (_lookAheadSecond.tokenType == TokenType.In)
                {
                    InCondition();
                }
                else if (_lookAheadSecond.tokenType == TokenType.NotIn)
                {
                    NotInCondition();
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
            _currentMatchCondition.Object = GetObject(_lookAheadFirst);
            DiscardToken();
            _currentMatchCondition.Operator = GetOperator(_lookAheadFirst);
            DiscardToken();
            _currentMatchCondition.Value = _lookAheadFirst.value;
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

        private void NotInCondition()
        {
            ParseInCondition(FPTOperator.NotIn);
        }

        private void InCondition()
        {
            ParseInCondition(FPTOperator.In);
        }

        private void ParseInCondition(FPTOperator inOperator)
        {
            _currentMatchCondition.Operator = inOperator;
            _currentMatchCondition.Values = new List<string>();
            _currentMatchCondition.Object = GetObject(_lookAheadFirst);
            DiscardToken();

            if (inOperator == FPTOperator.In)
                DiscardToken(TokenType.In);
            else if (inOperator == FPTOperator.NotIn)
                DiscardToken(TokenType.NotIn);

            DiscardToken(TokenType.OpenParenthesis);
            StringLiteralList();
            DiscardToken(TokenType.CloseParenthesis);
        }

        private void StringLiteralList()
        {
            _currentMatchCondition.Values.Add(ReadToken(TokenType.StringValue).value);
            DiscardToken(TokenType.StringValue);
            StringLiteralListNext();
        }

        private void StringLiteralListNext()
        {
            if (_lookAheadFirst.tokenType == TokenType.Comma)
            {
                DiscardToken(TokenType.Comma);
                _currentMatchCondition.Values.Add(ReadToken(TokenType.StringValue).value);
                DiscardToken(TokenType.StringValue);
                StringLiteralListNext();
            }
        }

        private void MatchConditionNext()
        {
            if (_lookAheadFirst.tokenType == TokenType.And)
                AndMatchCondition();
            else if (_lookAheadFirst.tokenType == TokenType.Or)
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

        //private void DateCondition()
        //{
        //    DiscardToken(TokenType.Between);

        //    _queryModel.dateRange = new DateRange();
        //    _queryModel.dateRange.from = DateTime.ParseExact();
        //}

        //private void DateConditionNext()
        //{

        //}

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
