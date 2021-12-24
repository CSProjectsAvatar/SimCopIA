using Core;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Agents;
namespace Compiler {
    class Program {
        static void Main(string[] args) {
            //Probando los agentes con servidores simples.
            var env = new Agents.Environment();
            var a1 = env.Build.SimpleServer();
            var a2 = env.Build.SimpleServer();
            var a3 = env.Build.DistributionServer(new List<string>(){"1","2"});


            env.AddRequest("0","3",10);
            env.AddRequest("0","3",15);
            env.AddRequest("0","3",22);
            
            env.Run();

            System.Console.WriteLine("Responses To Env:");
            foreach(var r in env.solutionResponses) 
                System.Console.WriteLine($"time:{r.responseTime} body:{r.body}");



        }
    }
}
