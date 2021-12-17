using System.Collections.Generic;
using System.Linq;
using DataClassHierarchy;

namespace Compiler {
    internal class ArgListUnt : Unterminal
    {
        public IEnumerable<string> Args { get; set; }

        //<arg-list> := ID | ID "," <arg-list>
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation)
        {
            Args = new List<string>();
            Args.Add((derivation[0] as Token).Lexem);
            if(derivation.Count > 1){
                var argList = (derivation[2] as ArgListUnt).Args;
                Args.Append(argList);
            }
            return null;
        }
    }
}