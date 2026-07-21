// PointerTypeTemplate.cs - Gbtc
// Copyright © 2026 The go2cs Authors. All rights reserved.
//
// Use of this source code is governed by an MIT-style license
// that can be found in the LICENSE file.

using static go2cs.Symbols;

namespace go2cs.Templates.InheritedType;

internal static class PointerTypeTemplate
{
    public static string Generate(string className, string targetTypeName) =>
        $$"""
        
                public override bool Equals(object? obj) => obj is {{className}} other && m_value == other.m_value;
                
                public override int GetHashCode() => m_value.GetHashCode();
        
                public ref {{targetTypeName}} Value => ref m_value.Value;

                public bool IsNull => m_value is null || m_value.IsNull;
                
                public {{PointerPrefix}}<TElem> of<TElem>(FieldRefFunc<TElem> fieldRefFunc) => m_value.of(fieldRefFunc);
                
                public {{PointerPrefix}}<TElem> of<TElem>(FieldRefFunc<{{targetTypeName}}, TElem> fieldRefFunc) => m_value.of(fieldRefFunc);
                
                public {{PointerPrefix}}<TElem> at<TElem>(nint index) => m_value.at<TElem>(index);
                
                static {{targetTypeName}} IPointer<{{targetTypeName}}>.operator ~(IPointer<{{targetTypeName}}> value) => value.Value;

                public static unsafe implicit operator {{className}}(uintptr value)
                {
                    return new {{className}}(new {{PointerPrefix}}<{{targetTypeName}}>(*({{targetTypeName}}*)value));
                }
                
                public static unsafe implicit operator uintptr({{className}} value)
                {
                    fixed (void* ptr = &value.Value)
                        return (uintptr)ptr;
                }
                
                public static unsafe implicit operator {{className}}(void* value)
                {
                    return new {{className}}(new {{PointerPrefix}}<{{targetTypeName}}>(*({{targetTypeName}}*)value));
                }
                
                public static unsafe implicit operator void*({{className}} value)
                {
                    fixed ({{targetTypeName}}* ptr = &value.Value)
                        return ptr;
                }
        """;
}
