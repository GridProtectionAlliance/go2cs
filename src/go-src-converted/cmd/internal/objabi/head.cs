// Derived from Inferno utils/6l/l.h and related files.
// https://bitbucket.org/inferno-os/inferno-os/src/default/utils/6l/l.h
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

// package objabi -- go2cs converted at 2020 August 29 08:46:20 UTC
// import "cmd/internal/objabi" ==> using objabi = go.cmd.@internal.objabi_package
// Original source: C:\Go\src\cmd\internal\objabi\head.go
using fmt = go.fmt_package;
using static go.builtin;

namespace go {
namespace cmd {
namespace @internal
{
    public static partial class objabi_package
    {
        // HeadType is the executable header type.
        public partial struct HeadType // : byte
        {
        }

        public static readonly HeadType Hunknown = iota;
        public static readonly var Hdarwin = 0;
        public static readonly var Hdragonfly = 1;
        public static readonly var Hfreebsd = 2;
        public static readonly var Hlinux = 3;
        public static readonly var Hnacl = 4;
        public static readonly var Hnetbsd = 5;
        public static readonly var Hopenbsd = 6;
        public static readonly var Hplan9 = 7;
        public static readonly var Hsolaris = 8;
        public static readonly var Hwindows = 9;

        private static error Set(this ref HeadType h, @string s)
        {
            switch (s)
            {
                case "darwin": 
                    h.Value = Hdarwin;
                    break;
                case "dragonfly": 
                    h.Value = Hdragonfly;
                    break;
                case "freebsd": 
                    h.Value = Hfreebsd;
                    break;
                case "linux": 

                case "android": 
                    h.Value = Hlinux;
                    break;
                case "nacl": 
                    h.Value = Hnacl;
                    break;
                case "netbsd": 
                    h.Value = Hnetbsd;
                    break;
                case "openbsd": 
                    h.Value = Hopenbsd;
                    break;
                case "plan9": 
                    h.Value = Hplan9;
                    break;
                case "solaris": 
                    h.Value = Hsolaris;
                    break;
                case "windows": 
                    h.Value = Hwindows;
                    break;
                default: 
                    return error.As(fmt.Errorf("invalid headtype: %q", s));
                    break;
            }
            return error.As(null);
        }

        private static @string String(this ref HeadType h)
        {

            if (h.Value == Hdarwin) 
                return "darwin";
            else if (h.Value == Hdragonfly) 
                return "dragonfly";
            else if (h.Value == Hfreebsd) 
                return "freebsd";
            else if (h.Value == Hlinux) 
                return "linux";
            else if (h.Value == Hnacl) 
                return "nacl";
            else if (h.Value == Hnetbsd) 
                return "netbsd";
            else if (h.Value == Hopenbsd) 
                return "openbsd";
            else if (h.Value == Hplan9) 
                return "plan9";
            else if (h.Value == Hsolaris) 
                return "solaris";
            else if (h.Value == Hwindows) 
                return "windows";
                        return fmt.Sprintf("HeadType(%d)", h.Value);
        }
    }
}}}
