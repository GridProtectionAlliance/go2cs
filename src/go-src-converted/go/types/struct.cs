// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using ast = go.ast_package;
using token = go.token_package;
using static @internal.types.errors_package;
using strconv = strconv_package;

partial class types_package {

// ----------------------------------------------------------------------------
// API

// A Struct represents a struct type.
[GoType] partial struct Struct {
    internal slice<ж<Var>> fields; // fields != nil indicates the struct is set up (possibly with len(fields) == 0)
    internal slice<@string> tags; // field tags; nil if there are no tags
}

// NewStruct returns a new struct with the given fields and corresponding field tags.
// If a field with index i has a tag, tags[i] must be that tag, but len(tags) may be
// only as long as required to hold the tag with the largest index i. Consequently,
// if no field has a tag, tags may be nil.
public static ж<Struct> NewStruct(slice<ж<Var>> fields, slice<@string> tags) {
    objset fset = default!;
    foreach (var (_, f) in fields) {
        if (f.name != "_"u8 && fset.insert(~f) != default!) {
            throw panic("multiple fields with the same name");
        }
    }
    if (len(tags) > len(fields)) {
        throw panic("more tags than fields");
    }
    var s = Ꮡ(new Struct(fields: fields, tags: tags));
    s.markComplete();
    return s;
}

// NumFields returns the number of fields in the struct (including blank and embedded fields).
[GoRecv] public static nint NumFields(this ref Struct s) {
    return len(s.fields);
}

// Field returns the i'th field for 0 <= i < NumFields().
[GoRecv] public static ж<Var> Field(this ref Struct s, nint i) {
    return s.fields[i];
}

// Tag returns the i'th field tag for 0 <= i < NumFields().
[GoRecv] public static @string Tag(this ref Struct s, nint i) {
    if (i < len(s.tags)) {
        return s.tags[i];
    }
    return ""u8;
}

[GoRecv("capture")] public static ΔType Underlying(this ref Struct t) {
    return ~t;
}

[GoRecv] public static @string String(this ref Struct t) {
    return TypeString(~t, default!);
}

// ----------------------------------------------------------------------------
// Implementation
[GoRecv] internal static void markComplete(this ref Struct s) {
    if (s.fields == default!) {
        s.fields = new slice<ж<Var>>(0);
    }
}

[GoRecv] public static void structType(this ref Checker check, ж<Struct> Ꮡstyp, ж<ast.StructType> Ꮡe) {
    ref var styp = ref Ꮡstyp.val;
    ref var e = ref Ꮡe.val;

    var list = e.Fields;
    if (list == nil) {
        styp.markComplete();
        return;
    }
    // struct fields and tags
    slice<ж<Var>> fields = default!;
    slice<@string> tags = default!;
    // for double-declaration checks
    objset fset = default!;
    // current field typ and tag
    ΔType typ = default!;
    @string tag = default!;
    var add = 
    var fieldsʗ1 = fields;
    var fsetʗ1 = fset;
    var tagsʗ1 = tags;
    var typʗ1 = typ;
    (ж<ast.Ident> ident, bool embedded) => {
        if (tag != ""u8 && tagsʗ1 == default!) {
            tagsʗ1 = new slice<@string>(len(fieldsʗ1));
        }
        if (tagsʗ1 != default!) {
            tagsʗ1 = append(tagsʗ1, tag);
        }
        tokenꓸPos pos = ident.Pos();
        @string name = ident.val.Name;
        var fld = NewField(pos, check.pkg, name, typʗ1, embedded);
        // spec: "Within a struct, non-blank field names must be unique."
        if (name == "_"u8 || check.declareInSet(Ꮡ(fsetʗ1), pos, ~fld)) {
            fieldsʗ1 = append(fieldsʗ1, fld);
            check.recordDef(ident, ~fld);
        }
    };
    // addInvalid adds an embedded field of invalid type to the struct for
    // fields with errors; this keeps the number of struct fields in sync
    // with the source as long as the fields are _ or have different names
    // (go.dev/issue/25627).
    var addInvalid = 
    var Typʗ1 = Typ;
    var addʗ1 = add;
    var typʗ2 = typ;
    (ж<ast.Ident> ident) => {
        typʗ2 = ~Typʗ1[Invalid];
        tag = ""u8;
        addʗ1(ident, true);
    };
    foreach (var (_, f) in (~list).List) {
        typ = check.varType((~f).Type);
        tag = check.tag((~f).Tag);
        if (len((~f).Names) > 0){
            // named fields
            foreach (var (_, name) in (~f).Names) {
                add(name, false);
            }
        } else {
            // embedded field
            // spec: "An embedded type must be specified as a type name T or as a
            // pointer to a non-interface type name *T, and T itself may not be a
            // pointer type."
            tokenꓸPos pos = (~f).Type.Pos();
            // position of type, for errors
            var name = embeddedFieldIdent((~f).Type);
            if (name == nil) {
                check.errorf((~f).Type, InvalidSyntaxTree, "embedded field type %s has no name"u8, (~f).Type);
                name = ast.NewIdent("_"u8);
                name.val.NamePos = pos;
                addInvalid(name);
                continue;
            }
            add(name, true);
            // struct{p.T} field has position of T
            // Because we have a name, typ must be of the form T or *T, where T is the name
            // of a (named or alias) type, and t (= deref(typ)) must be the type of T.
            // We must delay this check to the end because we don't want to instantiate
            // (via under(t)) a possibly incomplete type.
            // for use in the closure below
            var embeddedTyp = typ;
            var embeddedPos = f.val.Type;
            check.later(
            var embeddedPosʗ11 = embeddedPos;
            var embeddedTypʗ11 = embeddedTyp;
            () => {
                var (t, isPtr) = deref(embeddedTypʗ11);
                switch (under(t).type()) {
                case Basic.val u: {
                    if (!isValid(t)) {
                        return;
                    }
                    if ((~u).kind == UnsafePointer) {
                        check.error(embeddedPosʗ11, InvalidPtrEmbed, "embedded field type cannot be unsafe.Pointer"u8);
                    }
                    break;
                }
                case Pointer.val u: {
                    check.error(embeddedPosʗ11, InvalidPtrEmbed, "embedded field type cannot be a pointer"u8);
                    break;
                }
                case Interface.val u: {
                    if (isTypeParam(t)) {
                        check.error(embeddedPosʗ11, MisplacedTypeParam, "embedded field type cannot be a (pointer to a) type parameter"u8);
                        break;
                    }
                    if (isPtr) {
                        check.error(embeddedPosʗ11, InvalidPtrEmbed, "embedded field type cannot be a pointer to an interface"u8);
                    }
                    break;
                }}
            }).describef(embeddedPos, "check embedded type %s"u8, embeddedTyp);
        }
    }
    styp.fields = fields;
    styp.tags = tags;
    styp.markComplete();
}

internal static ж<ast.Ident> embeddedFieldIdent(ast.Expr e) {
    switch (e.type()) {
    case ж<ast.Ident> e: {
        return Ꮡe;
    }
    case ж<ast.StarExpr> e: {
        {
            var (_, ok) = (~e).X._<ж<ast.StarExpr>>(ᐧ); if (!ok) {
                // *T is valid, but **T is not
                return embeddedFieldIdent((~e).X);
            }
        }
        break;
    }
    case ж<ast.SelectorExpr> e: {
        return Ꮡ(~e).Sel;
    }
    case ж<ast.IndexExpr> e: {
        return embeddedFieldIdent((~e).X);
    }
    case ж<ast.IndexListExpr> e: {
        return embeddedFieldIdent((~e).X);
    }}
    return default!;
}

// invalid embedded field
[GoRecv] public static bool declareInSet(this ref Checker check, ж<objset> Ꮡoset, tokenꓸPos pos, Object obj) {
    ref var oset = ref Ꮡoset.val;

    {
        var alt = oset.insert(obj); if (alt != default!) {
            var err = check.newError(DuplicateDecl);
            err.addf(((atPos)pos), "%s redeclared"u8, obj.Name());
            err.addAltDecl(alt);
            err.report();
            return false;
        }
    }
    return true;
}

[GoRecv] public static @string tag(this ref Checker check, ж<ast.BasicLit> Ꮡt) {
    ref var t = ref Ꮡt.val;

    if (t != nil) {
        if (t.Kind == token.STRING) {
            {
                var (val, err) = strconv.Unquote(t.Value); if (err == default!) {
                    return val;
                }
            }
        }
        check.errorf(~t, InvalidSyntaxTree, "incorrect tag syntax: %q"u8, t.Value);
    }
    return ""u8;
}

} // end types_package
