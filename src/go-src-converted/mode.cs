// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 09 06:03:24 UTC
// import "golang.org/x/tools/go/ssa" ==> using ssa = go.golang.org.x.tools.go.ssa_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\mode.go
// This file defines the BuilderMode type and its command-line flag.

using bytes = go.bytes_package;
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go
{
    public static partial class ssa_package
    {
        // BuilderMode is a bitmask of options for diagnostics and checking.
        //
        // *BuilderMode satisfies the flag.Value interface.  Example:
        //
        //     var mode = ssa.BuilderMode(0)
        //     func init() { flag.Var(&mode, "build", ssa.BuilderModeDoc) }
        //
        public partial struct BuilderMode // : ulong
        {
        }

        public static readonly BuilderMode PrintPackages = (BuilderMode)1L << (int)(iota); // Print package inventory to stdout
        public static readonly var PrintFunctions = 0; // Print function SSA code to stdout
        public static readonly var LogSource = 1; // Log source locations as SSA builder progresses
        public static readonly var SanityCheckFunctions = 2; // Perform sanity checking of function bodies
        public static readonly var NaiveForm = 3; // Build naïve SSA form: don't replace local loads/stores with registers
        public static readonly var BuildSerially = 4; // Build packages serially, not in parallel.
        public static readonly var GlobalDebug = 5; // Enable debug info for all packages
        public static readonly var BareInits = 6; // Build init functions without guards or calls to dependent inits

        public static readonly @string BuilderModeDoc = (@string)@"Options controlling the SSA builder.
The value is a sequence of zero or more of these letters:
C	perform sanity [C]hecking of the SSA form.
D	include [D]ebug info for every function.
P	print [P]ackage inventory.
F	print [F]unction SSA code.
S	log [S]ource locations as SSA builder progresses.
L	build distinct packages seria[L]ly instead of in parallel.
N	build [N]aive SSA form: don't replace local loads/stores with registers.
I	build bare [I]nit functions: no init guards or calls to dependent inits.
";



        public static @string String(this BuilderMode m)
        {
            bytes.Buffer buf = default;
            if (m & GlobalDebug != 0L)
            {
                buf.WriteByte('D');
            }

            if (m & PrintPackages != 0L)
            {
                buf.WriteByte('P');
            }

            if (m & PrintFunctions != 0L)
            {
                buf.WriteByte('F');
            }

            if (m & LogSource != 0L)
            {
                buf.WriteByte('S');
            }

            if (m & SanityCheckFunctions != 0L)
            {
                buf.WriteByte('C');
            }

            if (m & NaiveForm != 0L)
            {
                buf.WriteByte('N');
            }

            if (m & BuildSerially != 0L)
            {
                buf.WriteByte('L');
            }

            return buf.String();

        }

        // Set parses the flag characters in s and updates *m.
        private static error Set(this ptr<BuilderMode> _addr_m, @string s)
        {
            ref BuilderMode m = ref _addr_m.val;

            BuilderMode mode = default;
            foreach (var (_, c) in s)
            {
                switch (c)
                {
                    case 'D': 
                        mode |= GlobalDebug;
                        break;
                    case 'P': 
                        mode |= PrintPackages;
                        break;
                    case 'F': 
                        mode |= PrintFunctions;
                        break;
                    case 'S': 
                        mode |= LogSource | BuildSerially;
                        break;
                    case 'C': 
                        mode |= SanityCheckFunctions;
                        break;
                    case 'N': 
                        mode |= NaiveForm;
                        break;
                    case 'L': 
                        mode |= BuildSerially;
                        break;
                    default: 
                        return error.As(fmt.Errorf("unknown BuilderMode option: %q", c))!;
                        break;
                }

            }
            m.val = mode;
            return error.As(null!)!;

        }

        // Get returns m.
        public static void Get(this BuilderMode m)
        {
            return m;
        }
    }
}}}}}
