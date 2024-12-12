using API;
using Phase.Resources;

namespace Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
            builder.Configuration.AddJsonFile("APPConfig.json");
            builder.Configuration.AddJsonFile("APIConfig.json");
            builder.Services.AddApiLibrary(builder);
            builder.Services.AddStaticConfiguration(builder.Configuration);
            


            var app = builder.Build();
           
            // app.UseCors("corsPolicy");

            app.UseSimpleResponseMiddleware();
            DbManager.Connect();
            app.Run();
            
        }
    }
}