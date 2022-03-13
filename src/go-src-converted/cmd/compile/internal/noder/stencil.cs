// Copyright 2021 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file will evolve, since we plan to do a mix of stenciling and passing
// around dictionaries.

// package noder -- go2cs converted at 2022 March 13 06:27:40 UTC
// import "cmd/compile/internal/noder" ==> using noder = go.cmd.compile.@internal.noder_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\noder\stencil.go
namespace go.cmd.compile.@internal;

using bytes = bytes_package;
using @base = cmd.compile.@internal.@base_package;
using ir = cmd.compile.@internal.ir_package;
using typecheck = cmd.compile.@internal.typecheck_package;
using types = cmd.compile.@internal.types_package;
using src = cmd.@internal.src_package;
using fmt = fmt_package;
using strings = strings_package;


// For catching problems as we add more features
// TODO(danscales): remove assertions or replace with base.FatalfAt()

using System;
public static partial class noder_package {

private static void assert(bool p) => func((_, panic, _) => {
    if (!p) {
        panic("assertion failed");
    }
});

// stencil scans functions for instantiated generic function calls and creates the
// required instantiations for simple generic functions. It also creates
// instantiated methods for all fully-instantiated generic types that have been
// encountered already or new ones that are encountered during the stenciling
// process.
private static void stencil(this ptr<irgen> _addr_g) {
    ref irgen g = ref _addr_g.val;

    g.target.Stencils = make_map<ptr<types.Sym>, ptr<ir.Func>>(); 

    // Instantiate the methods of instantiated generic types that we have seen so far.
    g.instantiateMethods(); 

    // Don't use range(g.target.Decls) - we also want to process any new instantiated
    // functions that are created during this loop, in order to handle generic
    // functions calling other generic functions.
    for (nint i = 0; i < len(g.target.Decls); i++) {
        var decl = g.target.Decls[i]; 

        // Look for function instantiations in bodies of non-generic
        // functions or in global assignments (ignore global type and
        // constant declarations).

        if (decl.Op() == ir.ODCLFUNC) 
            if (decl.Type().HasTParam()) { 
                // Skip any generic functions
                continue;
            } 
            // transformCall() below depends on CurFunc being set.
            ir.CurFunc = decl._<ptr<ir.Func>>();
        else if (decl.Op() == ir.OAS || decl.Op() == ir.OAS2 || decl.Op() == ir.OAS2DOTTYPE || decl.Op() == ir.OAS2FUNC || decl.Op() == ir.OAS2MAPR || decl.Op() == ir.OAS2RECV || decl.Op() == ir.OASOP)         else 
            // The other possible ops at the top level are ODCLCONST
            // and ODCLTYPE, which don't have any function
            // instantiations.
            continue;
        // For all non-generic code, search for any function calls using
        // generic function instantiations. Then create the needed
        // instantiated function if it hasn't been created yet, and change
        // to calling that function directly.
        var modified = false;
        var foundFuncInst = false;
        ir.Visit(decl, n => {
            if (n.Op() == ir.OFUNCINST) { 
                // We found a function instantiation that is not
                // immediately called.
                foundFuncInst = true;
            }
            if (n.Op() != ir.OCALL || n._<ptr<ir.CallExpr>>().X.Op() != ir.OFUNCINST) {
                return ;
            } 
            // We have found a function call using a generic function
            // instantiation.
            ptr<ir.CallExpr> call = n._<ptr<ir.CallExpr>>();
            ptr<ir.InstExpr> inst = call.X._<ptr<ir.InstExpr>>();
            var st = g.getInstantiationForNode(inst); 
            // Replace the OFUNCINST with a direct reference to the
            // new stenciled function
            call.X = st.Nname;
            if (inst.X.Op() == ir.OCALLPART) { 
                // When we create an instantiation of a method
                // call, we make it a function. So, move the
                // receiver to be the first arg of the function
                // call.
                var withRecv = make_slice<ir.Node>(len(call.Args) + 1);
                ptr<ir.SelectorExpr> dot = inst.X._<ptr<ir.SelectorExpr>>();
                withRecv[0] = dot.X;
                copy(withRecv[(int)1..], call.Args);
                call.Args = withRecv;
            } 
            // Transform the Call now, which changes OCALL
            // to OCALLFUNC and does typecheckaste/assignconvfn.
            transformCall(call);
            modified = true;
        }); 

        // If we found an OFUNCINST without a corresponding call in the
        // above decl, then traverse the nodes of decl again (with
        // EditChildren rather than Visit), where we actually change the
        // OFUNCINST node to an ONAME for the instantiated function.
        // EditChildren is more expensive than Visit, so we only do this
        // in the infrequent case of an OFUNCINSt without a corresponding
        // call.
        if (foundFuncInst) {
            Func<ir.Node, ir.Node> edit = default;
            edit = x => {
                if (x.Op() == ir.OFUNCINST) {
                    st = g.getInstantiationForNode(x._<ptr<ir.InstExpr>>());
                    return st.Nname;
                }
                ir.EditChildren(x, edit);
                return x;
            }
;
            edit(decl);
        }
        if (@base.Flag.W > 1 && modified) {
            ir.Dump(fmt.Sprintf("\nmodified %v", decl), decl);
        }
        ir.CurFunc = null; 
        // We may have seen new fully-instantiated generic types while
        // instantiating any needed functions/methods in the above
        // function. If so, instantiate all the methods of those types
        // (which will then lead to more function/methods to scan in the loop).
        g.instantiateMethods();
    }
}

// instantiateMethods instantiates all the methods of all fully-instantiated
// generic types that have been added to g.instTypeList.
private static void instantiateMethods(this ptr<irgen> _addr_g) {
    ref irgen g = ref _addr_g.val;

    for (nint i = 0; i < len(g.instTypeList); i++) {
        var typ = g.instTypeList[i]; 
        // Get the base generic type by looking up the symbol of the
        // generic (uninstantiated) name.
        var baseSym = typ.Sym().Pkg.Lookup(genericTypeName(_addr_typ.Sym()));
        ptr<ir.Name> baseType = baseSym.Def._<ptr<ir.Name>>().Type();
        foreach (var (j, m) in typ.Methods().Slice()) {
            ptr<ir.Name> name = m.Nname._<ptr<ir.Name>>();
            var targs = make_slice<ir.Node>(len(typ.RParams()));
            foreach (var (k, targ) in typ.RParams()) {
                targs[k] = ir.TypeNode(targ);
            }
            ptr<ir.Name> baseNname = baseType.Methods().Slice()[j].Nname._<ptr<ir.Name>>();
            name.Func = g.getInstantiation(baseNname, targs, true);
        }
    }
    g.instTypeList = null;
}

// genericSym returns the name of the base generic type for the type named by
// sym. It simply returns the name obtained by removing everything after the
// first bracket ("[").
private static @string genericTypeName(ptr<types.Sym> _addr_sym) {
    ref types.Sym sym = ref _addr_sym.val;

    return sym.Name[(int)0..(int)strings.Index(sym.Name, "[")];
}

// getInstantiationForNode returns the function/method instantiation for a
// InstExpr node inst.
private static ptr<ir.Func> getInstantiationForNode(this ptr<irgen> _addr_g, ptr<ir.InstExpr> _addr_inst) {
    ref irgen g = ref _addr_g.val;
    ref ir.InstExpr inst = ref _addr_inst.val;

    {
        ptr<ir.SelectorExpr> (meth, ok) = inst.X._<ptr<ir.SelectorExpr>>();

        if (ok) {
            return _addr_g.getInstantiation(meth.Selection.Nname._<ptr<ir.Name>>(), inst.Targs, true)!;
        }
        else
 {
            return _addr_g.getInstantiation(inst.X._<ptr<ir.Name>>(), inst.Targs, false)!;
        }
    }
}

// getInstantiation gets the instantiantion of the function or method nameNode
// with the type arguments targs. If the instantiated function is not already
// cached, then it calls genericSubst to create the new instantiation.
private static ptr<ir.Func> getInstantiation(this ptr<irgen> _addr_g, ptr<ir.Name> _addr_nameNode, slice<ir.Node> targs, bool isMeth) {
    ref irgen g = ref _addr_g.val;
    ref ir.Name nameNode = ref _addr_nameNode.val;

    var sym = makeInstName(_addr_nameNode.Sym(), targs, isMeth);
    var st = g.target.Stencils[sym];
    if (st == null) { 
        // If instantiation doesn't exist yet, create it and add
        // to the list of decls.
        st = g.genericSubst(sym, nameNode, targs, isMeth);
        g.target.Stencils[sym] = st;
        g.target.Decls = append(g.target.Decls, st);
        if (@base.Flag.W > 1) {
            ir.Dump(fmt.Sprintf("\nstenciled %v", st), st);
        }
    }
    return _addr_st!;
}

// makeInstName makes the unique name for a stenciled generic function or method,
// based on the name of the function fy=nsym and the targs. It replaces any
// existing bracket type list in the name. makeInstName asserts that fnsym has
// brackets in its name if and only if hasBrackets is true.
// TODO(danscales): remove the assertions and the hasBrackets argument later.
//
// Names of declared generic functions have no brackets originally, so hasBrackets
// should be false. Names of generic methods already have brackets, since the new
// type parameter is specified in the generic type of the receiver (e.g. func
// (func (v *value[T]).set(...) { ... } has the original name (*value[T]).set.
//
// The standard naming is something like: 'genFn[int,bool]' for functions and
// '(*genType[int,bool]).methodName' for methods
private static ptr<types.Sym> makeInstName(ptr<types.Sym> _addr_fnsym, slice<ir.Node> targs, bool hasBrackets) {
    ref types.Sym fnsym = ref _addr_fnsym.val;

    var b = bytes.NewBufferString("");
    var name = fnsym.Name;
    var i = strings.Index(name, "[");
    assert(hasBrackets == (i >= 0));
    if (i >= 0) {
        b.WriteString(name[(int)0..(int)i]);
    }
    else
 {
        b.WriteString(name);
    }
    b.WriteString("[");
    {
        var i__prev1 = i;

        foreach (var (__i, __targ) in targs) {
            i = __i;
            targ = __targ;
            if (i > 0) {
                b.WriteString(",");
            }
            b.WriteString(targ.Type().String());
        }
        i = i__prev1;
    }

    b.WriteString("]");
    if (i >= 0) {
        var i2 = strings.Index(name[(int)i..], "]");
        assert(i2 >= 0);
        b.WriteString(name[(int)i + i2 + 1..]);
    }
    return _addr_typecheck.Lookup(b.String())!;
}

// Struct containing info needed for doing the substitution as we create the
// instantiation of a generic function with specified type arguments.
private partial struct subster {
    public ptr<irgen> g;
    public bool isMethod; // If a method is being instantiated
    public ptr<ir.Func> newf; // Func node for the new stenciled function
    public slice<ptr<types.Field>> tparams;
    public slice<ir.Node> targs; // The substitution map from name nodes in the generic function to the
// name nodes in the new stenciled function.
    public map<ptr<ir.Name>, ptr<ir.Name>> vars;
}

// genericSubst returns a new function with name newsym. The function is an
// instantiation of a generic function or method specified by namedNode with type
// args targs. For a method with a generic receiver, it returns an instantiated
// function type where the receiver becomes the first parameter. Otherwise the
// instantiated method would still need to be transformed by later compiler
// phases.
private static ptr<ir.Func> genericSubst(this ptr<irgen> _addr_g, ptr<types.Sym> _addr_newsym, ptr<ir.Name> _addr_nameNode, slice<ir.Node> targs, bool isMethod) {
    ref irgen g = ref _addr_g.val;
    ref types.Sym newsym = ref _addr_newsym.val;
    ref ir.Name nameNode = ref _addr_nameNode.val;

    slice<ptr<types.Field>> tparams = default;
    if (isMethod) { 
        // Get the type params from the method receiver (after skipping
        // over any pointer)
        var recvType = nameNode.Type().Recv().Type;
        recvType = deref(_addr_recvType);
        tparams = make_slice<ptr<types.Field>>(len(recvType.RParams()));
        {
            var i__prev1 = i;

            foreach (var (__i, __rparam) in recvType.RParams()) {
                i = __i;
                rparam = __rparam;
                tparams[i] = types.NewField(src.NoXPos, null, rparam);
            }
    else

            i = i__prev1;
        }
    } {
        tparams = nameNode.Type().TParams().Fields().Slice();
    }
    var gf = nameNode.Func; 
    // Pos of the instantiated function is same as the generic function
    var newf = ir.NewFunc(gf.Pos());
    newf.Pragma = gf.Pragma; // copy over pragmas from generic function to stenciled implementation.
    newf.Nname = ir.NewNameAt(gf.Pos(), newsym);
    newf.Nname.Func = newf;
    newf.Nname.Defn = newf;
    newsym.Def = newf.Nname;
    var savef = ir.CurFunc; 
    // transformCall/transformReturn (called during stenciling of the body)
    // depend on ir.CurFunc being set.
    ir.CurFunc = newf;

    assert(len(tparams) == len(targs));

    ptr<subster> subst = addr(new subster(g:g,isMethod:isMethod,newf:newf,tparams:tparams,targs:targs,vars:make(map[*ir.Name]*ir.Name),));

    newf.Dcl = make_slice<ptr<ir.Name>>(len(gf.Dcl));
    {
        var i__prev1 = i;

        foreach (var (__i, __n) in gf.Dcl) {
            i = __i;
            n = __n;
            newf.Dcl[i] = subst.node(n)._<ptr<ir.Name>>();
        }
        i = i__prev1;
    }

    var oldt = nameNode.Type(); 
    // We also transform a generic method type to the corresponding
    // instantiated function type where the receiver is the first parameter.
    var newt = types.NewSignature(oldt.Pkg(), null, null, subst.fields(ir.PPARAM, append(oldt.Recvs().FieldSlice(), oldt.Params().FieldSlice()), newf.Dcl), subst.fields(ir.PPARAMOUT, oldt.Results().FieldSlice(), newf.Dcl));

    newf.Nname.SetType(newt);
    ir.MarkFunc(newf.Nname);
    newf.SetTypecheck(1);
    newf.Nname.SetTypecheck(1); 

    // Make sure name/type of newf is set before substituting the body.
    newf.Body = subst.list(gf.Body);
    ir.CurFunc = savef;

    return _addr_newf!;
}

// node is like DeepCopy(), but creates distinct ONAME nodes, and also descends
// into closures. It substitutes type arguments for type parameters in all the new
// nodes.
private static ir.Node node(this ptr<subster> _addr_subst, ir.Node n) {
    ref subster subst = ref _addr_subst.val;
 
    // Use closure to capture all state needed by the ir.EditChildren argument.
    Func<ir.Node, ir.Node> edit = default;
    edit = x => {

        if (x.Op() == ir.OTYPE) 
            return ir.TypeNode(subst.typ(x.Type()));
        else if (x.Op() == ir.ONAME) 
            ptr<ir.Name> name = x._<ptr<ir.Name>>();
            {
                var v = subst.vars[name];

                if (v != null) {
                    return v;
                }

            }
            var m = ir.NewNameAt(name.Pos(), name.Sym());
            if (name.IsClosureVar()) {
                m.SetIsClosureVar(true);
            }
            var t = x.Type();
            if (t == null) {
                assert(name.BuiltinOp != 0);
            }
            else
 {
                var newt = subst.typ(t);
                m.SetType(newt);
            }
            m.BuiltinOp = name.BuiltinOp;
            m.Curfn = subst.newf;
            m.Class = name.Class;
            m.Func = name.Func;
            subst.vars[name] = m;
            m.SetTypecheck(1);
            return m;
        else if (x.Op() == ir.OLITERAL || x.Op() == ir.ONIL) 
            if (x.Sym() != null) {
                return x;
            }
                m = ir.Copy(x);
        {
            ir.Expr (_, isExpr) = m._<ir.Expr>();

            if (isExpr) {
                t = x.Type();
                if (t == null) { 
                    // t can be nil only if this is a call that has no
                    // return values, so allow that and otherwise give
                    // an error.
                    ptr<ir.CallExpr> (_, isCallExpr) = m._<ptr<ir.CallExpr>>();
                    ptr<ir.StructKeyExpr> (_, isStructKeyExpr) = m._<ptr<ir.StructKeyExpr>>();
                    if (!isCallExpr && !isStructKeyExpr && x.Op() != ir.OPANIC && x.Op() != ir.OCLOSE) {
                        @base.Fatalf(fmt.Sprintf("Nil type for %v", x));
                    }
                }
                else if (x.Op() != ir.OCLOSURE) {
                    m.SetType(subst.typ(x.Type()));
                }
            }

        }
        ir.EditChildren(m, edit);

        if (x.Typecheck() == 3) { 
            // These are nodes whose transforms were delayed until
            // their instantiated type was known.
            m.SetTypecheck(1);
            if (typecheck.IsCmp(x.Op())) {
                transformCompare(m._<ptr<ir.BinaryExpr>>());
            }
            else
 {

                if (x.Op() == ir.OSLICE || x.Op() == ir.OSLICE3) 
                    transformSlice(m._<ptr<ir.SliceExpr>>());
                else if (x.Op() == ir.OADD) 
                    m = transformAdd(m._<ptr<ir.BinaryExpr>>());
                else if (x.Op() == ir.OINDEX) 
                    transformIndex(m._<ptr<ir.IndexExpr>>());
                else if (x.Op() == ir.OAS2) 
                    ptr<ir.AssignListStmt> as2 = m._<ptr<ir.AssignListStmt>>();
                    transformAssign(as2, as2.Lhs, as2.Rhs);
                else if (x.Op() == ir.OAS) 
                    ptr<ir.AssignStmt> @as = m._<ptr<ir.AssignStmt>>();
                    ir.Node lhs = new slice<ir.Node>(new ir.Node[] { as.X });
                    ir.Node rhs = new slice<ir.Node>(new ir.Node[] { as.Y });
                    transformAssign(as, lhs, rhs);
                else if (x.Op() == ir.OASOP) 
                    @as = m._<ptr<ir.AssignOpStmt>>();
                    transformCheckAssign(as, @as.X);
                else if (x.Op() == ir.ORETURN) 
                    transformReturn(m._<ptr<ir.ReturnStmt>>());
                else if (x.Op() == ir.OSEND) 
                    transformSend(m._<ptr<ir.SendStmt>>());
                else 
                    @base.Fatalf("Unexpected node with Typecheck() == 3");
                            }
        }

        if (x.Op() == ir.OLITERAL) 
            t = m.Type();
            if (t != x.Type()) { 
                // types2 will give us a constant with a type T,
                // if an untyped constant is used with another
                // operand of type T (in a provably correct way).
                // When we substitute in the type args during
                // stenciling, we now know the real type of the
                // constant. We may then need to change the
                // BasicLit.val to be the correct type (e.g.
                // convert an int64Val constant to a floatVal
                // constant).
                m.SetType(types.UntypedInt); // use any untyped type for DefaultLit to work
                m = typecheck.DefaultLit(m, t);
            }
        else if (x.Op() == ir.OXDOT) 
            // A method value/call via a type param will have been
            // left as an OXDOT. When we see this during stenciling,
            // finish the transformation, now that we have the
            // instantiated receiver type. We need to do this now,
            // since the access/selection to the method for the real
            // type is very different from the selection for the type
            // param. m will be transformed to an OCALLPART node. It
            // will be transformed to an ODOTMETH or ODOTINTER node if
            // we find in the OCALL case below that the method value
            // is actually called.
            transformDot(m._<ptr<ir.SelectorExpr>>(), false);
            m.SetTypecheck(1);
        else if (x.Op() == ir.OCALL) 
            ptr<ir.CallExpr> call = m._<ptr<ir.CallExpr>>();

            if (call.X.Op() == ir.OTYPE) 
                // Transform the conversion, now that we know the
                // type argument.
                m = transformConvCall(m._<ptr<ir.CallExpr>>());
            else if (call.X.Op() == ir.OCALLPART) 
                // Redo the transformation of OXDOT, now that we
                // know the method value is being called. Then
                // transform the call.
                call.X._<ptr<ir.SelectorExpr>>().SetOp(ir.OXDOT);
                transformDot(call.X._<ptr<ir.SelectorExpr>>(), true);
                transformCall(call);
            else if (call.X.Op() == ir.ODOT || call.X.Op() == ir.ODOTPTR) 
                // An OXDOT for a generic receiver was resolved to
                // an access to a field which has a function
                // value. Transform the call to that function, now
                // that the OXDOT was resolved.
                transformCall(call);
            else if (call.X.Op() == ir.ONAME) 
                name = call.X.Name();
                if (name.BuiltinOp != ir.OXXX) {

                    if (name.BuiltinOp == ir.OMAKE || name.BuiltinOp == ir.OREAL || name.BuiltinOp == ir.OIMAG || name.BuiltinOp == ir.OLEN || name.BuiltinOp == ir.OCAP || name.BuiltinOp == ir.OAPPEND) 
                        // Transform these builtins now that we
                        // know the type of the args.
                        m = transformBuiltin(call);
                    else 
                        @base.FatalfAt(call.Pos(), "Unexpected builtin op");
                                    }
                else
 { 
                    // This is the case of a function value that was a
                    // type parameter (implied to be a function via a
                    // structural constraint) which is now resolved.
                    transformCall(call);
                }
            else if (call.X.Op() == ir.OCLOSURE) 
                transformCall(call);
            else if (call.X.Op() == ir.OFUNCINST)             else 
                @base.FatalfAt(call.Pos(), fmt.Sprintf("Unexpected op with CALL during stenciling: %v", call.X.Op()));
                    else if (x.Op() == ir.OCLOSURE) 
            ptr<ir.ClosureExpr> x = x._<ptr<ir.ClosureExpr>>(); 
            // Need to duplicate x.Func.Nname, x.Func.Dcl, x.Func.ClosureVars, and
            // x.Func.Body.
            var oldfn = x.Func;
            var newfn = ir.NewFunc(oldfn.Pos());
            if (oldfn.ClosureCalled()) {
                newfn.SetClosureCalled(true);
            }
            newfn.SetIsHiddenClosure(true);
            m._<ptr<ir.ClosureExpr>>().Func = newfn; 
            // Closure name can already have brackets, if it derives
            // from a generic method
            var newsym = makeInstName(_addr_oldfn.Nname.Sym(), subst.targs, subst.isMethod);
            newfn.Nname = ir.NewNameAt(oldfn.Nname.Pos(), newsym);
            newfn.Nname.Func = newfn;
            newfn.Nname.Defn = newfn;
            ir.MarkFunc(newfn.Nname);
            newfn.OClosure = m._<ptr<ir.ClosureExpr>>();

            var saveNewf = subst.newf;
            ir.CurFunc = newfn;
            subst.newf = newfn;
            newfn.Dcl = subst.namelist(oldfn.Dcl);
            newfn.ClosureVars = subst.namelist(oldfn.ClosureVars);

            typed(subst.typ(oldfn.Nname.Type()), newfn.Nname);
            typed(newfn.Nname.Type(), m);
            newfn.SetTypecheck(1); 

            // Make sure type of closure function is set before doing body.
            newfn.Body = subst.list(oldfn.Body);
            subst.newf = saveNewf;
            ir.CurFunc = saveNewf;

            subst.g.target.Decls = append(subst.g.target.Decls, newfn);
                return m;
    };

    return edit(n);
}

private static slice<ptr<ir.Name>> namelist(this ptr<subster> _addr_subst, slice<ptr<ir.Name>> l) {
    ref subster subst = ref _addr_subst.val;

    var s = make_slice<ptr<ir.Name>>(len(l));
    foreach (var (i, n) in l) {
        s[i] = subst.node(n)._<ptr<ir.Name>>();
        if (n.Defn != null) {
            s[i].Defn = subst.node(n.Defn);
        }
        if (n.Outer != null) {
            s[i].Outer = subst.node(n.Outer)._<ptr<ir.Name>>();
        }
    }    return s;
}

private static slice<ir.Node> list(this ptr<subster> _addr_subst, slice<ir.Node> l) {
    ref subster subst = ref _addr_subst.val;

    var s = make_slice<ir.Node>(len(l));
    foreach (var (i, n) in l) {
        s[i] = subst.node(n);
    }    return s;
}

// tstruct substitutes type params in types of the fields of a structure type. For
// each field, if Nname is set, tstruct also translates the Nname using
// subst.vars, if Nname is in subst.vars. To always force the creation of a new
// (top-level) struct, regardless of whether anything changed with the types or
// names of the struct's fields, set force to true.
private static ptr<types.Type> tstruct(this ptr<subster> _addr_subst, ptr<types.Type> _addr_t, bool force) {
    ref subster subst = ref _addr_subst.val;
    ref types.Type t = ref _addr_t.val;

    if (t.NumFields() == 0) {
        if (t.HasTParam()) { 
            // For an empty struct, we need to return a new type,
            // since it may now be fully instantiated (HasTParam
            // becomes false).
            return _addr_types.NewStruct(t.Pkg(), null)!;
        }
        return _addr_t!;
    }
    slice<ptr<types.Field>> newfields = default;
    if (force) {
        newfields = make_slice<ptr<types.Field>>(t.NumFields());
    }
    foreach (var (i, f) in t.Fields().Slice()) {
        var t2 = subst.typ(f.Type);
        if ((t2 != f.Type || f.Nname != null) && newfields == null) {
            newfields = make_slice<ptr<types.Field>>(t.NumFields());
            for (nint j = 0; j < i; j++) {
                newfields[j] = t.Field(j);
            }
        }
        if (newfields != null) { 
            // TODO(danscales): make sure this works for the field
            // names of embedded types (which should keep the name of
            // the type param, not the instantiated type).
            newfields[i] = types.NewField(f.Pos, f.Sym, t2);
            if (f.Nname != null) { 
                // f.Nname may not be in subst.vars[] if this is
                // a function name or a function instantiation type
                // that we are translating
                var v = subst.vars[f.Nname._<ptr<ir.Name>>()]; 
                // Be careful not to put a nil var into Nname,
                // since Nname is an interface, so it would be a
                // non-nil interface.
                if (v != null) {
                    newfields[i].Nname = v;
                }
            }
        }
    }    if (newfields != null) {
        return _addr_types.NewStruct(t.Pkg(), newfields)!;
    }
    return _addr_t!;
}

// tinter substitutes type params in types of the methods of an interface type.
private static ptr<types.Type> tinter(this ptr<subster> _addr_subst, ptr<types.Type> _addr_t) {
    ref subster subst = ref _addr_subst.val;
    ref types.Type t = ref _addr_t.val;

    if (t.Methods().Len() == 0) {
        return _addr_t!;
    }
    slice<ptr<types.Field>> newfields = default;
    foreach (var (i, f) in t.Methods().Slice()) {
        var t2 = subst.typ(f.Type);
        if ((t2 != f.Type || f.Nname != null) && newfields == null) {
            newfields = make_slice<ptr<types.Field>>(t.Methods().Len());
            for (nint j = 0; j < i; j++) {
                newfields[j] = t.Methods().Index(j);
            }
        }
        if (newfields != null) {
            newfields[i] = types.NewField(f.Pos, f.Sym, t2);
        }
    }    if (newfields != null) {
        return _addr_types.NewInterface(t.Pkg(), newfields)!;
    }
    return _addr_t!;
}

// instTypeName creates a name for an instantiated type, based on the name of the
// generic type and the type args
private static @string instTypeName(@string name, slice<ptr<types.Type>> targs) {
    var b = bytes.NewBufferString(name);
    b.WriteByte('[');
    foreach (var (i, targ) in targs) {
        if (i > 0) {
            b.WriteByte(',');
        }
        b.WriteString(targ.String());
    }    b.WriteByte(']');
    return b.String();
}

// typ computes the type obtained by substituting any type parameter in t with the
// corresponding type argument in subst. If t contains no type parameters, the
// result is t; otherwise the result is a new type. It deals with recursive types
// by using TFORW types and finding partially or fully created types via sym.Def.
private static ptr<types.Type> typ(this ptr<subster> _addr_subst, ptr<types.Type> _addr_t) {
    ref subster subst = ref _addr_subst.val;
    ref types.Type t = ref _addr_t.val;

    if (!t.HasTParam() && t.Kind() != types.TFUNC) { 
        // Note: function types need to be copied regardless, as the
        // types of closures may contain declarations that need
        // to be copied. See #45738.
        return _addr_t!;
    }
    if (t.Kind() == types.TTYPEPARAM) {
        {
            var i__prev1 = i;

            foreach (var (__i, __tp) in subst.tparams) {
                i = __i;
                tp = __tp;
                if (tp.Type == t) {
                    return _addr_subst.targs[i].Type()!;
                }
            } 
            // If t is a simple typeparam T, then t has the name/symbol 'T'
            // and t.Underlying() == t.
            //
            // However, consider the type definition: 'type P[T any] T'. We
            // might use this definition so we can have a variant of type T
            // that we can add new methods to. Suppose t is a reference to
            // P[T]. t has the name 'P[T]', but its kind is TTYPEPARAM,
            // because P[T] is defined as T. If we look at t.Underlying(), it
            // is different, because the name of t.Underlying() is 'T' rather
            // than 'P[T]'. But the kind of t.Underlying() is also TTYPEPARAM.
            // In this case, we do the needed recursive substitution in the
            // case statement below.

            i = i__prev1;
        }

        if (t.Underlying() == t) { 
            // t is a simple typeparam that didn't match anything in tparam
            return _addr_t!;
        }
        assert(t.Sym() != null);
    }
    ptr<types.Sym> newsym;
    slice<ptr<types.Type>> neededTargs = default;
    ptr<types.Type> forw;

    if (t.Sym() != null) { 
        // Translate the type params for this type according to
        // the tparam/targs mapping from subst.
        neededTargs = make_slice<ptr<types.Type>>(len(t.RParams()));
        {
            var i__prev1 = i;

            foreach (var (__i, __rparam) in t.RParams()) {
                i = __i;
                rparam = __rparam;
                neededTargs[i] = subst.typ(rparam);
            } 
            // For a named (defined) type, we have to change the name of the
            // type as well. We do this first, so we can look up if we've
            // already seen this type during this substitution or other
            // definitions/substitutions.

            i = i__prev1;
        }

        var genName = genericTypeName(_addr_t.Sym());
        newsym = t.Sym().Pkg.Lookup(instTypeName(genName, neededTargs));
        if (newsym.Def != null) { 
            // We've already created this instantiated defined type.
            return _addr_newsym.Def.Type()!;
        }
        forw = newIncompleteNamedType(t.Pos(), newsym); 
        //println("Creating new type by sub", newsym.Name, forw.HasTParam())
        forw.SetRParams(neededTargs);
    }
    ptr<types.Type> newt;


    if (t.Kind() == types.TTYPEPARAM) 
        if (t.Sym() == newsym) { 
            // The substitution did not change the type.
            return _addr_t!;
        }
        newt = subst.typ(t.Underlying());
        assert(newt != t);
    else if (t.Kind() == types.TARRAY) 
        var elem = t.Elem();
        var newelem = subst.typ(elem);
        if (newelem != elem) {
            newt = types.NewArray(newelem, t.NumElem());
        }
    else if (t.Kind() == types.TPTR) 
        elem = t.Elem();
        newelem = subst.typ(elem);
        if (newelem != elem) {
            newt = types.NewPtr(newelem);
        }
    else if (t.Kind() == types.TSLICE) 
        elem = t.Elem();
        newelem = subst.typ(elem);
        if (newelem != elem) {
            newt = types.NewSlice(newelem);
        }
    else if (t.Kind() == types.TSTRUCT) 
        newt = subst.tstruct(t, false);
        if (newt == t) {
            newt = null;
        }
    else if (t.Kind() == types.TFUNC) 
        var newrecvs = subst.tstruct(t.Recvs(), false);
        var newparams = subst.tstruct(t.Params(), false);
        var newresults = subst.tstruct(t.Results(), false);
        if (newrecvs != t.Recvs() || newparams != t.Params() || newresults != t.Results()) { 
            // If any types have changed, then the all the fields of
            // of recv, params, and results must be copied, because they have
            // offset fields that are dependent, and so must have an
            // independent copy for each new signature.
            ptr<types.Field> newrecv;
            if (newrecvs.NumFields() > 0) {
                if (newrecvs == t.Recvs()) {
                    newrecvs = subst.tstruct(t.Recvs(), true);
                }
                newrecv = newrecvs.Field(0);
            }
            if (newparams == t.Params()) {
                newparams = subst.tstruct(t.Params(), true);
            }
            if (newresults == t.Results()) {
                newresults = subst.tstruct(t.Results(), true);
            }
            newt = types.NewSignature(t.Pkg(), newrecv, t.TParams().FieldSlice(), newparams.FieldSlice(), newresults.FieldSlice());
        }
    else if (t.Kind() == types.TINTER) 
        newt = subst.tinter(t);
        if (newt == t) {
            newt = null;
        }
    else if (t.Kind() == types.TMAP) 
        var newkey = subst.typ(t.Key());
        var newval = subst.typ(t.Elem());
        if (newkey != t.Key() || newval != t.Elem()) {
            newt = types.NewMap(newkey, newval);
        }
    else if (t.Kind() == types.TCHAN) 
        elem = t.Elem();
        newelem = subst.typ(elem);
        if (newelem != elem) {
            newt = types.NewChan(newelem, t.ChanDir());
            if (!newt.HasTParam()) { 
                // TODO(danscales): not sure why I have to do this
                // only for channels.....
                types.CheckSize(newt);
            }
        }
        if (newt == null) { 
        // Even though there were typeparams in the type, there may be no
        // change if this is a function type for a function call (which will
        // have its own tparams/targs in the function instantiation).
        return _addr_t!;
    }
    if (t.Sym() == null) { 
        // Not a named type, so there was no forwarding type and there are
        // no methods to substitute.
        assert(t.Methods().Len() == 0);
        return _addr_newt!;
    }
    forw.SetUnderlying(newt);
    newt = addr(forw);

    if (t.Kind() != types.TINTER && t.Methods().Len() > 0) { 
        // Fill in the method info for the new type.
        slice<ptr<types.Field>> newfields = default;
        newfields = make_slice<ptr<types.Field>>(t.Methods().Len());
        {
            var i__prev1 = i;

            foreach (var (__i, __f) in t.Methods().Slice()) {
                i = __i;
                f = __f;
                var t2 = subst.typ(f.Type);
                var oldsym = f.Nname.Sym();
                newsym = makeInstName(_addr_oldsym, subst.targs, true);
                ptr<ir.Name> nname;
                if (newsym.Def != null) {
                    nname = newsym.Def._<ptr<ir.Name>>();
                }
                else
 {
                    nname = ir.NewNameAt(f.Pos, newsym);
                    nname.SetType(t2);
                    newsym.Def = nname;
                }
                newfields[i] = types.NewField(f.Pos, f.Sym, t2);
                newfields[i].Nname = nname;
            }

            i = i__prev1;
        }

        newt.Methods().Set(newfields);
        if (!newt.HasTParam()) { 
            // Generate all the methods for a new fully-instantiated type.
            subst.g.instTypeList = append(subst.g.instTypeList, newt);
        }
    }
    return _addr_newt!;
}

// fields sets the Nname field for the Field nodes inside a type signature, based
// on the corresponding in/out parameters in dcl. It depends on the in and out
// parameters being in order in dcl.
private static slice<ptr<types.Field>> fields(this ptr<subster> _addr_subst, ir.Class @class, slice<ptr<types.Field>> oldfields, slice<ptr<ir.Name>> dcl) {
    ref subster subst = ref _addr_subst.val;
 
    // Find the starting index in dcl of declarations of the class (either
    // PPARAM or PPARAMOUT).
    nint i = default;
    foreach (var (__i) in dcl) {
        i = __i;
        if (dcl[i].Class == class) {
            break;
        }
    }
    var newfields = make_slice<ptr<types.Field>>(len(oldfields));
    foreach (var (j) in oldfields) {
        newfields[j] = oldfields[j].Copy();
        newfields[j].Type = subst.typ(oldfields[j].Type); 
        // A param field will be missing from dcl if its name is
        // unspecified or specified as "_". So, we compare the dcl sym
        // with the field sym. If they don't match, this dcl (if there is
        // one left) must apply to a later field.
        if (i < len(dcl) && dcl[i].Sym() == oldfields[j].Sym) {
            newfields[j].Nname = dcl[i];
            i++;
        }
    }    return newfields;
}

// defer does a single defer of type t, if it is a pointer type.
private static ptr<types.Type> deref(ptr<types.Type> _addr_t) {
    ref types.Type t = ref _addr_t.val;

    if (t.IsPtr()) {
        return _addr_t.Elem()!;
    }
    return _addr_t!;
}

// newIncompleteNamedType returns a TFORW type t with name specified by sym, such
// that t.nod and sym.Def are set correctly.
private static ptr<types.Type> newIncompleteNamedType(src.XPos pos, ptr<types.Sym> _addr_sym) {
    ref types.Sym sym = ref _addr_sym.val;

    var name = ir.NewDeclNameAt(pos, ir.OTYPE, sym);
    var forw = types.NewNamed(name);
    name.SetType(forw);
    sym.Def = name;
    return _addr_forw!;
}

} // end noder_package
