
namespace DataClassHierarchy
{
    public class LetVar:Statement
    {
        public string Identifier { get; set; }
        public Expression Expr { get; set; }

        
        public override bool Validate(Context context)
        {
            return Expr.Validate(context) && context.DefVariable(Identifier);
        }
    }
}