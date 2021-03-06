

using System.Collections.Generic;
namespace DataClassHierarchy {
    public class Print:AstNode, IStatement {
        public Expression Expr { get; set; }

        public override bool Validate(Context context) {
            return Expr.Validate(context);
        }
    }
}