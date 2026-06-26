// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
namespace go.go;

using fmt = fmt_package;
using token = go.token_package;
using io = io_package;
using sort = sort_package;

partial class scanner_package {

// In an [ErrorList], an error is represented by an *Error.
// The position Pos, if valid, points to the beginning of
// the offending token, and the error condition is described
// by Msg.
[GoType] partial struct ΔError {
    public go.token_package.ΔPosition Pos;
    public @string Msg;
}

// Error implements the error interface.
public static @string Error(this ΔError e) {
    if (e.Pos.Filename != ""u8 || e.Pos.IsValid()) {
        // don't print "<unknown position>"
        // TODO(gri) reconsider the semantics of Position.IsValid
        return e.Pos.String() + ": "u8 + e.Msg;
    }
    return e.Msg;
}

[GoType("[]ΔError")] partial struct ErrorList;

// Add adds an [Error] with given position and error message to an [ErrorList].
[GoRecv] public static void Add(this ref ErrorList p, tokenꓸPosition pos, @string msg) {
    p = append(p, Ꮡ(new ΔError(pos, msg)));
}

// Reset resets an [ErrorList] to no errors.
[GoRecv] public static unsafe void Reset(this ref ErrorList p) {
    p = new Span<ж<ErrorList>>((ErrorList**), 0);
}

// [ErrorList] implements the sort Interface.
public static nint Len(this ErrorList p) {
    return len(p);
}

public static void Swap(this ErrorList p, nint i, nint j) {
    (p[i], p[j]) = (p[j], p[i]);
}

public static bool Less(this ErrorList p, nint i, nint j) {
    var e = Ꮡ(p[i].Pos);
    var f = Ꮡ(p[j].Pos);
    // Note that it is not sufficient to simply compare file offsets because
    // the offsets do not reflect modified line information (through //line
    // comments).
    if ((~e).Filename != (~f).Filename) {
        return (~e).Filename < (~f).Filename;
    }
    if ((~e).Line != (~f).Line) {
        return (~e).Line < (~f).Line;
    }
    if ((~e).Column != (~f).Column) {
        return (~e).Column < (~f).Column;
    }
    return p[i].Msg < p[j].Msg;
}

// Sort sorts an [ErrorList]. *[Error] entries are sorted by position,
// other errors are sorted by error message, and before any *[Error]
// entry.
public static void Sort(this ErrorList p) {
    sort.Sort(p);
}

// RemoveMultiples sorts an [ErrorList] and removes all but the first error per line.
[GoRecv] public static unsafe void RemoveMultiples(this ref ErrorList p) {
    sort.Sort(~p);
    tokenꓸPosition last = default!;                          // initial last.Line is != any legal error line
    nint i = 0;
    foreach (var (_, e) in p) {
        if ((~e).Pos.Filename != last.Filename || (~e).Pos.Line != last.Line) {
            last = e.val.Pos;
            (ж<ж<ErrorList>>)[i] = e;
            i++;
        }
    }
    p = new Span<ж<ErrorList>>((ErrorList**), i);
}

// An [ErrorList] implements the error interface.
public static @string Error(this ErrorList p) {
    switch (len(p)) {
    case 0: {
        return "no errors"u8;
    }
    case 1: {
        return p[0].Error();
    }}

    return fmt.Sprintf("%s (and %d more errors)"u8, p[0], len(p) - 1);
}

// Err returns an error equivalent to this error list.
// If the list is empty, Err returns nil.
public static error Err(this ErrorList p) {
    if (len(p) == 0) {
        return default!;
    }
    return p;
}

// PrintError is a utility function that prints a list of errors to w,
// one error per line, if the err parameter is an [ErrorList]. Otherwise
// it prints the err string.
public static void PrintError(io.Writer w, error err) {
    {
        var (list, ok) = err._<ErrorList>(ᐧ); if (ok){
            foreach (var (_, e) in list) {
                fmt.Fprintf(w, "%s\n"u8, e);
            }
        } else 
        if (err != default!) {
            fmt.Fprintf(w, "%s\n"u8, err);
        }
    }
}

} // end scanner_package
