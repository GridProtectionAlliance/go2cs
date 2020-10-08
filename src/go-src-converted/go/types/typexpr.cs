// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements type-checking of identifiers and type expressions.

// package types -- go2cs converted at 2020 October 08 04:03:56 UTC
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
        // For the meaning of def, see Checker.definedType, below.
        // If wantType is set, the identifier e is expected to denote a type.
        //
        private static void ident(this ptr<Checker> _addr_check, ptr<operand> _addr_x, ptr<ast.Ident> _addr_e, ptr<Named> _addr_def, bool wantType)
        {
            ref Checker check = ref _addr_check.val;
            ref operand x = ref _addr_x.val;
            ref ast.Ident e = ref _addr_e.val;
            ref Named def = ref _addr_def.val;

            x.mode = invalid;
            x.expr = e; 

            // Note that we cannot use check.lookup here because the returned scope
            // may be different from obj.Parent(). See also Scope.LookupParent doc.
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
                return ;

            }
            check.recordUse(e, obj); 

            // Type-check the object.
            // Only call Checker.objDecl if the object doesn't have a type yet
            // (in which case we must actually determine it) or the object is a
            // TypeName and we also want a type (in which case we might detect
            // a cycle which needs to be reported). Otherwise we can skip the
            // call and avoid a possible cycle error in favor of the more
            // informative "not a type/value" error that this function's caller
            // will issue (see issue #25790).
            var typ = obj.Type();
            {
                ptr<TypeName> (_, gotType) = obj._<ptr<TypeName>>();

                if (typ == null || gotType && wantType)
                {
                    check.objDecl(obj, def);
                    typ = obj.Type(); // type must have been assigned by Checker.objDecl
                }
            }

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
                case ptr<PkgName> obj:
                    check.errorf(e.Pos(), "use of package %s not in selector", obj.name);
                    return ;
                    break;
                case ptr<Const> obj:
                    check.addDeclDep(obj);
                    if (typ == Typ[Invalid])
                    {
                        return ;
                    }
                    if (obj == universeIota)
                    {
                        if (check.iota == null)
                        {
                            check.errorf(e.Pos(), "cannot use iota outside constant declaration");
                            return ;
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
                case ptr<TypeName> obj:
                    x.mode = typexpr;
                    break;
                case ptr<Var> obj:
                    if (obj.pkg == check.pkg)
                    {
                        obj.used = true;
                    }
                    check.addDeclDep(obj);
                    if (typ == Typ[Invalid])
                    {
                        return ;
                    }
                    x.mode = variable;
                    break;
                case ptr<Func> obj:
                    check.addDeclDep(obj);
                    x.mode = value;
                    break;
                case ptr<Builtin> obj:
                    x.id = obj.id;
                    x.mode = builtin;
                    break;
                case ptr<Nil> obj:
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

        // typ type-checks the type expression e and returns its type, or Typ[Invalid].
        private static Type typ(this ptr<Checker> _addr_check, ast.Expr e)
        {
            ref Checker check = ref _addr_check.val;

            return check.definedType(e, null);
        }

        // definedType is like typ but also accepts a type name def.
        // If def != nil, e is the type specification for the defined type def, declared
        // in a type declaration, and def.underlying will be set to the type of e before
        // any components of e are type-checked.
        //
        private static Type definedType(this ptr<Checker> _addr_check, ast.Expr e, ptr<Named> _addr_def) => func((defer, _, __) =>
        {
            Type T = default;
            ref Checker check = ref _addr_check.val;
            ref Named def = ref _addr_def.val;

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

            T = check.typInternal(e, def);
            assert(isTyped(T));
            check.recordTypeAndValue(e, typexpr, T, null);

            return ;

        });

        // funcType type-checks a function or method type.
        private static void funcType(this ptr<Checker> _addr_check, ptr<Signature> _addr_sig, ptr<ast.FieldList> _addr_recvPar, ptr<ast.FuncType> _addr_ftyp)
        {
            ref Checker check = ref _addr_check.val;
            ref Signature sig = ref _addr_sig.val;
            ref ast.FieldList recvPar = ref _addr_recvPar.val;
            ref ast.FuncType ftyp = ref _addr_ftyp.val;

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
                ptr<Var> recv;
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
                            ptr<Named> (T, _) = t._<ptr<Named>>();

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
                                        case ptr<Basic> u:
                                            if (u.kind == UnsafePointer)
                                            {
                                                err = "unsafe.Pointer";
                                            }

                                            break;
                                        case ptr<Pointer> u:
                                            err = "pointer or interface type";
                                            break;
                                        case ptr<Interface> u:
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

        // typInternal drives type checking of types.
        // Must only be called by definedType.
        //
        private static Type typInternal(this ptr<Checker> _addr_check, ast.Expr e, ptr<Named> _addr_def)
        {
            ref Checker check = ref _addr_check.val;
            ref Named def = ref _addr_def.val;

            switch (e.type())
            {
                case ptr<ast.BadExpr> e:
                    break;
                case ptr<ast.Ident> e:
                    ref operand x = ref heap(out ptr<operand> _addr_x);
                    check.ident(_addr_x, e, def, true);


                    if (x.mode == typexpr) 
                        var typ = x.typ;
                        def.setUnderlying(typ);
                        return typ;
                    else if (x.mode == invalid)                     else if (x.mode == novalue) 
                        check.errorf(x.pos(), "%s used as type", _addr_x);
                    else 
                        check.errorf(x.pos(), "%s is not a type", _addr_x);
                                        break;
                case ptr<ast.SelectorExpr> e:
                    x = default;
                    check.selector(_addr_x, e);


                    if (x.mode == typexpr) 
                        typ = x.typ;
                        def.setUnderlying(typ);
                        return typ;
                    else if (x.mode == invalid)                     else if (x.mode == novalue) 
                        check.errorf(x.pos(), "%s used as type", _addr_x);
                    else 
                        check.errorf(x.pos(), "%s is not a type", _addr_x);
                                        break;
                case ptr<ast.ParenExpr> e:
                    return check.definedType(e.X, def);
                    break;
                case ptr<ast.ArrayType> e:
                    if (e.Len != null)
                    {
                        typ = @new<Array>();
                        def.setUnderlying(typ);
                        typ.len = check.arrayLength(e.Len);
                        typ.elem = check.typ(e.Elt);
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
                case ptr<ast.StructType> e:
                    typ = @new<Struct>();
                    def.setUnderlying(typ);
                    check.structType(typ, e);
                    return typ;
                    break;
                case ptr<ast.StarExpr> e:
                    typ = @new<Pointer>();
                    def.setUnderlying(typ);
                    typ.@base = check.typ(e.X);
                    return typ;
                    break;
                case ptr<ast.FuncType> e:
                    typ = @new<Signature>();
                    def.setUnderlying(typ);
                    check.funcType(typ, null, e);
                    return typ;
                    break;
                case ptr<ast.InterfaceType> e:
                    typ = @new<Interface>();
                    def.setUnderlying(typ);
                    check.interfaceType(typ, e, def);
                    return typ;
                    break;
                case ptr<ast.MapType> e:
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
                    check.atEnd(() =>
                    {
                        if (!Comparable(typ.key))
                        {
                            check.errorf(e.Key.Pos(), "invalid map key type %s", typ.key);
                        }

                    });

                    return typ;
                    break;
                case ptr<ast.ChanType> e:
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
        private static Type typOrNil(this ptr<Checker> _addr_check, ast.Expr e)
        {
            ref Checker check = ref _addr_check.val;

            ref operand x = ref heap(out ptr<operand> _addr_x);
            check.rawExpr(_addr_x, e, null);

            if (x.mode == invalid)
            {
                goto __switch_break0;
            }
            if (x.mode == novalue)
            {
                check.errorf(x.pos(), "%s used as type", _addr_x);
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
                check.errorf(x.pos(), "%s is not a type", _addr_x);

            __switch_break0:;
            return Typ[Invalid];

        }

        // arrayLength type-checks the array length expression e
        // and returns the constant length >= 0, or a value < 0
        // to indicate an error (and thus an unknown length).
        private static long arrayLength(this ptr<Checker> _addr_check, ast.Expr e)
        {
            ref Checker check = ref _addr_check.val;

            ref operand x = ref heap(out ptr<operand> _addr_x);
            check.expr(_addr_x, e);
            if (x.mode != constant_)
            {
                if (x.mode != invalid)
                {
                    check.errorf(x.pos(), "array length %s must be constant", _addr_x);
                }

                return -1L;

            }

            if (isUntyped(x.typ) || isInteger(x.typ))
            {
                {
                    var val = constant.ToInt(x.val);

                    if (val.Kind() == constant.Int)
                    {
                        if (representableConst(val, check, Typ[Int], null))
                        {
                            {
                                var (n, ok) = constant.Int64Val(val);

                                if (ok && n >= 0L)
                                {
                                    return n;
                                }

                            }

                            check.errorf(x.pos(), "invalid array length %s", _addr_x);
                            return -1L;

                        }

                    }

                }

            }

            check.errorf(x.pos(), "array length %s must be integer", _addr_x);
            return -1L;

        }

        private static (slice<ptr<Var>>, bool) collectParams(this ptr<Checker> _addr_check, ptr<Scope> _addr_scope, ptr<ast.FieldList> _addr_list, bool variadicOk)
        {
            slice<ptr<Var>> @params = default;
            bool variadic = default;
            ref Checker check = ref _addr_check.val;
            ref Scope scope = ref _addr_scope.val;
            ref ast.FieldList list = ref _addr_list.val;

            if (list == null)
            {
                return ;
            }

            bool named = default;            bool anonymous = default;

            foreach (var (i, field) in list.List)
            {
                var ftype = field.Type;
                {
                    ptr<ast.Ellipsis> (t, _) = ftype._<ptr<ast.Ellipsis>>();

                    if (t != null)
                    {
                        ftype = t.Elt;
                        if (variadicOk && i == len(list.List) - 1L && len(field.Names) <= 1L)
                        {
                            variadic = true;
                        }
                        else
                        {
                            check.softErrorf(t.Pos(), "can only use ... with final parameter in list"); 
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
            // Since we type-checked T rather than ...T, we also need to retro-actively
            // record the type for ...T.
            if (variadic)
            {
                var last = params[len(params) - 1L];
                last.typ = addr(new Slice(elem:last.typ));
                check.recordTypeAndValue(list.List[len(list.List) - 1L].Type, typexpr, last.typ, null);
            }

            return ;

        }

        private static bool declareInSet(this ptr<Checker> _addr_check, ptr<objset> _addr_oset, token.Pos pos, Object obj)
        {
            ref Checker check = ref _addr_check.val;
            ref objset oset = ref _addr_oset.val;

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

        private static void interfaceType(this ptr<Checker> _addr_check, ptr<Interface> _addr_ityp, ptr<ast.InterfaceType> _addr_iface, ptr<Named> _addr_def)
        {
            ref Checker check = ref _addr_check.val;
            ref Interface ityp = ref _addr_ityp.val;
            ref ast.InterfaceType iface = ref _addr_iface.val;
            ref Named def = ref _addr_def.val;

            foreach (var (_, f) in iface.Methods.List)
            {
                if (len(f.Names) > 0L)
                { 
                    // We have a method with name f.Names[0].
                    // (The parser ensures that there's only one method
                    // and we don't care if a constructed AST has more.)
                    var name = f.Names[0L];
                    if (name.Name == "_")
                    {
                        check.errorf(name.Pos(), "invalid method name _");
                        continue; // ignore
                    }

                    var typ = check.typ(f.Type);
                    ptr<Signature> (sig, _) = typ._<ptr<Signature>>();
                    if (sig == null)
                    {
                        if (typ != Typ[Invalid])
                        {
                            check.invalidAST(f.Type.Pos(), "%s is not a method signature", typ);
                        }

                        continue; // ignore
                    } 

                    // use named receiver type if available (for better error messages)
                    Type recvTyp = ityp;
                    if (def != null)
                    {
                        recvTyp = def;
                    }

                    sig.recv = NewVar(name.Pos(), check.pkg, "", recvTyp);

                    var m = NewFunc(name.Pos(), check.pkg, name.Name, sig);
                    check.recordDef(name, m);
                    ityp.methods = append(ityp.methods, m);

                }
                else
                { 
                    // We have an embedded interface and f.Type is its
                    // (possibly qualified) embedded type name. Collect
                    // it if it's a valid interface.
                    typ = check.typ(f.Type);

                    var utyp = check.underlying(typ);
                    {
                        ptr<Interface> (_, ok) = utyp._<ptr<Interface>>();

                        if (!ok)
                        {
                            if (utyp != Typ[Invalid])
                            {
                                check.errorf(f.Type.Pos(), "%s is not an interface", typ);
                            }

                            continue;

                        }

                    }


                    ityp.embeddeds = append(ityp.embeddeds, typ);
                    check.posMap[ityp] = append(check.posMap[ityp], f.Type.Pos());

                }

            }
            if (len(ityp.methods) == 0L && len(ityp.embeddeds) == 0L)
            { 
                // empty interface
                ityp.allMethods = markComplete;
                return ;

            } 

            // sort for API stability
            sort.Sort(byUniqueMethodName(ityp.methods));
            sort.Stable(byUniqueTypeName(ityp.embeddeds));

            check.later(() =>
            {
                check.completeInterface(ityp);
            });

        }

        private static void completeInterface(this ptr<Checker> _addr_check, ptr<Interface> _addr_ityp) => func((defer, panic, _) =>
        {
            ref Checker check = ref _addr_check.val;
            ref Interface ityp = ref _addr_ityp.val;

            if (ityp.allMethods != null)
            {
                return ;
            } 

            // completeInterface may be called via the LookupFieldOrMethod,
            // MissingMethod, Identical, or IdenticalIgnoreTags external API
            // in which case check will be nil. In this case, type-checking
            // must be finished and all interfaces should have been completed.
            if (check == null)
            {
                panic("internal error: incomplete interface");
            }

            if (trace)
            {
                check.trace(token.NoPos, "complete %s", ityp);
                check.indent++;
                defer(() =>
                {
                    check.indent--;
                    check.trace(token.NoPos, "=> %s", ityp);
                }());

            } 

            // An infinitely expanding interface (due to a cycle) is detected
            // elsewhere (Checker.validType), so here we simply assume we only
            // have valid interfaces. Mark the interface as complete to avoid
            // infinite recursion if the validType check occurs later for some
            // reason.
            ityp.allMethods = markComplete; 

            // Methods of embedded interfaces are collected unchanged; i.e., the identity
            // of a method I.m's Func Object of an interface I is the same as that of
            // the method m in an interface that embeds interface I. On the other hand,
            // if a method is embedded via multiple overlapping embedded interfaces, we
            // don't provide a guarantee which "original m" got chosen for the embedding
            // interface. See also issue #34421.
            //
            // If we don't care to provide this identity guarantee anymore, instead of
            // reusing the original method in embeddings, we can clone the method's Func
            // Object and give it the position of a corresponding embedded interface. Then
            // we can get rid of the mpos map below and simply use the cloned method's
            // position.

            objset seen = default;
            slice<ptr<Func>> methods = default;
            var mpos = make_map<ptr<Func>, token.Pos>(); // method specification or method embedding position, for good error messages
            Action<token.Pos, ptr<Func>, bool> addMethod = (pos, m, @explicit) =>
            {
                {
                    var other = seen.insert(m);


                    if (other == null) 
                        methods = append(methods, m);
                        mpos[m] = pos;
                    else if (explicit) 
                        check.errorf(pos, "duplicate method %s", m.name);
                        check.errorf(mpos[other._<ptr<Func>>()], "\tother declaration of %s", m.name); // secondary error, \t indented
                    else 
                        // check method signatures after all types are computed (issue #33656)
                        check.atEnd(() =>
                        {
                            if (!check.identical(m.typ, other.Type()))
                            {
                                check.errorf(pos, "duplicate method %s", m.name);
                                check.errorf(mpos[other._<ptr<Func>>()], "\tother declaration of %s", m.name); // secondary error, \t indented
                            }

                        });

                }

            }
;

            {
                var m__prev1 = m;

                foreach (var (_, __m) in ityp.methods)
                {
                    m = __m;
                    addMethod(m.pos, m, true);
                }

                m = m__prev1;
            }

            var posList = check.posMap[ityp];
            {
                var typ__prev1 = typ;

                foreach (var (__i, __typ) in ityp.embeddeds)
                {
                    i = __i;
                    typ = __typ;
                    var pos = posList[i]; // embedding position
                    ptr<Interface> (typ, ok) = check.underlying(typ)._<ptr<Interface>>();
                    if (!ok)
                    { 
                        // An error was reported when collecting the embedded types.
                        // Ignore it.
                        continue;

                    }

                    check.completeInterface(typ);
                    {
                        var m__prev2 = m;

                        foreach (var (_, __m) in typ.allMethods)
                        {
                            m = __m;
                            addMethod(pos, m, false); // use embedding position pos rather than m.pos
                        }

                        m = m__prev2;
                    }
                }

                typ = typ__prev1;
            }

            if (methods != null)
            {
                sort.Sort(byUniqueMethodName(methods));
                ityp.allMethods = methods;
            }

        });

        // byUniqueTypeName named type lists can be sorted by their unique type names.
        private partial struct byUniqueTypeName // : slice<Type>
        {
        }

        private static long Len(this byUniqueTypeName a)
        {
            return len(a);
        }
        private static bool Less(this byUniqueTypeName a, long i, long j)
        {
            return sortName(a[i]) < sortName(a[j]);
        }
        private static void Swap(this byUniqueTypeName a, long i, long j)
        {
            a[i] = a[j];
            a[j] = a[i];
        }

        private static @string sortName(Type t)
        {
            {
                ptr<Named> (named, _) = t._<ptr<Named>>();

                if (named != null)
                {
                    return named.obj.Id();
                }

            }

            return "";

        }

        // byUniqueMethodName method lists can be sorted by their unique method names.
        private partial struct byUniqueMethodName // : slice<ptr<Func>>
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

        private static @string tag(this ptr<Checker> _addr_check, ptr<ast.BasicLit> _addr_t)
        {
            ref Checker check = ref _addr_check.val;
            ref ast.BasicLit t = ref _addr_t.val;

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

        private static void structType(this ptr<Checker> _addr_check, ptr<Struct> _addr_styp, ptr<ast.StructType> _addr_e)
        {
            ref Checker check = ref _addr_check.val;
            ref Struct styp = ref _addr_styp.val;
            ref ast.StructType e = ref _addr_e.val;

            var list = e.Fields;
            if (list == null)
            {
                return ;
            } 

            // struct fields and tags
            slice<ptr<Var>> fields = default;
            slice<@string> tags = default; 

            // for double-declaration checks
            ref objset fset = ref heap(out ptr<objset> _addr_fset); 

            // current field typ and tag
            Type typ = default;
            @string tag = default;
            Action<ptr<ast.Ident>, bool, token.Pos> add = (ident, embedded, pos) =>
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
                var fld = NewField(pos, check.pkg, name, typ, embedded); 
                // spec: "Within a struct, non-blank field names must be unique."
                if (name == "_" || check.declareInSet(_addr_fset, pos, fld))
                {
                    fields = append(fields, fld);
                    check.recordDef(ident, fld);
                }

            } 

            // addInvalid adds an embedded field of invalid type to the struct for
            // fields with errors; this keeps the number of struct fields in sync
            // with the source as long as the fields are _ or have different names
            // (issue #25627).
; 

            // addInvalid adds an embedded field of invalid type to the struct for
            // fields with errors; this keeps the number of struct fields in sync
            // with the source as long as the fields are _ or have different names
            // (issue #25627).
            Action<ptr<ast.Ident>, token.Pos> addInvalid = (ident, pos) =>
            {
                typ = Typ[Invalid];
                tag = "";
                add(ident, true, pos);
            }
;

            foreach (var (_, f) in list.List)
            {
                typ = check.typ(f.Type);
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
                    // embedded field
                    // spec: "An embedded type must be specified as a type name T or as a pointer
                    // to a non-interface type name *T, and T itself may not be a pointer type."
                    var pos = f.Type.Pos();
                    name = embeddedFieldIdent(f.Type);
                    if (name == null)
                    {
                        check.invalidAST(pos, "embedded field type %s has no name", f.Type);
                        name = ast.NewIdent("_");
                        name.NamePos = pos;
                        addInvalid(name, pos);
                        continue;
                    }

                    var (t, isPtr) = deref(typ); 
                    // Because we have a name, typ must be of the form T or *T, where T is the name
                    // of a (named or alias) type, and t (= deref(typ)) must be the type of T.
                    switch (t.Underlying().type())
                    {
                        case ptr<Basic> t:
                            if (t == Typ[Invalid])
                            { 
                                // error was reported before
                                addInvalid(name, pos);
                                continue;

                            } 

                            // unsafe.Pointer is treated like a regular pointer
                            if (t.kind == UnsafePointer)
                            {
                                check.errorf(pos, "embedded field type cannot be unsafe.Pointer");
                                addInvalid(name, pos);
                                continue;
                            }

                            break;
                        case ptr<Pointer> t:
                            check.errorf(pos, "embedded field type cannot be a pointer");
                            addInvalid(name, pos);
                            continue;
                            break;
                        case ptr<Interface> t:
                            if (isPtr)
                            {
                                check.errorf(pos, "embedded field type cannot be a pointer to an interface");
                                addInvalid(name, pos);
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

        private static ptr<ast.Ident> embeddedFieldIdent(ast.Expr e)
        {
            switch (e.type())
            {
                case ptr<ast.Ident> e:
                    return _addr_e!;
                    break;
                case ptr<ast.StarExpr> e:
                    {
                        ptr<ast.StarExpr> (_, ok) = e.X._<ptr<ast.StarExpr>>();

                        if (!ok)
                        {
                            return _addr_embeddedFieldIdent(e.X)!;
                        }

                    }

                    break;
                case ptr<ast.SelectorExpr> e:
                    return _addr_e.Sel!;
                    break;
            }
            return _addr_null!; // invalid embedded field
        }
    }
}}
