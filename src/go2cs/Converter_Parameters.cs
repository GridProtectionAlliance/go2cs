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
        private readonly ParseTreeValues<(List<string> parameters, List<string> byRefParams)> m_parameterDeclarations = new ParseTreeValues<(List<string>, List<string>)>();

        public override void EnterParameters(GolangParser.ParametersContext context)
        {
            GolangParser.ParameterListContext parameterList = context.parameterList();

            if (parameterList != null)
                m_parameterDeclarations[parameterList] = (new List<string>(), new List<string>());
        }

        public override void ExitParameters(GolangParser.ParametersContext context)
        {
            GolangParser.ParameterListContext parameterList = context.parameterList();
            (List<string> parameters, List<string> byRefParams) parameterDeclarations = default;

            if (parameterList != null)
                m_parameterDeclarations.TryGetValue(parameterList, out parameterDeclarations);

            m_parameters[context] = $"({string.Join(", ", parameterDeclarations.parameters ?? new List<string>())})";
        }

        public override void ExitParameterDecl(GolangParser.ParameterDeclContext context)
        {
            if (!m_parameterDeclarations.TryGetValue(context.Parent, out (List<string> parameters, List<string> byRefParams) parameterDeclarations))
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
                        identifier = $"_p{parameterDeclarations.parameters.Count}";

                    if (typeInfo.IsPointer)
                    {
                        // Prefix pointers with underscore for unique parameter name propagation
                        // into function context wrapper that will contain ref parameters
                        string byRefIdentifier = $"_{identifier}";

                        if (i == identifiers.Length - 1 && hasVariadicParameter)
                        {
                            parameterDeclarations.parameters.Add($"params {typeInfo.PrimitiveName}[] {byRefIdentifier}");
                            parameterDeclarations.byRefParams.Add($"params {typeInfo.PrimitiveName}[] {identifier}");
                        }
                        else
                        {
                            parameterDeclarations.parameters.Add($"{typeInfo.PrimitiveName} {byRefIdentifier}");
                            parameterDeclarations.byRefParams.Add($"{typeInfo.PrimitiveName} {identifier}");
                        }
                    }
                    else
                    {
                        if (i == identifiers.Length - 1 && hasVariadicParameter)
                            parameterDeclarations.parameters.Add($"params {typeInfo.PrimitiveName}[] {identifier}");
                        else
                            parameterDeclarations.parameters.Add($"{typeInfo.PrimitiveName} {identifier}");
                    }
                }
            }
            else if (hasVariadicParameter)
            {
                string identifier = $"_p{parameterDeclarations.parameters.Count}";

                // Unnamed variadic parameter
                if (typeInfo.IsPointer)
                {
                    // Prefix pointers with underscore for unique parameter name propagation
                    // into function context wrapper that will contain ref parameters
                    string byRefIdentifier = $"_{identifier}";

                    parameterDeclarations.parameters.Add($"params {typeInfo.PrimitiveName}[] {byRefIdentifier}");
                    parameterDeclarations.byRefParams.Add($"params {typeInfo.PrimitiveName}[] {identifier}");
                }
                else
                {
                    parameterDeclarations.parameters.Add($"params {typeInfo.PrimitiveName}[] {identifier}");
                }
            }
            else
            {
                string identifier = $"_p{parameterDeclarations.parameters.Count}";

                // Unnamed parameter
                if (typeInfo.IsPointer)
                {
                    // Prefix pointers with underscore for unique parameter name propagation
                    // into function context wrapper that will contain ref parameters
                    string byRefIdentifier = $"_{identifier}";

                    parameterDeclarations.parameters.Add($"{typeInfo.PrimitiveName} {byRefIdentifier}");
                    parameterDeclarations.byRefParams.Add($"{typeInfo.PrimitiveName} {identifier}");
                }
                else
                {
                    parameterDeclarations.parameters.Add($"{typeInfo.PrimitiveName} {identifier}");
                }
            }
        }

        private void ExtractParameterLists(string parameters, out string namedParameters, out string parameterTypes)
        {
            if (string.IsNullOrWhiteSpace(parameters))
            {
                namedParameters = "";
                parameterTypes = "";
                return;
            }
                
            List<string> names = new List<string>();
            List<string> types = new List<string>();

            foreach (string declaration in parameters.Split(','))
            {
                string[] parts = declaration.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 2)
                {
                    types.Add(parts[0].Trim());
                    names.Add(parts[1].Trim());
                }
            }

            namedParameters = $", {string.Join(", ", names)}";
            parameterTypes = $", {string.Join(", ", types)}";
        }
    }
}