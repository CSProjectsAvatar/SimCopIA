using Core;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Collections.Generic;
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
            var env = new Agents.Environment(debug:true);
            

            env.AddAgent(new SimpleServer(env, "2"));
            env.AddAgent(new SimpleServer(env, "3"));
            env.AddAgent(new SimpleServer(env, "4"));
            env.AddAgent(new SimpleServer(env, "5"));
            env.AddAgent(new SimpleServer(env, "6"));
            env.AddAgent(new SimpleServer(env, "7"));

            var a1 = env.Build.DistributionRequestServer();

            env.AddRequest("0","1", "youtube.com", 10);
            env.AddRequest("0","1", "amazon.com", 15);
            env.AddRequest("0","1", "facebook.com", 22);
            env.AddRequest("0", "1", "claudia.com", 24);


            env.Run();

            System.Console.WriteLine("Responses To Env:");
            foreach(var r in env.solutionResponses) 
                System.Console.WriteLine($"time:{r.responseTime} body:{r.body}");



        }
    }
}
