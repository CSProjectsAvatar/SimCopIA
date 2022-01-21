using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Compiler {
    public struct Lr1Item {
        private uint dot;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="production"></param>
        /// <param name="lookahead"></param>
        /// <param name="dot">Índice para el punto. Puede ir desde 0 hasta 
        /// <paramref name="production"/>.Derivation.Count.
        /// <para>Por ejemplo, si <paramref name="dot"/> = 0, entonces</para><code>X -> .αω</code>
        /// mientras que si <paramref name="dot"/> = 2, entonces<code>X -> αω.</code></param>
        public Lr1Item(Production production, Token.TypeEnum lookahead, uint dot) {
            if (dot > production.Derivation.Count) {
                throw new ArgumentException("Index for dot is out of range.", nameof(dot));
            }
            Production = production;
            Lookahead = lookahead;
            this.dot = dot;
        }

        /// <summary>
        /// <para>Devuelve el <see cref="string"/> que identifica al símbolo que se encuentra luego del
        /// punto.</para>
        /// Por ejemplo, si tenemos el item <code>X -> α.Yω, c</code> entonces <see cref="NextSymbol"/>
        /// = "Y".
        /// </summary>
        public string NextSymbol {
            get {
                if (dot < Production.Derivation.Count) {
                    return Helper.SymbolTypeToStr(Production.Derivation[(int)dot]);
                }
                return default;
            }
        }

        public Token.TypeEnum Lookahead { get; private set; }
        public Production Production { get; private set; }
        public bool CanMoveDot => dot < Production.Derivation.Count;
        public uint Dot => dot;

        internal Lr1Item MoveDot() {
            if (!CanMoveDot) {
                throw new InvalidOperationException();
            }
            var res = this;
            res.dot++;  // @remind ASUMIENDO Q ESTE TIPO ES TRATA2 X VALOR
            return res;
        }

        /// <summary>
        /// Devuelve <code>S' -> .S, $</code> donde S es <paramref name="initialUnterminal"/> y S'
        /// es un nuevo símbolo.
        /// </summary>
        /// <param name="initialUnterminal"></param>
        /// <returns></returns>
        internal static Lr1Item Initial(UntType initialUnterminal) {
            return new Lr1Item(
                production: new Production(UntType.S, initialUnterminal),  // S' -> S
                lookahead: Token.TypeEnum.Eof,  // $
                dot: 0u
            );
        }

        public static implicit operator Lr1Item((Production, int, Token.TypeEnum) data) {
            return new Lr1Item(data.Item1, data.Item3, (uint)data.Item2);
        }

        public override string ToString() {
            var regex = new Regex(@"\w+");
            var prodStr = this.Production.ToString();
            var matches = regex.Matches(prodStr);
            var dotIdx = CanMoveDot
                ? matches[(int)this.dot + 1].Index
                : prodStr.Length;

            return $"{prodStr.Insert(dotIdx, ".")}, {this.Lookahead}";
        }
    }
}