// print.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

namespace go;

public static partial class fmt_package
{
    // Stringer is implemented by any value that has a String method,
    // which defines the ``native'' format for that value.
    // The String method is used to print values passed as an operand
    // to any format that accepts a string or to an unformatted printer
    // such as Print.
    public partial interface Stringer  {
        @string String();
    }
}
