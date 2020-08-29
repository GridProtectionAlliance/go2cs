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

// package driver -- go2cs converted at 2020 August 29 10:05:27 UTC
// import "cmd/vendor/github.com/google/pprof/internal/driver" ==> using driver = go.cmd.vendor.github.com.google.pprof.@internal.driver_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\internal\driver\options.go
using bufio = go.bufio_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using strings = go.strings_package;

using binutils = go.github.com.google.pprof.@internal.binutils_package;
using plugin = go.github.com.google.pprof.@internal.plugin_package;
using symbolizer = go.github.com.google.pprof.@internal.symbolizer_package;
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
        private static ref plugin.Options setDefaults(ref plugin.Options o)
        {
            plugin.Options d = ref new plugin.Options();
            if (o != null)
            {
                d.Value = o.Value;
            }
            if (d.Writer == null)
            {
                d.Writer = new oswriter();
            }
            if (d.Flagset == null)
            {
                d.Flagset = new goFlags();
            }
            if (d.Obj == null)
            {
                d.Obj = ref new binutils.Binutils();
            }
            if (d.UI == null)
            {
                d.UI = ref new stdUI(r:bufio.NewReader(os.Stdin));
            }
            if (d.Sym == null)
            {
                d.Sym = ref new symbolizer.Symbolizer(Obj:d.Obj,UI:d.UI);
            }
            return d;
        }

        // goFlags returns a flagset implementation based on the standard flag
        // package from the Go distribution. It implements the plugin.FlagSet
        // interface.
        private partial struct goFlags
        {
        }

        private static ref bool Bool(this goFlags _p0, @string o, bool d, @string c)
        {
            return flag.Bool(o, d, c);
        }

        private static ref long Int(this goFlags _p0, @string o, long d, @string c)
        {
            return flag.Int(o, d, c);
        }

        private static ref double Float64(this goFlags _p0, @string o, double d, @string c)
        {
            return flag.Float64(o, d, c);
        }

        private static ref @string String(this goFlags _p0, @string o, @string d, @string c)
        {
            return flag.String(o, d, c);
        }

        private static void BoolVar(this goFlags _p0, ref bool b, @string o, bool d, @string c)
        {
            flag.BoolVar(b, o, d, c);
        }

        private static void IntVar(this goFlags _p0, ref long i, @string o, long d, @string c)
        {
            flag.IntVar(i, o, d, c);
        }

        private static void Float64Var(this goFlags _p0, ref double f, @string o, double d, @string c)
        {
            flag.Float64Var(f, o, d, c);
        }

        private static void StringVar(this goFlags _p0, ref @string s, @string o, @string d, @string c)
        {
            flag.StringVar(s, o, d, c);
        }

        private static ref slice<ref @string> StringList(this goFlags _p0, @string o, @string d, @string c)
        {
            return ref new slice<ref @string>(new ref @string[] { flag.String(o,d,c) });
        }

        private static @string ExtraUsage(this goFlags _p0)
        {
            return "";
        }

        private static slice<@string> Parse(this goFlags _p0, Action usage)
        {
            flag.Usage = usage;
            flag.Parse();
            var args = flag.Args();
            if (len(args) == 0L)
            {
                usage();
            }
            return args;
        }

        private partial struct stdUI
        {
            public ptr<bufio.Reader> r;
        }

        private static (@string, error) ReadLine(this ref stdUI ui, @string prompt)
        {
            os.Stdout.WriteString(prompt);
            return ui.r.ReadString('\n');
        }

        private static void Print(this ref stdUI ui, params object[] args)
        {
            ui.fprint(os.Stderr, args);
        }

        private static void PrintErr(this ref stdUI ui, params object[] args)
        {
            ui.fprint(os.Stderr, args);
        }

        private static bool IsTerminal(this ref stdUI ui)
        {
            return false;
        }

        private static @string SetAutoComplete(this ref stdUI ui, Func<@string, @string> _p0)
        {
        }

        private static void fprint(this ref stdUI ui, ref os.File f, slice<object> args)
        {
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
            var (f, err) = os.Create(name);
            return (f, err);
        }
    }
}}}}}}}
