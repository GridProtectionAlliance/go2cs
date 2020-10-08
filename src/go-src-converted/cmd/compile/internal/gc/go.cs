// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 08 04:28:58 UTC
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
        public static readonly var BADWIDTH = (var)types.BADWIDTH;


 
        // maximum size variable which we will allocate on the stack.
        // This limit is for explicit variable declarations like "var x T" or "x := ...".
        // Note: the flag smallframes can update this value.
        private static var maxStackVarSize = int64(10L * 1024L * 1024L);        private static var maxImplicitStackVarSize = int64(64L * 1024L);        private static var smallArrayBytes = int64(256L);

        // isRuntimePkg reports whether p is package runtime.
        private static bool isRuntimePkg(ptr<types.Pkg> _addr_p)
        {
            ref types.Pkg p = ref _addr_p.val;

            if (compiling_runtime && p == localpkg)
            {
                return true;
            }

            return p.Path == "runtime";

        }

        // isReflectPkg reports whether p is package reflect.
        private static bool isReflectPkg(ptr<types.Pkg> _addr_p)
        {
            ref types.Pkg p = ref _addr_p.val;

            if (p == localpkg)
            {
                return myimportpath == "reflect";
            }

            return p.Path == "reflect";

        }

        // The Class of a variable/function describes the "storage class"
        // of a variable or function. During parsing, storage classes are
        // called declaration contexts.
        public partial struct Class // : byte
        {
        }

        //go:generate stringer -type=Class
        public static readonly Class Pxxx = (Class)iota; // no class; used during ssa conversion to indicate pseudo-variables
        public static readonly var PEXTERN = (var)0; // global variable
        public static readonly var PAUTO = (var)1; // local variables
        public static readonly var PAUTOHEAP = (var)2; // local variable or parameter moved to heap
        public static readonly var PPARAM = (var)3; // input arguments
        public static readonly var PPARAMOUT = (var)4; // output results
        public static readonly _ PFUNC = (_)uint((1L << (int)(3L)) - iota); // static assert for iota <= (1 << 3)

        // Slices in the runtime are represented by three components:
        //
        // type slice struct {
        //     ptr unsafe.Pointer
        //     len int
        //     cap int
        // }
        //
        // Strings in the runtime are represented by two components:
        //
        // type string struct {
        //     ptr unsafe.Pointer
        //     len int
        // }
        //
        // These variables are the offsets of fields and sizes of these structs.
        private static long slicePtrOffset = default;        private static long sliceLenOffset = default;        private static long sliceCapOffset = default;        private static long sizeofSlice = default;        private static long sizeofString = default;

        private static slice<slice<@string>> pragcgobuf = default;

        private static @string outfile = default;
        private static @string linkobj = default;

        // nerrors is the number of compiler errors reported
        // since the last call to saveerrors.
        private static long nerrors = default;

        // nsavederrors is the total number of compiler errors
        // reported before the last call to saveerrors.
        private static long nsavederrors = default;

        private static long nsyntaxerrors = default;

        private static int decldepth = default;

        private static bool nolocalimports = default;

        public static array<long> Debug = new array<long>(256L);

        private static @string debugstr = default;

        public static long Debug_checknil = default;
        public static long Debug_typeassert = default;

        private static ptr<types.Pkg> localpkg; // package being compiled

        private static bool inimport = default; // set during import

        private static ptr<types.Pkg> itabpkg; // fake pkg for itab entries

        private static ptr<types.Pkg> itablinkpkg; // fake package for runtime itab entries

        public static ptr<types.Pkg> Runtimepkg; // fake package runtime

        private static ptr<types.Pkg> racepkg; // package runtime/race

        private static ptr<types.Pkg> msanpkg; // package runtime/msan

        private static ptr<types.Pkg> unsafepkg; // package unsafe

        private static ptr<types.Pkg> trackpkg; // fake package for field tracking

        private static ptr<types.Pkg> mappkg; // fake package for map zero value

        private static ptr<types.Pkg> gopkg; // pseudo-package for method symbols on anonymous receiver types

        private static long zerosize = default;

        private static @string myimportpath = default;

        private static @string localimport = default;

        private static @string asmhdr = default;

        private static array<types.EType> simtype = new array<types.EType>(NTYPE);

        private static array<bool> isInt = new array<bool>(NTYPE);        private static array<bool> isFloat = new array<bool>(NTYPE);        private static array<bool> isComplex = new array<bool>(NTYPE);        private static array<bool> issimple = new array<bool>(NTYPE);

        private static array<bool> okforeq = new array<bool>(NTYPE);        private static array<bool> okforadd = new array<bool>(NTYPE);        private static array<bool> okforand = new array<bool>(NTYPE);        private static array<bool> okfornone = new array<bool>(NTYPE);        private static array<bool> okforcmp = new array<bool>(NTYPE);        private static array<bool> okforbool = new array<bool>(NTYPE);        private static array<bool> okforcap = new array<bool>(NTYPE);        private static array<bool> okforlen = new array<bool>(NTYPE);        private static array<bool> okforarith = new array<bool>(NTYPE);        private static array<bool> okforconst = new array<bool>(NTYPE);

        private static array<slice<bool>> okfor = new array<slice<bool>>(OEND);        private static array<bool> iscmp = new array<bool>(OEND);

        private static array<ptr<Mpint>> minintval = new array<ptr<Mpint>>(NTYPE);

        private static array<ptr<Mpint>> maxintval = new array<ptr<Mpint>>(NTYPE);

        private static array<ptr<Mpflt>> minfltval = new array<ptr<Mpflt>>(NTYPE);

        private static array<ptr<Mpflt>> maxfltval = new array<ptr<Mpflt>>(NTYPE);

        private static slice<ptr<Node>> xtop = default;

        private static slice<ptr<Node>> exportlist = default;

        private static slice<ptr<Node>> importlist = default; // imported functions and methods with inlinable bodies

        private static sync.Mutex funcsymsmu = default;        private static slice<ptr<types.Sym>> funcsyms = default;

        private static Class dclcontext = default; // PEXTERN/PAUTO

        public static ptr<Node> Curfn;

        public static long Widthptr = default;

        public static long Widthreg = default;

        private static ptr<Node> nblank;

        private static bool typecheckok = default;

        private static bool compiling_runtime = default;

        // Compiling the standard library
        private static bool compiling_std = default;

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

        public static ptr<obj.Link> Ctxt;

        private static bool writearchive = default;

        private static ptr<Node> nodfp;

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
            public Func<long, long> PadFrame; // ZeroRange zeroes a range of memory on stack. It is only inserted
// at function entry, and it is ok to clobber registers.
            public Func<ptr<Progs>, ptr<obj.Prog>, long, long, ptr<uint>, ptr<obj.Prog>> ZeroRange;
            public Func<ptr<Progs>, ptr<obj.Prog>> Ginsnop;
            public Func<ptr<Progs>, ptr<obj.Prog>> Ginsnopdefer; // special ginsnop for deferreturn

// SSAMarkMoves marks any MOVXconst ops that need to avoid clobbering flags.
            public Action<ptr<SSAGenState>, ptr<ssa.Block>> SSAMarkMoves; // SSAGenValue emits Prog(s) for the Value.
            public Action<ptr<SSAGenState>, ptr<ssa.Value>> SSAGenValue; // SSAGenBlock emits end-of-block Progs. SSAGenValue should be called
// for all values in the block before SSAGenBlock.
            public Action<ptr<SSAGenState>, ptr<ssa.Block>, ptr<ssa.Block>> SSAGenBlock;
        }

        private static Arch thearch = default;

        private static ptr<Node> staticuint64s;        private static ptr<Node> zerobase;

        private static ptr<obj.LSym> assertE2I;        private static ptr<obj.LSym> assertE2I2;        private static ptr<obj.LSym> assertI2I;        private static ptr<obj.LSym> assertI2I2;        private static ptr<obj.LSym> deferproc;        private static ptr<obj.LSym> deferprocStack;        public static ptr<obj.LSym> Deferreturn;        public static ptr<obj.LSym> Duffcopy;        public static ptr<obj.LSym> Duffzero;        private static ptr<obj.LSym> gcWriteBarrier;        private static ptr<obj.LSym> goschedguarded;        private static ptr<obj.LSym> growslice;        private static ptr<obj.LSym> msanread;        private static ptr<obj.LSym> msanwrite;        private static ptr<obj.LSym> newobject;        private static ptr<obj.LSym> newproc;        private static ptr<obj.LSym> panicdivide;        private static ptr<obj.LSym> panicshift;        private static ptr<obj.LSym> panicdottypeE;        private static ptr<obj.LSym> panicdottypeI;        private static ptr<obj.LSym> panicnildottype;        private static ptr<obj.LSym> panicoverflow;        private static ptr<obj.LSym> raceread;        private static ptr<obj.LSym> racereadrange;        private static ptr<obj.LSym> racewrite;        private static ptr<obj.LSym> racewriterange;        private static ptr<obj.LSym> x86HasPOPCNT;        private static ptr<obj.LSym> x86HasSSE41;        private static ptr<obj.LSym> x86HasFMA;        private static ptr<obj.LSym> armHasVFPv4;        private static ptr<obj.LSym> arm64HasATOMICS;        private static ptr<obj.LSym> typedmemclr;        private static ptr<obj.LSym> typedmemmove;        public static ptr<obj.LSym> Udiv;        private static ptr<obj.LSym> writeBarrier;        private static ptr<obj.LSym> zerobaseSym;

        public static array<ptr<obj.LSym>> BoundsCheckFunc = new array<ptr<obj.LSym>>(ssa.BoundsKindCount);        public static array<ptr<obj.LSym>> ExtendCheckFunc = new array<ptr<obj.LSym>>(ssa.BoundsKindCount);        public static ptr<obj.LSym> ControlWord64trunc;        public static ptr<obj.LSym> ControlWord32; 

        // Wasm
        public static ptr<obj.LSym> WasmMove;        public static ptr<obj.LSym> WasmZero;        public static ptr<obj.LSym> WasmDiv;        public static ptr<obj.LSym> WasmTruncS;        public static ptr<obj.LSym> WasmTruncU;        public static ptr<obj.LSym> SigPanic;


        // GCWriteBarrierReg maps from registers to gcWriteBarrier implementation LSyms.
        public static map<short, ptr<obj.LSym>> GCWriteBarrierReg = default;
    }
}}}}
