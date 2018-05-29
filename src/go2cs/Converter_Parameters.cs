//******************************************************************************************************
//  Converter_Parameters.cs - Gbtc
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

using System;
using System.Collections.Generic;
using static go2cs.Common;

namespace go2cs
{
    public partial class Converter
    {
        // Stack handlers:
        //  receiver (required)
        //  signature (required)
        //  result (optional)
        private readonly ParseTreeValues<string> m_parameters = new ParseTreeValues<string>();

        // Stack handlers:
        //  parameters via parameterList (required)
        private readonly ParseTreeValues<List<string>> m_parameterDeclarations = new ParseTreeValues<List<string>>();

        public override void EnterParameters(GolangParser.ParametersContext context)
        {
            GolangParser.ParameterListContext parameterList = context.parameterList();

            if (parameterList != null)
                m_parameterDeclarations[parameterList] = new List<string>();
        }

        public override void ExitParameters(GolangParser.ParametersContext context)
        {
            GolangParser.ParameterListContext parameterList = context.parameterList();
            List<string> parameterDeclarations = null;

            if (parameterList != null)
                m_parameterDeclarations.TryGetValue(parameterList, out parameterDeclarations);

            m_parameters[context] = $"({string.Join(", ", parameterDeclarations ?? new List<string>())})";
        }

        public override void ExitParameterDecl(GolangParser.ParameterDeclContext context)
        {
            m_parameterDeclarations.TryGetValue(context.Parent, out List<string> parameterDeclarations);

            if (parameterDeclarations == null)
                throw new InvalidOperationException("Parameter declarations undefined.");

            m_identifiers.TryGetValue(context.identifierList(), out string[] identifiers);

            // Check for variadic expression
            bool hasVariadicParameter = context.GetText().Contains("...");

            m_types.TryGetValue(context.type(), out GoTypeInfo typeInfo);

            if (typeInfo == null)
                typeInfo = ObjectType;

            if (identifiers != null)
            {
                for (int i = 0; i < identifiers.Length; i++)
                {
                    string identifier = SanitizedIdentifier(identifiers[i]);

                    // Check for unnamed parameters
                    if (string.IsNullOrWhiteSpace(identifier))
                        identifier = $"_p{parameterDeclarations.Count}";

                    if (i == identifiers.Length - 1 && hasVariadicParameter)
                        parameterDeclarations.Add($"params {typeInfo.PrimitiveName}[] {identifier}");
                    else
                        parameterDeclarations.Add($"{typeInfo.PrimitiveName} {identifier}");
                }
            }
            else if (hasVariadicParameter)
            {
                // Unnamed variadic parameter
                parameterDeclarations.Add($"params {typeInfo.PrimitiveName}[] _p{parameterDeclarations.Count}");
            }
            else
            {
                // Unnamed parameter
                parameterDeclarations.Add($"{typeInfo.PrimitiveName} _p{parameterDeclarations.Count}");
            }
        }
    }
}