// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
// This file implements scopes and the objects they contain.
namespace go.go;

using fmt = fmt_package;
using token = go.token_package;
using strings = strings_package;

partial class ast_package {

// A Scope maintains the set of named language entities declared
// in the scope and a link to the immediately surrounding (outer)
// scope.
//
// Deprecated: use the type checker [go/types] instead; see [Object].
[GoType] partial struct Scope {
    public ж<Scope> Outer;
    public map<@string, ж<Object>> Objects;
}

// NewScope creates a new scope nested in the outer scope.
public static ж<Scope> NewScope(ж<Scope> Ꮡouter) {
    ref var outer = ref Ꮡouter.val;

    static readonly UntypedInt n = 4; // initial scope capacity
    return Ꮡ(new Scope(Ꮡouter, new map<@string, ж<Object>>(n)));
}

// Lookup returns the object with the given name if it is
// found in scope s, otherwise it returns nil. Outer scopes
// are ignored.
[GoRecv] public static ж<Object> Lookup(this ref Scope s, @string name) {
    return s.Objects[name];
}

// Insert attempts to insert a named object obj into the scope s.
// If the scope already contains an object alt with the same name,
// Insert leaves the scope unchanged and returns alt. Otherwise
// it inserts obj and returns nil.
[GoRecv] public static ж<Object> /*alt*/ Insert(this ref Scope s, ж<Object> Ꮡobj) {
    ж<Object> alt = default!;

    ref var obj = ref Ꮡobj.val;
    {
        alt = s.Objects[obj.Name]; if (alt == nil) {
            s.Objects[obj.Name] = obj;
        }
    }
    return alt;
}

// Debugging support
[GoRecv] public static @string String(this ref Scope s) {
    ref var buf = ref heap(new strings_package.Builder(), out var Ꮡbuf);
    fmt.Fprintf(~Ꮡbuf, "scope %p {"u8, s);
    if (s != nil && len(s.Objects) > 0) {
        fmt.Fprintln(~Ꮡbuf);
        foreach (var (_, obj) in s.Objects) {
            fmt.Fprintf(~Ꮡbuf, "\t%s %s\n"u8, (~obj).Kind, (~obj).Name);
        }
    }
    fmt.Fprintf(~Ꮡbuf, "}\n"u8);
    return buf.String();
}

// ----------------------------------------------------------------------------
// Objects

// An Object describes a named language entity such as a package,
// constant, type, variable, function (incl. methods), or label.
//
// The Data fields contains object-specific data:
//
//	Kind    Data type         Data value
//	Pkg     *Scope            package scope
//	Con     int               iota for the respective declaration
//
// Deprecated: The relationship between Idents and Objects cannot be
// correctly computed without type information. For example, the
// expression T{K: 0} may denote a struct, map, slice, or array
// literal, depending on the type of T. If T is a struct, then K
// refers to a field of T, whereas for the other types it refers to a
// value in the environment.
//
// New programs should set the [parser.SkipObjectResolution] parser
// flag to disable syntactic object resolution (which also saves CPU
// and memory), and instead use the type checker [go/types] if object
// resolution is desired. See the Defs, Uses, and Implicits fields of
// the [types.Info] struct for details.
[GoType] partial struct Object {
    public ObjKind Kind;
    public @string Name; // declared name
    public any Decl;    // corresponding Field, XxxSpec, FuncDecl, LabeledStmt, AssignStmt, Scope; or nil
    public any Data;    // object-specific data; or nil
    public any Type;    // placeholder for type information; may be nil
}

// NewObj creates a new object of a given kind and name.
public static ж<Object> NewObj(ObjKind kind, @string name) {
    return Ꮡ(new Object(Kind: kind, Name: name));
}

// Pos computes the source position of the declaration of an object name.
// The result may be an invalid position if it cannot be computed
// (obj.Decl may be nil or not correct).
[GoRecv] public static tokenꓸPos Pos(this ref Object obj) {
    @string name = obj.Name;
    switch (obj.Decl.type()) {
    case Field.val d: {
        foreach (var (_, n) in (~d).Names) {
            if ((~n).Name == name) {
                return n.Pos();
            }
        }
        break;
    }
    case ImportSpec.val d: {
        if ((~d).Name != nil && (~(~d).Name).Name == name) {
            return (~d).Name.Pos();
        }
        return (~d).Path.Pos();
    }
    case ValueSpec.val d: {
        foreach (var (_, n) in (~d).Names) {
            if ((~n).Name == name) {
                return n.Pos();
            }
        }
        break;
    }
    case TypeSpec.val d: {
        if ((~(~d).Name).Name == name) {
            return (~d).Name.Pos();
        }
        break;
    }
    case FuncDecl.val d: {
        if ((~(~d).Name).Name == name) {
            return (~d).Name.Pos();
        }
        break;
    }
    case LabeledStmt.val d: {
        if ((~(~d).Label).Name == name) {
            return (~d).Label.Pos();
        }
        break;
    }
    case AssignStmt.val d: {
        foreach (var (_, x) in (~d).Lhs) {
            {
                var (ident, isIdent) = x._<Ident.val>(ᐧ); if (isIdent && (~ident).Name == name) {
                    return ident.Pos();
                }
            }
        }
        break;
    }
    case Scope.val d: {
        break;
    }}
    // predeclared object - nothing to do for now
    return token.NoPos;
}

[GoType("num:nint")] partial struct ObjKind;

// The list of possible [Object] kinds.
public static readonly ObjKind Bad = /* iota */ 0;  // for error handling

public static readonly ObjKind Pkg = 1;  // package

public static readonly ObjKind Con = 2;  // constant

public static readonly ObjKind Typ = 3;  // type

public static readonly ObjKind Var = 4;  // variable

public static readonly ObjKind Fun = 5;  // function or method

public static readonly ObjKind Lbl = 6;  // label

internal static array<@string> objKindStrings = new runtime.SparseArray<@string>{
    [Bad] = "bad"u8,
    [Pkg] = "package"u8,
    [Con] = "const"u8,
    [Typ] = "type"u8,
    [Var] = "var"u8,
    [Fun] = "func"u8,
    [Lbl] = "label"u8
}.array();

public static @string String(this ObjKind kind) {
    return objKindStrings[kind];
}

} // end ast_package
