// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 09 05:42:23 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\racewalk.go
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
using sys = go.cmd.@internal.sys_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // The racewalk pass is currently handled in three parts.
        //
        // First, for flag_race, it inserts calls to racefuncenter and
        // racefuncexit at the start and end (respectively) of each
        // function. This is handled below.
        //
        // Second, during buildssa, it inserts appropriate instrumentation
        // calls immediately before each memory load or store. This is handled
        // by the (*state).instrument method in ssa.go, so here we just set
        // the Func.InstrumentBody flag as needed. For background on why this
        // is done during SSA construction rather than a separate SSA pass,
        // see issue #19054.
        //
        // Third we remove calls to racefuncenter and racefuncexit, for leaf
        // functions without instrumented operations. This is done as part of
        // ssa opt pass via special rule.

        // TODO(dvyukov): do not instrument initialization as writes:
        // a := make([]int, 10)

        // Do not instrument the following packages at all,
        // at best instrumentation would cause infinite recursion.
        private static @string omit_pkgs = new slice<@string>(new @string[] { "runtime/internal/atomic", "runtime/internal/sys", "runtime/internal/math", "runtime", "runtime/race", "runtime/msan", "internal/cpu" });

        // Only insert racefuncenterfp/racefuncexit into the following packages.
        // Memory accesses in the packages are either uninteresting or will cause false positives.
        private static @string norace_inst_pkgs = new slice<@string>(new @string[] { "sync", "sync/atomic" });

        private static bool ispkgin(slice<@string> pkgs)
        {
            if (myimportpath != "")
            {
                foreach (var (_, p) in pkgs)
                {
                    if (myimportpath == p)
                    {
                        return true;
                    }

                }

            }

            return false;

        }

        private static void instrument(ptr<Node> _addr_fn)
        {
            ref Node fn = ref _addr_fn.val;

            if (fn.Func.Pragma & Norace != 0L)
            {
                return ;
            }

            if (!flag_race || !ispkgin(norace_inst_pkgs))
            {
                fn.Func.SetInstrumentBody(true);
            }

            if (flag_race)
            {
                var lno = lineno;
                lineno = src.NoXPos;

                if (thearch.LinkArch.Arch.Family != sys.AMD64)
                {
                    fn.Func.Enter.Prepend(mkcall("racefuncenterfp", null, null));
                    fn.Func.Exit.Append(mkcall("racefuncexit", null, null));
                }
                else
                {
                    // nodpc is the PC of the caller as extracted by
                    // getcallerpc. We use -widthptr(FP) for x86.
                    // This only works for amd64. This will not
                    // work on arm or others that might support
                    // race in the future.
                    var nodpc = nodfp.copy();
                    nodpc.Type = types.Types[TUINTPTR];
                    nodpc.Xoffset = int64(-Widthptr);
                    fn.Func.Dcl = append(fn.Func.Dcl, nodpc);
                    fn.Func.Enter.Prepend(mkcall("racefuncenter", null, null, nodpc));
                    fn.Func.Exit.Append(mkcall("racefuncexit", null, null));

                }

                lineno = lno;

            }

        }
    }
}}}}
