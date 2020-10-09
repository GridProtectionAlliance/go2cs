// package packages -- go2cs converted at 2020 October 09 06:02:21 UTC
// import "golang.org/x/tools/go/packages" ==> using packages = go.golang.org.x.tools.go.packages_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\packages\visit.go
using fmt = go.fmt_package;
using os = go.os_package;
using sort = go.sort_package;
using static go.builtin;
using System;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go
{
    public static partial class packages_package
    {
        // Visit visits all the packages in the import graph whose roots are
        // pkgs, calling the optional pre function the first time each package
        // is encountered (preorder), and the optional post function after a
        // package's dependencies have been visited (postorder).
        // The boolean result of pre(pkg) determines whether
        // the imports of package pkg are visited.
        public static void Visit(slice<ptr<Package>> pkgs, Func<ptr<Package>, bool> pre, Action<ptr<Package>> post)
        {
            var seen = make_map<ptr<Package>, bool>();
            Action<ptr<Package>> visit = default;
            visit = pkg =>
            {
                if (!seen[pkg])
                {
                    seen[pkg] = true;

                    if (pre == null || pre(pkg))
                    {
                        var paths = make_slice<@string>(0L, len(pkg.Imports));
                        {
                            var path__prev1 = path;

                            foreach (var (__path) in pkg.Imports)
                            {
                                path = __path;
                                paths = append(paths, path);
                            }
                            path = path__prev1;
                        }

                        sort.Strings(paths); // Imports is a map, this makes visit stable
                        {
                            var path__prev1 = path;

                            foreach (var (_, __path) in paths)
                            {
                                path = __path;
                                visit(pkg.Imports[path]);
                            }
                            path = path__prev1;
                        }
                    }
                    if (post != null)
                    {
                        post(pkg);
                    }
                }
            };
            foreach (var (_, pkg) in pkgs)
            {
                visit(pkg);
            }
        }

        // PrintErrors prints to os.Stderr the accumulated errors of all
        // packages in the import graph rooted at pkgs, dependencies first.
        // PrintErrors returns the number of errors printed.
        public static long PrintErrors(slice<ptr<Package>> pkgs)
        {
            long n = default;
            Visit(pkgs, null, pkg =>
            {
                foreach (var (_, err) in pkg.Errors)
                {
                    fmt.Fprintln(os.Stderr, err);
                    n++;
                }

            });
            return n;

        }
    }
}}}}}
