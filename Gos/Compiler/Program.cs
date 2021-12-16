using Core;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Compiler {
    class Program {
        static void Main(string[] args) {
           /* using var loggerFactory = LoggerFactory.Create(builder => {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("Core", LogLevel.Debug)
                    .AddConsole();
            });
            var simtor = new OneServerSimulator(loggerFactory.CreateLogger<OneServerSimulator>());
            simtor.Run(10);

            Console.WriteLine(string.Join('\n', simtor.Arrivals));
            Console.WriteLine();
            Console.WriteLine(string.Join('\n', simtor.Departures));
            */
             


            //Probando los agentes con servidores simples.
            var env = new AgentesDiscretos.Environment();
            var a1 = env.Build.SimpleServer();

            env.AddRequest("0","1",10);
            env.AddRequest("0","1",28);
            
            foreach( var a in env.Enumerable()){
                a();
            }

            foreach(var r in env.solutionResponses) 
                System.Console.WriteLine($"time:{r.responseTime} body:{r.body}");




        }
    }
}
