// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sym -- go2cs converted at 2020 October 09 05:48:55 UTC
// import "cmd/link/internal/sym" ==> using sym = go.cmd.link.@internal.sym_package
// Original source: C:\Go\src\cmd\link\internal\sym\symbol.go
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using elf = go.debug.elf_package;
using fmt = go.fmt_package;
using log = go.log_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace link {
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
            public long Value;
            public long Size;
            public ptr<Symbol> Outer;
            public LoaderSym SymIdx;
            public ptr<AuxSymbol> auxinfo;
            public ptr<Section> Sect; // P contains the raw symbol data.
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

        private static long Len(this ptr<Symbol> _addr_s)
        {
            ref Symbol s = ref _addr_s.val;

            return s.Size;
        }

        private static long Length(this ptr<Symbol> _addr_s, object dwarfContext)
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

        public partial struct Pcdata
        {
            public slice<byte> P;
        }
    }
}}}}
