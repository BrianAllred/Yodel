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

namespace Yodel.Helpers
{
    #region Using

    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using ElectronNET.API;
    using ElectronNET.API.Entities;
    using NYoutubeDL;

    #endregion

    public class ConsoleHelper
    {
        private readonly StringBuilder outputBuilder = new StringBuilder();

        private Rectangle consoleBounds;

        private int[] consolePosition;

        private BrowserWindow consoleWindow;

        public ConsoleHelper(YoutubeDL ydl)
        {
            ydl.StandardOutputEvent += this.OutputHandler;
            ydl.StandardErrorEvent += this.ErrorHandler;
        }

        public string Output => this.outputBuilder.ToString();

        public void OutputHandler(object sender, string e)
        {
            this.outputBuilder.AppendLine(e);
            this.OnOutputHandled?.Invoke(sender, e);
        }

        public void ErrorHandler(object sender, string e)
        {
            this.outputBuilder.AppendLine(e);
            this.OnErrorHandled?.Invoke(sender, e);
        }

        private async Task CreateConsole()
        {
            this.consoleWindow = await Electron.WindowManager.CreateWindowAsync(
                new BrowserWindowOptions { Show = false },
                $"http://localhost:{BridgeSettings.WebPort}/Console/Index");

            this.consoleWindow.OnMove += async delegate
            {
                this.consolePosition = await this.consoleWindow.GetPositionAsync();
            };

            this.consoleWindow.OnResize += async delegate
            {
                this.consoleBounds = await this.consoleWindow.GetBoundsAsync();
            };

            this.consoleWindow.OnShow += delegate
            {
                if (this.consolePosition != null)
                {
                    this.consoleWindow.SetPosition(this.consolePosition[0], this.consolePosition[1]);
                }

                if (this.consoleBounds != null)
                {
                    this.consoleWindow.SetBounds(this.consoleBounds);
                }
            };

            this.consoleWindow.OnClosed += this.OnClose;
        }

        private void OnClose()
        {
            Thread.Sleep(250);
            this.CreateConsole();
        }

        public async void Show()
        {
            if (this.consoleWindow == null)
            {
                await this.CreateConsole();
            }

            if (!await this.consoleWindow.IsVisibleAsync())
            {
                this.consoleWindow.Show();
            }

            this.consoleWindow.Focus();
        }

        public void Hide()
        {
            this.consoleWindow?.Close();
        }

        public void Close()
        {
            if (this.consoleWindow != null)
            {
                this.consoleWindow.OnClosed -= this.OnClose;
                this.consoleWindow.Close();
            }
        }

        public event EventHandler<string> OnOutputHandled;

        public event EventHandler<string> OnErrorHandled;
    }
}