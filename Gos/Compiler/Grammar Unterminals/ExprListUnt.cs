using System.Collections.Generic;
using System.Linq;
using DataClassHierarchy;

namespace Compiler {
    internal class ExprListUnt : Unterminal
    {
        public IEnumerable<Expression> Exprs { get; set; }
        // <expr-list> := <expr> | <expr> "," <expr-list>
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation)
        {
            Exprs = new [] { (derivation[0] as ExpressionUnt).Ast };
            if(derivation.Count > 1){
                var exprList = (derivation[2] as ExprListUnt).Exprs;
                Exprs = Exprs.Concat(exprList);
            }
            return null;
        }
    }
}