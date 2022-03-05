//******************************************************************************************************
//  ScannerBase_Parameters.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
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
//  05/03/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using go2cs.Metadata;
using System.Collections.Generic;
using static go2cs.Common;

namespace go2cs;

public partial class ScannerBase
{
    // Stack handlers:
    //  receiver (required)
    //  signature (required)
    //  result (optional)
    protected readonly ParseTreeValues<List<ParameterInfo>> Parameters = new ParseTreeValues<List<ParameterInfo>>();

    private readonly ParseTreeValues<List<ParameterInfo>> m_parameterDeclarations = new ParseTreeValues<List<ParameterInfo>>();

    public override void ExitParameters(GoParser.ParametersContext context)
    {
        List<ParameterInfo> parameters = new List<ParameterInfo>();

        for (int i = 0; i < context.parameterDecl().Length; i++)
        {
            if (m_parameterDeclarations.TryGetValue(context.parameterDecl(i), out List<ParameterInfo> parameterDeclarations))
                parameters.AddRange(parameterDeclarations);
        }

        Parameters[context] = parameters;
    }

    public override void ExitParameterDecl(GoParser.ParameterDeclContext context)
    {
        List<ParameterInfo> parameters = new List<ParameterInfo>();

        Identifiers.TryGetValue(context.identifierList(), out string[] identifiers);

        // Check for variadic expression
        bool hasVariadicParameter = context.GetText().Contains("...");

        if (!Types.TryGetValue(context.type_(), out TypeInfo typeInfo))
            typeInfo = TypeInfo.ObjectType;

        if (identifiers is not null)
        {
            for (int i = 0; i < identifiers.Length; i++)
            {
                string identifier = SanitizedIdentifier(identifiers[i]);

                // Check for unnamed parameters
                if (string.IsNullOrWhiteSpace(identifier))
                    identifier = $"_p{parameters.Count}";

                if (i == identifiers.Length - 1 && hasVariadicParameter)
                {
                    TypeInfo variadicType = ConvertByRefToBasicPointer(typeInfo.Clone());

                    if (variadicType.TypeClass != TypeClass.Array)
                    {
                        variadicType.TypeClass = TypeClass.Array;
                        variadicType.TypeName += "[]";
                        variadicType.FullTypeName += "[]";
                    }

                    parameters.Add(new()
                    {
                        Name = identifier,
                        Type = variadicType,
                        IsVariadic = true,                            
                    });
                }
                else
                {
                    parameters.Add(new()
                    {
                        Name = identifier,
                        Type = typeInfo,
                        IsVariadic = false
                    });
                }
            }
        }
        else if (hasVariadicParameter)
        {
            string identifier = $"_p{parameters.Count}";

            // Unnamed variadic parameter
            parameters.Add(new()
            {
                Name = identifier,
                Type = ConvertByRefToBasicPointer(typeInfo),
                IsVariadic = true
            });
        }
        else
        {
            string identifier = $"_p{parameters.Count}";

            // Unnamed parameter
            parameters.Add(new()
            {
                Name = identifier,
                Type = typeInfo,
                IsVariadic = false
            });
        }

        m_parameterDeclarations[context] = parameters;
    }
}
