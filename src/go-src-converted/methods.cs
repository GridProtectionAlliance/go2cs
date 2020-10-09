// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 09 06:03:24 UTC
// import "golang.org/x/tools/go/ssa" ==> using ssa = go.golang.org.x.tools.go.ssa_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\methods.go
// This file defines utilities for population of method sets.

using fmt = go.fmt_package;
using types = go.go.types_package;
using static go.builtin;
using System;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go
{
    public static partial class ssa_package
    {
        // MethodValue returns the Function implementing method sel, building
        // wrapper methods on demand.  It returns nil if sel denotes an
        // abstract (interface) method.
        //
        // Precondition: sel.Kind() == MethodVal.
        //
        // Thread-safe.
        //
        // EXCLUSIVE_LOCKS_ACQUIRED(prog.methodsMu)
        //
        private static ptr<Function> MethodValue(this ptr<Program> _addr_prog, ptr<types.Selection> _addr_sel) => func((defer, panic, _) =>
        {
            ref Program prog = ref _addr_prog.val;
            ref types.Selection sel = ref _addr_sel.val;

            if (sel.Kind() != types.MethodVal)
            {
                panic(fmt.Sprintf("MethodValue(%s) kind != MethodVal", sel));
            }
            var T = sel.Recv();
            if (isInterface(T))
            {
                return _addr_null!; // abstract method
            }
            if (prog.mode & LogSource != 0L)
            {
                defer(logStack("MethodValue %s %v", T, sel)());
            }
            prog.methodsMu.Lock();
            defer(prog.methodsMu.Unlock());

            return _addr_prog.addMethod(prog.createMethodSet(T), sel)!;

        });

        // LookupMethod returns the implementation of the method of type T
        // identified by (pkg, name).  It returns nil if the method exists but
        // is abstract, and panics if T has no such method.
        //
        private static ptr<Function> LookupMethod(this ptr<Program> _addr_prog, types.Type T, ptr<types.Package> _addr_pkg, @string name) => func((_, panic, __) =>
        {
            ref Program prog = ref _addr_prog.val;
            ref types.Package pkg = ref _addr_pkg.val;

            var sel = prog.MethodSets.MethodSet(T).Lookup(pkg, name);
            if (sel == null)
            {
                panic(fmt.Sprintf("%s has no method %s", T, types.Id(pkg, name)));
            }

            return _addr_prog.MethodValue(sel)!;

        });

        // methodSet contains the (concrete) methods of a non-interface type.
        private partial struct methodSet
        {
            public map<@string, ptr<Function>> mapping; // populated lazily
            public bool complete; // mapping contains all methods
        }

        // Precondition: !isInterface(T).
        // EXCLUSIVE_LOCKS_REQUIRED(prog.methodsMu)
        private static ptr<methodSet> createMethodSet(this ptr<Program> _addr_prog, types.Type T)
        {
            ref Program prog = ref _addr_prog.val;

            ptr<methodSet> (mset, ok) = prog.methodSets.At(T)._<ptr<methodSet>>();
            if (!ok)
            {
                mset = addr(new methodSet(mapping:make(map[string]*Function)));
                prog.methodSets.Set(T, mset);
            }

            return _addr_mset!;

        }

        // EXCLUSIVE_LOCKS_REQUIRED(prog.methodsMu)
        private static ptr<Function> addMethod(this ptr<Program> _addr_prog, ptr<methodSet> _addr_mset, ptr<types.Selection> _addr_sel) => func((_, panic, __) =>
        {
            ref Program prog = ref _addr_prog.val;
            ref methodSet mset = ref _addr_mset.val;
            ref types.Selection sel = ref _addr_sel.val;

            if (sel.Kind() == types.MethodExpr)
            {
                panic(sel);
            }

            var id = sel.Obj().Id();
            var fn = mset.mapping[id];
            if (fn == null)
            {
                ptr<types.Func> obj = sel.Obj()._<ptr<types.Func>>();

                var needsPromotion = len(sel.Index()) > 1L;
                var needsIndirection = !isPointer(recvType(obj)) && isPointer(sel.Recv());
                if (needsPromotion || needsIndirection)
                {
                    fn = makeWrapper(prog, sel);
                }
                else
                {
                    fn = prog.declaredFunc(obj);
                }

                if (fn.Signature.Recv() == null)
                {
                    panic(fn); // missing receiver
                }

                mset.mapping[id] = fn;

            }

            return _addr_fn!;

        });

        // RuntimeTypes returns a new unordered slice containing all
        // concrete types in the program for which a complete (non-empty)
        // method set is required at run-time.
        //
        // Thread-safe.
        //
        // EXCLUSIVE_LOCKS_ACQUIRED(prog.methodsMu)
        //
        private static slice<types.Type> RuntimeTypes(this ptr<Program> _addr_prog) => func((defer, _, __) =>
        {
            ref Program prog = ref _addr_prog.val;

            prog.methodsMu.Lock();
            defer(prog.methodsMu.Unlock());

            slice<types.Type> res = default;
            prog.methodSets.Iterate((T, v) =>
            {
                if (v._<ptr<methodSet>>().complete)
                {
                    res = append(res, T);
                }

            });
            return res;

        });

        // declaredFunc returns the concrete function/method denoted by obj.
        // Panic ensues if there is none.
        //
        private static ptr<Function> declaredFunc(this ptr<Program> _addr_prog, ptr<types.Func> _addr_obj) => func((_, panic, __) =>
        {
            ref Program prog = ref _addr_prog.val;
            ref types.Func obj = ref _addr_obj.val;

            {
                var v = prog.packageLevelValue(obj);

                if (v != null)
                {
                    return v._<ptr<Function>>();
                }

            }

            panic("no concrete method: " + obj.String());

        });

        // needMethodsOf ensures that runtime type information (including the
        // complete method set) is available for the specified type T and all
        // its subcomponents.
        //
        // needMethodsOf must be called for at least every type that is an
        // operand of some MakeInterface instruction, and for the type of
        // every exported package member.
        //
        // Precondition: T is not a method signature (*Signature with Recv()!=nil).
        //
        // Thread-safe.  (Called via emitConv from multiple builder goroutines.)
        //
        // TODO(adonovan): make this faster.  It accounts for 20% of SSA build time.
        //
        // EXCLUSIVE_LOCKS_ACQUIRED(prog.methodsMu)
        //
        private static void needMethodsOf(this ptr<Program> _addr_prog, types.Type T)
        {
            ref Program prog = ref _addr_prog.val;

            prog.methodsMu.Lock();
            prog.needMethods(T, false);
            prog.methodsMu.Unlock();
        }

        // Precondition: T is not a method signature (*Signature with Recv()!=nil).
        // Recursive case: skip => don't create methods for T.
        //
        // EXCLUSIVE_LOCKS_REQUIRED(prog.methodsMu)
        //
        private static void needMethods(this ptr<Program> _addr_prog, types.Type T, bool skip) => func((_, panic, __) =>
        {
            ref Program prog = ref _addr_prog.val;
 
            // Each package maintains its own set of types it has visited.
            {
                bool (prevSkip, ok) = prog.runtimeTypes.At(T)._<bool>();

                if (ok)
                { 
                    // needMethods(T) was previously called
                    if (!prevSkip || skip)
                    {
                        return ; // already seen, with same or false 'skip' value
                    }

                }

            }

            prog.runtimeTypes.Set(T, skip);

            var tmset = prog.MethodSets.MethodSet(T);

            if (!skip && !isInterface(T) && tmset.Len() > 0L)
            { 
                // Create methods of T.
                var mset = prog.createMethodSet(T);
                if (!mset.complete)
                {
                    mset.complete = true;
                    var n = tmset.Len();
                    {
                        long i__prev1 = i;

                        for (long i = 0L; i < n; i++)
                        {
                            prog.addMethod(mset, tmset.At(i));
                        }


                        i = i__prev1;
                    }

                }

            } 

            // Recursion over signatures of each method.
            {
                long i__prev1 = i;

                for (i = 0L; i < tmset.Len(); i++)
                {
                    ptr<types.Signature> sig = tmset.At(i).Type()._<ptr<types.Signature>>();
                    prog.needMethods(sig.Params(), false);
                    prog.needMethods(sig.Results(), false);
                }


                i = i__prev1;
            }

            switch (T.type())
            {
                case ptr<types.Basic> t:
                    break;
                case ptr<types.Interface> t:
                    break;
                case ptr<types.Pointer> t:
                    prog.needMethods(t.Elem(), false);
                    break;
                case ptr<types.Slice> t:
                    prog.needMethods(t.Elem(), false);
                    break;
                case ptr<types.Chan> t:
                    prog.needMethods(t.Elem(), false);
                    break;
                case ptr<types.Map> t:
                    prog.needMethods(t.Key(), false);
                    prog.needMethods(t.Elem(), false);
                    break;
                case ptr<types.Signature> t:
                    if (t.Recv() != null)
                    {
                        panic(fmt.Sprintf("Signature %s has Recv %s", t, t.Recv()));
                    }

                    prog.needMethods(t.Params(), false);
                    prog.needMethods(t.Results(), false);
                    break;
                case ptr<types.Named> t:
                    prog.needMethods(types.NewPointer(T), false); 

                    // Consider 'type T struct{S}' where S has methods.
                    // Reflection provides no way to get from T to struct{S},
                    // only to S, so the method set of struct{S} is unwanted,
                    // so set 'skip' flag during recursion.
                    prog.needMethods(t.Underlying(), true);
                    break;
                case ptr<types.Array> t:
                    prog.needMethods(t.Elem(), false);
                    break;
                case ptr<types.Struct> t:
                    {
                        long i__prev1 = i;
                        var n__prev1 = n;

                        for (i = 0L;
                        n = t.NumFields(); i < n; i++)
                        {
                            prog.needMethods(t.Field(i).Type(), false);
                        }


                        i = i__prev1;
                        n = n__prev1;
                    }
                    break;
                case ptr<types.Tuple> t:
                    {
                        long i__prev1 = i;
                        var n__prev1 = n;

                        for (i = 0L;
                        n = t.Len(); i < n; i++)
                        {
                            prog.needMethods(t.At(i).Type(), false);
                        }


                        i = i__prev1;
                        n = n__prev1;
                    }
                    break;
                default:
                {
                    var t = T.type();
                    panic(T);
                    break;
                }
            }

        });
    }
}}}}}
