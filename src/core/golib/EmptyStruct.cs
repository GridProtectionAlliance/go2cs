// EmptyStruct.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

namespace go;

/// <summary>
/// Represents an empty struct.
/// </summary>
public struct EmptyStruct
{
    public EmptyStruct(NilType _)
    {
    }

    public bool Equals(EmptyStruct other) => true;

    public override bool Equals(object? obj) => obj is EmptyStruct other && Equals(other);

    public override int GetHashCode() => base.GetHashCode();

    public static bool operator ==(EmptyStruct left, EmptyStruct right) => left.Equals(right);

    public static bool operator !=(EmptyStruct left, EmptyStruct right) => !(left == right);

    // Handle comparisons between 'nil' and struct 'EmptyStruct'
    public static bool operator ==(EmptyStruct value, NilType nil) => value.Equals(default(EmptyStruct));

    public static bool operator !=(EmptyStruct value, NilType nil) => !(value == nil);

    public static bool operator ==(NilType nil, EmptyStruct value) => value == nil;

    public static bool operator !=(NilType nil, EmptyStruct value) => value != nil;

    public static implicit operator EmptyStruct(NilType nil) => default(EmptyStruct);

    public override string ToString() => "{}";
}
