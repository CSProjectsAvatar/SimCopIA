
namespace DataClassHierarchy
{
    public class Variable:Expression // @audit Checkear
    {
        public string Identifier { get; set; }


        public override (bool Success, object Result) TryEval()
        {
            throw new System.NotImplementedException();
        }

        public override bool Validate(Context context)
        {
            return context.CheckVar(Identifier);
        }
    }
}