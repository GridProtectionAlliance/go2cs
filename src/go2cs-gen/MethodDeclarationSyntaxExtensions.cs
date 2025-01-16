//******************************************************************************************************
//  MethodDeclarationSyntaxExtensions.cs - Gbtc
//
//  Copyright © 2025, Grid Protection Alliance.  All Rights Reserved.
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
//  01/15/2025 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace go2cs;

public static class MethodSyntaxExtensions
{
    public static (List<(string typeName, string paramName)> paramInfo, bool refRecv) GetMethodParameters(this MethodDeclarationSyntax methodDeclaration, GeneratorExecutionContext context)
    {
        // Obtain the SemanticModel from the context
        SemanticModel semanticModel = context.Compilation.GetSemanticModel(methodDeclaration.SyntaxTree);
        bool refRecv = false;

        return (methodDeclaration.ParameterList.Parameters.Select(getParameterInfo).ToList(), refRecv);

        (string, string) getParameterInfo(ParameterSyntax param, int index)
        {
            if (param.Type is null)
                return ("object", param.Identifier.Text);

            TypeInfo typeInfo = ModelExtensions.GetTypeInfo(semanticModel, param.Type);
            ITypeSymbol? typeSymbol = typeInfo.Type;
            string fullyQualifiedTypeName = typeSymbol?.ToDisplayString() ?? "object";

            if (index == 0 && param.Modifiers.Any(SyntaxKind.ThisKeyword) && param.Modifiers.Any(m => m.IsKind(SyntaxKind.RefKeyword)))
                refRecv = true;

            return (fullyQualifiedTypeName, param.Identifier.Text);
        }
    }

    public static string GetReturnType(this MethodDeclarationSyntax methodDeclaration, GeneratorExecutionContext context)
    {
        SemanticModel semanticModel = context.Compilation.GetSemanticModel(methodDeclaration.SyntaxTree);

        TypeInfo typeInfo = semanticModel.GetTypeInfo(methodDeclaration.ReturnType);
        ITypeSymbol? typeSymbol = typeInfo.Type;

        return typeSymbol?.ToDisplayString() ?? "object";
    }
}
