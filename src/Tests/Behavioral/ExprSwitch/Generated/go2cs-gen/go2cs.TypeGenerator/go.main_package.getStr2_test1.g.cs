﻿//---------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool. Changes to this
//     file may cause incorrect behavior and will be lost
//     if the code is regenerated.
// </auto-generated>
//---------------------------------------------------------

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using go.runtime;
using ꓸꓸꓸany = global::System.Span<object>;

#nullable enable

namespace go;

public static partial class main_package
{
    [GeneratedCode("go2cs-gen", "0.1.4")]
    internal partial interface getStr2_test1
    {
        // Runtime interface conversion methods
        public static getStr2_test1 As<ΔTTarget>(in ΔTTarget target) =>
            (ΔgetStr2_test1<ΔTTarget>)target!;

        public static getStr2_test1 As<ΔTTarget>(ж<ΔTTarget> target_ptr) =>
            (ΔgetStr2_test1<ΔTTarget>)target_ptr;

        public static getStr2_test1? As(object target) =>
            typeof(ΔgetStr2_test1<>).CreateInterfaceHandler<getStr2_test1>(target);            
    }

    // Defines a runtime type for duck-typed interface implementations based on existing
    // extension methods that satisfy interface. This class is only used as fallback for
    // when the interface was not able to be implemented at transpile time, e.g., with
    // dynamically declared anonymous interfaces used with type assertions.
    [GeneratedCode("go2cs-gen", "0.1.4")]
    internal class ΔgetStr2_test1<ΔTTarget> : getStr2_test1
    {
        private ΔTTarget m_target = default!;
        private readonly ж<ΔTTarget>? m_target_ptr;
        private readonly bool m_target_is_ptr;
    
        public ref ΔTTarget Target
        {
            get
            {
                if (m_target_is_ptr && m_target_ptr is not null)
                    return ref m_target_ptr.val;
    
                return ref m_target;
            }
        }
    
        public ΔgetStr2_test1(in ΔTTarget target)
        {
            m_target = target;
        }
    
        public ΔgetStr2_test1(ж<ΔTTarget> target_ptr)
        {
            m_target_ptr = target_ptr;
            m_target_is_ptr = true;
        }
    
        public static explicit operator ΔgetStr2_test1<ΔTTarget>(in ж<ΔTTarget> target_ptr) => new(target_ptr);
    
        public static explicit operator ΔgetStr2_test1<ΔTTarget>(in ΔTTarget target) => new(target);

        public override int GetHashCode() => Target?.GetHashCode() ?? 0;

        public static bool operator ==(ΔgetStr2_test1<ΔTTarget>? left, ΔgetStr2_test1<ΔTTarget>? right) => left?.Equals(right) ?? right is null;
        
        public static bool operator !=(ΔgetStr2_test1<ΔTTarget>? left, ΔgetStr2_test1<ΔTTarget>? right) => !(left == right);

        #region [ Operator Constraint Implementations ]

        // These operator constraints exist to satisfy possible constraints defined on source interface,
        // however, the instance of this class is only used to implement the interface methods, so these
        // operators are only placeholders and not actually functional.

        public static bool operator <(ΔgetStr2_test1<ΔTTarget> left, ΔgetStr2_test1<ΔTTarget> right) => false;
        
        public static bool operator <=(ΔgetStr2_test1<ΔTTarget> left, ΔgetStr2_test1<ΔTTarget> right) => false;
        
        public static bool operator >(ΔgetStr2_test1<ΔTTarget> left, ΔgetStr2_test1<ΔTTarget> right) => false;
        
        public static bool operator >=(ΔgetStr2_test1<ΔTTarget> left, ΔgetStr2_test1<ΔTTarget> right) => false;
        
        public static ΔgetStr2_test1<ΔTTarget> operator +(ΔgetStr2_test1<ΔTTarget> left, ΔgetStr2_test1<ΔTTarget> right) => default!;
        
        public static ΔgetStr2_test1<ΔTTarget> operator -(ΔgetStr2_test1<ΔTTarget> left, ΔgetStr2_test1<ΔTTarget> right) => default!;
        
        public static ΔgetStr2_test1<ΔTTarget> operator -(ΔgetStr2_test1<ΔTTarget> value) => default!;
        
        public static ΔgetStr2_test1<ΔTTarget> operator *(ΔgetStr2_test1<ΔTTarget> left, ΔgetStr2_test1<ΔTTarget> right) => default!;
        
        public static ΔgetStr2_test1<ΔTTarget> operator /(ΔgetStr2_test1<ΔTTarget> left, ΔgetStr2_test1<ΔTTarget> right) => default!;
        
        public static ΔgetStr2_test1<ΔTTarget> operator %(ΔgetStr2_test1<ΔTTarget> left, ΔgetStr2_test1<ΔTTarget> right) => default!;

        public static ΔgetStr2_test1<ΔTTarget> operator &(ΔgetStr2_test1<ΔTTarget> left, ΔgetStr2_test1<ΔTTarget> right) => default!;
        
        public static ΔgetStr2_test1<ΔTTarget> operator |(ΔgetStr2_test1<ΔTTarget> left, ΔgetStr2_test1<ΔTTarget> right) => default!;
        
        public static ΔgetStr2_test1<ΔTTarget> operator ^(ΔgetStr2_test1<ΔTTarget> left, ΔgetStr2_test1<ΔTTarget> right) => default!;
        
        public static ΔgetStr2_test1<ΔTTarget> operator ~(ΔgetStr2_test1<ΔTTarget> value) => default!;
        
        public static ΔgetStr2_test1<ΔTTarget> operator <<(ΔgetStr2_test1<ΔTTarget> value, ΔgetStr2_test1<ΔTTarget> shiftAmount) => default!;
        
        public static ΔgetStr2_test1<ΔTTarget> operator >>(ΔgetStr2_test1<ΔTTarget> value, ΔgetStr2_test1<ΔTTarget> shiftAmount) => default!;
        
        public static ΔgetStr2_test1<ΔTTarget> operator >>>(ΔgetStr2_test1<ΔTTarget> value, ΔgetStr2_test1<ΔTTarget> shiftAmount) => default!;
        
        #endregion
    
        // Enable comparisons between nil and ΔgetStr2_test1<ΔTTarget> interface instance
        public static bool operator ==(ΔgetStr2_test1<ΔTTarget> value, NilType nil) => Activator.CreateInstance<ΔgetStr2_test1<ΔTTarget>>().Equals(value);
    
        public static bool operator !=(ΔgetStr2_test1<ΔTTarget> value, NilType nil) => !(value == nil);
    
        public static bool operator ==(NilType nil, ΔgetStr2_test1<ΔTTarget> value) => value == nil;
    
        public static bool operator !=(NilType nil, ΔgetStr2_test1<ΔTTarget> value) => value != nil;
    }
}
