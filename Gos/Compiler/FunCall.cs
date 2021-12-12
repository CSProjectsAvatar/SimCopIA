

using System.Collections.Generic;
namespace DataClassHierarchy
{
    public class FunCall:Expression
    {
        public string Identifier { get; set; }
        public List<Expression> Args { get; set; }

        public override (bool Success, object Result) TryEval()
        {
            throw new System.NotImplementedException();
        }

        public override bool Validate(Context context)
        {
            foreach (var expr in Args)
            {
                if (!expr.Validate(context))
                    return false;
            }
            return context.CheckFunc(Identifier, Args.Count);
        }
    }
}