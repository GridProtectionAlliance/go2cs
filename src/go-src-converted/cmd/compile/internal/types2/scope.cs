// Copyright 2013 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// This file implements Scopes.

// package types2 -- go2cs converted at 2022 March 13 06:26:15 UTC
// import "cmd/compile/internal/types2" ==> using types2 = go.cmd.compile.@internal.types2_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\types2\scope.go
namespace go.cmd.compile.@internal;

using bytes = bytes_package;
using syntax = cmd.compile.@internal.syntax_package;
using fmt = fmt_package;
using io = io_package;
using sort = sort_package;
using strings = strings_package;


// A Scope maintains a set of objects and links to its containing
// (parent) and contained (children) scopes. Objects may be inserted
// and looked up by name. The zero value for Scope is a ready-to-use
// empty scope.

using System;
public static partial class types2_package {

public partial struct Scope {
    public ptr<Scope> parent;
    public slice<ptr<Scope>> children;
    public map<@string, Object> elems; // lazily allocated
    public syntax.Pos pos; // scope extent; may be invalid
    public syntax.Pos end; // scope extent; may be invalid
    public @string comment; // for debugging only
    public bool isFunc; // set if this is a function scope (internal use only)
}

// NewScope returns a new, empty scope contained in the given parent
// scope, if any. The comment is for debugging only.
public static ptr<Scope> NewScope(ptr<Scope> _addr_parent, syntax.Pos pos, syntax.Pos end, @string comment) {
    ref Scope parent = ref _addr_parent.val;

    ptr<Scope> s = addr(new Scope(parent,nil,nil,pos,end,comment,false)); 
    // don't add children to Universe scope!
    if (parent != null && parent != Universe) {
        parent.children = append(parent.children, s);
    }
    return _addr_s!;
}

// Parent returns the scope's containing (parent) scope.
private static ptr<Scope> Parent(this ptr<Scope> _addr_s) {
    ref Scope s = ref _addr_s.val;

    return _addr_s.parent!;
}

// Len returns the number of scope elements.
private static nint Len(this ptr<Scope> _addr_s) {
    ref Scope s = ref _addr_s.val;

    return len(s.elems);
}

// Names returns the scope's element names in sorted order.
private static slice<@string> Names(this ptr<Scope> _addr_s) {
    ref Scope s = ref _addr_s.val;

    var names = make_slice<@string>(len(s.elems));
    nint i = 0;
    foreach (var (name) in s.elems) {
        names[i] = name;
        i++;
    }    sort.Strings(names);
    return names;
}

// NumChildren returns the number of scopes nested in s.
private static nint NumChildren(this ptr<Scope> _addr_s) {
    ref Scope s = ref _addr_s.val;

    return len(s.children);
}

// Child returns the i'th child scope for 0 <= i < NumChildren().
private static ptr<Scope> Child(this ptr<Scope> _addr_s, nint i) {
    ref Scope s = ref _addr_s.val;

    return _addr_s.children[i]!;
}

// Lookup returns the object in scope s with the given name if such an
// object exists; otherwise the result is nil.
private static Object Lookup(this ptr<Scope> _addr_s, @string name) {
    ref Scope s = ref _addr_s.val;

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
// time (see Insert). This can only happen for dot-imported objects
// whose scope is the scope of the package that exported them.
private static (ptr<Scope>, Object) LookupParent(this ptr<Scope> _addr_s, @string name, syntax.Pos pos) {
    ptr<Scope> _p0 = default!;
    Object _p0 = default;
    ref Scope s = ref _addr_s.val;

    while (s != null) {
        {
            var obj = s.elems[name];

            if (obj != null && (!pos.IsKnown() || obj.scopePos().Cmp(pos) <= 0)) {
                return (_addr_s!, obj);
        s = s.parent;
            }

        }
    }
    return (_addr_null!, null);
}

// Insert attempts to insert an object obj into scope s.
// If s already contains an alternative object alt with
// the same name, Insert leaves s unchanged and returns alt.
// Otherwise it inserts obj, sets the object's parent scope
// if not already set, and returns nil.
private static Object Insert(this ptr<Scope> _addr_s, Object obj) {
    ref Scope s = ref _addr_s.val;

    var name = obj.Name();
    {
        var alt = s.elems[name];

        if (alt != null) {
            return alt;
        }
    }
    if (s.elems == null) {
        s.elems = make_map<@string, Object>();
    }
    s.elems[name] = obj;
    if (obj.Parent() == null) {
        obj.setParent(s);
    }
    return null;
}

// Squash merges s with its parent scope p by adding all
// objects of s to p, adding all children of s to the
// children of p, and removing s from p's children.
// The function f is called for each object obj in s which
// has an object alt in p. s should be discarded after
// having been squashed.
private static void Squash(this ptr<Scope> _addr_s, Action<Object, Object> err) {
    ref Scope s = ref _addr_s.val;

    var p = s.parent;
    assert(p != null);
    foreach (var (_, obj) in s.elems) {
        obj.setParent(null);
        {
            var alt = p.Insert(obj);

            if (alt != null) {
                err(obj, alt);
            }

        }
    }    nint j = -1; // index of s in p.children
    foreach (var (i, ch) in p.children) {
        if (ch == s) {
            j = i;
            break;
        }
    }    assert(j >= 0);
    var k = len(p.children) - 1;
    p.children[j] = p.children[k];
    p.children = p.children[..(int)k];

    p.children = append(p.children, s.children);

    s.children = null;
    s.elems = null;
}

// Pos and End describe the scope's source code extent [pos, end).
// The results are guaranteed to be valid only if the type-checked
// AST has complete position information. The extent is undefined
// for Universe and package scopes.
private static syntax.Pos Pos(this ptr<Scope> _addr_s) {
    ref Scope s = ref _addr_s.val;

    return s.pos;
}
private static syntax.Pos End(this ptr<Scope> _addr_s) {
    ref Scope s = ref _addr_s.val;

    return s.end;
}

// Contains reports whether pos is within the scope's extent.
// The result is guaranteed to be valid only if the type-checked
// AST has complete position information.
private static bool Contains(this ptr<Scope> _addr_s, syntax.Pos pos) {
    ref Scope s = ref _addr_s.val;

    return s.pos.Cmp(pos) <= 0 && pos.Cmp(s.end) < 0;
}

// Innermost returns the innermost (child) scope containing
// pos. If pos is not within any scope, the result is nil.
// The result is also nil for the Universe scope.
// The result is guaranteed to be valid only if the type-checked
// AST has complete position information.
private static ptr<Scope> Innermost(this ptr<Scope> _addr_s, syntax.Pos pos) {
    ref Scope s = ref _addr_s.val;
 
    // Package scopes do not have extents since they may be
    // discontiguous, so iterate over the package's files.
    if (s.parent == Universe) {
        {
            var s__prev1 = s;

            foreach (var (_, __s) in s.children) {
                s = __s;
                {
                    var inner = s.Innermost(pos);

                    if (inner != null) {
                        return _addr_inner!;
                    }

                }
            }

            s = s__prev1;
        }
    }
    if (s.Contains(pos)) {
        {
            var s__prev1 = s;

            foreach (var (_, __s) in s.children) {
                s = __s;
                if (s.Contains(pos)) {
                    return _addr_s.Innermost(pos)!;
                }
            }

            s = s__prev1;
        }

        return _addr_s!;
    }
    return _addr_null!;
}

// WriteTo writes a string representation of the scope to w,
// with the scope elements sorted by name.
// The level of indentation is controlled by n >= 0, with
// n == 0 for no indentation.
// If recurse is set, it also writes nested (children) scopes.
private static void WriteTo(this ptr<Scope> _addr_s, io.Writer w, nint n, bool recurse) {
    ref Scope s = ref _addr_s.val;

    const @string ind = ".  ";

    var indn = strings.Repeat(ind, n);

    fmt.Fprintf(w, "%s%s scope %p {\n", indn, s.comment, s);

    var indn1 = indn + ind;
    foreach (var (_, name) in s.Names()) {
        fmt.Fprintf(w, "%s%s\n", indn1, s.elems[name]);
    }    if (recurse) {
        foreach (var (_, s) in s.children) {
            s.WriteTo(w, n + 1, recurse);
        }
    }
    fmt.Fprintf(w, "%s}\n", indn);
}

// String returns a string representation of the scope, for debugging.
private static @string String(this ptr<Scope> _addr_s) {
    ref Scope s = ref _addr_s.val;

    ref bytes.Buffer buf = ref heap(out ptr<bytes.Buffer> _addr_buf);
    s.WriteTo(_addr_buf, 0, false);
    return buf.String();
}

} // end types2_package
