using Compiler.AstHierarchy.Statements;
using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class InitUnt : Unterminal {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            // <init> := "init" "{" <stat-list> "}"
            return new InitAst(Helper.Logger<InitAst>()) {
                Token = derivation[0] as Token,
                Code = (derivation[^2] as StatListUnt).Statements
            };
        }
    }
}
