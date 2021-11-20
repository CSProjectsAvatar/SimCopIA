using Core;
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
                    .AddConsole();
            });
            var @params = JsonSerializer.Deserialize<SimParameters>(File.ReadAllText("appsettings.json"));
            var log = loggerFactory.CreateLogger<Program>();
            var metah = new MetaHeuristics();
            var indivs = Individual.Sampler(@params.Poblation);

            log.LogInformation(
                $"{@params.Followers} seguidores, " +
                $"lambda = {@params.Lambda}, " +
                $"el sistema cierra en el tiempo {@params.CloseTime}, " +
                "se minimiza el tiempo de atención a los pedidos, " +
                $"el costo de mantenimiento máximo mensual es {@params.MonthlyMaintenanceCost} y " +
                $"la metaheurística se ejecutará por {@params.RunTimeMilliseconds} milisegundos, " +
                $"en una población de {@params.Poblation} individuos.");
            log.LogInformation("Comienzo de la metaheurística...");

            metah.Run(indivs,
                (Individual i) => SimulationSystem.RunSimulation(i, default, @params.Lambda, @params.CloseTime),
                (Individual i) => 
                    0 < i.MonthlyMaintennanceCost && 
                    i.MonthlyMaintennanceCost < @params.MonthlyMaintenanceCost,
                @params.RunTimeMilliseconds);

            log.LogInformation("Metaheurística terminada.");
            Thread.Sleep(100); // esperando a q el mensaje d arriba se escriba
            Console.WriteLine("\n===========Resultados===========\n");
            Console.WriteLine($"El sistema debe poseer {indivs[0].Doers} doers.");
        }
    }
}
