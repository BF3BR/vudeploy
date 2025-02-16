using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using vusvc.Data;
using vusvc.Managers;

namespace vusvc
{
    /*
     * Flow Chain:
     * Player -> Logs in gets PlayerId
     * 
     * Player -> Uses Player Id to Create Lobby
     * 
     * Lobby -> Queues for a match
     * 
     * Lobby -> Pings MatchManager for status
     * 
     * Lobby -> Gets created match id
     * 
     * Lobby -> Pings MatchManager for status until waiting status
     * 
     * Player -> Pings MatchManager with MatchId for ConnectionInfo
     * 
     * Player -> Joins Server
     */
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // Add PlayerManager singleton
            services.AddSingleton<IPlayerManager, PlayerManager>();

            // Add LobbyManager singleton
            services.AddSingleton<ILobbyManager, LobbyManager>();

            // Add ServerManager singleton
            services.AddSingleton<IServerManager, ServerManager>();

            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime, IPlayerManager playerManager, ILobbyManager lobbyManager, IServerManager serverManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });

            lifetime.ApplicationStopping.Register(() =>
            {
                // Save the PlayerManager database to file
                playerManager.Save(PlayerManager.c_DefaultDatabasePath);

                // Clear all servers from the list
                serverManager.RemoveAllServers(true);
            });
        }
    }
}
