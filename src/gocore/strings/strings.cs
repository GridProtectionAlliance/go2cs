//******************************************************************************************************
//  strings_package.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  06/29/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using io = go.io_package;

namespace go;

public static partial class strings_package
{
    // A Reader implements the io.Reader, io.ReaderAt, io.Seeker, io.WriterTo,
    // io.ByteScanner, and io.RuneScanner interfaces by reading
    // from a string.
    // The zero value for Reader operates like a Reader of an empty string.
    public partial struct Reader {
        public @string s;
        public nint i;
        public int prevRune;
    }

    // NewReader returns a new Reader reading from s.
    // It is similar to bytes.NewBufferString but more efficient and read-only.
    public static ptr<Reader> NewReader(@string s) {
        return addr(new Reader(s, 0, -1));
    }

    public static (nint n, error err) Read(this ptr<Reader> r, in slice<byte> b) =>
        Read(ref r.val, b);

    // Size returns the original length of the underlying string.
    // Size is the number of bytes available for reading via ReadAt.
    // The returned value is always the same and is not affected by calls
    // to any other method.
    public static (nint n, error err) Read(this ref Reader r, in slice<byte> b) {
        nint n;
        error err = default!;

        if (r.i >= len(r.s))
            return (0, io.EOF);

        r.prevRune = -1;
        n = copy(b, r.s[(int)r.i..]);
        r.i += n;

        return (n, err);
    }

    public static @string Join(in slice<@string> source, @string separator) => 
        string.Join(separator, source);
}
