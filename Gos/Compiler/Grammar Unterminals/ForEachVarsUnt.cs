using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class ForEachVarsUnt : Unterminal {
        public IReadOnlyList<string> Vars { get; private set; }

        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            /*
             <foreach-vars> := ID
                             | ID "," ID
             */
            var vars = new List<string>();
            vars.Add((derivation[0] as Token).Lexem);

            if (derivation.Count == 3) {
                vars.Add((derivation[2] as Token).Lexem);
            }
            Vars = vars;
            return null;
        }
    }
}
