using Compiler.Lexer;
using Core;
using DataClassHierarchy;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace Compiler {
    class Program {
        static void Main(string[] args) {
            using var loggerFactory = LoggerFactory.Create(builder => { // configurando niveles de logueo
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("Core", LogLevel.Information)
                    .AddFilter("Compiler", LogLevel.Information)
                    .AddConsole();
            });
            Helper.LogFact = loggerFactory;

            ILogger<ReLexer> logReLex = loggerFactory.CreateLogger<ReLexer>();
            ILogger<Lr1> logLr1 = loggerFactory.CreateLogger<Lr1>();
            ILogger<Lr1Dfa> logLr1Dfa = loggerFactory.CreateLogger<Lr1Dfa>();
            ILogger<Lexer.Lexer> logLex = loggerFactory.CreateLogger<Lexer.Lexer>();
            var lex = new Lexer.Lexer(Helper.TokenWithRegexs, new ReGrammar(), logReLex, logLr1, logLr1Dfa, logLex);
            var tokens = lex.Tokenize(File.ReadAllText(args[0]));
            var parser = new Lr1(new GosGrammar(), logLr1, logLr1Dfa);

            if (parser.TryParse(tokens, out var ast)) {
                var ctx = new Context();

                if (ast.Validate(ctx)) {
                    var vis = new EvalVisitor(ctx, loggerFactory.CreateLogger<EvalVisitor>(), Console.Out);
                    vis.Visit(ast);
                }
            }
        }
    }
}
