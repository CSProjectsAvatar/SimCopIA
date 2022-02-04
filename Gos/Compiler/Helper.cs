using DataClassHierarchy;
using Microsoft.Extensions.Logging;
using ServersWithLayers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler {
    public static class Helper {
        public static ILoggerFactory LogFact;  // @remind ASIGNAR ESTO
        internal static IEnumerable<(string Regex, Token.TypeEnum Token)> TokenWithRegexs = new[] {
            ("print", Token.TypeEnum.Print),
            ("if", Token.TypeEnum.If),
            ("else", Token.TypeEnum.Else),
            ("else_if", Token.TypeEnum.ElseIf),
            ("let", Token.TypeEnum.Let),
            ("fun", Token.TypeEnum.Fun),
            ("return", Token.TypeEnum.Return),
            ("new", Token.TypeEnum.New),
            ("server|request|response|alarm", Token.TypeEnum.Class),
            ("forever", Token.TypeEnum.Forever),
            ("break", Token.TypeEnum.Break),
            ("for", Token.TypeEnum.For),
            ("in", Token.TypeEnum.In),
            ("and", Token.TypeEnum.And),
            ("or", Token.TypeEnum.Or),
            ("behav", Token.TypeEnum.Behavior),
            ("init", Token.TypeEnum.InitBehav),
            ("respond_or_save", Token.TypeEnum.RespondOrSave),
            ("process", Token.TypeEnum.Process),
            ("respond", Token.TypeEnum.Respond),
            ("accept", Token.TypeEnum.Accept),
            ("is", Token.TypeEnum.Is),
            ("not", Token.TypeEnum.Not),

            ("[0-9]+(.[0-9]+)?", Token.TypeEnum.Number),
            ("_*[a-zA-Z][_a-zA-Z0-9]*", Token.TypeEnum.Id),
            ("true|false", Token.TypeEnum.Bool),

            ("{", Token.TypeEnum.LBrace),
            ("}", Token.TypeEnum.RBrace),
            ("<", Token.TypeEnum.LowerThan),
            (">", Token.TypeEnum.GreaterThan),
            (@"\+", Token.TypeEnum.Plus),
            ("=", Token.TypeEnum.Eq),
            ("==", Token.TypeEnum.EqEq),
            (@"\-", Token.TypeEnum.Minus),
            (@"\*", Token.TypeEnum.Times),
            ("/", Token.TypeEnum.Div),
            ("%", Token.TypeEnum.Percent),
            (@"\(", Token.TypeEnum.LPar),
            (@"\)", Token.TypeEnum.RPar),
            (@"\->", Token.TypeEnum.RightArrow),
            (",", Token.TypeEnum.Comma),
            (@"\[", Token.TypeEnum.LBracket),
            (@"\]", Token.TypeEnum.RBracket),
            (".", Token.TypeEnum.Dot)
        };

        internal const string LogPref = "Line {l}, column {c}: ";
        internal const string StatusVar = "status";
        internal const string PercepVar = "percep";
        internal const string DoneReqsVar = "done_reqs";
        internal static readonly string HiddenDoneReqsHeapVar = "__done_reqs_heap";

        /// <summary>
        /// Convierte un símbolo a una representación en <see cref="string"/>.
        /// </summary>
        /// <param name="gramSymbol"></param>
        /// <returns></returns>
        public static string SymbolTypeToStr(GramSymType gramSymbol) {
            // si es un token, vamos a devolver un string q identifica al tipo d token, mientras q si
            // es un no-terminal, devolvemos el nombre del tipo
            return gramSymbol.ToString();
        }

        public static bool Inherits(this Type t, Type ancestor) {
            Type @base = t;

            do {
                if (@base.Name == ancestor.Name) {
                    return true;
                }
                @base = @base.BaseType;

            } while (@base != null);

            return false;
        }

        internal static GosType GetType(object obj) {
            return obj switch {
                double => GosType.Number,
                bool => GosType.Bool,
                //Agent => GosType.Server,
                List<object> => GosType.List,
                null => GosType.Null,
                Status => GosType.ServerStatus,
                Response => GosType.Response,
                Observer => GosType.Alarm,
                Request => GosType.Request,
                _ => throw new NotImplementedException()
            };
        }

        internal static ILogger<T> Logger<T>() {
            return LogFact.CreateLogger<T>();
        }

        /// <summary>
        /// Devuelve una función que ejecuta <paramref name="action"/> y sólo puede ser llamada una vez.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        internal static Action<T> Once<T>(Action<T> action) {
            var called = false;
            return t => {
                if (called) {
                    throw new InvalidOperationException("Function can only be called once.");
                }
                called = true;
                action(t);
            };
        }
    }
}
