// Copyright 2019 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Package difftest supplies a set of tests that will operate on any
// implementation of a diff algorithm as exposed by
// "golang.org/x/tools/internal/lsp/diff"
// package difftest -- go2cs converted at 2020 October 08 04:54:43 UTC
// import "golang.org/x/tools/internal/lsp/diff/difftest" ==> using difftest = go.golang.org.x.tools.@internal.lsp.diff.difftest_package
// Original source: C:\Users\ritchie\go\src\golang.org\x\tools\internal\lsp\diff\difftest\difftest.go
using fmt = go.fmt_package;
using testing = go.testing_package;

using diff = go.golang.org.x.tools.@internal.lsp.diff_package;
using span = go.golang.org.x.tools.@internal.span_package;
using static go.builtin;
using System;

namespace go {
namespace golang.org {
namespace x {
namespace tools {
namespace @internal {
namespace lsp {
namespace diff
{
    public static partial class difftest_package
    {
        public static readonly @string FileA = (@string)"from";
        public static readonly @string FileB = (@string)"to";
        public static readonly @string UnifiedPrefix = (@string)"--- " + FileA + "\n+++ " + FileB + "\n";




        private static void init()
        { 
            // expand all the spans to full versions
            // we need them all to have their line number and column
            foreach (var (_, tc) in TestCases)
            {
                var c = span.NewContentConverter("", (slice<byte>)tc.In);
                {
                    var i__prev2 = i;

                    foreach (var (__i) in tc.Edits)
                    {
                        i = __i;
                        tc.Edits[i].Span, _ = tc.Edits[i].Span.WithAll(c);
                    }

                    i = i__prev2;
                }

                {
                    var i__prev2 = i;

                    foreach (var (__i) in tc.LineEdits)
                    {
                        i = __i;
                        tc.LineEdits[i].Span, _ = tc.LineEdits[i].Span.WithAll(c);
                    }

                    i = i__prev2;
                }
            }

        }

        public static void DiffTest(ptr<testing.T> _addr_t, diff.ComputeEdits compute)
        {
            ref testing.T t = ref _addr_t.val;

            t.Helper();
            foreach (var (_, test) in TestCases)
            {
                t.Run(test.Name, t =>
                {
                    t.Helper();
                    var edits = compute(span.URIFromPath("/" + test.Name), test.In, test.Out);
                    var got = diff.ApplyEdits(test.In, edits);
                    var unified = fmt.Sprint(diff.ToUnified(FileA, FileB, test.In, edits));
                    if (got != test.Out)
                    {
                        t.Errorf("got patched:\n%v\nfrom diff:\n%v\nexpected:\n%v", got, unified, test.Out);
                    }

                    if (!test.NoDiff && unified != test.Unified)
                    {
                        t.Errorf("got diff:\n%v\nexpected:\n%v", unified, test.Unified);
                    }

                });

            }

        }

        private static span.Span newSpan(long start, long end)
        {
            return span.New("", span.NewPoint(0L, 0L, start), span.NewPoint(0L, 0L, end));
        }
    }
}}}}}}}
