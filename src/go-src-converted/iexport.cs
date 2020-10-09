// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Indexed binary package export.
// This file was derived from $GOROOT/src/cmd/compile/internal/gc/iexport.go;
// see that file for specification of the format.

// package gcimporter -- go2cs converted at 2020 October 09 06:02:17 UTC
// import "golang.org/x/tools/go/internal/gcimporter" ==> using gcimporter = go.golang.org.x.tools.go.@internal.gcimporter_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\internal\gcimporter\iexport.go
using bytes = go.bytes_package;
using binary = go.encoding.binary_package;
using ast = go.go.ast_package;
using constant = go.go.constant_package;
using token = go.go.token_package;
using types = go.go.types_package;
using io = go.io_package;
using big = go.math.big_package;
using reflect = go.reflect_package;
using sort = go.sort_package;
using static go.builtin;
using System;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace @internal
{
    public static partial class gcimporter_package
    {
        // Current indexed export format version. Increase with each format change.
        // 0: Go1.11 encoding
        private static readonly long iexportVersion = (long)0L;

        // IExportData returns the binary export data for pkg.
        //
        // If no file set is provided, position info will be missing.
        // The package path of the top-level package will not be recorded,
        // so that calls to IImportData can override with a provided package path.


        // IExportData returns the binary export data for pkg.
        //
        // If no file set is provided, position info will be missing.
        // The package path of the top-level package will not be recorded,
        // so that calls to IImportData can override with a provided package path.
        public static (slice<byte>, error) IExportData(ptr<token.FileSet> _addr_fset, ptr<types.Package> _addr_pkg) => func((defer, panic, _) =>
        {
            slice<byte> b = default;
            error err = default!;
            ref token.FileSet fset = ref _addr_fset.val;
            ref types.Package pkg = ref _addr_pkg.val;

            defer(() =>
            {
                {
                    var e = recover();

                    if (e != null)
                    {
                        {
                            internalError (ierr, ok) = e._<internalError>();

                            if (ok)
                            {
                                err = ierr;
                                return ;
                            } 
                            // Not an internal error; panic again.

                        } 
                        // Not an internal error; panic again.
                        panic(e);

                    }

                }

            }());

            iexporter p = new iexporter(out:bytes.NewBuffer(nil),fset:fset,allPkgs:map[*types.Package]bool{},stringIndex:map[string]uint64{},declIndex:map[types.Object]uint64{},typIndex:map[types.Type]uint64{},localpkg:pkg,);

            foreach (var (i, pt) in predeclared())
            {
                p.typIndex[pt] = uint64(i);
            }
            if (len(p.typIndex) > predeclReserved)
            {
                panic(internalErrorf("too many predeclared types: %d > %d", len(p.typIndex), predeclReserved));
            } 

            // Initialize work queue with exported declarations.
            var scope = pkg.Scope();
            foreach (var (_, name) in scope.Names())
            {
                if (ast.IsExported(name))
                {
                    p.pushDecl(scope.Lookup(name));
                }

            } 

            // Loop until no more work.
            while (!p.declTodo.empty())
            {
                p.doDecl(p.declTodo.popHead());
            } 

            // Append indices to data0 section.
 

            // Append indices to data0 section.
            var dataLen = uint64(p.data0.Len());
            var w = p.newWriter();
            w.writeIndex(p.declIndex);
            w.flush(); 

            // Assemble header.
            ref intWriter hdr = ref heap(out ptr<intWriter> _addr_hdr);
            hdr.WriteByte('i');
            hdr.uint64(iexportVersion);
            hdr.uint64(uint64(p.strings.Len()));
            hdr.uint64(dataLen); 

            // Flush output.
            io.Copy(p.@out, _addr_hdr);
            io.Copy(p.@out, _addr_p.strings);
            io.Copy(p.@out, _addr_p.data0);

            return (p.@out.Bytes(), error.As(null!)!);

        });

        // writeIndex writes out an object index. mainIndex indicates whether
        // we're writing out the main index, which is also read by
        // non-compiler tools and includes a complete package description
        // (i.e., name and height).
        private static void writeIndex(this ptr<exportWriter> _addr_w, map<types.Object, ulong> index)
        {
            ref exportWriter w = ref _addr_w.val;
 
            // Build a map from packages to objects from that package.
            map pkgObjs = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<types.Package>, slice<types.Object>>{}; 

            // For the main index, make sure to include every package that
            // we reference, even if we're not exporting (or reexporting)
            // any symbols from it.
            pkgObjs[w.p.localpkg] = null;
            {
                var pkg__prev1 = pkg;

                foreach (var (__pkg) in w.p.allPkgs)
                {
                    pkg = __pkg;
                    pkgObjs[pkg] = null;
                }

                pkg = pkg__prev1;
            }

            {
                var obj__prev1 = obj;

                foreach (var (__obj) in index)
                {
                    obj = __obj;
                    pkgObjs[obj.Pkg()] = append(pkgObjs[obj.Pkg()], obj);
                }

                obj = obj__prev1;
            }

            slice<ptr<types.Package>> pkgs = default;
            {
                var pkg__prev1 = pkg;
                var objs__prev1 = objs;

                foreach (var (__pkg, __objs) in pkgObjs)
                {
                    pkg = __pkg;
                    objs = __objs;
                    pkgs = append(pkgs, pkg);

                    sort.Slice(objs, (i, j) =>
                    {
                        return objs[i].Name() < objs[j].Name();
                    });

                }

                pkg = pkg__prev1;
                objs = objs__prev1;
            }

            sort.Slice(pkgs, (i, j) =>
            {
                return w.exportPath(pkgs[i]) < w.exportPath(pkgs[j]);
            });

            w.uint64(uint64(len(pkgs)));
            {
                var pkg__prev1 = pkg;

                foreach (var (_, __pkg) in pkgs)
                {
                    pkg = __pkg;
                    w.@string(w.exportPath(pkg));
                    w.@string(pkg.Name());
                    w.uint64(uint64(0L)); // package height is not needed for go/types

                    var objs = pkgObjs[pkg];
                    w.uint64(uint64(len(objs)));
                    {
                        var obj__prev2 = obj;

                        foreach (var (_, __obj) in objs)
                        {
                            obj = __obj;
                            w.@string(obj.Name());
                            w.uint64(index[obj]);
                        }

                        obj = obj__prev2;
                    }
                }

                pkg = pkg__prev1;
            }
        }

        private partial struct iexporter
        {
            public ptr<token.FileSet> fset;
            public ptr<bytes.Buffer> @out;
            public ptr<types.Package> localpkg; // allPkgs tracks all packages that have been referenced by
// the export data, so we can ensure to include them in the
// main index.
            public map<ptr<types.Package>, bool> allPkgs;
            public objQueue declTodo;
            public intWriter strings;
            public map<@string, ulong> stringIndex;
            public intWriter data0;
            public map<types.Object, ulong> declIndex;
            public map<types.Type, ulong> typIndex;
        }

        // stringOff returns the offset of s within the string section.
        // If not already present, it's added to the end.
        private static ulong stringOff(this ptr<iexporter> _addr_p, @string s)
        {
            ref iexporter p = ref _addr_p.val;

            var (off, ok) = p.stringIndex[s];
            if (!ok)
            {
                off = uint64(p.strings.Len());
                p.stringIndex[s] = off;

                p.strings.uint64(uint64(len(s)));
                p.strings.WriteString(s);
            }

            return off;

        }

        // pushDecl adds n to the declaration work queue, if not already present.
        private static void pushDecl(this ptr<iexporter> _addr_p, types.Object obj)
        {
            ref iexporter p = ref _addr_p.val;
 
            // Package unsafe is known to the compiler and predeclared.
            assert(obj.Pkg() != types.Unsafe);

            {
                var (_, ok) = p.declIndex[obj];

                if (ok)
                {
                    return ;
                }

            }


            p.declIndex[obj] = ~uint64(0L); // mark n present in work queue
            p.declTodo.pushTail(obj);

        }

        // exportWriter handles writing out individual data section chunks.
        private partial struct exportWriter
        {
            public ptr<iexporter> p;
            public intWriter data;
            public ptr<types.Package> currPkg;
            public @string prevFile;
            public long prevLine;
        }

        private static @string exportPath(this ptr<exportWriter> _addr_w, ptr<types.Package> _addr_pkg)
        {
            ref exportWriter w = ref _addr_w.val;
            ref types.Package pkg = ref _addr_pkg.val;

            if (pkg == w.p.localpkg)
            {
                return "";
            }

            return pkg.Path();

        }

        private static void doDecl(this ptr<iexporter> _addr_p, types.Object obj) => func((_, panic, __) =>
        {
            ref iexporter p = ref _addr_p.val;

            var w = p.newWriter();
            w.setPkg(obj.Pkg(), false);

            switch (obj.type())
            {
                case ptr<types.Var> obj:
                    w.tag('V');
                    w.pos(obj.Pos());
                    w.typ(obj.Type(), obj.Pkg());
                    break;
                case ptr<types.Func> obj:
                    ptr<types.Signature> (sig, _) = obj.Type()._<ptr<types.Signature>>();
                    if (sig.Recv() != null)
                    {
                        panic(internalErrorf("unexpected method: %v", sig));
                    }

                    w.tag('F');
                    w.pos(obj.Pos());
                    w.signature(sig);
                    break;
                case ptr<types.Const> obj:
                    w.tag('C');
                    w.pos(obj.Pos());
                    w.value(obj.Type(), obj.Val());
                    break;
                case ptr<types.TypeName> obj:
                    if (obj.IsAlias())
                    {
                        w.tag('A');
                        w.pos(obj.Pos());
                        w.typ(obj.Type(), obj.Pkg());
                        break;
                    } 

                    // Defined type.
                    w.tag('T');
                    w.pos(obj.Pos());

                    var underlying = obj.Type().Underlying();
                    w.typ(underlying, obj.Pkg());

                    var t = obj.Type();
                    if (types.IsInterface(t))
                    {
                        break;
                    }

                    ptr<types.Named> (named, ok) = t._<ptr<types.Named>>();
                    if (!ok)
                    {
                        panic(internalErrorf("%s is not a defined type", t));
                    }

                    var n = named.NumMethods();
                    w.uint64(uint64(n));
                    for (long i = 0L; i < n; i++)
                    {
                        var m = named.Method(i);
                        w.pos(m.Pos());
                        w.@string(m.Name());
                        (sig, _) = m.Type()._<ptr<types.Signature>>();
                        w.param(sig.Recv());
                        w.signature(sig);
                    }

                    break;
                default:
                {
                    var obj = obj.type();
                    panic(internalErrorf("unexpected object: %v", obj));
                    break;
                }

            }

            p.declIndex[obj] = w.flush();

        });

        private static void tag(this ptr<exportWriter> _addr_w, byte tag)
        {
            ref exportWriter w = ref _addr_w.val;

            w.data.WriteByte(tag);
        }

        private static void pos(this ptr<exportWriter> _addr_w, token.Pos pos)
        {
            ref exportWriter w = ref _addr_w.val;

            if (w.p.fset == null)
            {
                w.int64(0L);
                return ;
            }

            var p = w.p.fset.Position(pos);
            var file = p.Filename;
            var line = int64(p.Line); 

            // When file is the same as the last position (common case),
            // we can save a few bytes by delta encoding just the line
            // number.
            //
            // Note: Because data objects may be read out of order (or not
            // at all), we can only apply delta encoding within a single
            // object. This is handled implicitly by tracking prevFile and
            // prevLine as fields of exportWriter.

            if (file == w.prevFile)
            {
                var delta = line - w.prevLine;
                w.int64(delta);
                if (delta == deltaNewFile)
                {
                    w.int64(-1L);
                }

            }
            else
            {
                w.int64(deltaNewFile);
                w.int64(line); // line >= 0
                w.@string(file);
                w.prevFile = file;

            }

            w.prevLine = line;

        }

        private static void pkg(this ptr<exportWriter> _addr_w, ptr<types.Package> _addr_pkg)
        {
            ref exportWriter w = ref _addr_w.val;
            ref types.Package pkg = ref _addr_pkg.val;
 
            // Ensure any referenced packages are declared in the main index.
            w.p.allPkgs[pkg] = true;

            w.@string(w.exportPath(pkg));

        }

        private static void qualifiedIdent(this ptr<exportWriter> _addr_w, types.Object obj)
        {
            ref exportWriter w = ref _addr_w.val;
 
            // Ensure any referenced declarations are written out too.
            w.p.pushDecl(obj);

            w.@string(obj.Name());
            w.pkg(obj.Pkg());

        }

        private static void typ(this ptr<exportWriter> _addr_w, types.Type t, ptr<types.Package> _addr_pkg)
        {
            ref exportWriter w = ref _addr_w.val;
            ref types.Package pkg = ref _addr_pkg.val;

            w.data.uint64(w.p.typOff(t, pkg));
        }

        private static ptr<exportWriter> newWriter(this ptr<iexporter> _addr_p)
        {
            ref iexporter p = ref _addr_p.val;

            return addr(new exportWriter(p:p));
        }

        private static ulong flush(this ptr<exportWriter> _addr_w)
        {
            ref exportWriter w = ref _addr_w.val;

            var off = uint64(w.p.data0.Len());
            io.Copy(_addr_w.p.data0, _addr_w.data);
            return off;
        }

        private static ulong typOff(this ptr<iexporter> _addr_p, types.Type t, ptr<types.Package> _addr_pkg)
        {
            ref iexporter p = ref _addr_p.val;
            ref types.Package pkg = ref _addr_pkg.val;

            var (off, ok) = p.typIndex[t];
            if (!ok)
            {
                var w = p.newWriter();
                w.doTyp(t, pkg);
                off = predeclReserved + w.flush();
                p.typIndex[t] = off;
            }

            return off;

        }

        private static void startType(this ptr<exportWriter> _addr_w, itag k)
        {
            ref exportWriter w = ref _addr_w.val;

            w.data.uint64(uint64(k));
        }

        private static void doTyp(this ptr<exportWriter> _addr_w, types.Type t, ptr<types.Package> _addr_pkg) => func((_, panic, __) =>
        {
            ref exportWriter w = ref _addr_w.val;
            ref types.Package pkg = ref _addr_pkg.val;

            switch (t.type())
            {
                case ptr<types.Named> t:
                    w.startType(definedType);
                    w.qualifiedIdent(t.Obj());
                    break;
                case ptr<types.Pointer> t:
                    w.startType(pointerType);
                    w.typ(t.Elem(), pkg);
                    break;
                case ptr<types.Slice> t:
                    w.startType(sliceType);
                    w.typ(t.Elem(), pkg);
                    break;
                case ptr<types.Array> t:
                    w.startType(arrayType);
                    w.uint64(uint64(t.Len()));
                    w.typ(t.Elem(), pkg);
                    break;
                case ptr<types.Chan> t:
                    w.startType(chanType); 
                    // 1 RecvOnly; 2 SendOnly; 3 SendRecv
                    ulong dir = default;

                    if (t.Dir() == types.RecvOnly) 
                        dir = 1L;
                    else if (t.Dir() == types.SendOnly) 
                        dir = 2L;
                    else if (t.Dir() == types.SendRecv) 
                        dir = 3L;
                                        w.uint64(dir);
                    w.typ(t.Elem(), pkg);
                    break;
                case ptr<types.Map> t:
                    w.startType(mapType);
                    w.typ(t.Key(), pkg);
                    w.typ(t.Elem(), pkg);
                    break;
                case ptr<types.Signature> t:
                    w.startType(signatureType);
                    w.setPkg(pkg, true);
                    w.signature(t);
                    break;
                case ptr<types.Struct> t:
                    w.startType(structType);
                    w.setPkg(pkg, true);

                    var n = t.NumFields();
                    w.uint64(uint64(n));
                    {
                        long i__prev1 = i;

                        for (long i = 0L; i < n; i++)
                        {
                            var f = t.Field(i);
                            w.pos(f.Pos());
                            w.@string(f.Name());
                            w.typ(f.Type(), pkg);
                            w.@bool(f.Anonymous());
                            w.@string(t.Tag(i)); // note (or tag)
                        }


                        i = i__prev1;
                    }
                    break;
                case ptr<types.Interface> t:
                    w.startType(interfaceType);
                    w.setPkg(pkg, true);

                    n = t.NumEmbeddeds();
                    w.uint64(uint64(n));
                    {
                        long i__prev1 = i;

                        for (i = 0L; i < n; i++)
                        {
                            f = t.Embedded(i);
                            w.pos(f.Obj().Pos());
                            w.typ(f.Obj().Type(), f.Obj().Pkg());
                        }


                        i = i__prev1;
                    }

                    n = t.NumExplicitMethods();
                    w.uint64(uint64(n));
                    {
                        long i__prev1 = i;

                        for (i = 0L; i < n; i++)
                        {
                            var m = t.ExplicitMethod(i);
                            w.pos(m.Pos());
                            w.@string(m.Name());
                            ptr<types.Signature> (sig, _) = m.Type()._<ptr<types.Signature>>();
                            w.signature(sig);
                        }


                        i = i__prev1;
                    }
                    break;
                default:
                {
                    var t = t.type();
                    panic(internalErrorf("unexpected type: %v, %v", t, reflect.TypeOf(t)));
                    break;
                }
            }

        });

        private static void setPkg(this ptr<exportWriter> _addr_w, ptr<types.Package> _addr_pkg, bool write)
        {
            ref exportWriter w = ref _addr_w.val;
            ref types.Package pkg = ref _addr_pkg.val;

            if (write)
            {
                w.pkg(pkg);
            }

            w.currPkg = pkg;

        }

        private static void signature(this ptr<exportWriter> _addr_w, ptr<types.Signature> _addr_sig)
        {
            ref exportWriter w = ref _addr_w.val;
            ref types.Signature sig = ref _addr_sig.val;

            w.paramList(sig.Params());
            w.paramList(sig.Results());
            if (sig.Params().Len() > 0L)
            {
                w.@bool(sig.Variadic());
            }

        }

        private static void paramList(this ptr<exportWriter> _addr_w, ptr<types.Tuple> _addr_tup)
        {
            ref exportWriter w = ref _addr_w.val;
            ref types.Tuple tup = ref _addr_tup.val;

            var n = tup.Len();
            w.uint64(uint64(n));
            for (long i = 0L; i < n; i++)
            {
                w.param(tup.At(i));
            }


        }

        private static void param(this ptr<exportWriter> _addr_w, types.Object obj)
        {
            ref exportWriter w = ref _addr_w.val;

            w.pos(obj.Pos());
            w.localIdent(obj);
            w.typ(obj.Type(), obj.Pkg());
        }

        private static void value(this ptr<exportWriter> _addr_w, types.Type typ, constant.Value v) => func((_, panic, __) =>
        {
            ref exportWriter w = ref _addr_w.val;

            w.typ(typ, null);


            if (v.Kind() == constant.Bool) 
                w.@bool(constant.BoolVal(v));
            else if (v.Kind() == constant.Int) 
                ref big.Int i = ref heap(out ptr<big.Int> _addr_i);
                {
                    var (i64, exact) = constant.Int64Val(v);

                    if (exact)
                    {
                        i.SetInt64(i64);
                    }                    {
                        var (ui64, exact) = constant.Uint64Val(v);


                        else if (exact)
                        {
                            i.SetUint64(ui64);
                        }
                        else
                        {
                            i.SetString(v.ExactString(), 10L);
                        }

                    }


                }

                w.mpint(_addr_i, typ);
            else if (v.Kind() == constant.Float) 
                var f = constantToFloat(v);
                w.mpfloat(f, typ);
            else if (v.Kind() == constant.Complex) 
                w.mpfloat(constantToFloat(constant.Real(v)), typ);
                w.mpfloat(constantToFloat(constant.Imag(v)), typ);
            else if (v.Kind() == constant.String) 
                w.@string(constant.StringVal(v));
            else if (v.Kind() == constant.Unknown)             else 
                panic(internalErrorf("unexpected value %v (%T)", v, v));
            
        });

        // constantToFloat converts a constant.Value with kind constant.Float to a
        // big.Float.
        private static ptr<big.Float> constantToFloat(constant.Value x)
        {
            assert(x.Kind() == constant.Float); 
            // Use the same floating-point precision (512) as cmd/compile
            // (see Mpprec in cmd/compile/internal/gc/mpfloat.go).
            const long mpprec = (long)512L;

            ref big.Float f = ref heap(out ptr<big.Float> _addr_f);
            f.SetPrec(mpprec);
            {
                var (v, exact) = constant.Float64Val(x);

                if (exact)
                { 
                    // float64
                    f.SetFloat64(v);

                }                {
                    var num = constant.Num(x);
                    var denom = constant.Denom(x);


                    else if (num.Kind() == constant.Int)
                    { 
                        // TODO(gri): add big.Rat accessor to constant.Value.
                        var n = valueToRat(num);
                        var d = valueToRat(denom);
                        f.SetRat(n.Quo(n, d));

                    }
                    else
                    { 
                        // Value too large to represent as a fraction => inaccessible.
                        // TODO(gri): add big.Float accessor to constant.Value.
                        var (_, ok) = f.SetString(x.ExactString());
                        assert(ok);

                    }

                }


            }

            return _addr__addr_f!;

        }

        // mpint exports a multi-precision integer.
        //
        // For unsigned types, small values are written out as a single
        // byte. Larger values are written out as a length-prefixed big-endian
        // byte string, where the length prefix is encoded as its complement.
        // For example, bytes 0, 1, and 2 directly represent the integer
        // values 0, 1, and 2; while bytes 255, 254, and 253 indicate a 1-,
        // 2-, and 3-byte big-endian string follow.
        //
        // Encoding for signed types use the same general approach as for
        // unsigned types, except small values use zig-zag encoding and the
        // bottom bit of length prefix byte for large values is reserved as a
        // sign bit.
        //
        // The exact boundary between small and large encodings varies
        // according to the maximum number of bytes needed to encode a value
        // of type typ. As a special case, 8-bit types are always encoded as a
        // single byte.
        //
        // TODO(mdempsky): Is this level of complexity really worthwhile?
        private static void mpint(this ptr<exportWriter> _addr_w, ptr<big.Int> _addr_x, types.Type typ) => func((_, panic, __) =>
        {
            ref exportWriter w = ref _addr_w.val;
            ref big.Int x = ref _addr_x.val;

            ptr<types.Basic> (basic, ok) = typ.Underlying()._<ptr<types.Basic>>();
            if (!ok)
            {
                panic(internalErrorf("unexpected type %v (%T)", typ.Underlying(), typ.Underlying()));
            }

            var (signed, maxBytes) = intSize(basic);

            var negative = x.Sign() < 0L;
            if (!signed && negative)
            {
                panic(internalErrorf("negative unsigned integer; type %v, value %v", typ, x));
            }

            var b = x.Bytes();
            if (len(b) > 0L && b[0L] == 0L)
            {
                panic(internalErrorf("leading zeros"));
            }

            if (uint(len(b)) > maxBytes)
            {
                panic(internalErrorf("bad mpint length: %d > %d (type %v, value %v)", len(b), maxBytes, typ, x));
            }

            long maxSmall = 256L - maxBytes;
            if (signed)
            {
                maxSmall = 256L - 2L * maxBytes;
            }

            if (maxBytes == 1L)
            {
                maxSmall = 256L;
            } 

            // Check if x can use small value encoding.
            if (len(b) <= 1L)
            {
                ulong ux = default;
                if (len(b) == 1L)
                {
                    ux = uint(b[0L]);
                }

                if (signed)
                {
                    ux <<= 1L;
                    if (negative)
                    {
                        ux--;
                    }

                }

                if (ux < maxSmall)
                {
                    w.data.WriteByte(byte(ux));
                    return ;
                }

            }

            long n = 256L - uint(len(b));
            if (signed)
            {
                n = 256L - 2L * uint(len(b));
                if (negative)
                {
                    n |= 1L;
                }

            }

            if (n < maxSmall || n >= 256L)
            {
                panic(internalErrorf("encoding mistake: %d, %v, %v => %d", len(b), signed, negative, n));
            }

            w.data.WriteByte(byte(n));
            w.data.Write(b);

        });

        // mpfloat exports a multi-precision floating point number.
        //
        // The number's value is decomposed into mantissa × 2**exponent, where
        // mantissa is an integer. The value is written out as mantissa (as a
        // multi-precision integer) and then the exponent, except exponent is
        // omitted if mantissa is zero.
        private static void mpfloat(this ptr<exportWriter> _addr_w, ptr<big.Float> _addr_f, types.Type typ) => func((_, panic, __) =>
        {
            ref exportWriter w = ref _addr_w.val;
            ref big.Float f = ref _addr_f.val;

            if (f.IsInf())
            {
                panic("infinite constant");
            } 

            // Break into f = mant × 2**exp, with 0.5 <= mant < 1.
            ref big.Float mant = ref heap(out ptr<big.Float> _addr_mant);
            var exp = int64(f.MantExp(_addr_mant)); 

            // Scale so that mant is an integer.
            var prec = mant.MinPrec();
            mant.SetMantExp(_addr_mant, int(prec));
            exp -= int64(prec);

            var (manti, acc) = mant.Int(null);
            if (acc != big.Exact)
            {
                panic(internalErrorf("mantissa scaling failed for %f (%s)", f, acc));
            }

            w.mpint(manti, typ);
            if (manti.Sign() != 0L)
            {
                w.int64(exp);
            }

        });

        private static bool @bool(this ptr<exportWriter> _addr_w, bool b)
        {
            ref exportWriter w = ref _addr_w.val;

            ulong x = default;
            if (b)
            {
                x = 1L;
            }

            w.uint64(x);
            return b;

        }

        private static void int64(this ptr<exportWriter> _addr_w, long x)
        {
            ref exportWriter w = ref _addr_w.val;

            w.data.int64(x);
        }
        private static void uint64(this ptr<exportWriter> _addr_w, ulong x)
        {
            ref exportWriter w = ref _addr_w.val;

            w.data.uint64(x);
        }
        private static void @string(this ptr<exportWriter> _addr_w, @string s)
        {
            ref exportWriter w = ref _addr_w.val;

            w.uint64(w.p.stringOff(s));
        }

        private static void localIdent(this ptr<exportWriter> _addr_w, types.Object obj)
        {
            ref exportWriter w = ref _addr_w.val;
 
            // Anonymous parameters.
            if (obj == null)
            {
                w.@string("");
                return ;
            }

            var name = obj.Name();
            if (name == "_")
            {
                w.@string("_");
                return ;
            }

            w.@string(name);

        }

        private partial struct intWriter
        {
            public ref bytes.Buffer Buffer => ref Buffer_val;
        }

        private static void int64(this ptr<intWriter> _addr_w, long x)
        {
            ref intWriter w = ref _addr_w.val;

            array<byte> buf = new array<byte>(binary.MaxVarintLen64);
            var n = binary.PutVarint(buf[..], x);
            w.Write(buf[..n]);
        }

        private static void uint64(this ptr<intWriter> _addr_w, ulong x)
        {
            ref intWriter w = ref _addr_w.val;

            array<byte> buf = new array<byte>(binary.MaxVarintLen64);
            var n = binary.PutUvarint(buf[..], x);
            w.Write(buf[..n]);
        }

        private static void assert(bool cond) => func((_, panic, __) =>
        {
            if (!cond)
            {
                panic("internal error: assertion failed");
            }

        });

        // The below is copied from go/src/cmd/compile/internal/gc/syntax.go.

        // objQueue is a FIFO queue of types.Object. The zero value of objQueue is
        // a ready-to-use empty queue.
        private partial struct objQueue
        {
            public slice<types.Object> ring;
            public long head;
            public long tail;
        }

        // empty returns true if q contains no Nodes.
        private static bool empty(this ptr<objQueue> _addr_q)
        {
            ref objQueue q = ref _addr_q.val;

            return q.head == q.tail;
        }

        // pushTail appends n to the tail of the queue.
        private static void pushTail(this ptr<objQueue> _addr_q, types.Object obj)
        {
            ref objQueue q = ref _addr_q.val;

            if (len(q.ring) == 0L)
            {
                q.ring = make_slice<types.Object>(16L);
            }
            else if (q.head + len(q.ring) == q.tail)
            { 
                // Grow the ring.
                var nring = make_slice<types.Object>(len(q.ring) * 2L); 
                // Copy the old elements.
                var part = q.ring[q.head % len(q.ring)..];
                if (q.tail - q.head <= len(part))
                {
                    part = part[..q.tail - q.head];
                    copy(nring, part);
                }
                else
                {
                    var pos = copy(nring, part);
                    copy(nring[pos..], q.ring[..q.tail % len(q.ring)]);
                }

                q.ring = nring;
                q.head = 0L;
                q.tail = q.tail - q.head;

            }

            q.ring[q.tail % len(q.ring)] = obj;
            q.tail++;

        }

        // popHead pops a node from the head of the queue. It panics if q is empty.
        private static types.Object popHead(this ptr<objQueue> _addr_q) => func((_, panic, __) =>
        {
            ref objQueue q = ref _addr_q.val;

            if (q.empty())
            {
                panic("dequeue empty");
            }

            var obj = q.ring[q.head % len(q.ring)];
            q.head++;
            return obj;

        });
    }
}}}}}}
