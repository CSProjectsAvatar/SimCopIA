

namespace DataClassHierarchy
{
    public class MultOp:BinaryExpr
    {
        public MultOp(Expression left, Expression right):base(left, right){ }

        public override int Eval(){
            return Left.Eval() * Right.Eval();
        }
    }
}
    