//******************************************************************************************************
//  string_package_Reader.cs - Gbtc
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
//  06/30/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

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
