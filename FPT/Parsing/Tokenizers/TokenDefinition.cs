using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FPT.Parsing
{
    public class TokenDefinition
    {
        private Regex regex;
        private readonly TokenType returnsToken;

        public TokenDefinition(TokenType returnsToken, string regexPattern)
        {
            regex = new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            this.returnsToken = returnsToken;
        }
        public IEnumerable<TokenMatch> FindMatches(string inputString)
        {
            var matches = regex.Matches(inputString);
            for (int i = 0; i < matches.Count; i++)
            {
                yield return new TokenMatch()
                {
                    StartIndex = matches[i].Index,
                    EndIndex = matches[i].Index + matches[i].Length,
                    TokenType = returnsToken,
                    Value = matches[i].Value
                };
            }
        }
    }
}
