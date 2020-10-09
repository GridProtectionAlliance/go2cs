// Inferno utils/8l/asm.c
// https://bitbucket.org/inferno-os/inferno-os/src/master/utils/8l/asm.c
//
//    Copyright © 1994-1999 Lucent Technologies Inc.  All rights reserved.
//    Portions Copyright © 1995-1997 C H Forsyth (forsyth@terzarima.net)
//    Portions Copyright © 1997-1999 Vita Nuova Limited
//    Portions Copyright © 2000-2007 Vita Nuova Holdings Limited (www.vitanuova.com)
//    Portions Copyright © 2004,2006 Bruce Ellis
//    Portions Copyright © 2005-2007 C H Forsyth (forsyth@terzarima.net)
//    Revisions Copyright © 2000-2007 Lucent Technologies Inc. and others
//    Portions Copyright © 2009 The Go Authors. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

// package sym -- go2cs converted at 2020 October 09 05:51:12 UTC
// import "cmd/oldlink/internal/sym" ==> using sym = go.cmd.oldlink.@internal.sym_package
// Original source: C:\Go\src\cmd\oldlink\internal\sym\segment.go

using static go.builtin;

namespace go {
namespace cmd {
namespace oldlink {
namespace @internal
{
    public static partial class sym_package
    {
        // Terrible but standard terminology.
        // A segment describes a block of file to load into memory.
        // A section further describes the pieces of that block for
        // use in debuggers and such.
        public partial struct Segment
        {
            public byte Rwx; // permission as usual unix bits (5 = r-x etc)
            public ulong Vaddr; // virtual address
            public ulong Length; // length in memory
            public ulong Fileoff; // file offset
            public ulong Filelen; // length on disk
            public slice<ptr<Section>> Sections;
        }

        public partial struct Section
        {
            public byte Rwx;
            public short Extnum;
            public int Align;
            public @string Name;
            public ulong Vaddr;
            public ulong Length;
            public ptr<Segment> Seg;
            public ulong Reloff;
            public ulong Rellen;
        }
    }
}}}}
