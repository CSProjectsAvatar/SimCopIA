using System.Collections.Generic;
using System.Linq;
using DataClassHierarchy;

namespace Compiler {
    internal class IdListUnt : Unterminal
    {
        public IEnumerable<string> Ids { get; set; }

        //<arg-list> := ID | ID "," <arg-list>
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation)
        {
            Ids = new [] { (derivation[0] as Token).Lexem };
            if(derivation.Count > 1){
                var argList = (derivation[2] as IdListUnt).Ids;
                Ids = Ids.Concat(argList);
            }
            return null;
        }
    }
}