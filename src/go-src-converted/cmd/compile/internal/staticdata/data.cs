// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package staticdata -- go2cs converted at 2022 March 13 05:58:59 UTC
// import "cmd/compile/internal/staticdata" ==> using staticdata = go.cmd.compile.@internal.staticdata_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\staticdata\data.go
namespace go.cmd.compile.@internal;

using sha256 = crypto.sha256_package;
using fmt = fmt_package;
using constant = go.constant_package;
using buildcfg = @internal.buildcfg_package;
using io = io_package;
using ioutil = io.ioutil_package;
using os = os_package;
using sort = sort_package;
using strconv = strconv_package;
using sync = sync_package;

using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using objw = cmd.compile.@internal.objw_package;
using typecheck = cmd.compile.@internal.typecheck_package;
using types = cmd.compile.@internal.types_package;
using obj = cmd.@internal.obj_package;
using objabi = cmd.@internal.objabi_package;
using src = cmd.@internal.src_package;


// InitAddrOffset writes the static name symbol lsym to n, it does not modify n.
// It's the caller responsibility to make sure lsym is from ONAME/PEXTERN node.

using System;
public static partial class staticdata_package {

public static void InitAddrOffset(ptr<ir.Name> _addr_n, long noff, ptr<obj.LSym> _addr_lsym, long off) {
    ref ir.Name n = ref _addr_n.val;
    ref obj.LSym lsym = ref _addr_lsym.val;

    if (n.Op() != ir.ONAME) {
        @base.Fatalf("InitAddr n op %v", n.Op());
    }
    if (n.Sym() == null) {
        @base.Fatalf("InitAddr nil n sym");
    }
    var s = n.Linksym();
    s.WriteAddr(@base.Ctxt, noff, types.PtrSize, lsym, off);
}

// InitAddr is InitAddrOffset, with offset fixed to 0.
public static void InitAddr(ptr<ir.Name> _addr_n, long noff, ptr<obj.LSym> _addr_lsym) {
    ref ir.Name n = ref _addr_n.val;
    ref obj.LSym lsym = ref _addr_lsym.val;

    InitAddrOffset(_addr_n, noff, _addr_lsym, 0);
}

// InitSlice writes a static slice symbol {lsym, lencap, lencap} to n+noff, it does not modify n.
// It's the caller responsibility to make sure lsym is from ONAME node.
public static void InitSlice(ptr<ir.Name> _addr_n, long noff, ptr<obj.LSym> _addr_lsym, long lencap) {
    ref ir.Name n = ref _addr_n.val;
    ref obj.LSym lsym = ref _addr_lsym.val;

    var s = n.Linksym();
    s.WriteAddr(@base.Ctxt, noff, types.PtrSize, lsym, 0);
    s.WriteInt(@base.Ctxt, noff + types.SliceLenOffset, types.PtrSize, lencap);
    s.WriteInt(@base.Ctxt, noff + types.SliceCapOffset, types.PtrSize, lencap);
}

public static void InitSliceBytes(ptr<ir.Name> _addr_nam, long off, @string s) {
    ref ir.Name nam = ref _addr_nam.val;

    if (nam.Op() != ir.ONAME) {
        @base.Fatalf("InitSliceBytes %v", nam);
    }
    InitSlice(_addr_nam, off, _addr_slicedata(nam.Pos(), s).Linksym(), int64(len(s)));
}

private static readonly @string stringSymPrefix = "go.string.";
private static readonly @string stringSymPattern = ".gostring.%d.%x";

// StringSym returns a symbol containing the string s.
// The symbol contains the string data, not a string header.
public static ptr<obj.LSym> StringSym(src.XPos pos, @string s) {
    ptr<obj.LSym> data = default!;

    @string symname = default;
    if (len(s) > 100) { 
        // Huge strings are hashed to avoid long names in object files.
        // Indulge in some paranoia by writing the length of s, too,
        // as protection against length extension attacks.
        // Same pattern is known to fileStringSym below.
        var h = sha256.New();
        io.WriteString(h, s);
        symname = fmt.Sprintf(stringSymPattern, len(s), h.Sum(null));
    }
    else
 { 
        // Small strings get named directly by their contents.
        symname = strconv.Quote(s);
    }
    var symdata = @base.Ctxt.Lookup(stringSymPrefix + symname);
    if (!symdata.OnList()) {
        var off = dstringdata(_addr_symdata, 0, s, pos, "string");
        objw.Global(symdata, int32(off), obj.DUPOK | obj.RODATA | obj.LOCAL);
        symdata.Set(obj.AttrContentAddressable, true);
    }
    return _addr_symdata!;
}

// fileStringSym returns a symbol for the contents and the size of file.
// If readonly is true, the symbol shares storage with any literal string
// or other file with the same content and is placed in a read-only section.
// If readonly is false, the symbol is a read-write copy separate from any other,
// for use as the backing store of a []byte.
// The content hash of file is copied into hash. (If hash is nil, nothing is copied.)
// The returned symbol contains the data itself, not a string header.
private static (ptr<obj.LSym>, long, error) fileStringSym(src.XPos pos, @string file, bool @readonly, slice<byte> hash) => func((defer, _, _) => {
    ptr<obj.LSym> _p0 = default!;
    long _p0 = default;
    error _p0 = default!;

    var (f, err) = os.Open(file);
    if (err != null) {
        return (_addr_null!, 0, error.As(err)!);
    }
    defer(f.Close());
    var (info, err) = f.Stat();
    if (err != null) {
        return (_addr_null!, 0, error.As(err)!);
    }
    if (!info.Mode().IsRegular()) {
        return (_addr_null!, 0, error.As(fmt.Errorf("not a regular file"))!);
    }
    var size = info.Size();
    if (size <= 1 * 1024) {
        var (data, err) = ioutil.ReadAll(f);
        if (err != null) {
            return (_addr_null!, 0, error.As(err)!);
        }
        if (int64(len(data)) != size) {
            return (_addr_null!, 0, error.As(fmt.Errorf("file changed between reads"))!);
        }
        ptr<obj.LSym> sym;
        if (readonly) {
            sym = StringSym(pos, string(data));
        }
        else
 {
            sym = slicedata(pos, string(data)).Linksym();
        }
        if (len(hash) > 0) {
            var sum = sha256.Sum256(data);
            copy(hash, sum[..]);
        }
        return (_addr_sym!, size, error.As(null!)!);
    }
    if (size > 2e9F) { 
        // ggloblsym takes an int32,
        // and probably the rest of the toolchain
        // can't handle such big symbols either.
        // See golang.org/issue/9862.
        return (_addr_null!, 0, error.As(fmt.Errorf("file too large"))!);
    }
    sum = default;
    if (readonly || len(hash) > 0) {
        var h = sha256.New();
        var (n, err) = io.Copy(h, f);
        if (err != null) {
            return (_addr_null!, 0, error.As(err)!);
        }
        if (n != size) {
            return (_addr_null!, 0, error.As(fmt.Errorf("file changed between reads"))!);
        }
        sum = h.Sum(null);
        copy(hash, sum);
    }
    ptr<obj.LSym> symdata;
    if (readonly) {
        var symname = fmt.Sprintf(stringSymPattern, size, sum);
        symdata = @base.Ctxt.Lookup(stringSymPrefix + symname);
        if (!symdata.OnList()) {
            var info = symdata.NewFileInfo();
            info.Name = file;
            info.Size = size;
            objw.Global(symdata, int32(size), obj.DUPOK | obj.RODATA | obj.LOCAL); 
            // Note: AttrContentAddressable cannot be set here,
            // because the content-addressable-handling code
            // does not know about file symbols.
        }
    }
    else
 { 
        // Emit a zero-length data symbol
        // and then fix up length and content to use file.
        symdata = slicedata(pos, "").Linksym();
        symdata.Size = size;
        symdata.Type = objabi.SNOPTRDATA;
        info = symdata.NewFileInfo();
        info.Name = file;
        info.Size = size;
    }
    return (_addr_symdata!, size, error.As(null!)!);
});

private static nint slicedataGen = default;

private static ptr<ir.Name> slicedata(src.XPos pos, @string s) {
    slicedataGen++;
    var symname = fmt.Sprintf(".gobytes.%d", slicedataGen);
    var sym = types.LocalPkg.Lookup(symname);
    var symnode = typecheck.NewName(sym);
    sym.Def = symnode;

    var lsym = symnode.Linksym();
    var off = dstringdata(_addr_lsym, 0, s, pos, "slice");
    objw.Global(lsym, int32(off), obj.NOPTR | obj.LOCAL);

    return _addr_symnode!;
}

private static nint dstringdata(ptr<obj.LSym> _addr_s, nint off, @string t, src.XPos pos, @string what) {
    ref obj.LSym s = ref _addr_s.val;
 
    // Objects that are too large will cause the data section to overflow right away,
    // causing a cryptic error message by the linker. Check for oversize objects here
    // and provide a useful error message instead.
    if (int64(len(t)) > 2e9F) {
        @base.ErrorfAt(pos, "%v with length %v is too big", what, len(t));
        return 0;
    }
    s.WriteString(@base.Ctxt, int64(off), len(t), t);
    return off + len(t);
}

private static sync.Mutex funcsymsmu = default;private static slice<ptr<ir.Name>> funcsyms = default;

// FuncLinksym returns n·f, the function value symbol for n.
public static ptr<obj.LSym> FuncLinksym(ptr<ir.Name> _addr_n) {
    ref ir.Name n = ref _addr_n.val;

    if (n.Op() != ir.ONAME || n.Class != ir.PFUNC) {
        @base.Fatalf("expected func name: %v", n);
    }
    var s = n.Sym(); 

    // funcsymsmu here serves to protect not just mutations of funcsyms (below),
    // but also the package lookup of the func sym name,
    // since this function gets called concurrently from the backend.
    // There are no other concurrent package lookups in the backend,
    // except for the types package, which is protected separately.
    // Reusing funcsymsmu to also cover this package lookup
    // avoids a general, broader, expensive package lookup mutex.
    // Note NeedFuncSym also does package look-up of func sym names,
    // but that it is only called serially, from the front end.
    funcsymsmu.Lock();
    var (sf, existed) = s.Pkg.LookupOK(ir.FuncSymName(s)); 
    // Don't export s·f when compiling for dynamic linking.
    // When dynamically linking, the necessary function
    // symbols will be created explicitly with NeedFuncSym.
    // See the NeedFuncSym comment for details.
    if (!@base.Ctxt.Flag_dynlink && !existed) {
        funcsyms = append(funcsyms, n);
    }
    funcsymsmu.Unlock();

    return _addr_sf.Linksym()!;
}

public static ptr<obj.LSym> GlobalLinksym(ptr<ir.Name> _addr_n) {
    ref ir.Name n = ref _addr_n.val;

    if (n.Op() != ir.ONAME || n.Class != ir.PEXTERN) {
        @base.Fatalf("expected global variable: %v", n);
    }
    return _addr_n.Linksym()!;
}

// NeedFuncSym ensures that fn·f is exported, if needed.
// It is only used with -dynlink.
// When not compiling for dynamic linking,
// the funcsyms are created as needed by
// the packages that use them.
// Normally we emit the fn·f stubs as DUPOK syms,
// but DUPOK doesn't work across shared library boundaries.
// So instead, when dynamic linking, we only create
// the fn·f stubs in fn's package.
public static void NeedFuncSym(ptr<ir.Func> _addr_fn) {
    ref ir.Func fn = ref _addr_fn.val;

    if (@base.Ctxt.InParallel) { 
        // The append below probably just needs to lock
        // funcsymsmu, like in FuncSym.
        @base.Fatalf("NeedFuncSym must be called in serial");
    }
    if (fn.ABI != obj.ABIInternal && buildcfg.Experiment.RegabiWrappers) { 
        // Function values must always reference ABIInternal
        // entry points, so it doesn't make sense to create a
        // funcsym for other ABIs.
        //
        // (If we're using ABI aliases, it doesn't matter.)
        @base.Fatalf("expected ABIInternal: %v has %v", fn.Nname, fn.ABI);
    }
    if (ir.IsBlank(fn.Nname)) { 
        // Blank functions aren't unique, so we can't make a
        // funcsym for them.
        @base.Fatalf("NeedFuncSym called for _");
    }
    if (!@base.Ctxt.Flag_dynlink) {
        return ;
    }
    var s = fn.Nname.Sym();
    if (@base.Flag.CompilingRuntime && (s.Name == "getg" || s.Name == "getclosureptr" || s.Name == "getcallerpc" || s.Name == "getcallersp") || (@base.Ctxt.Pkgpath == "internal/abi" && (s.Name == "FuncPCABI0" || s.Name == "FuncPCABIInternal"))) { 
        // runtime.getg(), getclosureptr(), getcallerpc(), getcallersp(),
        // and internal/abi.FuncPCABIxxx() are not real functions and so
        // do not get funcsyms.
        return ;
    }
    funcsyms = append(funcsyms, fn.Nname);
}

public static void WriteFuncSyms() {
    sort.Slice(funcsyms, (i, j) => funcsyms[i].Linksym().Name < funcsyms[j].Linksym().Name);
    foreach (var (_, nam) in funcsyms) {
        var s = nam.Sym();
        var sf = s.Pkg.Lookup(ir.FuncSymName(s)).Linksym(); 
        // Function values must always reference ABIInternal
        // entry points.
        var target = s.Linksym();
        if (target.ABI() != obj.ABIInternal) {
            @base.Fatalf("expected ABIInternal: %v has %v", target, target.ABI());
        }
        objw.SymPtr(sf, 0, target, 0);
        objw.Global(sf, int32(types.PtrSize), obj.DUPOK | obj.RODATA);
    }
}

// InitConst writes the static literal c to n.
// Neither n nor c is modified.
public static void InitConst(ptr<ir.Name> _addr_n, long noff, ir.Node c, nint wid) {
    ref ir.Name n = ref _addr_n.val;

    if (n.Op() != ir.ONAME) {
        @base.Fatalf("InitConst n op %v", n.Op());
    }
    if (n.Sym() == null) {
        @base.Fatalf("InitConst nil n sym");
    }
    if (c.Op() == ir.ONIL) {
        return ;
    }
    if (c.Op() != ir.OLITERAL) {
        @base.Fatalf("InitConst c op %v", c.Op());
    }
    var s = n.Linksym();
    {
        var u = c.Val();


        if (u.Kind() == constant.Bool) 
            var i = int64(obj.Bool2int(constant.BoolVal(u)));
            s.WriteInt(@base.Ctxt, noff, wid, i);
        else if (u.Kind() == constant.Int) 
            s.WriteInt(@base.Ctxt, noff, wid, ir.IntVal(c.Type(), u));
        else if (u.Kind() == constant.Float) 
            var (f, _) = constant.Float64Val(u);

            if (c.Type().Kind() == types.TFLOAT32) 
                s.WriteFloat32(@base.Ctxt, noff, float32(f));
            else if (c.Type().Kind() == types.TFLOAT64) 
                s.WriteFloat64(@base.Ctxt, noff, f);
                    else if (u.Kind() == constant.Complex) 
            var (re, _) = constant.Float64Val(constant.Real(u));
            var (im, _) = constant.Float64Val(constant.Imag(u));

            if (c.Type().Kind() == types.TCOMPLEX64) 
                s.WriteFloat32(@base.Ctxt, noff, float32(re));
                s.WriteFloat32(@base.Ctxt, noff + 4, float32(im));
            else if (c.Type().Kind() == types.TCOMPLEX128) 
                s.WriteFloat64(@base.Ctxt, noff, re);
                s.WriteFloat64(@base.Ctxt, noff + 8, im);
                    else if (u.Kind() == constant.String) 
            i = constant.StringVal(u);
            var symdata = StringSym(n.Pos(), i);
            s.WriteAddr(@base.Ctxt, noff, types.PtrSize, symdata, 0);
            s.WriteInt(@base.Ctxt, noff + int64(types.PtrSize), types.PtrSize, int64(len(i)));
        else 
            @base.Fatalf("InitConst unhandled OLITERAL %v", c);

    }
}

} // end staticdata_package
