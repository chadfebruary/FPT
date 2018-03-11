using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPT.DataRepresentation
{
    public class FPTQueryModel
    {
        public DateRange dateRange { get; set; }
        public int? limit { get; set; }
        public IList<MatchCondition> matchConditions { get; set; }
        public FPTQueryModel()
        {
            matchConditions = new List<MatchCondition>();
        }
    }
}
