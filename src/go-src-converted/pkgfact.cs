// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// The pkgfact package is a demonstration and test of the package fact
// mechanism.
//
// The output of the pkgfact analysis is a set of key/values pairs
// gathered from the analyzed package and its imported dependencies.
// Each key/value pair comes from a top-level constant declaration
// whose name starts and ends with "_".  For example:
//
//      package p
//
//     const _greeting_  = "hello"
//     const _audience_  = "world"
//
// the pkgfact analysis output for package p would be:
//
//   {"greeting": "hello", "audience": "world"}.
//
// In addition, the analysis reports a diagnostic at each import
// showing which key/value pairs it contributes.
// package pkgfact -- go2cs converted at 2020 October 09 06:04:06 UTC
// import "golang.org/x/tools/go/analysis/passes/pkgfact" ==> using pkgfact = go.golang.org.x.tools.go.analysis.passes.pkgfact_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\go\analysis\passes\pkgfact\pkgfact.go
using fmt = go.fmt_package;
using ast = go.go.ast_package;
using token = go.go.token_package;
using types = go.go.types_package;
using reflect = go.reflect_package;
using sort = go.sort_package;
using strings = go.strings_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using static go.builtin;
using System;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace analysis {
namespace passes
{
    public static partial class pkgfact_package
    {
        public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"pkgfact",Doc:"gather name/value pairs from constant declarations",Run:run,FactTypes:[]analysis.Fact{new(pairsFact)},ResultType:reflect.TypeOf(map[string]string{}),));

        // A pairsFact is a package-level fact that records
        // an set of key=value strings accumulated from constant
        // declarations in this package and its dependencies.
        // Elements are ordered by keys, which are unique.
        private partial struct pairsFact // : slice<@string>
        {
        }

        private static void AFact(this ptr<pairsFact> _addr_f)
        {
            ref pairsFact f = ref _addr_f.val;

        }
        private static @string String(this ptr<pairsFact> _addr_f)
        {
            ref pairsFact f = ref _addr_f.val;

            return "pairs(" + strings.Join(f.val, ", ") + ")";
        }

        private static (object, error) run(ptr<analysis.Pass> _addr_pass)
        {
            object _p0 = default;
            error _p0 = default!;
            ref analysis.Pass pass = ref _addr_pass.val;

            var result = make_map<@string, @string>(); 

            // At each import, print the fact from the imported
            // package and accumulate its information into the result.
            // (Warning: accumulation leads to quadratic growth of work.)
            Action<ptr<ast.ImportSpec>> doImport = spec =>
            {
                var pkg = imported(_addr_pass.TypesInfo, _addr_spec);
                ref pairsFact fact = ref heap(out ptr<pairsFact> _addr_fact);
                if (pass.ImportPackageFact(pkg, _addr_fact))
                {
                    foreach (var (_, pair) in fact)
                    {
                        var eq = strings.IndexByte(pair, '=');
                        result[pair[..eq]] = pair[1L + eq..];
                    }
                    pass.ReportRangef(spec, "%s", strings.Join(fact, " "));

                }

            } 

            // At each "const _name_ = value", add a fact into env.
; 

            // At each "const _name_ = value", add a fact into env.
            Action<ptr<ast.ValueSpec>> doConst = spec =>
            {
                if (len(spec.Names) == len(spec.Values))
                {
                    foreach (var (i) in spec.Names)
                    {
                        var name = spec.Names[i].Name;
                        if (strings.HasPrefix(name, "_") && strings.HasSuffix(name, "_"))
                        {
                            {
                                var key__prev3 = key;

                                var key = strings.Trim(name, "_");

                                if (key != "")
                                {
                                    var value = pass.TypesInfo.Types[spec.Values[i]].Value.String();
                                    result[key] = value;
                                }

                                key = key__prev3;

                            }

                        }

                    }

                }

            }
;

            foreach (var (_, f) in pass.Files)
            {
                {
                    var decl__prev2 = decl;

                    foreach (var (_, __decl) in f.Decls)
                    {
                        decl = __decl;
                        {
                            var decl__prev1 = decl;

                            ptr<ast.GenDecl> (decl, ok) = decl._<ptr<ast.GenDecl>>();

                            if (ok)
                            {
                                foreach (var (_, spec) in decl.Specs)
                                {

                                    if (decl.Tok == token.IMPORT) 
                                        doImport(spec._<ptr<ast.ImportSpec>>());
                                    else if (decl.Tok == token.CONST) 
                                        doConst(spec._<ptr<ast.ValueSpec>>());
                                    
                                }

                            }

                            decl = decl__prev1;

                        }

                    }

                    decl = decl__prev2;
                }
            } 

            // Sort/deduplicate the result and save it as a package fact.
            var keys = make_slice<@string>(0L, len(result));
            {
                var key__prev1 = key;

                foreach (var (__key) in result)
                {
                    key = __key;
                    keys = append(keys, key);
                }

                key = key__prev1;
            }

            sort.Strings(keys);
            fact = default;
            {
                var key__prev1 = key;

                foreach (var (_, __key) in keys)
                {
                    key = __key;
                    fact = append(fact, fmt.Sprintf("%s=%s", key, result[key]));
                }

                key = key__prev1;
            }

            if (len(fact) > 0L)
            {
                pass.ExportPackageFact(_addr_fact);
            }

            return (result, error.As(null!)!);

        }

        private static ptr<types.Package> imported(ptr<types.Info> _addr_info, ptr<ast.ImportSpec> _addr_spec)
        {
            ref types.Info info = ref _addr_info.val;
            ref ast.ImportSpec spec = ref _addr_spec.val;

            var (obj, ok) = info.Implicits[spec];
            if (!ok)
            {
                obj = info.Defs[spec.Name]; // renaming import
            }

            return obj._<ptr<types.PkgName>>().Imported();

        }
    }
}}}}}}}
