// Copyright 2012 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ld -- go2cs converted at 2020 August 29 10:03:19 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\decodesym.go
using bytes = go.bytes_package;
using objabi = go.cmd.@internal.objabi_package;
using sys = go.cmd.@internal.sys_package;
using sym = go.cmd.link.@internal.sym_package;
using elf = go.debug.elf_package;
using fmt = go.fmt_package;
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
        private static readonly long tflagUncommon = 1L << (int)(0L);
        private static readonly long tflagExtraStar = 1L << (int)(1L);

        private static ref sym.Reloc decodeReloc(ref sym.Symbol s, int off)
        {
            foreach (var (i) in s.R)
            {
                if (s.R[i].Off == off)
                {
                    return ref s.R[i];
                }
            }
            return null;
        }

        private static ref sym.Symbol decodeRelocSym(ref sym.Symbol s, int off)
        {
            var r = decodeReloc(s, off);
            if (r == null)
            {
                return null;
            }
            return r.Sym;
        }

        private static ulong decodeInuxi(ref sys.Arch _arch, slice<byte> p, long sz) => func(_arch, (ref sys.Arch arch, Defer _, Panic panic, Recover __) =>
        {
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

        private static long commonsize(ref sys.Arch arch)
        {
            return 4L * arch.PtrSize + 8L + 8L;
        } // runtime._type
        private static long structfieldSize(ref sys.Arch arch)
        {
            return 3L * arch.PtrSize;
        } // runtime.structfield
        private static long uncommonSize()
        {
            return 4L + 2L + 2L + 4L + 4L;
        } // runtime.uncommontype

        // Type.commonType.kind
        private static byte decodetypeKind(ref sys.Arch arch, ref sym.Symbol s)
        {
            return s.P[2L * arch.PtrSize + 7L] & objabi.KindMask; //  0x13 / 0x1f
        }

        // Type.commonType.kind
        private static byte decodetypeUsegcprog(ref sys.Arch arch, ref sym.Symbol s)
        {
            return s.P[2L * arch.PtrSize + 7L] & objabi.KindGCProg; //  0x13 / 0x1f
        }

        // Type.commonType.size
        private static long decodetypeSize(ref sys.Arch arch, ref sym.Symbol s)
        {
            return int64(decodeInuxi(arch, s.P, arch.PtrSize)); // 0x8 / 0x10
        }

        // Type.commonType.ptrdata
        private static long decodetypePtrdata(ref sys.Arch arch, ref sym.Symbol s)
        {
            return int64(decodeInuxi(arch, s.P[arch.PtrSize..], arch.PtrSize)); // 0x8 / 0x10
        }

        // Type.commonType.tflag
        private static bool decodetypeHasUncommon(ref sys.Arch arch, ref sym.Symbol s)
        {
            return s.P[2L * arch.PtrSize + 4L] & tflagUncommon != 0L;
        }

        // Find the elf.Section of a given shared library that contains a given address.
        private static ref elf.Section findShlibSection(ref Link ctxt, @string path, ulong addr)
        {
            foreach (var (_, shlib) in ctxt.Shlibs)
            {
                if (shlib.Path == path)
                {
                    foreach (var (_, sect) in shlib.File.Sections)
                    {
                        if (sect.Addr <= addr && addr <= sect.Addr + sect.Size)
                        {
                            return sect;
                        }
                    }
                }
            }
            return null;
        }

        // Type.commonType.gc
        private static slice<byte> decodetypeGcprog(ref Link ctxt, ref sym.Symbol s)
        {
            if (s.Type == sym.SDYNIMPORT)
            {
                var addr = decodetypeGcprogShlib(ctxt, s);
                var sect = findShlibSection(ctxt, s.File, addr);
                if (sect != null)
                { 
                    // A gcprog is a 4-byte uint32 indicating length, followed by
                    // the actual program.
                    var progsize = make_slice<byte>(4L);
                    sect.ReadAt(progsize, int64(addr - sect.Addr));
                    var progbytes = make_slice<byte>(ctxt.Arch.ByteOrder.Uint32(progsize));
                    sect.ReadAt(progbytes, int64(addr - sect.Addr + 4L));
                    return append(progsize, progbytes);
                }
                Exitf("cannot find gcprog for %s", s.Name);
                return null;
            }
            return decodeRelocSym(s, 2L * int32(ctxt.Arch.PtrSize) + 8L + 1L * int32(ctxt.Arch.PtrSize)).P;
        }

        private static ulong decodetypeGcprogShlib(ref Link ctxt, ref sym.Symbol s)
        {
            if (ctxt.Arch.Family == sys.ARM64)
            {
                foreach (var (_, shlib) in ctxt.Shlibs)
                {
                    if (shlib.Path == s.File)
                    {
                        return shlib.gcdataAddresses[s];
                    }
                }
                return 0L;
            }
            return decodeInuxi(ctxt.Arch, s.P[2L * int32(ctxt.Arch.PtrSize) + 8L + 1L * int32(ctxt.Arch.PtrSize)..], ctxt.Arch.PtrSize);
        }

        private static slice<byte> decodetypeGcmask(ref Link ctxt, ref sym.Symbol s)
        {
            if (s.Type == sym.SDYNIMPORT)
            {
                var addr = decodetypeGcprogShlib(ctxt, s);
                var ptrdata = decodetypePtrdata(ctxt.Arch, s);
                var sect = findShlibSection(ctxt, s.File, addr);
                if (sect != null)
                {
                    var r = make_slice<byte>(ptrdata / int64(ctxt.Arch.PtrSize));
                    sect.ReadAt(r, int64(addr - sect.Addr));
                    return r;
                }
                Exitf("cannot find gcmask for %s", s.Name);
                return null;
            }
            var mask = decodeRelocSym(s, 2L * int32(ctxt.Arch.PtrSize) + 8L + 1L * int32(ctxt.Arch.PtrSize));
            return mask.P;
        }

        // Type.ArrayType.elem and Type.SliceType.Elem
        private static ref sym.Symbol decodetypeArrayElem(ref sys.Arch arch, ref sym.Symbol s)
        {
            return decodeRelocSym(s, int32(commonsize(arch))); // 0x1c / 0x30
        }

        private static long decodetypeArrayLen(ref sys.Arch arch, ref sym.Symbol s)
        {
            return int64(decodeInuxi(arch, s.P[commonsize(arch) + 2L * arch.PtrSize..], arch.PtrSize));
        }

        // Type.PtrType.elem
        private static ref sym.Symbol decodetypePtrElem(ref sys.Arch arch, ref sym.Symbol s)
        {
            return decodeRelocSym(s, int32(commonsize(arch))); // 0x1c / 0x30
        }

        // Type.MapType.key, elem
        private static ref sym.Symbol decodetypeMapKey(ref sys.Arch arch, ref sym.Symbol s)
        {
            return decodeRelocSym(s, int32(commonsize(arch))); // 0x1c / 0x30
        }

        private static ref sym.Symbol decodetypeMapValue(ref sys.Arch arch, ref sym.Symbol s)
        {
            return decodeRelocSym(s, int32(commonsize(arch)) + int32(arch.PtrSize)); // 0x20 / 0x38
        }

        // Type.ChanType.elem
        private static ref sym.Symbol decodetypeChanElem(ref sys.Arch arch, ref sym.Symbol s)
        {
            return decodeRelocSym(s, int32(commonsize(arch))); // 0x1c / 0x30
        }

        // Type.FuncType.dotdotdot
        private static bool decodetypeFuncDotdotdot(ref sys.Arch arch, ref sym.Symbol s)
        {
            return uint16(decodeInuxi(arch, s.P[commonsize(arch) + 2L..], 2L)) & (1L << (int)(15L)) != 0L;
        }

        // Type.FuncType.inCount
        private static long decodetypeFuncInCount(ref sys.Arch arch, ref sym.Symbol s)
        {
            return int(decodeInuxi(arch, s.P[commonsize(arch)..], 2L));
        }

        private static long decodetypeFuncOutCount(ref sys.Arch arch, ref sym.Symbol s)
        {
            return int(uint16(decodeInuxi(arch, s.P[commonsize(arch) + 2L..], 2L)) & (1L << (int)(15L) - 1L));
        }

        private static ref sym.Symbol decodetypeFuncInType(ref sys.Arch arch, ref sym.Symbol s, long i)
        {
            var uadd = commonsize(arch) + 4L;
            if (arch.PtrSize == 8L)
            {
                uadd += 4L;
            }
            if (decodetypeHasUncommon(arch, s))
            {
                uadd += uncommonSize();
            }
            return decodeRelocSym(s, int32(uadd + i * arch.PtrSize));
        }

        private static ref sym.Symbol decodetypeFuncOutType(ref sys.Arch arch, ref sym.Symbol s, long i)
        {
            return decodetypeFuncInType(arch, s, i + decodetypeFuncInCount(arch, s));
        }

        // Type.StructType.fields.Slice::length
        private static long decodetypeStructFieldCount(ref sys.Arch arch, ref sym.Symbol s)
        {
            return int(decodeInuxi(arch, s.P[commonsize(arch) + 2L * arch.PtrSize..], arch.PtrSize));
        }

        private static long decodetypeStructFieldArrayOff(ref sys.Arch arch, ref sym.Symbol s, long i)
        {
            var off = commonsize(arch) + 4L * arch.PtrSize;
            if (decodetypeHasUncommon(arch, s))
            {
                off += uncommonSize();
            }
            off += i * structfieldSize(arch);
            return off;
        }

        // decodetypeStr returns the contents of an rtype's str field (a nameOff).
        private static @string decodetypeStr(ref sys.Arch arch, ref sym.Symbol s)
        {
            var str = decodetypeName(s, 4L * arch.PtrSize + 8L);
            if (s.P[2L * arch.PtrSize + 4L] & tflagExtraStar != 0L)
            {
                return str[1L..];
            }
            return str;
        }

        // decodetypeName decodes the name from a reflect.name.
        private static @string decodetypeName(ref sym.Symbol s, long off)
        {
            var r = decodeReloc(s, int32(off));
            if (r == null)
            {
                return "";
            }
            var data = r.Sym.P;
            var namelen = int(uint16(data[1L]) << (int)(8L) | uint16(data[2L]));
            return string(data[3L..3L + namelen]);
        }

        private static @string decodetypeStructFieldName(ref sys.Arch arch, ref sym.Symbol s, long i)
        {
            var off = decodetypeStructFieldArrayOff(arch, s, i);
            return decodetypeName(s, off);
        }

        private static ref sym.Symbol decodetypeStructFieldType(ref sys.Arch arch, ref sym.Symbol s, long i)
        {
            var off = decodetypeStructFieldArrayOff(arch, s, i);
            return decodeRelocSym(s, int32(off + arch.PtrSize));
        }

        private static long decodetypeStructFieldOffs(ref sys.Arch arch, ref sym.Symbol s, long i)
        {
            return decodetypeStructFieldOffsAnon(arch, s, i) >> (int)(1L);
        }

        private static long decodetypeStructFieldOffsAnon(ref sys.Arch arch, ref sym.Symbol s, long i)
        {
            var off = decodetypeStructFieldArrayOff(arch, s, i);
            return int64(decodeInuxi(arch, s.P[off + 2L * arch.PtrSize..], arch.PtrSize));
        }

        // InterfaceType.methods.length
        private static long decodetypeIfaceMethodCount(ref sys.Arch arch, ref sym.Symbol s)
        {
            return int64(decodeInuxi(arch, s.P[commonsize(arch) + 2L * arch.PtrSize..], arch.PtrSize));
        }

        // methodsig is a fully qualified typed method signature, like
        // "Visit(type.go/ast.Node) (type.go/ast.Visitor)".
        private partial struct methodsig // : @string
        {
        }

        // Matches runtime/typekind.go and reflect.Kind.
        private static readonly long kindArray = 17L;
        private static readonly long kindChan = 18L;
        private static readonly long kindFunc = 19L;
        private static readonly long kindInterface = 20L;
        private static readonly long kindMap = 21L;
        private static readonly long kindPtr = 22L;
        private static readonly long kindSlice = 23L;
        private static readonly long kindStruct = 25L;
        private static readonly long kindMask = (1L << (int)(5L)) - 1L;

        // decodeMethodSig decodes an array of method signature information.
        // Each element of the array is size bytes. The first 4 bytes is a
        // nameOff for the method name, and the next 4 bytes is a typeOff for
        // the function type.
        //
        // Conveniently this is the layout of both runtime.method and runtime.imethod.
        private static slice<methodsig> decodeMethodSig(ref sys.Arch arch, ref sym.Symbol s, long off, long size, long count)
        {
            bytes.Buffer buf = default;
            slice<methodsig> methods = default;
            {
                long i__prev1 = i;

                for (long i = 0L; i < count; i++)
                {
                    buf.WriteString(decodetypeName(s, off));
                    var mtypSym = decodeRelocSym(s, int32(off + 4L));

                    buf.WriteRune('(');
                    var inCount = decodetypeFuncInCount(arch, mtypSym);
                    {
                        long i__prev2 = i;

                        for (i = 0L; i < inCount; i++)
                        {
                            if (i > 0L)
                            {
                                buf.WriteString(", ");
                            }
                            buf.WriteString(decodetypeFuncInType(arch, mtypSym, i).Name);
                        }


                        i = i__prev2;
                    }
                    buf.WriteString(") (");
                    var outCount = decodetypeFuncOutCount(arch, mtypSym);
                    {
                        long i__prev2 = i;

                        for (i = 0L; i < outCount; i++)
                        {
                            if (i > 0L)
                            {
                                buf.WriteString(", ");
                            }
                            buf.WriteString(decodetypeFuncOutType(arch, mtypSym, i).Name);
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

        private static slice<methodsig> decodeIfaceMethods(ref sys.Arch _arch, ref sym.Symbol _s) => func(_arch, _s, (ref sys.Arch arch, ref sym.Symbol s, Defer _, Panic panic, Recover __) =>
        {
            if (decodetypeKind(arch, s) & kindMask != kindInterface)
            {
                panic(fmt.Sprintf("symbol %q is not an interface", s.Name));
            }
            var r = decodeReloc(s, int32(commonsize(arch) + arch.PtrSize));
            if (r == null)
            {
                return null;
            }
            if (r.Sym != s)
            {
                panic(fmt.Sprintf("imethod slice pointer in %q leads to a different symbol", s.Name));
            }
            var off = int(r.Add); // array of reflect.imethod values
            var numMethods = int(decodetypeIfaceMethodCount(arch, s));
            long sizeofIMethod = 4L + 4L;
            return decodeMethodSig(arch, s, off, sizeofIMethod, numMethods);
        });

        private static slice<methodsig> decodetypeMethods(ref sys.Arch _arch, ref sym.Symbol _s) => func(_arch, _s, (ref sys.Arch arch, ref sym.Symbol s, Defer _, Panic panic, Recover __) =>
        {
            if (!decodetypeHasUncommon(arch, s))
            {
                panic(fmt.Sprintf("no methods on %q", s.Name));
            }
            var off = commonsize(arch); // reflect.rtype

            if (decodetypeKind(arch, s) & kindMask == kindStruct) // reflect.structType
                off += 4L * arch.PtrSize;
            else if (decodetypeKind(arch, s) & kindMask == kindPtr) // reflect.ptrType
                off += arch.PtrSize;
            else if (decodetypeKind(arch, s) & kindMask == kindFunc) // reflect.funcType
                off += arch.PtrSize; // 4 bytes, pointer aligned
            else if (decodetypeKind(arch, s) & kindMask == kindSlice) // reflect.sliceType
                off += arch.PtrSize;
            else if (decodetypeKind(arch, s) & kindMask == kindArray) // reflect.arrayType
                off += 3L * arch.PtrSize;
            else if (decodetypeKind(arch, s) & kindMask == kindChan) // reflect.chanType
                off += 2L * arch.PtrSize;
            else if (decodetypeKind(arch, s) & kindMask == kindMap) // reflect.mapType
                off += 4L * arch.PtrSize + 8L;
            else if (decodetypeKind(arch, s) & kindMask == kindInterface) // reflect.interfaceType
                off += 3L * arch.PtrSize;
            else                         var mcount = int(decodeInuxi(arch, s.P[off + 4L..], 2L));
            var moff = int(decodeInuxi(arch, s.P[off + 4L + 2L + 2L..], 4L));
            off += moff; // offset to array of reflect.method values
            const long sizeofMethod = 4L * 4L; // sizeof reflect.method in program
 // sizeof reflect.method in program
            return decodeMethodSig(arch, s, off, sizeofMethod, mcount);
        });
    }
}}}}
