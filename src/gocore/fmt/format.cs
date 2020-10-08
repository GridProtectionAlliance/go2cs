//******************************************************************************************************
//  fmt_package.cs - Gbtc
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
//  06/21/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;

namespace go
{
    // This is a simple proxy for the fmt package for testing...
    public static partial class fmt_package
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static @string ToString(object arg)
        {
            Stringer? stringer = arg as Stringer ?? Stringer.As(arg);

            if (!(stringer is null))
                return stringer.String();

            error? err = arg as error ?? error.As(arg);

            if (!(err is null))
                return err.Error();

            if (arg is bool)
                return arg.ToString()!.ToLowerInvariant();

            return arg?.ToString() ?? "<nil>";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Print(params object[] args) =>
            Console.Write(string.Join(" ", args.Select(ToString)));

        /// <summary>
        /// Formats arguments in an implementation-specific way and writes the result to console along with a new line.
        /// </summary>
        /// <param name="args">Arguments to display.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Println(params object[] args)
        {
            if (args.Length == 1 && !(args[0] is @string) && !(args[0] is string) && args[0] is IEnumerable array)
            {
                Console.WriteLine($"[{string.Join(" ", array.Cast<object>().Select(ToString!))}]");
                return;
            }

            Console.WriteLine(string.Join(" ", args.Select(ToString)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Printf(@string format, params object[] args) =>
            Console.Write(Sprintf(format, args));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Sprintf(@string format, params object[] args) =>
            string.Format(format, args.Select(arg => (object)ToString(arg)).ToArray());
    }
}
