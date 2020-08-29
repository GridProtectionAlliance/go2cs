// Derived from Inferno utils/6l/obj.c and utils/6l/span.c
// https://bitbucket.org/inferno-os/inferno-os/src/default/utils/6l/obj.c
// https://bitbucket.org/inferno-os/inferno-os/src/default/utils/6l/span.c
//
//    Copyright © 1994-1999 Lucent Technologies Inc.  All rights reserved.
//    Portions Copyright © 1995-1997 C H Forsyth (forsyth@terzarima.net)
//    Portions Copyright © 1997-1999 Vita Nuova Limited
//    Portions Copyright © 2000-2007 Vita Nuova Holdings Limited (www.vitanuova.com)
//    Portions Copyright © 2004,2006 Bruce Ellis
//    Portions Copyright © 2005-2007 C H Forsyth (forsyth@terzarima.net)
//    Revisions Copyright © 2000-2007 Lucent Technologies Inc. and others
//    Portions Copyright © 2009 The Go Authors. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

// package ld -- go2cs converted at 2020 August 29 10:03:44 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\ld.go
using sym = go.cmd.link.@internal.sym_package;
using ioutil = go.io.ioutil_package;
using log = go.log_package;
using os = go.os_package;
using path = go.path_package;
using filepath = go.path.filepath_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace link {
namespace @internal
{
    public static partial class ld_package
    {
        private static void readImportCfg(this ref Link ctxt, @string file)
        {
            ctxt.PackageFile = make_map<@string, @string>();
            ctxt.PackageShlib = make_map<@string, @string>();
            var (data, err) = ioutil.ReadFile(file);
            if (err != null)
            {
                log.Fatalf("-importcfg: %v", err);
            }
            foreach (var (lineNum, line) in strings.Split(string(data), "\n"))
            {
                lineNum++; // 1-based
                line = strings.TrimSpace(line);
                if (line == "")
                {
                    continue;
                }
                if (line == "" || strings.HasPrefix(line, "#"))
                {
                    continue;
                }
                @string verb = default;                @string args = default;

                {
                    var i__prev1 = i;

                    var i = strings.Index(line, " ");

                    if (i < 0L)
                    {
                        verb = line;
                    }
                    else
                    {
                        verb = line[..i];
                        args = strings.TrimSpace(line[i + 1L..]);
                    }
                    i = i__prev1;

                }
                @string before = default;                @string after = default;

                {
                    var i__prev1 = i;

                    i = strings.Index(args, "=");

                    if (i >= 0L)
                    {
                        before = args[..i];
                        after = args[i + 1L..];
                    }
                    i = i__prev1;

                }
                switch (verb)
                {
                    case "packagefile": 
                        if (before == "" || after == "")
                        {
                            log.Fatalf("%s:%d: invalid packagefile: syntax is \"packagefile path=filename\"", file, lineNum);
                        }
                        ctxt.PackageFile[before] = after;
                        break;
                    case "packageshlib": 
                        if (before == "" || after == "")
                        {
                            log.Fatalf("%s:%d: invalid packageshlib: syntax is \"packageshlib path=filename\"", file, lineNum);
                        }
                        ctxt.PackageShlib[before] = after;
                        break;
                    default: 
                        log.Fatalf("%s:%d: unknown directive %q", file, lineNum, verb);
                        break;
                }
            }
        }

        private static @string pkgname(ref Link ctxt, @string lib)
        {
            var name = path.Clean(lib); 

            // When using importcfg, we have the final package name.
            if (ctxt.PackageFile != null)
            {
                return name;
            } 

            // runtime.a -> runtime, runtime.6 -> runtime
            var pkg = name;
            if (len(pkg) >= 2L && pkg[len(pkg) - 2L] == '.')
            {
                pkg = pkg[..len(pkg) - 2L];
            }
            return pkg;
        }

        private static (@string, bool) findlib(ref Link ctxt, @string lib)
        {
            var name = path.Clean(lib);

            @string pname = default;
            var isshlib = false;

            if (ctxt.linkShared && ctxt.PackageShlib[name] != "")
            {
                pname = ctxt.PackageShlib[name];
                isshlib = true;
            }
            else if (ctxt.PackageFile != null)
            {
                pname = ctxt.PackageFile[name];
                if (pname == "")
                {
                    ctxt.Logf("cannot find package %s (using -importcfg)\n", name);
                    return ("", false);
                }
            }
            else
            {
                if (filepath.IsAbs(name))
                {
                    pname = name;
                }
                else
                {
                    var pkg = pkgname(ctxt, lib); 
                    // Add .a if needed; the new -importcfg modes
                    // do not put .a into the package name anymore.
                    // This only matters when people try to mix
                    // compiles using -importcfg with links not using -importcfg,
                    // such as when running quick things like
                    // 'go tool compile x.go && go tool link x.o'
                    // by hand against a standard library built using -importcfg.
                    if (!strings.HasSuffix(name, ".a") && !strings.HasSuffix(name, ".o"))
                    {
                        name += ".a";
                    } 
                    // try dot, -L "libdir", and then goroot.
                    foreach (var (_, dir) in ctxt.Libdir)
                    {
                        if (ctxt.linkShared)
                        {
                            pname = dir + "/" + pkg + ".shlibname";
                            {
                                var (_, err) = os.Stat(pname);

                                if (err == null)
                                {
                                    isshlib = true;
                                    break;
                                }

                            }
                        }
                        pname = dir + "/" + name;
                        {
                            (_, err) = os.Stat(pname);

                            if (err == null)
                            {
                                break;
                            }

                        }
                    }
                }
                pname = path.Clean(pname);
            }
            return (pname, isshlib);
        }

        private static ref sym.Library addlib(ref Link ctxt, @string src, @string obj, @string lib)
        {
            var pkg = pkgname(ctxt, lib); 

            // already loaded?
            {
                var l = ctxt.LibraryByPkg[pkg];

                if (l != null)
                {
                    return l;
                }

            }

            var (pname, isshlib) = findlib(ctxt, lib);

            if (ctxt.Debugvlog > 1L)
            {
                ctxt.Logf("%5.2f addlib: %s %s pulls in %s isshlib %v\n", elapsed(), obj, src, pname, isshlib);
            }
            if (isshlib)
            {
                return addlibpath(ctxt, src, obj, "", pkg, pname);
            }
            return addlibpath(ctxt, src, obj, pname, pkg, "");
        }

        /*
         * add library to library list, return added library.
         *    srcref: src file referring to package
         *    objref: object file referring to package
         *    file: object file, e.g., /home/rsc/go/pkg/container/vector.a
         *    pkg: package import path, e.g. container/vector
         *    shlib: path to shared library, or .shlibname file holding path
         */
        private static ref sym.Library addlibpath(ref Link ctxt, @string srcref, @string objref, @string file, @string pkg, @string shlib)
        {
            {
                var l__prev1 = l;

                var l = ctxt.LibraryByPkg[pkg];

                if (l != null)
                {
                    return l;
                }

                l = l__prev1;

            }

            if (ctxt.Debugvlog > 1L)
            {
                ctxt.Logf("%5.2f addlibpath: srcref: %s objref: %s file: %s pkg: %s shlib: %s\n", Cputime(), srcref, objref, file, pkg, shlib);
            }
            l = ref new sym.Library();
            ctxt.LibraryByPkg[pkg] = l;
            ctxt.Library = append(ctxt.Library, l);
            l.Objref = objref;
            l.Srcref = srcref;
            l.File = file;
            l.Pkg = pkg;
            if (shlib != "")
            {
                if (strings.HasSuffix(shlib, ".shlibname"))
                {
                    var (data, err) = ioutil.ReadFile(shlib);
                    if (err != null)
                    {
                        Errorf(null, "cannot read %s: %v", shlib, err);
                    }
                    shlib = strings.TrimSpace(string(data));
                }
                l.Shlib = shlib;
            }
            return l;
        }

        private static long atolwhex(@string s)
        {
            var (n, _) = strconv.ParseInt(s, 0L, 64L);
            return n;
        }
    }
}}}}
