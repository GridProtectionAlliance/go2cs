// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 August 29 08:53:31 UTC
// import "cmd/compile/internal/ssa" ==> using ssa = go.cmd.compile.@internal.ssa_package
// Original source: C:\Go\src\cmd\compile\internal\ssa\config.go
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using os = go.os_package;
using strconv = go.strconv_package;
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
            public slice<Register> registers; // machine registers
            public regMask gpRegMask; // general purpose integer register mask
            public regMask fpRegMask; // floating point register mask
            public regMask specialRegMask; // special register mask
            public sbyte FPReg; // register number of frame pointer, -1 if not used
            public sbyte LinkReg; // register number of link register if it is a general purpose register, -1 if not used
            public bool hasGReg; // has hardware g register
            public ptr<obj.Link> ctxt; // Generic arch information
            public bool optimize; // Do optimization
            public bool noDuffDevice; // Don't use Duff's device
            public bool useSSE; // Use SSE for non-float operations
            public bool nacl; // GOOS=nacl
            public bool use387; // GO386=387
            public bool SoftFloat; //
            public bool NeedsFpScratch; // No direct move between GP and FP register sets
            public bool BigEndian; //
            public ulong sparsePhiCutoff; // Sparse phi location algorithm used above this #blocks*#variables score
        }

        public delegate  bool blockRewriter(ref Block);
        public delegate  bool valueRewriter(ref Value);        public partial struct Types
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

        public partial interface Logger
        {
            bool Logf(@string _p0, params object _p0); // Log returns true if logging is not a no-op
// some logging calls account for more than a few heap allocations.
            bool Log(); // Fatal reports a compiler error and exits.
            bool Fatalf(src.XPos pos, @string msg, params object[] args); // Warnl writes compiler messages in the form expected by "errorcheck" tests
            bool Warnl(src.XPos pos, @string fmt_, params object[] args); // Forwards the Debug flags from gc
            bool Debug_checknil();
            bool Debug_eagerwb();
        }

        public partial interface Frontend : Logger
        {
            bool CanSSA(ref types.Type t);
            bool StringData(@string _p0); // returns *gc.Sym

// Auto returns a Node for an auto variable of the given type.
// The SSA compiler uses this function to allocate space for spills.
            bool Auto(src.XPos _p0, ref types.Type _p0); // Given the name for a compound type, returns the name we should use
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
            bool DerefItab(ref obj.LSym sym, long offset); // Line returns a string describing the given position.
            bool Line(src.XPos _p0); // AllocFrame assigns frame offsets to all live auto variables.
            bool AllocFrame(ref Func f); // Syslook returns a symbol of the runtime function/variable with the
// given name.
            bool Syslook(@string _p0); // UseWriteBarrier returns whether write barrier is enabled
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
            StorageClass StorageClass();
        }

        public partial struct StorageClass // : byte
        {
        }

        public static readonly StorageClass ClassAuto = iota; // local stack variable
        public static readonly var ClassParam = 0; // argument
        public static readonly var ClassParamOut = 1; // return value

        // NewConfig returns a new configuration object for the given architecture.
        public static ref Config NewConfig(@string arch, Types types, ref obj.Link ctxt, bool optimize)
        {
            Config c = ref new Config(arch:arch,Types:types);

            if (arch == "amd64")
            {
                c.PtrSize = 8L;
                c.RegSize = 8L;
                c.lowerBlock = rewriteBlockAMD64;
                c.lowerValue = rewriteValueAMD64;
                c.registers = registersAMD64[..];
                c.gpRegMask = gpRegMaskAMD64;
                c.fpRegMask = fpRegMaskAMD64;
                c.FPReg = framepointerRegAMD64;
                c.LinkReg = linkRegAMD64;
                c.hasGReg = false;
                goto __switch_break0;
            }
            if (arch == "amd64p32")
            {
                c.PtrSize = 4L;
                c.RegSize = 8L;
                c.lowerBlock = rewriteBlockAMD64;
                c.lowerValue = rewriteValueAMD64;
                c.registers = registersAMD64[..];
                c.gpRegMask = gpRegMaskAMD64;
                c.fpRegMask = fpRegMaskAMD64;
                c.FPReg = framepointerRegAMD64;
                c.LinkReg = linkRegAMD64;
                c.hasGReg = false;
                c.noDuffDevice = true;
                goto __switch_break0;
            }
            if (arch == "386")
            {
                c.PtrSize = 4L;
                c.RegSize = 4L;
                c.lowerBlock = rewriteBlock386;
                c.lowerValue = rewriteValue386;
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
            // default: 
                ctxt.Diag("arch %s not implemented", arch);

            __switch_break0:;
            c.ctxt = ctxt;
            c.optimize = optimize;
            c.nacl = objabi.GOOS == "nacl";
            c.useSSE = true; 

            // Don't use Duff's device nor SSE on Plan 9 AMD64, because
            // floating point operations are not allowed in note handler.
            if (objabi.GOOS == "plan9" && arch == "amd64")
            {
                c.noDuffDevice = true;
                c.useSSE = false;
            }
            if (c.nacl)
            {
                c.noDuffDevice = true; // Don't use Duff's device on NaCl

                // runtime call clobber R12 on nacl
                opcodeTable[OpARMCALLudiv].reg.clobbers |= 1L << (int)(12L); // R12
            } 

            // cutoff is compared with product of numblocks and numvalues,
            // if product is smaller than cutoff, use old non-sparse method.
            // cutoff == 0 implies all sparse.
            // cutoff == -1 implies none sparse.
            // Good cutoff values seem to be O(million) depending on constant factor cost of sparse.
            // TODO: get this from a flag, not an environment variable
            c.sparsePhiCutoff = 2500000L; // 0 for testing. // 2500000 determined with crude experiments w/ make.bash
            var ev = os.Getenv("GO_SSA_PHI_LOC_CUTOFF");
            if (ev != "")
            {
                var (v, err) = strconv.ParseInt(ev, 10L, 64L);
                if (err != null)
                {
                    ctxt.Diag("Environment variable GO_SSA_PHI_LOC_CUTOFF (value '%s') did not parse as a number", ev);
                }
                c.sparsePhiCutoff = uint64(v); // convert -1 to maxint, for never use sparse
            }
            return c;
        }

        private static void Set387(this ref Config c, bool b)
        {
            c.NeedsFpScratch = b;
            c.use387 = b;
        }

        private static ulong SparsePhiCutoff(this ref Config c)
        {
            return c.sparsePhiCutoff;
        }
        private static ref obj.Link Ctxt(this ref Config c)
        {
            return c.ctxt;
        }
    }
}}}}
