// PanicException.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;
using System.Diagnostics;

namespace go;

/// <summary>
/// Represents an exception for the "panic" keyword.
/// </summary>
[DebuggerNonUserCode]
public class PanicException(object? state, Exception? innerException = null) : 
    Exception(state?.ToString() ?? "nil", innerException)
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public object? State { get; } = state;
}
