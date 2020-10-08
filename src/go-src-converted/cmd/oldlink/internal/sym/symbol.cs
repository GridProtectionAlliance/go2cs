// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sym -- go2cs converted at 2020 October 08 04:40:31 UTC
// import "cmd/oldlink/internal/sym" ==> using sym = go.cmd.oldlink.@internal.sym_package
// Original source: C:\Go\src\cmd\oldlink\internal\sym\symbol.go
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using elf = go.debug.elf_package;
using fmt = go.fmt_package;
using log = go.log_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace oldlink {
namespace @internal
{
    public static partial class sym_package
    {
        // Symbol is an entry in the symbol table.
        public partial struct Symbol
        {
            public @string Name;
            public SymKind Type;
            public short Version;
            public Attribute Attr;
            public int Dynid;
            public int Align;
            public int Elfsym;
            public int LocalElfsym;
            public long Value;
            public long Size;
            public ptr<Symbol> Sub;
            public ptr<Symbol> Outer;
            public ptr<Symbol> Gotype;
            public @string File; // actually package!
            public ptr<AuxSymbol> auxinfo;
            public ptr<Section> Sect;
            public ptr<FuncInfo> FuncInfo;
            public ptr<CompilationUnit> Unit; // P contains the raw symbol data.
            public slice<byte> P;
            public slice<Reloc> R;
        }

        // AuxSymbol contains less-frequently used sym.Symbol fields.
        public partial struct AuxSymbol
        {
            public @string extname;
            public @string dynimplib;
            public @string dynimpvers;
            public byte localentry;
            public int plt;
            public int got; // ElfType is set for symbols read from shared libraries by ldshlibsyms. It
// is not set for symbols defined by the packages being linked or by symbols
// read by ldelf (and so is left as elf.STT_NOTYPE).
            public elf.SymType elftype;
        }

        public static readonly long SymVerABI0 = (long)0L;
        public static readonly long SymVerABIInternal = (long)1L;
        public static readonly long SymVerStatic = (long)10L; // Minimum version used by static (file-local) syms

        public static long ABIToVersion(obj.ABI abi)
        {

            if (abi == obj.ABI0) 
                return SymVerABI0;
            else if (abi == obj.ABIInternal) 
                return SymVerABIInternal;
                        return -1L;

        }

        public static (obj.ABI, bool) VersionToABI(long v)
        {
            obj.ABI _p0 = default;
            bool _p0 = default;


            if (v == SymVerABI0) 
                return (obj.ABI0, true);
            else if (v == SymVerABIInternal) 
                return (obj.ABIInternal, true);
                        return (~obj.ABI(0L), false);

        }

        private static @string String(this ptr<Symbol> _addr_s)
        {
            ref Symbol s = ref _addr_s.val;

            if (s.Version == 0L)
            {
                return s.Name;
            }

            return fmt.Sprintf("%s<%d>", s.Name, s.Version);

        }

        private static bool IsFileLocal(this ptr<Symbol> _addr_s)
        {
            ref Symbol s = ref _addr_s.val;

            return s.Version >= SymVerStatic;
        }

        private static int ElfsymForReloc(this ptr<Symbol> _addr_s)
        {
            ref Symbol s = ref _addr_s.val;
 
            // If putelfsym created a local version of this symbol, use that in all
            // relocations.
            if (s.LocalElfsym != 0L)
            {
                return s.LocalElfsym;
            }
            else
            {
                return s.Elfsym;
            }

        }

        private static long Length(this ptr<Symbol> _addr_s, object _)
        {
            ref Symbol s = ref _addr_s.val;

            return s.Size;
        }

        private static void Grow(this ptr<Symbol> _addr_s, long siz)
        {
            ref Symbol s = ref _addr_s.val;

            if (int64(int(siz)) != siz)
            {
                log.Fatalf("symgrow size %d too long", siz);
            }

            if (int64(len(s.P)) >= siz)
            {
                return ;
            }

            if (cap(s.P) < int(siz))
            {
                var p = make_slice<byte>(2L * (siz + 1L));
                s.P = append(p[..0L], s.P);
            }

            s.P = s.P[..siz];

        }

        private static long AddBytes(this ptr<Symbol> _addr_s, slice<byte> bytes)
        {
            ref Symbol s = ref _addr_s.val;

            if (s.Type == 0L)
            {
                s.Type = SDATA;
            }

            s.Attr |= AttrReachable;
            s.P = append(s.P, bytes);
            s.Size = int64(len(s.P));

            return s.Size;

        }

        private static long AddUint8(this ptr<Symbol> _addr_s, byte v)
        {
            ref Symbol s = ref _addr_s.val;

            var off = s.Size;
            if (s.Type == 0L)
            {
                s.Type = SDATA;
            }

            s.Attr |= AttrReachable;
            s.Size++;
            s.P = append(s.P, v);

            return off;

        }

        private static long AddUint16(this ptr<Symbol> _addr_s, ptr<sys.Arch> _addr_arch, ushort v)
        {
            ref Symbol s = ref _addr_s.val;
            ref sys.Arch arch = ref _addr_arch.val;

            return s.AddUintXX(arch, uint64(v), 2L);
        }

        private static long AddUint32(this ptr<Symbol> _addr_s, ptr<sys.Arch> _addr_arch, uint v)
        {
            ref Symbol s = ref _addr_s.val;
            ref sys.Arch arch = ref _addr_arch.val;

            return s.AddUintXX(arch, uint64(v), 4L);
        }

        private static long AddUint64(this ptr<Symbol> _addr_s, ptr<sys.Arch> _addr_arch, ulong v)
        {
            ref Symbol s = ref _addr_s.val;
            ref sys.Arch arch = ref _addr_arch.val;

            return s.AddUintXX(arch, v, 8L);
        }

        private static long AddUint(this ptr<Symbol> _addr_s, ptr<sys.Arch> _addr_arch, ulong v)
        {
            ref Symbol s = ref _addr_s.val;
            ref sys.Arch arch = ref _addr_arch.val;

            return s.AddUintXX(arch, v, arch.PtrSize);
        }

        private static long SetUint8(this ptr<Symbol> _addr_s, ptr<sys.Arch> _addr_arch, long r, byte v)
        {
            ref Symbol s = ref _addr_s.val;
            ref sys.Arch arch = ref _addr_arch.val;

            return s.setUintXX(arch, r, uint64(v), 1L);
        }

        private static long SetUint16(this ptr<Symbol> _addr_s, ptr<sys.Arch> _addr_arch, long r, ushort v)
        {
            ref Symbol s = ref _addr_s.val;
            ref sys.Arch arch = ref _addr_arch.val;

            return s.setUintXX(arch, r, uint64(v), 2L);
        }

        private static long SetUint32(this ptr<Symbol> _addr_s, ptr<sys.Arch> _addr_arch, long r, uint v)
        {
            ref Symbol s = ref _addr_s.val;
            ref sys.Arch arch = ref _addr_arch.val;

            return s.setUintXX(arch, r, uint64(v), 4L);
        }

        private static long SetUint(this ptr<Symbol> _addr_s, ptr<sys.Arch> _addr_arch, long r, ulong v)
        {
            ref Symbol s = ref _addr_s.val;
            ref sys.Arch arch = ref _addr_arch.val;

            return s.setUintXX(arch, r, v, int64(arch.PtrSize));
        }

        private static long addAddrPlus(this ptr<Symbol> _addr_s, ptr<sys.Arch> _addr_arch, ptr<Symbol> _addr_t, long add, objabi.RelocType typ)
        {
            ref Symbol s = ref _addr_s.val;
            ref sys.Arch arch = ref _addr_arch.val;
            ref Symbol t = ref _addr_t.val;

            if (s.Type == 0L)
            {
                s.Type = SDATA;
            }

            s.Attr |= AttrReachable;
            var i = s.Size;
            s.Size += int64(arch.PtrSize);
            s.Grow(s.Size);
            var r = s.AddRel();
            r.Sym = t;
            r.Off = int32(i);
            r.Siz = uint8(arch.PtrSize);
            r.Type = typ;
            r.Add = add;
            return i + int64(r.Siz);

        }

        private static long AddAddrPlus(this ptr<Symbol> _addr_s, ptr<sys.Arch> _addr_arch, ptr<Symbol> _addr_t, long add)
        {
            ref Symbol s = ref _addr_s.val;
            ref sys.Arch arch = ref _addr_arch.val;
            ref Symbol t = ref _addr_t.val;

            return s.addAddrPlus(arch, t, add, objabi.R_ADDR);
        }

        private static long AddCURelativeAddrPlus(this ptr<Symbol> _addr_s, ptr<sys.Arch> _addr_arch, ptr<Symbol> _addr_t, long add)
        {
            ref Symbol s = ref _addr_s.val;
            ref sys.Arch arch = ref _addr_arch.val;
            ref Symbol t = ref _addr_t.val;

            return s.addAddrPlus(arch, t, add, objabi.R_ADDRCUOFF);
        }

        private static long AddPCRelPlus(this ptr<Symbol> _addr_s, ptr<sys.Arch> _addr_arch, ptr<Symbol> _addr_t, long add)
        {
            ref Symbol s = ref _addr_s.val;
            ref sys.Arch arch = ref _addr_arch.val;
            ref Symbol t = ref _addr_t.val;

            if (s.Type == 0L)
            {
                s.Type = SDATA;
            }

            s.Attr |= AttrReachable;
            var i = s.Size;
            s.Size += 4L;
            s.Grow(s.Size);
            var r = s.AddRel();
            r.Sym = t;
            r.Off = int32(i);
            r.Add = add;
            r.Type = objabi.R_PCREL;
            r.Siz = 4L;
            if (arch.Family == sys.S390X || arch.Family == sys.PPC64)
            {
                r.InitExt();
            }

            if (arch.Family == sys.S390X)
            {
                r.Variant = RV_390_DBL;
            }

            return i + int64(r.Siz);

        }

        private static long AddAddr(this ptr<Symbol> _addr_s, ptr<sys.Arch> _addr_arch, ptr<Symbol> _addr_t)
        {
            ref Symbol s = ref _addr_s.val;
            ref sys.Arch arch = ref _addr_arch.val;
            ref Symbol t = ref _addr_t.val;

            return s.AddAddrPlus(arch, t, 0L);
        }

        private static long SetAddrPlus(this ptr<Symbol> _addr_s, ptr<sys.Arch> _addr_arch, long off, ptr<Symbol> _addr_t, long add)
        {
            ref Symbol s = ref _addr_s.val;
            ref sys.Arch arch = ref _addr_arch.val;
            ref Symbol t = ref _addr_t.val;

            if (s.Type == 0L)
            {
                s.Type = SDATA;
            }

            s.Attr |= AttrReachable;
            if (off + int64(arch.PtrSize) > s.Size)
            {
                s.Size = off + int64(arch.PtrSize);
                s.Grow(s.Size);
            }

            var r = s.AddRel();
            r.Sym = t;
            r.Off = int32(off);
            r.Siz = uint8(arch.PtrSize);
            r.Type = objabi.R_ADDR;
            r.Add = add;
            return off + int64(r.Siz);

        }

        private static long SetAddr(this ptr<Symbol> _addr_s, ptr<sys.Arch> _addr_arch, long off, ptr<Symbol> _addr_t)
        {
            ref Symbol s = ref _addr_s.val;
            ref sys.Arch arch = ref _addr_arch.val;
            ref Symbol t = ref _addr_t.val;

            return s.SetAddrPlus(arch, off, t, 0L);
        }

        private static long AddSize(this ptr<Symbol> _addr_s, ptr<sys.Arch> _addr_arch, ptr<Symbol> _addr_t)
        {
            ref Symbol s = ref _addr_s.val;
            ref sys.Arch arch = ref _addr_arch.val;
            ref Symbol t = ref _addr_t.val;

            if (s.Type == 0L)
            {
                s.Type = SDATA;
            }

            s.Attr |= AttrReachable;
            var i = s.Size;
            s.Size += int64(arch.PtrSize);
            s.Grow(s.Size);
            var r = s.AddRel();
            r.Sym = t;
            r.Off = int32(i);
            r.Siz = uint8(arch.PtrSize);
            r.Type = objabi.R_SIZE;
            return i + int64(r.Siz);

        }

        private static long AddAddrPlus4(this ptr<Symbol> _addr_s, ptr<Symbol> _addr_t, long add)
        {
            ref Symbol s = ref _addr_s.val;
            ref Symbol t = ref _addr_t.val;

            if (s.Type == 0L)
            {
                s.Type = SDATA;
            }

            s.Attr |= AttrReachable;
            var i = s.Size;
            s.Size += 4L;
            s.Grow(s.Size);
            var r = s.AddRel();
            r.Sym = t;
            r.Off = int32(i);
            r.Siz = 4L;
            r.Type = objabi.R_ADDR;
            r.Add = add;
            return i + int64(r.Siz);

        }

        private static ptr<Reloc> AddRel(this ptr<Symbol> _addr_s)
        {
            ref Symbol s = ref _addr_s.val;

            s.R = append(s.R, new Reloc());
            return _addr__addr_s.R[len(s.R) - 1L]!;
        }

        private static long AddUintXX(this ptr<Symbol> _addr_s, ptr<sys.Arch> _addr_arch, ulong v, long wid)
        {
            ref Symbol s = ref _addr_s.val;
            ref sys.Arch arch = ref _addr_arch.val;

            var off = s.Size;
            s.setUintXX(arch, off, v, int64(wid));
            return off;
        }

        private static long setUintXX(this ptr<Symbol> _addr_s, ptr<sys.Arch> _addr_arch, long off, ulong v, long wid)
        {
            ref Symbol s = ref _addr_s.val;
            ref sys.Arch arch = ref _addr_arch.val;

            if (s.Type == 0L)
            {
                s.Type = SDATA;
            }

            s.Attr |= AttrReachable;
            if (s.Size < off + wid)
            {
                s.Size = off + wid;
                s.Grow(s.Size);
            }

            switch (wid)
            {
                case 1L: 
                    s.P[off] = uint8(v);
                    break;
                case 2L: 
                    arch.ByteOrder.PutUint16(s.P[off..], uint16(v));
                    break;
                case 4L: 
                    arch.ByteOrder.PutUint32(s.P[off..], uint32(v));
                    break;
                case 8L: 
                    arch.ByteOrder.PutUint64(s.P[off..], v);
                    break;
            }

            return off + wid;

        }

        private static void makeAuxInfo(this ptr<Symbol> _addr_s)
        {
            ref Symbol s = ref _addr_s.val;

            if (s.auxinfo == null)
            {
                s.auxinfo = addr(new AuxSymbol(extname:s.Name,plt:-1,got:-1));
            }

        }

        private static @string Extname(this ptr<Symbol> _addr_s)
        {
            ref Symbol s = ref _addr_s.val;

            if (s.auxinfo == null)
            {
                return s.Name;
            }

            return s.auxinfo.extname;

        }

        private static void SetExtname(this ptr<Symbol> _addr_s, @string n)
        {
            ref Symbol s = ref _addr_s.val;

            if (s.auxinfo == null)
            {
                if (s.Name == n)
                {
                    return ;
                }

                s.makeAuxInfo();

            }

            s.auxinfo.extname = n;

        }

        private static @string Dynimplib(this ptr<Symbol> _addr_s)
        {
            ref Symbol s = ref _addr_s.val;

            if (s.auxinfo == null)
            {
                return "";
            }

            return s.auxinfo.dynimplib;

        }

        private static @string Dynimpvers(this ptr<Symbol> _addr_s)
        {
            ref Symbol s = ref _addr_s.val;

            if (s.auxinfo == null)
            {
                return "";
            }

            return s.auxinfo.dynimpvers;

        }

        private static void SetDynimplib(this ptr<Symbol> _addr_s, @string lib)
        {
            ref Symbol s = ref _addr_s.val;

            if (s.auxinfo == null)
            {
                s.makeAuxInfo();
            }

            s.auxinfo.dynimplib = lib;

        }

        private static void SetDynimpvers(this ptr<Symbol> _addr_s, @string vers)
        {
            ref Symbol s = ref _addr_s.val;

            if (s.auxinfo == null)
            {
                s.makeAuxInfo();
            }

            s.auxinfo.dynimpvers = vers;

        }

        private static void ResetDyninfo(this ptr<Symbol> _addr_s)
        {
            ref Symbol s = ref _addr_s.val;

            if (s.auxinfo != null)
            {
                s.auxinfo.dynimplib = "";
                s.auxinfo.dynimpvers = "";
            }

        }

        private static byte Localentry(this ptr<Symbol> _addr_s)
        {
            ref Symbol s = ref _addr_s.val;

            if (s.auxinfo == null)
            {
                return 0L;
            }

            return s.auxinfo.localentry;

        }

        private static void SetLocalentry(this ptr<Symbol> _addr_s, byte val)
        {
            ref Symbol s = ref _addr_s.val;

            if (s.auxinfo == null)
            {
                if (val != 0L)
                {
                    return ;
                }

                s.makeAuxInfo();

            }

            s.auxinfo.localentry = val;

        }

        private static int Plt(this ptr<Symbol> _addr_s)
        {
            ref Symbol s = ref _addr_s.val;

            if (s.auxinfo == null)
            {
                return -1L;
            }

            return s.auxinfo.plt;

        }

        private static void SetPlt(this ptr<Symbol> _addr_s, int val)
        {
            ref Symbol s = ref _addr_s.val;

            if (s.auxinfo == null)
            {
                if (val == -1L)
                {
                    return ;
                }

                s.makeAuxInfo();

            }

            s.auxinfo.plt = val;

        }

        private static int Got(this ptr<Symbol> _addr_s)
        {
            ref Symbol s = ref _addr_s.val;

            if (s.auxinfo == null)
            {
                return -1L;
            }

            return s.auxinfo.got;

        }

        private static void SetGot(this ptr<Symbol> _addr_s, int val)
        {
            ref Symbol s = ref _addr_s.val;

            if (s.auxinfo == null)
            {
                if (val == -1L)
                {
                    return ;
                }

                s.makeAuxInfo();

            }

            s.auxinfo.got = val;

        }

        private static elf.SymType ElfType(this ptr<Symbol> _addr_s)
        {
            ref Symbol s = ref _addr_s.val;

            if (s.auxinfo == null)
            {
                return elf.STT_NOTYPE;
            }

            return s.auxinfo.elftype;

        }

        private static void SetElfType(this ptr<Symbol> _addr_s, elf.SymType val)
        {
            ref Symbol s = ref _addr_s.val;

            if (s.auxinfo == null)
            {
                if (val == elf.STT_NOTYPE)
                {
                    return ;
                }

                s.makeAuxInfo();

            }

            s.auxinfo.elftype = val;

        }

        // SortSub sorts a linked-list (by Sub) of *Symbol by Value.
        // Used for sub-symbols when loading host objects (see e.g. ldelf.go).
        public static ptr<Symbol> SortSub(ptr<Symbol> _addr_l)
        {
            ref Symbol l = ref _addr_l.val;

            if (l == null || l.Sub == null)
            {
                return _addr_l!;
            }

            var l1 = l;
            var l2 = l;
            while (true)
            {
                l2 = l2.Sub;
                if (l2 == null)
                {
                    break;
                }

                l2 = l2.Sub;
                if (l2 == null)
                {
                    break;
                }

                l1 = l1.Sub;

            }


            l2 = l1.Sub;
            l1.Sub = null;
            l1 = SortSub(_addr_l);
            l2 = SortSub(_addr_l2); 

            /* set up lead element */
            if (l1.Value < l2.Value)
            {
                l = l1;
                l1 = l1.Sub;
            }
            else
            {
                l = l2;
                l2 = l2.Sub;
            }

            var le = l;

            while (true)
            {
                if (l1 == null)
                {
                    while (l2 != null)
                    {
                        le.Sub = l2;
                        le = l2;
                        l2 = l2.Sub;
                    }


                    le.Sub = null;
                    break;

                }

                if (l2 == null)
                {
                    while (l1 != null)
                    {
                        le.Sub = l1;
                        le = l1;
                        l1 = l1.Sub;
                    }


                    break;

                }

                if (l1.Value < l2.Value)
                {
                    le.Sub = l1;
                    le = l1;
                    l1 = l1.Sub;
                }
                else
                {
                    le.Sub = l2;
                    le = l2;
                    l2 = l2.Sub;
                }

            }


            le.Sub = null;
            return _addr_l!;

        }

        public partial struct FuncInfo
        {
            public int Args;
            public int Locals;
            public Pcdata Pcsp;
            public Pcdata Pcfile;
            public Pcdata Pcline;
            public Pcdata Pcinline;
            public slice<Pcdata> Pcdata;
            public slice<ptr<Symbol>> Funcdata;
            public slice<long> Funcdataoff;
            public slice<ptr<Symbol>> File;
            public slice<InlinedCall> InlTree;
        }

        // InlinedCall is a node in a local inlining tree (FuncInfo.InlTree).
        public partial struct InlinedCall
        {
            public int Parent; // index of parent in InlTree
            public ptr<Symbol> File; // file of the inlined call
            public int Line; // line number of the inlined call
            public @string Func; // name of the function that was inlined
            public int ParentPC; // PC of the instruction just before the inlined body (offset from function start)
        }

        public partial struct Pcdata
        {
            public slice<byte> P;
        }
    }
}}}}
