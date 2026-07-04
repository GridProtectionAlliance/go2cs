//******************************************************************************************************
//  ValueAdapterImplTemplate.cs - Gbtc
//
//  Copyright © 2026, Grid Protection Alliance.  All Rights Reserved.
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
//  07/04/2026 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.Text;
using static go2cs.Common;

namespace go2cs.Templates.InterfaceImpl;

/// <summary>
/// Generates the VALUE-sourced interface implementation adapter for a
/// <c>GoImplement&lt;TStruct, TInterface&gt;</c> attribute whose struct is FOREIGN — declared in
/// another assembly, so the value-boxing partial-struct implementation is impossible there and
/// unknowable here (os's <c>Signal</c> interface is downstream of <c>syscall.Signal</c>; neither
/// assembly can partial the other).
/// </summary>
/// <remarks>
/// A Go interface value created from a VALUE holds a copy, so the adapter wraps a COPY of the
/// struct and forwards interface members to its methods (binding the foreign package's extension
/// receivers through the file's usings). Equality is VALUE equality, matching Go's
/// value-interface comparison semantics.
/// </remarks>
internal class ValueAdapterImplTemplate : TemplateBase
{
    // Template Parameters
    public required string StructName;
    public required string InterfaceName;
    public required string AdapterName;
    public required string AdapterScope;
    public bool ImplementsFormattable;
    public required List<MethodInfo> Methods;

    public override string TemplateBody =>
        $$"""
             /// <summary>
             /// Value-sourced '{{GetSimpleName(InterfaceName)}}' implementation adapter for the foreign
             /// '{{StructName}}' — wraps a COPY, exactly as Go's interface holds a value.
             /// </summary>
             {{AdapterScope}} sealed class {{AdapterName}} : {{InterfaceName}}
             {
                 private readonly {{StructName}} m_value;

                 public {{AdapterName}}({{StructName}} value) => m_value = value;

                 {{MethodsImplementation}}

                 // Go value-interface equality compares the held values.
                 public override bool Equals(object? obj) => obj switch
                 {
                     {{AdapterName}} adapter => m_value.Equals(adapter.m_value),
                     {{StructName}} value => m_value.Equals(value),
                     _ => false
                 };

                 public override int GetHashCode() => m_value.GetHashCode();

                 public override string? ToString() => m_value.ToString();{{FormattableImplementation}}
             }
         """;

    // The interface may inherit System.IFormattable (the hand-finished io stub's Reader);
    // its member cannot forward through the copy uncast — implement directly.
    private string FormattableImplementation =>
        ImplementsFormattable
            ? "\r\n\r\n        string System.IFormattable.ToString(string? format, System.IFormatProvider? formatProvider) => m_value.ToString() ?? \"\";"
            : string.Empty;

    private string MethodsImplementation
    {
        get
        {
            StringBuilder result = new();

            foreach (MethodInfo method in Methods)
            {
                string simpleMethodName = GetSimpleName(method.Name);

                if (result.Length > 0)
                    result.Append("\r\n\r\n        ");

                result.Append($"{method.ReturnType} {method.GetSignature()} => m_value.{simpleMethodName}{method.GetGenericSignature()}({method.CallParameters});");
            }

            return result.ToString();
        }
    }
}
