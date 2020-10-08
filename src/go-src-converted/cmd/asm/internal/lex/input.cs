// Copyright 2015 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package lex -- go2cs converted at 2020 October 08 04:04:38 UTC
// import "cmd/asm/internal/lex" ==> using lex = go.cmd.asm.@internal.lex_package
// Original source: C:\Go\src\cmd\asm\internal\lex\input.go
using fmt = go.fmt_package;
using os = go.os_package;
using filepath = go.path.filepath_package;
using strconv = go.strconv_package;
using strings = go.strings_package;
using scanner = go.text.scanner_package;

using flags = go.cmd.asm.@internal.flags_package;
using objabi = go.cmd.@internal.objabi_package;
using src = go.cmd.@internal.src_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace asm {
namespace @internal
{
    public static partial class lex_package
    {
        // Input is the main input: a stack of readers and some macro definitions.
        // It also handles #include processing (by pushing onto the input stack)
        // and parses and instantiates macro definitions.
        public partial struct Input
        {
            public ref Stack Stack => ref Stack_val;
            public slice<@string> includes;
            public bool beginningOfLine;
            public slice<bool> ifdefStack;
            public map<@string, ptr<Macro>> macros;
            public @string text; // Text of last token returned by Next.
            public bool peek;
            public ScanToken peekToken;
            public @string peekText;
        }

        // NewInput returns an Input from the given path.
        public static ptr<Input> NewInput(@string name)
        {
            return addr(new Input(includes:append([]string{filepath.Dir(name)},flags.I...),beginningOfLine:true,macros:predefine(flags.D),));
        }

        // predefine installs the macros set by the -D flag on the command line.
        private static map<@string, ptr<Macro>> predefine(flags.MultiFlag defines)
        {
            var macros = make_map<@string, ptr<Macro>>();
            foreach (var (_, name) in defines)
            {
                @string value = "1";
                var i = strings.IndexRune(name, '=');
                if (i > 0L)
                {
                    name = name[..i];
                    value = name[i + 1L..];

                }

                var tokens = Tokenize(name);
                if (len(tokens) != 1L || tokens[0L].ScanToken != scanner.Ident)
                {
                    fmt.Fprintf(os.Stderr, "asm: parsing -D: %q is not a valid identifier name\n", tokens[0L]);
                    flags.Usage();
                }

                macros[name] = addr(new Macro(name:name,args:nil,tokens:Tokenize(value),));

            }
            return macros;

        }

        private static bool panicOnError = default; // For testing.

        private static void Error(this ptr<Input> _addr_@in, params object[] args) => func((_, panic, __) =>
        {
            args = args.Clone();
            ref Input @in = ref _addr_@in.val;

            if (panicOnError)
            {
                panic(fmt.Errorf("%s:%d: %s", @in.File(), @in.Line(), fmt.Sprintln(args)));
            }

            fmt.Fprintf(os.Stderr, "%s:%d: %s", @in.File(), @in.Line(), fmt.Sprintln(args));
            os.Exit(1L);

        });

        // expectText is like Error but adds "got XXX" where XXX is a quoted representation of the most recent token.
        private static void expectText(this ptr<Input> _addr_@in, params object[] args)
        {
            args = args.Clone();
            ref Input @in = ref _addr_@in.val;

            @in.Error(append(args, "; got", strconv.Quote(@in.Stack.Text())));
        }

        // enabled reports whether the input is enabled by an ifdef, or is at the top level.
        private static bool enabled(this ptr<Input> _addr_@in)
        {
            ref Input @in = ref _addr_@in.val;

            return len(@in.ifdefStack) == 0L || @in.ifdefStack[len(@in.ifdefStack) - 1L];
        }

        private static void expectNewline(this ptr<Input> _addr_@in, @string directive)
        {
            ref Input @in = ref _addr_@in.val;

            var tok = @in.Stack.Next();
            if (tok != '\n')
            {
                @in.expectText("expected newline after", directive);
            }

        }

        private static ScanToken Next(this ptr<Input> _addr_@in)
        {
            ref Input @in = ref _addr_@in.val;

            if (@in.peek)
            {
                @in.peek = false;
                var tok = @in.peekToken;
                @in.text = @in.peekText;
                return tok;
            } 
            // If we cannot generate a token after 100 macro invocations, we're in trouble.
            // The usual case is caught by Push, below, but be safe.
            {
                long nesting = 0L;

                while (nesting < 100L)
                {
                    tok = @in.Stack.Next();

                    if (tok == '#')
                    {
                        if (!@in.beginningOfLine)
                        {
                            @in.Error("'#' must be first item on line");
                        }

                        @in.beginningOfLine = @in.hash();
                        goto __switch_break0;
                    }
                    if (tok == scanner.Ident) 
                    {
                        // Is it a macro name?
                        var name = @in.Stack.Text();
                        var macro = @in.macros[name];
                        if (macro != null)
                        {
                            nesting++;
                            @in.invokeMacro(macro);
                            continue;
                        }

                    }
                    // default: 
                        if (tok == scanner.EOF && len(@in.ifdefStack) > 0L)
                        { 
                            // We're skipping text but have run out of input with no #endif.
                            @in.Error("unclosed #ifdef or #ifndef");

                        }

                        @in.beginningOfLine = tok == '\n';
                        if (@in.enabled())
                        {
                            @in.text = @in.Stack.Text();
                            return tok;
                        }


                    __switch_break0:;

                }

            }
            @in.Error("recursive macro invocation");
            return 0L;

        }

        private static @string Text(this ptr<Input> _addr_@in)
        {
            ref Input @in = ref _addr_@in.val;

            return @in.text;
        }

        // hash processes a # preprocessor directive. It reports whether it completes.
        private static bool hash(this ptr<Input> _addr_@in)
        {
            ref Input @in = ref _addr_@in.val;
 
            // We have a '#'; it must be followed by a known word (define, include, etc.).
            var tok = @in.Stack.Next();
            if (tok != scanner.Ident)
            {
                @in.expectText("expected identifier after '#'");
            }

            if (!@in.enabled())
            { 
                // Can only start including again if we are at #else or #endif but also
                // need to keep track of nested #if[n]defs.
                // We let #line through because it might affect errors.
                switch (@in.Stack.Text())
                {
                    case "else": 

                    case "endif": 

                    case "ifdef": 

                    case "ifndef": 

                    case "line": 
                        break;
                    default: 
                        return false;
                        break;
                }

            }

            switch (@in.Stack.Text())
            {
                case "define": 
                    @in.define();
                    break;
                case "else": 
                    @in.else_();
                    break;
                case "endif": 
                    @in.endif();
                    break;
                case "ifdef": 
                    @in.ifdef(true);
                    break;
                case "ifndef": 
                    @in.ifdef(false);
                    break;
                case "include": 
                    @in.include();
                    break;
                case "line": 
                    @in.line();
                    break;
                case "undef": 
                    @in.undef();
                    break;
                default: 
                    @in.Error("unexpected token after '#':", @in.Stack.Text());
                    break;
            }
            return true;

        }

        // macroName returns the name for the macro being referenced.
        private static @string macroName(this ptr<Input> _addr_@in)
        {
            ref Input @in = ref _addr_@in.val;
 
            // We use the Stack's input method; no macro processing at this stage.
            var tok = @in.Stack.Next();
            if (tok != scanner.Ident)
            {
                @in.expectText("expected identifier after # directive");
            } 
            // Name is alphanumeric by definition.
            return @in.Stack.Text();

        }

        // #define processing.
        private static void define(this ptr<Input> _addr_@in)
        {
            ref Input @in = ref _addr_@in.val;

            var name = @in.macroName();
            var (args, tokens) = @in.macroDefinition(name);
            @in.defineMacro(name, args, tokens);
        }

        // defineMacro stores the macro definition in the Input.
        private static void defineMacro(this ptr<Input> _addr_@in, @string name, slice<@string> args, slice<Token> tokens)
        {
            ref Input @in = ref _addr_@in.val;

            if (@in.macros[name] != null)
            {
                @in.Error("redefinition of macro:", name);
            }

            @in.macros[name] = addr(new Macro(name:name,args:args,tokens:tokens,));

        }

        // macroDefinition returns the list of formals and the tokens of the definition.
        // The argument list is nil for no parens on the definition; otherwise a list of
        // formal argument names.
        private static (slice<@string>, slice<Token>) macroDefinition(this ptr<Input> _addr_@in, @string name)
        {
            slice<@string> _p0 = default;
            slice<Token> _p0 = default;
            ref Input @in = ref _addr_@in.val;

            var prevCol = @in.Stack.Col();
            var tok = @in.Stack.Next();
            if (tok == '\n' || tok == scanner.EOF)
            {
                return (null, null); // No definition for macro
            }

            slice<@string> args = default; 
            // The C preprocessor treats
            //    #define A(x)
            // and
            //    #define A (x)
            // distinctly: the first is a macro with arguments, the second without.
            // Distinguish these cases using the column number, since we don't
            // see the space itself. Note that text/scanner reports the position at the
            // end of the token. It's where you are now, and you just read this token.
            if (tok == '(' && @in.Stack.Col() == prevCol + 1L)
            { 
                // Macro has arguments. Scan list of formals.
                var acceptArg = true;
                args = new slice<@string>(new @string[] {  }); // Zero length but not nil.
Loop:
                while (true)
                {
                    tok = @in.Stack.Next();

                    if (tok == ')') 
                        tok = @in.Stack.Next(); // First token of macro definition.
                        _breakLoop = true;
                        break;
                    else if (tok == ',') 
                        if (acceptArg)
                        {
                            @in.Error("bad syntax in definition for macro:", name);
                        }

                        acceptArg = true;
                    else if (tok == scanner.Ident) 
                        if (!acceptArg)
                        {
                            @in.Error("bad syntax in definition for macro:", name);
                        }

                        var arg = @in.Stack.Text();
                        {
                            var i = lookup(args, arg);

                            if (i >= 0L)
                            {
                                @in.Error("duplicate argument", arg, "in definition for macro:", name);
                            }

                        }

                        args = append(args, arg);
                        acceptArg = false;
                    else 
                        @in.Error("bad definition for macro:", name);
                    
                }

            }

            slice<Token> tokens = default; 
            // Scan to newline. Backslashes escape newlines.
            while (tok != '\n')
            {
                if (tok == scanner.EOF)
                {
                    @in.Error("missing newline in definition for macro:", name);
                }

                if (tok == '\\')
                {
                    tok = @in.Stack.Next();
                    if (tok != '\n' && tok != '\\')
                    {
                        @in.Error("can only escape \\ or \\n in definition for macro:", name);
                    }

                }

                tokens = append(tokens, Make(tok, @in.Stack.Text()));
                tok = @in.Stack.Next();

            }

            return (args, tokens);

        }

        private static long lookup(slice<@string> args, @string arg)
        {
            foreach (var (i, a) in args)
            {
                if (a == arg)
                {
                    return i;
                }

            }
            return -1L;

        }

        // invokeMacro pushes onto the input Stack a Slice that holds the macro definition with the actual
        // parameters substituted for the formals.
        // Invoking a macro does not touch the PC/line history.
        private static void invokeMacro(this ptr<Input> _addr_@in, ptr<Macro> _addr_macro)
        {
            ref Input @in = ref _addr_@in.val;
            ref Macro macro = ref _addr_macro.val;
 
            // If the macro has no arguments, just substitute the text.
            if (macro.args == null)
            {
                @in.Push(NewSlice(@in.Base(), @in.Line(), macro.tokens));
                return ;
            }

            var tok = @in.Stack.Next();
            if (tok != '(')
            { 
                // If the macro has arguments but is invoked without them, all we push is the macro name.
                // First, put back the token.
                @in.peekToken = tok;
                @in.peekText = @in.text;
                @in.peek = true;
                @in.Push(NewSlice(@in.Base(), @in.Line(), new slice<Token>(new Token[] { Make(macroName,macro.name) })));
                return ;

            }

            var actuals = @in.argsFor(macro);
            slice<Token> tokens = default;
            {
                var tok__prev1 = tok;

                foreach (var (_, __tok) in macro.tokens)
                {
                    tok = __tok;
                    if (tok.ScanToken != scanner.Ident)
                    {
                        tokens = append(tokens, tok);
                        continue;
                    }

                    var substitution = actuals[tok.text];
                    if (substitution == null)
                    {
                        tokens = append(tokens, tok);
                        continue;
                    }

                    tokens = append(tokens, substitution);

                }

                tok = tok__prev1;
            }

            @in.Push(NewSlice(@in.Base(), @in.Line(), tokens));

        }

        // argsFor returns a map from formal name to actual value for this argumented macro invocation.
        // The opening parenthesis has been absorbed.
        private static map<@string, slice<Token>> argsFor(this ptr<Input> _addr_@in, ptr<Macro> _addr_macro)
        {
            ref Input @in = ref _addr_@in.val;
            ref Macro macro = ref _addr_macro.val;

            slice<slice<Token>> args = default; 
            // One macro argument per iteration. Collect them all and check counts afterwards.
            for (long argNum = 0L; args; argNum++)
            {
                var (tokens, tok) = @in.collectArgument(macro);
                args = append(args, tokens);
                if (tok == ')')
                {
                    break;
                }

            } 
            // Zero-argument macros are tricky.
 
            // Zero-argument macros are tricky.
            if (len(macro.args) == 0L && len(args) == 1L && args[0L] == null)
            {
                args = null;
            }
            else if (len(args) != len(macro.args))
            {
                @in.Error("wrong arg count for macro", macro.name);
            }

            var argMap = make_map<@string, slice<Token>>();
            foreach (var (i, arg) in args)
            {
                argMap[macro.args[i]] = arg;
            }
            return argMap;

        }

        // collectArgument returns the actual tokens for a single argument of a macro.
        // It also returns the token that terminated the argument, which will always
        // be either ',' or ')'. The starting '(' has been scanned.
        private static (slice<Token>, ScanToken) collectArgument(this ptr<Input> _addr_@in, ptr<Macro> _addr_macro)
        {
            slice<Token> _p0 = default;
            ScanToken _p0 = default;
            ref Input @in = ref _addr_@in.val;
            ref Macro macro = ref _addr_macro.val;

            long nesting = 0L;
            slice<Token> tokens = default;
            while (true)
            {
                var tok = @in.Stack.Next();
                if (tok == scanner.EOF || tok == '\n')
                {
                    @in.Error("unterminated arg list invoking macro:", macro.name);
                }

                if (nesting == 0L && (tok == ')' || tok == ','))
                {
                    return (tokens, tok);
                }

                if (tok == '(')
                {
                    nesting++;
                }

                if (tok == ')')
                {
                    nesting--;
                }

                tokens = append(tokens, Make(tok, @in.Stack.Text()));

            }


        }

        // #ifdef and #ifndef processing.
        private static void ifdef(this ptr<Input> _addr_@in, bool truth)
        {
            ref Input @in = ref _addr_@in.val;

            var name = @in.macroName();
            @in.expectNewline("#if[n]def");
            if (!@in.enabled())
            {
                truth = false;
            }            {
                var (_, defined) = @in.macros[name];


                else if (!defined)
                {
                    truth = !truth;
                }

            }

            @in.ifdefStack = append(@in.ifdefStack, truth);

        }

        // #else processing
        private static void else_(this ptr<Input> _addr_@in)
        {
            ref Input @in = ref _addr_@in.val;

            @in.expectNewline("#else");
            if (len(@in.ifdefStack) == 0L)
            {
                @in.Error("unmatched #else");
            }

            if (len(@in.ifdefStack) == 1L || @in.ifdefStack[len(@in.ifdefStack) - 2L])
            {
                @in.ifdefStack[len(@in.ifdefStack) - 1L] = !@in.ifdefStack[len(@in.ifdefStack) - 1L];
            }

        }

        // #endif processing.
        private static void endif(this ptr<Input> _addr_@in)
        {
            ref Input @in = ref _addr_@in.val;

            @in.expectNewline("#endif");
            if (len(@in.ifdefStack) == 0L)
            {
                @in.Error("unmatched #endif");
            }

            @in.ifdefStack = @in.ifdefStack[..len(@in.ifdefStack) - 1L];

        }

        // #include processing.
        private static void include(this ptr<Input> _addr_@in)
        {
            ref Input @in = ref _addr_@in.val;
 
            // Find and parse string.
            var tok = @in.Stack.Next();
            if (tok != scanner.String)
            {
                @in.expectText("expected string after #include");
            }

            var (name, err) = strconv.Unquote(@in.Stack.Text());
            if (err != null)
            {
                @in.Error("unquoting include file name: ", err);
            }

            @in.expectNewline("#include"); 
            // Push tokenizer for file onto stack.
            var (fd, err) = os.Open(name);
            if (err != null)
            {
                foreach (var (_, dir) in @in.includes)
                {
                    fd, err = os.Open(filepath.Join(dir, name));
                    if (err == null)
                    {
                        break;
                    }

                }
                if (err != null)
                {
                    @in.Error("#include:", err);
                }

            }

            @in.Push(NewTokenizer(name, fd, fd));

        }

        // #line processing.
        private static void line(this ptr<Input> _addr_@in)
        {
            ref Input @in = ref _addr_@in.val;
 
            // Only need to handle Plan 9 format: #line 337 "filename"
            var tok = @in.Stack.Next();
            if (tok != scanner.Int)
            {
                @in.expectText("expected line number after #line");
            }

            var (line, err) = strconv.Atoi(@in.Stack.Text());
            if (err != null)
            {
                @in.Error("error parsing #line (cannot happen):", err);
            }

            tok = @in.Stack.Next();
            if (tok != scanner.String)
            {
                @in.expectText("expected file name in #line");
            }

            var (file, err) = strconv.Unquote(@in.Stack.Text());
            if (err != null)
            {
                @in.Error("unquoting #line file name: ", err);
            }

            tok = @in.Stack.Next();
            if (tok != '\n')
            {
                @in.Error("unexpected token at end of #line: ", tok);
            }

            var pos = src.MakePos(@in.Base(), uint(@in.Line()) + 1L, 1L); // +1 because #line nnn means line nnn starts on next line
            @in.Stack.SetBase(src.NewLinePragmaBase(pos, file, objabi.AbsFile(objabi.WorkingDir(), file, flags.TrimPath.val), uint(line), 1L));

        }

        // #undef processing
        private static void undef(this ptr<Input> _addr_@in)
        {
            ref Input @in = ref _addr_@in.val;

            var name = @in.macroName();
            if (@in.macros[name] == null)
            {
                @in.Error("#undef for undefined macro:", name);
            } 
            // Newline must be next.
            var tok = @in.Stack.Next();
            if (tok != '\n')
            {
                @in.Error("syntax error in #undef for macro:", name);
            }

            delete(@in.macros, name);

        }

        private static void Push(this ptr<Input> _addr_@in, TokenReader r)
        {
            ref Input @in = ref _addr_@in.val;

            if (len(@in.tr) > 100L)
            {
                @in.Error("input recursion");
            }

            @in.Stack.Push(r);

        }

        private static void Close(this ptr<Input> _addr_@in)
        {
            ref Input @in = ref _addr_@in.val;

        }
    }
}}}}
