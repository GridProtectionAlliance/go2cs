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
using System.Text.RegularExpressions;

namespace go;

// This is a simple proxy for the fmt package for testing...
public static partial class fmt_package
{
    [GeneratedRegex(@"\%.*(?<type>[vtbcdoOqxXUeEfFgGspT%])", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace)]
    private static partial Regex FormatExpr();

    private static readonly Regex s_formatExpr = FormatExpr();
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static @string ToString(object arg)
    {
        Stringer? stringer = arg as Stringer ?? Stringer.As(arg);

        if (stringer is not null)
            return stringer.String();

        error? err = arg as error ?? error.As(arg);

        if (err is not null)
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
        if (args.Length == 1 && args[0] is not @string && args[0] is not string && args[0] is IEnumerable array)
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
    public static string Sprintf(@string format, params object[] args)
    {
        (format, args) = ConvertFormat(format, args);
        return string.Format(format, args.Select(arg => (object)ToString(arg)).ToArray());
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
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Print(ReadOnlySpan<byte> arg) =>
        Console.Write(((sstring)arg).ToString());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Print(ReadOnlySpan<byte> arg1, ReadOnlySpan<byte> arg2) =>
        Console.Write($"{(sstring)arg1} {(sstring)arg2}");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Print(ReadOnlySpan<byte> arg1, ReadOnlySpan<byte> arg2, ReadOnlySpan<byte> arg3) =>
        Console.Write($"{(sstring)arg1} {(sstring)arg2} {(sstring)arg3}");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Print(ReadOnlySpan<byte> arg1, ReadOnlySpan<byte> arg2, ReadOnlySpan<byte> arg3, ReadOnlySpan<byte> arg4) =>
        Console.Write($"{(sstring)arg1} {(sstring)arg2} {(sstring)arg3} {(sstring)arg4}");
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Println(ReadOnlySpan<byte> arg) => 
        Console.WriteLine((sstring)arg);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Println(ReadOnlySpan<byte> arg1, ReadOnlySpan<byte> arg2) =>
        Console.WriteLine($"{(sstring)arg1} {(sstring)arg2}");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Println(ReadOnlySpan<byte> arg1, ReadOnlySpan<byte> arg2, ReadOnlySpan<byte> arg3) =>
        Console.WriteLine($"{(sstring)arg1} {(sstring)arg2} {(sstring)arg3}");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Println(ReadOnlySpan<byte> arg1, ReadOnlySpan<byte> arg2, ReadOnlySpan<byte> arg3, ReadOnlySpan<byte> arg4) =>
        Console.WriteLine($"{(sstring)arg1} {(sstring)arg2} {(sstring)arg3} {(sstring)arg4}");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Printf(ReadOnlySpan<byte> format, params object[] args) =>
        Printf((@string)format, args);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Sprintf(ReadOnlySpan<byte> format, params object[] args) =>
        Sprintf((@string)format, args);
}
