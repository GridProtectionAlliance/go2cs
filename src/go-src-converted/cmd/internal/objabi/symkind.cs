// Derived from Inferno utils/6l/l.h and related files.
// https://bitbucket.org/inferno-os/inferno-os/src/master/utils/6l/l.h
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

// package objabi -- go2cs converted at 2020 October 09 05:08:53 UTC
// import "cmd/internal/objabi" ==> using objabi = go.cmd.@internal.objabi_package
// Original source: C:\Go\src\cmd\internal\objabi\symkind.go

using static go.builtin;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class objabi_package
    {
        // A SymKind describes the kind of memory represented by a symbol.
        public partial struct SymKind // : byte
        {
        }

        // Defined SymKind values.
        // These are used to index into cmd/link/internal/sym/AbiSymKindToSymKind
        //
        // TODO(rsc): Give idiomatic Go names.
        //go:generate stringer -type=SymKind
 
        // An otherwise invalid zero value for the type
        public static readonly SymKind Sxxx = (SymKind)iota; 
        // Executable instructions
        public static readonly var STEXT = 0; 
        // Read only static data
        public static readonly var SRODATA = 1; 
        // Static data that does not contain any pointers
        public static readonly var SNOPTRDATA = 2; 
        // Static data
        public static readonly var SDATA = 3; 
        // Statically data that is initially all 0s
        public static readonly var SBSS = 4; 
        // Statically data that is initially all 0s and does not contain pointers
        public static readonly var SNOPTRBSS = 5; 
        // Thread-local data that is initially all 0s
        public static readonly var STLSBSS = 6; 
        // Debugging data
        public static readonly var SDWARFINFO = 7;
        public static readonly var SDWARFRANGE = 8;
        public static readonly var SDWARFLOC = 9;
        public static readonly var SDWARFLINES = 10; 
        // ABI alias. An ABI alias symbol is an empty symbol with a
        // single relocation with 0 size that references the native
        // function implementation symbol.
        //
        // TODO(austin): Remove this and all uses once the compiler
        // generates real ABI wrappers rather than symbol aliases.
        public static readonly var SABIALIAS = 11; 
        // Coverage instrumentation counter for libfuzzer.
        public static readonly var SLIBFUZZER_EXTRA_COUNTER = 12; 
        // Update cmd/link/internal/sym/AbiSymKindToSymKind for new SymKind values.

    }
}}}
