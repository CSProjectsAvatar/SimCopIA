
namespace DataClassHierarchy
{
    public class LetVar:AstNode, IStatement
    {
        public string Identifier { get; set; }
        public Expression Expr { get; set; }

        
        public override bool Validate(Context context)
        {
            return Expr.Validate(context) && context.DefVariable(Identifier);
        }
    }
}