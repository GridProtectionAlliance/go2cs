// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using ast = go.ast_package;
using token = go.token_package;
using static @internal.types.errors_package;

partial class types_package {

// ----------------------------------------------------------------------------
// API

// An Interface represents an interface type.
[GoType] partial struct Interface {
    internal ж<Checker> check;  // for error reporting; nil once type set is computed
    internal slice<ж<Func>> methods; // ordered list of explicitly declared methods
    internal slice<ΔType> embeddeds; // ordered list of explicitly embedded elements
    internal ж<tokenꓸPos> embedPos; // positions of embedded elements; or nil (for error messages) - use pointer to save space
    internal bool @implicit;         // interface is wrapper for type set literal (non-interface T, ~T, or A|B)
    internal bool complete;         // indicates that obj, methods, and embeddeds are set and type set can be computed
    internal ж<_TypeSet> tset; // type set described by this interface, computed lazily
}

// typeSet returns the type set for interface t.
[GoRecv] internal static ж<_TypeSet> typeSet(this ref Interface t) {
    return computeInterfaceTypeSet(t.check, nopos, t);
}

// emptyInterface represents the empty (completed) interface
internal static Interface emptyInterface = new Interface(complete: true, tset: Ꮡ(topTypeSet));

// NewInterface returns a new interface for the given methods and embedded types.
// NewInterface takes ownership of the provided methods and may modify their types
// by setting missing receivers.
//
// Deprecated: Use NewInterfaceType instead which allows arbitrary embedded types.
public static ж<Interface> NewInterface(slice<ж<Func>> methods, slice<ж<Named>> embeddeds) {
    var tnames = new slice<ΔType>(len(embeddeds));
    foreach (var (i, t) in embeddeds) {
        tnames[i] = t;
    }
    return NewInterfaceType(methods, tnames);
}

// NewInterfaceType returns a new interface for the given methods and embedded
// types. NewInterfaceType takes ownership of the provided methods and may
// modify their types by setting missing receivers.
//
// To avoid race conditions, the interface's type set should be computed before
// concurrent use of the interface, by explicitly calling Complete.
public static ж<Interface> NewInterfaceType(slice<ж<Func>> methods, slice<ΔType> embeddeds) {
    if (len(methods) == 0 && len(embeddeds) == 0) {
        return Ꮡ(emptyInterface);
    }
    // set method receivers if necessary
    var typ = ((ж<Checker>)(default!)).val.newInterface();
    foreach (var (_, m) in methods) {
        {
            var sig = m.typ._<ΔSignature.val>(); if ((~sig).recv == nil) {
                sig.val.recv = NewVar(m.pos, m.pkg, ""u8, ~typ);
            }
        }
    }
    // sort for API stability
    sortMethods(methods);
    typ.val.methods = methods;
    typ.val.embeddeds = embeddeds;
    typ.val.complete = true;
    return typ;
}

// check may be nil
[GoRecv] internal static ж<Interface> newInterface(this ref Checker check) {
    var typ = Ꮡ(new Interface(check: check));
    if (check != nil) {
        check.needsCleanup(~typ);
    }
    return typ;
}

// MarkImplicit marks the interface t as implicit, meaning this interface
// corresponds to a constraint literal such as ~T or A|B without explicit
// interface embedding. MarkImplicit should be called before any concurrent use
// of implicit interfaces.
[GoRecv] public static void MarkImplicit(this ref Interface t) {
    t.@implicit = true;
}

// NumExplicitMethods returns the number of explicitly declared methods of interface t.
[GoRecv] public static nint NumExplicitMethods(this ref Interface t) {
    return len(t.methods);
}

// ExplicitMethod returns the i'th explicitly declared method of interface t for 0 <= i < t.NumExplicitMethods().
// The methods are ordered by their unique [Id].
[GoRecv] public static ж<Func> ExplicitMethod(this ref Interface t, nint i) {
    return t.methods[i];
}

// NumEmbeddeds returns the number of embedded types in interface t.
[GoRecv] public static nint NumEmbeddeds(this ref Interface t) {
    return len(t.embeddeds);
}

// Embedded returns the i'th embedded defined (*[Named]) type of interface t for 0 <= i < t.NumEmbeddeds().
// The result is nil if the i'th embedded type is not a defined type.
//
// Deprecated: Use [Interface.EmbeddedType] which is not restricted to defined (*[Named]) types.
[GoRecv] public static ж<Named> Embedded(this ref Interface t, nint i) {
    return asNamed(t.embeddeds[i]);
}

// EmbeddedType returns the i'th embedded type of interface t for 0 <= i < t.NumEmbeddeds().
[GoRecv] public static ΔType EmbeddedType(this ref Interface t, nint i) {
    return t.embeddeds[i];
}

// NumMethods returns the total number of methods of interface t.
[GoRecv] public static nint NumMethods(this ref Interface t) {
    return t.typeSet().NumMethods();
}

// Method returns the i'th method of interface t for 0 <= i < t.NumMethods().
// The methods are ordered by their unique Id.
[GoRecv] public static ж<Func> Method(this ref Interface t, nint i) {
    return t.typeSet().Method(i);
}

// Empty reports whether t is the empty interface.
[GoRecv] public static bool Empty(this ref Interface t) {
    return t.typeSet().IsAll();
}

// IsComparable reports whether each type in interface t's type set is comparable.
[GoRecv] public static bool IsComparable(this ref Interface t) {
    return t.typeSet().IsComparable(default!);
}

// IsMethodSet reports whether the interface t is fully described by its method
// set.
[GoRecv] public static bool IsMethodSet(this ref Interface t) {
    return t.typeSet().IsMethodSet();
}

// IsImplicit reports whether the interface t is a wrapper for a type set literal.
[GoRecv] public static bool IsImplicit(this ref Interface t) {
    return t.@implicit;
}

// Complete computes the interface's type set. It must be called by users of
// [NewInterfaceType] and [NewInterface] after the interface's embedded types are
// fully defined and before using the interface type in any way other than to
// form other types. The interface must not contain duplicate methods or a
// panic occurs. Complete returns the receiver.
//
// Interface types that have been completed are safe for concurrent use.
[GoRecv("capture")] public static ж<Interface> Complete(this ref Interface t) {
    if (!t.complete) {
        t.complete = true;
    }
    t.typeSet();
    // checks if t.tset is already set
    return CompleteꓸᏑt;
}

[GoRecv("capture")] public static ΔType Underlying(this ref Interface t) {
    return ~t;
}

[GoRecv] public static @string String(this ref Interface t) {
    return TypeString(~t, default!);
}

// ----------------------------------------------------------------------------
// Implementation
[GoRecv] internal static void cleanup(this ref Interface t) {
    t.typeSet();
    // any interface that escapes type checking must be safe for concurrent use
    t.check = default!;
    t.embedPos = default!;
}

[GoRecv] public static void interfaceType(this ref Checker check, ж<Interface> Ꮡityp, ж<ast.InterfaceType> Ꮡiface, ж<TypeName> Ꮡdef) {
    ref var ityp = ref Ꮡityp.val;
    ref var iface = ref Ꮡiface.val;
    ref var def = ref Ꮡdef.val;

    var addEmbedded = (tokenꓸPos pos, ΔType typ) => {
        ityp.embeddeds = append(ityp.embeddeds, typ);
        if (ityp.embedPos == nil) {
            ityp.embedPos = @new<slice<tokenꓸPos>>();
        }
        ityp.embedPos = append(ityp.embedPos, pos);
    };
    foreach (var (_, f) in iface.Methods.List) {
        if (len((~f).Names) == 0) {
            addEmbedded((~f).Type.Pos(), parseUnion(check, (~f).Type));
            continue;
        }
        // f.Name != nil
        // We have a method with name f.Names[0].
        var name = (~f).Names[0];
        if ((~name).Name == "_"u8) {
            check.error(~name, BlankIfaceMethod, "methods must have a unique non-blank name"u8);
            continue;
        }
        // ignore
        var typ = check.typ((~f).Type);
        var (sig, _) = typ._<ΔSignature.val>(ᐧ);
        if (sig == nil) {
            if (isValid(typ)) {
                check.errorf((~f).Type, InvalidSyntaxTree, "%s is not a method signature"u8, typ);
            }
            continue;
        }
        // ignore
        // The go/parser doesn't accept method type parameters but an ast.FuncType may have them.
        if ((~sig).tparams != nil) {
            positioner at = (~f).Type;
            {
                var (ftyp, _) = (~f).Type._<ж<ast.FuncType>>(ᐧ); if (ftyp != nil && (~ftyp).TypeParams != nil) {
                    at = ~ftyp.val.TypeParams;
                }
            }
            check.error(at, InvalidSyntaxTree, "methods cannot have type parameters"u8);
        }
        // use named receiver type if available (for better error messages)
        ΔType recvTyp = ityp;
        if (def != nil) {
            {
                var named = asNamed(def.typ); if (named != nil) {
                    recvTyp = ~named;
                }
            }
        }
        sig.val.recv = NewVar(name.Pos(), check.pkg, ""u8, recvTyp);
        var m = NewFunc(name.Pos(), check.pkg, (~name).Name, sig);
        check.recordDef(name, ~m);
        ityp.methods = append(ityp.methods, m);
    }
    // All methods and embedded elements for this interface are collected;
    // i.e., this interface may be used in a type set computation.
    ityp.complete = true;
    if (len(ityp.methods) == 0 && len(ityp.embeddeds) == 0) {
        // empty interface
        ityp.tset = Ꮡ(topTypeSet);
        return;
    }
    // sort for API stability
    sortMethods(ityp.methods);
    // (don't sort embeddeds: they must correspond to *embedPos entries)
    // Compute type set as soon as possible to report any errors.
    // Subsequent uses of type sets will use this computed type
    // set and won't need to pass in a *Checker.
    check.later(() => {
        computeInterfaceTypeSet(check, iface.Pos(), Ꮡityp);
    }).describef(~iface, "compute type set for %s"u8, ityp);
}

} // end types_package
