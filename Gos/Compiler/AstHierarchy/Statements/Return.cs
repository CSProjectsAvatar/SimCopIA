
namespace DataClassHierarchy
{
    public class Return:AstNode, IStatement
    {
        public Expression Expr { get; set; }

        public override bool Validate(Context context)
        {
            return Expr.Validate(context);
        }
    }
}