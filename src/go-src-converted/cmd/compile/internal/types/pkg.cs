// Copyright 2017 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package types -- go2cs converted at 2020 October 08 04:09:47 UTC
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
        private static var pkgMap = make_map<@string, ptr<Pkg>>();

        // MaxPkgHeight is a height greater than any likely package height.
        public static readonly float MaxPkgHeight = (float)1e9F;



        public partial struct Pkg
        {
            public @string Path; // string literal used in import statement, e.g. "runtime/internal/sys"
            public @string Name; // package name, e.g. "sys"
            public @string Prefix; // escaped path for use in symbol table
            public map<@string, ptr<Sym>> Syms;
            public ptr<obj.LSym> Pathsym; // Height is the package's height in the import graph. Leaf
// packages (i.e., packages with no imports) have height 0,
// and all other packages have height 1 plus the maximum
// height of their imported packages.
            public long Height;
            public bool Imported; // export data of this package was parsed
            public bool Direct; // imported directly
        }

        // NewPkg returns a new Pkg for the given package path and name.
        // Unless name is the empty string, if the package exists already,
        // the existing package name and the provided name must match.
        public static ptr<Pkg> NewPkg(@string path, @string name) => func((_, panic, __) =>
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

                    return _addr_p!;

                }

                p = p__prev1;

            }


            p = @new<Pkg>();
            p.Path = path;
            p.Name = name;
            p.Prefix = objabi.PathToPrefix(path);
            p.Syms = make_map<@string, ptr<Sym>>();
            pkgMap[path] = p;

            return _addr_p!;

        });

        // ImportedPkgList returns the list of directly imported packages.
        // The list is sorted by package path.
        public static slice<ptr<Pkg>> ImportedPkgList()
        {
            slice<ptr<Pkg>> list = default;
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

        private partial struct byPath // : slice<ptr<Pkg>>
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

        private static ptr<Pkg> nopkg = addr(new Pkg(Syms:make(map[string]*Sym),));

        private static ptr<Sym> Lookup(this ptr<Pkg> _addr_pkg, @string name)
        {
            ref Pkg pkg = ref _addr_pkg.val;

            var (s, _) = pkg.LookupOK(name);
            return _addr_s!;
        }

        // List of .inittask entries in imported packages, in source code order.
        public static slice<ptr<Sym>> InitSyms = default;

        // LookupOK looks up name in pkg and reports whether it previously existed.
        private static (ptr<Sym>, bool) LookupOK(this ptr<Pkg> _addr_pkg, @string name)
        {
            ptr<Sym> s = default!;
            bool existed = default;
            ref Pkg pkg = ref _addr_pkg.val;
 
            // TODO(gri) remove this check in favor of specialized lookup
            if (pkg == null)
            {
                pkg = nopkg;
            }

            {
                var s = pkg.Syms[name];

                if (s != null)
                {
                    return (_addr_s!, true);
                }

            }


            s = addr(new Sym(Name:name,Pkg:pkg,));
            if (name == ".inittask")
            {
                InitSyms = append(InitSyms, s);
            }

            pkg.Syms[name] = s;
            return (_addr_s!, false);

        }

        private static ptr<Sym> LookupBytes(this ptr<Pkg> _addr_pkg, slice<byte> name)
        {
            ref Pkg pkg = ref _addr_pkg.val;
 
            // TODO(gri) remove this check in favor of specialized lookup
            if (pkg == null)
            {
                pkg = nopkg;
            }

            {
                var s = pkg.Syms[string(name)];

                if (s != null)
                {
                    return _addr_s!;
                }

            }

            var str = InternString(name);
            return _addr_pkg.Lookup(str)!;

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

        // CleanroomDo invokes f in an environment with no preexisting packages.
        // For testing of import/export only.
        public static void CleanroomDo(Action f)
        {
            var saved = pkgMap;
            pkgMap = make_map<@string, ptr<Pkg>>();
            f();
            pkgMap = saved;
        }
    }
}}}}
