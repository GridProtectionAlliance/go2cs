// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ir -- go2cs converted at 2022 March 06 22:49:17 UTC
// import "cmd/compile/internal/ir" ==> using ir = go.cmd.compile.@internal.ir_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\ir\type.go
using @base = go.cmd.compile.@internal.@base_package;
using types = go.cmd.compile.@internal.types_package;
using src = go.cmd.@internal.src_package;
using fmt = go.fmt_package;
using System;


namespace go.cmd.compile.@internal;

public static partial class ir_package {

    // Nodes that represent the syntax of a type before type-checking.
    // After type-checking, they serve only as shells around a *types.Type.
    // Calling TypeNode converts a *types.Type to a Node shell.

    // An Ntype is a Node that syntactically looks like a type.
    // It can be the raw syntax for a type before typechecking,
    // or it can be an OTYPE with Type() set to a *types.Type.
    // Note that syntax doesn't guarantee it's a type: an expression
    // like *fmt is an Ntype (we don't know whether names are types yet),
    // but at least 1+1 is not an Ntype.
public partial interface Ntype {
    void CanBeNtype();
}

// A miniType is a minimal type syntax Node implementation,
// to be embedded as the first field in a larger node implementation.
private partial struct miniType {
    public ref miniNode miniNode => ref miniNode_val;
    public ptr<types.Type> typ;
}

private static void CanBeNtype(this ptr<miniType> _addr__p0) {
    ref miniType _p0 = ref _addr__p0.val;

}

private static ptr<types.Type> Type(this ptr<miniType> _addr_n) {
    ref miniType n = ref _addr_n.val;

    return _addr_n.typ!;
}

// setOTYPE changes n to be an OTYPE node returning t.
// Rewriting the node in place this way should not be strictly
// necessary (we should be able to update the uses with
// proper OTYPE nodes), but it's mostly harmless and easy
// to keep doing for now.
//
// setOTYPE also records t.Nod = self if t.Nod is not already set.
// (Some types are shared by multiple OTYPE nodes, so only
// the first such node is used as t.Nod.)
private static void setOTYPE(this ptr<miniType> _addr_n, ptr<types.Type> _addr_t, Ntype self) => func((_, panic, _) => {
    ref miniType n = ref _addr_n.val;
    ref types.Type t = ref _addr_t.val;

    if (n.typ != null) {
        panic(n.op.String() + " SetType: type already set");
    }
    n.op = OTYPE;
    n.typ = t;
    t.SetNod(self);

});

private static ptr<types.Sym> Sym(this ptr<miniType> _addr_n) {
    ref miniType n = ref _addr_n.val;

    return _addr_null!;
} // for Format OTYPE
private static bool Implicit(this ptr<miniType> _addr_n) {
    ref miniType n = ref _addr_n.val;

    return false;
} // for Format OTYPE

// A ChanType represents a chan Elem syntax with the direction Dir.
public partial struct ChanType {
    public ref miniType miniType => ref miniType_val;
    public Ntype Elem;
    public types.ChanDir Dir;
}

public static ptr<ChanType> NewChanType(src.XPos pos, Ntype elem, types.ChanDir dir) {
    ptr<ChanType> n = addr(new ChanType(Elem:elem,Dir:dir));
    n.op = OTCHAN;
    n.pos = pos;
    return _addr_n!;
}

private static void SetOTYPE(this ptr<ChanType> _addr_n, ptr<types.Type> _addr_t) {
    ref ChanType n = ref _addr_n.val;
    ref types.Type t = ref _addr_t.val;

    n.setOTYPE(t, n);
    n.Elem = null;
}

// A MapType represents a map[Key]Value type syntax.
public partial struct MapType {
    public ref miniType miniType => ref miniType_val;
    public Ntype Key;
    public Ntype Elem;
}

public static ptr<MapType> NewMapType(src.XPos pos, Ntype key, Ntype elem) {
    ptr<MapType> n = addr(new MapType(Key:key,Elem:elem));
    n.op = OTMAP;
    n.pos = pos;
    return _addr_n!;
}

private static void SetOTYPE(this ptr<MapType> _addr_n, ptr<types.Type> _addr_t) {
    ref MapType n = ref _addr_n.val;
    ref types.Type t = ref _addr_t.val;

    n.setOTYPE(t, n);
    n.Key = null;
    n.Elem = null;
}

// A StructType represents a struct { ... } type syntax.
public partial struct StructType {
    public ref miniType miniType => ref miniType_val;
    public slice<ptr<Field>> Fields;
}

public static ptr<StructType> NewStructType(src.XPos pos, slice<ptr<Field>> fields) {
    ptr<StructType> n = addr(new StructType(Fields:fields));
    n.op = OTSTRUCT;
    n.pos = pos;
    return _addr_n!;
}

private static void SetOTYPE(this ptr<StructType> _addr_n, ptr<types.Type> _addr_t) {
    ref StructType n = ref _addr_n.val;
    ref types.Type t = ref _addr_t.val;

    n.setOTYPE(t, n);
    n.Fields = null;
}

// An InterfaceType represents a struct { ... } type syntax.
public partial struct InterfaceType {
    public ref miniType miniType => ref miniType_val;
    public slice<ptr<Field>> Methods;
}

public static ptr<InterfaceType> NewInterfaceType(src.XPos pos, slice<ptr<Field>> methods) {
    ptr<InterfaceType> n = addr(new InterfaceType(Methods:methods));
    n.op = OTINTER;
    n.pos = pos;
    return _addr_n!;
}

private static void SetOTYPE(this ptr<InterfaceType> _addr_n, ptr<types.Type> _addr_t) {
    ref InterfaceType n = ref _addr_n.val;
    ref types.Type t = ref _addr_t.val;

    n.setOTYPE(t, n);
    n.Methods = null;
}

// A FuncType represents a func(Args) Results type syntax.
public partial struct FuncType {
    public ref miniType miniType => ref miniType_val;
    public ptr<Field> Recv;
    public slice<ptr<Field>> Params;
    public slice<ptr<Field>> Results;
}

public static ptr<FuncType> NewFuncType(src.XPos pos, ptr<Field> _addr_rcvr, slice<ptr<Field>> args, slice<ptr<Field>> results) {
    ref Field rcvr = ref _addr_rcvr.val;

    ptr<FuncType> n = addr(new FuncType(Recv:rcvr,Params:args,Results:results));
    n.op = OTFUNC;
    n.pos = pos;
    return _addr_n!;
}

private static void SetOTYPE(this ptr<FuncType> _addr_n, ptr<types.Type> _addr_t) {
    ref FuncType n = ref _addr_n.val;
    ref types.Type t = ref _addr_t.val;

    n.setOTYPE(t, n);
    n.Recv = null;
    n.Params = null;
    n.Results = null;
}

// A Field is a declared struct field, interface method, or function argument.
// It is not a Node.
public partial struct Field {
    public src.XPos Pos;
    public ptr<types.Sym> Sym;
    public Ntype Ntype;
    public ptr<types.Type> Type;
    public bool Embedded;
    public bool IsDDD;
    public @string Note;
    public ptr<Name> Decl;
}

public static ptr<Field> NewField(src.XPos pos, ptr<types.Sym> _addr_sym, Ntype ntyp, ptr<types.Type> _addr_typ) {
    ref types.Sym sym = ref _addr_sym.val;
    ref types.Type typ = ref _addr_typ.val;

    return addr(new Field(Pos:pos,Sym:sym,Ntype:ntyp,Type:typ));
}

private static @string String(this ptr<Field> _addr_f) {
    ref Field f = ref _addr_f.val;

    @string typ = default;
    if (f.Type != null) {
        typ = fmt.Sprint(f.Type);
    }
    else
 {
        typ = fmt.Sprint(f.Ntype);
    }
    if (f.Sym != null) {
        return fmt.Sprintf("%v %v", f.Sym, typ);
    }
    return typ;

}

// TODO(mdempsky): Make Field a Node again so these can be generated?
// Fields are Nodes in go/ast and cmd/compile/internal/syntax.

private static ptr<Field> copyField(ptr<Field> _addr_f) {
    ref Field f = ref _addr_f.val;

    if (f == null) {
        return _addr_null!;
    }
    ref Field c = ref heap(f, out ptr<Field> _addr_c);
    return _addr__addr_c!;

}
private static bool doField(ptr<Field> _addr_f, Func<Node, bool> @do) {
    ref Field f = ref _addr_f.val;

    if (f == null) {
        return false;
    }
    if (f.Decl != null && do(f.Decl)) {
        return true;
    }
    if (f.Ntype != null && do(f.Ntype)) {
        return true;
    }
    return false;

}
private static Node editField(ptr<Field> _addr_f, Func<Node, Node> edit) {
    ref Field f = ref _addr_f.val;

    if (f == null) {
        return ;
    }
    if (f.Decl != null) {
        f.Decl = edit(f.Decl)._<ptr<Name>>();
    }
    if (f.Ntype != null) {
        f.Ntype = edit(f.Ntype)._<Ntype>();
    }
}

private static slice<ptr<Field>> copyFields(slice<ptr<Field>> list) {
    var @out = make_slice<ptr<Field>>(len(list));
    foreach (var (i, f) in list) {
        out[i] = copyField(_addr_f);
    }    return out;
}
private static bool doFields(slice<ptr<Field>> list, Func<Node, bool> @do) {
    foreach (var (_, x) in list) {
        if (doField(_addr_x, do)) {
            return true;
        }
    }    return false;

}
private static Node editFields(slice<ptr<Field>> list, Func<Node, Node> edit) {
    foreach (var (_, f) in list) {
        editField(_addr_f, edit);
    }
}

// A SliceType represents a []Elem type syntax.
// If DDD is true, it's the ...Elem at the end of a function list.
public partial struct SliceType {
    public ref miniType miniType => ref miniType_val;
    public Ntype Elem;
    public bool DDD;
}

public static ptr<SliceType> NewSliceType(src.XPos pos, Ntype elem) {
    ptr<SliceType> n = addr(new SliceType(Elem:elem));
    n.op = OTSLICE;
    n.pos = pos;
    return _addr_n!;
}

private static void SetOTYPE(this ptr<SliceType> _addr_n, ptr<types.Type> _addr_t) {
    ref SliceType n = ref _addr_n.val;
    ref types.Type t = ref _addr_t.val;

    n.setOTYPE(t, n);
    n.Elem = null;
}

// An ArrayType represents a [Len]Elem type syntax.
// If Len is nil, the type is a [...]Elem in an array literal.
public partial struct ArrayType {
    public ref miniType miniType => ref miniType_val;
    public Node Len;
    public Ntype Elem;
}

public static ptr<ArrayType> NewArrayType(src.XPos pos, Node len, Ntype elem) {
    ptr<ArrayType> n = addr(new ArrayType(Len:len,Elem:elem));
    n.op = OTARRAY;
    n.pos = pos;
    return _addr_n!;
}

private static void SetOTYPE(this ptr<ArrayType> _addr_n, ptr<types.Type> _addr_t) {
    ref ArrayType n = ref _addr_n.val;
    ref types.Type t = ref _addr_t.val;

    n.setOTYPE(t, n);
    n.Len = null;
    n.Elem = null;
}

// A typeNode is a Node wrapper for type t.
private partial struct typeNode {
    public ref miniNode miniNode => ref miniNode_val;
    public ptr<types.Type> typ;
}

private static ptr<typeNode> newTypeNode(src.XPos pos, ptr<types.Type> _addr_typ) {
    ref types.Type typ = ref _addr_typ.val;

    ptr<typeNode> n = addr(new typeNode(typ:typ));
    n.pos = pos;
    n.op = OTYPE;
    return _addr_n!;
}

private static ptr<types.Type> Type(this ptr<typeNode> _addr_n) {
    ref typeNode n = ref _addr_n.val;

    return _addr_n.typ!;
}
private static ptr<types.Sym> Sym(this ptr<typeNode> _addr_n) {
    ref typeNode n = ref _addr_n.val;

    return _addr_n.typ.Sym()!;
}
private static void CanBeNtype(this ptr<typeNode> _addr_n) {
    ref typeNode n = ref _addr_n.val;

}

// TypeNode returns the Node representing the type t.
public static Ntype TypeNode(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    {
        var n = t.Obj();

        if (n != null) {
            if (n.Type() != t) {
                @base.Fatalf("type skew: %v has type %v, but expected %v", n, n.Type(), t);
            }
            return n._<Ntype>();
        }
    }

    return newTypeNode(src.NoXPos, _addr_t);

}

} // end ir_package
