//******************************************************************************************************
//  RuntimeErrorPanic.cs - Gbtc
//
//  Copyright © 2024, Grid Protection Alliance.  All Rights Reserved.
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
//  12/27/2024 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Diagnostics.CodeAnalysis;

namespace go.golib;

/// <summary>
/// Represents common runtime error messages thrown in Go environment.
/// </summary>
public static class RuntimeErrorPanic
{
    private const string RuntimeErrorMessage = "runtime error: ";

    private const string NilPointerDereferenceMessage = $"{RuntimeErrorMessage}invalid memory address or nil pointer dereference";
    public static PanicException NilPointerDereference()
    {
        return new PanicException(NilPointerDereferenceMessage);
    }

    private const string IndexOutOfRangeMessage = $"{RuntimeErrorMessage}index out of range [{{0}}] with length {{1}}";
    public static PanicException IndexOutOfRange(int64 index, int64 length)
    {
        return new PanicException(string.Format(IndexOutOfRangeMessage, index, length));
    }

    private const string SliceBoundsOutOfRangeMessage = $"{RuntimeErrorMessage}slice bounds out of range ";
    public static PanicException SliceBoundsOutOfRange(int64 low, int64 high, int64 max, int64 capacity)
    {
        // Mirrors the Go runtime's message shapes for a slice expression s[low:high:max]
        string bounds;

        if (max > capacity)
            bounds = $"[::{max}] with capacity {capacity}";
        else if (high > max)
            bounds = max == capacity ? $"[:{high}] with capacity {capacity}" : $"[:{high}:{max}]";
        else if (low < 0)
            bounds = $"[{low}:]";
        else
            bounds = $"[{low}:{high}]";

        return new PanicException(SliceBoundsOutOfRangeMessage + bounds);
    }

    private const string IntegerDivideByZeroMessage = $"{RuntimeErrorMessage}integer divide by zero";

    /// <summary>
    /// Supplies the Go runtime's OWN divide-by-zero panic value (<c>runtime.divideError</c>, whose
    /// dynamic type is the unexported <c>runtime.errorString</c>).
    /// </summary>
    /// <remarks>
    /// Go's compiler lowers an integer division to a zero check plus <c>runtime.panicdivide()</c>, so
    /// <c>recover()</c> yields a value satisfying <c>runtime.Error</c> — math/bits' TestDiv32PanicZero
    /// asserts exactly that (<c>err.(runtime.Error)</c>) on the panic raised by the IMPLICIT hardware
    /// division in <c>Div32</c>, unlike Div/Div64 which panic explicitly. golib sits UNDER the converted
    /// <c>runtime</c> package and so cannot name that value; the runtime package registers it here
    /// (see its <c>panicvalues_impl.cs</c> bridge), leaving this layer dependency-free. When nothing has
    /// registered — a converted program that never links <c>runtime</c> — the panic falls back to the
    /// plain message below, which still reads and prints identically and only loses the type assertion.
    /// </remarks>
    public static Func<object>? IntegerDivideByZeroValue { get; set; }

    public static PanicException IntegerDivideByZero()
    {
        return new PanicException(IntegerDivideByZeroValue?.Invoke() ?? IntegerDivideByZeroMessage);
    }

    private const string MakeSliceLenOutOfRangeMessage = $"{RuntimeErrorMessage}makeslice: len out of range";
    public static PanicException MakeSliceLenOutOfRange()
    {
        return new PanicException(MakeSliceLenOutOfRangeMessage);
    }

    private const string MakeSliceCapOutOfRangeMessage = $"{RuntimeErrorMessage}makeslice: cap out of range";
    public static PanicException MakeSliceCapOutOfRange()
    {
        return new PanicException(MakeSliceCapOutOfRangeMessage);
    }

    /// <summary>
    /// Converts a .NET exception that corresponds to a Go runtime panic into a <see cref="PanicException"/>,
    /// so it can be recovered with <c>recover()</c> and reported like a Go panic. Returns <c>false</c>
    /// (leaving the exception to propagate unchanged) for exceptions that are not Go runtime panics.
    /// </summary>
    /// <param name="ex">Exception to inspect.</param>
    /// <param name="panic">Resulting panic when the exception maps to a Go runtime panic.</param>
    /// <returns><c>true</c> if <paramref name="ex"/> is (or maps to) a Go panic; otherwise <c>false</c>.</returns>
    public static bool TryAsPanic(Exception ex, [NotNullWhen(true)] out PanicException? panic)
    {
        switch (ex)
        {
            case PanicException panicException:
                panic = panicException;
                return true;
            case DivideByZeroException:
                // Go: integer division or modulo by zero panics with a runtime error. .NET raises
                // DivideByZeroException for the same operation; map it so recover() behaves like Go.
                panic = IntegerDivideByZero();
                return true;
            default:
                panic = null;
                return false;
        }
    }
}
