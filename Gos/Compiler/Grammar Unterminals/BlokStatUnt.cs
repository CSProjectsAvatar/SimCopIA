using System.Collections.Generic;
using System.Linq;
using DataClassHierarchy;

namespace Compiler {
    internal class BlockStUnt : Unterminal
    {
        public IEnumerable<string> Args { get; set; }

        //<block-stat> := <if> | <def-func>
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation)
        {
            return derivation[0] switch{
                IfUnt ifUnt => ifUnt.Ast,
                DefFunUnt defFuncUnt => defFuncUnt.Ast,
                _ => throw new System.Exception("BlockStUnt.SetAst: grammar symbol no esperado")
            };
        
        }
    }
}