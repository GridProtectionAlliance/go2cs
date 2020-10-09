// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package ssa -- go2cs converted at 2020 October 09 06:03:23 UTC
// import "golang.org/x/tools/go/ssa" ==> using ssa = go.golang.org.x.tools.go.ssa_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\ssa\lvalue.go
// lvalues are the union of addressable expressions and map-index
// expressions.

using ast = go.go.ast_package;
using token = go.go.token_package;
using types = go.go.types_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go
{
    public static partial class ssa_package
    {
        // An lvalue represents an assignable location that may appear on the
        // left-hand side of an assignment.  This is a generalization of a
        // pointer to permit updates to elements of maps.
        //
        private partial interface lvalue
        {
            types.Type store(ptr<Function> fn, Value v); // stores v into the location
            types.Type load(ptr<Function> fn); // loads the contents of the location
            types.Type address(ptr<Function> fn); // address of the location
            types.Type typ(); // returns the type of the location
        }

        // An address is an lvalue represented by a true pointer.
        private partial struct address
        {
            public Value addr;
            public token.Pos pos; // source position
            public ast.Expr expr; // source syntax of the value (not address) [debug mode]
        }

        private static Value load(this ptr<address> _addr_a, ptr<Function> _addr_fn)
        {
            ref address a = ref _addr_a.val;
            ref Function fn = ref _addr_fn.val;

            var load = emitLoad(fn, a.addr);
            load.pos = a.pos;
            return load;
        }

        private static void store(this ptr<address> _addr_a, ptr<Function> _addr_fn, Value v)
        {
            ref address a = ref _addr_a.val;
            ref Function fn = ref _addr_fn.val;

            var store = emitStore(fn, a.addr, v, a.pos);
            if (a.expr != null)
            { 
                // store.Val is v, converted for assignability.
                emitDebugRef(fn, a.expr, store.Val, false);

            }

        }

        private static Value address(this ptr<address> _addr_a, ptr<Function> _addr_fn)
        {
            ref address a = ref _addr_a.val;
            ref Function fn = ref _addr_fn.val;

            if (a.expr != null)
            {
                emitDebugRef(fn, a.expr, a.addr, true);
            }

            return a.addr;

        }

        private static types.Type typ(this ptr<address> _addr_a)
        {
            ref address a = ref _addr_a.val;

            return deref(a.addr.Type());
        }

        // An element is an lvalue represented by m[k], the location of an
        // element of a map or string.  These locations are not addressable
        // since pointers cannot be formed from them, but they do support
        // load(), and in the case of maps, store().
        //
        private partial struct element
        {
            public Value m; // map or string
            public Value k; // map or string
            public types.Type t; // map element type or string byte type
            public token.Pos pos; // source position of colon ({k:v}) or lbrack (m[k]=v)
        }

        private static Value load(this ptr<element> _addr_e, ptr<Function> _addr_fn)
        {
            ref element e = ref _addr_e.val;
            ref Function fn = ref _addr_fn.val;

            ptr<Lookup> l = addr(new Lookup(X:e.m,Index:e.k,));
            l.setPos(e.pos);
            l.setType(e.t);
            return fn.emit(l);
        }

        private static void store(this ptr<element> _addr_e, ptr<Function> _addr_fn, Value v)
        {
            ref element e = ref _addr_e.val;
            ref Function fn = ref _addr_fn.val;

            ptr<MapUpdate> up = addr(new MapUpdate(Map:e.m,Key:e.k,Value:emitConv(fn,v,e.t),));
            up.pos = e.pos;
            fn.emit(up);
        }

        private static Value address(this ptr<element> _addr_e, ptr<Function> _addr_fn) => func((_, panic, __) =>
        {
            ref element e = ref _addr_e.val;
            ref Function fn = ref _addr_fn.val;

            panic("map/string elements are not addressable");
        });

        private static types.Type typ(this ptr<element> _addr_e)
        {
            ref element e = ref _addr_e.val;

            return e.t;
        }

        // A blank is a dummy variable whose name is "_".
        // It is not reified: loads are illegal and stores are ignored.
        //
        private partial struct blank
        {
        }

        private static Value load(this blank bl, ptr<Function> _addr_fn) => func((_, panic, __) =>
        {
            ref Function fn = ref _addr_fn.val;

            panic("blank.load is illegal");
        });

        private static void store(this blank bl, ptr<Function> _addr_fn, Value v)
        {
            ref Function fn = ref _addr_fn.val;
 
            // no-op
        }

        private static Value address(this blank bl, ptr<Function> _addr_fn) => func((_, panic, __) =>
        {
            ref Function fn = ref _addr_fn.val;

            panic("blank var is not addressable");
        });

        private static types.Type typ(this blank bl) => func((_, panic, __) =>
        { 
            // This should be the type of the blank Ident; the typechecker
            // doesn't provide this yet, but fortunately, we don't need it
            // yet either.
            panic("blank.typ is unimplemented");

        });
    }
}}}}}
