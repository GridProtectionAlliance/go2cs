// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package browser provides utilities for interacting with users' browsers.
// package browser -- go2cs converted at 2020 August 29 09:59:28 UTC
// import "cmd/internal/browser" ==> using browser = go.cmd.@internal.browser_package
// Original source: C:\Go\src\cmd\internal\browser\browser.go
using os = go.os_package;
using exec = go.os.exec_package;
using runtime = go.runtime_package;
using time = go.time_package;
using static go.builtin;
using System;
using System.Threading;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class browser_package
    {
        // Commands returns a list of possible commands to use to open a url.
        public static slice<slice<@string>> Commands()
        {
            slice<slice<@string>> cmds = default;
            {
                var exe = os.Getenv("BROWSER");

                if (exe != "")
                {
                    cmds = append(cmds, new slice<@string>(new @string[] { exe }));
                }
            }
            switch (runtime.GOOS)
            {
                case "darwin": 
                    cmds = append(cmds, new slice<@string>(new @string[] { "/usr/bin/open" }));
                    break;
                case "windows": 
                    cmds = append(cmds, new slice<@string>(new @string[] { "cmd", "/c", "start" }));
                    break;
                default: 
                    if (os.Getenv("DISPLAY") != "")
                    { 
                        // xdg-open is only for use in a desktop environment.
                        cmds = append(cmds, new slice<@string>(new @string[] { "xdg-open" }));
                    }
                    break;
            }
            cmds = append(cmds, new slice<@string>(new @string[] { "chrome" }), new slice<@string>(new @string[] { "google-chrome" }), new slice<@string>(new @string[] { "chromium" }), new slice<@string>(new @string[] { "firefox" }));
            return cmds;
        }

        // Open tries to open url in a browser and reports whether it succeeded.
        public static bool Open(@string url)
        {
            foreach (var (_, args) in Commands())
            {
                var cmd = exec.Command(args[0L], append(args[1L..], url));
                if (cmd.Start() == null && appearsSuccessful(cmd, 3L * time.Second))
                {
                    return true;
                }
            }
            return false;
        }

        // appearsSuccessful reports whether the command appears to have run successfully.
        // If the command runs longer than the timeout, it's deemed successful.
        // If the command runs within the timeout, it's deemed successful if it exited cleanly.
        private static bool appearsSuccessful(ref exec.Cmd cmd, time.Duration timeout)
        {
            var errc = make_channel<error>(1L);
            go_(() => () =>
            {
                errc.Send(cmd.Wait());
            }());

            return true;
            return err == null;
        }
    }
}}}
