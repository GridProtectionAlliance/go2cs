//******************************************************************************************************
//  StructDeclarationSyntaxExtensions.cs - Gbtc
//
//  Copyright © 2024, Grid Protection Alliance.  All Rights Reserved.
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
//  09/16/2024 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace go2cs;

public static class StructDeclarationSyntaxExtensions
{
    public static List<(string typeName, string fieldName)> GetStructFields(this StructDeclarationSyntax structDeclaration, GeneratorExecutionContext context)
    {
        // Obtain the SemanticModel from the context
        SemanticModel semanticModel = context.Compilation.GetSemanticModel(structDeclaration.SyntaxTree);

        List<(string typeName, string fieldName)> fields = [];

        foreach (FieldDeclarationSyntax? fieldDeclaration in structDeclaration.Members.OfType<FieldDeclarationSyntax>())
        {
            if (fieldDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
                continue;

            TypeSyntax variableTypeSyntax = fieldDeclaration.Declaration.Type;

            TypeInfo typeInfo = semanticModel.GetTypeInfo(variableTypeSyntax);
            ITypeSymbol? typeSymbol = typeInfo.Type;

            if (typeSymbol == null)
                continue; // Type couldn't be resolved

            string fullyQualifiedTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            foreach (VariableDeclaratorSyntax variableDeclarator in fieldDeclaration.Declaration.Variables)
            {
                string fieldName = variableDeclarator.Identifier.Text;
                fields.Add((fullyQualifiedTypeName, fieldName));
            }
        }

        return fields;
    }
}
