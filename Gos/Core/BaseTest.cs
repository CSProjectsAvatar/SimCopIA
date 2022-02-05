using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Core {
    [TestClass]
    public abstract class BaseTest {
        public ServiceCollection services;

        public ServiceProvider Container =>
            services.BuildServiceProvider();

        //public IConfiguration Configuration;
        protected static ILoggerFactory LoggerFact;

        [TestCleanup]
        public void BaseCleanUp() {
            LoggerFact.Dispose();
        }

        [TestInitialize]
        public void PrimaryInitializer() {
            services = new ServiceCollection();

            services.AddLogging();

            LoggerFact = LoggerFactory.Create(builder => {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("Core", LogLevel.Debug)
                    .AddFilter("Compiler", LogLevel.Information)
                    .AddFilter("DataClassHierarchy", LogLevel.Information)
                    .AddConsole();
            });
        } // init

    } // class
}