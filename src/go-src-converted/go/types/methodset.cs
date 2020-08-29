// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements method sets.

// package types -- go2cs converted at 2020 August 29 08:47:43 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\methodset.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using sort = go.sort_package;
using static go.builtin;
using System;

namespace go {
namespace go
{
    public static partial class types_package
    {
        // A MethodSet is an ordered set of concrete or abstract (interface) methods;
        // a method is a MethodVal selection, and they are ordered by ascending m.Obj().Id().
        // The zero value for a MethodSet is a ready-to-use empty method set.
        public partial struct MethodSet
        {
            public slice<ref Selection> list;
        }

        private static @string String(this ref MethodSet s)
        {
            if (s.Len() == 0L)
            {
                return "MethodSet {}";
            }
            bytes.Buffer buf = default;
            fmt.Fprintln(ref buf, "MethodSet {");
            foreach (var (_, f) in s.list)
            {
                fmt.Fprintf(ref buf, "\t%s\n", f);
            }
            fmt.Fprintln(ref buf, "}");
            return buf.String();
        }

        // Len returns the number of methods in s.
        private static long Len(this ref MethodSet s)
        {
            return len(s.list);
        }

        // At returns the i'th method in s for 0 <= i < s.Len().
        private static ref Selection At(this ref MethodSet s, long i)
        {
            return s.list[i];
        }

        // Lookup returns the method with matching package and name, or nil if not found.
        private static ref Selection Lookup(this ref MethodSet s, ref Package pkg, @string name)
        {
            if (s.Len() == 0L)
            {
                return null;
            }
            var key = Id(pkg, name);
            var i = sort.Search(len(s.list), i =>
            {
                var m = s.list[i];
                return m.obj.Id() >= key;
            });
            if (i < len(s.list))
            {
                m = s.list[i];
                if (m.obj.Id() == key)
                {
                    return m;
                }
            }
            return null;
        }

        // Shared empty method set.
        private static MethodSet emptyMethodSet = default;

        // NewMethodSet returns the method set for the given type T.
        // It always returns a non-nil method set, even if it is empty.
        public static ref MethodSet NewMethodSet(Type T)
        { 
            // WARNING: The code in this function is extremely subtle - do not modify casually!
            //          This function and lookupFieldOrMethod should be kept in sync.

            // method set up to the current depth, allocated lazily
            methodSet @base = default;

            var (typ, isPtr) = deref(T); 

            // *typ where typ is an interface has no methods.
            if (isPtr && IsInterface(typ))
            {
                return ref emptyMethodSet;
            } 

            // Start with typ as single entry at shallowest depth.
            embeddedType current = new slice<embeddedType>(new embeddedType[] { {typ,nil,isPtr,false} }); 

            // Named types that we have seen already, allocated lazily.
            // Used to avoid endless searches in case of recursive types.
            // Since only Named types can be used for recursive types, we
            // only need to track those.
            // (If we ever allow type aliases to construct recursive types,
            // we must use type identity rather than pointer equality for
            // the map key comparison, as we do in consolidateMultiples.)
            map<ref Named, bool> seen = default; 

            // collect methods at current depth
            while (len(current) > 0L)
            {
                slice<embeddedType> next = default; // embedded types found at current depth

                // field and method sets at current depth, allocated lazily
                fieldSet fset = default;
                methodSet mset = default;

                foreach (var (_, e) in current)
                {
                    var typ = e.typ; 

                    // If we have a named type, we may have associated methods.
                    // Look for those first.
                    {
                        ref Named (named, _) = typ._<ref Named>();

                        if (named != null)
                        {
                            if (seen[named])
                            { 
                                // We have seen this type before, at a more shallow depth
                                // (note that multiples of this type at the current depth
                                // were consolidated before). The type at that depth shadows
                                // this same type at the current depth, so we can ignore
                                // this one.
                                continue;
                            }
                            if (seen == null)
                            {
                                seen = make_map<ref Named, bool>();
                            }
                            seen[named] = true;

                            mset = mset.add(named.methods, e.index, e.indirect, e.multiples); 

                            // continue with underlying type
                            typ = named.underlying;
                        }

                    }

                    switch (typ.type())
                    {
                        case ref Struct t:
                            {
                                var f__prev3 = f;

                                foreach (var (__i, __f) in t.fields)
                                {
                                    i = __i;
                                    f = __f;
                                    fset = fset.add(f, e.multiples); 

                                    // Embedded fields are always of the form T or *T where
                                    // T is a type name. If typ appeared multiple times at
                                    // this depth, f.Type appears multiple times at the next
                                    // depth.
                                    if (f.anonymous)
                                    {
                                        (typ, isPtr) = deref(f.typ); 
                                        // TODO(gri) optimization: ignore types that can't
                                        // have fields or methods (only Named, Struct, and
                                        // Interface types need to be considered).
                                        next = append(next, new embeddedType(typ,concat(e.index,i),e.indirect||isPtr,e.multiples));
                                    }
                                }

                                f = f__prev3;
                            }
                            break;
                        case ref Interface t:
                            mset = mset.add(t.allMethods, e.index, true, e.multiples);
                            break;
                    }
                } 

                // Add methods and collisions at this depth to base if no entries with matching
                // names exist already.
                {
                    var k__prev2 = k;
                    var m__prev2 = m;

                    foreach (var (__k, __m) in mset)
                    {
                        k = __k;
                        m = __m;
                        {
                            var (_, found) = base[k];

                            if (!found)
                            { 
                                // Fields collide with methods of the same name at this depth.
                                {
                                    (_, found) = fset[k];

                                    if (found)
                                    {
                                        m = null; // collision
                                    }

                                }
                                if (base == null)
                                {
                                    base = make(methodSet);
                                }
                                base[k] = m;
                            }

                        }
                    } 

                    // Multiple fields with matching names collide at this depth and shadow all
                    // entries further down; add them as collisions to base if no entries with
                    // matching names exist already.

                    k = k__prev2;
                    m = m__prev2;
                }

                {
                    var k__prev2 = k;
                    var f__prev2 = f;

                    foreach (var (__k, __f) in fset)
                    {
                        k = __k;
                        f = __f;
                        if (f == null)
                        {
                            {
                                (_, found) = base[k];

                                if (!found)
                                {
                                    if (base == null)
                                    {
                                        base = make(methodSet);
                                    }
                                    base[k] = null; // collision
                                }

                            }
                        }
                    }

                    k = k__prev2;
                    f = f__prev2;
                }

                current = consolidateMultiples(next);
            }


            if (len(base) == 0L)
            {
                return ref emptyMethodSet;
            } 

            // collect methods
            slice<ref Selection> list = default;
            {
                var m__prev1 = m;

                foreach (var (_, __m) in base)
                {
                    m = __m;
                    if (m != null)
                    {
                        m.recv = T;
                        list = append(list, m);
                    }
                } 
                // sort by unique name

                m = m__prev1;
            }

            sort.Slice(list, (i, j) =>
            {
                return list[i].obj.Id() < list[j].obj.Id();
            });
            return ref new MethodSet(list);
        }

        // A fieldSet is a set of fields and name collisions.
        // A collision indicates that multiple fields with the
        // same unique id appeared.
        private partial struct fieldSet // : map<@string, ref Var>
        {
        } // a nil entry indicates a name collision

        // Add adds field f to the field set s.
        // If multiples is set, f appears multiple times
        // and is treated as a collision.
        private static fieldSet add(this fieldSet s, ref Var f, bool multiples)
        {
            if (s == null)
            {
                s = make(fieldSet);
            }
            var key = f.Id(); 
            // if f is not in the set, add it
            if (!multiples)
            {
                {
                    var (_, found) = s[key];

                    if (!found)
                    {
                        s[key] = f;
                        return s;
                    }

                }
            }
            s[key] = null; // collision
            return s;
        }

        // A methodSet is a set of methods and name collisions.
        // A collision indicates that multiple methods with the
        // same unique id appeared.
        private partial struct methodSet // : map<@string, ref Selection>
        {
        } // a nil entry indicates a name collision

        // Add adds all functions in list to the method set s.
        // If multiples is set, every function in list appears multiple times
        // and is treated as a collision.
        private static methodSet add(this methodSet s, slice<ref Func> list, slice<long> index, bool indirect, bool multiples)
        {
            if (len(list) == 0L)
            {
                return s;
            }
            if (s == null)
            {
                s = make(methodSet);
            }
            foreach (var (i, f) in list)
            {
                var key = f.Id(); 
                // if f is not in the set, add it
                if (!multiples)
                { 
                    // TODO(gri) A found method may not be added because it's not in the method set
                    // (!indirect && ptrRecv(f)). A 2nd method on the same level may be in the method
                    // set and may not collide with the first one, thus leading to a false positive.
                    // Is that possible? Investigate.
                    {
                        var (_, found) = s[key];

                        if (!found && (indirect || !ptrRecv(f)))
                        {
                            s[key] = ref new Selection(MethodVal,nil,f,concat(index,i),indirect);
                            continue;
                        }

                    }
                }
                s[key] = null; // collision
            }
            return s;
        }

        // ptrRecv reports whether the receiver is of the form *T.
        // The receiver must exist.
        private static bool ptrRecv(ref Func f)
        {
            var (_, isPtr) = deref(f.typ._<ref Signature>().recv.typ);
            return isPtr;
        }
    }
}}
