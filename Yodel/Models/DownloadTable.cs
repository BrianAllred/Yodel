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

namespace Yodel.Models
{
    #region Using

    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using NYoutubeDL.Models;

    #endregion

    public class DownloadTable : IDisposable
    {
        public delegate void TableUpdateEventHandler(object sender, EventArgs e);

        private const string TableHeader =
                "<thead><tr><th>Thumbnail</th><th>Title</th><th>Size</th><th>Progress</th><th>ETA</th><th>Speed</th></tr></thead>"
            ;

        private readonly DownloadInfo info;

        private readonly object tableLock = new object();

        private readonly Dictionary<string, string> videoRows = new Dictionary<string, string>();

        public DownloadTable(DownloadInfo info)
        {
            this.info = info;
            if (this.info is VideoDownloadInfo)
            {
                this.info.PropertyChanged += this.VideoDownloadChanged;
                this.VideoDownloadChanged(this.info, null);
            }
            else if (this.info is PlaylistDownloadInfo playlistDownloadInfo)
            {
                foreach (VideoDownloadInfo video in playlistDownloadInfo.Videos)
                {
                    video.PropertyChanged += this.VideoDownloadChanged;
                    this.VideoDownloadChanged(video, null);
                }
            }
            else if (this.info is MultiDownloadInfo multiDownloadInfo)
            {
                foreach (VideoDownloadInfo video in multiDownloadInfo.Videos)
                {
                    video.PropertyChanged += this.VideoDownloadChanged;
                    this.VideoDownloadChanged(video, null);
                }

                foreach (PlaylistDownloadInfo playlist in multiDownloadInfo.Playlists)
                {
                    foreach (VideoDownloadInfo plVideo in playlist.Videos)
                    {
                        plVideo.PropertyChanged += this.VideoDownloadChanged;
                        this.VideoDownloadChanged(plVideo, null);
                    }
                }
            }
        }

        public string Table
        {
            get
            {
                lock (this.tableLock)
                {
                    return TableHeader + "<tbody>" + string.Join("", this.videoRows.Values) + "</tbody>";
                }
            }
        }

        public void Dispose()
        {
            if (this.info != null)
            {
                if (this.info is VideoDownloadInfo)
                {
                    this.info.PropertyChanged -= this.VideoDownloadChanged;
                }
                else if (this.info is PlaylistDownloadInfo playlistDownloadInfo)
                {
                    foreach (VideoDownloadInfo video in playlistDownloadInfo.Videos)
                    {
                        video.PropertyChanged -= this.VideoDownloadChanged;
                    }
                }
                else if (this.info is MultiDownloadInfo multiDownloadInfo)
                {
                    foreach (VideoDownloadInfo video in multiDownloadInfo.Videos)
                    {
                        video.PropertyChanged -= this.VideoDownloadChanged;
                    }

                    foreach (PlaylistDownloadInfo playlist in multiDownloadInfo.Playlists)
                    {
                        foreach (VideoDownloadInfo plVideo in playlist.Videos)
                        {
                            plVideo.PropertyChanged -= this.VideoDownloadChanged;
                        }
                    }
                }
            }

            this.TableUpdateEvent = null;
        }

        private void VideoDownloadChanged(object sender, PropertyChangedEventArgs e)
        {
            lock (this.tableLock)
            {
                VideoDownloadInfo videoInfo = (VideoDownloadInfo) sender;

                if (!string.IsNullOrEmpty(videoInfo.Id) && !string.IsNullOrEmpty(videoInfo.Title))
                {
                    this.videoRows[videoInfo.Id] =
                        $"<tr><td><img src=\"{videoInfo.Thumbnail ?? videoInfo.Thumbnails?.FirstOrDefault()?.Url}\" width=\"100\"></td><td>{videoInfo.Title.Trim()}</td><td>{videoInfo.VideoSize}</td><td><div class=\"progress\"><div class=\"determinate\" style=\"width: {videoInfo.VideoProgress}%\"></div</td><td>{videoInfo.Eta}</td><td>{videoInfo.DownloadRate}</td></tr>";
                }
            }

            this.TableUpdateEvent?.Invoke(this.Table, EventArgs.Empty);
        }

        public event TableUpdateEventHandler TableUpdateEvent;
    }
}