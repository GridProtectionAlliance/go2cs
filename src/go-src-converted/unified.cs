// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// package diff -- go2cs converted at 2020 October 08 04:54:41 UTC
// import "golang.org/x/tools/internal/lsp/diff" ==> using diff = go.golang.org.x.tools.@internal.lsp.diff_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\lsp\diff\unified.go
using fmt = go.fmt_package;
using strings = go.strings_package;
using static go.builtin;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace @internal {
namespace lsp
{
    public static partial class diff_package
    {
        // Unified represents a set of edits as a unified diff.
        public partial struct Unified
        {
            public @string From; // To is the name of the modified file.
            public @string To; // Hunks is the set of edit hunks needed to transform the file content.
            public slice<ptr<Hunk>> Hunks;
        }

        // Hunk represents a contiguous set of line edits to apply.
        public partial struct Hunk
        {
            public long FromLine; // The line in the original source where the hunk finishes.
            public long ToLine; // The set of line based edits to apply.
            public slice<Line> Lines;
        }

        // Line represents a single line operation to apply as part of a Hunk.
        public partial struct Line
        {
            public OpKind Kind; // Content is the content of this line.
// For deletion it is the line being removed, for all others it is the line
// to put in the output.
            public @string Content;
        }

        // OpKind is used to denote the type of operation a line represents.
        public partial struct OpKind // : long
        {
        }

 
        // Delete is the operation kind for a line that is present in the input
        // but not in the output.
        public static readonly OpKind Delete = (OpKind)iota; 
        // Insert is the operation kind for a line that is new in the output.
        public static readonly var Insert = (var)0; 
        // Equal is the operation kind for a line that is the same in the input and
        // output, often used to provide context around edited lines.
        public static readonly var Equal = (var)1;


        // String returns a human readable representation of an OpKind. It is not
        // intended for machine processing.
        public static @string String(this OpKind k) => func((_, panic, __) =>
        {

            if (k == Delete) 
                return "delete";
            else if (k == Insert) 
                return "insert";
            else if (k == Equal) 
                return "equal";
            else 
                panic("unknown operation kind");
            
        });

        private static readonly long edge = (long)3L;
        private static readonly var gap = (var)edge * 2L;


        // ToUnified takes a file contents and a sequence of edits, and calculates
        // a unified diff that represents those edits.
        public static Unified ToUnified(@string from, @string to, @string content, slice<TextEdit> edits)
        {
            Unified u = new Unified(From:from,To:to,);
            if (len(edits) == 0L)
            {
                return u;
            }

            var (c, edits, partial) = prepareEdits(content, edits);
            if (partial)
            {
                edits = lineEdits(content, c, edits);
            }

            var lines = splitLines(content);
            ptr<Hunk> h;
            long last = 0L;
            long toLine = 0L;
            foreach (var (_, edit) in edits)
            {
                var start = edit.Span.Start().Line() - 1L;
                var end = edit.Span.End().Line() - 1L;

                if (h != null && start == last)                 else if (h != null && start <= last + gap) 
                    //within range of previous lines, add the joiners
                    addEqualLines(h, lines, last, start);
                else 
                    //need to start a new hunk
                    if (h != null)
                    { 
                        // add the edge to the previous hunk
                        addEqualLines(h, lines, last, last + edge);
                        u.Hunks = append(u.Hunks, h);

                    }

                    toLine += start - last;
                    h = addr(new Hunk(FromLine:start+1,ToLine:toLine+1,)); 
                    // add the edge to the new hunk
                    var delta = addEqualLines(h, lines, start - edge, start);
                    h.FromLine -= delta;
                    h.ToLine -= delta;
                                last = start;
                for (var i = start; i < end; i++)
                {
                    h.Lines = append(h.Lines, new Line(Kind:Delete,Content:lines[i]));
                    last++;
                }

                if (edit.NewText != "")
                {
                    foreach (var (_, line) in splitLines(edit.NewText))
                    {
                        h.Lines = append(h.Lines, new Line(Kind:Insert,Content:line));
                        toLine++;
                    }

                }

            }
            if (h != null)
            { 
                // add the edge to the final hunk
                addEqualLines(h, lines, last, last + edge);
                u.Hunks = append(u.Hunks, h);

            }

            return u;

        }

        private static slice<@string> splitLines(@string text)
        {
            var lines = strings.SplitAfter(text, "\n");
            if (lines[len(lines) - 1L] == "")
            {
                lines = lines[..len(lines) - 1L];
            }

            return lines;

        }

        private static long addEqualLines(ptr<Hunk> _addr_h, slice<@string> lines, long start, long end)
        {
            ref Hunk h = ref _addr_h.val;

            long delta = 0L;
            for (var i = start; i < end; i++)
            {
                if (i < 0L)
                {
                    continue;
                }

                if (i >= len(lines))
                {
                    return delta;
                }

                h.Lines = append(h.Lines, new Line(Kind:Equal,Content:lines[i]));
                delta++;

            }

            return delta;

        }

        // Format converts a unified diff to the standard textual form for that diff.
        // The output of this function can be passed to tools like patch.
        public static void Format(this Unified u, fmt.State f, int r)
        {
            if (len(u.Hunks) == 0L)
            {
                return ;
            }

            fmt.Fprintf(f, "--- %s\n", u.From);
            fmt.Fprintf(f, "+++ %s\n", u.To);
            foreach (var (_, hunk) in u.Hunks)
            {
                long fromCount = 0L;
                long toCount = 0L;
                {
                    var l__prev2 = l;

                    foreach (var (_, __l) in hunk.Lines)
                    {
                        l = __l;

                        if (l.Kind == Delete) 
                            fromCount++;
                        else if (l.Kind == Insert) 
                            toCount++;
                        else 
                            fromCount++;
                            toCount++;
                        
                    }

                    l = l__prev2;
                }

                fmt.Fprint(f, "@@");
                if (fromCount > 1L)
                {
                    fmt.Fprintf(f, " -%d,%d", hunk.FromLine, fromCount);
                }
                else
                {
                    fmt.Fprintf(f, " -%d", hunk.FromLine);
                }

                if (toCount > 1L)
                {
                    fmt.Fprintf(f, " +%d,%d", hunk.ToLine, toCount);
                }
                else
                {
                    fmt.Fprintf(f, " +%d", hunk.ToLine);
                }

                fmt.Fprint(f, " @@\n");
                {
                    var l__prev2 = l;

                    foreach (var (_, __l) in hunk.Lines)
                    {
                        l = __l;

                        if (l.Kind == Delete) 
                            fmt.Fprintf(f, "-%s", l.Content);
                        else if (l.Kind == Insert) 
                            fmt.Fprintf(f, "+%s", l.Content);
                        else 
                            fmt.Fprintf(f, " %s", l.Content);
                                                if (!strings.HasSuffix(l.Content, "\n"))
                        {
                            fmt.Fprintf(f, "\n\\ No newline at end of file\n");
                        }

                    }

                    l = l__prev2;
                }
            }

        }
    }
}}}}}}
