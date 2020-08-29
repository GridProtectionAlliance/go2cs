// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements type-checking of identifiers and type expressions.

// package types -- go2cs converted at 2020 August 29 08:48:07 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\typexpr.go
using ast = go.go.ast_package;
using constant = go.go.constant_package;
using token = go.go.token_package;
using sort = go.sort_package;
using strconv = go.strconv_package;
using static go.builtin;
using System;

namespace go {
namespace go
{
    public static partial class types_package
    {
        // ident type-checks identifier e and initializes x with the value or type of e.
        // If an error occurred, x.mode is set to invalid.
        // For the meaning of def and path, see check.typ, below.
        //
        private static void ident(this ref Checker check, ref operand x, ref ast.Ident e, ref Named def, slice<ref TypeName> path)
        {
            x.mode = invalid;
            x.expr = e;

            var (scope, obj) = check.scope.LookupParent(e.Name, check.pos);
            if (obj == null)
            {
                if (e.Name == "_")
                {
                    check.errorf(e.Pos(), "cannot use _ as value or type");
                }
                else
                {
                    check.errorf(e.Pos(), "undeclared name: %s", e.Name);
                }
                return;
            }
            check.recordUse(e, obj);

            check.objDecl(obj, def, path);
            var typ = obj.Type();
            assert(typ != null); 

            // The object may be dot-imported: If so, remove its package from
            // the map of unused dot imports for the respective file scope.
            // (This code is only needed for dot-imports. Without them,
            // we only have to mark variables, see *Var case below).
            {
                var pkg = obj.Pkg();

                if (pkg != check.pkg && pkg != null)
                {
                    delete(check.unusedDotImports[scope], pkg);
                }
            }

            switch (obj.type())
            {
                case ref PkgName obj:
                    check.errorf(e.Pos(), "use of package %s not in selector", obj.name);
                    return;
                    break;
                case ref Const obj:
                    check.addDeclDep(obj);
                    if (typ == Typ[Invalid])
                    {
                        return;
                    }
                    if (obj == universeIota)
                    {
                        if (check.iota == null)
                        {
                            check.errorf(e.Pos(), "cannot use iota outside constant declaration");
                            return;
                        }
                        x.val = check.iota;
                    }
                    else
                    {
                        x.val = obj.val;
                    }
                    assert(x.val != null);
                    x.mode = constant_;
                    break;
                case ref TypeName obj:
                    x.mode = typexpr; 
                    // check for cycle
                    // (it's ok to iterate forward because each named type appears at most once in path)
                    foreach (var (i, prev) in path)
                    {
                        if (prev == obj)
                        {
                            check.errorf(obj.pos, "illegal cycle in declaration of %s", obj.name); 
                            // print cycle
                            {
                                var obj__prev2 = obj;

                                foreach (var (_, __obj) in path[i..])
                                {
                                    obj = __obj;
                                    check.errorf(obj.Pos(), "\t%s refers to", obj.Name()); // secondary error, \t indented
                                }
                                obj = obj__prev2;
                            }

                            check.errorf(obj.Pos(), "\t%s", obj.Name()); 
                            // maintain x.mode == typexpr despite error
                            typ = Typ[Invalid];
                            break;
                        }
                    }                    break;
                case ref Var obj:
                    if (obj.pkg == check.pkg)
                    {
                        obj.used = true;
                    }
                    check.addDeclDep(obj);
                    if (typ == Typ[Invalid])
                    {
                        return;
                    }
                    x.mode = variable;
                    break;
                case ref Func obj:
                    check.addDeclDep(obj);
                    x.mode = value;
                    break;
                case ref Builtin obj:
                    x.id = obj.id;
                    x.mode = builtin;
                    break;
                case ref Nil obj:
                    x.mode = value;
                    break;
                default:
                {
                    var obj = obj.type();
                    unreachable();
                    break;
                }

            }

            x.typ = typ;
        }

        // typExpr type-checks the type expression e and returns its type, or Typ[Invalid].
        // If def != nil, e is the type specification for the named type def, declared
        // in a type declaration, and def.underlying will be set to the type of e before
        // any components of e are type-checked. Path contains the path of named types
        // referring to this type.
        //
        private static Type typExpr(this ref Checker _check, ast.Expr e, ref Named _def, slice<ref TypeName> path) => func(_check, _def, (ref Checker check, ref Named def, Defer defer, Panic _, Recover __) =>
        {
            if (trace)
            {
                check.trace(e.Pos(), "%s", e);
                check.indent++;
                defer(() =>
                {
                    check.indent--;
                    check.trace(e.Pos(), "=> %s", T);
                }());
            }
            T = check.typExprInternal(e, def, path);
            assert(isTyped(T));
            check.recordTypeAndValue(e, typexpr, T, null);

            return;
        });

        private static Type typ(this ref Checker check, ast.Expr e)
        {
            return check.typExpr(e, null, null);
        }

        // funcType type-checks a function or method type.
        private static void funcType(this ref Checker check, ref Signature sig, ref ast.FieldList recvPar, ref ast.FuncType ftyp)
        {
            var scope = NewScope(check.scope, token.NoPos, token.NoPos, "function");
            scope.isFunc = true;
            check.recordScope(ftyp, scope);

            var (recvList, _) = check.collectParams(scope, recvPar, false);
            var (params, variadic) = check.collectParams(scope, ftyp.Params, true);
            var (results, _) = check.collectParams(scope, ftyp.Results, false);

            if (recvPar != null)
            { 
                // recv parameter list present (may be empty)
                // spec: "The receiver is specified via an extra parameter section preceding the
                // method name. That parameter section must declare a single parameter, the receiver."
                ref Var recv = default;
                switch (len(recvList))
                {
                    case 0L: 
                        check.error(recvPar.Pos(), "method is missing receiver");
                        recv = NewParam(0L, null, "", Typ[Invalid]); // ignore recv below
                        break;
                    case 1L: 
                        recv = recvList[0L];
                        break;
                    default: 
                        // more than one receiver
                        check.error(recvList[len(recvList) - 1L].Pos(), "method must have exactly one receiver");
                        break;
                } 
                // spec: "The receiver type must be of the form T or *T where T is a type name."
                // (ignore invalid types - error was reported before)
                {
                    var (t, _) = deref(recv.typ);

                    if (t != Typ[Invalid])
                    {
                        @string err = default;
                        {
                            ref Named (T, _) = t._<ref Named>();

                            if (T != null)
                            { 
                                // spec: "The type denoted by T is called the receiver base type; it must not
                                // be a pointer or interface type and it must be declared in the same package
                                // as the method."
                                if (T.obj.pkg != check.pkg)
                                {
                                    err = "type not defined in this package";
                                }
                                else
                                { 
                                    // TODO(gri) This is not correct if the underlying type is unknown yet.
                                    switch (T.underlying.type())
                                    {
                                        case ref Basic u:
                                            if (u.kind == UnsafePointer)
                                            {
                                                err = "unsafe.Pointer";
                                            }
                                            break;
                                        case ref Pointer u:
                                            err = "pointer or interface type";
                                            break;
                                        case ref Interface u:
                                            err = "pointer or interface type";
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                err = "basic or unnamed type";
                            }

                        }
                        if (err != "")
                        {
                            check.errorf(recv.pos, "invalid receiver %s (%s)", recv.typ, err); 
                            // ok to continue
                        }
                    }

                }
                sig.recv = recv;
            }
            sig.scope = scope;
            sig.@params = NewTuple(params);
            sig.results = NewTuple(results);
            sig.variadic = variadic;
        }

        // typExprInternal drives type checking of types.
        // Must only be called by typExpr.
        //
        private static Type typExprInternal(this ref Checker check, ast.Expr e, ref Named def, slice<ref TypeName> path)
        {
            switch (e.type())
            {
                case ref ast.BadExpr e:
                    break;
                case ref ast.Ident e:
                    operand x = default;
                    check.ident(ref x, e, def, path);


                    if (x.mode == typexpr) 
                        var typ = x.typ;
                        def.setUnderlying(typ);
                        return typ;
                    else if (x.mode == invalid)                     else if (x.mode == novalue) 
                        check.errorf(x.pos(), "%s used as type", ref x);
                    else 
                        check.errorf(x.pos(), "%s is not a type", ref x);
                                        break;
                case ref ast.SelectorExpr e:
                    x = default;
                    check.selector(ref x, e);


                    if (x.mode == typexpr) 
                        typ = x.typ;
                        def.setUnderlying(typ);
                        return typ;
                    else if (x.mode == invalid)                     else if (x.mode == novalue) 
                        check.errorf(x.pos(), "%s used as type", ref x);
                    else 
                        check.errorf(x.pos(), "%s is not a type", ref x);
                                        break;
                case ref ast.ParenExpr e:
                    return check.typExpr(e.X, def, path);
                    break;
                case ref ast.ArrayType e:
                    if (e.Len != null)
                    {
                        typ = @new<Array>();
                        def.setUnderlying(typ);
                        typ.len = check.arrayLength(e.Len);
                        typ.elem = check.typExpr(e.Elt, null, path);
                        return typ;

                    }
                    else
                    {
                        typ = @new<Slice>();
                        def.setUnderlying(typ);
                        typ.elem = check.typ(e.Elt);
                        return typ;
                    }
                    break;
                case ref ast.StructType e:
                    typ = @new<Struct>();
                    def.setUnderlying(typ);
                    check.structType(typ, e, path);
                    return typ;
                    break;
                case ref ast.StarExpr e:
                    typ = @new<Pointer>();
                    def.setUnderlying(typ);
                    typ.@base = check.typ(e.X);
                    return typ;
                    break;
                case ref ast.FuncType e:
                    typ = @new<Signature>();
                    def.setUnderlying(typ);
                    check.funcType(typ, null, e);
                    return typ;
                    break;
                case ref ast.InterfaceType e:
                    typ = @new<Interface>();
                    def.setUnderlying(typ);
                    check.interfaceType(typ, e, def, path);
                    return typ;
                    break;
                case ref ast.MapType e:
                    typ = @new<Map>();
                    def.setUnderlying(typ);

                    typ.key = check.typ(e.Key);
                    typ.elem = check.typ(e.Value); 

                    // spec: "The comparison operators == and != must be fully defined
                    // for operands of the key type; thus the key type must not be a
                    // function, map, or slice."
                    //
                    // Delay this check because it requires fully setup types;
                    // it is safe to continue in any case (was issue 6667).
                    check.delay(() =>
                    {
                        if (!Comparable(typ.key))
                        {
                            check.errorf(e.Key.Pos(), "invalid map key type %s", typ.key);
                        }
                    });

                    return typ;
                    break;
                case ref ast.ChanType e:
                    typ = @new<Chan>();
                    def.setUnderlying(typ);

                    var dir = SendRecv;

                    if (e.Dir == ast.SEND | ast.RECV)                     else if (e.Dir == ast.SEND) 
                        dir = SendOnly;
                    else if (e.Dir == ast.RECV) 
                        dir = RecvOnly;
                    else 
                        check.invalidAST(e.Pos(), "unknown channel direction %d", e.Dir); 
                        // ok to continue
                                        typ.dir = dir;
                    typ.elem = check.typ(e.Value);
                    return typ;
                    break;
                default:
                {
                    var e = e.type();
                    check.errorf(e.Pos(), "%s is not a type", e);
                    break;
                }

            }

            typ = Typ[Invalid];
            def.setUnderlying(typ);
            return typ;
        }

        // typeOrNil type-checks the type expression (or nil value) e
        // and returns the typ of e, or nil.
        // If e is neither a type nor nil, typOrNil returns Typ[Invalid].
        //
        private static Type typOrNil(this ref Checker check, ast.Expr e)
        {
            operand x = default;
            check.rawExpr(ref x, e, null);

            if (x.mode == invalid)
            {
                goto __switch_break0;
            }
            if (x.mode == novalue)
            {
                check.errorf(x.pos(), "%s used as type", ref x);
                goto __switch_break0;
            }
            if (x.mode == typexpr)
            {
                return x.typ;
                goto __switch_break0;
            }
            if (x.mode == value)
            {
                if (x.isNil())
                {
                    return null;
                }
            }
            // default: 
                check.errorf(x.pos(), "%s is not a type", ref x);

            __switch_break0:;
            return Typ[Invalid];
        }

        private static long arrayLength(this ref Checker check, ast.Expr e)
        {
            operand x = default;
            check.expr(ref x, e);
            if (x.mode != constant_)
            {
                if (x.mode != invalid)
                {
                    check.errorf(x.pos(), "array length %s must be constant", ref x);
                }
                return 0L;
            }
            if (isUntyped(x.typ) || isInteger(x.typ))
            {
                {
                    var val = constant.ToInt(x.val);

                    if (val.Kind() == constant.Int)
                    {
                        if (representableConst(val, check.conf, Typ[Int], null))
                        {
                            {
                                var (n, ok) = constant.Int64Val(val);

                                if (ok && n >= 0L)
                                {
                                    return n;
                                }

                            }
                            check.errorf(x.pos(), "invalid array length %s", ref x);
                            return 0L;
                        }
                    }

                }
            }
            check.errorf(x.pos(), "array length %s must be integer", ref x);
            return 0L;
        }

        private static (slice<ref Var>, bool) collectParams(this ref Checker check, ref Scope scope, ref ast.FieldList list, bool variadicOk)
        {
            if (list == null)
            {
                return;
            }
            bool named = default;            bool anonymous = default;

            foreach (var (i, field) in list.List)
            {
                var ftype = field.Type;
                {
                    ref ast.Ellipsis (t, _) = ftype._<ref ast.Ellipsis>();

                    if (t != null)
                    {
                        ftype = t.Elt;
                        if (variadicOk && i == len(list.List) - 1L)
                        {
                            variadic = true;
                        }
                        else
                        {
                            check.invalidAST(field.Pos(), "... not permitted"); 
                            // ignore ... and continue
                        }
                    }

                }
                var typ = check.typ(ftype); 
                // The parser ensures that f.Tag is nil and we don't
                // care if a constructed AST contains a non-nil tag.
                if (len(field.Names) > 0L)
                { 
                    // named parameter
                    foreach (var (_, name) in field.Names)
                    {
                        if (name.Name == "")
                        {
                            check.invalidAST(name.Pos(), "anonymous parameter"); 
                            // ok to continue
                        }
                        var par = NewParam(name.Pos(), check.pkg, name.Name, typ);
                        check.declare(scope, name, par, scope.pos);
                        params = append(params, par);
                    }
                else
                    named = true;
                }                { 
                    // anonymous parameter
                    par = NewParam(ftype.Pos(), check.pkg, "", typ);
                    check.recordImplicit(field, par);
                    params = append(params, par);
                    anonymous = true;
                }
            }
            if (named && anonymous)
            {
                check.invalidAST(list.Pos(), "list contains both named and anonymous parameters"); 
                // ok to continue
            } 

            // For a variadic function, change the last parameter's type from T to []T.
            if (variadic && len(params) > 0L)
            {
                var last = params[len(params) - 1L];
                last.typ = ref new Slice(elem:last.typ);
            }
            return;
        }

        private static bool declareInSet(this ref Checker check, ref objset oset, token.Pos pos, Object obj)
        {
            {
                var alt = oset.insert(obj);

                if (alt != null)
                {
                    check.errorf(pos, "%s redeclared", obj.Name());
                    check.reportAltDecl(alt);
                    return false;
                }

            }
            return true;
        }

        private static void interfaceType(this ref Checker check, ref Interface iface, ref ast.InterfaceType ityp, ref Named def, slice<ref TypeName> path)
        { 
            // empty interface: common case
            if (ityp.Methods == null)
            {
                return;
            } 

            // The parser ensures that field tags are nil and we don't
            // care if a constructed AST contains non-nil tags.

            // use named receiver type if available (for better error messages)
            Type recvTyp = iface;
            if (def != null)
            {
                recvTyp = def;
            } 

            // Phase 1: Collect explicitly declared methods, the corresponding
            //          signature (AST) expressions, and the list of embedded
            //          type (AST) expressions. Do not resolve signatures or
            //          embedded types yet to avoid cycles referring to this
            //          interface.
            objset mset = default;            slice<ast.Expr> signatures = default;            slice<ast.Expr> embedded = default;
            foreach (var (_, f) in ityp.Methods.List)
            {
                if (len(f.Names) > 0L)
                { 
                    // The parser ensures that there's only one method
                    // and we don't care if a constructed AST has more.
                    var name = f.Names[0L];
                    var pos = name.Pos(); 
                    // spec: "As with all method sets, in an interface type,
                    // each method must have a unique non-blank name."
                    if (name.Name == "_")
                    {
                        check.errorf(pos, "invalid method name _");
                        continue;
                    } 
                    // Don't type-check signature yet - use an
                    // empty signature now and update it later.
                    // Since we know the receiver, set it up now
                    // (required to avoid crash in ptrRecv; see
                    // e.g. test case for issue 6638).
                    // TODO(gri) Consider marking methods signatures
                    // as incomplete, for better error messages. See
                    // also the T4 and T5 tests in testdata/cycles2.src.
                    ptr<Signature> sig = @new<Signature>();
                    sig.recv = NewVar(pos, check.pkg, "", recvTyp);
                    var m = NewFunc(pos, check.pkg, name.Name, sig);
                    if (check.declareInSet(ref mset, pos, m))
                    {
                        iface.methods = append(iface.methods, m);
                        iface.allMethods = append(iface.allMethods, m);
                        signatures = append(signatures, f.Type);
                        check.recordDef(name, m);
                    }
                }
                else
                { 
                    // embedded type
                    embedded = append(embedded, f.Type);
                }
            } 

            // Phase 2: Resolve embedded interfaces. Because an interface must not
            //          embed itself (directly or indirectly), each embedded interface
            //          can be fully resolved without depending on any method of this
            //          interface (if there is a cycle or another error, the embedded
            //          type resolves to an invalid type and is ignored).
            //          In particular, the list of methods for each embedded interface
            //          must be complete (it cannot depend on this interface), and so
            //          those methods can be added to the list of all methods of this
            //          interface.
            foreach (var (_, e) in embedded)
            {
                pos = e.Pos();
                var typ = check.typExpr(e, null, path); 
                // Determine underlying embedded (possibly incomplete) type
                // by following its forward chain.
                ref Named (named, _) = typ._<ref Named>();
                var under = underlying(named);
                ref Interface (embed, _) = under._<ref Interface>();
                if (embed == null)
                {
                    if (typ != Typ[Invalid])
                    {
                        check.errorf(pos, "%s is not an interface", typ);
                    }
                    continue;
                }
                iface.embeddeds = append(iface.embeddeds, named); 
                // collect embedded methods
                if (embed.allMethods == null)
                {
                    check.errorf(pos, "internal error: incomplete embedded interface %s (issue #18395)", named);
                }
                {
                    var m__prev2 = m;

                    foreach (var (_, __m) in embed.allMethods)
                    {
                        m = __m;
                        if (check.declareInSet(ref mset, pos, m))
                        {
                            iface.allMethods = append(iface.allMethods, m);
                        }
                    }

                    m = m__prev2;
                }

            } 

            // Phase 3: At this point all methods have been collected for this interface.
            //          It is now safe to type-check the signatures of all explicitly
            //          declared methods, even if they refer to this interface via a cycle
            //          and embed the methods of this interface in a parameter of interface
            //          type.
            {
                var m__prev1 = m;

                foreach (var (__i, __m) in iface.methods)
                {
                    i = __i;
                    m = __m;
                    var expr = signatures[i];
                    typ = check.typ(expr);
                    ref Signature (sig, _) = typ._<ref Signature>();
                    if (sig == null)
                    {
                        if (typ != Typ[Invalid])
                        {
                            check.invalidAST(expr.Pos(), "%s is not a method signature", typ);
                        }
                        continue; // keep method with empty method signature
                    } 
                    // update signature, but keep recv that was set up before
                    ref Signature old = m.typ._<ref Signature>();
                    sig.recv = old.recv;
                    old.Value = sig.Value; // update signature (don't replace it!)
                } 

                // TODO(gri) The list of explicit methods is only sorted for now to
                // produce the same Interface as NewInterface. We may be able to
                // claim source order in the future. Revisit.

                m = m__prev1;
            }

            sort.Sort(byUniqueMethodName(iface.methods)); 

            // TODO(gri) The list of embedded types is only sorted for now to
            // produce the same Interface as NewInterface. We may be able to
            // claim source order in the future. Revisit.
            sort.Sort(byUniqueTypeName(iface.embeddeds));

            if (iface.allMethods == null)
            {
                iface.allMethods = make_slice<ref Func>(0L); // mark interface as complete
            }
            else
            {
                sort.Sort(byUniqueMethodName(iface.allMethods));
            }
        }

        // byUniqueTypeName named type lists can be sorted by their unique type names.
        private partial struct byUniqueTypeName // : slice<ref Named>
        {
        }

        private static long Len(this byUniqueTypeName a)
        {
            return len(a);
        }
        private static bool Less(this byUniqueTypeName a, long i, long j)
        {
            return a[i].obj.Id() < a[j].obj.Id();
        }
        private static void Swap(this byUniqueTypeName a, long i, long j)
        {
            a[i] = a[j];
            a[j] = a[i];

        }

        // byUniqueMethodName method lists can be sorted by their unique method names.
        private partial struct byUniqueMethodName // : slice<ref Func>
        {
        }

        private static long Len(this byUniqueMethodName a)
        {
            return len(a);
        }
        private static bool Less(this byUniqueMethodName a, long i, long j)
        {
            return a[i].Id() < a[j].Id();
        }
        private static void Swap(this byUniqueMethodName a, long i, long j)
        {
            a[i] = a[j];
            a[j] = a[i];

        }

        private static @string tag(this ref Checker check, ref ast.BasicLit t)
        {
            if (t != null)
            {
                if (t.Kind == token.STRING)
                {
                    {
                        var (val, err) = strconv.Unquote(t.Value);

                        if (err == null)
                        {
                            return val;
                        }

                    }
                }
                check.invalidAST(t.Pos(), "incorrect tag syntax: %q", t.Value);
            }
            return "";
        }

        private static void structType(this ref Checker check, ref Struct styp, ref ast.StructType e, slice<ref TypeName> path)
        {
            var list = e.Fields;
            if (list == null)
            {
                return;
            } 

            // struct fields and tags
            slice<ref Var> fields = default;
            slice<@string> tags = default; 

            // for double-declaration checks
            objset fset = default; 

            // current field typ and tag
            Type typ = default;
            @string tag = default;
            Action<ref ast.Ident, bool, token.Pos> add = (ident, anonymous, pos) =>
            {
                if (tag != "" && tags == null)
                {
                    tags = make_slice<@string>(len(fields));
                }
                if (tags != null)
                {
                    tags = append(tags, tag);
                }
                var name = ident.Name;
                var fld = NewField(pos, check.pkg, name, typ, anonymous); 
                // spec: "Within a struct, non-blank field names must be unique."
                if (name == "_" || check.declareInSet(ref fset, pos, fld))
                {
                    fields = append(fields, fld);
                    check.recordDef(ident, fld);
                }
            }
;

            foreach (var (_, f) in list.List)
            {
                typ = check.typExpr(f.Type, null, path);
                tag = check.tag(f.Tag);
                if (len(f.Names) > 0L)
                { 
                    // named fields
                    {
                        var name__prev2 = name;

                        foreach (var (_, __name) in f.Names)
                        {
                            name = __name;
                            add(name, false, name.Pos());
                        }
                else

                        name = name__prev2;
                    }

                }                { 
                    // anonymous field
                    // spec: "An embedded type must be specified as a type name T or as a pointer
                    // to a non-interface type name *T, and T itself may not be a pointer type."
                    var pos = f.Type.Pos();
                    name = anonymousFieldIdent(f.Type);
                    if (name == null)
                    {
                        check.invalidAST(pos, "anonymous field type %s has no name", f.Type);
                        continue;
                    }
                    var (t, isPtr) = deref(typ); 
                    // Because we have a name, typ must be of the form T or *T, where T is the name
                    // of a (named or alias) type, and t (= deref(typ)) must be the type of T.
                    switch (t.Underlying().type())
                    {
                        case ref Basic t:
                            if (t == Typ[Invalid])
                            { 
                                // error was reported before
                                continue;
                            } 

                            // unsafe.Pointer is treated like a regular pointer
                            if (t.kind == UnsafePointer)
                            {
                                check.errorf(pos, "anonymous field type cannot be unsafe.Pointer");
                                continue;
                            }
                            break;
                        case ref Pointer t:
                            check.errorf(pos, "anonymous field type cannot be a pointer");
                            continue;
                            break;
                        case ref Interface t:
                            if (isPtr)
                            {
                                check.errorf(pos, "anonymous field type cannot be a pointer to an interface");
                                continue;
                            }
                            break;
                    }
                    add(name, true, pos);
                }
            }
            styp.fields = fields;
            styp.tags = tags;
        }

        private static ref ast.Ident anonymousFieldIdent(ast.Expr e)
        {
            switch (e.type())
            {
                case ref ast.Ident e:
                    return e;
                    break;
                case ref ast.StarExpr e:
                    {
                        ref ast.StarExpr (_, ok) = e.X._<ref ast.StarExpr>();

                        if (!ok)
                        {
                            return anonymousFieldIdent(e.X);
                        }

                    }
                    break;
                case ref ast.SelectorExpr e:
                    return e.Sel;
                    break;
            }
            return null; // invalid anonymous field
        }
    }
}}
