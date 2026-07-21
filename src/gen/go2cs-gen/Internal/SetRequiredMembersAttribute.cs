// SetRequiredMembersAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace
namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// Specifies that this constructor sets all required members for the current type, and callers
/// do not need to set any required members themselves.
/// </summary>
[AttributeUsage(AttributeTargets.Constructor)]
internal sealed class SetsRequiredMembersAttribute : Attribute
{ }
