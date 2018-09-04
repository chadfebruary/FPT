using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FPT.Parsing
{
    public class UnitFactory
    {
        public UnitFactory()
        {
        }
        public UnitFactory(string exchangeUrl, string cachePath)
        {
            this.converters = new UnitConverter[] {
            };
        }

        // Breaks up the expression into either a conversion of (expr) to unit or (exp) unit or unit(exp) with whitespace optional.
        private static Regex unitParser = new Regex
        (
            @"^(?:(?<value>.+)\s+(?:to|as|in)\s+(?<to>[A-z$€£฿]+)|(?<value>(?!\s(?:to|as)\s).+?)\s*(?<from>[A-z$€£฿""']+)|(?<from>[$€£฿])\s*(?<value>(?!\s(?:to|as)\s).+?))$"
        );
        // ""'
        private UnitConverter[] converters;

        public IMathNode TryParse(string expression, Parser parser)
        {
            //var possiblyhex = this.isSpeicalFormattedNumber(expression);
            //if (possiblyhex != null)
            //{
            //    return possiblyhex;
            //}

            var results = unitParser.Match(expression);

            if (false == results.Success)
            {
                return null;
            }

            var expr = results.Groups["value"];
            var to = results.Groups["to"];
            var from = results.Groups["from"];
            Group actionable;


            if (expr.Value == "\"" && from.Value.Last() == '"')
            {
                return null;
            }

            // Continue parsing the left side of this Unit declaration
            // If this is a conversion, this will come back here until there
            // are no more units left to parse through

            // Check for " " to indicate a string literal.
            //if (expr.Value.Contains('"'))
            //{
            //    // Otherwise, we are parsing a complex expression
            //    var valu = parser.Parse(expr.Value);
            //}
            //else
            //{
            //    // Otherwise, we are parsing a complex expression
            //    var valu = parser.Parse(expr.Value);
            //}

            // Otherwise, we are parsing a complex expression
            var valu = parser.Parse(expr.Value);


            // If we are finally inside the expression of expr units
            if (from.Success)
            {
                actionable = from;
            }
            else
            {
                actionable = to;
            }

            UnitConverter converter = this.GetUnitConverter(actionable.Value);

            if (converter == null)
            {
                if (actionable.Value.ToLower() != "string" && actionable.Value.ToLower() != "str")
                {
                    throw new Exception(actionable.Value);
                }

                // Convert the int/long/hex value into a string/char.
                return new StringMathNode(valu);
            }


            // Determines things like DistanceImperical, DistanceMetric, etc
            Enum tmpEnum;
            converter.GetUnitFromString(actionable.Value, out tmpEnum);
            var hold = tmpEnum.GetAttributeOfType<UnitTypeAttribute>().FirstOrDefault();

            if (hold != null)
            {
                //valu.UnitType = hold.UnitType;
            }

            return new UnitUniLeafMathNode(valu, (hold == null ? UnitTypes.None : hold.UnitType), converter, tmpEnum);

        }

        /*private IMathNode isSpeicalFormattedNumber(string expression)
        {
            expression = expression.Trim();
            var nbc = new NumericBaseConverter();

            // Probably formatted via 0x hex? Check for hex input
            if (expression[0] == '0' && (expression[1] == 'x' || expression[1] == 'X'))
            {
                try
                {
                    string number = expression.Substring(2);
                    var val = (decimal)Int64.Parse(number, System.Globalization.NumberStyles.HexNumber);
                    var inner = new NumericMathNode(new UnitDouble(val, UnitTypes.Hexadecimal, NumericBaseUnits.Hexadecimal, nbc)) { UnitType = UnitTypes.Hexadecimal };
                    return new UnitUniLeafMathNode(inner, UnitTypes.Hexadecimal, nbc, NumericBaseUnits.Hexadecimal);
                }
                catch { }

            }
            if (expression[0] == '0' && (expression[1] == 'b' || expression[1] == 'B'))
            {
                try
                {
                    string number = expression.Substring(2);
                    var val = (decimal)Convert.ToInt64(number, 2);
                    var inner = new NumericMathNode(new UnitDouble(val, UnitTypes.Binary, NumericBaseUnits.Binary, nbc)) { UnitType = UnitTypes.Binary };
                    return new UnitUniLeafMathNode(inner, UnitTypes.Binary, nbc, NumericBaseUnits.Binary);
                }
                catch { }

            }
            if (expression[0] == '0')
            {
                try
                {
                    var val = (decimal)Convert.ToInt64(expression, 8);
                    var inner = new NumericMathNode(new UnitDouble(val, UnitTypes.Octal, NumericBaseUnits.Octal, nbc)) { UnitType = UnitTypes.Octal };
                    return new UnitUniLeafMathNode(inner, UnitTypes.Octal, nbc, NumericBaseUnits.Octal);
                }
                catch { }
            }

            return null;

        }*/

        public UnitConverter GetUnitConverter(string unit)
        {
            Enum tmp;
            return converters.Where(p => p.GetUnitFromString(unit, out tmp)).FirstOrDefault();
        }
    }

    public class StringMathNode : UniLeafMathNode
    {
        public StringMathNode(IMathNode inner)
        {
            this.value = inner;
        }
        public StringMathNode(string inner)
        {
            this.value = inner;
        }

        public override UnitDouble Evaluate()
        {

            if (this.value is IMathNode)
            {
                return new StringFromDouble(((IMathNode)this.value).Evaluate());
            }
            else if (this.value is string)
            {
                var bytez = System.Text.Encoding.UTF8.GetBytes((string)this.value).Reverse().ToArray();
                byte[] bytez2 = new byte[sizeof(long)];

                //var offset = ;
                var offset = Math.Max(0, (bytez.Length - 1) - (bytez2.Length - 1));
                var end = Math.Min(bytez.Length - 1, bytez2.Length - 1);

                for (var i = 0; i <= end; i++)
                {
                    bytez2[i] = bytez[i + +offset];
                }

                long l = BitConverter.ToInt64(bytez2, 0);

                return new StringFromDouble(l);
            }

            // Shouldn't ever happen?
            throw new Exception("Invalid type for StringMathNode Value");

        }

        public override string ToString()
        {
            if (this.value is IMathNode)
            {
                return ((IMathNode)this.value).ToString();
            }
            else if (this.value is string)
            {
                return "\"" + this.value + "\"";
            }


            // Shouldn't ever happen?
            throw new Exception("Invalid type for StringMathNode Value");
        }
    }

    public class StringFromDouble : UnitDouble
    {
        public StringFromDouble(UnitDouble copy) : base(copy)
        {
        }

        public StringFromDouble(decimal value) : base(value)
        {
        }
        public StringFromDouble(long value) : base(value)
        {
        }

        public StringFromDouble(decimal value, UnitTypes unitType, Enum unit, UnitConverter convert) : base(value, unitType, unit, convert)
        {
        }


        public override string ToString()
        {
            //		Unit	Decimal	System.Enum {dab.Library.MathParser.NumericBaseUnits}
            if (this.Unit != null /*&& (NumericBaseUnits)this.Unit == NumericBaseUnits.Decimal && true == this.Reduce*/)
            {
                return base.ToString();
            }
            else
            {
                var bytes = BitConverter.GetBytes((long)this.Value).Reverse().ToArray();
                var val = System.Text.Encoding.UTF8.GetString(bytes).Replace("\0", "");
                return val;
            }

        }
    }
}
