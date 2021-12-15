using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler {
    /// <summary>
    /// Parser LR(1).
    /// </summary>
    public class Lr1 {
        private readonly Lr1Dfa dfa;
        private readonly Grammar gram;
        private readonly ILogger<Lr1> log;

        public Lr1(Grammar grammar, ILogger<Lr1> logger, ILogger<Lr1Dfa> dfaLogger) {
            this.dfa = new Lr1Dfa(grammar, dfaLogger);
            this.gram = grammar;
            this.log = logger;
        }

        /// <summary>
        /// Devuelve si se puede parsear <paramref name="tokens"/> y en <paramref name="root"/> se devuelve 
        /// el node raíz del AST.
        /// </summary>
        /// <param name="tokens">El último token debe ser <see cref="Token.TypeEnum.Eof"/>.</param>
        /// <returns></returns>
        public bool TryParse(IEnumerable<Token> tokens, out AstNode root) {
            root = null;
            var state = 0u;
            var history = new Stack<(GramSymbol Symbol, uint State)>();
            var toksTor = tokens.GetEnumerator();
            toksTor.MoveNext();

            var (action, actData) = this.dfa.Action(state, toksTor.Current.Type);

            while (action != Lr1Dfa.ActionEnum.Ok) {
                if (toksTor.Current.Type == Token.TypeEnum.Eof) {
                    this.log.LogError("Unexpected end of file.");
                    return false;
                }
                switch (action) {
                    case Lr1Dfa.ActionEnum.Shift:
                        history.Push((toksTor.Current, state));  // guardando estado actual
                        toksTor.MoveNext();  // consumiendo el token
                        state = actData;  // en este caso actData tiene el nuevo estado
                        break;

                    case Lr1Dfa.ActionEnum.Reduce:
                        state = Reduce(actData, history);  // en este caso actData tiene la produccio'n q c reduce
                        break;

                    case Lr1Dfa.ActionEnum.ShiftReduce or Lr1Dfa.ActionEnum.ReduceReduce:
                        this.log.LogError("Syntax error in line {line}, column {col}.",  // @todo MEJORA ESTE MENSAG 
                            toksTor.Current.Line, 
                            toksTor.Current.Column);
                        return false;

                    default:
                        throw new NotImplementedException();
                }
                (action, actData) = this.dfa.Action(state, toksTor.Current.Type);
            }
            var (symbol, _) = history.Pop();  // no-terminal inicial (this.gram.Initial)
            root = (symbol as Unterminal).Ast;
            return true;
        }

        /// <summary>
        /// Hace la operación REDUCE y devuelve el nuevo estado.
        /// </summary>
        /// <param name="productionId"></param>
        /// <param name="history"></param>
        /// <returns></returns>
        private uint Reduce(uint productionId, Stack<(GramSymbol Symbol, uint State)> history) {
            var prod = this.gram.Productions[(int)productionId];
            IReadOnlyList<GramSymbol> derivants = PopDerivants(history, prod.Derivation.Count);
            Unterminal unterminal = Unterminal.FromReduction(prod.Unterminal, derivants);

            var (_, topState) = history.Peek();
            uint nextState = this.dfa.Goto(topState, unterminal.GetType());
            history.Push((unterminal, nextState));

            return nextState;
        }

        /// <summary>
        /// Extrae de la pila <paramref name="count"/> elementos y los devuelve en el orden inverso
        /// a como fueron extraídos.
        /// </summary>
        /// <param name="history"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private IReadOnlyList<GramSymbol> PopDerivants(
                Stack<(GramSymbol Symbol, uint State)> history, 
                int count) {

            GramSymbol[] res = new GramSymbol[count];

            for (int i = count-1; i >= 0; i++) {
                res[i] = history.Pop().Symbol;
            }
            return res;
        }
    }
}
