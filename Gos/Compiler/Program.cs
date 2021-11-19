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

            var simtor = new OneLeaderFollowersSimulator(
                @params.Followers,
                @params.Lambda,
                loggerFactory.CreateLogger<OneLeaderFollowersSimulator>());

            log.LogInformation("Comienzo de la simulación...");
            log.LogInformation(
                $"{@params.Followers} seguidores, " +
                $"lambda = {@params.Lambda} y " +
                $"el sistema cierra en el tiempo {@params.CloseTime}.");

            simtor.Run(@params.CloseTime);

            log.LogInformation("Simulación terminada.");

            Thread.Sleep(100); // esperando a q el mensaje d arriba se escriba

            Console.WriteLine("\n===========Resultados===========\n");
            Console.WriteLine("Arribos:");
            Console.WriteLine(string.Join('\n', simtor.Arrivals));
            Console.WriteLine("\nPartidas:");
            Console.WriteLine(string.Join('\n', simtor.GetDepartures()));
        }
    }
}
