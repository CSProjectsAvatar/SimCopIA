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
            ($"{ServerClass}|request|response|alarm|{ResourceClass}|{LayerClass}", Token.TypeEnum.Class),
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
            ("ping", Token.TypeEnum.Ping),
            ("alarm_me", Token.TypeEnum.AlarmMe),
            ("ask", Token.TypeEnum.Ask),
            ("order", Token.TypeEnum.Order),
            ("save", Token.TypeEnum.Save),

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
            (",", Token.TypeEnum.Comma),
            (@"\[", Token.TypeEnum.LBracket),
            (@"\]", Token.TypeEnum.RBracket),
            (".", Token.TypeEnum.Dot)
        };
        private static int _resrcCount = 0;
        private static int _servCount = 0;
        internal const string ServerClass = "server";
        internal const string LogPref = "Line {l}, column {c}: ";
        internal const string StatusVar = "status";
        internal const string PercepVar = "percep";
        internal const string DoneReqsVar = "done_reqs";
        internal static readonly string HiddenDoneReqsHeapVar = "__done_reqs_heap";
        internal static readonly Random Random = new Random(Environment.TickCount);
        internal const string RandFun = "rand";
        internal const string RandIntFun = "rand_int";
        internal const string GeneticFun = "genetic";
        internal const string EnvVar = "env";
        internal const string ResourceClass = "resource";
        internal const string LayerClass = "layer";

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
                Server => GosType.Server,
                List<object> => GosType.List,
                null => GosType.Null,
                Status => GosType.ServerStatus,
                Response => GosType.Response,
                Observer => GosType.Alarm,
                Request => GosType.Request,
                Env => GosType.Environment,
                string => GosType.String,
                Resource => GosType.Resource,
                Behavior => GosType.Behavior,
                DefFun => GosType.Function,
                Layer => GosType.Layer,
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

        internal static bool IsInteger(double x) {
            return !(x - (int)x > double.Epsilon);
        }

        internal static string NewResrcName() {
            return $"resrc_{++_resrcCount}";
        }

        public static void Dispose() {
            _resrcCount = 0;
            _servCount = 0;
        }

        internal static string NewServerName() {
            return $"serv_{++_servCount}";
        }
    }
}
