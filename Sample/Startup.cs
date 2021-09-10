using CSDistributeTransaction.Core.Option;
using CSDistributeTransaction.Core.Tcc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sample.Entity;
using Sample.Services;
using Sample.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sample
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
            services.AddScoped<ReduceStockStep, ReduceStockStep>();
            services.AddScoped<PlaceOrderStep, PlaceOrderStep>();


            services.AddScoped(typeof(IStore<>),typeof(InMemeryStore<>));

            services.AddScoped<StockService,StockService>();
            services.AddScoped<TccTransactionOption, TccTransactionOption>();
            services.AddScoped<TccTransactionManager, TccTransactionManager>();


            services.AddLogging(config=> 
            {
                config.AddConsole();
            });
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            using (var scope = app.ApplicationServices.CreateScope())
            {
               
                var store = scope.ServiceProvider.GetService<IStore<Stock>>();

                for (int i = 0; i < 10; i++)
                {
                    store.Add(new Stock()
                    {
                        CreateDate = DateTime.Now,
                        GoodsId = i.ToString(),
                        Id = Guid.NewGuid().ToString(),
                        Records = new List<StockRecord>(),
                        TotalStock = 99
                    });
                }
            }
           
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
