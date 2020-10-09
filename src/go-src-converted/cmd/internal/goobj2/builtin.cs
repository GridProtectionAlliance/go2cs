// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package goobj2 -- go2cs converted at 2020 October 09 05:08:46 UTC
// import "cmd/internal/goobj2" ==> using goobj2 = go.cmd.@internal.goobj2_package
// Original source: C:\Go\src\cmd\internal\goobj2\builtin.go

using static go.builtin;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class goobj2_package
    {
        // Builtin (compiler-generated) function references appear
        // frequently. We assign special indices for them, so they
        // don't need to be referenced by name.

        // NBuiltin returns the number of listed builtin
        // symbols.
        public static long NBuiltin()
        {
            return len(builtins);
        }

        // BuiltinName returns the name and ABI of the i-th
        // builtin symbol.
        public static (@string, long) BuiltinName(long i)
        {
            @string _p0 = default;
            long _p0 = default;

            return (builtins[i].name, builtins[i].abi);
        }

        // BuiltinIdx returns the index of the builtin with the
        // given name and abi, or -1 if it is not a builtin.
        public static long BuiltinIdx(@string name, long abi)
        {
            var (i, ok) = builtinMap[name];
            if (!ok)
            {
                return -1L;
            }

            if (builtins[i].abi != abi)
            {
                return -1L;
            }

            return i;

        }

        //go:generate go run mkbuiltin.go

        private static map<@string, long> builtinMap = default;

        private static void init()
        {
            builtinMap = make_map<@string, long>(len(builtins));
            foreach (var (i, b) in builtins)
            {
                builtinMap[b.name] = i;
            }

        }
    }
}}}
