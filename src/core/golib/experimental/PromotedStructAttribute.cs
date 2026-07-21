// PromotedStructAttribute.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

// ReSharper disable CheckNamespace

using System;

namespace go.experimental
{
    /// <summary>
    /// Marks a declared type as having an anonymous promoted structure element.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class PromotedStructAttribute : Attribute
    {
        /// <summary>
        /// Promoted declaration type.
        /// </summary>
        public Type PromotedType { get; }

        /// <summary>
        /// Creates a new <see cref="PromotedStructAttribute"/>.
        /// </summary>
        /// <param name="promotedType">Type of promoted structure.</param>
        public PromotedStructAttribute(Type promotedType) => PromotedType = promotedType;
    }
}
