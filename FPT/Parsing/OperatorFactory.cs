using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPT.Parsing
{
    internal class OperatorFactory
    {
        /// <summary>
        /// Create an operator node based on a character
        /// </summary>
        /// <param name="c">The operator being + - * / or ^</param>
        /// <param name="left">The left part of the operator</param>
        /// <param name="right">The right part of the operator</param>
        /// <returns>Valid operators return IMathNode, null if no operator</returns>
        public IMathNode CreateOperatorNode(string c, IMathNode left, IMathNode right)
        {
            switch (c)
            {
                case "+":
                    return new AdditionBiLeafMathNode(left, right);
                case "-":
                    return new SubtractionBiLeafMathNode(left, right);
                case "*":
                    return new MultiplicationBiLeafMathNode(left, right);
                case "/":
                    return new DivisionBiLeafMathNode(left, right);
                default:
                    return null;
            }
        }

        public abstract class BiLeafMathNode : IMathNode
        {
            public UnitTypes UnitType { get; set; }
            /// <summary>
            /// Get the value of this node
            /// </summary>
            public object Value { get { return this.value; } }

            /// <summary>
            /// Get the left node on the binary tree.
            /// </summary>
            public IMathNode Left { get { return this.left; } }

            /// <summary>
            /// Get the right node on the binary tree.
            /// </summary>
            public IMathNode Right { get { return this.right; } }

            public BiLeafMathNode(IMathNode left, IMathNode right, object value)
            {
                this.left = left;
                this.right = right;
                this.value = value;
            }

            /// <summary>
            /// Perform evaluation of its leaves recursively
            /// </summary>
            /// <returns>The calculated value</returns>
            public abstract UnitDouble Evaluate();

            /// <summary>
            /// Get the left node on the binary tree.
            /// </summary>
            protected IMathNode left;

            /// <summary>
            /// Get the right node on the binary tree.
            /// </summary>
            protected IMathNode right;

            /// <summary>
            /// Stores the value of this node
            /// </summary>
            protected object value;
        }

        public abstract class SymbolMathNode : BiLeafMathNode
        {
            public SymbolMathNode(IMathNode left, IMathNode right, char symbol)
                : base(left, right, symbol)
            {
            }
            public SymbolMathNode(IMathNode left, IMathNode right, string symbol)
                : base(left, right, symbol)
            {
            }

            public override string ToString()
            {
                return "(" + left.ToString() + " " + Value + " " + right.ToString() + ")";
            }

        }

        public class NegateUniLeafMathNode : UniLeafMathNode
        {
            public IMathNode Inner { get; private set; }

            public NegateUniLeafMathNode(IMathNode inner)
                : base(null)
            {
                this.Inner = inner;
            }

            /// <summary>
            /// Perform evaluation of its leaves recursively
            /// </summary>
            /// <returns>The calculated value</returns>
            public override UnitDouble Evaluate()
            {
                if (null == this.Inner) throw new Exception("Bitwise Negate needs a number");

                var tmp = this.Inner.Evaluate();
                return ~tmp;
            }

            public override string ToString()
            {
                return "~(" + this.Inner.ToString() + ")";
            }
        }

        public class MultiplicationBiLeafMathNode : SymbolMathNode
        {
            public MultiplicationBiLeafMathNode(IMathNode left, IMathNode right)
                : base(left, right, '*')
            {
            }

            public override UnitDouble Evaluate()
            {
                if (null == this.left || null == this.right) throw new Exception("One or more required arguments are empty");

                return left.Evaluate() * right.Evaluate();
            }
        }

        public class AdditionBiLeafMathNode : SymbolMathNode
        {
            public AdditionBiLeafMathNode(IMathNode left, IMathNode right)
                : base(left, right, '+')
            {
            }

            public override UnitDouble Evaluate()
            {
                if (null == this.left || null == this.right) throw new Exception("One or more required arguments are empty");

                return left.Evaluate() + right.Evaluate();
            }
        }

        public class DivisionBiLeafMathNode : SymbolMathNode
        {
            public DivisionBiLeafMathNode(IMathNode left, IMathNode right)
                : base(left, right, '/')
            {
            }

            public override UnitDouble Evaluate()
            {
                if (null == this.left || null == this.right) throw new Exception("One or more required arguments are empty");

                var denom = right.Evaluate();

                if (0 == denom.Value)
                {
                    // For sake of debugging in the future, I want to track MathParser thrown exceptions
                    // vs system thrown ones. Will help tell between a flaw vs a bug
                    throw new Exception("Divide by zero");
                }

                return left.Evaluate() / right.Evaluate();
            }
        }

        public class SubtractionBiLeafMathNode : SymbolMathNode
        {
            public SubtractionBiLeafMathNode(IMathNode left, IMathNode right)
                : base(left, right, '-')
            {
            }

            public override UnitDouble Evaluate()
            {
                if (null == this.left || null == this.right) throw new Exception("One or more required arguments are empty");

                return left.Evaluate() - right.Evaluate();
            }
        }
    }
}
