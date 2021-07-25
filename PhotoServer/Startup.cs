using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PhotoServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseCors(builder =>
                builder
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin());

            // configure port
            var a = app.ServerFeatures.Get<IServerAddressesFeature>();
            a.Addresses.Clear();
            a.Addresses.Add($"http://*:5002");

            string myHostName = Dns.GetHostName().ToString();
            IPHostEntry iphostentry = Dns.GetHostEntry(myHostName);

            // Enumerate IP addresses
            foreach (IPAddress ipaddress in iphostentry.AddressList.Where(a => a.AddressFamily == AddressFamily.InterNetwork))
            {
                Console.WriteLine($"Listening on {ipaddress}");
            }
            Console.WriteLine($"Store location: {Settings.Location}");
        }
    }
}
