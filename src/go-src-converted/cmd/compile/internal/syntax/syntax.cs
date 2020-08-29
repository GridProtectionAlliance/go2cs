// Copyright 2016 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package syntax -- go2cs converted at 2020 August 29 09:26:27 UTC
// import "cmd/compile/internal/syntax" ==> using syntax = go.cmd.compile.@internal.syntax_package
// Original source: C:\Go\src\cmd\compile\internal\syntax\syntax.go
using src = go.cmd.@internal.src_package;
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
        public static readonly Mode CheckBranches = 1L << (int)(iota); // check correct use of labels, break, continue, and goto statements

        // Error describes a syntax error. Error implements the error interface.
        public partial struct Error
        {
            public src.Pos Pos;
            public @string Msg;
        }

        public static @string Error(this Error err)
        {
            return fmt.Sprintf("%s: %s", err.Pos, err.Msg);
        }

        private static error _ = error.As(new Error()); // verify that Error implements error

        // An ErrorHandler is called for each error encountered reading a .go file.
        public delegate void ErrorHandler(error);

        // A Pragma value is a set of flags that augment a function or
        // type declaration. Callers may assign meaning to the flags as
        // appropriate.
        public partial struct Pragma // : ushort
        {
        }

        // A PragmaHandler is used to process //go: directives as
        // they're scanned. The returned Pragma value will be unioned into the
        // next FuncDecl node.
        public delegate  Pragma PragmaHandler(src.Pos,  @string);

        // A FilenameHandler is used to process each filename encountered
        // in //line directives. The returned value is used as the absolute filename.
        public delegate  @string FilenameHandler(@string);

        // Parse parses a single Go source file from src and returns the corresponding
        // syntax tree. If there are errors, Parse will return the first error found,
        // and a possibly partially constructed syntax tree, or nil if no correct package
        // clause was found. The base argument is only used for position information.
        //
        // If errh != nil, it is called with each error encountered, and Parse will
        // process as much source as possible. If errh is nil, Parse will terminate
        // immediately upon encountering an error.
        //
        // If a PragmaHandler is provided, it is called with each pragma encountered.
        //
        // If a FilenameHandler is provided, it is called to process each filename
        // encountered in //line directives.
        //
        // The Mode argument is currently ignored.
        public static (ref File, error) Parse(ref src.PosBase _@base, io.Reader src, ErrorHandler errh, PragmaHandler pragh, FilenameHandler fileh, Mode mode) => func(_@base, (ref src.PosBase @base, Defer defer, Panic panic, Recover _) =>
        {
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
                                return;
                            }

                        }
                        panic(p);
                    }

                    p = p__prev1;

                }
            }());

            p = default;
            p.init(base, src, errh, pragh, fileh, mode);
            p.next();
            return (p.fileOrNil(), p.first);
        });

        // ParseBytes behaves like Parse but it reads the source from the []byte slice provided.
        public static (ref File, error) ParseBytes(ref src.PosBase @base, slice<byte> src, ErrorHandler errh, PragmaHandler pragh, FilenameHandler fileh, Mode mode)
        {
            return Parse(base, ref new bytesReader(src), errh, pragh, fileh, mode);
        }

        private partial struct bytesReader
        {
            public slice<byte> data;
        }

        private static (long, error) Read(this ref bytesReader r, slice<byte> p)
        {
            if (len(r.data) > 0L)
            {
                var n = copy(p, r.data);
                r.data = r.data[n..];
                return (n, null);
            }
            return (0L, io.EOF);
        }

        // ParseFile behaves like Parse but it reads the source from the named file.
        public static (ref File, error) ParseFile(@string filename, ErrorHandler errh, PragmaHandler pragh, Mode mode) => func((defer, _, __) =>
        {
            var (f, err) = os.Open(filename);
            if (err != null)
            {
                if (errh != null)
                {
                    errh(err);
                }
                return (null, err);
            }
            defer(f.Close());
            return Parse(src.NewFileBase(filename, filename), f, errh, pragh, null, mode);
        });
    }
}}}}
