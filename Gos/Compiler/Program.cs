using System.Collections.Generic;
namespace DataClassHierarchy
{
    public class Program:AstNode
    {
        public List<Statement>  Statements { get; set; }

        
        public override bool Validate(Context context)
        {
            foreach (var st in Statements)
            {
                if(!st.Validate(context))
                    return false;
            }
            return true;
        }
    }
}
