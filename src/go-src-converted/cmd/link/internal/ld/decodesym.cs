// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2020 October 09 05:49:17 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\decodesym.go
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using loader = go.cmd.link.@internal.loader_package;
using sym = go.cmd.link.@internal.sym_package;
using elf = go.debug.elf_package;
using log = go.log_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        // Decoding the type.* symbols.     This has to be in sync with
        // ../../runtime/type.go, or more specifically, with what
        // cmd/compile/internal/gc/reflect.go stuffs in these.

        // tflag is documented in reflect/type.go.
        //
        // tflag values must be kept in sync with copies in:
        //    cmd/compile/internal/gc/reflect.go
        //    cmd/link/internal/ld/decodesym.go
        //    reflect/type.go
        //    runtime/type.go
        private static readonly long tflagUncommon = (long)1L << (int)(0L);
        private static readonly long tflagExtraStar = (long)1L << (int)(1L);


        private static ulong decodeInuxi(ptr<sys.Arch> _addr_arch, slice<byte> p, long sz) => func((_, panic, __) =>
        {
            ref sys.Arch arch = ref _addr_arch.val;

            switch (sz)
            {
                case 2L: 
                    return uint64(arch.ByteOrder.Uint16(p));
                    break;
                case 4L: 
                    return uint64(arch.ByteOrder.Uint32(p));
                    break;
                case 8L: 
                    return arch.ByteOrder.Uint64(p);
                    break;
                default: 
                    Exitf("dwarf: decode inuxi %d", sz);
                    panic("unreachable");
                    break;
            }

        });

        private static long commonsize(ptr<sys.Arch> _addr_arch)
        {
            ref sys.Arch arch = ref _addr_arch.val;

            return 4L * arch.PtrSize + 8L + 8L;
        } // runtime._type
        private static long structfieldSize(ptr<sys.Arch> _addr_arch)
        {
            ref sys.Arch arch = ref _addr_arch.val;

            return 3L * arch.PtrSize;
        } // runtime.structfield
        private static long uncommonSize()
        {
            return 4L + 2L + 2L + 4L + 4L;
        } // runtime.uncommontype

        // Type.commonType.kind
        private static byte decodetypeKind(ptr<sys.Arch> _addr_arch, slice<byte> p)
        {
            ref sys.Arch arch = ref _addr_arch.val;

            return p[2L * arch.PtrSize + 7L] & objabi.KindMask; //  0x13 / 0x1f
        }

        // Type.commonType.kind
        private static byte decodetypeUsegcprog(ptr<sys.Arch> _addr_arch, slice<byte> p)
        {
            ref sys.Arch arch = ref _addr_arch.val;

            return p[2L * arch.PtrSize + 7L] & objabi.KindGCProg; //  0x13 / 0x1f
        }

        // Type.commonType.size
        private static long decodetypeSize(ptr<sys.Arch> _addr_arch, slice<byte> p)
        {
            ref sys.Arch arch = ref _addr_arch.val;

            return int64(decodeInuxi(_addr_arch, p, arch.PtrSize)); // 0x8 / 0x10
        }

        // Type.commonType.ptrdata
        private static long decodetypePtrdata(ptr<sys.Arch> _addr_arch, slice<byte> p)
        {
            ref sys.Arch arch = ref _addr_arch.val;

            return int64(decodeInuxi(_addr_arch, p[arch.PtrSize..], arch.PtrSize)); // 0x8 / 0x10
        }

        // Type.commonType.tflag
        private static bool decodetypeHasUncommon(ptr<sys.Arch> _addr_arch, slice<byte> p)
        {
            ref sys.Arch arch = ref _addr_arch.val;

            return p[2L * arch.PtrSize + 4L] & tflagUncommon != 0L;
        }

        // Type.FuncType.dotdotdot
        private static bool decodetypeFuncDotdotdot(ptr<sys.Arch> _addr_arch, slice<byte> p)
        {
            ref sys.Arch arch = ref _addr_arch.val;

            return uint16(decodeInuxi(_addr_arch, p[commonsize(_addr_arch) + 2L..], 2L)) & (1L << (int)(15L)) != 0L;
        }

        // Type.FuncType.inCount
        private static long decodetypeFuncInCount(ptr<sys.Arch> _addr_arch, slice<byte> p)
        {
            ref sys.Arch arch = ref _addr_arch.val;

            return int(decodeInuxi(_addr_arch, p[commonsize(_addr_arch)..], 2L));
        }

        private static long decodetypeFuncOutCount(ptr<sys.Arch> _addr_arch, slice<byte> p)
        {
            ref sys.Arch arch = ref _addr_arch.val;

            return int(uint16(decodeInuxi(_addr_arch, p[commonsize(_addr_arch) + 2L..], 2L)) & (1L << (int)(15L) - 1L));
        }

        // InterfaceType.methods.length
        private static long decodetypeIfaceMethodCount(ptr<sys.Arch> _addr_arch, slice<byte> p)
        {
            ref sys.Arch arch = ref _addr_arch.val;

            return int64(decodeInuxi(_addr_arch, p[commonsize(_addr_arch) + 2L * arch.PtrSize..], arch.PtrSize));
        }

        // methodsig is a fully qualified typed method signature, like
        // "Visit(type.go/ast.Node) (type.go/ast.Visitor)".
        private partial struct methodsig // : @string
        {
        }

        // Matches runtime/typekind.go and reflect.Kind.
        private static readonly long kindArray = (long)17L;
        private static readonly long kindChan = (long)18L;
        private static readonly long kindFunc = (long)19L;
        private static readonly long kindInterface = (long)20L;
        private static readonly long kindMap = (long)21L;
        private static readonly long kindPtr = (long)22L;
        private static readonly long kindSlice = (long)23L;
        private static readonly long kindStruct = (long)25L;
        private static readonly long kindMask = (long)(1L << (int)(5L)) - 1L;


        private static loader.Reloc2 decodeReloc(ptr<loader.Loader> _addr_ldr, loader.Sym symIdx, ptr<loader.Relocs> _addr_relocs, int off)
        {
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref loader.Relocs relocs = ref _addr_relocs.val;

            for (long j = 0L; j < relocs.Count(); j++)
            {
                var rel = relocs.At2(j);
                if (rel.Off() == off)
                {
                    return rel;
                }

            }

            return new loader.Reloc2();

        }

        private static loader.Sym decodeRelocSym(ptr<loader.Loader> _addr_ldr, loader.Sym symIdx, ptr<loader.Relocs> _addr_relocs, int off)
        {
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref loader.Relocs relocs = ref _addr_relocs.val;

            return decodeReloc(_addr_ldr, symIdx, _addr_relocs, off).Sym();
        }

        // decodetypeName decodes the name from a reflect.name.
        private static @string decodetypeName(ptr<loader.Loader> _addr_ldr, loader.Sym symIdx, ptr<loader.Relocs> _addr_relocs, long off)
        {
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref loader.Relocs relocs = ref _addr_relocs.val;

            var r = decodeRelocSym(_addr_ldr, symIdx, _addr_relocs, int32(off));
            if (r == 0L)
            {
                return "";
            }

            var data = ldr.Data(r);
            var namelen = int(uint16(data[1L]) << (int)(8L) | uint16(data[2L]));
            return string(data[3L..3L + namelen]);

        }

        private static loader.Sym decodetypeFuncInType(ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx, ptr<loader.Relocs> _addr_relocs, long i)
        {
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref sys.Arch arch = ref _addr_arch.val;
            ref loader.Relocs relocs = ref _addr_relocs.val;

            var uadd = commonsize(_addr_arch) + 4L;
            if (arch.PtrSize == 8L)
            {
                uadd += 4L;
            }

            if (decodetypeHasUncommon(_addr_arch, ldr.Data(symIdx)))
            {
                uadd += uncommonSize();
            }

            return decodeRelocSym(_addr_ldr, symIdx, _addr_relocs, int32(uadd + i * arch.PtrSize));

        }

        private static loader.Sym decodetypeFuncOutType(ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx, ptr<loader.Relocs> _addr_relocs, long i)
        {
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref sys.Arch arch = ref _addr_arch.val;
            ref loader.Relocs relocs = ref _addr_relocs.val;

            return decodetypeFuncInType(_addr_ldr, _addr_arch, symIdx, _addr_relocs, i + decodetypeFuncInCount(_addr_arch, ldr.Data(symIdx)));
        }

        private static loader.Sym decodetypeArrayElem(ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx)
        {
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref sys.Arch arch = ref _addr_arch.val;

            ref var relocs = ref heap(ldr.Relocs(symIdx), out ptr<var> _addr_relocs);
            return decodeRelocSym(_addr_ldr, symIdx, _addr_relocs, int32(commonsize(_addr_arch))); // 0x1c / 0x30
        }

        private static long decodetypeArrayLen(ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx)
        {
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref sys.Arch arch = ref _addr_arch.val;

            var data = ldr.Data(symIdx);
            return int64(decodeInuxi(_addr_arch, data[commonsize(_addr_arch) + 2L * arch.PtrSize..], arch.PtrSize));
        }

        private static loader.Sym decodetypeChanElem(ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx)
        {
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref sys.Arch arch = ref _addr_arch.val;

            ref var relocs = ref heap(ldr.Relocs(symIdx), out ptr<var> _addr_relocs);
            return decodeRelocSym(_addr_ldr, symIdx, _addr_relocs, int32(commonsize(_addr_arch))); // 0x1c / 0x30
        }

        private static loader.Sym decodetypeMapKey(ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx)
        {
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref sys.Arch arch = ref _addr_arch.val;

            ref var relocs = ref heap(ldr.Relocs(symIdx), out ptr<var> _addr_relocs);
            return decodeRelocSym(_addr_ldr, symIdx, _addr_relocs, int32(commonsize(_addr_arch))); // 0x1c / 0x30
        }

        private static loader.Sym decodetypeMapValue(ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx)
        {
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref sys.Arch arch = ref _addr_arch.val;

            ref var relocs = ref heap(ldr.Relocs(symIdx), out ptr<var> _addr_relocs);
            return decodeRelocSym(_addr_ldr, symIdx, _addr_relocs, int32(commonsize(_addr_arch)) + int32(arch.PtrSize)); // 0x20 / 0x38
        }

        private static loader.Sym decodetypePtrElem(ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx)
        {
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref sys.Arch arch = ref _addr_arch.val;

            ref var relocs = ref heap(ldr.Relocs(symIdx), out ptr<var> _addr_relocs);
            return decodeRelocSym(_addr_ldr, symIdx, _addr_relocs, int32(commonsize(_addr_arch))); // 0x1c / 0x30
        }

        private static long decodetypeStructFieldCount(ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx)
        {
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref sys.Arch arch = ref _addr_arch.val;

            var data = ldr.Data(symIdx);
            return int(decodeInuxi(_addr_arch, data[commonsize(_addr_arch) + 2L * arch.PtrSize..], arch.PtrSize));
        }

        private static long decodetypeStructFieldArrayOff(ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx, long i)
        {
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref sys.Arch arch = ref _addr_arch.val;

            var data = ldr.Data(symIdx);
            var off = commonsize(_addr_arch) + 4L * arch.PtrSize;
            if (decodetypeHasUncommon(_addr_arch, data))
            {
                off += uncommonSize();
            }

            off += i * structfieldSize(_addr_arch);
            return off;

        }

        private static @string decodetypeStructFieldName(ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx, long i)
        {
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref sys.Arch arch = ref _addr_arch.val;

            var off = decodetypeStructFieldArrayOff(_addr_ldr, _addr_arch, symIdx, i);
            ref var relocs = ref heap(ldr.Relocs(symIdx), out ptr<var> _addr_relocs);
            return decodetypeName(_addr_ldr, symIdx, _addr_relocs, off);
        }

        private static loader.Sym decodetypeStructFieldType(ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx, long i)
        {
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref sys.Arch arch = ref _addr_arch.val;

            var off = decodetypeStructFieldArrayOff(_addr_ldr, _addr_arch, symIdx, i);
            ref var relocs = ref heap(ldr.Relocs(symIdx), out ptr<var> _addr_relocs);
            return decodeRelocSym(_addr_ldr, symIdx, _addr_relocs, int32(off + arch.PtrSize));
        }

        private static long decodetypeStructFieldOffsAnon(ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx, long i)
        {
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref sys.Arch arch = ref _addr_arch.val;

            var off = decodetypeStructFieldArrayOff(_addr_ldr, _addr_arch, symIdx, i);
            var data = ldr.Data(symIdx);
            return int64(decodeInuxi(_addr_arch, data[off + 2L * arch.PtrSize..], arch.PtrSize));
        }

        // decodetypeStr returns the contents of an rtype's str field (a nameOff).
        private static @string decodetypeStr(ptr<loader.Loader> _addr_ldr, ptr<sys.Arch> _addr_arch, loader.Sym symIdx)
        {
            ref loader.Loader ldr = ref _addr_ldr.val;
            ref sys.Arch arch = ref _addr_arch.val;

            ref var relocs = ref heap(ldr.Relocs(symIdx), out ptr<var> _addr_relocs);
            var str = decodetypeName(_addr_ldr, symIdx, _addr_relocs, 4L * arch.PtrSize + 8L);
            var data = ldr.Data(symIdx);
            if (data[2L * arch.PtrSize + 4L] & tflagExtraStar != 0L)
            {
                return str[1L..];
            }

            return str;

        }

        private static slice<byte> decodetypeGcmask(ptr<Link> _addr_ctxt, loader.Sym s)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (ctxt.loader.SymType(s) == sym.SDYNIMPORT)
            {
                var symData = ctxt.loader.Data(s);
                var addr = decodetypeGcprogShlib(_addr_ctxt, symData);
                var ptrdata = decodetypePtrdata(_addr_ctxt.Arch, symData);
                var sect = findShlibSection(_addr_ctxt, ctxt.loader.SymPkg(s), addr);
                if (sect != null)
                {
                    var bits = ptrdata / int64(ctxt.Arch.PtrSize);
                    var r = make_slice<byte>((bits + 7L) / 8L); 
                    // ldshlibsyms avoids closing the ELF file so sect.ReadAt works.
                    // If we remove this read (and the ones in decodetypeGcprog), we
                    // can close the file.
                    var (_, err) = sect.ReadAt(r, int64(addr - sect.Addr));
                    if (err != null)
                    {
                        log.Fatal(err);
                    }

                    return r;

                }

                Exitf("cannot find gcmask for %s", ctxt.loader.SymName(s));
                return null;

            }

            ref var relocs = ref heap(ctxt.loader.Relocs(s), out ptr<var> _addr_relocs);
            var mask = decodeRelocSym(_addr_ctxt.loader, s, _addr_relocs, 2L * int32(ctxt.Arch.PtrSize) + 8L + 1L * int32(ctxt.Arch.PtrSize));
            return ctxt.loader.Data(mask);

        }

        // Type.commonType.gc
        private static slice<byte> decodetypeGcprog(ptr<Link> _addr_ctxt, loader.Sym s)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            if (ctxt.loader.SymType(s) == sym.SDYNIMPORT)
            {
                var symData = ctxt.loader.Data(s);
                var addr = decodetypeGcprogShlib(_addr_ctxt, symData);
                var sect = findShlibSection(_addr_ctxt, ctxt.loader.SymPkg(s), addr);
                if (sect != null)
                { 
                    // A gcprog is a 4-byte uint32 indicating length, followed by
                    // the actual program.
                    var progsize = make_slice<byte>(4L);
                    var (_, err) = sect.ReadAt(progsize, int64(addr - sect.Addr));
                    if (err != null)
                    {
                        log.Fatal(err);
                    }

                    var progbytes = make_slice<byte>(ctxt.Arch.ByteOrder.Uint32(progsize));
                    _, err = sect.ReadAt(progbytes, int64(addr - sect.Addr + 4L));
                    if (err != null)
                    {
                        log.Fatal(err);
                    }

                    return append(progsize, progbytes);

                }

                Exitf("cannot find gcmask for %s", ctxt.loader.SymName(s));
                return null;

            }

            ref var relocs = ref heap(ctxt.loader.Relocs(s), out ptr<var> _addr_relocs);
            var rs = decodeRelocSym(_addr_ctxt.loader, s, _addr_relocs, 2L * int32(ctxt.Arch.PtrSize) + 8L + 1L * int32(ctxt.Arch.PtrSize));
            return ctxt.loader.Data(rs);

        }

        // Find the elf.Section of a given shared library that contains a given address.
        private static ptr<elf.Section> findShlibSection(ptr<Link> _addr_ctxt, @string path, ulong addr)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            foreach (var (_, shlib) in ctxt.Shlibs)
            {
                if (shlib.Path == path)
                {
                    foreach (var (_, sect) in shlib.File.Sections[1L..])
                    { // skip the NULL section
                        if (sect.Addr <= addr && addr <= sect.Addr + sect.Size)
                        {
                            return _addr_sect!;
                        }

                    }

                }

            }
            return _addr_null!;

        }

        private static ulong decodetypeGcprogShlib(ptr<Link> _addr_ctxt, slice<byte> data)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            return decodeInuxi(_addr_ctxt.Arch, data[2L * int32(ctxt.Arch.PtrSize) + 8L + 1L * int32(ctxt.Arch.PtrSize)..], ctxt.Arch.PtrSize);
        }
    }
}}}}
