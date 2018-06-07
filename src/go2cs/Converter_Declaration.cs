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

using System.Collections.Generic;
using System.IO;
using go2cs.Templates;
using static go2cs.Common;

namespace go2cs
{
    public partial class Converter
    {
        public override void ExitTypeSpec(GolangParser.TypeSpecContext context)
        {
            string identifier = context.IDENTIFIER().GetText();
            string scope = char.IsUpper(identifier[0]) ? "public" : "private";
            string target = Path.GetFileNameWithoutExtension(TargetFileName);

            identifier = SanitizedIdentifier(identifier);

            if (!m_firstTopLevelDeclaration)
                m_targetFile.AppendLine();

            if (m_interfaceMethods.TryGetValue(context.type()?.typeLit()?.interfaceType(), out List<(string functionName, string parameterSignature, string namedParameters, string parameterTypes, string resultType)> functions))
            {
                string ancillaryInterfaceFileName = Path.Combine(TargetFilePath, $"{target}_{identifier}Interface.cs");

                using (StreamWriter writer = File.CreateText(ancillaryInterfaceFileName))
                    writer.Write(new InterfaceTypeTemplate
                    {
                        NamespacePrefix = m_packageNamespace,
                        NamespaceHeader = m_namespaceHeader,
                        NamespaceFooter = m_namespaceFooter,
                        PackageName = m_package,
                        InterfaceName = identifier,
                        Scope = scope,
                        Functions = functions.ToArray()
                    }
                    .TransformText());

                // Track file name associated with package
                AddFileToPackage(m_package, ancillaryInterfaceFileName, m_packageNamespace);

                m_targetFile.AppendLine($"{Spacing()}{scope} partial interface {identifier}"); // TODO: << add <#=InheritedInterfaces#>
                m_targetFile.AppendLine($"{Spacing()}{{");

                foreach (var function in functions)
                {
                    m_targetFile.AppendLine($"{Spacing(1)}public {function.resultType} {function.functionName}({function.parameterSignature});");
                }

                m_targetFile.AppendLine($"{Spacing()}}}");
            }
            else if (m_structFields.TryGetValue(context.type()?.typeLit()?.structType(), out string fields))
            {
                // Handle struct type declaration
                string ancillaryStructFileName = Path.Combine(TargetFilePath, $"{target}_{identifier}Struct.cs");

                using (StreamWriter writer = File.CreateText(ancillaryStructFileName))
                    writer.Write(new StructTypeTemplate
                    {
                        NamespacePrefix = m_packageNamespace,
                        NamespaceHeader = m_namespaceHeader,
                        NamespaceFooter = m_namespaceFooter,
                        PackageName = m_package,
                        StructName = identifier,
                        Scope = scope,
                        InheritedInterfaces = new[] { "" }, // TODO <<
                        PromotedStructs = new[] { "" }     // TODO <<
                    }
                    .TransformText());

                // Track file name associated with package
                AddFileToPackage(m_package, ancillaryStructFileName, m_packageNamespace);

                m_targetFile.AppendLine($"{Spacing()}{scope} partial struct {identifier}");
                m_targetFile.AppendLine($"{Spacing()}{{");
                m_targetFile.AppendLine(fields);
                m_targetFile.AppendLine($"{Spacing()}}}");
            }
            else if (m_types.TryGetValue(context.type(), out GoTypeInfo typeInfo))
            {
                // Handle declaration like "type MyFloat float64"
                string ancillaryInheritedTypeFileName = Path.Combine(TargetFilePath, $"{target}_{identifier}StructOf({RemoveInvalidCharacters(typeInfo.PrimitiveName)}).cs");

                // TODO: The following works well for a primitive type definition, but new templates will be needed for other inherited types, e.g., Map / Pointer / Array etc.
                using (StreamWriter writer = File.CreateText(ancillaryInheritedTypeFileName))
                    writer.Write(new InheritedTypeTemplate
                    {
                        NamespacePrefix = m_packageNamespace,
                        NamespaceHeader = m_namespaceHeader,
                        NamespaceFooter = m_namespaceFooter,
                        PackageName = m_package,
                        StructName = identifier,
                        Scope = scope,
                        TypeName = typeInfo.PrimitiveName
                    }
                    .TransformText());

                // Track file name associated with package
                AddFileToPackage(m_package, ancillaryInheritedTypeFileName, m_packageNamespace);

                m_targetFile.AppendLine($"{Spacing()}{scope} partial struct {identifier} // {typeInfo.PrimitiveName}");
                m_targetFile.AppendLine($"{Spacing()}{{");
                m_targetFile.AppendLine($"{Spacing()}}}");
            }
        }
    }
}
