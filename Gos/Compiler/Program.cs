using Compiler.Lexer;
using Core;
using DataClassHierarchy;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Agents;
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
            ILogger<Lr1Dfa> logLr1Dfa = loggerFactory.CreateLogger<Lr1Dfa>();

            if (args.Length == 0) {
                Console.WriteLine(@"
Usage:

To build the parser DFA:
gos init

To run a gos file:
gos run FILE");
            } else {
                switch (args[0]) {
                    case "init":
                        var dfa = new Lr1Dfa(new GosGrammar(), logLr1Dfa);
                        dfa.SaveToFile("./lr1-dfa.json");
                        break;
                    case "run" when args.Length > 1:
                        Helper.LogFact = loggerFactory;

                        ILogger<ReLexer> logReLex = loggerFactory.CreateLogger<ReLexer>();
                        ILogger<Lr1> logLr1 = loggerFactory.CreateLogger<Lr1>();
                        ILogger<Lexer.Lexer> logLex = loggerFactory.CreateLogger<Lexer.Lexer>();
                        var lex = new Lexer.Lexer(Helper.TokenWithRegexs, new ReGrammar(), logReLex, logLr1, logLr1Dfa, logLex);
                        var tokens = lex.Tokenize(File.ReadAllText(args[1]));
                        var parser = new Lr1(new GosGrammar(), "./lr1-dfa.json", logLr1, logLr1Dfa);

                        if (parser.TryParse(tokens, out var ast)) {
                            if (ast.Validate(new Context())) {
                                var vis = new EvalVisitor(new Context(), loggerFactory.CreateLogger<EvalVisitor>(), Console.Out);
                                vis.Visit(ast);
                            }
                        }
                        break;
                    default:
                        Console.WriteLine(@"
Usage:

To build the parser DFA:
gos init

To run a gos file:
gos run FILE");
                        break;
                }
            }
        }
    }
}
