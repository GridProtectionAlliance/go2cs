//******************************************************************************************************
//  Converter_Declaration.cs - Gbtc
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

using System.IO;
using Antlr4.Runtime.Misc;
using go2cs.Templates;

namespace go2cs
{
    public partial class Converter
    {
        public override void ExitTypeSpec([NotNull] GolangParser.TypeSpecContext context)
        {
            string identifier = context.IDENTIFIER().GetText();
            string scope = char.IsUpper(identifier[0]) ? "public" : "private";

            if (!m_topLevelDeclaration)
                m_targetFile.AppendLine();

            m_targetFile.AppendLine($"{Spacing()}{scope} partial struct {identifier}");
            m_targetFile.AppendLine($"{Spacing()}{{");

            if (m_structFields.TryGetValue(context.type()?.typeLit()?.structType(), out string fields))
                m_targetFile.AppendLine($"{Spacing(1)}{fields}");

            m_targetFile.AppendLine($"{Spacing()}}}");

            string ancillaryStructFileName = Path.Combine(TargetFilePath, $"{m_package}_{identifier}Struct.cs");

            using (StreamWriter writer = File.CreateText(ancillaryStructFileName))
                writer.Write(new StructTypeTemplate
                {
                    NamespaceHeader = m_namespaceHeader,
                    NamespaceFooter = m_namespaceFooter,
                    PackageName = m_package,
                    StructName = identifier,
                    Scope = scope
                }
                .TransformText());

            // Track file name associated with package
            AddFileToPackage(m_package, ancillaryStructFileName, m_packageNamespace);
        }
    }
}
