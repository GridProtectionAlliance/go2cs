// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2020 August 29 08:53:13 UTC
// import "cmd/compile/internal/types" ==> using types = go.cmd.compile.@internal.types_package
// Original source: C:\Go\src\cmd\compile\internal\types\pkg.go
using obj = go.cmd.@internal.obj_package;
using objabi = go.cmd.@internal.objabi_package;
using fmt = go.fmt_package;
using sort = go.sort_package;
using sync = go.sync_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class types_package
    {
        // pkgMap maps a package path to a package.
        private static var pkgMap = make_map<@string, ref Pkg>();

        public partial struct Pkg
        {
            public @string Path; // string literal used in import statement, e.g. "runtime/internal/sys"
            public @string Name; // package name, e.g. "sys"
            public ptr<obj.LSym> Pathsym;
            public @string Prefix; // escaped path for use in symbol table
            public bool Imported; // export data of this package was parsed
            public bool Direct; // imported directly
            public map<@string, ref Sym> Syms;
        }

        // NewPkg returns a new Pkg for the given package path and name.
        // Unless name is the empty string, if the package exists already,
        // the existing package name and the provided name must match.
        public static ref Pkg NewPkg(@string path, @string name) => func((_, panic, __) =>
        {
            {
                var p__prev1 = p;

                var p = pkgMap[path];

                if (p != null)
                {
                    if (name != "" && p.Name != name)
                    {
                        panic(fmt.Sprintf("conflicting package names %s and %s for path %q", p.Name, name, path));
                    }
                    return p;
                }

                p = p__prev1;

            }

            p = @new<Pkg>();
            p.Path = path;
            p.Name = name;
            p.Prefix = objabi.PathToPrefix(path);
            p.Syms = make_map<@string, ref Sym>();
            pkgMap[path] = p;

            return p;
        });

        // ImportedPkgList returns the list of directly imported packages.
        // The list is sorted by package path.
        public static slice<ref Pkg> ImportedPkgList()
        {
            slice<ref Pkg> list = default;
            foreach (var (_, p) in pkgMap)
            {
                if (p.Direct)
                {
                    list = append(list, p);
                }
            }
            sort.Sort(byPath(list));
            return list;
        }

        private partial struct byPath // : slice<ref Pkg>
        {
        }

        private static long Len(this byPath a)
        {
            return len(a);
        }
        private static bool Less(this byPath a, long i, long j)
        {
            return a[i].Path < a[j].Path;
        }
        private static void Swap(this byPath a, long i, long j)
        {
            a[i] = a[j];
            a[j] = a[i];

        }

        private static Pkg nopkg = ref new Pkg(Syms:make(map[string]*Sym),);

        private static ref Sym Lookup(this ref Pkg pkg, @string name)
        {
            var (s, _) = pkg.LookupOK(name);
            return s;
        }

        public static slice<ref Sym> InitSyms = default;

        // LookupOK looks up name in pkg and reports whether it previously existed.
        private static (ref Sym, bool) LookupOK(this ref Pkg pkg, @string name)
        { 
            // TODO(gri) remove this check in favor of specialized lookup
            if (pkg == null)
            {
                pkg = nopkg;
            }
            {
                var s = pkg.Syms[name];

                if (s != null)
                {
                    return (s, true);
                }

            }

            s = ref new Sym(Name:name,Pkg:pkg,);
            if (name == "init")
            {
                InitSyms = append(InitSyms, s);
            }
            pkg.Syms[name] = s;
            return (s, false);
        }

        private static ref Sym LookupBytes(this ref Pkg pkg, slice<byte> name)
        { 
            // TODO(gri) remove this check in favor of specialized lookup
            if (pkg == null)
            {
                pkg = nopkg;
            }
            {
                var s = pkg.Syms[string(name)];

                if (s != null)
                {
                    return s;
                }

            }
            var str = InternString(name);
            return pkg.Lookup(str);
        }

        private static sync.Mutex internedStringsmu = default;        private static map internedStrings = /* TODO: Fix this in ScannerBase_Expression::ExitCompositeLit */ new map<@string, @string>{};

        public static @string InternString(slice<byte> b)
        {
            internedStringsmu.Lock();
            var (s, ok) = internedStrings[string(b)]; // string(b) here doesn't allocate
            if (!ok)
            {
                s = string(b);
                internedStrings[s] = s;
            }
            internedStringsmu.Unlock();
            return s;
        }

        // CleanroomDo invokes f in an environment with with no preexisting packages.
        // For testing of import/export only.
        public static void CleanroomDo(Action f)
        {
            var saved = pkgMap;
            pkgMap = make_map<@string, ref Pkg>();
            f();
            pkgMap = saved;
        }
    }
}}}}
