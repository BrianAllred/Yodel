﻿// Copyright 2017 Brian Allred
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
    using NYoutubeDL.Options;

    #endregion

    public static class Extensions
    {
        public static bool IsHttpUri(this string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public static void CopyAuthOptions(this Options toOptions, Options fromOptions)
        {
            toOptions.AuthenticationOptions.NetRc = fromOptions.AuthenticationOptions.NetRc;
            toOptions.AuthenticationOptions.Password = fromOptions.AuthenticationOptions.Password;
            toOptions.AuthenticationOptions.TwoFactor = fromOptions.AuthenticationOptions.TwoFactor;
            toOptions.AuthenticationOptions.Username = fromOptions.AuthenticationOptions.Username;
            toOptions.AuthenticationOptions.VideoPassword = fromOptions.AuthenticationOptions.VideoPassword;
        }
    }
}