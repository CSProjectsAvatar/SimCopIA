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
            switch(derivation[0]){
                case IfUnt ifUnt:
                    return ifUnt.Ast;
                case DefFuncUnt defFuncUnt:
                    return defFuncUnt.Ast;
                default:
                    throw new System.Exception("BlockStUnt: unexpected symbol");
            }
        }
    }
}