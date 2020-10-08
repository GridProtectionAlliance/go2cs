// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2020 October 08 04:40:51 UTC
// import "cmd/oldlink/internal/ld" ==> using ld = go.cmd.oldlink.@internal.ld_package
// Original source: C:\Go\src\cmd\oldlink\internal\ld\decodesym.go
using bytes = go.bytes_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using sym = go.cmd.oldlink.@internal.sym_package;
using elf = go.debug.elf_package;
using fmt = go.fmt_package;
using log = go.log_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace oldlink {
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
        //    cmd/oldlink/internal/ld/decodesym.go
        //    reflect/type.go
        //    runtime/type.go
        private static readonly long tflagUncommon = (long)1L << (int)(0L);
        private static readonly long tflagExtraStar = (long)1L << (int)(1L);


        private static ptr<sym.Reloc> decodeReloc(ptr<sym.Symbol> _addr_s, int off)
        {
            ref sym.Symbol s = ref _addr_s.val;

            foreach (var (i) in s.R)
            {
                if (s.R[i].Off == off)
                {
                    return _addr__addr_s.R[i]!;
                }

            }
            return _addr_null!;

        }

        private static ptr<sym.Symbol> decodeRelocSym(ptr<sym.Symbol> _addr_s, int off)
        {
            ref sym.Symbol s = ref _addr_s.val;

            var r = decodeReloc(_addr_s, off);
            if (r == null)
            {
                return _addr_null!;
            }

            return _addr_r.Sym!;

        }

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

        // Type.commonType.gc
        private static slice<byte> decodetypeGcprog(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_s)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;

            if (s.Type == sym.SDYNIMPORT)
            {
                var addr = decodetypeGcprogShlib(_addr_ctxt, _addr_s);
                var sect = findShlibSection(_addr_ctxt, s.File, addr);
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

                Exitf("cannot find gcprog for %s", s.Name);
                return null;

            }

            return decodeRelocSym(_addr_s, 2L * int32(ctxt.Arch.PtrSize) + 8L + 1L * int32(ctxt.Arch.PtrSize)).P;

        }

        private static ulong decodetypeGcprogShlib(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_s)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;

            return decodeInuxi(_addr_ctxt.Arch, s.P[2L * int32(ctxt.Arch.PtrSize) + 8L + 1L * int32(ctxt.Arch.PtrSize)..], ctxt.Arch.PtrSize);
        }

        private static slice<byte> decodetypeGcmask(ptr<Link> _addr_ctxt, ptr<sym.Symbol> _addr_s)
        {
            ref Link ctxt = ref _addr_ctxt.val;
            ref sym.Symbol s = ref _addr_s.val;

            if (s.Type == sym.SDYNIMPORT)
            {
                var addr = decodetypeGcprogShlib(_addr_ctxt, _addr_s);
                var ptrdata = decodetypePtrdata(_addr_ctxt.Arch, s.P);
                var sect = findShlibSection(_addr_ctxt, s.File, addr);
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

                Exitf("cannot find gcmask for %s", s.Name);
                return null;

            }

            var mask = decodeRelocSym(_addr_s, 2L * int32(ctxt.Arch.PtrSize) + 8L + 1L * int32(ctxt.Arch.PtrSize));
            return mask.P;

        }

        // Type.ArrayType.elem and Type.SliceType.Elem
        private static ptr<sym.Symbol> decodetypeArrayElem(ptr<sys.Arch> _addr_arch, ptr<sym.Symbol> _addr_s)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbol s = ref _addr_s.val;

            return _addr_decodeRelocSym(_addr_s, int32(commonsize(_addr_arch)))!; // 0x1c / 0x30
        }

        private static long decodetypeArrayLen(ptr<sys.Arch> _addr_arch, ptr<sym.Symbol> _addr_s)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbol s = ref _addr_s.val;

            return int64(decodeInuxi(_addr_arch, s.P[commonsize(_addr_arch) + 2L * arch.PtrSize..], arch.PtrSize));
        }

        // Type.PtrType.elem
        private static ptr<sym.Symbol> decodetypePtrElem(ptr<sys.Arch> _addr_arch, ptr<sym.Symbol> _addr_s)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbol s = ref _addr_s.val;

            return _addr_decodeRelocSym(_addr_s, int32(commonsize(_addr_arch)))!; // 0x1c / 0x30
        }

        // Type.MapType.key, elem
        private static ptr<sym.Symbol> decodetypeMapKey(ptr<sys.Arch> _addr_arch, ptr<sym.Symbol> _addr_s)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbol s = ref _addr_s.val;

            return _addr_decodeRelocSym(_addr_s, int32(commonsize(_addr_arch)))!; // 0x1c / 0x30
        }

        private static ptr<sym.Symbol> decodetypeMapValue(ptr<sys.Arch> _addr_arch, ptr<sym.Symbol> _addr_s)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbol s = ref _addr_s.val;

            return _addr_decodeRelocSym(_addr_s, int32(commonsize(_addr_arch)) + int32(arch.PtrSize))!; // 0x20 / 0x38
        }

        // Type.ChanType.elem
        private static ptr<sym.Symbol> decodetypeChanElem(ptr<sys.Arch> _addr_arch, ptr<sym.Symbol> _addr_s)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbol s = ref _addr_s.val;

            return _addr_decodeRelocSym(_addr_s, int32(commonsize(_addr_arch)))!; // 0x1c / 0x30
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

        private static ptr<sym.Symbol> decodetypeFuncInType(ptr<sys.Arch> _addr_arch, ptr<sym.Symbol> _addr_s, long i)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbol s = ref _addr_s.val;

            var uadd = commonsize(_addr_arch) + 4L;
            if (arch.PtrSize == 8L)
            {
                uadd += 4L;
            }

            if (decodetypeHasUncommon(_addr_arch, s.P))
            {
                uadd += uncommonSize();
            }

            return _addr_decodeRelocSym(_addr_s, int32(uadd + i * arch.PtrSize))!;

        }

        private static ptr<sym.Symbol> decodetypeFuncOutType(ptr<sys.Arch> _addr_arch, ptr<sym.Symbol> _addr_s, long i)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbol s = ref _addr_s.val;

            return _addr_decodetypeFuncInType(_addr_arch, _addr_s, i + decodetypeFuncInCount(_addr_arch, s.P))!;
        }

        // Type.StructType.fields.Slice::length
        private static long decodetypeStructFieldCount(ptr<sys.Arch> _addr_arch, ptr<sym.Symbol> _addr_s)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbol s = ref _addr_s.val;

            return int(decodeInuxi(_addr_arch, s.P[commonsize(_addr_arch) + 2L * arch.PtrSize..], arch.PtrSize));
        }

        private static long decodetypeStructFieldArrayOff(ptr<sys.Arch> _addr_arch, ptr<sym.Symbol> _addr_s, long i)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbol s = ref _addr_s.val;

            var off = commonsize(_addr_arch) + 4L * arch.PtrSize;
            if (decodetypeHasUncommon(_addr_arch, s.P))
            {
                off += uncommonSize();
            }

            off += i * structfieldSize(_addr_arch);
            return off;

        }

        // decodetypeStr returns the contents of an rtype's str field (a nameOff).
        private static @string decodetypeStr(ptr<sys.Arch> _addr_arch, ptr<sym.Symbol> _addr_s)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbol s = ref _addr_s.val;

            var str = decodetypeName(_addr_s, 4L * arch.PtrSize + 8L);
            if (s.P[2L * arch.PtrSize + 4L] & tflagExtraStar != 0L)
            {
                return str[1L..];
            }

            return str;

        }

        // decodetypeName decodes the name from a reflect.name.
        private static @string decodetypeName(ptr<sym.Symbol> _addr_s, long off)
        {
            ref sym.Symbol s = ref _addr_s.val;

            var r = decodeReloc(_addr_s, int32(off));
            if (r == null)
            {
                return "";
            }

            var data = r.Sym.P;
            var namelen = int(uint16(data[1L]) << (int)(8L) | uint16(data[2L]));
            return string(data[3L..3L + namelen]);

        }

        private static @string decodetypeStructFieldName(ptr<sys.Arch> _addr_arch, ptr<sym.Symbol> _addr_s, long i)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbol s = ref _addr_s.val;

            var off = decodetypeStructFieldArrayOff(_addr_arch, _addr_s, i);
            return decodetypeName(_addr_s, off);
        }

        private static ptr<sym.Symbol> decodetypeStructFieldType(ptr<sys.Arch> _addr_arch, ptr<sym.Symbol> _addr_s, long i)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbol s = ref _addr_s.val;

            var off = decodetypeStructFieldArrayOff(_addr_arch, _addr_s, i);
            return _addr_decodeRelocSym(_addr_s, int32(off + arch.PtrSize))!;
        }

        private static long decodetypeStructFieldOffs(ptr<sys.Arch> _addr_arch, ptr<sym.Symbol> _addr_s, long i)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbol s = ref _addr_s.val;

            return decodetypeStructFieldOffsAnon(_addr_arch, _addr_s, i) >> (int)(1L);
        }

        private static long decodetypeStructFieldOffsAnon(ptr<sys.Arch> _addr_arch, ptr<sym.Symbol> _addr_s, long i)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbol s = ref _addr_s.val;

            var off = decodetypeStructFieldArrayOff(_addr_arch, _addr_s, i);
            return int64(decodeInuxi(_addr_arch, s.P[off + 2L * arch.PtrSize..], arch.PtrSize));
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


        // decodeMethodSig decodes an array of method signature information.
        // Each element of the array is size bytes. The first 4 bytes is a
        // nameOff for the method name, and the next 4 bytes is a typeOff for
        // the function type.
        //
        // Conveniently this is the layout of both runtime.method and runtime.imethod.
        private static slice<methodsig> decodeMethodSig(ptr<sys.Arch> _addr_arch, ptr<sym.Symbol> _addr_s, long off, long size, long count)
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbol s = ref _addr_s.val;

            bytes.Buffer buf = default;
            slice<methodsig> methods = default;
            {
                long i__prev1 = i;

                for (long i = 0L; i < count; i++)
                {
                    buf.WriteString(decodetypeName(_addr_s, off));
                    var mtypSym = decodeRelocSym(_addr_s, int32(off + 4L));

                    buf.WriteRune('(');
                    var inCount = decodetypeFuncInCount(_addr_arch, mtypSym.P);
                    {
                        long i__prev2 = i;

                        for (i = 0L; i < inCount; i++)
                        {
                            if (i > 0L)
                            {
                                buf.WriteString(", ");
                            }

                            buf.WriteString(decodetypeFuncInType(_addr_arch, _addr_mtypSym, i).Name);

                        }


                        i = i__prev2;
                    }
                    buf.WriteString(") (");
                    var outCount = decodetypeFuncOutCount(_addr_arch, mtypSym.P);
                    {
                        long i__prev2 = i;

                        for (i = 0L; i < outCount; i++)
                        {
                            if (i > 0L)
                            {
                                buf.WriteString(", ");
                            }

                            buf.WriteString(decodetypeFuncOutType(_addr_arch, _addr_mtypSym, i).Name);

                        }


                        i = i__prev2;
                    }
                    buf.WriteRune(')');

                    off += size;
                    methods = append(methods, methodsig(buf.String()));
                    buf.Reset();

                }


                i = i__prev1;
            }
            return methods;

        }

        private static slice<methodsig> decodeIfaceMethods(ptr<sys.Arch> _addr_arch, ptr<sym.Symbol> _addr_s) => func((_, panic, __) =>
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbol s = ref _addr_s.val;

            if (decodetypeKind(_addr_arch, s.P) & kindMask != kindInterface)
            {
                panic(fmt.Sprintf("symbol %q is not an interface", s.Name));
            }

            var r = decodeReloc(_addr_s, int32(commonsize(_addr_arch) + arch.PtrSize));
            if (r == null)
            {
                return null;
            }

            if (r.Sym != s)
            {
                panic(fmt.Sprintf("imethod slice pointer in %q leads to a different symbol", s.Name));
            }

            var off = int(r.Add); // array of reflect.imethod values
            var numMethods = int(decodetypeIfaceMethodCount(_addr_arch, s.P));
            long sizeofIMethod = 4L + 4L;
            return decodeMethodSig(_addr_arch, _addr_s, off, sizeofIMethod, numMethods);

        });

        private static slice<methodsig> decodetypeMethods(ptr<sys.Arch> _addr_arch, ptr<sym.Symbol> _addr_s) => func((_, panic, __) =>
        {
            ref sys.Arch arch = ref _addr_arch.val;
            ref sym.Symbol s = ref _addr_s.val;

            if (!decodetypeHasUncommon(_addr_arch, s.P))
            {
                panic(fmt.Sprintf("no methods on %q", s.Name));
            }

            var off = commonsize(_addr_arch); // reflect.rtype

            if (decodetypeKind(_addr_arch, s.P) & kindMask == kindStruct) // reflect.structType
                off += 4L * arch.PtrSize;
            else if (decodetypeKind(_addr_arch, s.P) & kindMask == kindPtr) // reflect.ptrType
                off += arch.PtrSize;
            else if (decodetypeKind(_addr_arch, s.P) & kindMask == kindFunc) // reflect.funcType
                off += arch.PtrSize; // 4 bytes, pointer aligned
            else if (decodetypeKind(_addr_arch, s.P) & kindMask == kindSlice) // reflect.sliceType
                off += arch.PtrSize;
            else if (decodetypeKind(_addr_arch, s.P) & kindMask == kindArray) // reflect.arrayType
                off += 3L * arch.PtrSize;
            else if (decodetypeKind(_addr_arch, s.P) & kindMask == kindChan) // reflect.chanType
                off += 2L * arch.PtrSize;
            else if (decodetypeKind(_addr_arch, s.P) & kindMask == kindMap) // reflect.mapType
                off += 4L * arch.PtrSize + 8L;
            else if (decodetypeKind(_addr_arch, s.P) & kindMask == kindInterface) // reflect.interfaceType
                off += 3L * arch.PtrSize;
            else                         var mcount = int(decodeInuxi(_addr_arch, s.P[off + 4L..], 2L));
            var moff = int(decodeInuxi(_addr_arch, s.P[off + 4L + 2L + 2L..], 4L));
            off += moff; // offset to array of reflect.method values
            const long sizeofMethod = (long)4L * 4L; // sizeof reflect.method in program
 // sizeof reflect.method in program
            return decodeMethodSig(_addr_arch, _addr_s, off, sizeofMethod, mcount);

        });
    }
}}}}
