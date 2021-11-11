using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Tests {
    [TestClass]
    public abstract class BaseTest {
        public ServiceCollection services;

        public ServiceProvider Container =>
            services.BuildServiceProvider();

        public IConfiguration Configuration;

        [TestInitialize]
        public void PrimaryInitializer() {
            services = new ServiceCollection();

            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json")
                .Build();

            services.AddTransient(c => Configuration);
            services.AddLogging();
        } // init

    } // class
}
