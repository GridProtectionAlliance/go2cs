// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package diff supports a pluggable diff algorithm.
// package diff -- go2cs converted at 2020 October 09 06:01:25 UTC
// import "golang.org/x/tools/internal/lsp/diff" ==> using diff = go.golang.org.x.tools.@internal.lsp.diff_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\lsp\diff\diff.go
using sort = go.sort_package;
using strings = go.strings_package;

using span = go.golang.org.x.tools.@internal.span_package;
using static go.builtin;
using System;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace @internal {
namespace lsp
{
    public static partial class diff_package
    {
        // TextEdit represents a change to a section of a document.
        // The text within the specified span should be replaced by the supplied new text.
        public partial struct TextEdit
        {
            public span.Span Span;
            public @string NewText;
        }

        // ComputeEdits is the type for a function that produces a set of edits that
        // convert from the before content to the after content.
        public delegate  slice<TextEdit> ComputeEdits(span.URI,  @string,  @string);

        // SortTextEdits attempts to order all edits by their starting points.
        // The sort is stable so that edits with the same starting point will not
        // be reordered.
        public static void SortTextEdits(slice<TextEdit> d)
        { 
            // Use a stable sort to maintain the order of edits inserted at the same position.
            sort.SliceStable(d, (i, j) =>
            {
                return span.Compare(d[i].Span, d[j].Span) < 0L;
            });

        }

        // ApplyEdits applies the set of edits to the before and returns the resulting
        // content.
        // It may panic or produce garbage if the edits are not valid for the provided
        // before content.
        public static @string ApplyEdits(@string before, slice<TextEdit> edits)
        { 
            // Preconditions:
            //   - all of the edits apply to before
            //   - and all the spans for each TextEdit have the same URI
            if (len(edits) == 0L)
            {
                return before;
            }

            _, edits, _ = prepareEdits(before, edits);
            strings.Builder after = new strings.Builder();
            long last = 0L;
            foreach (var (_, edit) in edits)
            {
                var start = edit.Span.Start().Offset();
                if (start > last)
                {
                    after.WriteString(before[last..start]);
                    last = start;
                }

                after.WriteString(edit.NewText);
                last = edit.Span.End().Offset();

            }
            if (last < len(before))
            {
                after.WriteString(before[last..]);
            }

            return after.String();

        }

        // LineEdits takes a set of edits and expands and merges them as necessary
        // to ensure that there are only full line edits left when it is done.
        public static slice<TextEdit> LineEdits(@string before, slice<TextEdit> edits)
        {
            if (len(edits) == 0L)
            {
                return null;
            }

            var (c, edits, partial) = prepareEdits(before, edits);
            if (partial)
            {
                edits = lineEdits(before, _addr_c, edits);
            }

            return edits;

        }

        // prepareEdits returns a sorted copy of the edits
        private static (ptr<span.TokenConverter>, slice<TextEdit>, bool) prepareEdits(@string before, slice<TextEdit> edits)
        {
            ptr<span.TokenConverter> _p0 = default!;
            slice<TextEdit> _p0 = default;
            bool _p0 = default;

            var partial = false;
            var c = span.NewContentConverter("", (slice<byte>)before);
            var copied = make_slice<TextEdit>(len(edits));
            foreach (var (i, edit) in edits)
            {
                edit.Span, _ = edit.Span.WithAll(c);
                copied[i] = edit;
                partial = partial || edit.Span.Start().Offset() >= len(before) || edit.Span.Start().Column() > 1L || edit.Span.End().Column() > 1L;
            }
            SortTextEdits(copied);
            return (_addr_c!, copied, partial);

        }

        // lineEdits rewrites the edits to always be full line edits
        private static slice<TextEdit> lineEdits(@string before, ptr<span.TokenConverter> _addr_c, slice<TextEdit> edits)
        {
            ref span.TokenConverter c = ref _addr_c.val;

            var adjusted = make_slice<TextEdit>(0L, len(edits));
            TextEdit current = new TextEdit(Span:span.Invalid);
            foreach (var (_, edit) in edits)
            {
                if (current.Span.IsValid() && edit.Span.Start().Line() <= current.Span.End().Line())
                { 
                    // overlaps with the current edit, need to combine
                    // first get the gap from the previous edit
                    var gap = before[current.Span.End().Offset()..edit.Span.Start().Offset()]; 
                    // now add the text of this edit
                    current.NewText += gap + edit.NewText; 
                    // and then adjust the end position
                    current.Span = span.New(current.Span.URI(), current.Span.Start(), edit.Span.End());

                }
                else
                { 
                    // does not overlap, add previous run (if there is one)
                    adjusted = addEdit(before, adjusted, current); 
                    // and then remember this edit as the start of the next run
                    current = edit;

                }

            } 
            // add the current pending run if there is one
            return addEdit(before, adjusted, current);

        }

        private static slice<TextEdit> addEdit(@string before, slice<TextEdit> edits, TextEdit edit)
        {
            if (!edit.Span.IsValid())
            {
                return edits;
            } 
            // if edit is partial, expand it to full line now
            var start = edit.Span.Start();
            var end = edit.Span.End();
            if (start.Column() > 1L)
            { 
                // prepend the text and adjust to start of line
                var delta = start.Column() - 1L;
                start = span.NewPoint(start.Line(), 1L, start.Offset() - delta);
                edit.Span = span.New(edit.Span.URI(), start, end);
                edit.NewText = before[start.Offset()..start.Offset() + delta] + edit.NewText;

            }

            if (start.Offset() >= len(before) && start.Line() > 1L && before[len(before) - 1L] != '\n')
            { 
                // after end of file that does not end in eol, so join to last line of file
                // to do this we need to know where the start of the last line was
                var eol = strings.LastIndex(before, "\n");
                if (eol < 0L)
                { 
                    // file is one non terminated line
                    eol = 0L;

                }

                delta = len(before) - eol;
                start = span.NewPoint(start.Line() - 1L, 1L, start.Offset() - delta);
                edit.Span = span.New(edit.Span.URI(), start, end);
                edit.NewText = before[start.Offset()..start.Offset() + delta] + edit.NewText;

            }

            if (end.Column() > 1L)
            {
                var remains = before[end.Offset()..];
                eol = strings.IndexRune(remains, '\n');
                if (eol < 0L)
                {
                    eol = len(remains);
                }
                else
                {
                    eol++;
                }

                end = span.NewPoint(end.Line() + 1L, 1L, end.Offset() + eol);
                edit.Span = span.New(edit.Span.URI(), start, end);
                edit.NewText = edit.NewText + remains[..eol];

            }

            edits = append(edits, edit);
            return edits;

        }
    }
}}}}}}
