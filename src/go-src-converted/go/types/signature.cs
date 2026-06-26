// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using fmt = fmt_package;
using ast = go.ast_package;
using token = go.token_package;
using static @internal.types.errors_package;

partial class types_package {

// ----------------------------------------------------------------------------
// API

// A Signature represents a (non-builtin) function or method type.
// The receiver is ignored when comparing signatures for identity.
[GoType] partial struct ΔSignature {
    // We need to keep the scope in Signature (rather than passing it around
    // and store it in the Func Object) because when type-checking a function
    // literal we call the general type checker which returns a general Type.
    // We then unpack the *Signature and use the scope for the literal body.
    internal ж<TypeParamList> rparams; // receiver type parameters from left to right, or nil
    internal ж<TypeParamList> tparams; // type parameters from left to right, or nil
    internal ж<ΔScope> scope;    // function scope for package-local and non-instantiated signatures; nil otherwise
    internal ж<Var> recv;        // nil if not a method
    internal ж<Tuple> @params;      // (incoming) parameters from left to right; or nil
    internal ж<Tuple> results;      // (outgoing) results from left to right; or nil
    internal bool variadic;           // true if the last parameter's type is of the form ...T (or string, for append built-in only)
}

// NewSignature returns a new function type for the given receiver, parameters,
// and results, either of which may be nil. If variadic is set, the function
// is variadic, it must have at least one parameter, and the last parameter
// must be of unnamed slice type.
//
// Deprecated: Use [NewSignatureType] instead which allows for type parameters.
public static ж<ΔSignature> NewSignature(ж<Var> Ꮡrecv, ж<Tuple> Ꮡparams, ж<Tuple> Ꮡresults, bool variadic) {
    ref var recv = ref Ꮡrecv.val;
    ref var @params = ref Ꮡparams.val;
    ref var results = ref Ꮡresults.val;

    return NewSignatureType(Ꮡrecv, default!, default!, Ꮡparams, Ꮡresults, variadic);
}

// NewSignatureType creates a new function type for the given receiver,
// receiver type parameters, type parameters, parameters, and results. If
// variadic is set, params must hold at least one parameter and the last
// parameter's core type must be of unnamed slice or bytestring type.
// If recv is non-nil, typeParams must be empty. If recvTypeParams is
// non-empty, recv must be non-nil.
public static ж<ΔSignature> NewSignatureType(ж<Var> Ꮡrecv, slice<ж<TypeParam>> recvTypeParams, slice<ж<TypeParam>> typeParams, ж<Tuple> Ꮡparams, ж<Tuple> Ꮡresults, bool variadic) {
    ref var recv = ref Ꮡrecv.val;
    ref var @params = ref Ꮡparams.val;
    ref var results = ref Ꮡresults.val;

    if (variadic) {
        nint n = @params.Len();
        if (n == 0) {
            throw panic("variadic function must have at least one parameter");
        }
        var core = coreString(@params.At(n - 1).typ);
        {
            var (_, ok) = core._<Slice.val>(ᐧ); if (!ok && !isString(core)) {
                throw panic(fmt.Sprintf("got %s, want variadic parameter with unnamed slice type or string as core type"u8, core.String()));
            }
        }
    }
    var sig = Ꮡ(new ΔSignature(recv: recv, @params: @params, results: results, variadic: variadic));
    if (len(recvTypeParams) != 0) {
        if (recv == nil) {
            throw panic("function with receiver type parameters must have a receiver");
        }
        sig.val.rparams = bindTParams(recvTypeParams);
    }
    if (len(typeParams) != 0) {
        if (recv != nil) {
            throw panic("function with type parameters cannot have a receiver");
        }
        sig.val.tparams = bindTParams(typeParams);
    }
    return sig;
}

// Recv returns the receiver of signature s (if a method), or nil if a
// function. It is ignored when comparing signatures for identity.
//
// For an abstract method, Recv returns the enclosing interface either
// as a *[Named] or an *[Interface]. Due to embedding, an interface may
// contain methods whose receiver type is a different interface.
[GoRecv] public static ж<Var> Recv(this ref ΔSignature s) {
    return s.recv;
}

// TypeParams returns the type parameters of signature s, or nil.
[GoRecv] public static ж<TypeParamList> TypeParams(this ref ΔSignature s) {
    return s.tparams;
}

// RecvTypeParams returns the receiver type parameters of signature s, or nil.
[GoRecv] public static ж<TypeParamList> RecvTypeParams(this ref ΔSignature s) {
    return s.rparams;
}

// Params returns the parameters of signature s, or nil.
[GoRecv] public static ж<Tuple> Params(this ref ΔSignature s) {
    return s.@params;
}

// Results returns the results of signature s, or nil.
[GoRecv] public static ж<Tuple> Results(this ref ΔSignature s) {
    return s.results;
}

// Variadic reports whether the signature s is variadic.
[GoRecv] public static bool Variadic(this ref ΔSignature s) {
    return s.variadic;
}

[GoRecv("capture")] public static ΔType Underlying(this ref ΔSignature t) {
    return ~t;
}

[GoRecv] public static @string String(this ref ΔSignature t) {
    return TypeString(~t, default!);
}

// ----------------------------------------------------------------------------
// Implementation

// funcType type-checks a function or method type.
[GoRecv] public static void funcType(this ref Checker check, ж<ΔSignature> Ꮡsig, ж<ast.FieldList> ᏑrecvPar, ж<ast.FuncType> Ꮡftyp) => func((defer, _) => {
    ref var sig = ref Ꮡsig.val;
    ref var recvPar = ref ᏑrecvPar.val;
    ref var ftyp = ref Ꮡftyp.val;

    check.openScope(~ftyp, "function"u8);
    check.scope.isFunc = true;
    check.recordScope(~ftyp, check.scope);
    sig.scope = check.scope;
    defer(check.closeScope);
    if (recvPar != nil && len(recvPar.List) > 0) {
        // collect generic receiver type parameters, if any
        // - a receiver type parameter is like any other type parameter, except that it is declared implicitly
        // - the receiver specification acts as local declaration for its type parameters, which may be blank
        var (_, rname, rparams) = check.unpackRecv(recvPar.List[0].Type, true);
        if (len(rparams) > 0) {
            // The scope of the type parameter T in "func (r T[T]) f()"
            // starts after f, not at "r"; see #52038.
            tokenꓸPos scopePosΔ1 = ftyp.Params.Pos();
            var tparams = check.declareTypeParams(default!, rparams, scopePosΔ1);
            sig.rparams = bindTParams(tparams);
            // Blank identifiers don't get declared, so naive type-checking of the
            // receiver type expression would fail in Checker.collectParams below,
            // when Checker.ident cannot resolve the _ to a type.
            //
            // Checker.recvTParamMap maps these blank identifiers to their type parameter
            // types, so that they may be resolved in Checker.ident when they fail
            // lookup in the scope.
            foreach (var (i, p) in rparams) {
                if ((~p).Name == "_"u8) {
                    if (check.recvTParamMap == default!) {
                        check.recvTParamMap = new ast.Ident>*TypeParam();
                    }
                    check.recvTParamMap[p] = tparams[i];
                }
            }
            // determine receiver type to get its type parameters
            // and the respective type parameter bounds
            slice<ж<TypeParam>> recvTParams = default!;
            if (rname != nil) {
                // recv should be a Named type (otherwise an error is reported elsewhere)
                // Also: Don't report an error via genericType since it will be reported
                //       again when we type-check the signature.
                // TODO(gri) maybe the receiver should be marked as invalid instead?
                {
                    var recvΔ1 = asNamed(check.genericType(~rname, nil)); if (recvΔ1 != nil) {
                        recvTParams = recvΔ1.TypeParams().list();
                    }
                }
            }
            // provide type parameter bounds
            if (len(tparams) == len(recvTParams)){
                var smap = makeRenameMap(recvTParams, tparams);
                foreach (var (i, tpar) in tparams) {
                    var recvTPar = recvTParams[i];
                    check.mono.recordCanon(tpar, recvTPar);
                    // recvTPar.bound is (possibly) parameterized in the context of the
                    // receiver type declaration. Substitute parameters for the current
                    // context.
                    tpar.val.bound = check.subst((~tpar).obj.pos, (~recvTPar).bound, smap, nil, check.context());
                }
            } else 
            if (len(tparams) < len(recvTParams)) {
                // Reporting an error here is a stop-gap measure to avoid crashes in the
                // compiler when a type parameter/argument cannot be inferred later. It
                // may lead to follow-on errors (see issues go.dev/issue/51339, go.dev/issue/51343).
                // TODO(gri) find a better solution
                @string got = measure(len(tparams), "type parameter"u8);
                check.errorf(~recvPar, BadRecv, "got %s, but receiver base type declares %d"u8, got, len(recvTParams));
            }
        }
    }
    if (ftyp.TypeParams != nil) {
        check.collectTypeParams(Ꮡ(sig.tparams), ftyp.TypeParams);
        // Always type-check method type parameters but complain that they are not allowed.
        // (A separate check is needed when type-checking interface method signatures because
        // they don't have a receiver specification.)
        if (recvPar != nil) {
            check.error(~ftyp.TypeParams, InvalidMethodTypeParams, "methods cannot have type parameters"u8);
        }
    }
    // Use a temporary scope for all parameter declarations and then
    // squash that scope into the parent scope (and report any
    // redeclarations at that time).
    //
    // TODO(adonovan): now that each declaration has the correct
    // scopePos, there should be no need for scope squashing.
    // Audit to ensure all lookups honor scopePos and simplify.
    var scope = NewScope(check.scope, nopos, nopos, "function body (temp. scope)"u8);
    tokenꓸPos scopePos = ftyp.End();
    // all parameters' scopes start after the signature
    var (recvList, _) = check.collectParams(scope, ᏑrecvPar, false, scopePos);
    var (@params, variadic) = check.collectParams(scope, ftyp.Params, true, scopePos);
    var (results, _) = check.collectParams(scope, ftyp.Results, false, scopePos);
    scope.squash((Object obj, Object alt) => {
        var err = check.newError(DuplicateDecl);
        err.addf(obj, "%s redeclared in this block"u8, obj.Name());
        err.addAltDecl(alt);
        err.report();
    });
    if (recvPar != nil) {
        // recv parameter list present (may be empty)
        // spec: "The receiver is specified via an extra parameter section preceding the
        // method name. That parameter section must declare a single parameter, the receiver."
        ж<Var> recv = default!;
        switch (len(recvList)) {
        case 0: {
            recv = NewParam(nopos, // error reported by resolver
 nil, ""u8, ~Typ[Invalid]);
            break;
        }
        default: {
            check.error(~recvList[len(recvList) - 1], // ignore recv below
 // more than one receiver
 InvalidRecv, "method has multiple receivers"u8);
            break;
        }
        case 1: {
            recv = recvList[0];
            break;
        }}

        // continue with first receiver
        sig.recv = recv;
        // Delay validation of receiver type as it may cause premature expansion
        // of types the receiver type is dependent on (see issues go.dev/issue/51232, go.dev/issue/51233).
        check.later(
        var recvʗ11 = recv;
        () => {
            var (rtyp, _) = deref(recvʗ11.typ);
            var atyp = Unalias(rtyp);
            if (!isValid(atyp)) {
                return;
            }
            switch (atyp.type()) {
            case Named.val T: {
                if (T.TypeArgs() != nil && sig.RecvTypeParams() == nil) {
                    check.errorf(~recvʗ11, InvalidRecv, "cannot define new methods on instantiated type %s"u8, rtyp);
                    break;
                }
                if ((~T).obj.pkg != check.pkg) {
                    check.errorf(~recvʗ11, InvalidRecv, "cannot define new methods on non-local type %s"u8, rtyp);
                    break;
                }
                @string cause = default!;
                switch (T.under().type()) {
                case Basic.val u: {
                    if ((~u).kind == UnsafePointer) {
                        cause = "unsafe.Pointer"u8;
                    }
                    break;
                }
                case Pointer.val u: {
                    cause = "pointer or interface type"u8;
                    break;
                }
                case Interface.val u: {
                    cause = "pointer or interface type"u8;
                    break;
                }
                case TypeParam.val u: {
                    throw panic("unreachable");
                    break;
                }}
                if (cause != ""u8) {
                    check.errorf(~recvʗ11, InvalidRecv, "invalid receiver type %s (%s)"u8, rtyp, cause);
                }
                break;
            }
            case Basic.val T: {
                check.errorf(~recvʗ11, InvalidRecv, "cannot define new methods on non-local type %s"u8, rtyp);
                break;
            }
            default: {
                var T = atyp.type();
                check.errorf(~recvʗ11, InvalidRecv, "invalid receiver type %s"u8, recvʗ11.typ);
                break;
            }}
        }).describef(~recv, "validate receiver %s"u8, recv);
    }
    sig.@params = NewTuple(Ꮡparams.ꓸꓸꓸ);
    sig.results = NewTuple(Ꮡresults.ꓸꓸꓸ);
    sig.variadic = variadic;
});

// collectParams declares the parameters of list in scope and returns the corresponding
// variable list.
[GoRecv] public static (slice<ж<Var>> @params, bool variadic) collectParams(this ref Checker check, ж<ΔScope> Ꮡscope, ж<ast.FieldList> Ꮡlist, bool variadicOk, tokenꓸPos scopePos) {
    slice<ж<Var>> @params = default!;
    bool variadic = default!;

    ref var scope = ref Ꮡscope.val;
    ref var list = ref Ꮡlist.val;
    if (list == nil) {
        return (@params, variadic);
    }
    bool named = default!;
    bool anonymous = default!;
    foreach (var (i, field) in list.List) {
        var ftype = field.val.Type;
        {
            var (t, _) = ftype._<ж<ast.Ellipsis>>(ᐧ); if (t != nil) {
                ftype = t.val.Elt;
                if (variadicOk && i == len(list.List) - 1 && len((~field).Names) <= 1){
                    variadic = true;
                } else {
                    check.softErrorf(~t, MisplacedDotDotDot, "can only use ... with final parameter in list"u8);
                }
            }
        }
        // ignore ... and continue
        var typ = check.varType(ftype);
        // The parser ensures that f.Tag is nil and we don't
        // care if a constructed AST contains a non-nil tag.
        if (len((~field).Names) > 0){
            // named parameter
            foreach (var (_, name) in (~field).Names) {
                if ((~name).Name == ""u8) {
                    check.error(~name, InvalidSyntaxTree, "anonymous parameter"u8);
                }
                // ok to continue
                var par = NewParam(name.Pos(), check.pkg, (~name).Name, typ);
                check.declare(Ꮡscope, name, ~par, scopePos);
                @params = append(@params, par);
            }
            named = true;
        } else {
            // anonymous parameter
            var par = NewParam(ftype.Pos(), check.pkg, ""u8, typ);
            check.recordImplicit(~field, ~par);
            @params = append(@params, par);
            anonymous = true;
        }
    }
    if (named && anonymous) {
        check.error(~list, InvalidSyntaxTree, "list contains both named and anonymous parameters"u8);
    }
    // ok to continue
    // For a variadic function, change the last parameter's type from T to []T.
    // Since we type-checked T rather than ...T, we also need to retro-actively
    // record the type for ...T.
    if (variadic) {
        var last = @params[len(@params) - 1];
        last.typ = Ꮡ(new Slice(elem: last.typ));
        check.recordTypeAndValue(list.List[len(list.List) - 1].Type, typexpr, last.typ, default!);
    }
    return (@params, variadic);
}

} // end types_package
