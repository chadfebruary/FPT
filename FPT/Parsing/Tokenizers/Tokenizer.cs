using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPT.Parsing
{
    public class Tokenizer : ITokenizer
    {
        private List<TokenDefinition> _tokenDefinitions;

        public Tokenizer()
        {
            _tokenDefinitions = new List<TokenDefinition>();

            _tokenDefinitions.Add(new TokenDefinition(TokenType.Add, "add", 1));
            _tokenDefinitions.Add(new TokenDefinition(TokenType.Subtract, "subtract", 1));
            _tokenDefinitions.Add(new TokenDefinition(TokenType.Multiply, "multiply", 1));
            _tokenDefinitions.Add(new TokenDefinition(TokenType.Divide, "divide", 1));
        }
        public IEnumerable<FPTToken> Tokenize(string errorMessage)
        {
            var tokenMatches = FindTokenMatches(errorMessage);
            var groupedByIndex = tokenMatches.GroupBy(x => x.StartIndex).OrderBy(x => x.Key).ToList();
            TokenMatch lastMatch = null;

            for (int i = 0; i < groupedByIndex.Count; i++)
            {
                var bestMatch = groupedByIndex[i].OrderBy(x => x.Precedence).First();

                if (lastMatch != null && bestMatch.StartIndex < lastMatch.EndIndex)
                    continue;

                yield return new FPTToken(bestMatch.TokenType, bestMatch.Value);

                lastMatch = bestMatch;
            }

            yield return new FPTToken(TokenType.SequenceTerminator);
        }

        private List<TokenMatch> FindTokenMatches(string errorMessage)
        {
            var tokenMatches = new List<TokenMatch>();

            foreach (var tokenDefiniton in _tokenDefinitions)
                tokenMatches.AddRange(tokenDefiniton.FindMatches(errorMessage).ToList());

            return tokenMatches;
        }
    }
}
