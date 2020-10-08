// Copyright 2014 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// package driver -- go2cs converted at 2020 October 08 04:43:05 UTC
// import "cmd/vendor/github.com/google/pprof/internal/driver" ==> using driver = go.cmd.vendor.github.com.google.pprof.@internal.driver_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\internal\driver\options.go
using bufio = go.bufio_package;
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using strings = go.strings_package;

using binutils = go.github.com.google.pprof.@internal.binutils_package;
using plugin = go.github.com.google.pprof.@internal.plugin_package;
using symbolizer = go.github.com.google.pprof.@internal.symbolizer_package;
using transport = go.github.com.google.pprof.@internal.transport_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vendor {
namespace github.com {
namespace google {
namespace pprof {
namespace @internal
{
    public static partial class driver_package
    {
        // setDefaults returns a new plugin.Options with zero fields sets to
        // sensible defaults.
        private static ptr<plugin.Options> setDefaults(ptr<plugin.Options> _addr_o)
        {
            ref plugin.Options o = ref _addr_o.val;

            ptr<plugin.Options> d = addr(new plugin.Options());
            if (o != null)
            {
                d.val = o;
            }
            if (d.Writer == null)
            {
                d.Writer = new oswriter();
            }
            if (d.Flagset == null)
            {
                d.Flagset = addr(new GoFlags());
            }
            if (d.Obj == null)
            {
                d.Obj = addr(new binutils.Binutils());
            }
            if (d.UI == null)
            {
                d.UI = addr(new stdUI(r:bufio.NewReader(os.Stdin)));
            }
            if (d.HTTPTransport == null)
            {
                d.HTTPTransport = transport.New(d.Flagset);
            }
            if (d.Sym == null)
            {
                d.Sym = addr(new symbolizer.Symbolizer(Obj:d.Obj,UI:d.UI,Transport:d.HTTPTransport));
            }
            return _addr_d!;

        }

        private partial struct stdUI
        {
            public ptr<bufio.Reader> r;
        }

        private static (@string, error) ReadLine(this ptr<stdUI> _addr_ui, @string prompt)
        {
            @string _p0 = default;
            error _p0 = default!;
            ref stdUI ui = ref _addr_ui.val;

            os.Stdout.WriteString(prompt);
            return ui.r.ReadString('\n');
        }

        private static void Print(this ptr<stdUI> _addr_ui, params object[] args)
        {
            args = args.Clone();
            ref stdUI ui = ref _addr_ui.val;

            ui.fprint(os.Stderr, args);
        }

        private static void PrintErr(this ptr<stdUI> _addr_ui, params object[] args)
        {
            args = args.Clone();
            ref stdUI ui = ref _addr_ui.val;

            ui.fprint(os.Stderr, args);
        }

        private static bool IsTerminal(this ptr<stdUI> _addr_ui)
        {
            ref stdUI ui = ref _addr_ui.val;

            return false;
        }

        private static bool WantBrowser(this ptr<stdUI> _addr_ui)
        {
            ref stdUI ui = ref _addr_ui.val;

            return true;
        }

        private static @string SetAutoComplete(this ptr<stdUI> _addr_ui, Func<@string, @string> _p0)
        {
            ref stdUI ui = ref _addr_ui.val;

        }

        private static void fprint(this ptr<stdUI> _addr_ui, ptr<os.File> _addr_f, slice<object> args)
        {
            ref stdUI ui = ref _addr_ui.val;
            ref os.File f = ref _addr_f.val;

            var text = fmt.Sprint(args);
            if (!strings.HasSuffix(text, "\n"))
            {
                text += "\n";
            }

            f.WriteString(text);

        }

        // oswriter implements the Writer interface using a regular file.
        private partial struct oswriter
        {
        }

        private static (io.WriteCloser, error) Open(this oswriter _p0, @string name)
        {
            io.WriteCloser _p0 = default;
            error _p0 = default!;

            var (f, err) = os.Create(name);
            return (f, error.As(err)!);
        }
    }
}}}}}}}
