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
using System.Text.RegularExpressions;
using ꓸꓸꓸobject = System.Span<object>;

namespace go;

// This is a simple proxy for the fmt package for testing...
public static partial class fmt_package
{
    [GeneratedRegex(@"\%(?:[0-9]*(?:\.[0-9]*)?)?(?<type>[vtbcdoOqxXUeEfFgGspT%])", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace)]
    private static partial Regex FormatExpr();

    private static readonly Regex s_formatExpr = FormatExpr();
    
    private static @string ToString(object arg)
    {
        if (arg is Stringer stringer)
            return stringer.String();

        if (arg is error err)
            return err.Error();

        if (arg is bool)
            return arg.ToString()!.ToLowerInvariant();

        return arg?.ToString() ?? "<nil>";
    }

    public static void Print(params object[] args) =>
        Console.Write(string.Join("", args.Select(ToString)));

    /// <summary>
    /// Formats arguments in an implementation-specific way and writes the result to console along with a new line.
    /// </summary>
    /// <param name="args">Arguments to display.</param>
    public static void Println(params object[] args)
    {
        if (args.Length == 1 && args[0] is not @string && args[0] is not string && args[0] is IEnumerable array)
        {
            Console.WriteLine($"[{string.Join(" ", array.Cast<object>().Select(ToString!))}]");
            return;
        }

        Console.WriteLine(string.Join(" ", args.Select(ToString)));
    }

    public static void Printf(@string format, params object[] args) =>
        Console.Write(Sprintf(format, args));

    public static string Sprint(object? arg) => arg?.ToString() ?? "<nil>";

    public static string Sprintf(@string format, params object[] args)
    {
        (format, args) = ConvertFormat(format, args);
        return string.Format(format, args.Select(arg => (object)ToString(arg)).ToArray());
    }

    public static string Sprintf(string format, params ꓸꓸꓸobject args)
    {
        (format, object[] args2) = ConvertFormat(format, args.ToArray());
        return string.Format(format, args2.Select(arg => (object)ToString(arg)).ToArray());
    }

    private static (string, object[]) ConvertFormat(string format, object[] args)
    {
        int index = 0;
        
        foreach (Match match in s_formatExpr.Matches(format))
        {
            switch (match.Groups["type"].Value)
            {
                case "%":
                    format = new Regex(Regex.Escape(match.Value)).Replace(format, "%", 1);
                    break;
                case "T":
                    string gotTypeName = GetGoTypeName(args[index]);
                    
                    if (gotTypeName.StartsWith("go."))
                        gotTypeName = gotTypeName[3..];

                    if (gotTypeName == "nil")
                        gotTypeName = "<nil>";

                    args[index] = gotTypeName;
                    format = new Regex(Regex.Escape(match.Value)).Replace(format, $"{{{index++}}}", 1);
                    break;
                default:
                    format = new Regex(Regex.Escape(match.Value)).Replace(format, $"{{{index++}}}", 1);
                    break;
            }
        }

        return (format, args);
    }
    
    public static void Printf(ReadOnlySpan<byte> format, params object[] args) =>
        Printf((@string)format, args);

    public static string Sprintf(ReadOnlySpan<byte> format, params object[] args) =>
        Sprintf((@string)format, args);

    public static error Errorf(ReadOnlySpan<byte> format, params object[] args) =>
        null!;
}
