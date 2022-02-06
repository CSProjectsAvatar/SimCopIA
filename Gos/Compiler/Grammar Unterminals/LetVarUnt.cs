using System.Collections.Generic;
using DataClassHierarchy;

namespace Compiler {
    internal class LetVarUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            // <let-var> := "let" ID "=" <expr>
            var t = derivation[1] as Token;
            string id = t.Lexem;
            var expr = (derivation[3] as ExpressionUnt).Ast as Expression;
            return new LetVar(Helper.Logger<LetVar>()) {
                Identifier = id,
                Expr = expr,
                Token = t
            };
        }
    }
}