using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace OAuth.Server
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
            services.AddAuthentication("OAuth")
                   .AddJwtBearer("OAuth", config =>
                   {
                       var secretBytes = Encoding.UTF8.GetBytes(Constants.Secret);
                       var key = new SymmetricSecurityKey(secretBytes);

                       
                       config.Events = new JwtBearerEvents()
                       {
                           OnMessageReceived = context =>
                           {
                               if (context.Request.Query.ContainsKey("access_token"))
                               {
                                   //queryString ����� �����ϱ����� �ڵ�. WebAPI ��ſ��� Authorization ����� �������� ��� ���
                                   context.Token = context.Request.Query["access_token"];
                               }
                               else if (context.Request.Cookies.ContainsKey("access_token"))
                               {
                                   // ��Ű ����� �����ϱ����� �ڵ�. OAuth �������񽺿� ���� ��������� �����񽺰� ���� �ҽ��� ���� �幰�� ������ �Ϲ������� ������� ����.
                                   context.Token = context.Request.Cookies["access_token"];
                               }
                               return Task.CompletedTask;
                           }
                       };

                       // ��ū ����. ��� "Authorization"�� Value "Bearer {JWT}"�� Ȯ���Ͽ� ��ū�� �ùٸ��� [Authorize]�� ������ ���� ��������.
                       config.TokenValidationParameters = new TokenValidationParameters()
                       {
                           ValidIssuer = Constants.Issuer, // ������ ����
                           ValidAudience = Constants.Audiance, // ��뼭�� ����
                           IssuerSigningKey = key, // Ű ����
                           ClockSkew = TimeSpan.Zero, // �⺻���� 5��. exipre �ð� �ʰ����� Ȯ�ν� ��Ű� �����ð��� ������ �ð���
                       };
                   });
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
