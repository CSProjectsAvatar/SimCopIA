

namespace DataClassHierarchy
{
    public class AddOp:BinaryExpr
    {
        public AddOp(Expression left, Expression right):base(left, right){ }

        public override int Eval(){
            return Left.Eval() + Right.Eval();
        }
    }
}
    