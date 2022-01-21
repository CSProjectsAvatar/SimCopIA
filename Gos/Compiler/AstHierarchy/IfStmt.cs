

using System.Collections.Generic;
namespace DataClassHierarchy
{
    public class IfStmt:Expression, IStatement
    {
        public Expression Condition { get; set; }
        public List<IStatement> Then { get; set; }
        public override bool Validate(Context context)
        {
            if (!Condition.Validate(context))
                return false;
            foreach (var statement in Then)
            {
                if (!statement.Validate(context))
                    return false;
            }
            return true;
        }
    }
}