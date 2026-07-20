// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using ast = global::go.go.ast_package;
using token = global::go.go.token_package;
using static global::go.@internal.types.errors_package;
using constant = global::go.go.constant_package;
using errors = global::go.@internal.types.errors_package;
using global::go.go;

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

public static ΔType Underlying(this ж<Union> Ꮡu) {
    return new UnionжΔType(Ꮡu);
}

public static @string String(this ж<Union> Ꮡu) {
    return TypeString(new UnionжΔType(Ꮡu), default!);
}

[GoType("term")] partial struct ΔTerm;

// NewTerm returns a new union term.
public static ж<ΔTerm> NewTerm(bool tilde, ΔType typ) {
    return Ꮡ(new ΔTerm(new term(tilde, typ)));
}

[GoRecv] public static bool Tilde(this ref ΔTerm t) {
    return t.tilde;
}

[GoRecv] public static ΔType Type(this ref ΔTerm t) {
    return t.typ;
}

public static @string String(this ж<ΔTerm> Ꮡt) {
    ref var t = ref Ꮡt.Value;

    return (Ꮡ((term)(t))).String();
}

// ----------------------------------------------------------------------------
// Implementation

// Avoid excessive type-checking times due to quadratic termlist operations.
internal static readonly UntypedInt maxTermCount = 100;

// parseUnion parses uexpr as a union of expressions.
// The result is a Union type, or Typ[Invalid] for some errors.
internal static ΔType parseUnion(ж<Checker> Ꮡcheck, ast.Expr uexpr) {
    ref var check = ref Ꮡcheck.Value;

    var (blist, tlist) = flattenUnion(default!, uexpr);
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
                Ꮡcheck.errorf(new ast_Exprᴠpositioner(x), InvalidUnion, "cannot handle more than %d union terms (implementation limitation)"u8, maxTermCount);
                u = new BasicжΔType(Typ[Invalid]);
            }
        } else {
            terms = append(terms, term);
            u = new UnionжΔType(Ꮡ(new Union(terms)));
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
    var termsʗ1 = terms;
    var tlistʗ1 = tlist;

    var termsʗ3 = terms;
    var tlistʗ3 = tlist;

    var termsʗ5 = terms;
    var tlistʗ5 = tlist;

    var termsʗ7 = terms;
    var tlistʗ7 = tlist;
    check.later(() => {
        foreach (var (i, t) in termsʗ7) {
            if (!isValid((~t).typ)) {
                continue;
            }
            var uΔ1 = under((~t).typ);
            var (f, _) = uΔ1._<ж<Interface>>(ᐧ);
            if ((~t).tilde) {
                if (f != nil) {
                    Ꮡcheck.errorf(new ast_Exprᴠpositioner(tlistʗ7[i]), InvalidUnion, "invalid use of ~ (%s is an interface)"u8, (~t).typ);
                    continue;
                }
                if (!Identical(uΔ1, (~t).typ)) {
                    Ꮡcheck.errorf(new ast_Exprᴠpositioner(tlistʗ7[i]), InvalidUnion, "invalid use of ~ (underlying type of %s is %s)"u8, (~t).typ, uΔ1);
                    continue;
                }
            }
            if (f != nil) {
                var tset = f.typeSet();
                switch (ᐧ) {
                case {} when tset.NumMethods() is not 0: {
                    Ꮡcheck.errorf(new ast_Exprᴠpositioner(tlistʗ7[i]), InvalidUnion, "cannot use %s in union (%s contains methods)"u8, t, t);
                    break;
                }
                case {} when AreEqual((~t).typ, universeComparable.Type()): {
                    Ꮡcheck.error(new ast_Exprᴠpositioner(tlistʗ7[i]), InvalidUnion, "cannot use comparable in union"u8);
                    break;
                }
                case {} when (~tset).comparable: {
                    Ꮡcheck.errorf(new ast_Exprᴠpositioner(tlistʗ7[i]), InvalidUnion, "cannot use %s in union (%s embeds comparable)"u8, t, t);
                    break;
                }}

                continue;
            }
            {
                nint j = overlappingTerm(termsʗ7[..(int)(i)], t); if (j >= 0) {
                    Ꮡcheck.softErrorf(new ast_Exprᴠpositioner(tlistʗ7[i]), InvalidUnion, "overlapping terms %s and %s"u8, t, termsʗ7[j]);
                }
            }
        }
    }).describef(new ast_Exprᴠpositioner(uexpr), "check term validity %s"u8, uexpr);
    return u;
}

internal static ж<ΔTerm> parseTilde(ж<Checker> Ꮡcheck, ast.Expr tx) {
    ref var check = ref Ꮡcheck.Value;

    var x = tx;
    bool tilde = default!;
    {
        var (op, _) = x._<ж<ast.UnaryExpr>>(ᐧ); if (op != nil && (~op).Op == token.TILDE) {
            x = op.Value.X;
            tilde = true;
        }
    }
    var typ = Ꮡcheck.typ(x);
    // Embedding stand-alone type parameters is not permitted (go.dev/issue/47127).
    // We don't need this restriction anymore if we make the underlying type of a type
    // parameter its constraint interface: if we embed a lone type parameter, we will
    // simply use its underlying type (like we do for other named, embedded interfaces),
    // and since the underlying type is an interface the embedding is well defined.
    if (isTypeParam(typ)) {
        if (tilde){
            Ꮡcheck.errorf(new ast_Exprᴠpositioner(x), MisplacedTypeParam, "type in term %s cannot be a type parameter"u8, tx);
        } else {
            Ꮡcheck.error(new ast_Exprᴠpositioner(x), MisplacedTypeParam, "term cannot be a type parameter"u8);
        }
        typ = new BasicжΔType(Typ[Invalid]);
    }
    var term = NewTerm(tilde, typ);
    if (tilde) {
        check.recordTypeAndValue(tx, typexpr, new UnionжΔType(Ꮡ(new Union(new ж<ΔTerm>[]{term}.slice()))), default!);
    }
    return term;
}

// overlappingTerm reports the index of the term x in terms which is
// overlapping (not disjoint) from y. The result is < 0 if there is no
// such term. The type of term y must not be an interface, and terms
// with an interface type are ignored in the terms list.
internal static nint overlappingTerm(slice<ж<ΔTerm>> terms, ж<ΔTerm> Ꮡy) {
    ref var y = ref Ꮡy.DerefOrNil();

    assert(!IsInterface(y.typ));
    foreach (var (i, x) in terms) {
        if (IsInterface((~x).typ)) {
            continue;
        }
        // disjoint requires non-nil, non-top arguments,
        // and non-interface types as term types.
        if (debug) {
            if (x == nil || (~x).typ == default! || Ꮡy == nil || y.typ == default!) {
                throw panic("empty or top union term");
            }
        }
        if (!(Ꮡ((term)(~x))).disjoint(Ꮡ((term)(y)))) {
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
            blist = append(blist, (ast.Expr)(new ast.BinaryExprжExpr(o)));
            x = o.Value.Y;
        }
    }
    return (blist, append(tlist, x));
}

} // end types_package
