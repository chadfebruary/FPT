using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPT.Parsing
{
    public class FPTToken
    {
        public TokenType tokenType { get; set; }
        public string value { get; set; }
        public FPTToken(TokenType tokenType)
        {
            this.tokenType = tokenType;
            value = string.Empty;
        }
        public FPTToken(TokenType tokenType, string value)
        {
            this.tokenType = tokenType;
            this.value = value;
        }
        public FPTToken Clone()
        {
            return new FPTToken(this.tokenType, this.value);
        }
    }
}
