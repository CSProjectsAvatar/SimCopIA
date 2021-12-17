using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class ExprListUnt : Unterminal {
        public IEnumerable<Expression> Expressions { get; private set; }

        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            /*
             <expr-list> := <expr>
                          | <expr> "," <expr-list>
             */
            Expressions = new[] { ((Unterminal)derivation[0]).Ast as Expression };

            if (derivation.Count != 1) {
                var insideExprs = ((ExprListUnt)derivation[2]).Expressions;
                Expressions = Expressions.Concat(insideExprs);
            }
            return null;
        }
    }
}
