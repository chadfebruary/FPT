using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPT.Parsing
{
    public interface ITokenizer
    {
        IEnumerable<FPTToken> Tokenize(string queryFPT);
    }
}
