

namespace DataClassHierarchy
{
    public class SubOp:BinaryExpr
    {
        public SubOp(Expression left, Expression right):base(left, right){ }

        public override int Eval(){
            return Left.Eval() - Right.Eval();
        }
    }
}
    