// Copyright 2020 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package @base -- go2cs converted at 2022 March 06 23:14:31 UTC
// import "cmd/compile/internal/base" ==> using @base = go.cmd.compile.@internal.@base_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\base\print.go
using fmt = go.fmt_package;
using buildcfg = go.@internal.buildcfg_package;
using os = go.os_package;
using debug = go.runtime.debug_package;
using sort = go.sort_package;
using strings = go.strings_package;

using src = go.cmd.@internal.src_package;

namespace go.cmd.compile.@internal;

public static partial class @base_package {

    // An errorMsg is a queued error message, waiting to be printed.
private partial struct errorMsg {
    public src.XPos pos;
    public @string msg;
}

// Pos is the current source position being processed,
// printed by Errorf, ErrorfLang, Fatalf, and Warnf.
public static src.XPos Pos = default;

private static slice<errorMsg> errorMsgs = default;private static nint numErrors = default;private static nint numSyntaxErrors = default;

// Errors returns the number of errors reported.
public static nint Errors() {
    return numErrors;
}

// SyntaxErrors returns the number of syntax errors reported
public static nint SyntaxErrors() {
    return numSyntaxErrors;
}

// addErrorMsg adds a new errorMsg (which may be a warning) to errorMsgs.
private static void addErrorMsg(src.XPos pos, @string format, params object[] args) {
    args = args.Clone();

    var msg = fmt.Sprintf(format, args); 
    // Only add the position if know the position.
    // See issue golang.org/issue/11361.
    if (pos.IsKnown()) {
        msg = fmt.Sprintf("%v: %s", FmtPos(pos), msg);
    }
    errorMsgs = append(errorMsgs, new errorMsg(pos:pos,msg:msg+"\n",));

}

// FmtPos formats pos as a file:line string.
public static @string FmtPos(src.XPos pos) {
    if (Ctxt == null) {
        return "???";
    }
    return Ctxt.OutermostPos(pos).Format(Flag.C == 0, Flag.L == 1);

}

// byPos sorts errors by source position.
private partial struct byPos { // : slice<errorMsg>
}

private static nint Len(this byPos x) {
    return len(x);
}
private static bool Less(this byPos x, nint i, nint j) {
    return x[i].pos.Before(x[j].pos);
}
private static void Swap(this byPos x, nint i, nint j) {
    (x[i], x[j]) = (x[j], x[i]);
}

// FlushErrors sorts errors seen so far by line number, prints them to stdout,
// and empties the errors array.
public static void FlushErrors() {
    if (Ctxt != null && Ctxt.Bso != null) {
        Ctxt.Bso.Flush();
    }
    if (len(errorMsgs) == 0) {
        return ;
    }
    sort.Stable(byPos(errorMsgs));
    foreach (var (i, err) in errorMsgs) {
        if (i == 0 || err.msg != errorMsgs[i - 1].msg) {
            fmt.Printf("%s", err.msg);
        }
    }    errorMsgs = errorMsgs[..(int)0];

}

// lasterror keeps track of the most recently issued error,
// to avoid printing multiple error messages on the same line.
private static var lasterror = default;

// sameline reports whether two positions a, b are on the same line.
private static bool sameline(src.XPos a, src.XPos b) {
    var p = Ctxt.PosTable.Pos(a);
    var q = Ctxt.PosTable.Pos(b);
    return p.Base() == q.Base() && p.Line() == q.Line();
}

// Errorf reports a formatted error at the current line.
public static void Errorf(@string format, params object[] args) {
    args = args.Clone();

    ErrorfAt(Pos, format, args);
}

// ErrorfAt reports a formatted error message at pos.
public static void ErrorfAt(src.XPos pos, @string format, params object[] args) {
    args = args.Clone();

    var msg = fmt.Sprintf(format, args);

    if (strings.HasPrefix(msg, "syntax error")) {
        numSyntaxErrors++; 
        // only one syntax error per line, no matter what error
        if (sameline(lasterror.syntax, pos)) {
            return ;
        }
        lasterror.syntax = pos;

    }
    else
 { 
        // only one of multiple equal non-syntax errors per line
        // (FlushErrors shows only one of them, so we filter them
        // here as best as we can (they may not appear in order)
        // so that we don't count them here and exit early, and
        // then have nothing to show for.)
        if (sameline(lasterror.other, pos) && lasterror.msg == msg) {
            return ;
        }
        lasterror.other = pos;
        lasterror.msg = msg;

    }
    addErrorMsg(pos, "%s", msg);
    numErrors++;

    hcrash();
    if (numErrors >= 10 && Flag.LowerE == 0) {
        FlushErrors();
        fmt.Printf("%v: too many errors\n", FmtPos(pos));
        ErrorExit();
    }
}

// ErrorfVers reports that a language feature (format, args) requires a later version of Go.
public static void ErrorfVers(@string lang, @string format, params object[] args) {
    args = args.Clone();

    Errorf("%s requires %s or later (-lang was set to %s; check go.mod)", fmt.Sprintf(format, args), lang, Flag.Lang);
}

// UpdateErrorDot is a clumsy hack that rewrites the last error,
// if it was "LINE: undefined: NAME", to be "LINE: undefined: NAME in EXPR".
// It is used to give better error messages for dot (selector) expressions.
public static void UpdateErrorDot(@string line, @string name, @string expr) {
    if (len(errorMsgs) == 0) {
        return ;
    }
    var e = _addr_errorMsgs[len(errorMsgs) - 1];
    if (strings.HasPrefix(e.msg, line) && e.msg == fmt.Sprintf("%v: undefined: %v\n", line, name)) {
        e.msg = fmt.Sprintf("%v: undefined: %v in %v\n", line, name, expr);
    }
}

// Warnf reports a formatted warning at the current line.
// In general the Go compiler does NOT generate warnings,
// so this should be used only when the user has opted in
// to additional output by setting a particular flag.
public static void Warn(@string format, params object[] args) {
    args = args.Clone();

    WarnfAt(Pos, format, args);
}

// WarnfAt reports a formatted warning at pos.
// In general the Go compiler does NOT generate warnings,
// so this should be used only when the user has opted in
// to additional output by setting a particular flag.
public static void WarnfAt(src.XPos pos, @string format, params object[] args) {
    args = args.Clone();

    addErrorMsg(pos, format, args);
    if (Flag.LowerM != 0) {
        FlushErrors();
    }
}

// Fatalf reports a fatal error - an internal problem - at the current line and exits.
// If other errors have already been printed, then Fatalf just quietly exits.
// (The internal problem may have been caused by incomplete information
// after the already-reported errors, so best to let users fix those and
// try again without being bothered about a spurious internal error.)
//
// But if no errors have been printed, or if -d panic has been specified,
// Fatalf prints the error as an "internal compiler error". In a released build,
// it prints an error asking to file a bug report. In development builds, it
// prints a stack trace.
//
// If -h has been specified, Fatalf panics to force the usual runtime info dump.
public static void Fatalf(@string format, params object[] args) {
    args = args.Clone();

    FatalfAt(Pos, format, args);
}

// FatalfAt reports a fatal error - an internal problem - at pos and exits.
// If other errors have already been printed, then FatalfAt just quietly exits.
// (The internal problem may have been caused by incomplete information
// after the already-reported errors, so best to let users fix those and
// try again without being bothered about a spurious internal error.)
//
// But if no errors have been printed, or if -d panic has been specified,
// FatalfAt prints the error as an "internal compiler error". In a released build,
// it prints an error asking to file a bug report. In development builds, it
// prints a stack trace.
//
// If -h has been specified, FatalfAt panics to force the usual runtime info dump.
public static void FatalfAt(src.XPos pos, @string format, params object[] args) {
    args = args.Clone();

    FlushErrors();

    if (Debug.Panic != 0 || numErrors == 0) {
        fmt.Printf("%v: internal compiler error: ", FmtPos(pos));
        fmt.Printf(format, args);
        fmt.Printf("\n"); 

        // If this is a released compiler version, ask for a bug report.
        if (strings.HasPrefix(buildcfg.Version, "go")) {
            fmt.Printf("\n");
            fmt.Printf("Please file a bug report including a short program that triggers the error.\n");
            fmt.Printf("https://golang.org/issue/new\n");
        }
        else
 { 
            // Not a release; dump a stack trace, too.
            fmt.Println();
            os.Stdout.Write(debug.Stack());
            fmt.Println();

        }
    }
    hcrash();
    ErrorExit();

}

// hcrash crashes the compiler when -h is set, to find out where a message is generated.
private static void hcrash() => func((_, panic, _) => {
    if (Flag.LowerH != 0) {
        FlushErrors();
        if (Flag.LowerO != "") {
            os.Remove(Flag.LowerO);
        }
        panic("-h");

    }
});

// ErrorExit handles an error-status exit.
// It flushes any pending errors, removes the output file, and exits.
public static void ErrorExit() {
    FlushErrors();
    if (Flag.LowerO != "") {
        os.Remove(Flag.LowerO);
    }
    os.Exit(2);

}

// ExitIfErrors calls ErrorExit if any errors have been reported.
public static void ExitIfErrors() {
    if (Errors() > 0) {
        ErrorExit();
    }
}

public static src.XPos AutogeneratedPos = default;

} // end @base_package
