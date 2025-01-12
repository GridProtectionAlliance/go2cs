//******************************************************************************************************
//  SyntaxNodeExtensions.cs - Gbtc
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

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace go2cs;

public static class SyntaxNodeExtensions
{
    public static string GetNamespaceName(this SyntaxNode syntaxNode)
    {
        BaseNamespaceDeclarationSyntax[] namespaceDeclarations = syntaxNode.Ancestors()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .ToArray();

        if (!namespaceDeclarations.Any())
            return string.Empty;

        // Build the full namespace by joining nested namespaces
        string namespaceName = string.Join(".", namespaceDeclarations
            .Select(ns => ns.Name.ToString())
            .Reverse());

        return namespaceName;
    }

    public static string GetParentClassName(this SyntaxNode syntaxNode)
    {
        ClassDeclarationSyntax? classDeclaration = syntaxNode.Ancestors()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault();

        return classDeclaration?.Identifier.Text ?? string.Empty;
    }
}
