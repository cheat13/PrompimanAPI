﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using PrompimanAPI.Dac;
using PrompimanAPI.Services;

namespace PrompimanAPI
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });

            var webConfig = Configuration.GetSection("WebConfig").Get<WebConfig>();
            var dbConfig = Configuration.GetSection("MongoDbConfig").Get<DbConfig>();

            services.AddTransient(x => dbConfig);
            services.AddTransient(x => webConfig);
            services.AddSingleton<IDbService, DbService>();
            services.AddTransient<IMemberDac, MemberDac>();
            services.AddTransient<IReservationDac, ReservationDac>();
            services.AddTransient<IRoomActivatedDac, RoomActivatedDac>();
            services.AddTransient<IRoomDac, RoomDac>();
            services.AddTransient<IMasterDac, MasterDac>();
            services.AddTransient<IMemberService, MemberService>();
            services.AddTransient<IReservationService, ReservationService>();
            services.AddTransient<IMasterService, MasterService>();
            services.AddTransient<IRoomActService, RoomActService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            // app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
