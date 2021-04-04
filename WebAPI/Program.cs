
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Persistencia;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Dominio;

namespace WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var hostServer = CreateHostBuilder(args).Build();

            using (var ambiente = hostServer.Services.CreateScope())
            {
                var services = ambiente.ServiceProvider;

                try
                {
                    var userManager = services.GetRequiredService<UserManager<Usuario>>();
                    var context = services.GetRequiredService<CursosOnlineContext>();
                    context.Database.Migrate();
                    DataPrueba.InsertarData(context, userManager).Wait();

                }
                catch (System.Exception e)
                {
                    var logging = services.GetRequiredService<ILogger<Program>>();
                    logging.LogError(e, "Ocurrio un error en la migracion");
                }
            }
            hostServer.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
