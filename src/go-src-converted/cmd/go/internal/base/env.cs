// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package @base -- go2cs converted at 2020 August 29 10:01:59 UTC
// import "cmd/go/internal/base" ==> using @base = go.cmd.go.@internal.@base_package
// Original source: C:\Go\src\cmd\go\internal\base\env.go
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class @base_package
    {
        // EnvForDir returns a copy of the environment
        // suitable for running in the given directory.
        // The environment is the current process's environment
        // but with an updated $PWD, so that an os.Getwd in the
        // child will be faster.
        public static slice<@string> EnvForDir(@string dir, slice<@string> @base)
        { 
            // Internally we only use rooted paths, so dir is rooted.
            // Even if dir is not rooted, no harm done.
            return MergeEnvLists(new slice<@string>(new @string[] { "PWD="+dir }), base);
        }

        // MergeEnvLists merges the two environment lists such that
        // variables with the same name in "in" replace those in "out".
        // This always returns a newly allocated slice.
        public static slice<@string> MergeEnvLists(slice<@string> @in, slice<@string> @out)
        {
            out = append((slice<@string>)null, out);
NextVar:
            foreach (var (_, inkv) in in)
            {
                var k = strings.SplitAfterN(inkv, "=", 2L)[0L];
                foreach (var (i, outkv) in out)
                {
                    if (strings.HasPrefix(outkv, k))
                    {
                        out[i] = inkv;
                        _continueNextVar = true;
                        break;
                    }
                }
                out = append(out, inkv);
            }
            return out;
        }
    }
}}}}
