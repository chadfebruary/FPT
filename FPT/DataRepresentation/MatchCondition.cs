using FPT.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPT.DataRepresentation
{
    public class MatchCondition
    {
        public FPTObject Object { get; set; }
        //public FPTOperator Operator { get; set; }
        public string Value { get; set; }
        public List<string> Values { get; set; }
        //public FPTLogicalOperator LogOpToNextCondition { get; set; }
    }
}
