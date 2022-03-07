// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2022 March 06 22:47:50 UTC
// import "cmd/compile/internal/types" ==> using types = go.cmd.compile.@internal.types_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\types\fmt.go
using bytes = go.bytes_package;
using md5 = go.crypto.md5_package;
using binary = go.encoding.binary_package;
using fmt = go.fmt_package;
using constant = go.go.constant_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using sync = go.sync_package;

using @base = go.cmd.compile.@internal.@base_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class types_package {

    // BuiltinPkg is a fake package that declares the universe block.
public static ptr<Pkg> BuiltinPkg;

// LocalPkg is the package being compiled.
public static ptr<Pkg> LocalPkg;

// BlankSym is the blank (_) symbol.
public static ptr<Sym> BlankSym;

// OrigSym returns the original symbol written by the user.
public static ptr<Sym> OrigSym(ptr<Sym> _addr_s) {
    ref Sym s = ref _addr_s.val;

    if (s == null) {
        return _addr_null!;
    }
    if (len(s.Name) > 1 && s.Name[0] == '~') {
        switch (s.Name[1]) {
            case 'r': // originally an unnamed result
                return _addr_null!;
                break;
            case 'b': // originally the blank identifier _
                // TODO(mdempsky): Does s.Pkg matter here?
                return _addr_BlankSym!;
                break;
        }
        return _addr_s!;

    }
    if (strings.HasPrefix(s.Name, ".anon")) { 
        // originally an unnamed or _ name (see subr.go: NewFuncParams)
        return _addr_null!;

    }
    return _addr_s!;

}

// numImport tracks how often a package with a given name is imported.
// It is used to provide a better error message (by using the package
// path to disambiguate) if a package that appears multiple times with
// the same name appears in an error message.
public static var NumImport = make_map<@string, nint>();

// fmtMode represents the kind of printing being done.
// The default is regular Go syntax (fmtGo).
// fmtDebug is like fmtGo but for debugging dumps and prints the type kind too.
// fmtTypeID and fmtTypeIDName are for generating various unique representations
// of types used in hashes and the linker.
private partial struct fmtMode { // : nint
}

private static readonly fmtMode fmtGo = iota;
private static readonly var fmtDebug = 0;
private static readonly var fmtTypeID = 1;
private static readonly var fmtTypeIDName = 2;


// Sym

// Format implements formatting for a Sym.
// The valid formats are:
//
//    %v    Go syntax: Name for symbols in the local package, PkgName.Name for imported symbols.
//    %+v    Debug syntax: always include PkgName. prefix even for local names.
//    %S    Short syntax: Name only, no matter what.
//
private static void Format(this ptr<Sym> _addr_s, fmt.State f, int verb) {
    ref Sym s = ref _addr_s.val;

    var mode = fmtGo;
    switch (verb) {
        case 'v': 

        case 'S': 
            if (verb == 'v' && f.Flag('+')) {
                mode = fmtDebug;
            }
            fmt.Fprint(f, sconv(_addr_s, verb, mode));

            break;
        default: 
            fmt.Fprintf(f, "%%!%c(*types.Sym=%p)", verb, s);
            break;
    }

}

private static @string String(this ptr<Sym> _addr_s) {
    ref Sym s = ref _addr_s.val;

    return sconv(_addr_s, 0, fmtGo);
}

// See #16897 for details about performance implications
// before changing the implementation of sconv.
private static @string sconv(ptr<Sym> _addr_s, int verb, fmtMode mode) => func((defer, panic, _) => {
    ref Sym s = ref _addr_s.val;

    if (verb == 'L') {
        panic("linksymfmt");
    }
    if (s == null) {
        return "<S>";
    }
    var q = pkgqual(_addr_s.Pkg, verb, mode);
    if (q == "") {
        return s.Name;
    }
    ptr<bytes.Buffer> buf = fmtBufferPool.Get()._<ptr<bytes.Buffer>>();
    buf.Reset();
    defer(fmtBufferPool.Put(buf));

    buf.WriteString(q);
    buf.WriteByte('.');
    buf.WriteString(s.Name);
    return InternString(buf.Bytes());

});

private static void sconv2(ptr<bytes.Buffer> _addr_b, ptr<Sym> _addr_s, int verb, fmtMode mode) => func((_, panic, _) => {
    ref bytes.Buffer b = ref _addr_b.val;
    ref Sym s = ref _addr_s.val;

    if (verb == 'L') {
        panic("linksymfmt");
    }
    if (s == null) {
        b.WriteString("<S>");
        return ;
    }
    symfmt(_addr_b, _addr_s, verb, mode);

});

private static void symfmt(ptr<bytes.Buffer> _addr_b, ptr<Sym> _addr_s, int verb, fmtMode mode) {
    ref bytes.Buffer b = ref _addr_b.val;
    ref Sym s = ref _addr_s.val;

    {
        var q = pkgqual(_addr_s.Pkg, verb, mode);

        if (q != "") {
            b.WriteString(q);
            b.WriteByte('.');
        }
    }

    b.WriteString(s.Name);

}

// pkgqual returns the qualifier that should be used for printing
// symbols from the given package in the given mode.
// If it returns the empty string, no qualification is needed.
private static @string pkgqual(ptr<Pkg> _addr_pkg, int verb, fmtMode mode) {
    ref Pkg pkg = ref _addr_pkg.val;

    if (verb != 'S') {

        if (mode == fmtGo) // This is for the user
            if (pkg == BuiltinPkg || pkg == LocalPkg) {
                return "";
            } 

            // If the name was used by multiple packages, display the full path,
            if (pkg.Name != "" && NumImport[pkg.Name] > 1) {
                return strconv.Quote(pkg.Path);
            }

            return pkg.Name;
        else if (mode == fmtDebug) 
            return pkg.Name;
        else if (mode == fmtTypeIDName) 
            // dcommontype, typehash
            return pkg.Name;
        else if (mode == fmtTypeID) 
            // (methodsym), typesym, weaksym
            return pkg.Prefix;
        
    }
    return "";

}

// Type

public static @string BasicTypeNames = new slice<@string>(InitKeyedValues<@string>((TINT, "int"), (TUINT, "uint"), (TINT8, "int8"), (TUINT8, "uint8"), (TINT16, "int16"), (TUINT16, "uint16"), (TINT32, "int32"), (TUINT32, "uint32"), (TINT64, "int64"), (TUINT64, "uint64"), (TUINTPTR, "uintptr"), (TFLOAT32, "float32"), (TFLOAT64, "float64"), (TCOMPLEX64, "complex64"), (TCOMPLEX128, "complex128"), (TBOOL, "bool"), (TANY, "any"), (TSTRING, "string"), (TNIL, "nil"), (TIDEAL, "untyped number"), (TBLANK, "blank")));

private static sync.Pool fmtBufferPool = new sync.Pool(New:func()interface{}{returnnew(bytes.Buffer)},);

// Format implements formatting for a Type.
// The valid formats are:
//
//    %v    Go syntax
//    %+v    Debug syntax: Go syntax with a KIND- prefix for all but builtins.
//    %L    Go syntax for underlying type if t is named
//    %S    short Go syntax: drop leading "func" in function type
//    %-S    special case for method receiver symbol
//
private static void Format(this ptr<Type> _addr_t, fmt.State s, int verb) {
    ref Type t = ref _addr_t.val;

    var mode = fmtGo;
    switch (verb) {
        case 'v': 

        case 'S': 

        case 'L': 
            if (verb == 'v' && s.Flag('+')) { // %+v is debug format
                mode = fmtDebug;

            }
            if (verb == 'S' && s.Flag('-')) { // %-S is special case for receiver - short typeid format
                mode = fmtTypeID;

            }
            fmt.Fprint(s, tconv(_addr_t, verb, mode));

            break;
        default: 
            fmt.Fprintf(s, "%%!%c(*Type=%p)", verb, t);
            break;
    }

}

// String returns the Go syntax for the type t.
private static @string String(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return tconv(_addr_t, 0, fmtGo);
}

// ShortString generates a short description of t.
// It is used in autogenerated method names, reflection,
// and itab names.
private static @string ShortString(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return tconv(_addr_t, 0, fmtTypeID);
}

// LongString generates a complete description of t.
// It is useful for reflection,
// or when a unique fingerprint or hash of a type is required.
private static @string LongString(this ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    return tconv(_addr_t, 0, fmtTypeIDName);
}

private static @string tconv(ptr<Type> _addr_t, int verb, fmtMode mode) => func((defer, _, _) => {
    ref Type t = ref _addr_t.val;

    ptr<bytes.Buffer> buf = fmtBufferPool.Get()._<ptr<bytes.Buffer>>();
    buf.Reset();
    defer(fmtBufferPool.Put(buf));

    tconv2(buf, _addr_t, verb, mode, null);
    return InternString(buf.Bytes());
});

// tconv2 writes a string representation of t to b.
// flag and mode control exactly what is printed.
// Any types x that are already in the visited map get printed as @%d where %d=visited[x].
// See #16897 before changing the implementation of tconv.
private static void tconv2(ptr<bytes.Buffer> _addr_b, ptr<Type> _addr_t, int verb, fmtMode mode, map<ptr<Type>, nint> visited) => func((defer, _, _) => {
    ref bytes.Buffer b = ref _addr_b.val;
    ref Type t = ref _addr_t.val;

    {
        var (off, ok) = visited[t];

        if (ok) { 
            // We've seen this type before, so we're trying to print it recursively.
            // Print a reference to it instead.
            fmt.Fprintf(b, "@%d", off);
            return ;

        }
    }

    if (t == null) {
        b.WriteString("<T>");
        return ;
    }
    if (t.Kind() == TSSA) {
        b.WriteString(t.Extra._<@string>());
        return ;
    }
    if (t.Kind() == TTUPLE) {
        b.WriteString(t.FieldType(0).String());
        b.WriteByte(',');
        b.WriteString(t.FieldType(1).String());
        return ;
    }
    if (t.Kind() == TRESULTS) {
        ptr<Results> tys = t.Extra._<ptr<Results>>().Types;
        {
            var i__prev1 = i;

            foreach (var (__i, __et) in tys) {
                i = __i;
                et = __et;
                if (i > 0) {
                    b.WriteByte(',');
                }
                b.WriteString(et.String());
            }

            i = i__prev1;
        }

        return ;

    }
    if (t == ByteType || t == RuneType) { 
        // in %-T mode collapse rune and byte with their originals.

        if (mode == fmtTypeIDName || mode == fmtTypeID) 
            t = Types[t.Kind()];
        else 
            sconv2(_addr_b, _addr_t.Sym(), 'S', mode);
            return ;
        
    }
    if (t == ErrorType) {
        b.WriteString("error");
        return ;
    }
    if (verb != 'L' && t.Sym() != null && t != Types[t.Kind()]) {

        if (mode == fmtTypeID || mode == fmtTypeIDName) 
            if (verb == 'S') {
                if (t.Vargen != 0) {
                    sconv2(_addr_b, _addr_t.Sym(), 'S', mode);
                    fmt.Fprintf(b, "·%d", t.Vargen);
                    return ;
                }
                sconv2(_addr_b, _addr_t.Sym(), 'S', mode);
                return ;
            }
            if (mode == fmtTypeIDName) {
                sconv2(_addr_b, _addr_t.Sym(), 'v', fmtTypeIDName);
                return ;
            }
            if (t.Sym().Pkg == LocalPkg && t.Vargen != 0) {
                sconv2(_addr_b, _addr_t.Sym(), 'v', mode);
                fmt.Fprintf(b, "·%d", t.Vargen);
                return ;
            }
                sconv2(_addr_b, _addr_t.Sym(), 'v', mode);
        return ;

    }
    if (int(t.Kind()) < len(BasicTypeNames) && BasicTypeNames[t.Kind()] != "") {
        @string name = default;

        if (t == UntypedBool) 
            name = "untyped bool";
        else if (t == UntypedString) 
            name = "untyped string";
        else if (t == UntypedInt) 
            name = "untyped int";
        else if (t == UntypedRune) 
            name = "untyped rune";
        else if (t == UntypedFloat) 
            name = "untyped float";
        else if (t == UntypedComplex) 
            name = "untyped complex";
        else 
            name = BasicTypeNames[t.Kind()];
                b.WriteString(name);
        return ;

    }
    if (mode == fmtDebug) {
        b.WriteString(t.Kind().String());
        b.WriteByte('-');
        tconv2(_addr_b, _addr_t, 'v', fmtGo, visited);
        return ;
    }
    if (visited == null) {
        visited = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<ptr<Type>, nint>{};
    }
    visited[t] = b.Len();
    defer(delete(visited, t));


    if (t.Kind() == TPTR) 
        b.WriteByte('*');

        if (mode == fmtTypeID || mode == fmtTypeIDName) 
            if (verb == 'S') {
                tconv2(_addr_b, _addr_t.Elem(), 'S', mode, visited);
                return ;
            }
                tconv2(_addr_b, _addr_t.Elem(), 'v', mode, visited);
    else if (t.Kind() == TARRAY) 
        b.WriteByte('[');
        b.WriteString(strconv.FormatInt(t.NumElem(), 10));
        b.WriteByte(']');
        tconv2(_addr_b, _addr_t.Elem(), 0, mode, visited);
    else if (t.Kind() == TSLICE) 
        b.WriteString("[]");
        tconv2(_addr_b, _addr_t.Elem(), 0, mode, visited);
    else if (t.Kind() == TCHAN) 

        if (t.ChanDir() == Crecv) 
            b.WriteString("<-chan ");
            tconv2(_addr_b, _addr_t.Elem(), 0, mode, visited);
        else if (t.ChanDir() == Csend) 
            b.WriteString("chan<- ");
            tconv2(_addr_b, _addr_t.Elem(), 0, mode, visited);
        else 
            b.WriteString("chan ");
            if (t.Elem() != null && t.Elem().IsChan() && t.Elem().Sym() == null && t.Elem().ChanDir() == Crecv) {
                b.WriteByte('(');
                tconv2(_addr_b, _addr_t.Elem(), 0, mode, visited);
                b.WriteByte(')');
            }
            else
 {
                tconv2(_addr_b, _addr_t.Elem(), 0, mode, visited);
            }

            else if (t.Kind() == TMAP) 
        b.WriteString("map[");
        tconv2(_addr_b, _addr_t.Key(), 0, mode, visited);
        b.WriteByte(']');
        tconv2(_addr_b, _addr_t.Elem(), 0, mode, visited);
    else if (t.Kind() == TINTER) 
        if (t.IsEmptyInterface()) {
            b.WriteString("interface {}");
            break;
        }
        b.WriteString("interface {");
        {
            var i__prev1 = i;
            var f__prev1 = f;

            foreach (var (__i, __f) in t.AllMethods().Slice()) {
                i = __i;
                f = __f;
                if (i != 0) {
                    b.WriteByte(';');
                }
                b.WriteByte(' ');

                if (f.Sym == null) 
                    // Check first that a symbol is defined for this type.
                    // Wrong interface definitions may have types lacking a symbol.
                    break;
                else if (IsExported(f.Sym.Name)) 
                    sconv2(_addr_b, _addr_f.Sym, 'S', mode);
                else 
                    if (mode != fmtTypeIDName) {
                        mode = fmtTypeID;
                    }
                    sconv2(_addr_b, _addr_f.Sym, 'v', mode);
                                tconv2(_addr_b, _addr_f.Type, 'S', mode, visited);

            }

            i = i__prev1;
            f = f__prev1;
        }

        if (t.AllMethods().Len() != 0) {
            b.WriteByte(' ');
        }
        b.WriteByte('}');
    else if (t.Kind() == TFUNC) 
        if (verb == 'S') { 
            // no leading func
        }
        else
 {
            if (t.Recv() != null) {
                b.WriteString("method");
                tconv2(_addr_b, _addr_t.Recvs(), 0, mode, visited);
                b.WriteByte(' ');
            }
            b.WriteString("func");
        }
        if (t.NumTParams() > 0) {
            tconv2(_addr_b, _addr_t.TParams(), 0, mode, visited);
        }
        tconv2(_addr_b, _addr_t.Params(), 0, mode, visited);

        switch (t.NumResults()) {
            case 0: 

                break;
            case 1: 
                b.WriteByte(' ');
                tconv2(_addr_b, _addr_t.Results().Field(0).Type, 0, mode, visited); // struct->field->field's type
                break;
            default: 
                b.WriteByte(' ');
                tconv2(_addr_b, _addr_t.Results(), 0, mode, visited);
                break;
        }
    else if (t.Kind() == TSTRUCT) 
        {
            var m = t.StructType().Map;

            if (m != null) {
                var mt = m.MapType(); 
                // Format the bucket struct for map[x]y as map.bucket[x]y.
                // This avoids a recursive print that generates very long names.

                if (t == mt.Bucket) 
                    b.WriteString("map.bucket[");
                else if (t == mt.Hmap) 
                    b.WriteString("map.hdr[");
                else if (t == mt.Hiter) 
                    b.WriteString("map.iter[");
                else 
                    @base.Fatalf("unknown internal map type");
                                tconv2(_addr_b, _addr_m.Key(), 0, mode, visited);
                b.WriteByte(']');
                tconv2(_addr_b, _addr_m.Elem(), 0, mode, visited);
                break;

            }

        }


        {
            var funarg = t.StructType().Funarg;

            if (funarg != FunargNone) {
                char open = '(';
                char close = ')';
                if (funarg == FunargTparams) {
                    (open, close) = ('[', ']');
                }

                b.WriteByte(byte(open));
                char fieldVerb = 'v';

                if (mode == fmtTypeID || mode == fmtTypeIDName || mode == fmtGo) 
                    // no argument names on function signature, and no "noescape"/"nosplit" tags
                    fieldVerb = 'S';
                                {
                    var i__prev1 = i;
                    var f__prev1 = f;

                    foreach (var (__i, __f) in t.Fields().Slice()) {
                        i = __i;
                        f = __f;
                        if (i != 0) {
                            b.WriteString(", ");
                        }
                        fldconv(_addr_b, _addr_f, fieldVerb, mode, visited, funarg);
                    }
            else

                    i = i__prev1;
                    f = f__prev1;
                }

                b.WriteByte(byte(close));

            } {
                b.WriteString("struct {");
                {
                    var i__prev1 = i;
                    var f__prev1 = f;

                    foreach (var (__i, __f) in t.Fields().Slice()) {
                        i = __i;
                        f = __f;
                        if (i != 0) {
                            b.WriteByte(';');
                        }
                        b.WriteByte(' ');
                        fldconv(_addr_b, _addr_f, 'L', mode, visited, funarg);
                    }

                    i = i__prev1;
                    f = f__prev1;
                }

                if (t.NumFields() != 0) {
                    b.WriteByte(' ');
                }

                b.WriteByte('}');

            }

        }


    else if (t.Kind() == TFORW) 
        b.WriteString("undefined");
        if (t.Sym() != null) {
            b.WriteByte(' ');
            sconv2(_addr_b, _addr_t.Sym(), 'v', mode);
        }
    else if (t.Kind() == TUNSAFEPTR) 
        b.WriteString("unsafe.Pointer");
    else if (t.Kind() == TTYPEPARAM) 
        if (t.Sym() != null) {
            sconv2(_addr_b, _addr_t.Sym(), 'v', mode);
        }
        else
 {
            b.WriteString("tp"); 
            // Print out the pointer value for now to disambiguate type params
            b.WriteString(fmt.Sprintf("%p", t));

        }
    else if (t.Kind() == Txxx) 
        b.WriteString("Txxx");
    else 
        // Don't know how to handle - fall back to detailed prints
        b.WriteString(t.Kind().String());
        b.WriteString(" <");
        sconv2(_addr_b, _addr_t.Sym(), 'v', mode);
        b.WriteString(">");
    
});

private static void fldconv(ptr<bytes.Buffer> _addr_b, ptr<Field> _addr_f, int verb, fmtMode mode, map<ptr<Type>, nint> visited, Funarg funarg) {
    ref bytes.Buffer b = ref _addr_b.val;
    ref Field f = ref _addr_f.val;

    if (f == null) {
        b.WriteString("<T>");
        return ;
    }
    @string name = default;
    if (verb != 'S') {
        var s = f.Sym; 

        // Take the name from the original.
        if (mode == fmtGo) {
            s = OrigSym(_addr_s);
        }
        if (s != null && f.Embedded == 0) {
            if (funarg != FunargNone) {
                name = fmt.Sprint(f.Nname);
            }
            else if (verb == 'L') {
                name = s.Name;
                if (name == ".F") {
                    name = "F"; // Hack for toolstash -cmp.
                }

                if (!IsExported(name) && mode != fmtTypeIDName) {
                    name = sconv(_addr_s, 0, mode); // qualify non-exported names (used on structs, not on funarg)
                }

            }
            else
 {
                name = sconv(_addr_s, 0, mode);
            }

        }
    }
    if (name != "") {
        b.WriteString(name);
        b.WriteString(" ");
    }
    if (f.IsDDD()) {
        ptr<Type> et;
        if (f.Type != null) {
            et = f.Type.Elem();
        }
        b.WriteString("...");
        tconv2(_addr_b, et, 0, mode, visited);

    }
    else
 {
        tconv2(_addr_b, _addr_f.Type, 0, mode, visited);
    }
    if (verb != 'S' && funarg == FunargNone && f.Note != "") {
        b.WriteString(" ");
        b.WriteString(strconv.Quote(f.Note));
    }
}

// Val

public static @string FmtConst(constant.Value v, bool sharp) {
    if (!sharp && v.Kind() == constant.Complex) {
        var real = constant.Real(v);
        var imag = constant.Imag(v);

        @string re = default;
        var sre = constant.Sign(real);
        if (sre != 0) {
            re = real.String();
        }
        @string im = default;
        var sim = constant.Sign(imag);
        if (sim != 0) {
            im = imag.String();
        }

        if (sre == 0 && sim == 0) 
            return "0";
        else if (sre == 0) 
            return im + "i";
        else if (sim == 0) 
            return re;
        else if (sim < 0) 
            return fmt.Sprintf("(%s%si)", re, im);
        else 
            return fmt.Sprintf("(%s+%si)", re, im);
        
    }
    return v.String();

}

// TypeHash computes a hash value for type t to use in type switch statements.
public static uint TypeHash(ptr<Type> _addr_t) {
    ref Type t = ref _addr_t.val;

    var p = t.LongString(); 

    // Using MD5 is overkill, but reduces accidental collisions.
    var h = md5.Sum((slice<byte>)p);
    return binary.LittleEndian.Uint32(h[..(int)4]);

}

} // end types_package
