using DataClassHierarchy;
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
    public class Lr1 : IDisposable {
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
        /// el nodo raíz del AST.
        /// </summary>
        /// <param name="tokens">El último token debe ser <see cref="Token.TypeEnum.Eof"/>.</param>
        /// <returns></returns>
        public bool TryParse(IEnumerable<Token> tokens, out AstNode root) {
            root = null;
            var state = 0u;
            var history = new Stack<(GramSymbol Symbol, uint State)>();  // (g, s): utilic el si'mbolo g pa moverme del estado s
            var toksTor = tokens.GetEnumerator();
            toksTor.MoveNext();

            if (!TryGetAction(state, toksTor.Current, out var action, out var actData)) {
                SyntaxError(toksTor.Current);
                return false;
            }
            while (action != Lr1Dfa.ActionEnum.Ok) {
                switch (action) {
                    case Lr1Dfa.ActionEnum.Shift:
                        history.Push((toksTor.Current, state));  // guardando estado actual
                        toksTor.MoveNext();  // consumiendo el token
                        state = actData;  // en este caso actData tiene el nuevo estado
                        break;

                    case Lr1Dfa.ActionEnum.Reduce:
                        if (!TryReduce(actData, history, out state)) {  // en este caso actData tiene la produccio'n q c reduce
                            SyntaxError(toksTor.Current);
                            return false;
                        }
                        break;

                    case Lr1Dfa.ActionEnum.ShiftReduce or Lr1Dfa.ActionEnum.ReduceReduce:
                        SyntaxError(toksTor.Current);
                        return false;

                    default:
                        throw new NotImplementedException();
                }
                if (!TryGetAction(state, toksTor.Current, out action, out actData)) {
                    SyntaxError(toksTor.Current);
                    return false;
                }
            }
            var (symbol, _) = history.Pop();  // no-terminal inicial (this.gram.Initial)
            root = (symbol as FakeS).Ast;
            return true;
        }

        /// <summary>
        /// Trata de obtener la acción que se debe aplicar cuando el autómata se encuentra en el estado <paramref name="state"/>
        /// y viene el token <paramref name="current"/>.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="current"></param>
        /// <param name="action"></param>
        /// <param name="actionData"></param>
        /// <returns></returns>
        private bool TryGetAction(uint state, Token current, out Lr1Dfa.ActionEnum action, out uint actionData) {
            try {
                (action, actionData) = this.dfa.Action(state, current.Type);
                return true;

            } catch (KeyNotFoundException) {
                if (current.Type == Token.TypeEnum.Eof) {
                    this.log?.LogError("Unexpected end of file.");
                }
                action = default;
                actionData = default;
                return false;
            }
        }

        private void SyntaxError(Token token) {
            this.log?.LogError("Syntax error in line {line}, column {col}.",  // @todo MEJORA ESTE MENSAG 
                token.Line,
                token.Column);
        }

        /// <summary>
        /// Trata de hacer la operación REDUCE y devuelve el nuevo estado.
        /// de sintaxis.
        /// </summary>
        /// <param name="productionId"></param>
        /// <param name="history"></param>
        /// <returns></returns>
        internal bool TryReduce(uint productionId, Stack<(GramSymbol Symbol, uint State)> history, out uint newState) {
            var prod = this.gram.Productions.First(p => p.Id == productionId);
            IReadOnlyList<GramSymbol> derivants = PopDerivants(history, prod.Derivation.Count, out var topState);
            Unterminal unterminal = Unterminal.FromReduction(prod.Unterminal, derivants);

            try {
                newState = this.dfa.Goto(topState, unterminal.GetType());

            } catch (KeyNotFoundException) {
                newState = default;
                return false;
            }
            history.Push((unterminal, topState));

            return true;
        }

        /// <summary>
        /// Extrae de la pila <paramref name="count"/> elementos y los devuelve en el orden inverso
        /// a como fueron extraídos.
        /// </summary>
        /// <param name="history"></param>
        /// <param name="count"></param>
        /// <param name="currentState">Estado en el que se encuentra el autómata al sacar de la pila los 
        /// símbolos.</param>
        /// <returns></returns>
        private IReadOnlyList<GramSymbol> PopDerivants(
                Stack<(GramSymbol Symbol, uint State)> history, 
                int count,
                out uint currentState) {

            GramSymbol[] res = new GramSymbol[count];

            for (int i = count-1; i > 0; i--) {
                res[i] = history.Pop().Symbol;
            }
            (res[0], currentState) = history.Pop();

            return res;
        }

        public void Dispose() {
            gram.Dispose();
        }
    }
}
