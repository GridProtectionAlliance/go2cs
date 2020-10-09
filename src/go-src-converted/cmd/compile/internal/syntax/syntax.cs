// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syntax -- go2cs converted at 2020 October 09 05:41:06 UTC
// import "cmd/compile/internal/syntax" ==> using syntax = go.cmd.compile.@internal.syntax_package
// Original source: C:\Go\src\cmd\compile\internal\syntax\syntax.go
using fmt = go.fmt_package;
using io = go.io_package;
using os = go.os_package;
using static go.builtin;
using System;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class syntax_package
    {
        // Mode describes the parser mode.
        public partial struct Mode // : ulong
        {
        }

        // Modes supported by the parser.
        public static readonly Mode CheckBranches = (Mode)1L << (int)(iota); // check correct use of labels, break, continue, and goto statements

        // Error describes a syntax error. Error implements the error interface.
        public partial struct Error
        {
            public Pos Pos;
            public @string Msg;
        }

        public static @string Error(this Error err)
        {
            return fmt.Sprintf("%s: %s", err.Pos, err.Msg);
        }

        private static error _ = error.As(new Error())!; // verify that Error implements error

        // An ErrorHandler is called for each error encountered reading a .go file.
        public delegate void ErrorHandler(error);

        // A Pragma value augments a package, import, const, func, type, or var declaration.
        // Its meaning is entirely up to the PragmaHandler,
        // except that nil is used to mean “no pragma seen.”
        public partial interface Pragma
        {
        }

        // A PragmaHandler is used to process //go: directives while scanning.
        // It is passed the current pragma value, which starts out being nil,
        // and it returns an updated pragma value.
        // The text is the directive, with the "//" prefix stripped.
        // The current pragma is saved at each package, import, const, func, type, or var
        // declaration, into the File, ImportDecl, ConstDecl, FuncDecl, TypeDecl, or VarDecl node.
        //
        // If text is the empty string, the pragma is being returned
        // to the handler unused, meaning it appeared before a non-declaration.
        // The handler may wish to report an error. In this case, pos is the
        // current parser position, not the position of the pragma itself.
        // Blank specifies whether the line is blank before the pragma.
        public delegate  Pragma PragmaHandler(Pos,  bool,  @string,  Pragma);

        // Parse parses a single Go source file from src and returns the corresponding
        // syntax tree. If there are errors, Parse will return the first error found,
        // and a possibly partially constructed syntax tree, or nil.
        //
        // If errh != nil, it is called with each error encountered, and Parse will
        // process as much source as possible. In this case, the returned syntax tree
        // is only nil if no correct package clause was found.
        // If errh is nil, Parse will terminate immediately upon encountering the first
        // error, and the returned syntax tree is nil.
        //
        // If pragh != nil, it is called with each pragma encountered.
        //
        public static (ptr<File>, error) Parse(ptr<PosBase> _addr_@base, io.Reader src, ErrorHandler errh, PragmaHandler pragh, Mode mode) => func((defer, panic, _) =>
        {
            ptr<File> _ = default!;
            error first = default!;
            ref PosBase @base = ref _addr_@base.val;

            defer(() =>
            {
                {
                    var p__prev1 = p;

                    var p = recover();

                    if (p != null)
                    {
                        {
                            Error (err, ok) = p._<Error>();

                            if (ok)
                            {
                                first = err;
                                return ;
                            }

                        }

                        panic(p);

                    }

                    p = p__prev1;

                }

            }());

            p = default;
            p.init(base, src, errh, pragh, mode);
            p.next();
            return (_addr_p.fileOrNil()!, error.As(p.first)!);

        });

        // ParseFile behaves like Parse but it reads the source from the named file.
        public static (ptr<File>, error) ParseFile(@string filename, ErrorHandler errh, PragmaHandler pragh, Mode mode) => func((defer, _, __) =>
        {
            ptr<File> _p0 = default!;
            error _p0 = default!;

            var (f, err) = os.Open(filename);
            if (err != null)
            {
                if (errh != null)
                {
                    errh(err);
                }

                return (_addr_null!, error.As(err)!);

            }

            defer(f.Close());
            return _addr_Parse(_addr_NewFileBase(filename), f, errh, pragh, mode)!;

        });
    }
}}}}
