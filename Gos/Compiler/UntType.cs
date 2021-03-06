using System;

namespace Compiler {
    /// <summary>
    /// Representa el tipo de un no-terminal.
    /// </summary>
    public class UntType : GramSymType {
        private readonly Type unterminal;
        public readonly static UntType E = new UntType(typeof(FakeE));
        public readonly static UntType F = new UntType(typeof(FakeF));
        public readonly static UntType T = new UntType(typeof(FakeT));
        public readonly static UntType X = new UntType(typeof(FakeX));
        public readonly static UntType Y = new UntType(typeof(FakeY));
        public readonly static UntType S = new UntType(typeof(FakeS));
        public readonly static UntType Prog = new UntType(typeof(ProgramUnt));

        public UntType(Type unterminal) {
            if (!unterminal.Inherits(typeof(Unterminal))) {
                throw new ArgumentException(
                    $"Provided type must inherits {nameof(Unterminal)}.",
                    nameof(unterminal));
            }
            this.unterminal = unterminal;
        }

        public override string Name => unterminal.Name;

        public override string ToString() {
            return this.unterminal.Name;
        }

        public static explicit operator Type(UntType @this) {
            return @this.unterminal;
        }

        // sobrecargando operadores > para construir producciones

        public static Production operator >(UntType @this, (GramSymType, GramSymType) symbols) {
            return new Production(@this, symbols.Item1, symbols.Item2);
        }

        public static Production operator >(UntType @this, (GramSymType, GramSymType, GramSymType) symbols) {
            return new Production(@this, symbols.Item1, symbols.Item2, symbols.Item3);
        }

        public static Production operator >(UntType @this, (GramSymType, GramSymType, GramSymType, GramSymType) symbols) {
            return new Production(@this, symbols.Item1, symbols.Item2, symbols.Item3, symbols.Item4);
        }
        public static Production operator >(UntType @this, (GramSymType, GramSymType, GramSymType, GramSymType, GramSymType) symbols) {
            return new Production(@this, symbols.Item1, symbols.Item2, symbols.Item3, symbols.Item4, symbols.Item5);
        }

        public static Production operator <(UntType @this, (GramSymType, GramSymType, GramSymType, GramSymType) symbols) {
            return new Production(@this, symbols.Item1, symbols.Item2, symbols.Item3, symbols.Item4);
        }
        public static Production operator <(UntType @this, (GramSymType, GramSymType, GramSymType, GramSymType, GramSymType) symbols) {
            return new Production(@this, symbols.Item1, symbols.Item2, symbols.Item3, symbols.Item4, symbols.Item5);
        }

        public static Production operator >(
                UntType @this,
                (GramSymType, GramSymType, GramSymType, GramSymType, GramSymType, GramSymType, GramSymType) symbols) {
            return new Production(@this, symbols.Item1, symbols.Item2, symbols.Item3, symbols.Item4, symbols.Item5, symbols.Item6, symbols.Item7);
        }

        public static Production operator <(
                UntType @this,
                (GramSymType, GramSymType, GramSymType, GramSymType, GramSymType, GramSymType, GramSymType) symbols) {
            throw new NotImplementedException();
        }

        public static Production operator >(
                UntType @this, 
                (GramSymType, GramSymType, GramSymType, GramSymType, GramSymType, GramSymType, GramSymType, GramSymType) symbols) {
            return new Production(@this, symbols.Item1, symbols.Item2, symbols.Item3, symbols.Item4, symbols.Item5, symbols.Item6, symbols.Item7, symbols.Item8);
        }

        public static Production operator <(
                UntType @this,
                (GramSymType, GramSymType, GramSymType, GramSymType, GramSymType, GramSymType, GramSymType, GramSymType) symbols) {
            throw new NotImplementedException();
        }

        public static Production operator >(UntType @this, GramSymType symbol) {
            return new Production(@this, symbol);
        }

        // @audit Revisa esto, me molestaba y comente el metodo para que lo coja de la clase padre
        
        // public static Production operator -(UntType @this, GramSymType symbol) {
        //     return new Production(@this, symbol);
        // }


        public static Production operator <(UntType _, GramSymType __) {
            throw new NotImplementedException();
        }

        public static Production operator <(UntType _, (GramSymType, GramSymType) __) {
            throw new NotImplementedException();
        }

        public static Production operator <(UntType _, (GramSymType, GramSymType, GramSymType) __) {
            throw new NotImplementedException();
        }
    }
}