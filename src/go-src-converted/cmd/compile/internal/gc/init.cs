// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 October 09 05:41:42 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\init.go
using types = go.cmd.compile.@internal.types_package;
using obj = go.cmd.@internal.obj_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // A function named init is a special case.
        // It is called by the initialization before main is run.
        // To make it unique within a package and also uncallable,
        // the name, normally "pkg.init", is altered to "pkg.init.0".
        private static long renameinitgen = default;

        // Dummy function for autotmps generated during typechecking.
        private static var dummyInitFn = nod(ODCLFUNC, null, null);

        private static ptr<types.Sym> renameinit()
        {
            var s = lookupN("init.", renameinitgen);
            renameinitgen++;
            return _addr_s!;
        }

        // fninit makes an initialization record for the package.
        // See runtime/proc.go:initTask for its layout.
        // The 3 tasks for initialization are:
        //   1) Initialize all of the packages the current package depends on.
        //   2) Initialize all the variables that have initializers.
        //   3) Run any init functions.
        private static void fninit(slice<ptr<Node>> n)
        {
            var nf = initOrder(n);

            slice<ptr<obj.LSym>> deps = default; // initTask records for packages the current package depends on
            slice<ptr<obj.LSym>> fns = default; // functions to call for package initialization

            // Find imported packages with init tasks.
            {
                var s__prev1 = s;

                foreach (var (_, __s) in types.InitSyms)
                {
                    s = __s;
                    deps = append(deps, s.Linksym());
                } 

                // Make a function that contains all the initialization statements.

                s = s__prev1;
            }

            if (len(nf) > 0L)
            {
                lineno = nf[0L].Pos; // prolog/epilog gets line number of first init stmt
                var initializers = lookup("init");
                disableExport(initializers);
                var fn = dclfunc(initializers, nod(OTFUNC, null, null));
                foreach (var (_, dcl) in dummyInitFn.Func.Dcl)
                {
                    dcl.Name.Curfn = fn;
                }
                fn.Func.Dcl = append(fn.Func.Dcl, dummyInitFn.Func.Dcl);
                dummyInitFn.Func.Dcl = null;

                fn.Nbody.Set(nf);
                funcbody();

                fn = typecheck(fn, ctxStmt);
                Curfn = fn;
                typecheckslice(nf, ctxStmt);
                Curfn = null;
                funccompile(fn);
                fns = append(fns, initializers.Linksym());

            }

            if (dummyInitFn.Func.Dcl != null)
            { 
                // We only generate temps using dummyInitFn if there
                // are package-scope initialization statements, so
                // something's weird if we get here.
                Fatalf("dummyInitFn still has declarations");

            } 

            // Record user init functions.
            for (long i = 0L; i < renameinitgen; i++)
            {
                var s = lookupN("init.", i);
                fn = asNode(s.Def).Name.Defn; 
                // Skip init functions with empty bodies.
                // noder.go doesn't allow external init functions, and
                // order.go has already removed any OEMPTY nodes, so
                // checking Len() == 0 is sufficient here.
                if (fn.Nbody.Len() == 0L)
                {
                    continue;
                }

                fns = append(fns, s.Linksym());

            }


            if (len(deps) == 0L && len(fns) == 0L && localpkg.Name != "main" && localpkg.Name != "runtime")
            {
                return ; // nothing to initialize
            } 

            // Make an .inittask structure.
            var sym = lookup(".inittask");
            var nn = newname(sym);
            nn.Type = types.Types[TUINT8]; // dummy type
            nn.SetClass(PEXTERN);
            sym.Def = asTypesNode(nn);
            exportsym(nn);
            var lsym = sym.Linksym();
            long ot = 0L;
            ot = duintptr(lsym, ot, 0L); // state: not initialized yet
            ot = duintptr(lsym, ot, uint64(len(deps)));
            ot = duintptr(lsym, ot, uint64(len(fns)));
            foreach (var (_, d) in deps)
            {
                ot = dsymptr(lsym, ot, d, 0L);
            }
            foreach (var (_, f) in fns)
            {
                ot = dsymptr(lsym, ot, f, 0L);
            } 
            // An initTask has pointers, but none into the Go heap.
            // It's not quite read only, the state field must be modifiable.
            ggloblsym(lsym, int32(ot), obj.NOPTR);

        }
    }
}}}}
