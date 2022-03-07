// Inferno utils/5l/asm.c
// https://bitbucket.org/inferno-os/inferno-os/src/master/utils/5l/asm.c
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

// package arm -- go2cs converted at 2022 March 06 23:22:35 UTC
// import "cmd/link/internal/arm" ==> using arm = go.cmd.link.@internal.arm_package
// Original source: C:\Program Files\Go\src\cmd\link\internal\arm\l.go


namespace go.cmd.link.@internal;

public static partial class arm_package {

    // Writing object files.

    // Inferno utils/5l/l.h
    // https://bitbucket.org/inferno-os/inferno-os/src/master/utils/5l/l.h
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
private static readonly nint maxAlign = 8; // max data alignment
private static readonly nint minAlign = 1; // min data alignment
private static readonly nint funcAlign = 4; // single-instruction alignment

/* Used by ../internal/ld/dwarf.go */
private static readonly nint dwarfRegSP = 13;
private static readonly nint dwarfRegLR = 14;


} // end arm_package
