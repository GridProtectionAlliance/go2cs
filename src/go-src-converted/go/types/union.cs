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

// A Union represents a union of terms embedded in an interface.
[GoType] partial struct Union {
    internal slice<ж<ΔTerm>> terms; // list of syntactical terms (not a canonicalized termlist)
}

// NewUnion returns a new [Union] type with the given terms.
// It is an error to create an empty union; they are syntactically not possible.
public static ж<Union> NewUnion(slice<ж<ΔTerm>> terms) {
    if (len(terms) == 0) {
        throw panic("empty union");
    }
    return Ꮡ(new Union(terms));
}

[GoRecv] public static nint Len(this ref Union u) {
    return len(u.terms);
}

[GoRecv] public static ж<ΔTerm> Term(this ref Union u, nint i) {
    return u.terms[i];
}

[GoRecv("capture")] public static ΔType Underlying(this ref Union u) {
    return ~u;
}

[GoRecv] public static @string String(this ref Union u) {
    return TypeString(~u, default!);
}

[GoType("struct{tilde bool; typ go.types.Type}")] partial struct ΔTerm;

// NewTerm returns a new union term.
public static ж<ΔTerm> NewTerm(bool tilde, ΔType typ) {
    return Ꮡ(new ΔTerm(tilde, typ));
}

[GoRecv] public static bool Tilde(this ref ΔTerm t) {
    return t.tilde;
}

[GoRecv] public static ΔType Type(this ref ΔTerm t) {
    return t.typ;
}

[GoRecv] public static @string String(this ref ΔTerm t) {
    return (((ж<term>)(t?.val ?? default!))).val.String();
}

// ----------------------------------------------------------------------------
// Implementation

// Avoid excessive type-checking times due to quadratic termlist operations.
internal static readonly UntypedInt maxTermCount = 100;

// parseUnion parses uexpr as a union of expressions.
// The result is a Union type, or Typ[Invalid] for some errors.
internal static ΔType parseUnion(ж<Checker> Ꮡcheck, ast.Expr uexpr) {
    ref var check = ref Ꮡcheck.val;

    (blist, tlist) = flattenUnion(default!, uexpr);
    assert(len(blist) == len(tlist) - 1);
    slice<ж<ΔTerm>> terms = default!;
    ΔType u = default!;
    foreach (var (i, x) in tlist) {
        var term = parseTilde(Ꮡcheck, x);
        if (len(tlist) == 1 && !(~term).tilde) {
            // Single type. Ok to return early because all relevant
            // checks have been performed in parseTilde (no need to
            // run through term validity check below).
            return (~term).typ;
        }
        // typ already recorded through check.typ in parseTilde
        if (len(terms) >= maxTermCount){
            if (isValid(u)) {
                check.errorf(x, InvalidUnion, "cannot handle more than %d union terms (implementation limitation)"u8, maxTermCount);
                u = ~Typ[Invalid];
            }
        } else {
            terms = append(terms, term);
            Ꮡu = new Union(terms); u = ref Ꮡu.val;
        }
        if (i > 0) {
            check.recordTypeAndValue(blist[i - 1], typexpr, u, default!);
        }
    }
    if (!isValid(u)) {
        return u;
    }
    // Check validity of terms.
    // Do this check later because it requires types to be set up.
    // Note: This is a quadratic algorithm, but unions tend to be short.
    check.later(
    var termsʗ11 = terms;
    var tlistʗ11 = tlist;
    () => {
        foreach (var (i, t) in termsʗ11) {
            if (!isValid((~t).typ)) {
                continue;
            }
            var u = under((~t).typ);
            var (f, _) = u._<Interface.val>(ᐧ);
            if ((~t).tilde) {
                if (f != nil) {
                    check.errorf(tlistʗ11[i], InvalidUnion, "invalid use of ~ (%s is an interface)"u8, (~t).typ);
                    continue;
                }
                if (!Identical(u, (~t).typ)) {
                    check.errorf(tlistʗ11[i], InvalidUnion, "invalid use of ~ (underlying type of %s is %s)"u8, (~t).typ, u);
                    continue;
                }
            }
            if (f != nil) {
                var tset = f.typeSet();
                switch (ᐧ) {
                case {} when tset.NumMethods() is != 0: {
                    check.errorf(tlistʗ11[i], InvalidUnion, "cannot use %s in union (%s contains methods)"u8, t, t);
                    break;
                }
                case {} when AreEqual((~t).typ, universeComparable.Type()): {
                    check.error(tlistʗ11[i], InvalidUnion, "cannot use comparable in union"u8);
                    break;
                }
                case {} when (~tset).comparable: {
                    check.errorf(tlistʗ11[i], InvalidUnion, "cannot use %s in union (%s embeds comparable)"u8, t, t);
                    break;
                }}

                continue;
            }
            {
                nint j = overlappingTerm(termsʗ11[..(int)(i)], t); if (j >= 0) {
                    check.softErrorf(tlistʗ11[i], InvalidUnion, "overlapping terms %s and %s"u8, t, termsʗ11[j]);
                }
            }
        }
    }).describef(uexpr, "check term validity %s"u8, uexpr);
    return u;
}

internal static ж<ΔTerm> parseTilde(ж<Checker> Ꮡcheck, ast.Expr tx) {
    ref var check = ref Ꮡcheck.val;

    var x = tx;
    bool tilde = default!;
    {
        var (op, _) = x._<ж<ast.UnaryExpr>>(ᐧ); if (op != nil && (~op).Op == token.TILDE) {
            x = op.val.X;
            tilde = true;
        }
    }
    var typ = check.typ(x);
    // Embedding stand-alone type parameters is not permitted (go.dev/issue/47127).
    // We don't need this restriction anymore if we make the underlying type of a type
    // parameter its constraint interface: if we embed a lone type parameter, we will
    // simply use its underlying type (like we do for other named, embedded interfaces),
    // and since the underlying type is an interface the embedding is well defined.
    if (isTypeParam(typ)) {
        if (tilde){
            check.errorf(x, MisplacedTypeParam, "type in term %s cannot be a type parameter"u8, tx);
        } else {
            check.error(x, MisplacedTypeParam, "term cannot be a type parameter"u8);
        }
        typ = ~Typ[Invalid];
    }
    var term = NewTerm(tilde, typ);
    if (tilde) {
        check.recordTypeAndValue(tx, typexpr, new Union(new ж<ΔTerm>[]{term}.slice()), default!);
    }
    return term;
}

// overlappingTerm reports the index of the term x in terms which is
// overlapping (not disjoint) from y. The result is < 0 if there is no
// such term. The type of term y must not be an interface, and terms
// with an interface type are ignored in the terms list.
internal static nint overlappingTerm(slice<ж<ΔTerm>> terms, ж<ΔTerm> Ꮡy) {
    ref var y = ref Ꮡy.val;

    assert(!IsInterface(y.typ));
    foreach (var (i, x) in terms) {
        if (IsInterface((~x).typ)) {
            continue;
        }
        // disjoint requires non-nil, non-top arguments,
        // and non-interface types as term types.
        if (debug) {
            if (x == nil || (~x).typ == default! || y == nil || y.typ == default!) {
                throw panic("empty or top union term");
            }
        }
        if (!(((ж<term>)(x?.val ?? default!))).val.disjoint(((ж<term>)(y?.val ?? default!)))) {
            return i;
        }
    }
    return -1;
}

// flattenUnion walks a union type expression of the form A | B | C | ...,
// extracting both the binary exprs (blist) and leaf types (tlist).
internal static (slice<ast.Expr> blist, slice<ast.Expr> tlist) flattenUnion(slice<ast.Expr> list, ast.Expr x) {
    slice<ast.Expr> blist = default!;
    slice<ast.Expr> tlist = default!;

    {
        var (o, _) = x._<ж<ast.BinaryExpr>>(ᐧ); if (o != nil && (~o).Op == token.OR) {
            (blist, tlist) = flattenUnion(list, (~o).X);
            blist = append(blist, ~o);
            x = o.val.Y;
        }
    }
    return (blist, append(tlist, x));
}

} // end types_package
