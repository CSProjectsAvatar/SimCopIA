using Compiler.AstHierarchy.Statements;
using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class ForEachUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            // <foreach> := "for" <foreach-vars> "in" <expr> "{" <stat-list> "}"
            IReadOnlyList<string> vars = (derivation[1] as ForEachVarsUnt).Vars;

            return new ForEachAst {
                Token = derivation[0] as Token,
                Item = vars[^1],
                Index = vars.Count == 2 ? vars[0] : default,
                Iterable = (derivation[3] as ExpressionUnt).Ast as Expression,
                Code = (derivation[^2] as StatListUnt).Statements
            };
        }
    }
}
