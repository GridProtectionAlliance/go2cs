// Copyright 2018 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// The unmarshal package defines an Analyzer that checks for passing
// non-pointer or non-interface types to unmarshal and decode functions.
// package unmarshal -- go2cs converted at 2020 October 08 04:58:13 UTC
// import "cmd/vendor/golang.org/x/tools/go/analysis/passes/unmarshal" ==> using unmarshal = go.cmd.vendor.golang.org.x.tools.go.analysis.passes.unmarshal_package
// Original source: C:\Go\src\cmd\vendor\golang.org\x\tools\go\analysis\passes\unmarshal\unmarshal.go
using ast = go.go.ast_package;
using types = go.go.types_package;

using analysis = go.golang.org.x.tools.go.analysis_package;
using inspect = go.golang.org.x.tools.go.analysis.passes.inspect_package;
using inspector = go.golang.org.x.tools.go.ast.inspector_package;
using typeutil = go.golang.org.x.tools.go.types.typeutil_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace vendor {
namespace golang.org {
namespace x {
namespace tools {
namespace go {
namespace analysis {
namespace passes
{
    public static partial class unmarshal_package
    {
        public static readonly @string Doc = (@string)"report passing non-pointer or non-interface values to unmarshal\n\nThe unmarshal an" +
    "alysis reports calls to functions such as json.Unmarshal\nin which the argument t" +
    "ype is not a pointer or an interface.";



        public static ptr<analysis.Analyzer> Analyzer = addr(new analysis.Analyzer(Name:"unmarshal",Doc:Doc,Requires:[]*analysis.Analyzer{inspect.Analyzer},Run:run,));

        private static (object, error) run(ptr<analysis.Pass> _addr_pass)
        {
            object _p0 = default;
            error _p0 = default!;
            ref analysis.Pass pass = ref _addr_pass.val;

            switch (pass.Pkg.Path())
            {
                case "encoding/gob": 
                    // These packages know how to use their own APIs.
                    // Sometimes they are testing what happens to incorrect programs.

                case "encoding/json": 
                    // These packages know how to use their own APIs.
                    // Sometimes they are testing what happens to incorrect programs.

                case "encoding/xml": 
                    // These packages know how to use their own APIs.
                    // Sometimes they are testing what happens to incorrect programs.
                    return (null, error.As(null!)!);
                    break;
            }

            ptr<inspector.Inspector> inspect = pass.ResultOf[inspect.Analyzer]._<ptr<inspector.Inspector>>();

            ast.Node nodeFilter = new slice<ast.Node>(new ast.Node[] { (*ast.CallExpr)(nil) });
            inspect.Preorder(nodeFilter, n =>
            {
                ptr<ast.CallExpr> call = n._<ptr<ast.CallExpr>>();
                var fn = typeutil.StaticCallee(pass.TypesInfo, call);
                if (fn == null)
                {
                    return ; // not a static call
                } 

                // Classify the callee (without allocating memory).
                long argidx = -1L;
                ptr<types.Signature> recv = fn.Type()._<ptr<types.Signature>>().Recv();
                if (fn.Name() == "Unmarshal" && recv == null)
                { 
                    // "encoding/json".Unmarshal
                    //  "encoding/xml".Unmarshal
                    switch (fn.Pkg().Path())
                    {
                        case "encoding/json": 

                        case "encoding/xml": 
                            argidx = 1L; // func([]byte, interface{})
                            break;
                    }

                }
                else if (fn.Name() == "Decode" && recv != null)
                { 
                    // (*"encoding/json".Decoder).Decode
                    // (* "encoding/gob".Decoder).Decode
                    // (* "encoding/xml".Decoder).Decode
                    var t = recv.Type();
                    {
                        ptr<types.Pointer> (ptr, ok) = t._<ptr<types.Pointer>>();

                        if (ok)
                        {
                            t = ptr.Elem();
                        }

                    }

                    ptr<types.Named> tname = t._<ptr<types.Named>>().Obj();
                    if (tname.Name() == "Decoder")
                    {
                        switch (tname.Pkg().Path())
                        {
                            case "encoding/json": 

                            case "encoding/xml": 

                            case "encoding/gob": 
                                argidx = 0L; // func(interface{})
                                break;
                        }

                    }

                }

                if (argidx < 0L)
                {
                    return ; // not a function we are interested in
                }

                if (len(call.Args) < argidx + 1L)
                {
                    return ; // not enough arguments, e.g. called with return values of another function
                }

                t = pass.TypesInfo.Types[call.Args[argidx]].Type;
                switch (t.Underlying().type())
                {
                    case ptr<types.Pointer> _:
                        return ;
                        break;
                    case ptr<types.Interface> _:
                        return ;
                        break;

                }

                switch (argidx)
                {
                    case 0L: 
                        pass.Reportf(call.Lparen, "call of %s passes non-pointer", fn.Name());
                        break;
                    case 1L: 
                        pass.Reportf(call.Lparen, "call of %s passes non-pointer as second argument", fn.Name());
                        break;
                }

            });
            return (null, error.As(null!)!);

        }
    }
}}}}}}}}}
