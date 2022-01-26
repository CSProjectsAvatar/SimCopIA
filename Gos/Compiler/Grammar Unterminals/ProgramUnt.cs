using System.Collections.Generic;
using System.Linq;
using Compiler.Grammar_Unterminals;
using DataClassHierarchy;

namespace Compiler {
    internal class ProgramUnt : Unterminal
    {
        //<program> := <stat-list>
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation)
        {
            return new ProgramNode {
                Statements = (derivation[0] as StatListUnt).Statements.ToList()
            };
        }
    }
}