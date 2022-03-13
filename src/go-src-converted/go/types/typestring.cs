// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements printing of types.

// package types -- go2cs converted at 2022 March 13 05:53:33 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Program Files\Go\src\go\types\typestring.go
namespace go.go;

using bytes = bytes_package;
using fmt = fmt_package;
using token = go.token_package;
using utf8 = unicode.utf8_package;


// A Qualifier controls how named package-level objects are printed in
// calls to TypeString, ObjectString, and SelectionString.
//
// These three formatting routines call the Qualifier for each
// package-level object O, and if the Qualifier returns a non-empty
// string p, the object is printed in the form p.O.
// If it returns an empty string, only the object name O is printed.
//
// Using a nil Qualifier is equivalent to using (*Package).Path: the
// object is qualified by the import path, e.g., "encoding/json.Marshal".
//

using System;
public static partial class types_package {

public delegate  @string Qualifier(ptr<Package>);

// RelativeTo returns a Qualifier that fully qualifies members of
// all packages other than pkg.
public static Qualifier RelativeTo(ptr<Package> _addr_pkg) {
    ref Package pkg = ref _addr_pkg.val;

    if (pkg == null) {
        return null;
    }
    return other => {
        if (pkg == other) {
            return ""; // same package; unqualified
        }
        return other.Path();
    };
}

// If gcCompatibilityMode is set, printing of types is modified
// to match the representation of some types in the gc compiler:
//
//    - byte and rune lose their alias name and simply stand for
//      uint8 and int32 respectively
//    - embedded interfaces get flattened (the embedding info is lost,
//      and certain recursive interface types cannot be printed anymore)
//
// This makes it easier to compare packages computed with the type-
// checker vs packages imported from gc export data.
//
// Caution: This flag affects all uses of WriteType, globally.
// It is only provided for testing in conjunction with
// gc-generated data.
//
// This flag is exported in the x/tools/go/types package. We don't
// need it at the moment in the std repo and so we don't export it
// anymore. We should eventually try to remove it altogether.
// TODO(gri) remove this
private static bool gcCompatibilityMode = default;

// TypeString returns the string representation of typ.
// The Qualifier controls the printing of
// package-level objects, and may be nil.
public static @string TypeString(Type typ, Qualifier qf) {
    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
    WriteType(_addr_buf, typ, qf);
    return buf.String();
}

// WriteType writes the string representation of typ to buf.
// The Qualifier controls the printing of
// package-level objects, and may be nil.
public static void WriteType(ptr<bytes.Buffer> _addr_buf, Type typ, Qualifier qf) {
    ref bytes.Buffer buf = ref _addr_buf.val;

    writeType(_addr_buf, typ, qf, make_slice<Type>(0, 8));
}

// instanceMarker is the prefix for an instantiated type
// in "non-evaluated" instance form.
private static readonly char instanceMarker = '#';



private static void writeType(ptr<bytes.Buffer> _addr_buf, Type typ, Qualifier qf, slice<Type> visited) => func((_, panic, _) => {
    ref bytes.Buffer buf = ref _addr_buf.val;
 
    // Theoretically, this is a quadratic lookup algorithm, but in
    // practice deeply nested composite types with unnamed component
    // types are uncommon. This code is likely more efficient than
    // using a map.
    {
        var t__prev1 = t;

        foreach (var (_, __t) in visited) {
            t = __t;
            if (t == typ) {
                fmt.Fprintf(buf, "○%T", goTypeName(typ)); // cycle to typ
                return ;
            }
        }
        t = t__prev1;
    }

    visited = append(visited, typ);

    switch (typ.type()) {
        case 
            buf.WriteString("<nil>");
            break;
        case ptr<Basic> t:
            if (token.IsExported(t.name)) {
                {
                    ptr<TypeName> (obj, _) = Unsafe.scope.Lookup(t.name)._<ptr<TypeName>>();

                    if (obj != null) {
                        writeTypeName(_addr_buf, obj, qf);
                        break;
                    }

                }
            }
            if (gcCompatibilityMode) { 
                // forget the alias names

                if (t.kind == Byte) 
                    t = Typ[Uint8];
                else if (t.kind == Rune) 
                    t = Typ[Int32];
                            }
            buf.WriteString(t.name);
            break;
        case ptr<Array> t:
            fmt.Fprintf(buf, "[%d]", t.len);
            writeType(_addr_buf, t.elem, qf, visited);
            break;
        case ptr<Slice> t:
            buf.WriteString("[]");
            writeType(_addr_buf, t.elem, qf, visited);
            break;
        case ptr<Struct> t:
            buf.WriteString("struct{");
            {
                var i__prev1 = i;

                foreach (var (__i, __f) in t.fields) {
                    i = __i;
                    f = __f;
                    if (i > 0) {
                        buf.WriteString("; ");
                    } 
                    // This doesn't do the right thing for embedded type
                    // aliases where we should print the alias name, not
                    // the aliased type (see issue #44410).
                    if (!f.embedded) {
                        buf.WriteString(f.name);
                        buf.WriteByte(' ');
                    }
                    writeType(_addr_buf, f.typ, qf, visited);
                    {
                        var tag = t.Tag(i);

                        if (tag != "") {
                            fmt.Fprintf(buf, " %q", tag);
                        }

                    }
                }

                i = i__prev1;
            }

            buf.WriteByte('}');
            break;
        case ptr<Pointer> t:
            buf.WriteByte('*');
            writeType(_addr_buf, t.@base, qf, visited);
            break;
        case ptr<Tuple> t:
            writeTuple(_addr_buf, _addr_t, false, qf, visited);
            break;
        case ptr<Signature> t:
            buf.WriteString("func");
            writeSignature(_addr_buf, _addr_t, qf, visited);
            break;
        case ptr<_Sum> t:
            {
                var i__prev1 = i;
                var t__prev1 = t;

                foreach (var (__i, __t) in t.types) {
                    i = __i;
                    t = __t;
                    if (i > 0) {
                        buf.WriteString(", ");
                    }
                    writeType(_addr_buf, t, qf, visited);
                }

                i = i__prev1;
                t = t__prev1;
            }
            break;
        case ptr<Interface> t:
            buf.WriteString("interface{");
            var empty = true;
            if (gcCompatibilityMode) { 
                // print flattened interface
                // (useful to compare against gc-generated interfaces)
                {
                    var i__prev1 = i;
                    var m__prev1 = m;

                    foreach (var (__i, __m) in t.allMethods) {
                        i = __i;
                        m = __m;
                        if (i > 0) {
                            buf.WriteString("; ");
                        }
                        buf.WriteString(m.name);
                        writeSignature(_addr_buf, m.typ._<ptr<Signature>>(), qf, visited);
                        empty = false;
                    }
            else

                    i = i__prev1;
                    m = m__prev1;
                }

                if (!empty && t.allTypes != null) {
                    buf.WriteString("; ");
                }
                if (t.allTypes != null) {
                    buf.WriteString("type ");
                    writeType(_addr_buf, t.allTypes, qf, visited);
                }
            } { 
                // print explicit interface methods and embedded types
                {
                    var i__prev1 = i;
                    var m__prev1 = m;

                    foreach (var (__i, __m) in t.methods) {
                        i = __i;
                        m = __m;
                        if (i > 0) {
                            buf.WriteString("; ");
                        }
                        buf.WriteString(m.name);
                        writeSignature(_addr_buf, m.typ._<ptr<Signature>>(), qf, visited);
                        empty = false;
                    }

                    i = i__prev1;
                    m = m__prev1;
                }

                if (!empty && t.types != null) {
                    buf.WriteString("; ");
                }
                if (t.types != null) {
                    buf.WriteString("type ");
                    writeType(_addr_buf, t.types, qf, visited);
                    empty = false;
                }
                if (!empty && len(t.embeddeds) > 0) {
                    buf.WriteString("; ");
                }
                {
                    var i__prev1 = i;

                    foreach (var (__i, __typ) in t.embeddeds) {
                        i = __i;
                        typ = __typ;
                        if (i > 0) {
                            buf.WriteString("; ");
                        }
                        writeType(_addr_buf, typ, qf, visited);
                        empty = false;
                    }

                    i = i__prev1;
                }
            }
            if (t.allMethods == null || len(t.methods) > len(t.allMethods)) {
                if (!empty) {
                    buf.WriteByte(' ');
                }
                buf.WriteString("/* incomplete */");
            }
            buf.WriteByte('}');
            break;
        case ptr<Map> t:
            buf.WriteString("map[");
            writeType(_addr_buf, t.key, qf, visited);
            buf.WriteByte(']');
            writeType(_addr_buf, t.elem, qf, visited);
            break;
        case ptr<Chan> t:
            @string s = default;
            bool parens = default;

            if (t.dir == SendRecv) 
                s = "chan "; 
                // chan (<-chan T) requires parentheses
                {
                    ptr<Chan> (c, _) = t.elem._<ptr<Chan>>();

                    if (c != null && c.dir == RecvOnly) {
                        parens = true;
                    }

                }
            else if (t.dir == SendOnly) 
                s = "chan<- ";
            else if (t.dir == RecvOnly) 
                s = "<-chan ";
            else 
                panic("unreachable");
                        buf.WriteString(s);
            if (parens) {
                buf.WriteByte('(');
            }
            writeType(_addr_buf, t.elem, qf, visited);
            if (parens) {
                buf.WriteByte(')');
            }
            break;
        case ptr<Named> t:
            writeTypeName(_addr_buf, _addr_t.obj, qf);
            if (t.targs != null) { 
                // instantiated type
                buf.WriteByte('[');
                writeTypeList(_addr_buf, t.targs, qf, visited);
                buf.WriteByte(']');
            }
            else if (t.tparams != null) { 
                // parameterized type
                writeTParamList(_addr_buf, t.tparams, qf, visited);
            }
            break;
        case ptr<_TypeParam> t:
            s = "?";
            if (t.obj != null) {
                s = t.obj.name;
            }
            buf.WriteString(s + subscript(t.id));
            break;
        case ptr<instance> t:
            buf.WriteByte(instanceMarker); // indicate "non-evaluated" syntactic instance
            writeTypeName(_addr_buf, _addr_t.@base.obj, qf);
            buf.WriteByte('[');
            writeTypeList(_addr_buf, t.targs, qf, visited);
            buf.WriteByte(']');
            break;
        case ptr<bottom> t:
            buf.WriteString("⊥");
            break;
        case ptr<top> t:
            buf.WriteString("⊤");
            break;
        default:
        {
            var t = typ.type();
            buf.WriteString(t.String());
            break;
        }
    }
});

private static void writeTypeList(ptr<bytes.Buffer> _addr_buf, slice<Type> list, Qualifier qf, slice<Type> visited) {
    ref bytes.Buffer buf = ref _addr_buf.val;

    foreach (var (i, typ) in list) {
        if (i > 0) {
            buf.WriteString(", ");
        }
        writeType(_addr_buf, typ, qf, visited);
    }
}

private static void writeTParamList(ptr<bytes.Buffer> _addr_buf, slice<ptr<TypeName>> list, Qualifier qf, slice<Type> visited) {
    ref bytes.Buffer buf = ref _addr_buf.val;
 
    // TODO(rFindley) compare this with the corresponding implementation in types2
    buf.WriteString("[");
    Type prev = default;
    foreach (var (i, p) in list) { 
        // TODO(rFindley) support 'any' sugar here.
        Type b = _addr_emptyInterface;
        {
            ptr<_TypeParam> t__prev1 = t;

            ptr<_TypeParam> (t, _) = p.typ._<ptr<_TypeParam>>();

            if (t != null && t.bound != null) {
                b = t.bound;
            }

            t = t__prev1;

        }
        if (i > 0) {
            if (b != prev) { 
                // type bound changed - write previous one before advancing
                buf.WriteByte(' ');
                writeType(_addr_buf, prev, qf, visited);
            }
            buf.WriteString(", ");
        }
        prev = b;

        {
            ptr<_TypeParam> t__prev1 = t;

            (t, _) = p.typ._<ptr<_TypeParam>>();

            if (t != null) {
                writeType(_addr_buf, t, qf, visited);
            }
            else
 {
                buf.WriteString(p.name);
            }

            t = t__prev1;

        }
    }    if (prev != null) {
        buf.WriteByte(' ');
        writeType(_addr_buf, prev, qf, visited);
    }
    buf.WriteByte(']');
}

private static void writeTypeName(ptr<bytes.Buffer> _addr_buf, ptr<TypeName> _addr_obj, Qualifier qf) {
    ref bytes.Buffer buf = ref _addr_buf.val;
    ref TypeName obj = ref _addr_obj.val;

    @string s = "<Named w/o object>";
    if (obj != null) {
        if (obj.pkg != null) {
            writePackage(buf, obj.pkg, qf);
        }
        s = obj.name;
    }
    buf.WriteString(s);
}

private static void writeTuple(ptr<bytes.Buffer> _addr_buf, ptr<Tuple> _addr_tup, bool variadic, Qualifier qf, slice<Type> visited) => func((_, panic, _) => {
    ref bytes.Buffer buf = ref _addr_buf.val;
    ref Tuple tup = ref _addr_tup.val;

    buf.WriteByte('(');
    if (tup != null) {
        foreach (var (i, v) in tup.vars) {
            if (i > 0) {
                buf.WriteString(", ");
            }
            if (v.name != "") {
                buf.WriteString(v.name);
                buf.WriteByte(' ');
            }
            var typ = v.typ;
            if (variadic && i == len(tup.vars) - 1) {
                {
                    ptr<Slice> (s, ok) = typ._<ptr<Slice>>();

                    if (ok) {
                        buf.WriteString("...");
                        typ = s.elem;
                    }
                    else
 { 
                        // special case:
                        // append(s, "foo"...) leads to signature func([]byte, string...)
                        {
                            var t = asBasic(typ);

                            if (t == null || t.kind != String) {
                                panic("internal error: string type expected");
                            }

                        }
                        writeType(_addr_buf, typ, qf, visited);
                        buf.WriteString("...");
                        continue;
                    }

                }
            }
            writeType(_addr_buf, typ, qf, visited);
        }
    }
    buf.WriteByte(')');
});

// WriteSignature writes the representation of the signature sig to buf,
// without a leading "func" keyword.
// The Qualifier controls the printing of
// package-level objects, and may be nil.
public static void WriteSignature(ptr<bytes.Buffer> _addr_buf, ptr<Signature> _addr_sig, Qualifier qf) {
    ref bytes.Buffer buf = ref _addr_buf.val;
    ref Signature sig = ref _addr_sig.val;

    writeSignature(_addr_buf, _addr_sig, qf, make_slice<Type>(0, 8));
}

private static void writeSignature(ptr<bytes.Buffer> _addr_buf, ptr<Signature> _addr_sig, Qualifier qf, slice<Type> visited) {
    ref bytes.Buffer buf = ref _addr_buf.val;
    ref Signature sig = ref _addr_sig.val;

    if (sig.tparams != null) {
        writeTParamList(_addr_buf, sig.tparams, qf, visited);
    }
    writeTuple(_addr_buf, _addr_sig.@params, sig.variadic, qf, visited);

    var n = sig.results.Len();
    if (n == 0) { 
        // no result
        return ;
    }
    buf.WriteByte(' ');
    if (n == 1 && sig.results.vars[0].name == "") { 
        // single unnamed result
        writeType(_addr_buf, sig.results.vars[0].typ, qf, visited);
        return ;
    }
    writeTuple(_addr_buf, _addr_sig.results, false, qf, visited);
}

// subscript returns the decimal (utf8) representation of x using subscript digits.
private static @string subscript(ulong x) {
    const var w = len("₀"); // all digits 0...9 have the same utf8 width
 // all digits 0...9 have the same utf8 width
    array<byte> buf = new array<byte>(32 * w);
    var i = len(buf);
    while (true) {
        i -= w;
        utf8.EncodeRune(buf[(int)i..], '₀' + rune(x % 10)); // '₀' == U+2080
        x /= 10;
        if (x == 0) {
            break;
        }
    }
    return string(buf[(int)i..]);
}

} // end types_package
