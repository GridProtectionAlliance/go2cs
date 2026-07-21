// ISupportMake.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

namespace go;

/// <summary>
/// Defines an interface to support 'make' function parameters.
/// </summary>
public interface ISupportMake<out T>
{
    /// <summary>
    /// Initializes type with make size and capacity parameters.
    /// </summary>
    /// <param name="p1">First integer parameter, commonly for size.</param>
    /// <param name="p2">Second integer parameter, commonly for capacity.</param>
    static abstract T Make(nint p1, nint p2);
}
