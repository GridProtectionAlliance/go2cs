// strings_Reader.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

namespace go;

public static partial class strings_package
{
    public partial struct Reader
    {
        public Reader((@string, nint, int) i) :
            this(i.Item1, i.Item2)
        { }

        public Reader(@string s = default, nint i = default, int prevRune = default)
        {
            this.s = s;
            this.i = i;
            this.prevRune = prevRune;
        }

        public override string ToString() => $"{{{s}}} {{{i}}} {{{prevRune}}}";

        public static implicit operator Reader((@string, nint, int) value) => new Reader(value);

        // Person to nil comparisons
        public static bool operator ==(Reader obj, NilType _) => obj.Equals(default(Reader));

        public static bool operator !=(Reader obj, NilType nil) => !(obj == nil);

        public static bool operator ==(NilType nil, Reader obj) => obj == nil;

        public static bool operator !=(NilType nil, Reader obj) => obj != nil;
    }
}
