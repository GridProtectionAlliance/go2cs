// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package scanner -- go2cs converted at 2020 August 29 08:48:23 UTC
// import "go/scanner" ==> using scanner = go.go.scanner_package
// Original source: C:\Go\src\go\scanner\errors.go
using fmt = go.fmt_package;
using token = go.go.token_package;
using io = go.io_package;
using sort = go.sort_package;
using static go.builtin;

namespace go {
namespace go
{
    public static partial class scanner_package
    {
        // In an ErrorList, an error is represented by an *Error.
        // The position Pos, if valid, points to the beginning of
        // the offending token, and the error condition is described
        // by Msg.
        //
        public partial struct Error
        {
            public token.Position Pos;
            public @string Msg;
        }

        // Error implements the error interface.
        public static @string Error(this Error e)
        {
            if (e.Pos.Filename != "" || e.Pos.IsValid())
            { 
                // don't print "<unknown position>"
                // TODO(gri) reconsider the semantics of Position.IsValid
                return e.Pos.String() + ": " + e.Msg;
            }
            return e.Msg;
        }

        // ErrorList is a list of *Errors.
        // The zero value for an ErrorList is an empty ErrorList ready to use.
        //
        public partial struct ErrorList // : slice<ref Error>
        {
        }

        // Add adds an Error with given position and error message to an ErrorList.
        private static void Add(this ref ErrorList p, token.Position pos, @string msg)
        {
            p.Value = append(p.Value, ref new Error(pos,msg));
        }

        // Reset resets an ErrorList to no errors.
        private static void Reset(this ref ErrorList p)
        {
            p.Value = (p.Value)[0L..0L];

        }

        // ErrorList implements the sort Interface.
        public static long Len(this ErrorList p)
        {
            return len(p);
        }
        public static void Swap(this ErrorList p, long i, long j)
        {
            p[i] = p[j];
            p[j] = p[i];

        }

        public static bool Less(this ErrorList p, long i, long j)
        {
            var e = ref p[i].Pos;
            var f = ref p[j].Pos; 
            // Note that it is not sufficient to simply compare file offsets because
            // the offsets do not reflect modified line information (through //line
            // comments).
            if (e.Filename != f.Filename)
            {
                return e.Filename < f.Filename;
            }
            if (e.Line != f.Line)
            {
                return e.Line < f.Line;
            }
            if (e.Column != f.Column)
            {
                return e.Column < f.Column;
            }
            return p[i].Msg < p[j].Msg;
        }

        // Sort sorts an ErrorList. *Error entries are sorted by position,
        // other errors are sorted by error message, and before any *Error
        // entry.
        //
        public static void Sort(this ErrorList p)
        {
            sort.Sort(p);
        }

        // RemoveMultiples sorts an ErrorList and removes all but the first error per line.
        private static void RemoveMultiples(this ref ErrorList p)
        {
            sort.Sort(p);
            token.Position last = default; // initial last.Line is != any legal error line
            long i = 0L;
            foreach (var (_, e) in p.Value)
            {
                if (e.Pos.Filename != last.Filename || e.Pos.Line != last.Line)
                {
                    last = e.Pos;
                    (p.Value)[i] = e;
                    i++;
                }
            }
            (p.Value) = (p.Value)[0L..i];
        }

        // An ErrorList implements the error interface.
        public static @string Error(this ErrorList p)
        {
            switch (len(p))
            {
                case 0L: 
                    return "no errors";
                    break;
                case 1L: 
                    return p[0L].Error();
                    break;
            }
            return fmt.Sprintf("%s (and %d more errors)", p[0L], len(p) - 1L);
        }

        // Err returns an error equivalent to this error list.
        // If the list is empty, Err returns nil.
        public static error Err(this ErrorList p)
        {
            if (len(p) == 0L)
            {
                return error.As(null);
            }
            return error.As(p);
        }

        // PrintError is a utility function that prints a list of errors to w,
        // one error per line, if the err parameter is an ErrorList. Otherwise
        // it prints the err string.
        //
        public static void PrintError(io.Writer w, error err)
        {
            {
                ErrorList (list, ok) = err._<ErrorList>();

                if (ok)
                {
                    foreach (var (_, e) in list)
                    {
                        fmt.Fprintf(w, "%s\n", e);
                    }
                }
                else if (err != null)
                {
                    fmt.Fprintf(w, "%s\n", err);
                }

            }
        }
    }
}}
