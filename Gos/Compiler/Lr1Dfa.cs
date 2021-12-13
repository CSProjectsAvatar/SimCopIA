using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Compiler {
    internal class Lr1Dfa {
        private Dictionary<(uint, Token.TypeEnum), (ActionEnum, uint)> action;  // (estado, token) -> (accio'n, data)
        private Dictionary<(uint, string), uint> @goto;  // (estado, no-terminal) -> nuevo estado
        private readonly Grammar grammar;
        private Dictionary<string, ICollection<Token.TypeEnum>> first;
        private Dictionary<string, bool> derivesEps;

        public Lr1Dfa(Grammar grammar) {
            CalcEpsilonDerivations();
            CalcFirsts();

            this.action = new();
            this.@goto = new();
            this.grammar = grammar;
            var toProcess = new Queue<Lr1State>();
            var states = new HashSet<Lr1State>();  // para no analizar 2 veces el mismo estado

            // considerando estado inicial
            Lr1State initState = Closure(Lr1Item.Initial(grammar.Initial));
            toProcess.Enqueue(initState);  // encolando estado inicial para procesarlo
            states.Add(initState);  // anyadie'ndolo al conjunto de estados
            this.action[(initState.Id, Token.TypeEnum.Eof)] = (ActionEnum.Ok, default);  // ACTION[I0, $] = 'OK'

            var count = 1u;  // contador de estados

            while (toProcess.Count != 0) {
                var curState = toProcess.Dequeue();

                foreach (var group in curState.GroupBy(i => i.NextSymbol)) {  // agrupando por el si'mbolo q viene despue's del punto
                    if (group.Key == default) {  // X -> α., s
                        SetReduceActions(curState);
                        continue;
                    }
                    var @goto = Closure(from i in @group where i.CanMoveDot select i.MoveDot());  // Goto(curState, group.Key) (conf 12, diapo 21)
                    
                    if (!states.Contains(@goto)) {
                        ProcessNewState(toProcess, states, ref count, curState, group.Key, @goto);
                    }
                }
            }
        }

        /// <summary>
        /// Determina cuáles no-terminales derivan en épsilon y almacena los resultados en 
        /// <see cref="derivesEps"/>.
        /// </summary>
        private void CalcEpsilonDerivations() {
            this.derivesEps = new();
            bool change;

            void SetVal(string unterminal, bool value) {
                change = true;
                this.derivesEps[unterminal] = value;
            }
            do {
                change = false;
                foreach (var prod in this.grammar.Productions
                        .Where(p => !this.derivesEps.ContainsKey(p.Unterminal.Name))) {  // producciones de no terminales en los cuales una decisio'n no ha si2 tomada
                    if (IsToken(Helper.SymbolTypeToStr(prod.Derivation[0]), out var token)
                            && token == Token.TypeEnum.Epsilon) {  // X -> eps
                        SetVal(prod.Unterminal.Name, true);
                    } else {
                        var derives = prod.Derivation.Aggregate(  // 😎: trata d determinar si deriva en eps d acuer2 a la derivacio'n
                            (bool?)true,
                            (accum, unter) => accum switch {
                                true => this.derivesEps.ContainsKey(unter.Name)
                                    ? this.derivesEps[unter.Name]
                                    : null,  // no conozco si el si'mbolo deriva a no en eps
                                _ => accum  // propago el 1er false o null q m encuentre desp d un true
                            }
                        ); // es true si todos los si'mbolos derivan en eps, null si el ma's a la izq q no deriva en eps no c conoce, y false en otro caso
                        if (derives.HasValue) {
                            SetVal(prod.Unterminal.Name, derives.Value);
                        }
                    }
                }
            } while (change);
        }

        /// <summary>
        /// Computa el First de todos los no-terminales y almacena los resultados en 
        /// <see cref="first"/>.
        /// </summary>
        private void CalcFirsts() {
            var firsts = new Dictionary<string, HashSet<Token.TypeEnum>>();
            bool change;

            void EnsureSubset(HashSet<Token.TypeEnum> subset, HashSet<Token.TypeEnum> superset) {
                if (!subset.IsSubsetOf(superset)) {
                    change = true;
                    superset.UnionWith(subset);
                }
            }
            do {
                change = false;

                foreach (var group in this.grammar.Productions.GroupBy(p => p.Unterminal.Name)) {
                    if (DerivesEpsilon(group.Key)) {  // X ->* eps
                        EnsureSubset(
                            new HashSet<Token.TypeEnum>(new[] { Token.TypeEnum.Epsilon }),
                            firsts[group.Key]);  // eps ∈ First(X)
                    }
                    foreach (var prod in group) {  // por cada Wi tq X -> Wi
                        CalcFirst(prod.Derivation, firsts, group.Key, EnsureSubset);
                    }
                }
            } while (change);

            this.first = firsts.ToDictionary(kv => kv.Key, kv => kv.Value as ICollection<Token.TypeEnum>);
        }

        /// <summary>
        /// Aplica las reglas necesarias para calcular el First de una forma oracional.
        /// </summary>
        /// <param name="sentence"></param>
        /// <param name="firsts"></param>
        /// <param name="unterminal">El derivante de la forma oracional <paramref name="sentence"/>.</param>
        /// <param name="ensureSubset"></param>
        private void CalcFirst(
                IReadOnlyList<Type> sentence, 
                Dictionary<string, HashSet<Token.TypeEnum>> firsts, 
                string unterminal, 
                Action<HashSet<Token.TypeEnum>, HashSet<Token.TypeEnum>> ensureSubset) {

            for (int i = 0; i < sentence.Count; i++) {
                if (IsToken(Helper.SymbolTypeToStr(sentence[i]), out var token)) {  // Wi = xZ
                    ensureSubset(
                        new HashSet<Token.TypeEnum>(new[] { token }),
                        firsts[unterminal]);  // x = First(Wi) ⊆ First(X)
                    break;
                } else {  // Wi = YZ
                    ensureSubset(firsts[sentence[i].Name], firsts[unterminal]);  // First(Y) ⊆ First(Wi) ⊆ First(X)
                
                    if (!DerivesEpsilon(sentence[i].Name)) {
                        break;
                    }
                }
            }
        }

        private Lr1State Closure(Lr1Item lr1Item) {
            return Closure(new[] { lr1Item });
        }

        private Lr1State Closure(IEnumerable<Lr1Item> items) {
            var q = new Queue<Lr1Item>(items);
            var set = new HashSet<Lr1Item>(items);

            while (q.Count != 0) {
                var item = q.Dequeue();

                if (item.CanMoveDot && !IsToken(item.NextSymbol, out _)) {  // Y -> α.Xδ, c
                    var delta = item.Production.Derivation.Skip((int)item.Dot + 1);
                    var first = First(delta);

                    if (DerivesEpsilon(delta)) {
                        first.Add(item.Lookahead);
                    }
                    var prods = from p in this.grammar.Productions
                                where p.Unterminal.Name == item.NextSymbol
                                select p;  // producciones de item.NextSymbol

                    foreach (var terminal in first.Where(t => t != Token.TypeEnum.Epsilon)) {  // b ∈ First(δc)     @remind HAY Q PREGUNTAR SI HAY Q CONSIDERAR EL CASO b = epsilon
                        foreach (var newItem in prods
                                .Select(p => new Lr1Item(p, terminal, 0))  // X -> .β, b
                                .Where(i => !set.Contains(i))) {  // no considerar las q ya consideramos
                            q.Enqueue(newItem);
                        }
                    }
                }
            }
            return new Lr1State(set);
        }

        /// <summary>
        /// Determina si la forma oracional dada deriva en épsilon.
        /// </summary>
        /// <param name="delta"></param>
        /// <returns></returns>
        private bool DerivesEpsilon(IEnumerable<Type> delta) {
            foreach (var t in delta) {
                if (!IsToken(t.Name, out _) && !DerivesEpsilon(t.Name)) {
                    return false;
                }
            }
            return true;
        }

        private ICollection<Token.TypeEnum> First(IEnumerable<Type> symbols) {
            var res = new List<Token.TypeEnum>();

            foreach (var s in symbols) {
                if (IsToken(Helper.SymbolTypeToStr(s), out var token)) {
                    res.Add(token);
                    return res;
                }
                res.AddRange(this.first[s.Name]);

                if (!DerivesEpsilon(s.Name)) {
                    return res;
                }
            }
            return res;
        }

        /// <summary>
        /// Determina si el no-terminal dado deriva en épsilon, directa o indirectamente.
        /// </summary>
        /// <param name="unterminal"></param>
        /// <returns></returns>
        private bool DerivesEpsilon(string unterminal) {
            return this.derivesEps[unterminal];
        }

        /// <summary>
        /// Realiza las operaciones necesarias sobre el nuevo estado <paramref name="newState"/>.
        /// </summary>
        /// <param name="toProcess"></param>
        /// <param name="states">Conjunto de estados.</param>
        /// <param name="count">Cantidad de estados encontrados.</param>
        /// <param name="curState">Estado actual.</param>
        /// <param name="transitionSymbol">Símbolo por el cual se llega de 
        /// <paramref name="curState"/> a <paramref name="newState"/>.</param>
        /// <param name="newState">Nuevo estado.</param>
        private void ProcessNewState(
                Queue<Lr1State> toProcess, 
                HashSet<Lr1State> states, 
                ref uint count, 
                Lr1State curState, 
                string transitionSymbol, 
                Lr1State newState) {
            newState.Id = count++;

            if (IsToken(transitionSymbol, out var token)) {  // X -> α.cω, s y Goto(curState, c) = @goto.Id
                SetAction(curState.Id, token, ActionEnum.Shift, newState.Id);  // ACTION[curState, c] = shift pa @goto.Id
            } else {  // X → α.Yω, s y Goto(curState, Y) = @goto.Id
                this.@goto[(curState.Id, transitionSymbol)] = newState.Id;
            }
            states.Add(newState);
            toProcess.Enqueue(newState);  // procesar luego el estado nuevo
        }

        /// <summary>
        /// Registra la acción y se encarga de chequear la existencia de conflictos.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="token"></param>
        /// <param name="action"></param>
        /// <param name="data"></param>
        private void SetAction(uint state, Token.TypeEnum token, ActionEnum action, uint data) {
            if (!this.action.TryGetValue((state, token), out var old)) {
                this.action[(state, token)] = (action, data);
            } else if (old.Item1!=ActionEnum.ShiftReduce && old.Item1!=ActionEnum.ReduceReduce) {
                switch (action) {
                    case ActionEnum.Shift when old.Item1 == ActionEnum.Reduce:  // conflicto shift-reduce
                        this.action[(state, token)] = (ActionEnum.ShiftReduce, default);
                        break;
                    case ActionEnum.Reduce when old.Item1 == ActionEnum.Shift:  // conflicto shift-reduce
                        this.action[(state, token)] = (ActionEnum.ShiftReduce, default);
                        break;
                    case ActionEnum.Reduce when old.Item1 == ActionEnum.Reduce:  // conflicto reduce-reduce
                        this.action[(state, token)] = (ActionEnum.ReduceReduce, default);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private bool IsToken(string grammarSymbol, out Token.TypeEnum token) {
            return Enum.TryParse(grammarSymbol, out token);
        }

        /// <summary>
        /// Va x cada item de la forma <code>X -> α., s</code> y coloca en la tabla la acción REDUCE
        /// correspondiente.
        /// </summary>
        /// <param name="curState"></param>
        private void SetReduceActions(Lr1State curState) {
            foreach (var (lookAh, prodId) in curState.Select(i => (i.Lookahead, i.Production.Id))) {
                SetAction(curState.Id, lookAh, ActionEnum.Reduce, prodId);  // ACTION[curState, lookAh] = Rk
            }
        }

        internal enum ActionEnum {
            Ok,
            Shift,
            Reduce,

            // conflictos
            ShiftReduce,
            ReduceReduce
        }

        internal (ActionEnum Action, uint Data) Action(uint state, Token.TypeEnum token) {
            return this.action[(state, token)];
        }

        internal uint Goto(uint state, Type unterminal) {
            return this.@goto[(state, unterminal.Name)];
        }
    }
}