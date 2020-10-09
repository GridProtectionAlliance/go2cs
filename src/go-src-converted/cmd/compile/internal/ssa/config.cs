// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 09 05:24:27 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\config.go
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class ssa_package
    {
        // A Config holds readonly compilation information.
        // It is created once, early during compilation,
        // and shared across all compilations.
        public partial struct Config
        {
            public @string arch; // "amd64", etc.
            public long PtrSize; // 4 or 8; copy of cmd/internal/sys.Arch.PtrSize
            public long RegSize; // 4 or 8; copy of cmd/internal/sys.Arch.RegSize
            public Types Types;
            public blockRewriter lowerBlock; // lowering function
            public valueRewriter lowerValue; // lowering function
            public valueRewriter splitLoad; // function for splitting merged load ops; only used on some architectures
            public slice<Register> registers; // machine registers
            public regMask gpRegMask; // general purpose integer register mask
            public regMask fpRegMask; // floating point register mask
            public regMask fp32RegMask; // floating point register mask
            public regMask fp64RegMask; // floating point register mask
            public regMask specialRegMask; // special register mask
            public slice<ptr<Register>> GCRegMap; // garbage collector register map, by GC register index
            public sbyte FPReg; // register number of frame pointer, -1 if not used
            public sbyte LinkReg; // register number of link register if it is a general purpose register, -1 if not used
            public bool hasGReg; // has hardware g register
            public ptr<obj.Link> ctxt; // Generic arch information
            public bool optimize; // Do optimization
            public bool noDuffDevice; // Don't use Duff's device
            public bool useSSE; // Use SSE for non-float operations
            public bool useAvg; // Use optimizations that need Avg* operations
            public bool useHmul; // Use optimizations that need Hmul* operations
            public bool use387; // GO386=387
            public bool SoftFloat; //
            public bool Race; // race detector enabled
            public bool NeedsFpScratch; // No direct move between GP and FP register sets
            public bool BigEndian; //
            public bool UseFMA; // Use hardware FMA operation
        }

        public delegate  bool blockRewriter(ptr<Block>);
        public delegate  bool valueRewriter(ptr<Value>);
        public partial struct Types
        {
            public ptr<types.Type> Bool;
            public ptr<types.Type> Int8;
            public ptr<types.Type> Int16;
            public ptr<types.Type> Int32;
            public ptr<types.Type> Int64;
            public ptr<types.Type> UInt8;
            public ptr<types.Type> UInt16;
            public ptr<types.Type> UInt32;
            public ptr<types.Type> UInt64;
            public ptr<types.Type> Int;
            public ptr<types.Type> Float32;
            public ptr<types.Type> Float64;
            public ptr<types.Type> UInt;
            public ptr<types.Type> Uintptr;
            public ptr<types.Type> String;
            public ptr<types.Type> BytePtr; // TODO: use unsafe.Pointer instead?
            public ptr<types.Type> Int32Ptr;
            public ptr<types.Type> UInt32Ptr;
            public ptr<types.Type> IntPtr;
            public ptr<types.Type> UintptrPtr;
            public ptr<types.Type> Float32Ptr;
            public ptr<types.Type> Float64Ptr;
            public ptr<types.Type> BytePtrPtr;
        }

        // NewTypes creates and populates a Types.
        public static ptr<Types> NewTypes()
        {
            ptr<Types> t = @new<Types>();
            t.SetTypPtrs();
            return _addr_t!;
        }

        // SetTypPtrs populates t.
        private static void SetTypPtrs(this ptr<Types> _addr_t)
        {
            ref Types t = ref _addr_t.val;

            t.Bool = types.Types[types.TBOOL];
            t.Int8 = types.Types[types.TINT8];
            t.Int16 = types.Types[types.TINT16];
            t.Int32 = types.Types[types.TINT32];
            t.Int64 = types.Types[types.TINT64];
            t.UInt8 = types.Types[types.TUINT8];
            t.UInt16 = types.Types[types.TUINT16];
            t.UInt32 = types.Types[types.TUINT32];
            t.UInt64 = types.Types[types.TUINT64];
            t.Int = types.Types[types.TINT];
            t.Float32 = types.Types[types.TFLOAT32];
            t.Float64 = types.Types[types.TFLOAT64];
            t.UInt = types.Types[types.TUINT];
            t.Uintptr = types.Types[types.TUINTPTR];
            t.String = types.Types[types.TSTRING];
            t.BytePtr = types.NewPtr(types.Types[types.TUINT8]);
            t.Int32Ptr = types.NewPtr(types.Types[types.TINT32]);
            t.UInt32Ptr = types.NewPtr(types.Types[types.TUINT32]);
            t.IntPtr = types.NewPtr(types.Types[types.TINT]);
            t.UintptrPtr = types.NewPtr(types.Types[types.TUINTPTR]);
            t.Float32Ptr = types.NewPtr(types.Types[types.TFLOAT32]);
            t.Float64Ptr = types.NewPtr(types.Types[types.TFLOAT64]);
            t.BytePtrPtr = types.NewPtr(types.NewPtr(types.Types[types.TUINT8]));
        }

        public partial interface Logger
        {
            bool Logf(@string _p0, params object _p0); // Log reports whether logging is not a no-op
// some logging calls account for more than a few heap allocations.
            bool Log(); // Fatal reports a compiler error and exits.
            bool Fatalf(src.XPos pos, @string msg, params object[] args); // Warnl writes compiler messages in the form expected by "errorcheck" tests
            bool Warnl(src.XPos pos, @string fmt_, params object[] args); // Forwards the Debug flags from gc
            bool Debug_checknil();
        }

        public partial interface Frontend : Logger
        {
            bool CanSSA(ptr<types.Type> t);
            bool StringData(@string _p0); // Auto returns a Node for an auto variable of the given type.
// The SSA compiler uses this function to allocate space for spills.
            bool Auto(src.XPos _p0, ptr<types.Type> _p0); // Given the name for a compound type, returns the name we should use
// for the parts of that compound type.
            bool SplitString(LocalSlot _p0);
            bool SplitInterface(LocalSlot _p0);
            bool SplitSlice(LocalSlot _p0);
            bool SplitComplex(LocalSlot _p0);
            bool SplitStruct(LocalSlot _p0, long _p0);
            bool SplitArray(LocalSlot _p0); // array must be length 1
            bool SplitInt64(LocalSlot _p0); // returns (hi, lo)

// DerefItab dereferences an itab function
// entry, given the symbol of the itab and
// the byte offset of the function pointer.
// It may return nil.
            bool DerefItab(ptr<obj.LSym> sym, long offset); // Line returns a string describing the given position.
            bool Line(src.XPos _p0); // AllocFrame assigns frame offsets to all live auto variables.
            bool AllocFrame(ptr<Func> f); // Syslook returns a symbol of the runtime function/variable with the
// given name.
            bool Syslook(@string _p0); // UseWriteBarrier reports whether write barrier is enabled
            bool UseWriteBarrier(); // SetWBPos indicates that a write barrier has been inserted
// in this function at position pos.
            bool SetWBPos(src.XPos pos);
        }

        // interface used to hold a *gc.Node (a stack variable).
        // We'd use *gc.Node directly but that would lead to an import cycle.
        public partial interface GCNode
        {
            StorageClass Typ();
            StorageClass String();
            StorageClass IsSynthetic();
            StorageClass IsAutoTmp();
            StorageClass StorageClass();
        }

        public partial struct StorageClass // : byte
        {
        }

        public static readonly StorageClass ClassAuto = (StorageClass)iota; // local stack variable
        public static readonly var ClassParam = 0; // argument
        public static readonly var ClassParamOut = 1; // return value

        // NewConfig returns a new configuration object for the given architecture.
        public static ptr<Config> NewConfig(@string arch, Types types, ptr<obj.Link> _addr_ctxt, bool optimize)
        {
            ref obj.Link ctxt = ref _addr_ctxt.val;

            ptr<Config> c = addr(new Config(arch:arch,Types:types));
            c.useAvg = true;
            c.useHmul = true;

            if (arch == "amd64")
            {
                c.PtrSize = 8L;
                c.RegSize = 8L;
                c.lowerBlock = rewriteBlockAMD64;
                c.lowerValue = rewriteValueAMD64;
                c.splitLoad = rewriteValueAMD64splitload;
                c.registers = registersAMD64[..];
                c.gpRegMask = gpRegMaskAMD64;
                c.fpRegMask = fpRegMaskAMD64;
                c.FPReg = framepointerRegAMD64;
                c.LinkReg = linkRegAMD64;
                c.hasGReg = false;
                goto __switch_break0;
            }
            if (arch == "386")
            {
                c.PtrSize = 4L;
                c.RegSize = 4L;
                c.lowerBlock = rewriteBlock386;
                c.lowerValue = rewriteValue386;
                c.splitLoad = rewriteValue386splitload;
                c.registers = registers386[..];
                c.gpRegMask = gpRegMask386;
                c.fpRegMask = fpRegMask386;
                c.FPReg = framepointerReg386;
                c.LinkReg = linkReg386;
                c.hasGReg = false;
                goto __switch_break0;
            }
            if (arch == "arm")
            {
                c.PtrSize = 4L;
                c.RegSize = 4L;
                c.lowerBlock = rewriteBlockARM;
                c.lowerValue = rewriteValueARM;
                c.registers = registersARM[..];
                c.gpRegMask = gpRegMaskARM;
                c.fpRegMask = fpRegMaskARM;
                c.FPReg = framepointerRegARM;
                c.LinkReg = linkRegARM;
                c.hasGReg = true;
                goto __switch_break0;
            }
            if (arch == "arm64")
            {
                c.PtrSize = 8L;
                c.RegSize = 8L;
                c.lowerBlock = rewriteBlockARM64;
                c.lowerValue = rewriteValueARM64;
                c.registers = registersARM64[..];
                c.gpRegMask = gpRegMaskARM64;
                c.fpRegMask = fpRegMaskARM64;
                c.FPReg = framepointerRegARM64;
                c.LinkReg = linkRegARM64;
                c.hasGReg = true;
                c.noDuffDevice = objabi.GOOS == "darwin"; // darwin linker cannot handle BR26 reloc with non-zero addend
                goto __switch_break0;
            }
            if (arch == "ppc64")
            {
                c.BigEndian = true;
                fallthrough = true;
            }
            if (fallthrough || arch == "ppc64le")
            {
                c.PtrSize = 8L;
                c.RegSize = 8L;
                c.lowerBlock = rewriteBlockPPC64;
                c.lowerValue = rewriteValuePPC64;
                c.registers = registersPPC64[..];
                c.gpRegMask = gpRegMaskPPC64;
                c.fpRegMask = fpRegMaskPPC64;
                c.FPReg = framepointerRegPPC64;
                c.LinkReg = linkRegPPC64;
                c.noDuffDevice = true; // TODO: Resolve PPC64 DuffDevice (has zero, but not copy)
                c.hasGReg = true;
                goto __switch_break0;
            }
            if (arch == "mips64")
            {
                c.BigEndian = true;
                fallthrough = true;
            }
            if (fallthrough || arch == "mips64le")
            {
                c.PtrSize = 8L;
                c.RegSize = 8L;
                c.lowerBlock = rewriteBlockMIPS64;
                c.lowerValue = rewriteValueMIPS64;
                c.registers = registersMIPS64[..];
                c.gpRegMask = gpRegMaskMIPS64;
                c.fpRegMask = fpRegMaskMIPS64;
                c.specialRegMask = specialRegMaskMIPS64;
                c.FPReg = framepointerRegMIPS64;
                c.LinkReg = linkRegMIPS64;
                c.hasGReg = true;
                goto __switch_break0;
            }
            if (arch == "s390x")
            {
                c.PtrSize = 8L;
                c.RegSize = 8L;
                c.lowerBlock = rewriteBlockS390X;
                c.lowerValue = rewriteValueS390X;
                c.registers = registersS390X[..];
                c.gpRegMask = gpRegMaskS390X;
                c.fpRegMask = fpRegMaskS390X;
                c.FPReg = framepointerRegS390X;
                c.LinkReg = linkRegS390X;
                c.hasGReg = true;
                c.noDuffDevice = true;
                c.BigEndian = true;
                goto __switch_break0;
            }
            if (arch == "mips")
            {
                c.BigEndian = true;
                fallthrough = true;
            }
            if (fallthrough || arch == "mipsle")
            {
                c.PtrSize = 4L;
                c.RegSize = 4L;
                c.lowerBlock = rewriteBlockMIPS;
                c.lowerValue = rewriteValueMIPS;
                c.registers = registersMIPS[..];
                c.gpRegMask = gpRegMaskMIPS;
                c.fpRegMask = fpRegMaskMIPS;
                c.specialRegMask = specialRegMaskMIPS;
                c.FPReg = framepointerRegMIPS;
                c.LinkReg = linkRegMIPS;
                c.hasGReg = true;
                c.noDuffDevice = true;
                goto __switch_break0;
            }
            if (arch == "riscv64")
            {
                c.PtrSize = 8L;
                c.RegSize = 8L;
                c.lowerBlock = rewriteBlockRISCV64;
                c.lowerValue = rewriteValueRISCV64;
                c.registers = registersRISCV64[..];
                c.gpRegMask = gpRegMaskRISCV64;
                c.fpRegMask = fpRegMaskRISCV64;
                c.FPReg = framepointerRegRISCV64;
                c.hasGReg = true;
                goto __switch_break0;
            }
            if (arch == "wasm")
            {
                c.PtrSize = 8L;
                c.RegSize = 8L;
                c.lowerBlock = rewriteBlockWasm;
                c.lowerValue = rewriteValueWasm;
                c.registers = registersWasm[..];
                c.gpRegMask = gpRegMaskWasm;
                c.fpRegMask = fpRegMaskWasm;
                c.fp32RegMask = fp32RegMaskWasm;
                c.fp64RegMask = fp64RegMaskWasm;
                c.FPReg = framepointerRegWasm;
                c.LinkReg = linkRegWasm;
                c.hasGReg = true;
                c.noDuffDevice = true;
                c.useAvg = false;
                c.useHmul = false;
                goto __switch_break0;
            }
            // default: 
                ctxt.Diag("arch %s not implemented", arch);

            __switch_break0:;
            c.ctxt = ctxt;
            c.optimize = optimize;
            c.useSSE = true;
            c.UseFMA = true; 

            // On Plan 9, floating point operations are not allowed in note handler.
            if (objabi.GOOS == "plan9")
            { 
                // Don't use FMA on Plan 9
                c.UseFMA = false; 

                // Don't use Duff's device and SSE on Plan 9 AMD64.
                if (arch == "amd64")
                {
                    c.noDuffDevice = true;
                    c.useSSE = false;
                }

            }

            if (ctxt.Flag_shared)
            { 
                // LoweredWB is secretly a CALL and CALLs on 386 in
                // shared mode get rewritten by obj6.go to go through
                // the GOT, which clobbers BX.
                opcodeTable[Op386LoweredWB].reg.clobbers |= 1L << (int)(3L); // BX
            } 

            // Create the GC register map index.
            // TODO: This is only used for debug printing. Maybe export config.registers?
            var gcRegMapSize = int16(0L);
            {
                var r__prev1 = r;

                foreach (var (_, __r) in c.registers)
                {
                    r = __r;
                    if (r.gcNum + 1L > gcRegMapSize)
                    {
                        gcRegMapSize = r.gcNum + 1L;
                    }

                }

                r = r__prev1;
            }

            c.GCRegMap = make_slice<ptr<Register>>(gcRegMapSize);
            {
                var r__prev1 = r;

                foreach (var (__i, __r) in c.registers)
                {
                    i = __i;
                    r = __r;
                    if (r.gcNum != -1L)
                    {
                        c.GCRegMap[r.gcNum] = _addr_c.registers[i];
                    }

                }

                r = r__prev1;
            }

            return _addr_c!;

        }

        private static void Set387(this ptr<Config> _addr_c, bool b)
        {
            ref Config c = ref _addr_c.val;

            c.NeedsFpScratch = b;
            c.use387 = b;
        }

        private static ptr<obj.Link> Ctxt(this ptr<Config> _addr_c)
        {
            ref Config c = ref _addr_c.val;

            return _addr_c.ctxt!;
        }
    }
}}}}
