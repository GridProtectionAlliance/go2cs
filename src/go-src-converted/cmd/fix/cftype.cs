// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package main -- go2cs converted at 2020 October 08 04:33:13 UTC
// Original source: C:\Go\src\cmd\fix\cftype.go
using ast = go.go.ast_package;
using token = go.go.token_package;
using reflect = go.reflect_package;
using strings = go.strings_package;
using static go.builtin;
using System;

namespace go
{
    public static partial class main_package
    {
        private static void init()
        {
            register(cftypeFix);
        }

        private static fix cftypeFix = new fix(name:"cftype",date:"2017-09-27",f:cftypefix,desc:`Fixes initializers and casts of C.*Ref and JNI types`,disabled:false,);

        // Old state:
        //   type CFTypeRef unsafe.Pointer
        // New state:
        //   type CFTypeRef uintptr
        // and similar for other *Ref types.
        // This fix finds nils initializing these types and replaces the nils with 0s.
        private static bool cftypefix(ptr<ast.File> _addr_f)
        {
            ref ast.File f = ref _addr_f.val;

            return typefix(_addr_f, s =>
            {
                return strings.HasPrefix(s, "C.") && strings.HasSuffix(s, "Ref") && s != "C.CFAllocatorRef";
            });

        }

        // typefix replaces nil with 0 for all nils whose type, when passed to badType, returns true.
        private static bool typefix(ptr<ast.File> _addr_f, Func<@string, bool> badType)
        {
            ref ast.File f = ref _addr_f.val;

            if (!imports(f, "C"))
            {
                return false;
            }

            var (typeof, _) = typecheck(addr(new TypeConfig()), f);
            var changed = false; 

            // step 1: Find all the nils with the offending types.
            // Compute their replacement.
            walk(f, n =>
            {
                {
                    ptr<ast.Ident> i__prev1 = i;

                    ptr<ast.Ident> (i, ok) = n._<ptr<ast.Ident>>();

                    if (ok && i.Name == "nil" && badType(typeof[n]))
                    {
                        badNils[n] = addr(new ast.BasicLit(ValuePos:i.NamePos,Kind:token.INT,Value:"0"));
                    }

                    i = i__prev1;

                }

            }); 

            // step 2: find all uses of the bad nils, replace them with 0.
            // There's no easy way to map from an ast.Expr to all the places that use them, so
            // we use reflect to find all such references.
            if (len(badNils) > 0L)
            {
                var exprType = reflect.TypeOf((ast.Expr.val)(null)).Elem();
                var exprSliceType = reflect.TypeOf((slice<ast.Expr>)null);
                walk(f, n =>
                {
                    if (n == null)
                    {
                        return ;
                    }

                    var v = reflect.ValueOf(n);
                    if (v.Type().Kind() != reflect.Ptr)
                    {
                        return ;
                    }

                    if (v.IsNil())
                    {
                        return ;
                    }

                    v = v.Elem();
                    if (v.Type().Kind() != reflect.Struct)
                    {
                        return ;
                    }

                    {
                        ptr<ast.Ident> i__prev1 = i;

                        for (long i = 0L; i < v.NumField(); i++)
                        {
                            var f = v.Field(i);
                            if (f.Type() == exprType)
                            {
                                {
                                    var r__prev3 = r;

                                    var r = badNils[f.Interface()];

                                    if (r != null)
                                    {
                                        f.Set(reflect.ValueOf(r));
                                        changed = true;
                                    }

                                    r = r__prev3;

                                }

                            }

                            if (f.Type() == exprSliceType)
                            {
                                for (long j = 0L; j < f.Len(); j++)
                                {
                                    var e = f.Index(j);
                                    {
                                        var r__prev3 = r;

                                        r = badNils[e.Interface()];

                                        if (r != null)
                                        {
                                            e.Set(reflect.ValueOf(r));
                                            changed = true;
                                        }

                                        r = r__prev3;

                                    }

                                }


                            }

                        }


                        i = i__prev1;
                    }

                });

            } 

            // step 3: fix up invalid casts.
            // It used to be ok to cast between *unsafe.Pointer and *C.CFTypeRef in a single step.
            // Now we need unsafe.Pointer as an intermediate cast.
            // (*unsafe.Pointer)(x) where x is type *bad -> (*unsafe.Pointer)(unsafe.Pointer(x))
            // (*bad.type)(x) where x is type *unsafe.Pointer -> (*bad.type)(unsafe.Pointer(x))
            walk(f, n =>
            {
                if (n == null)
                {
                    return ;
                } 
                // Find pattern like (*a.b)(x)
                ptr<ast.CallExpr> (c, ok) = n._<ptr<ast.CallExpr>>();
                if (!ok)
                {
                    return ;
                }

                if (len(c.Args) != 1L)
                {
                    return ;
                }

                ptr<ast.ParenExpr> (p, ok) = c.Fun._<ptr<ast.ParenExpr>>();
                if (!ok)
                {
                    return ;
                }

                ptr<ast.StarExpr> (s, ok) = p.X._<ptr<ast.StarExpr>>();
                if (!ok)
                {
                    return ;
                }

                ptr<ast.SelectorExpr> (t, ok) = s.X._<ptr<ast.SelectorExpr>>();
                if (!ok)
                {
                    return ;
                }

                ptr<ast.Ident> (pkg, ok) = t.X._<ptr<ast.Ident>>();
                if (!ok)
                {
                    return ;
                }

                var dst = pkg.Name + "." + t.Sel.Name;
                var src = typeof[c.Args[0L]];
                if (badType(dst) && src == "*unsafe.Pointer" || dst == "unsafe.Pointer" && strings.HasPrefix(src, "*") && badType(src[1L..]))
                {
                    c.Args[0L] = addr(new ast.CallExpr(Fun:&ast.SelectorExpr{X:&ast.Ident{Name:"unsafe"},Sel:&ast.Ident{Name:"Pointer"}},Args:[]ast.Expr{c.Args[0]},));
                    changed = true;
                }

            });

            return changed;

        }
    }
}
