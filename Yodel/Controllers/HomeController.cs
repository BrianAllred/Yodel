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

namespace Yodel.Controllers
{
    #region Using

    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using ElectronNET.API;
    using ElectronNET.API.Entities;
    using Helpers;
    using Microsoft.AspNetCore.Mvc;
    using Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NYoutubeDL;
    using NYoutubeDL.Options;

    #endregion

    public class HomeController : Controller
    {
        private const string DefaultTemplate = "%(title)s.%(ext)s";

        private readonly string configFolder = Electron.App.GetPathAsync(PathName.userData).Result;

        private readonly object serializeLock = new object();

        private static BrowserWindow MainWindow => Electron.WindowManager.BrowserWindows.First();

        private static YoutubeDL Ydl => YoutubeHelper.Instance.YoutubeDl;

        private static ConsoleHelper Console => YoutubeHelper.Instance.Console;

        public IActionResult Index()
        {
            Electron.IpcMain.On(Messages.DOWNLOAD, this.Download);

            Electron.IpcMain.On(Messages.SELECT_DIRECTORY, SelectDirectory);

            Electron.IpcMain.On(Messages.UPDATE_OPTIONS, this.UpdateOptions);

            Electron.IpcMain.On(Messages.SET_YDL_PROPERTY, SetYdlProperty);

            Electron.IpcMain.On(Messages.CONSOLE, delegate { Console.Show(); });

            MainWindow.OnShow += delegate
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    Thread.Sleep(1000);
                    this.LoadOptions();
                    Console.Show();
                    Thread.Sleep(250);
                    Console.Hide();
                });
            };

            MainWindow.OnClose += delegate
            {
                this.SaveOptions();
                Thread.Sleep(1000);
                Console.Close();
            };

            return this.View();
        }

        public static void StartHeartbeat()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                while (true)
                {
                    Electron.IpcMain.Send(MainWindow, Messages.HEARTBEAT);
                    Thread.Sleep(1000);
                }
            });
        }

        private static void SetYdlProperty(dynamic obj)
        {
            string propName = obj.property;
            PropertyInfo propInfo = Ydl.GetType().GetProperty(propName);
            object value = obj.value.ToObject(propInfo.PropertyType);
            Ydl.GetType().GetProperty(propName).SetValue(Ydl, value);
        }

        private void LoadOptions()
        {
            lock (this.serializeLock)
            {
                JObject options = JObject.Parse(Ydl.Options.Serialize());
                if (options.HasValues)
                {
                    Electron.IpcMain.Send(MainWindow, Messages.LOAD_OPTIONS, Ydl.Options.Serialize());
                }
            }
        }

        private void SaveOptions()
        {
            if (string.IsNullOrEmpty(this.configFolder))
            {
                return;
            }

            string optionsFilePath = this.configFolder + Path.DirectorySeparatorChar + "yodel" +
                                     Path.DirectorySeparatorChar + "options.conf";

            string yodelDir = Path.GetDirectoryName(optionsFilePath);

            if (!string.IsNullOrWhiteSpace(Ydl.Options.FilesystemOptions.Output) &&
                !Directory.Exists(Ydl.Options.FilesystemOptions.Output))
            {
                Ydl.Options.FilesystemOptions.Output =
                    Path.GetDirectoryName(Ydl.Options.FilesystemOptions.Output);
            }

            lock (this.serializeLock)
            {
                if (!Directory.Exists(yodelDir))
                {
                    Directory.CreateDirectory(yodelDir);
                }

                System.IO.File.WriteAllText(optionsFilePath, Ydl.Options.Serialize());
            }
        }

        private void UpdateOptions(dynamic options)
        {
            JObject optionsObj = options;
            lock (this.serializeLock)
            {
                Ydl.Options = Options.Deserialize(optionsObj.ToString());
            }
        }

        private static async void SelectDirectory(dynamic args)
        {
            string id = args.id.ToString();
            string functionName = args.functionName.ToString();

            OpenDialogOptions options = new OpenDialogOptions
            {
                Properties = new[]
                {
                    OpenDialogProperty.openDirectory
                }
            };

            string[] directory = await Electron.Dialog.ShowOpenDialogAsync(MainWindow, options);

            dynamic directoryReply = new JObject();
            directoryReply.id = id;

            directoryReply.directory = directory.Length > 0 ? directory.First() : string.Empty;
            directoryReply.functionName = functionName;

            Electron.IpcMain.Send(MainWindow, Messages.DIRECTORY_SELECTED, directoryReply);
        }

        private void Download(dynamic options)
        {
            string oldOptions;
            lock (this.serializeLock)
            {
                oldOptions = Ydl.Options.Serialize();
                Ydl.Options = Options.Deserialize(options.options.ToString());
            }

            if (Ydl.Options.GeneralOptions.Update)
            {
                Process update = Ydl.Download(true);
                update.Exited += delegate
                {
                    Electron.IpcMain.Send(MainWindow, Messages.DOWNLOAD_COMPLETE);
                    Ydl.Options = Options.Deserialize(oldOptions);
                };
            }
            else
            {
                string[] urls = options.urls.ToObject<string[]>();
                string[] badUrls = urls.
                    Where(url => url == null || !url.IsHttpUri() && !string.IsNullOrWhiteSpace(url)).
                    ToArray();

                if (badUrls.Length > 0)
                {
                    Electron.IpcMain.Send(MainWindow, Messages.URL_PARSING_ERROR,
                        JsonConvert.SerializeObject(badUrls));
                    return;
                }

                Ydl.Options.FilesystemOptions.Output +=
                    Path.DirectorySeparatorChar + DefaultTemplate;

                // Start the download
                Process download = Ydl.Download(string.Join(" ", urls));

                DownloadTable table = new DownloadTable(Ydl.Info);

                download.Exited += (sender, args) =>
                {
                    Electron.IpcMain.Send(MainWindow, Messages.DOWNLOAD_COMPLETE);
                    NotificationOptions notificationOptions =
                        new NotificationOptions("Yodel", "Download complete!")
                        {
                            OnClick = delegate { MainWindow.Focus(); }
                        };

                    Electron.Notification.Show(notificationOptions);
                    table.Dispose();
                };

                table.TableUpdateEvent += delegate(object sender, EventArgs args)
                {
                    Electron.IpcMain.Send(MainWindow, Messages.UPDATE_TABLE, sender as string);
                };
            }
        }
    }
}