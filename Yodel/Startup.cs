// Copyright 2017 Brian Allred
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to
// deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
// sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

namespace Yodel
{
    #region Using

    using System.IO;
    using Controllers;
    using ElectronNET.API;
    using ElectronNET.API.Entities;
    using Helpers;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using NYoutubeDL.Options;

    #endregion

    public class Startup
    {
        private string configFolder;

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");
            });

            this.configFolder = Electron.App.GetPathAsync(PathName.userData).Result;
            string optionsFilePath = this.configFolder + Path.DirectorySeparatorChar + "yodel" +
                                     Path.DirectorySeparatorChar + "options.conf";

            if (File.Exists(optionsFilePath))
            {
                YoutubeHelper.Instance.YoutubeDl.Options = Options.Deserialize(File.ReadAllText(optionsFilePath));
            }

            BrowserWindow browserWindow = Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
                {
                    Width = 1000,
                    Height = 800,
                    Show = false
                }).
                Result;

            browserWindow.WebContents.OnCrashed += async killed =>
            {
                MessageBoxOptions options = new MessageBoxOptions("This process has crashed.")
                {
                    Type = MessageBoxType.info,
                    Title = "Renderer Process Crashed",
                    Buttons = new[] { "Reload", "Close" }
                };

                MessageBoxResult result = await Electron.Dialog.ShowMessageBoxAsync(options);

                if (result.Response == 0)
                {
                    browserWindow.Reload();
                }
                else
                {
                    browserWindow.Close();
                }
            };

            browserWindow.OnUnresponsive += async () =>
            {
                MessageBoxOptions options = new MessageBoxOptions("This process is hanging.")
                {
                    Type = MessageBoxType.info,
                    Title = "Renderer Process Hanging",
                    Buttons = new[] { "Reload", "Close" }
                };

                MessageBoxResult result = await Electron.Dialog.ShowMessageBoxAsync(options);

                if (result.Response == 0)
                {
                    browserWindow.Reload();
                }
                else
                {
                    browserWindow.Close();
                }
            };

            browserWindow.OnReadyToShow += () => browserWindow.Show();
            browserWindow.SetTitle("Yodel - a youtube-dl UI");

            //browserWindow.OnClosed += delegate
            //{
            //    foreach (BrowserWindow window in Electron.WindowManager.BrowserWindows)
            //    {
            //        window.Close();
            //    }
            //};

            browserWindow.OnShow += delegate
            {
                if (this.Configuration["EnableHeartbeat"].Equals(bool.TrueString))
                {
                    HomeController.StartHeartbeat();
                }
            };
        }
    }
}