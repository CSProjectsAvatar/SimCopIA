using Core;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Agents;
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
            var env = new Agents.Environment();
            var a1 = env.Build.SimpleServer();

            env.AddRequest("0","1",10);
            env.AddRequest("0","1",15);
            env.AddRequest("0","1",22);
            
            env.Run();

            System.Console.WriteLine("Responses To Env:");
            foreach(var r in env.solutionResponses) 
                System.Console.WriteLine($"time:{r.responseTime} body:{r.body}");



        }
    }
}
