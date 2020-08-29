// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 09:27:07 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\go.go
using ssa = go.cmd.compile.@internal.ssa_package;
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using src = go.cmd.@internal.src_package;
using sync = go.sync_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        public static readonly var BADWIDTH = types.BADWIDTH;
        private static readonly long maxStackVarSize = 10L * 1024L * 1024L;

        // isRuntimePkg reports whether p is package runtime.
        private static bool isRuntimePkg(ref types.Pkg p)
        {
            if (compiling_runtime && p == localpkg)
            {
                return true;
            }
            return p.Path == "runtime";
        }

        // The Class of a variable/function describes the "storage class"
        // of a variable or function. During parsing, storage classes are
        // called declaration contexts.
        public partial struct Class // : byte
        {
        }

        //go:generate stringer -type=Class
        public static readonly Class Pxxx = iota; // no class; used during ssa conversion to indicate pseudo-variables
        public static readonly var PEXTERN = 0; // global variable
        public static readonly var PAUTO = 1; // local variables
        public static readonly var PAUTOHEAP = 2; // local variable or parameter moved to heap
        public static readonly var PPARAM = 3; // input arguments
        public static readonly var PPARAMOUT = 4; // output results
        public static readonly var PFUNC = 5; // global function

        public static readonly var PDISCARD = 6; // discard during parse of duplicate import
        // Careful: Class is stored in three bits in Node.flags.
        // Adding a new Class will overflow that.

        private static void init() => func((_, panic, __) =>
        {
            if (PDISCARD != 7L)
            {
                panic("PDISCARD changed; does all Class values still fit in three bits?");
            }
        });

        // note this is the runtime representation
        // of the compilers arrays.
        //
        // typedef    struct
        // {                // must not move anything
        //     uchar    array[8];    // pointer to data
        //     uchar    nel[4];        // number of elements
        //     uchar    cap[4];        // allocated number of elements
        // } Array;
        private static long array_array = default; // runtime offsetof(Array,array) - same for String

        private static long array_nel = default; // runtime offsetof(Array,nel) - same for String

        private static long array_cap = default; // runtime offsetof(Array,cap)

        private static long sizeof_Array = default; // runtime sizeof(Array)

        // note this is the runtime representation
        // of the compilers strings.
        //
        // typedef    struct
        // {                // must not move anything
        //     uchar    array[8];    // pointer to data
        //     uchar    nel[4];        // number of elements
        // } String;
        private static long sizeof_String = default; // runtime sizeof(String)

        private static @string pragcgobuf = default;

        private static @string outfile = default;
        private static @string linkobj = default;
        private static bool dolinkobj = default;

        // nerrors is the number of compiler errors reported
        // since the last call to saveerrors.
        private static long nerrors = default;

        // nsavederrors is the total number of compiler errors
        // reported before the last call to saveerrors.
        private static long nsavederrors = default;

        private static long nsyntaxerrors = default;

        private static int decldepth = default;

        private static bool safemode = default;

        private static bool nolocalimports = default;

        public static array<long> Debug = new array<long>(256L);

        private static @string debugstr = default;

        public static long Debug_checknil = default;
        public static long Debug_typeassert = default;

        private static ref types.Pkg localpkg = default; // package being compiled

        private static bool inimport = default; // set during import

        private static ref types.Pkg itabpkg = default; // fake pkg for itab entries

        private static ref types.Pkg itablinkpkg = default; // fake package for runtime itab entries

        public static ref types.Pkg Runtimepkg = default; // fake package runtime

        private static ref types.Pkg racepkg = default; // package runtime/race

        private static ref types.Pkg msanpkg = default; // package runtime/msan

        private static ref types.Pkg unsafepkg = default; // package unsafe

        private static ref types.Pkg trackpkg = default; // fake package for field tracking

        private static ref types.Pkg mappkg = default; // fake package for map zero value
        private static long zerosize = default;

        private static @string myimportpath = default;

        private static @string localimport = default;

        private static @string asmhdr = default;

        private static array<types.EType> simtype = new array<types.EType>(NTYPE);

        private static array<bool> isforw = new array<bool>(NTYPE);        private static array<bool> isInt = new array<bool>(NTYPE);        private static array<bool> isFloat = new array<bool>(NTYPE);        private static array<bool> isComplex = new array<bool>(NTYPE);        private static array<bool> issimple = new array<bool>(NTYPE);

        private static array<bool> okforeq = new array<bool>(NTYPE);        private static array<bool> okforadd = new array<bool>(NTYPE);        private static array<bool> okforand = new array<bool>(NTYPE);        private static array<bool> okfornone = new array<bool>(NTYPE);        private static array<bool> okforcmp = new array<bool>(NTYPE);        private static array<bool> okforbool = new array<bool>(NTYPE);        private static array<bool> okforcap = new array<bool>(NTYPE);        private static array<bool> okforlen = new array<bool>(NTYPE);        private static array<bool> okforarith = new array<bool>(NTYPE);        private static array<bool> okforconst = new array<bool>(NTYPE);

        private static array<slice<bool>> okfor = new array<slice<bool>>(OEND);        private static array<bool> iscmp = new array<bool>(OEND);

        private static array<ref Mpint> minintval = new array<ref Mpint>(NTYPE);

        private static array<ref Mpint> maxintval = new array<ref Mpint>(NTYPE);

        private static array<ref Mpflt> minfltval = new array<ref Mpflt>(NTYPE);

        private static array<ref Mpflt> maxfltval = new array<ref Mpflt>(NTYPE);

        private static slice<ref Node> xtop = default;

        private static slice<ref Node> exportlist = default;

        private static slice<ref Node> importlist = default; // imported functions and methods with inlinable bodies

        private static sync.Mutex funcsymsmu = default;        private static slice<ref types.Sym> funcsyms = default;

        private static Class dclcontext = default; // PEXTERN/PAUTO

        public static ref Node Curfn = default;

        public static long Widthptr = default;

        public static long Widthreg = default;

        private static ref Node nblank = default;

        private static bool typecheckok = default;

        private static bool compiling_runtime = default;

        // Compiling the standard library
        private static bool compiling_std = default;

        private static bool compiling_wrappers = default;

        private static bool use_writebarrier = default;

        private static bool pure_go = default;

        private static @string flag_installsuffix = default;

        private static bool flag_race = default;

        private static bool flag_msan = default;

        private static bool flagDWARF = default;

        // Whether we are adding any sort of code instrumentation, such as
        // when the race detector is enabled.
        private static bool instrumenting = default;

        // Whether we are tracking lexical scopes for DWARF.
        private static bool trackScopes = default;

        // Controls generation of DWARF inlined instance records. Zero
        // disables, 1 emits inlined routines but suppresses var info,
        // and 2 emits inlined routines with tracking of formals/locals.
        private static long genDwarfInline = default;

        private static long debuglive = default;

        public static ref obj.Link Ctxt = default;

        private static bool writearchive = default;

        public static bool Nacl = default;

        private static ref Node nodfp = default;

        private static long disable_checknil = default;

        private static src.XPos autogeneratedPos = default;

        // interface to back end

        public partial struct Arch
        {
            public ptr<obj.LinkArch> LinkArch;
            public long REGSP;
            public long MAXWIDTH;
            public bool Use387; // should 386 backend use 387 FP instructions instead of sse2.
            public bool SoftFloat;
            public Func<long, long> PadFrame;
            public Func<ref Progs, ref obj.Prog, long, long, ref uint, ref obj.Prog> ZeroRange;
            public Action<ref Progs> Ginsnop; // SSAMarkMoves marks any MOVXconst ops that need to avoid clobbering flags.
            public Action<ref SSAGenState, ref ssa.Block> SSAMarkMoves; // SSAGenValue emits Prog(s) for the Value.
            public Action<ref SSAGenState, ref ssa.Value> SSAGenValue; // SSAGenBlock emits end-of-block Progs. SSAGenValue should be called
// for all values in the block before SSAGenBlock.
            public Action<ref SSAGenState, ref ssa.Block, ref ssa.Block> SSAGenBlock; // ZeroAuto emits code to zero the given auto stack variable.
// ZeroAuto must not use any non-temporary registers.
// ZeroAuto will only be called for variables which contain a pointer.
            public Action<ref Progs, ref Node> ZeroAuto;
        }

        private static Arch thearch = default;

        private static ref Node staticbytes = default;        private static ref Node zerobase = default;

        public static ref obj.LSym Newproc = default;        public static ref obj.LSym Deferproc = default;        public static ref obj.LSym Deferreturn = default;        public static ref obj.LSym Duffcopy = default;        public static ref obj.LSym Duffzero = default;        private static ref obj.LSym panicindex = default;        private static ref obj.LSym panicslice = default;        private static ref obj.LSym panicdivide = default;        private static ref obj.LSym growslice = default;        private static ref obj.LSym panicdottypeE = default;        private static ref obj.LSym panicdottypeI = default;        private static ref obj.LSym panicnildottype = default;        private static ref obj.LSym assertE2I = default;        private static ref obj.LSym assertE2I2 = default;        private static ref obj.LSym assertI2I = default;        private static ref obj.LSym assertI2I2 = default;        private static ref obj.LSym goschedguarded = default;        private static ref obj.LSym writeBarrier = default;        private static ref obj.LSym writebarrierptr = default;        private static ref obj.LSym gcWriteBarrier = default;        private static ref obj.LSym typedmemmove = default;        private static ref obj.LSym typedmemclr = default;        public static ref obj.LSym Udiv = default; 

        // GO386=387
        public static ref obj.LSym ControlWord64trunc = default;        public static ref obj.LSym ControlWord32 = default;
    }
}}}}
