// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2022 March 13 05:53:14 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Program Files\Go\src\go\types\object.go
namespace go.go;

using bytes = bytes_package;
using fmt = fmt_package;
using constant = go.constant_package;
using token = go.token_package;


// An Object describes a named language entity such as a package,
// constant, type, variable, function (incl. methods), or label.
// All objects implement the Object interface.
//

public static partial class types_package {

public partial interface Object {
    token.Pos Parent(); // scope in which this object is declared; nil for methods and struct fields
    token.Pos Pos(); // position of object identifier in declaration
    token.Pos Pkg(); // package to which this object belongs; nil for labels and objects in the Universe scope
    token.Pos Name(); // package local object name
    token.Pos Type(); // object type
    token.Pos Exported(); // reports whether the name starts with a capital letter
    token.Pos Id(); // object name if exported, qualified name if not exported (see func Id)

// String returns a human-readable string of the object.
    token.Pos String(); // order reflects a package-level object's source order: if object
// a is before object b in the source, then a.order() < b.order().
// order returns a value > 0 for package-level objects; it returns
// 0 for all other objects (including objects in file scopes).
    token.Pos order(); // color returns the object's color.
    token.Pos color(); // setType sets the type of the object.
    token.Pos setType(Type _p0); // setOrder sets the order number of the object. It must be > 0.
    token.Pos setOrder(uint _p0); // setColor sets the object's color. It must not be white.
    token.Pos setColor(color color); // setParent sets the parent scope of the object.
    token.Pos setParent(ptr<Scope> _p0); // sameId reports whether obj.Id() and Id(pkg, name) are the same.
    token.Pos sameId(ptr<Package> pkg, @string name); // scopePos returns the start position of the scope of this Object
    token.Pos scopePos(); // setScopePos sets the start position of the scope for this Object.
    token.Pos setScopePos(token.Pos pos);
}

// Id returns name if it is exported, otherwise it
// returns the name qualified with the package path.
public static @string Id(ptr<Package> _addr_pkg, @string name) {
    ref Package pkg = ref _addr_pkg.val;

    if (token.IsExported(name)) {
        return name;
    }
    @string path = "_"; 
    // pkg is nil for objects in Universe scope and possibly types
    // introduced via Eval (see also comment in object.sameId)
    if (pkg != null && pkg.path != "") {
        path = pkg.path;
    }
    return path + "." + name;
}

// An object implements the common parts of an Object.
private partial struct @object {
    public ptr<Scope> parent;
    public token.Pos pos;
    public ptr<Package> pkg;
    public @string name;
    public Type typ;
    public uint order_;
    public color color_;
    public token.Pos scopePos_;
}

// color encodes the color of an object (see Checker.objDecl for details).
private partial struct color { // : uint
}

// An object may be painted in one of three colors.
// Color values other than white or black are considered grey.
private static readonly color white = iota;
private static readonly var black = 0;
private static readonly var grey = 1; // must be > white and black

private static @string String(this color c) {

    if (c == white) 
        return "white";
    else if (c == black) 
        return "black";
    else 
        return "grey";
    }

// colorFor returns the (initial) color for an object depending on
// whether its type t is known or not.
private static color colorFor(Type t) {
    if (t != null) {
        return black;
    }
    return white;
}

// Parent returns the scope in which the object is declared.
// The result is nil for methods and struct fields.
private static ptr<Scope> Parent(this ptr<object> _addr_obj) {
    ref object obj = ref _addr_obj.val;

    return _addr_obj.parent!;
}

// Pos returns the declaration position of the object's identifier.
private static token.Pos Pos(this ptr<object> _addr_obj) {
    ref object obj = ref _addr_obj.val;

    return obj.pos;
}

// Pkg returns the package to which the object belongs.
// The result is nil for labels and objects in the Universe scope.
private static ptr<Package> Pkg(this ptr<object> _addr_obj) {
    ref object obj = ref _addr_obj.val;

    return _addr_obj.pkg!;
}

// Name returns the object's (package-local, unqualified) name.
private static @string Name(this ptr<object> _addr_obj) {
    ref object obj = ref _addr_obj.val;

    return obj.name;
}

// Type returns the object's type.
private static Type Type(this ptr<object> _addr_obj) {
    ref object obj = ref _addr_obj.val;

    return obj.typ;
}

// Exported reports whether the object is exported (starts with a capital letter).
// It doesn't take into account whether the object is in a local (function) scope
// or not.
private static bool Exported(this ptr<object> _addr_obj) {
    ref object obj = ref _addr_obj.val;

    return token.IsExported(obj.name);
}

// Id is a wrapper for Id(obj.Pkg(), obj.Name()).
private static @string Id(this ptr<object> _addr_obj) {
    ref object obj = ref _addr_obj.val;

    return Id(_addr_obj.pkg, obj.name);
}

private static @string String(this ptr<object> _addr_obj) => func((_, panic, _) => {
    ref object obj = ref _addr_obj.val;

    panic("abstract");
});
private static uint order(this ptr<object> _addr_obj) {
    ref object obj = ref _addr_obj.val;

    return obj.order_;
}
private static color color(this ptr<object> _addr_obj) {
    ref object obj = ref _addr_obj.val;

    return obj.color_;
}
private static token.Pos scopePos(this ptr<object> _addr_obj) {
    ref object obj = ref _addr_obj.val;

    return obj.scopePos_;
}

private static void setParent(this ptr<object> _addr_obj, ptr<Scope> _addr_parent) {
    ref object obj = ref _addr_obj.val;
    ref Scope parent = ref _addr_parent.val;

    obj.parent = parent;
}
private static void setType(this ptr<object> _addr_obj, Type typ) {
    ref object obj = ref _addr_obj.val;

    obj.typ = typ;
}
private static void setOrder(this ptr<object> _addr_obj, uint order) {
    ref object obj = ref _addr_obj.val;

    assert(order > 0);

    obj.order_ = order;
}
private static void setColor(this ptr<object> _addr_obj, color color) {
    ref object obj = ref _addr_obj.val;

    assert(color != white);

    obj.color_ = color;
}
private static void setScopePos(this ptr<object> _addr_obj, token.Pos pos) {
    ref object obj = ref _addr_obj.val;

    obj.scopePos_ = pos;
}

private static bool sameId(this ptr<object> _addr_obj, ptr<Package> _addr_pkg, @string name) {
    ref object obj = ref _addr_obj.val;
    ref Package pkg = ref _addr_pkg.val;
 
    // spec:
    // "Two identifiers are different if they are spelled differently,
    // or if they appear in different packages and are not exported.
    // Otherwise, they are the same."
    if (name != obj.name) {
        return false;
    }
    if (obj.Exported()) {
        return true;
    }
    if (pkg == null || obj.pkg == null) {
        return pkg == obj.pkg;
    }
    return pkg.path == obj.pkg.path;
}

// A PkgName represents an imported Go package.
// PkgNames don't have a type.
public partial struct PkgName {
    public ref object @object => ref @object_val;
    public ptr<Package> imported;
    public bool used; // set if the package was used
}

// NewPkgName returns a new PkgName object representing an imported package.
// The remaining arguments set the attributes found with all Objects.
public static ptr<PkgName> NewPkgName(token.Pos pos, ptr<Package> _addr_pkg, @string name, ptr<Package> _addr_imported) {
    ref Package pkg = ref _addr_pkg.val;
    ref Package imported = ref _addr_imported.val;

    return addr(new PkgName(object{nil,pos,pkg,name,Typ[Invalid],0,black,token.NoPos},imported,false));
}

// Imported returns the package that was imported.
// It is distinct from Pkg(), which is the package containing the import statement.
private static ptr<Package> Imported(this ptr<PkgName> _addr_obj) {
    ref PkgName obj = ref _addr_obj.val;

    return _addr_obj.imported!;
}

// A Const represents a declared constant.
public partial struct Const {
    public ref object @object => ref @object_val;
    public constant.Value val;
}

// NewConst returns a new constant with value val.
// The remaining arguments set the attributes found with all Objects.
public static ptr<Const> NewConst(token.Pos pos, ptr<Package> _addr_pkg, @string name, Type typ, constant.Value val) {
    ref Package pkg = ref _addr_pkg.val;

    return addr(new Const(object{nil,pos,pkg,name,typ,0,colorFor(typ),token.NoPos},val));
}

// Val returns the constant's value.
private static constant.Value Val(this ptr<Const> _addr_obj) {
    ref Const obj = ref _addr_obj.val;

    return obj.val;
}

private static void isDependency(this ptr<Const> _addr__p0) {
    ref Const _p0 = ref _addr__p0.val;

} // a constant may be a dependency of an initialization expression

// A TypeName represents a name for a (defined or alias) type.
public partial struct TypeName {
    public ref object @object => ref @object_val;
}

// NewTypeName returns a new type name denoting the given typ.
// The remaining arguments set the attributes found with all Objects.
//
// The typ argument may be a defined (Named) type or an alias type.
// It may also be nil such that the returned TypeName can be used as
// argument for NewNamed, which will set the TypeName's type as a side-
// effect.
public static ptr<TypeName> NewTypeName(token.Pos pos, ptr<Package> _addr_pkg, @string name, Type typ) {
    ref Package pkg = ref _addr_pkg.val;

    return addr(new TypeName(object{nil,pos,pkg,name,typ,0,colorFor(typ),token.NoPos}));
}

// IsAlias reports whether obj is an alias name for a type.
private static bool IsAlias(this ptr<TypeName> _addr_obj) {
    ref TypeName obj = ref _addr_obj.val;

    switch (obj.typ.type()) {
        case 
            return false;
            break;
        case ptr<Basic> t:
            if (obj.pkg == Unsafe) {
                return false;
            } 
            // Any user-defined type name for a basic type is an alias for a
            // basic type (because basic types are pre-declared in the Universe
            // scope, outside any package scope), and so is any type name with
            // a different name than the name of the basic type it refers to.
            // Additionally, we need to look for "byte" and "rune" because they
            // are aliases but have the same names (for better error messages).
            return obj.pkg != null || t.name != obj.name || t == universeByte || t == universeRune;
            break;
        case ptr<Named> t:
            return obj != t.obj;
            break;
        default:
        {
            var t = obj.typ.type();
            return true;
            break;
        }
    }
}

// A Variable represents a declared variable (including function parameters and results, and struct fields).
public partial struct Var {
    public ref object @object => ref @object_val;
    public bool embedded; // if set, the variable is an embedded struct field, and name is the type name
    public bool isField; // var is struct field
    public bool used; // set if the variable was used
}

// NewVar returns a new variable.
// The arguments set the attributes found with all Objects.
public static ptr<Var> NewVar(token.Pos pos, ptr<Package> _addr_pkg, @string name, Type typ) {
    ref Package pkg = ref _addr_pkg.val;

    return addr(new Var(object:object{nil,pos,pkg,name,typ,0,colorFor(typ),token.NoPos}));
}

// NewParam returns a new variable representing a function parameter.
public static ptr<Var> NewParam(token.Pos pos, ptr<Package> _addr_pkg, @string name, Type typ) {
    ref Package pkg = ref _addr_pkg.val;

    return addr(new Var(object:object{nil,pos,pkg,name,typ,0,colorFor(typ),token.NoPos},used:true)); // parameters are always 'used'
}

// NewField returns a new variable representing a struct field.
// For embedded fields, the name is the unqualified type name
/// under which the field is accessible.
public static ptr<Var> NewField(token.Pos pos, ptr<Package> _addr_pkg, @string name, Type typ, bool embedded) {
    ref Package pkg = ref _addr_pkg.val;

    return addr(new Var(object:object{nil,pos,pkg,name,typ,0,colorFor(typ),token.NoPos},embedded:embedded,isField:true));
}

// Anonymous reports whether the variable is an embedded field.
// Same as Embedded; only present for backward-compatibility.
private static bool Anonymous(this ptr<Var> _addr_obj) {
    ref Var obj = ref _addr_obj.val;

    return obj.embedded;
}

// Embedded reports whether the variable is an embedded field.
private static bool Embedded(this ptr<Var> _addr_obj) {
    ref Var obj = ref _addr_obj.val;

    return obj.embedded;
}

// IsField reports whether the variable is a struct field.
private static bool IsField(this ptr<Var> _addr_obj) {
    ref Var obj = ref _addr_obj.val;

    return obj.isField;
}

private static void isDependency(this ptr<Var> _addr__p0) {
    ref Var _p0 = ref _addr__p0.val;

} // a variable may be a dependency of an initialization expression

// A Func represents a declared function, concrete method, or abstract
// (interface) method. Its Type() is always a *Signature.
// An abstract method may belong to many interfaces due to embedding.
public partial struct Func {
    public ref object @object => ref @object_val;
    public bool hasPtrRecv; // only valid for methods that don't have a type yet
}

// NewFunc returns a new function with the given signature, representing
// the function's type.
public static ptr<Func> NewFunc(token.Pos pos, ptr<Package> _addr_pkg, @string name, ptr<Signature> _addr_sig) {
    ref Package pkg = ref _addr_pkg.val;
    ref Signature sig = ref _addr_sig.val;
 
    // don't store a (typed) nil signature
    Type typ = default;
    if (sig != null) {
        typ = sig;
    }
    return addr(new Func(object{nil,pos,pkg,name,typ,0,colorFor(typ),token.NoPos},false));
}

// FullName returns the package- or receiver-type-qualified name of
// function or method obj.
private static @string FullName(this ptr<Func> _addr_obj) {
    ref Func obj = ref _addr_obj.val;

    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
    writeFuncName(_addr_buf, _addr_obj, null);
    return buf.String();
}

// Scope returns the scope of the function's body block.
private static ptr<Scope> Scope(this ptr<Func> _addr_obj) {
    ref Func obj = ref _addr_obj.val;

    return obj.typ._<ptr<Signature>>().scope;
}

private static void isDependency(this ptr<Func> _addr__p0) {
    ref Func _p0 = ref _addr__p0.val;

} // a function may be a dependency of an initialization expression

// A Label represents a declared label.
// Labels don't have a type.
public partial struct Label {
    public ref object @object => ref @object_val;
    public bool used; // set if the label was used
}

// NewLabel returns a new label.
public static ptr<Label> NewLabel(token.Pos pos, ptr<Package> _addr_pkg, @string name) {
    ref Package pkg = ref _addr_pkg.val;

    return addr(new Label(object{pos:pos,pkg:pkg,name:name,typ:Typ[Invalid],color_:black},false));
}

// A Builtin represents a built-in function.
// Builtins don't have a valid type.
public partial struct Builtin {
    public ref object @object => ref @object_val;
    public builtinId id;
}

private static ptr<Builtin> newBuiltin(builtinId id) {
    return addr(new Builtin(object{name:predeclaredFuncs[id].name,typ:Typ[Invalid],color_:black},id));
}

// Nil represents the predeclared value nil.
public partial struct Nil {
    public ref object @object => ref @object_val;
}

private static void writeObject(ptr<bytes.Buffer> _addr_buf, Object obj, Qualifier qf) => func((_, panic, _) => {
    ref bytes.Buffer buf = ref _addr_buf.val;

    ptr<TypeName> tname;
    var typ = obj.Type();

    switch (obj.type()) {
        case ptr<PkgName> obj:
            fmt.Fprintf(buf, "package %s", obj.Name());
            {
                var path = obj.imported.path;

                if (path != "" && path != obj.name) {
                    fmt.Fprintf(buf, " (%q)", path);
                }

            }
            return ;
            break;
        case ptr<Const> obj:
            buf.WriteString("const");
            break;
        case ptr<TypeName> obj:
            tname = obj;
            buf.WriteString("type");
            break;
        case ptr<Var> obj:
            if (obj.isField) {
                buf.WriteString("field");
            }
            else
 {
                buf.WriteString("var");
            }
            break;
        case ptr<Func> obj:
            buf.WriteString("func ");
            writeFuncName(_addr_buf, _addr_obj, qf);
            if (typ != null) {
                WriteSignature(buf, typ._<ptr<Signature>>(), qf);
            }
            return ;
            break;
        case ptr<Label> obj:
            buf.WriteString("label");
            typ = null;
            break;
        case ptr<Builtin> obj:
            buf.WriteString("builtin");
            typ = null;
            break;
        case ptr<Nil> obj:
            buf.WriteString("nil");
            return ;
            break;
        default:
        {
            var obj = obj.type();
            panic(fmt.Sprintf("writeObject(%T)", obj));
            break;
        }

    }

    buf.WriteByte(' '); 

    // For package-level objects, qualify the name.
    if (obj.Pkg() != null && obj.Pkg().scope.Lookup(obj.Name()) == obj) {
        writePackage(_addr_buf, _addr_obj.Pkg(), qf);
    }
    buf.WriteString(obj.Name());

    if (typ == null) {
        return ;
    }
    if (tname != null) { 
        // We have a type object: Don't print anything more for
        // basic types since there's no more information (names
        // are the same; see also comment in TypeName.IsAlias).
        {
            ptr<Basic> (_, ok) = typ._<ptr<Basic>>();

            if (ok) {
                return ;
            }

        }
        if (tname.IsAlias()) {
            buf.WriteString(" =");
        }
        else
 {
            typ = under(typ);
        }
    }
    buf.WriteByte(' ');
    WriteType(buf, typ, qf);
});

private static void writePackage(ptr<bytes.Buffer> _addr_buf, ptr<Package> _addr_pkg, Qualifier qf) {
    ref bytes.Buffer buf = ref _addr_buf.val;
    ref Package pkg = ref _addr_pkg.val;

    if (pkg == null) {
        return ;
    }
    @string s = default;
    if (qf != null) {
        s = qf(pkg);
    }
    else
 {
        s = pkg.Path();
    }
    if (s != "") {
        buf.WriteString(s);
        buf.WriteByte('.');
    }
}

// ObjectString returns the string form of obj.
// The Qualifier controls the printing of
// package-level objects, and may be nil.
public static @string ObjectString(Object obj, Qualifier qf) {
    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
    writeObject(_addr_buf, obj, qf);
    return buf.String();
}

private static @string String(this ptr<PkgName> _addr_obj) {
    ref PkgName obj = ref _addr_obj.val;

    return ObjectString(obj, null);
}
private static @string String(this ptr<Const> _addr_obj) {
    ref Const obj = ref _addr_obj.val;

    return ObjectString(obj, null);
}
private static @string String(this ptr<TypeName> _addr_obj) {
    ref TypeName obj = ref _addr_obj.val;

    return ObjectString(obj, null);
}
private static @string String(this ptr<Var> _addr_obj) {
    ref Var obj = ref _addr_obj.val;

    return ObjectString(obj, null);
}
private static @string String(this ptr<Func> _addr_obj) {
    ref Func obj = ref _addr_obj.val;

    return ObjectString(obj, null);
}
private static @string String(this ptr<Label> _addr_obj) {
    ref Label obj = ref _addr_obj.val;

    return ObjectString(obj, null);
}
private static @string String(this ptr<Builtin> _addr_obj) {
    ref Builtin obj = ref _addr_obj.val;

    return ObjectString(obj, null);
}
private static @string String(this ptr<Nil> _addr_obj) {
    ref Nil obj = ref _addr_obj.val;

    return ObjectString(obj, null);
}

private static void writeFuncName(ptr<bytes.Buffer> _addr_buf, ptr<Func> _addr_f, Qualifier qf) {
    ref bytes.Buffer buf = ref _addr_buf.val;
    ref Func f = ref _addr_f.val;

    if (f.typ != null) {
        ptr<Signature> sig = f.typ._<ptr<Signature>>();
        {
            var recv = sig.Recv();

            if (recv != null) {
                buf.WriteByte('(');
                {
                    ptr<Interface> (_, ok) = recv.Type()._<ptr<Interface>>();

                    if (ok) { 
                        // gcimporter creates abstract methods of
                        // named interfaces using the interface type
                        // (not the named type) as the receiver.
                        // Don't print it in full.
                        buf.WriteString("interface");
                    }
                    else
 {
                        WriteType(buf, recv.Type(), qf);
                    }

                }
                buf.WriteByte(')');
                buf.WriteByte('.');
            }
            else if (f.pkg != null) {
                writePackage(_addr_buf, _addr_f.pkg, qf);
            }

        }
    }
    buf.WriteString(f.name);
}

} // end types_package
