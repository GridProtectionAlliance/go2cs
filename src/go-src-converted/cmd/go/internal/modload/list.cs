// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package modload -- go2cs converted at 2020 October 08 04:35:31 UTC
// import "cmd/go/internal/modload" ==> using modload = go.cmd.go.@internal.modload_package
// Original source: C:\Go\src\cmd\go\internal\modload\list.go
using errors = go.errors_package;
using fmt = go.fmt_package;
using os = go.os_package;
using strings = go.strings_package;

using @base = go.cmd.go.@internal.@base_package;
using cfg = go.cmd.go.@internal.cfg_package;
using modinfo = go.cmd.go.@internal.modinfo_package;
using par = go.cmd.go.@internal.par_package;
using search = go.cmd.go.@internal.search_package;

using module = go.golang.org.x.mod.module_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace go {
namespace @internal
{
    public static partial class modload_package
    {
        public static slice<ptr<modinfo.ModulePublic>> ListModules(slice<@string> args, bool listU, bool listVersions)
        {
            var mods = listModules(args, listVersions);
            if (listU || listVersions)
            {
                par.Work work = default;
                {
                    var m__prev1 = m;

                    foreach (var (_, __m) in mods)
                    {
                        m = __m;
                        work.Add(m);
                        if (m.Replace != null)
                        {
                            work.Add(m.Replace);
                        }
                    }
                    m = m__prev1;
                }

                work.Do(10L, item =>
                {
                    ptr<modinfo.ModulePublic> m = item._<ptr<modinfo.ModulePublic>>();
                    if (listU)
                    {
                        addUpdate(m);
                    }
                    if (listVersions)
                    {
                        addVersions(m);
                    }
                });

            }
            return mods;

        }

        private static slice<ptr<modinfo.ModulePublic>> listModules(slice<@string> args, bool listVersions)
        {
            LoadBuildList();
            if (len(args) == 0L)
            {
                return new slice<ptr<modinfo.ModulePublic>>(new ptr<modinfo.ModulePublic>[] { moduleInfo(buildList[0],true) });
            }

            slice<ptr<modinfo.ModulePublic>> mods = default;
            var matchedBuildList = make_slice<bool>(len(buildList));
            foreach (var (_, arg) in args)
            {
                if (strings.Contains(arg, "\\"))
                {
                    @base.Fatalf("go: module paths never use backslash");
                }

                if (search.IsRelativePath(arg))
                {
                    @base.Fatalf("go: cannot use relative path %s to specify module", arg);
                }

                if (!HasModRoot() && (arg == "all" || strings.Contains(arg, "...")))
                {
                    @base.Fatalf("go: cannot match %q: working directory is not part of a module", arg);
                }

                {
                    var i__prev1 = i;

                    var i = strings.Index(arg, "@");

                    if (i >= 0L)
                    {
                        var path = arg[..i];
                        var vers = arg[i + 1L..];
                        @string current = default;
                        {
                            var m__prev2 = m;

                            foreach (var (_, __m) in buildList)
                            {
                                m = __m;
                                if (m.Path == path)
                                {
                                    current = m.Version;
                                    break;
                                }

                            }

                            m = m__prev2;
                        }

                        var (info, err) = Query(path, vers, current, null);
                        if (err != null)
                        {
                            mods = append(mods, addr(new modinfo.ModulePublic(Path:path,Version:vers,Error:modinfoError(path,vers,err),)));
                            continue;
                        }

                        mods = append(mods, moduleInfo(new module.Version(Path:path,Version:info.Version), false));
                        continue;

                    } 

                    // Module path or pattern.

                    i = i__prev1;

                } 

                // Module path or pattern.
                Func<@string, bool> match = default;
                bool literal = default;
                if (arg == "all")
                {
                    match = _p0 => true;
                }
                else if (strings.Contains(arg, "..."))
                {
                    match = search.MatchPattern(arg);
                }
                else
                {
                    match = p => arg == p;
                    literal = true;
                }

                var matched = false;
                {
                    var i__prev2 = i;
                    var m__prev2 = m;

                    foreach (var (__i, __m) in buildList)
                    {
                        i = __i;
                        m = __m;
                        if (i == 0L && !HasModRoot())
                        { 
                            // The root module doesn't actually exist: omit it.
                            continue;

                        }

                        if (match(m.Path))
                        {
                            matched = true;
                            if (!matchedBuildList[i])
                            {
                                matchedBuildList[i] = true;
                                mods = append(mods, moduleInfo(m, true));
                            }

                        }

                    }

                    i = i__prev2;
                    m = m__prev2;
                }

                if (!matched)
                {
                    if (literal)
                    {
                        if (listVersions)
                        { 
                            // Don't make the user provide an explicit '@latest' when they're
                            // explicitly asking what the available versions are.
                            // Instead, resolve the module, even if it isn't an existing dependency.
                            (info, err) = Query(arg, "latest", "", null);
                            if (err == null)
                            {
                                mods = append(mods, moduleInfo(new module.Version(Path:arg,Version:info.Version), false));
                            }
                            else
                            {
                                mods = append(mods, addr(new modinfo.ModulePublic(Path:arg,Error:modinfoError(arg,"",err),)));
                            }

                            continue;

                        }

                        if (cfg.BuildMod == "vendor")
                        { 
                            // In vendor mode, we can't determine whether a missing module is “a
                            // known dependency” because the module graph is incomplete.
                            // Give a more explicit error message.
                            mods = append(mods, addr(new modinfo.ModulePublic(Path:arg,Error:modinfoError(arg,"",errors.New("can't resolve module using the vendor directory\n\t(Use -mod=mod or -mod=readonly to bypass.)")),)));

                        }
                        else
                        {
                            mods = append(mods, addr(new modinfo.ModulePublic(Path:arg,Error:modinfoError(arg,"",errors.New("not a known dependency")),)));
                        }

                    }
                    else
                    {
                        fmt.Fprintf(os.Stderr, "warning: pattern %q matched no module dependencies\n", arg);
                    }

                }

            }
            return mods;

        }

        // modinfoError wraps an error to create an error message in
        // modinfo.ModuleError with minimal redundancy.
        private static ptr<modinfo.ModuleError> modinfoError(@string path, @string vers, error err)
        {
            ptr<NoMatchingVersionError> nerr;
            ptr<module.ModuleError> merr;
            if (errors.As(err, _addr_nerr))
            { 
                // NoMatchingVersionError contains the query, so we don't mention the
                // query again in ModuleError.
                err = addr(new module.ModuleError(Path:path,Err:err));

            }
            else if (!errors.As(err, _addr_merr))
            { 
                // If the error does not contain path and version, wrap it in a
                // module.ModuleError.
                err = addr(new module.ModuleError(Path:path,Version:vers,Err:err));

            }

            return addr(new modinfo.ModuleError(Err:err.Error()));

        }
    }
}}}}
