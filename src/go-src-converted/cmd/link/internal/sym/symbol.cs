// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package sym -- go2cs converted at 2020 August 29 10:02:56 UTC
// import "cmd/link/internal/sym" ==> using sym = go.cmd.link.@internal.sym_package
// Original source: C:\Go\src\cmd\link\internal\sym\symbol.go
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
            public @string Extname;
            public SymKind Type;
            public short Version;
            public Attribute Attr;
            public byte Localentry;
            public int Dynid;
            public int Plt;
            public int Got;
            public int Align;
            public int Elfsym;
            public int LocalElfsym;
            public long Value;
            public long Size; // ElfType is set for symbols read from shared libraries by ldshlibsyms. It
// is not set for symbols defined by the packages being linked or by symbols
// read by ldelf (and so is left as elf.STT_NOTYPE).
            public elf.SymType ElfType;
            public ptr<Symbol> Sub;
            public ptr<Symbol> Outer;
            public ptr<Symbol> Gotype;
            public ptr<Symbol> Reachparent;
            public @string File;
            public @string Dynimplib;
            public @string Dynimpvers;
            public ptr<Section> Sect;
            public ptr<FuncInfo> FuncInfo;
            public ptr<Library> Lib; // Package defining this symbol
// P contains the raw symbol data.
            public slice<byte> P;
            public slice<Reloc> R;
        }

        private static @string String(this ref Symbol s)
        {
            if (s.Version == 0L)
            {
                return s.Name;
            }
            return fmt.Sprintf("%s<%d>", s.Name, s.Version);
        }

        private static int ElfsymForReloc(this ref Symbol s)
        { 
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

        private static long Len(this ref Symbol s)
        {
            return s.Size;
        }

        private static void Grow(this ref Symbol s, long siz)
        {
            if (int64(int(siz)) != siz)
            {
                log.Fatalf("symgrow size %d too long", siz);
            }
            if (int64(len(s.P)) >= siz)
            {
                return;
            }
            if (cap(s.P) < int(siz))
            {
                var p = make_slice<byte>(2L * (siz + 1L));
                s.P = append(p[..0L], s.P);
            }
            s.P = s.P[..siz];
        }

        private static long AddBytes(this ref Symbol s, slice<byte> bytes)
        {
            if (s.Type == 0L)
            {
                s.Type = SDATA;
            }
            s.Attr |= AttrReachable;
            s.P = append(s.P, bytes);
            s.Size = int64(len(s.P));

            return s.Size;
        }

        private static long AddUint8(this ref Symbol s, byte v)
        {
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

        private static long AddUint16(this ref Symbol s, ref sys.Arch arch, ushort v)
        {
            return s.AddUintXX(arch, uint64(v), 2L);
        }

        private static long AddUint32(this ref Symbol s, ref sys.Arch arch, uint v)
        {
            return s.AddUintXX(arch, uint64(v), 4L);
        }

        private static long AddUint64(this ref Symbol s, ref sys.Arch arch, ulong v)
        {
            return s.AddUintXX(arch, v, 8L);
        }

        private static long AddUint(this ref Symbol s, ref sys.Arch arch, ulong v)
        {
            return s.AddUintXX(arch, v, arch.PtrSize);
        }

        private static long SetUint8(this ref Symbol s, ref sys.Arch arch, long r, byte v)
        {
            return s.setUintXX(arch, r, uint64(v), 1L);
        }

        private static long SetUint32(this ref Symbol s, ref sys.Arch arch, long r, uint v)
        {
            return s.setUintXX(arch, r, uint64(v), 4L);
        }

        private static long SetUint(this ref Symbol s, ref sys.Arch arch, long r, ulong v)
        {
            return s.setUintXX(arch, r, v, int64(arch.PtrSize));
        }

        private static long addAddrPlus(this ref Symbol s, ref sys.Arch arch, ref Symbol t, long add, objabi.RelocType typ)
        {
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

        private static long AddAddrPlus(this ref Symbol s, ref sys.Arch arch, ref Symbol t, long add)
        {
            return s.addAddrPlus(arch, t, add, objabi.R_ADDR);
        }

        private static long AddCURelativeAddrPlus(this ref Symbol s, ref sys.Arch arch, ref Symbol t, long add)
        {
            return s.addAddrPlus(arch, t, add, objabi.R_ADDRCUOFF);
        }

        private static long AddPCRelPlus(this ref Symbol s, ref sys.Arch arch, ref Symbol t, long add)
        {
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
            if (arch.Family == sys.S390X)
            {
                r.Variant = RV_390_DBL;
            }
            return i + int64(r.Siz);
        }

        private static long AddAddr(this ref Symbol s, ref sys.Arch arch, ref Symbol t)
        {
            return s.AddAddrPlus(arch, t, 0L);
        }

        private static long SetAddrPlus(this ref Symbol s, ref sys.Arch arch, long off, ref Symbol t, long add)
        {
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

        private static long SetAddr(this ref Symbol s, ref sys.Arch arch, long off, ref Symbol t)
        {
            return s.SetAddrPlus(arch, off, t, 0L);
        }

        private static long AddSize(this ref Symbol s, ref sys.Arch arch, ref Symbol t)
        {
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

        private static long AddAddrPlus4(this ref Symbol s, ref Symbol t, long add)
        {
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

        private static ref Reloc AddRel(this ref Symbol s)
        {
            s.R = append(s.R, new Reloc());
            return ref s.R[len(s.R) - 1L];
        }

        private static long AddUintXX(this ref Symbol s, ref sys.Arch arch, ulong v, long wid)
        {
            var off = s.Size;
            s.setUintXX(arch, off, v, int64(wid));
            return off;
        }

        private static long setUintXX(this ref Symbol s, ref sys.Arch arch, long off, ulong v, long wid)
        {
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

        // SortSub sorts a linked-list (by Sub) of *Symbol by Value.
        // Used for sub-symbols when loading host objects (see e.g. ldelf.go).
        public static ref Symbol SortSub(ref Symbol l)
        {
            if (l == null || l.Sub == null)
            {
                return l;
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
            l1 = SortSub(l);
            l2 = SortSub(l2); 

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
            return l;
        }

        public partial struct FuncInfo
        {
            public int Args;
            public int Locals;
            public slice<Auto> Autom;
            public Pcdata Pcsp;
            public Pcdata Pcfile;
            public Pcdata Pcline;
            public Pcdata Pcinline;
            public slice<Pcdata> Pcdata;
            public slice<ref Symbol> Funcdata;
            public slice<long> Funcdataoff;
            public slice<ref Symbol> File;
            public slice<InlinedCall> InlTree;
        }

        // InlinedCall is a node in a local inlining tree (FuncInfo.InlTree).
        public partial struct InlinedCall
        {
            public int Parent; // index of parent in InlTree
            public ptr<Symbol> File; // file of the inlined call
            public int Line; // line number of the inlined call
            public ptr<Symbol> Func; // function that was inlined
        }

        public partial struct Pcdata
        {
            public slice<byte> P;
        }

        public partial struct Auto
        {
            public ptr<Symbol> Asym;
            public ptr<Symbol> Gotype;
            public int Aoffset;
            public short Name;
        }
    }
}}}}
