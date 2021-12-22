using Compiler.Lexer.AstHierarchy;
using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Compiler.Lexer {
    public class ItemListUnt : Unterminal {
        public IReadOnlyList<BaseRegexAst> Items { get; private set; }

        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            /*
             <item-list> := <item> 
                          | <item> <item-list>
            */
            IEnumerable<BaseRegexAst> items = new[] { (derivation[0] as ItemUnt).Ast as BaseRegexAst };
            if (derivation.Count == 2) {
                var itemsTail = (derivation[1] as ItemListUnt).Items;
                items = items.Concat(itemsTail);
            } else if (derivation.Count != 1) {
                throw new ArgumentException("Too many symbols.", nameof(derivation));
            }
            Items = items.ToList();
            return null;
        }
    }
}