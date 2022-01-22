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
            //Probando los agentes con servidores simples.
            var env = new Agents.Environment(debug:true);

            //workers
            env.AddAgent(new Worker(env, "2"));
            env.AddAgent(new Worker(env, "6"));
            
            // Distribuidor de carga con 2 workers
            env.AddAgent(new Worker(env, "4"));
            env.AddAgent(new Worker(env, "5"));
            env.AddAgent(new Distributor(env, "dist1", new List<string>{"4","5"}));
            

            // interactive worker con 3 workers enlazados
            var a1 = env.Build.InteractiveWorker("facebook.com");
            a1.AddToRequirmentsDic("/",new List<string>{"2","dist1","6"});
            a1.AddToRequirmentsDic("/other",new List<string>{"4,5"});
            a1.AddToRequirmentsDic("/about",new List<string>{"6"});

            

            // request del lado del cliente (desde el environment, su identificador es 0)
            env.AddRequest("0","facebook.com", "/", 10);
            env.AddRequest("0","facebook.com", 15);
            
            // corre la simulación
            env.Run();


            // Imprime los responses que llegaron al cliente (environment)
            System.Console.WriteLine("Responses To Env:");
            foreach(var r in env.solutionResponses) 
                System.Console.WriteLine($"time:{r.responseTime} body:{r.body}");



        }
    }
}
