using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Lexer.AstHierarchy {
    /// <summary>
    /// Visita los AST de REGEX para construir un autómata que sea capaz de reconocer cadenas de caracteres que cumplan con una
    /// expresión regular.
    /// </summary>
    internal class NfaBuilderVisitor : Visitor<Automaton> {
        internal Automaton Visiting(BaseRegexAst charAst) {
            throw new NotImplementedException($"{nameof(Visiting)} method not found.");  // se busco' x un me'todo Visiting en la jerarqui'a y no c encontro'
        }

        internal Automaton Visiting(CharAst charAst) {
            return Automaton.FromChar(charAst.Value);
        }

        internal Automaton Visiting(ClosureAst closureAst) {
            return Visit(closureAst.Target)
                .Mult();
        }

        internal Automaton Visiting(ConcatAst concatAst) {
            return Visit(concatAst.Left)
                .Concat(Visit(concatAst.Right));
        }

        internal Automaton Visiting(PositClosureAst positClosureAst) {
            return Visit(positClosureAst.Target)
                .Plus();
        }

        internal Automaton Visiting(RangeAst rangeAst) {
            var count = rangeAst.Right - rangeAst.Left + 1;
            var first = Automaton.FromChar(rangeAst.Left);

            if (count == 1) {
                return first;
            }
            return Enumerable.Range(rangeAst.Left + 1, count - 1)  // se omite el 1er elemento, ya q c cog d semilla en el Aggregate
                .Aggregate(first, (accum, x) => accum.Union(Automaton.FromChar((char)x)));  // unio'n d los auto'matas d cada char
        }

        internal Automaton Visiting(SetAst setAst) {
            var first = Visit(setAst.Items[0]);

            if (setAst.Items.Count == 1) {
                return first;
            }
            return setAst.Items
                .Skip(1)  // para evitar modelar un auto'mata vaci'o q c ponga de semilla en el Aggregate
                .Aggregate(first, (accum, x) => accum.Union(Visit(x)));  // la unio'n d todos los auto'matas
        }

        internal Automaton Visiting(UnionAst unionAst) {
            return Visit(unionAst.Left)
                .Union(Visit(unionAst.Right));
        }

        internal Automaton Visiting(ZeroOnceAst zeroOnceAst) {
            return Visit(zeroOnceAst.Target)
                .Maybe();
        }
    }
}
