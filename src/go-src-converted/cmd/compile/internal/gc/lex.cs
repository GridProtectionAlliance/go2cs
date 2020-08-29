// Copyright 2009 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package gc -- go2cs converted at 2020 August 29 09:27:19 UTC
// import "cmd/compile/internal/gc" ==> using gc = go.cmd.compile.@internal.gc_package
// Original source: C:\Go\src\cmd\compile\internal\gc\lex.go
using syntax = go.cmd.compile.@internal.syntax_package;
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using fmt = go.fmt_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace compile {
namespace @internal
{
    public static partial class gc_package
    {
        // lineno is the source position at the start of the most recently lexed token.
        // TODO(gri) rename and eventually remove
        private static src.XPos lineno = default;

        private static src.XPos makePos(ref src.PosBase @base, ulong line, ulong col)
        {
            return Ctxt.PosTable.XPos(src.MakePos(base, line, col));
        }

        private static bool isSpace(int c)
        {
            return c == ' ' || c == '\t' || c == '\n' || c == '\r';
        }

        private static bool isQuoted(@string s)
        {
            return len(s) >= 2L && s[0L] == '"' && s[len(s) - 1L] == '"';
        }

        private static @string plan9quote(@string s)
        {
            if (s == "")
            {
                return "''";
            }
            foreach (var (_, c) in s)
            {
                if (c <= ' ' || c == '\'')
                {
                    return "'" + strings.Replace(s, "'", "''", -1L) + "'";
                }
            }
            return s;
        }

 
        // Func pragmas.
        public static readonly syntax.Pragma Nointerface = 1L << (int)(iota);
        public static readonly var Noescape = 0; // func parameters don't escape
        public static readonly var Norace = 1; // func must not have race detector annotations
        public static readonly var Nosplit = 2; // func should not execute on separate stack
        public static readonly var Noinline = 3; // func should not be inlined
        public static readonly var CgoUnsafeArgs = 4; // treat a pointer to one arg as a pointer to them all
        public static readonly var UintptrEscapes = 5; // pointers converted to uintptr escape

        // Runtime-only func pragmas.
        // See ../../../../runtime/README.md for detailed descriptions.
        public static readonly var Systemstack = 6; // func must run on system stack
        public static readonly var Nowritebarrier = 7; // emit compiler error instead of write barrier
        public static readonly var Nowritebarrierrec = 8; // error on write barrier in this or recursive callees
        public static readonly var Yeswritebarrierrec = 9; // cancels Nowritebarrierrec in this function and callees

        // Runtime-only type pragmas
        public static readonly var NotInHeap = 10; // values of this type must not be heap allocated

        private static syntax.Pragma pragmaValue(@string verb)
        {
            switch (verb)
            {
                case "go:nointerface": 
                    if (objabi.Fieldtrack_enabled != 0L)
                    {
                        return Nointerface;
                    }
                    break;
                case "go:noescape": 
                    return Noescape;
                    break;
                case "go:norace": 
                    return Norace;
                    break;
                case "go:nosplit": 
                    return Nosplit;
                    break;
                case "go:noinline": 
                    return Noinline;
                    break;
                case "go:systemstack": 
                    return Systemstack;
                    break;
                case "go:nowritebarrier": 
                    return Nowritebarrier;
                    break;
                case "go:nowritebarrierrec": 
                    return Nowritebarrierrec | Nowritebarrier; // implies Nowritebarrier
                    break;
                case "go:yeswritebarrierrec": 
                    return Yeswritebarrierrec;
                    break;
                case "go:cgo_unsafe_args": 
                    return CgoUnsafeArgs;
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
                    return UintptrEscapes;
                    break;
                case "go:notinheap": 
                    return NotInHeap;
                    break;
            }
            return 0L;
        }

        // pragcgo is called concurrently if files are parsed concurrently.
        private static @string pragcgo(this ref noder p, src.Pos pos, @string text)
        {
            var f = pragmaFields(text);

            var verb = f[0L][3L..]; // skip "go:"
            switch (verb)
            {
                case "cgo_export_static": 

                case "cgo_export_dynamic": 

                    if (len(f) == 2L && !isQuoted(f[1L])) 
                        var local = plan9quote(f[1L]);
                        return fmt.Sprintln(verb, local);
                    else if (len(f) == 3L && !isQuoted(f[1L]) && !isQuoted(f[2L])) 
                        local = plan9quote(f[1L]);
                        var remote = plan9quote(f[2L]);
                        return fmt.Sprintln(verb, local, remote);
                    else 
                        p.error(new syntax.Error(Pos:pos,Msg:fmt.Sprintf(`usage: //go:%s local [remote]`,verb)));
                    break;
                case "cgo_import_dynamic": 

                    if (len(f) == 2L && !isQuoted(f[1L])) 
                        local = plan9quote(f[1L]);
                        return fmt.Sprintln(verb, local);
                    else if (len(f) == 3L && !isQuoted(f[1L]) && !isQuoted(f[2L])) 
                        local = plan9quote(f[1L]);
                        remote = plan9quote(f[2L]);
                        return fmt.Sprintln(verb, local, remote);
                    else if (len(f) == 4L && !isQuoted(f[1L]) && !isQuoted(f[2L]) && isQuoted(f[3L])) 
                        local = plan9quote(f[1L]);
                        remote = plan9quote(f[2L]);
                        var library = plan9quote(strings.Trim(f[3L], "\""));
                        return fmt.Sprintln(verb, local, remote, library);
                    else 
                        p.error(new syntax.Error(Pos:pos,Msg:`usage: //go:cgo_import_dynamic local [remote ["library"]]`));
                    break;
                case "cgo_import_static": 

                    if (len(f) == 2L && !isQuoted(f[1L])) 
                        local = plan9quote(f[1L]);
                        return fmt.Sprintln(verb, local);
                    else 
                        p.error(new syntax.Error(Pos:pos,Msg:`usage: //go:cgo_import_static local`));
                    break;
                case "cgo_dynamic_linker": 

                    if (len(f) == 2L && isQuoted(f[1L])) 
                        var path = plan9quote(strings.Trim(f[1L], "\""));
                        return fmt.Sprintln(verb, path);
                    else 
                        p.error(new syntax.Error(Pos:pos,Msg:`usage: //go:cgo_dynamic_linker "path"`));
                    break;
                case "cgo_ldflag": 

                    if (len(f) == 2L && isQuoted(f[1L])) 
                        var arg = plan9quote(strings.Trim(f[1L], "\""));
                        return fmt.Sprintln(verb, arg);
                    else 
                        p.error(new syntax.Error(Pos:pos,Msg:`usage: //go:cgo_ldflag "arg"`));
                    break;
            }
            return "";
        }

        // pragmaFields is similar to strings.FieldsFunc(s, isSpace)
        // but does not split when inside double quoted regions and always
        // splits before the start and after the end of a double quoted region.
        // pragmaFields does not recognize escaped quotes. If a quote in s is not
        // closed the part after the opening quote will not be returned as a field.
        private static slice<@string> pragmaFields(@string s)
        {
            slice<@string> a = default;
            var inQuote = false;
            long fieldStart = -1L; // Set to -1 when looking for start of field.
            foreach (var (i, c) in s)
            {

                if (c == '"') 
                    if (inQuote)
                    {
                        inQuote = false;
                        a = append(a, s[fieldStart..i + 1L]);
                        fieldStart = -1L;
                    }
                    else
                    {
                        inQuote = true;
                        if (fieldStart >= 0L)
                        {
                            a = append(a, s[fieldStart..i]);
                        }
                        fieldStart = i;
                    }
                else if (!inQuote && isSpace(c)) 
                    if (fieldStart >= 0L)
                    {
                        a = append(a, s[fieldStart..i]);
                        fieldStart = -1L;
                    }
                else 
                    if (fieldStart == -1L)
                    {
                        fieldStart = i;
                    }
                            }
            if (!inQuote && fieldStart >= 0L)
            { // Last field might end at the end of the string.
                a = append(a, s[fieldStart..]);
            }
            return a;
        }
    }
}}}}
