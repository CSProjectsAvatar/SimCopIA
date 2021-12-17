using System.Collections.Generic;
using DataClassHierarchy;

namespace Compiler {
    internal class LetVarUnt : Unterminal
    {

        // <let-var> := "let" ID "=" <expr>
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation)
        {
            string id = (derivation[1] as Token).Lexem;
            var expr = (derivation[3] as Unterminal).Ast as Expression;
            return new LetVar(){
                Identifier = id,
                Expr = expr
            };
            
        }
    }
}