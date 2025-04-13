//******************************************************************************************************
//  GeneratorExecutionContextExtensions.cs - Gbtc
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
//  01/27/2025 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static go2cs.Common;

namespace go2cs;

public static class GeneratorExecutionContextExtensions
{
    public static StructDeclarationSyntax? GetStructDeclaration(this GeneratorExecutionContext context, string structTypeName)
    {
        // If struct type is a pointer, i.e., ж<T>, then get the underlying type
        int startIndex = structTypeName.IndexOf(PointerPrefix, StringComparison.Ordinal);

        if (startIndex > -1 && structTypeName.EndsWith(">"))
            structTypeName = structTypeName[(startIndex + 2)..^1];

        return context
            .Compilation
            .SyntaxTrees
            .SelectMany(tree => tree.GetRoot()
                .DescendantNodes()
                .OfType<StructDeclarationSyntax>())
            .FirstOrDefault(structDecl =>
            {
                ISymbol? symbol = context
                    .Compilation
                    .GetSemanticModel(structDecl.SyntaxTree)
                    .GetDeclaredSymbol(structDecl);

                string symbolName = symbol?.ToDisplayString() ?? "";

                if (!symbolName.StartsWith("global::") && structTypeName.StartsWith("global::"))
                    symbolName = "global::" + symbolName;

                return symbolName == structTypeName;
            });
    }
}
