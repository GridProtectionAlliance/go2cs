// Copyright 2014 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements resolveOrder.

// package types -- go2cs converted at 2020 August 29 08:47:47 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\ordering.go
using ast = go.go.ast_package;
using sort = go.sort_package;
using static go.builtin;

namespace go {
namespace go
{
    public static partial class types_package
    {
        // resolveOrder computes the order in which package-level objects
        // must be type-checked.
        //
        // Interface types appear first in the list, sorted topologically
        // by dependencies on embedded interfaces that are also declared
        // in this package, followed by all other objects sorted in source
        // order.
        //
        // TODO(gri) Consider sorting all types by dependencies here, and
        // in the process check _and_ report type cycles. This may simplify
        // the full type-checking phase.
        //
        private static slice<Object> resolveOrder(this ref Checker check)
        {
            slice<Object> ifaces = default;            slice<Object> others = default; 

            // collect interface types with their dependencies, and all other objects
 

            // collect interface types with their dependencies, and all other objects
            {
                var obj__prev1 = obj;

                foreach (var (__obj) in check.objMap)
                {
                    obj = __obj;
                    {
                        var ityp = check.interfaceFor(obj);

                        if (ityp != null)
                        {
                            ifaces = append(ifaces, obj); 
                            // determine dependencies on embedded interfaces
                            foreach (var (_, f) in ityp.Methods.List)
                            {
                                if (len(f.Names) == 0L)
                                { 
                                    // Embedded interface: The type must be a (possibly
                                    // qualified) identifier denoting another interface.
                                    // Imported interfaces are already fully resolved,
                                    // so we can ignore qualified identifiers.
                                    {
                                        ref ast.Ident (ident, _) = f.Type._<ref ast.Ident>();

                                        if (ident != null)
                                        {
                                            var embedded = check.pkg.scope.Lookup(ident.Name);
                                            if (check.interfaceFor(embedded) != null)
                                            {
                                                check.objMap[obj].addDep(embedded);
                                            }
                                        }
                                    }
                                }
                            }
                        else
                        }                        {
                            others = append(others, obj);
                        }
                    }
                }
                obj = obj__prev1;
            }

            slice<Object> order = default; 

            // sort interface types topologically by dependencies,
            // and in source order if there are no dependencies
            sort.Sort(inSourceOrder(ifaces));
            var visited = make(objSet);
            {
                var obj__prev1 = obj;

                foreach (var (_, __obj) in ifaces)
                {
                    obj = __obj;
                    check.appendInPostOrder(ref order, obj, visited);
                }
                obj = obj__prev1;
            }

            sort.Sort(inSourceOrder(others));

            return append(order, others);
        }

        // interfaceFor returns the AST interface denoted by obj, or nil.
        private static ref ast.InterfaceType interfaceFor(this ref Checker check, Object obj)
        {
            ref TypeName (tname, _) = obj._<ref TypeName>();
            if (tname == null)
            {
                return null; // not a type
            }
            var d = check.objMap[obj];
            if (d == null)
            {
                check.dump("%s: %s should have been declared", obj.Pos(), obj.Name());
                unreachable();
            }
            if (d.typ == null)
            {
                return null; // invalid AST - ignore (will be handled later)
            }
            ref ast.InterfaceType (ityp, _) = d.typ._<ref ast.InterfaceType>();
            return ityp;
        }

        private static void appendInPostOrder(this ref Checker check, ref slice<Object> order, Object obj, objSet visited)
        {
            if (visited[obj])
            { 
                // We've already seen this object; either because it's
                // already added to order, or because we have a cycle.
                // In both cases we stop. Cycle errors are reported
                // when type-checking types.
                return;
            }
            visited[obj] = true;

            var d = check.objMap[obj];
            foreach (var (_, obj) in orderedSetObjects(d.deps))
            {
                check.appendInPostOrder(order, obj, visited);
            }
            order.Value = append(order.Value, obj);
        }

        private static slice<Object> orderedSetObjects(objSet set)
        {
            var list = make_slice<Object>(len(set));
            long i = 0L;
            foreach (var (obj) in set)
            { 
                // we don't care about the map element value
                list[i] = obj;
                i++;
            }
            sort.Sort(inSourceOrder(list));
            return list;
        }

        // inSourceOrder implements the sort.Sort interface.
        private partial struct inSourceOrder // : slice<Object>
        {
        }

        private static long Len(this inSourceOrder a)
        {
            return len(a);
        }
        private static bool Less(this inSourceOrder a, long i, long j)
        {
            return a[i].order() < a[j].order();
        }
        private static void Swap(this inSourceOrder a, long i, long j)
        {
            a[i] = a[j];
            a[j] = a[i];

        }
    }
}}
