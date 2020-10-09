//  Copyright 2018 Google Inc. All Rights Reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

// package driver -- go2cs converted at 2020 October 09 05:53:29 UTC
// import "cmd/vendor/github.com/google/pprof/internal/driver" ==> using driver = go.cmd.vendor.github.com.google.pprof.@internal.driver_package
// Original source: C:\Go\src\cmd\vendor\github.com\google\pprof\internal\driver\flags.go
using flag = go.flag_package;
using strings = go.strings_package;
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
        // GoFlags implements the plugin.FlagSet interface.
        public partial struct GoFlags
        {
            public slice<@string> UsageMsgs;
        }

        // Bool implements the plugin.FlagSet interface.
        private static ptr<bool> Bool(this ptr<GoFlags> _addr__p0, @string o, bool d, @string c)
        {
            ref GoFlags _p0 = ref _addr__p0.val;

            return _addr_flag.Bool(o, d, c)!;
        }

        // Int implements the plugin.FlagSet interface.
        private static ptr<long> Int(this ptr<GoFlags> _addr__p0, @string o, long d, @string c)
        {
            ref GoFlags _p0 = ref _addr__p0.val;

            return _addr_flag.Int(o, d, c)!;
        }

        // Float64 implements the plugin.FlagSet interface.
        private static ptr<double> Float64(this ptr<GoFlags> _addr__p0, @string o, double d, @string c)
        {
            ref GoFlags _p0 = ref _addr__p0.val;

            return _addr_flag.Float64(o, d, c)!;
        }

        // String implements the plugin.FlagSet interface.
        private static ptr<@string> String(this ptr<GoFlags> _addr__p0, @string o, @string d, @string c)
        {
            ref GoFlags _p0 = ref _addr__p0.val;

            return _addr_flag.String(o, d, c)!;
        }

        // StringList implements the plugin.FlagSet interface.
        private static ptr<slice<ptr<@string>>> StringList(this ptr<GoFlags> _addr__p0, @string o, @string d, @string c)
        {
            ref GoFlags _p0 = ref _addr__p0.val;

            return addr(new slice<ptr<@string>>(new ptr<@string>[] { flag.String(o,d,c) }));
        }

        // ExtraUsage implements the plugin.FlagSet interface.
        private static @string ExtraUsage(this ptr<GoFlags> _addr_f)
        {
            ref GoFlags f = ref _addr_f.val;

            return strings.Join(f.UsageMsgs, "\n");
        }

        // AddExtraUsage implements the plugin.FlagSet interface.
        private static void AddExtraUsage(this ptr<GoFlags> _addr_f, @string eu)
        {
            ref GoFlags f = ref _addr_f.val;

            f.UsageMsgs = append(f.UsageMsgs, eu);
        }

        // Parse implements the plugin.FlagSet interface.
        private static slice<@string> Parse(this ptr<GoFlags> _addr__p0, Action usage)
        {
            ref GoFlags _p0 = ref _addr__p0.val;

            flag.Usage = usage;
            flag.Parse();
            var args = flag.Args();
            if (len(args) == 0L)
            {
                usage();
            }

            return args;

        }
    }
}}}}}}}
