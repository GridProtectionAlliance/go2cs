// GoPackageAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using System;

namespace go;

/// <summary>
/// Marks a class as a Go package.
/// </summary>
/// <param name="packageName">Defines the package name.</param>
[AttributeUsage(AttributeTargets.Class)]
public class GoPackageAttribute(string packageName) : Attribute
{
    /// <summary>
    /// Gets the package name.
    /// </summary>
    public string PackageName => packageName;
}
