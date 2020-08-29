// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements Scopes.

// package types -- go2cs converted at 2020 August 29 08:47:53 UTC
// import "go/types" ==> using types = go.go.types_package
// Original source: C:\Go\src\go\types\scope.go
using bytes = go.bytes_package;
using fmt = go.fmt_package;
using token = go.go.token_package;
using io = go.io_package;
using sort = go.sort_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace go
{
    public static partial class types_package
    {
        // TODO(gri) Provide scopes with a name or other mechanism so that
        //           objects can use that information for better printing.

        // A Scope maintains a set of objects and links to its containing
        // (parent) and contained (children) scopes. Objects may be inserted
        // and looked up by name. The zero value for Scope is a ready-to-use
        // empty scope.
        public partial struct Scope
        {
            public ptr<Scope> parent;
            public slice<ref Scope> children;
            public map<@string, Object> elems; // lazily allocated
            public token.Pos pos; // scope extent; may be invalid
            public token.Pos end; // scope extent; may be invalid
            public @string comment; // for debugging only
            public bool isFunc; // set if this is a function scope (internal use only)
        }

        // NewScope returns a new, empty scope contained in the given parent
        // scope, if any. The comment is for debugging only.
        public static ref Scope NewScope(ref Scope parent, token.Pos pos, token.Pos end, @string comment)
        {
            Scope s = ref new Scope(parent,nil,nil,pos,end,comment,false); 
            // don't add children to Universe scope!
            if (parent != null && parent != Universe)
            {
                parent.children = append(parent.children, s);
            }
            return s;
        }

        // Parent returns the scope's containing (parent) scope.
        private static ref Scope Parent(this ref Scope s)
        {
            return s.parent;
        }

        // Len() returns the number of scope elements.
        private static long Len(this ref Scope s)
        {
            return len(s.elems);
        }

        // Names returns the scope's element names in sorted order.
        private static slice<@string> Names(this ref Scope s)
        {
            var names = make_slice<@string>(len(s.elems));
            long i = 0L;
            foreach (var (name) in s.elems)
            {
                names[i] = name;
                i++;
            }
            sort.Strings(names);
            return names;
        }

        // NumChildren() returns the number of scopes nested in s.
        private static long NumChildren(this ref Scope s)
        {
            return len(s.children);
        }

        // Child returns the i'th child scope for 0 <= i < NumChildren().
        private static ref Scope Child(this ref Scope s, long i)
        {
            return s.children[i];
        }

        // Lookup returns the object in scope s with the given name if such an
        // object exists; otherwise the result is nil.
        private static Object Lookup(this ref Scope s, @string name)
        {
            return s.elems[name];
        }

        // LookupParent follows the parent chain of scopes starting with s until
        // it finds a scope where Lookup(name) returns a non-nil object, and then
        // returns that scope and object. If a valid position pos is provided,
        // only objects that were declared at or before pos are considered.
        // If no such scope and object exists, the result is (nil, nil).
        //
        // Note that obj.Parent() may be different from the returned scope if the
        // object was inserted into the scope and already had a parent at that
        // time (see Insert, below). This can only happen for dot-imported objects
        // whose scope is the scope of the package that exported them.
        private static (ref Scope, Object) LookupParent(this ref Scope s, @string name, token.Pos pos)
        {
            while (s != null)
            {
                {
                    var obj = s.elems[name];

                    if (obj != null && (!pos.IsValid() || obj.scopePos() <= pos))
                    {
                        return (s, obj);
                s = s.parent;
                    }

                }
            }

            return (null, null);
        }

        // Insert attempts to insert an object obj into scope s.
        // If s already contains an alternative object alt with
        // the same name, Insert leaves s unchanged and returns alt.
        // Otherwise it inserts obj, sets the object's parent scope
        // if not already set, and returns nil.
        private static Object Insert(this ref Scope s, Object obj)
        {
            var name = obj.Name();
            {
                var alt = s.elems[name];

                if (alt != null)
                {
                    return alt;
                }

            }
            if (s.elems == null)
            {
                s.elems = make_map<@string, Object>();
            }
            s.elems[name] = obj;
            if (obj.Parent() == null)
            {
                obj.setParent(s);
            }
            return null;
        }

        // Pos and End describe the scope's source code extent [pos, end).
        // The results are guaranteed to be valid only if the type-checked
        // AST has complete position information. The extent is undefined
        // for Universe and package scopes.
        private static token.Pos Pos(this ref Scope s)
        {
            return s.pos;
        }
        private static token.Pos End(this ref Scope s)
        {
            return s.end;
        }

        // Contains returns true if pos is within the scope's extent.
        // The result is guaranteed to be valid only if the type-checked
        // AST has complete position information.
        private static bool Contains(this ref Scope s, token.Pos pos)
        {
            return s.pos <= pos && pos < s.end;
        }

        // Innermost returns the innermost (child) scope containing
        // pos. If pos is not within any scope, the result is nil.
        // The result is also nil for the Universe scope.
        // The result is guaranteed to be valid only if the type-checked
        // AST has complete position information.
        private static ref Scope Innermost(this ref Scope s, token.Pos pos)
        { 
            // Package scopes do not have extents since they may be
            // discontiguous, so iterate over the package's files.
            if (s.parent == Universe)
            {
                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in s.children)
                    {
                        s = __s;
                        {
                            var inner = s.Innermost(pos);

                            if (inner != null)
                            {
                                return inner;
                            }

                        }
                    }

                    s = s__prev1;
                }

            }
            if (s.Contains(pos))
            {
                {
                    var s__prev1 = s;

                    foreach (var (_, __s) in s.children)
                    {
                        s = __s;
                        if (s.Contains(pos))
                        {
                            return s.Innermost(pos);
                        }
                    }

                    s = s__prev1;
                }

                return s;
            }
            return null;
        }

        // WriteTo writes a string representation of the scope to w,
        // with the scope elements sorted by name.
        // The level of indentation is controlled by n >= 0, with
        // n == 0 for no indentation.
        // If recurse is set, it also writes nested (children) scopes.
        private static void WriteTo(this ref Scope s, io.Writer w, long n, bool recurse)
        {
            const @string ind = ".  ";

            var indn = strings.Repeat(ind, n);

            fmt.Fprintf(w, "%s%s scope %p {", indn, s.comment, s);
            if (len(s.elems) == 0L)
            {
                fmt.Fprintf(w, "}\n");
                return;
            }
            fmt.Fprintln(w);
            var indn1 = indn + ind;
            foreach (var (_, name) in s.Names())
            {
                fmt.Fprintf(w, "%s%s\n", indn1, s.elems[name]);
            }
            if (recurse)
            {
                foreach (var (_, s) in s.children)
                {
                    fmt.Fprintln(w);
                    s.WriteTo(w, n + 1L, recurse);
                }
            }
            fmt.Fprintf(w, "%s}", indn);
        }

        // String returns a string representation of the scope, for debugging.
        private static @string String(this ref Scope s)
        {
            bytes.Buffer buf = default;
            s.WriteTo(ref buf, 0L, false);
            return buf.String();
        }
    }
}}
