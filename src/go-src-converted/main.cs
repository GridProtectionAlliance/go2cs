// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// The gopackages command is a diagnostic tool that demonstrates
// how to use golang.org/x/tools/go/packages to load, parse,
// type-check, and print one or more Go packages.
// Its precise output is unspecified and may change.
// package main -- go2cs converted at 2020 October 08 04:55:40 UTC
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\packages\gopackages\main.go
using context = go.context_package;
using json = go.encoding.json_package;
using flag = go.flag_package;
using fmt = go.fmt_package;
using types = go.go.types_package;
using os = go.os_package;
using sort = go.sort_package;
using strings = go.strings_package;

using packages = go.golang.org.x.tools.go.packages_package;
using typeutil = go.golang.org.x.tools.go.types.typeutil_package;
using tool = go.golang.org.x.tools.@internal.tool_package;
using static go.builtin;
using System.ComponentModel;
using System;

namespace go
{
    public static partial class main_package
    {
        private static void Main()
        {
            tool.Main(context.Background(), addr(new application(Mode:"imports")), os.Args[1L..]);
        }

        private partial struct application
        {
            public ref tool.Profile Profile => ref Profile_val;
            [Description("flag:\"deps\" help:\"show dependencies too\"")]
            public bool Deps;
            [Description("flag:\"test\" help:\"include any tests implied by the patterns\"")]
            public bool Test;
            [Description("flag:\"mode\" help:\"mode (one of files, imports, types, syntax, allsyntax)\"")]
            public @string Mode;
            [Description("flag:\"private\" help:\"show non-exported declarations too\"")]
            public bool Private;
            [Description("flag:\"json\" help:\"print package in JSON form\"")]
            public bool PrintJSON;
            [Description("flag:\"buildflag\" help:\"pass argument to underlying build system (may be repeated)" +
    "\"")]
            public stringListValue BuildFlags;
        }

        // Name implements tool.Application returning the binary name.
        private static @string Name(this ptr<application> _addr_app)
        {
            ref application app = ref _addr_app.val;

            return "gopackages";
        }

        // Usage implements tool.Application returning empty extra argument usage.
        private static @string Usage(this ptr<application> _addr_app)
        {
            ref application app = ref _addr_app.val;

            return "package...";
        }

        // ShortHelp implements tool.Application returning the main binary help.
        private static @string ShortHelp(this ptr<application> _addr_app)
        {
            ref application app = ref _addr_app.val;

            return "gopackages loads, parses, type-checks, and prints one or more Go packages.";
        }

        // DetailedHelp implements tool.Application returning the main binary help.
        private static void DetailedHelp(this ptr<application> _addr_app, ptr<flag.FlagSet> _addr_f)
        {
            ref application app = ref _addr_app.val;
            ref flag.FlagSet f = ref _addr_f.val;

            fmt.Fprint(f.Output(), "\nPackages are specified using the notation of \"go list\",\nor other underlying buil" +
    "d system.\n\nFlags:\n");
            f.PrintDefaults();

        }

        // Run takes the args after flag processing and performs the specified query.
        private static error Run(this ptr<application> _addr_app, context.Context ctx, params @string[] args)
        {
            args = args.Clone();
            ref application app = ref _addr_app.val;

            if (len(args) == 0L)
            {
                return error.As(tool.CommandLineErrorf("not enough arguments"))!;
            } 

            // Load, parse, and type-check the packages named on the command line.
            ptr<packages.Config> cfg = addr(new packages.Config(Mode:packages.LoadSyntax,Tests:app.Test,BuildFlags:app.BuildFlags,)); 

            // -mode flag
            switch (strings.ToLower(app.Mode))
            {
                case "files": 
                    cfg.Mode = packages.LoadFiles;
                    break;
                case "imports": 
                    cfg.Mode = packages.LoadImports;
                    break;
                case "types": 
                    cfg.Mode = packages.LoadTypes;
                    break;
                case "syntax": 
                    cfg.Mode = packages.LoadSyntax;
                    break;
                case "allsyntax": 
                    cfg.Mode = packages.LoadAllSyntax;
                    break;
                default: 
                    return error.As(tool.CommandLineErrorf("invalid mode: %s", app.Mode))!;
                    break;
            }

            var (lpkgs, err) = packages.Load(cfg, args);
            if (err != null)
            {
                return error.As(err)!;
            } 

            // -deps: print dependencies too.
            if (app.Deps)
            { 
                // We can't use packages.All because
                // we need an ordered traversal.
                slice<ptr<packages.Package>> all = default; // postorder
                var seen = make_map<ptr<packages.Package>, bool>();
                Action<ptr<packages.Package>> visit = default;
                visit = lpkg =>
                {
                    if (!seen[lpkg])
                    {
                        seen[lpkg] = true; 

                        // visit imports
                        slice<@string> importPaths = default;
                        {
                            var path__prev1 = path;

                            foreach (var (__path) in lpkg.Imports)
                            {
                                path = __path;
                                importPaths = append(importPaths, path);
                            }

                            path = path__prev1;
                        }

                        sort.Strings(importPaths); // for determinism
                        {
                            var path__prev1 = path;

                            foreach (var (_, __path) in importPaths)
                            {
                                path = __path;
                                visit(lpkg.Imports[path]);
                            }

                            path = path__prev1;
                        }

                        all = append(all, lpkg);

                    }

                }
;
                {
                    var lpkg__prev1 = lpkg;

                    foreach (var (_, __lpkg) in lpkgs)
                    {
                        lpkg = __lpkg;
                        visit(lpkg);
                    }

                    lpkg = lpkg__prev1;
                }

                lpkgs = all;

            }

            {
                var lpkg__prev1 = lpkg;

                foreach (var (_, __lpkg) in lpkgs)
                {
                    lpkg = __lpkg;
                    app.print(lpkg);
                }

                lpkg = lpkg__prev1;
            }

            return error.As(null!)!;

        }

        private static void print(this ptr<application> _addr_app, ptr<packages.Package> _addr_lpkg)
        {
            ref application app = ref _addr_app.val;
            ref packages.Package lpkg = ref _addr_lpkg.val;

            if (app.PrintJSON)
            {
                var (data, _) = json.MarshalIndent(lpkg, "", "\t");
                os.Stdout.Write(data);
                return ;
            } 
            // title
            @string kind = default; 
            // TODO(matloob): If IsTest is added back print "test command" or
            // "test package" for packages with IsTest == true.
            if (lpkg.Name == "main")
            {
                kind += "command";
            }
            else
            {
                kind += "package";
            }

            fmt.Printf("Go %s %q:\n", kind, lpkg.ID); // unique ID
            fmt.Printf("\tpackage %s\n", lpkg.Name); 

            // characterize type info
            if (lpkg.Types == null)
            {
                fmt.Printf("\thas no exported type info\n");
            }
            else if (!lpkg.Types.Complete())
            {
                fmt.Printf("\thas incomplete exported type info\n");
            }
            else if (len(lpkg.Syntax) == 0L)
            {
                fmt.Printf("\thas complete exported type info\n");
            }
            else
            {
                fmt.Printf("\thas complete exported type info and typed ASTs\n");
            }

            if (lpkg.Types != null && lpkg.IllTyped && len(lpkg.Errors) == 0L)
            {
                fmt.Printf("\thas an error among its dependencies\n");
            } 

            // source files
            foreach (var (_, src) in lpkg.GoFiles)
            {
                fmt.Printf("\tfile %s\n", src);
            } 

            // imports
            slice<@string> lines = default;
            foreach (var (importPath, imp) in lpkg.Imports)
            {
                @string line = default;
                if (imp.ID == importPath)
                {
                    line = fmt.Sprintf("\timport %q", importPath);
                }
                else
                {
                    line = fmt.Sprintf("\timport %q => %q", importPath, imp.ID);
                }

                lines = append(lines, line);

            }
            sort.Strings(lines);
            {
                @string line__prev1 = line;

                foreach (var (_, __line) in lines)
                {
                    line = __line;
                    fmt.Println(line);
                } 

                // errors

                line = line__prev1;
            }

            foreach (var (_, err) in lpkg.Errors)
            {
                fmt.Printf("\t%s\n", err);
            } 

            // package members (TypeCheck or WholeProgram mode)
            if (lpkg.Types != null)
            {
                var qual = types.RelativeTo(lpkg.Types);
                var scope = lpkg.Types.Scope();
                foreach (var (_, name) in scope.Names())
                {
                    var obj = scope.Lookup(name);
                    if (!obj.Exported() && !app.Private)
                    {
                        continue; // skip unexported names
                    }

                    fmt.Printf("\t%s\n", types.ObjectString(obj, qual));
                    {
                        ptr<types.TypeName> (_, ok) = obj._<ptr<types.TypeName>>();

                        if (ok)
                        {
                            foreach (var (_, meth) in typeutil.IntuitiveMethodSet(obj.Type(), null))
                            {
                                if (!meth.Obj().Exported() && !app.Private)
                                {
                                    continue; // skip unexported names
                                }

                                fmt.Printf("\t%s\n", types.SelectionString(meth, qual));

                            }

                        }

                    }

                }

            }

            fmt.Println();

        }

        // stringListValue is a flag.Value that accumulates strings.
        // e.g. --flag=one --flag=two would produce []string{"one", "two"}.
        private partial struct stringListValue // : slice<@string>
        {
        }

        private static ptr<stringListValue> newStringListValue(slice<@string> val, ptr<slice<@string>> _addr_p)
        {
            ref slice<@string> p = ref _addr_p.val;

            p = val;
            return _addr_(stringListValue.val)(p)!;
        }

        private static void Get(this ptr<stringListValue> _addr_ss)
        {
            ref stringListValue ss = ref _addr_ss.val;

            return (slice<@string>)ss.val;
        }

        private static @string String(this ptr<stringListValue> _addr_ss)
        {
            ref stringListValue ss = ref _addr_ss.val;

            return fmt.Sprintf("%q", ss.val);
        }

        private static error Set(this ptr<stringListValue> _addr_ss, @string s)
        {
            ref stringListValue ss = ref _addr_ss.val;

            ss.val = append(ss.val, s);

            return error.As(null!)!;
        }
    }
}
