// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package noder -- go2cs converted at 2022 March 06 23:13:57 UTC
// import "cmd/compile/internal/noder" ==> using noder = go.cmd.compile.@internal.noder_package
// Original source: C:\Program Files\Go\src\cmd\compile\internal\noder\lex.go
using fmt = go.fmt_package;
using buildcfg = go.@internal.buildcfg_package;
using strings = go.strings_package;

using ir = go.cmd.compile.@internal.ir_package;
using syntax = go.cmd.compile.@internal.syntax_package;

namespace go.cmd.compile.@internal;

public static partial class noder_package {

private static bool isSpace(int c) {
    return c == ' ' || c == '\t' || c == '\n' || c == '\r';
}

private static bool isQuoted(@string s) {
    return len(s) >= 2 && s[0] == '"' && s[len(s) - 1] == '"';
}

private static readonly var funcPragmas = ir.Nointerface | ir.Noescape | ir.Norace | ir.Nosplit | ir.Noinline | ir.NoCheckPtr | ir.RegisterParams | ir.CgoUnsafeArgs | ir.UintptrEscapes | ir.Systemstack | ir.Nowritebarrier | ir.Nowritebarrierrec | ir.Yeswritebarrierrec;

private static readonly var typePragmas = ir.NotInHeap;


private static ir.PragmaFlag pragmaFlag(@string verb) {
    switch (verb) {
        case "go:build": 
            return ir.GoBuildPragma;
            break;
        case "go:nointerface": 
            if (buildcfg.Experiment.FieldTrack) {
                return ir.Nointerface;
            }
            break;
        case "go:noescape": 
            return ir.Noescape;
            break;
        case "go:norace": 
            return ir.Norace;
            break;
        case "go:nosplit": 
            return ir.Nosplit | ir.NoCheckPtr; // implies NoCheckPtr (see #34972)
            break;
        case "go:noinline": 
            return ir.Noinline;
            break;
        case "go:nocheckptr": 
            return ir.NoCheckPtr;
            break;
        case "go:systemstack": 
            return ir.Systemstack;
            break;
        case "go:nowritebarrier": 
            return ir.Nowritebarrier;
            break;
        case "go:nowritebarrierrec": 
            return ir.Nowritebarrierrec | ir.Nowritebarrier; // implies Nowritebarrier
            break;
        case "go:yeswritebarrierrec": 
            return ir.Yeswritebarrierrec;
            break;
        case "go:cgo_unsafe_args": 
            return ir.CgoUnsafeArgs | ir.NoCheckPtr; // implies NoCheckPtr (see #34968)
            break;
        case "go:uintptrescapes": 
            // For the next function declared in the file
            // any uintptr arguments may be pointer values
            // converted to uintptr. This directive
            // ensures that the referenced allocated
            // object, if any, is retained and not moved
            // until the call completes, even though from
            // the types alone it would appear that the
            // object is no longer needed during the
            // call. The conversion to uintptr must appear
            // in the argument list.
            // Used in syscall/dll_windows.go.
            return ir.UintptrEscapes;
            break;
        case "go:registerparams": // TODO(register args) remove after register abi is working
            return ir.RegisterParams;
            break;
        case "go:notinheap": 
            return ir.NotInHeap;
            break;
    }
    return 0;

}

// pragcgo is called concurrently if files are parsed concurrently.
private static void pragcgo(this ptr<noder> _addr_p, syntax.Pos pos, @string text) {
    ref noder p = ref _addr_p.val;

    var f = pragmaFields(text);

    var verb = strings.TrimPrefix(f[0], "go:");
    f[0] = verb;

    switch (verb) {
        case "cgo_export_static": 

        case "cgo_export_dynamic": 

            if (len(f) == 2 && !isQuoted(f[1]))         else if (len(f) == 3 && !isQuoted(f[1]) && !isQuoted(f[2]))         else 
                p.error(new syntax.Error(Pos:pos,Msg:fmt.Sprintf(`usage: //go:%s local [remote]`,verb)));
                return ;

            break;
        case "cgo_import_dynamic": 

            if (len(f) == 2 && !isQuoted(f[1]))         else if (len(f) == 3 && !isQuoted(f[1]) && !isQuoted(f[2]))         else if (len(f) == 4 && !isQuoted(f[1]) && !isQuoted(f[2]) && isQuoted(f[3])) 
                f[3] = strings.Trim(f[3], "\"");
                if (buildcfg.GOOS == "aix" && f[3] != "") { 
                    // On Aix, library pattern must be "lib.a/object.o"
                    // or "lib.a/libname.so.X"
                    var n = strings.Split(f[3], "/");
                    if (len(n) != 2 || !strings.HasSuffix(n[0], ".a") || (!strings.HasSuffix(n[1], ".o") && !strings.Contains(n[1], ".so."))) {
                        p.error(new syntax.Error(Pos:pos,Msg:`usage: //go:cgo_import_dynamic local [remote ["lib.a/object.o"]]`));
                        return ;
                    }

                }

            else 
                p.error(new syntax.Error(Pos:pos,Msg:`usage: //go:cgo_import_dynamic local [remote ["library"]]`));
                return ;

            break;
        case "cgo_import_static": 

            if (len(f) == 2 && !isQuoted(f[1]))         else 
                p.error(new syntax.Error(Pos:pos,Msg:`usage: //go:cgo_import_static local`));
                return ;

            break;
        case "cgo_dynamic_linker": 

            if (len(f) == 2 && isQuoted(f[1])) 
                f[1] = strings.Trim(f[1], "\"");
            else 
                p.error(new syntax.Error(Pos:pos,Msg:`usage: //go:cgo_dynamic_linker "path"`));
                return ;

            break;
        case "cgo_ldflag": 

            if (len(f) == 2 && isQuoted(f[1])) 
                f[1] = strings.Trim(f[1], "\"");
            else 
                p.error(new syntax.Error(Pos:pos,Msg:`usage: //go:cgo_ldflag "arg"`));
                return ;

            break;
        default: 
            return ;
            break;
    }
    p.pragcgobuf = append(p.pragcgobuf, f);

}

// pragmaFields is similar to strings.FieldsFunc(s, isSpace)
// but does not split when inside double quoted regions and always
// splits before the start and after the end of a double quoted region.
// pragmaFields does not recognize escaped quotes. If a quote in s is not
// closed the part after the opening quote will not be returned as a field.
private static slice<@string> pragmaFields(@string s) {
    slice<@string> a = default;
    var inQuote = false;
    nint fieldStart = -1; // Set to -1 when looking for start of field.
    foreach (var (i, c) in s) {

        if (c == '"') 
            if (inQuote) {
                inQuote = false;
                a = append(a, s[(int)fieldStart..(int)i + 1]);
                fieldStart = -1;
            }
            else
 {
                inQuote = true;
                if (fieldStart >= 0) {
                    a = append(a, s[(int)fieldStart..(int)i]);
                }
                fieldStart = i;
            }

        else if (!inQuote && isSpace(c)) 
            if (fieldStart >= 0) {
                a = append(a, s[(int)fieldStart..(int)i]);
                fieldStart = -1;
            }
        else 
            if (fieldStart == -1) {
                fieldStart = i;
            }
        
    }    if (!inQuote && fieldStart >= 0) { // Last field might end at the end of the string.
        a = append(a, s[(int)fieldStart..]);

    }
    return a;

}

} // end noder_package
