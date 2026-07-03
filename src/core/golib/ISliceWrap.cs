//******************************************************************************************************
//  ISliceWrap.cs - Gbtc
//
//  Copyright (c) 2026, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements.
//  See the NOTICE file distributed with this work for additional information regarding copyright
//  ownership. The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on
//  an "AS-IS" BASIS, WITHOUT WARRANTIES OR REPRESENTATIONS OF ANY KIND, either express or implied.
//  Refer to the License for the specific language governing permissions and limitations.
//
//******************************************************************************************************
// ReSharper disable InconsistentNaming

namespace go;

/// <summary>
/// Marks a slice-shaped type that can wrap an existing <see cref="slice{T}"/> window in its own
/// type WITHOUT copying — the factory a generic body needs to keep a constrained type parameter
/// (<c>S ~[]E</c>) through sub-slice and append operations (Go's sub-slice of a named slice type
/// yields the same named type, sharing backing storage).
/// </summary>
/// <typeparam name="TSelf">Implementing type (the named slice type itself).</typeparam>
/// <typeparam name="T">Element type.</typeparam>
public interface ISliceWrap<out TSelf, T>
{
    /// <summary>
    /// Wraps the given slice window in <typeparamref name="TSelf"/>, sharing its backing storage.
    /// </summary>
    /// <param name="source">Slice window to wrap.</param>
    /// <returns>A <typeparamref name="TSelf"/> over the same storage.</returns>
    static abstract TSelf Wrap(in slice<T> source);
}
