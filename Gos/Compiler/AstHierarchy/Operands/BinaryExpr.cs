

using System;

namespace DataClassHierarchy
{
    public enum Operator{
        Add,
        Sub,
        Mult,
        Div
    }
    public abstract class BinaryExpr:Expression
    {
        public Expression Left { get; set; }
        public Expression Right { get; set; }

        public BinaryExpr(){ }
        public BinaryExpr(Expression left, Expression right)
        {
            Left = left;
            Right = right;
        }

        [Obsolete]
        public virtual (bool, object) TryCompute(object left, object right) {  // @todo pa borrar
            throw new InvalidOperationException();
        }

        public override bool Validate(Context context){
            return Left.Validate(context) && Right.Validate(context);
        }
    }
}