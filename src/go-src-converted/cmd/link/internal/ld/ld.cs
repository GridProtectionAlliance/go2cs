// Derived from Inferno utils/6l/obj.c and utils/6l/span.c
// https://bitbucket.org/inferno-os/inferno-os/src/master/utils/6l/obj.c
// https://bitbucket.org/inferno-os/inferno-os/src/master/utils/6l/span.c
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

// package ld -- go2cs converted at 2020 October 08 04:38:46 UTC
// import "cmd/link/internal/ld" ==> using ld = go.cmd.link.@internal.ld_package
// Original source: C:\Go\src\cmd\link\internal\ld\ld.go
using goobj2 = go.cmd.@internal.goobj2_package;
using loader = go.cmd.link.@internal.loader_package;
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
        private static void readImportCfg(this ptr<Link> _addr_ctxt, @string file)
        {
            ref Link ctxt = ref _addr_ctxt.val;

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

        private static @string pkgname(ptr<Link> _addr_ctxt, @string lib)
        {
            ref Link ctxt = ref _addr_ctxt.val;

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

        private static (@string, bool) findlib(ptr<Link> _addr_ctxt, @string lib)
        {
            @string _p0 = default;
            bool _p0 = default;
            ref Link ctxt = ref _addr_ctxt.val;

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
                    var pkg = pkgname(_addr_ctxt, lib); 
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
                            pname = filepath.Join(dir, pkg + ".shlibname");
                            {
                                var (_, err) = os.Stat(pname);

                                if (err == null)
                                {
                                    isshlib = true;
                                    break;
                                }

                            }

                        }

                        pname = filepath.Join(dir, name);
                        {
                            (_, err) = os.Stat(pname);

                            if (err == null)
                            {
                                break;
                            }

                        }

                    }

                }

                pname = filepath.Clean(pname);

            }

            return (pname, isshlib);

        }

        private static ptr<sym.Library> addlib(ptr<Link> _addr_ctxt, @string src, @string obj, @string lib, goobj2.FingerprintType fingerprint)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            var pkg = pkgname(_addr_ctxt, lib); 

            // already loaded?
            {
                var l = ctxt.LibraryByPkg[pkg];

                if (l != null && !l.Fingerprint.IsZero())
                { 
                    // Normally, packages are loaded in dependency order, and if l != nil
                    // l is already loaded with the actual fingerprint. In shared build mode,
                    // however, packages may be added not in dependency order, and it is
                    // possible that l's fingerprint is not yet loaded -- exclude it in
                    // checking.
                    checkFingerprint(l, l.Fingerprint, src, fingerprint);
                    return _addr_l!;

                }

            }


            var (pname, isshlib) = findlib(_addr_ctxt, lib);

            if (ctxt.Debugvlog > 1L)
            {
                ctxt.Logf("addlib: %s %s pulls in %s isshlib %v\n", obj, src, pname, isshlib);
            }

            if (isshlib)
            {
                return _addr_addlibpath(_addr_ctxt, src, obj, "", pkg, pname, fingerprint)!;
            }

            return _addr_addlibpath(_addr_ctxt, src, obj, pname, pkg, "", fingerprint)!;

        }

        /*
         * add library to library list, return added library.
         *    srcref: src file referring to package
         *    objref: object file referring to package
         *    file: object file, e.g., /home/rsc/go/pkg/container/vector.a
         *    pkg: package import path, e.g. container/vector
         *    shlib: path to shared library, or .shlibname file holding path
         *    fingerprint: if not 0, expected fingerprint for import from srcref
         *                 fingerprint is 0 if the library is not imported (e.g. main)
         */
        private static ptr<sym.Library> addlibpath(ptr<Link> _addr_ctxt, @string srcref, @string objref, @string file, @string pkg, @string shlib, goobj2.FingerprintType fingerprint)
        {
            ref Link ctxt = ref _addr_ctxt.val;

            {
                var l__prev1 = l;

                var l = ctxt.LibraryByPkg[pkg];

                if (l != null)
                {
                    return _addr_l!;
                }

                l = l__prev1;

            }


            if (ctxt.Debugvlog > 1L)
            {
                ctxt.Logf("addlibpath: srcref: %s objref: %s file: %s pkg: %s shlib: %s fingerprint: %x\n", srcref, objref, file, pkg, shlib, fingerprint);
            }

            l = addr(new sym.Library());
            ctxt.LibraryByPkg[pkg] = l;
            ctxt.Library = append(ctxt.Library, l);
            l.Objref = objref;
            l.Srcref = srcref;
            l.File = file;
            l.Pkg = pkg;
            l.Fingerprint = fingerprint;
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

            return _addr_l!;

        }

        private static long atolwhex(@string s)
        {
            var (n, _) = strconv.ParseInt(s, 0L, 64L);
            return n;
        }

        // PrepareAddmoduledata returns a symbol builder that target-specific
        // code can use to build up the linker-generated go.link.addmoduledata
        // function, along with the sym for runtime.addmoduledata itself. If
        // this function is not needed (for example in cases where we're
        // linking a module that contains the runtime) the returned builder
        // will be nil.
        public static (ptr<loader.SymbolBuilder>, loader.Sym) PrepareAddmoduledata(ptr<Link> _addr_ctxt)
        {
            ptr<loader.SymbolBuilder> _p0 = default!;
            loader.Sym _p0 = default;
            ref Link ctxt = ref _addr_ctxt.val;

            if (!ctxt.DynlinkingGo())
            {
                return (_addr_null!, 0L);
            }

            var amd = ctxt.loader.LookupOrCreateSym("runtime.addmoduledata", 0L);
            if (ctxt.loader.SymType(amd) == sym.STEXT && ctxt.BuildMode != BuildModePlugin)
            { 
                // we're linking a module containing the runtime -> no need for
                // an init function
                return (_addr_null!, 0L);

            }

            ctxt.loader.SetAttrReachable(amd, true); 

            // Create a new init func text symbol. Caller will populate this
            // sym with arch-specific content.
            var ifs = ctxt.loader.LookupOrCreateSym("go.link.addmoduledata", 0L);
            var initfunc = ctxt.loader.MakeSymbolUpdater(ifs);
            ctxt.loader.SetAttrReachable(ifs, true);
            ctxt.loader.SetAttrLocal(ifs, true);
            initfunc.SetType(sym.STEXT); 

            // Add the init func and/or addmoduledata to Textp2.
            if (ctxt.BuildMode == BuildModePlugin)
            {
                ctxt.Textp2 = append(ctxt.Textp2, amd);
            }

            ctxt.Textp2 = append(ctxt.Textp2, initfunc.Sym()); 

            // Create an init array entry
            var amdi = ctxt.loader.LookupOrCreateSym("go.link.addmoduledatainit", 0L);
            var initarray_entry = ctxt.loader.MakeSymbolUpdater(amdi);
            ctxt.loader.SetAttrReachable(amdi, true);
            ctxt.loader.SetAttrLocal(amdi, true);
            initarray_entry.SetType(sym.SINITARR);
            initarray_entry.AddAddr(ctxt.Arch, ifs);

            return (_addr_initfunc!, amd);

        }
    }
}}}}
