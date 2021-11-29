//******************************************************************************************************
//  Converter_Statement.cs - Gbtc
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
//  05/04/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using go2cs.Metadata;
using System;
using System.Text;
using static go2cs.Common;

namespace go2cs
{
    public partial class Converter
    {
        private bool TryGetFunctionVariable(string identifier, out VariableInfo variable)
        {
            variable = default;
            return InFunction && (CurrentFunction?.Variables?.TryGetValue(identifier, out variable) ?? false) && variable is not null;
        }

        private string OpenRedeclaredVariableBlock(GoParser.IdentifierListContext identifierList, int level)
        {
            if (identifierList is null)
                return "";

            if (!Identifiers.TryGetValue(identifierList, out string[] identifiers))
            {
                // Pre-visit identifiers if they are not defined yet
                EnterIdentifierList(identifierList);
                
                if (!Identifiers.TryGetValue(identifierList, out identifiers))
                {
                    AddWarning(identifierList, $"Failed to find identifier lists needed to hold and restore values for short var declaration statements: {identifierList.GetText()}");
                    return "";
                }
            }

            StringBuilder block = new StringBuilder();
            bool addNewLine = false;

            foreach (string identifier in identifiers)
            {
                if (TryGetFunctionVariable(identifier, out VariableInfo variable) && variable.Redeclared)
                {
                    addNewLine = true;
                    block.AppendLine($"{Spacing()}{variable.Type.TypeName ?? "var"} {identifier}__prev{level} = {identifier};");
                }
            }

            if (addNewLine)
                block.AppendLine();

            return block.ToString();
        }

        private string CloseRedeclaredVariableBlock(GoParser.IdentifierListContext identifierList, int level)
        {
            if (identifierList is null)
                return "";

            StringBuilder block = new StringBuilder();

            if (Identifiers.TryGetValue(identifierList, out string[] identifiers))
            {
                bool addNewLine = false;

                foreach (string identifier in identifiers)
                {
                    if (TryGetFunctionVariable(identifier, out VariableInfo variable) && variable.Redeclared)
                    {
                        addNewLine = true;
                        block.AppendLine($"{Spacing()}{identifier} = {identifier}__prev{level};");
                    }
                }

                if (addNewLine)
                    block.Insert(0, $"{Environment.NewLine}");
            }
            else
            {
                AddWarning(identifierList, $"Failed to find identifier lists needed to stack creation for short var declaration statement: {identifierList.GetText()}");
            }

            return block.ToString();
        }

        /// <remarks>
        /// See related operations:
        /// <see cref="ExitVarSpec(GoParser.VarSpecContext)"/>
        /// <see cref="ExitAssignment(GoParser.AssignmentContext)"/>
        /// </remarks>
        public override void ExitShortVarDecl(GoParser.ShortVarDeclContext context)
        {
            // shortVarDecl
            //     : identifierList ':=' expressionList

            GoParser.IdentifierListContext identifierList = context.identifierList();

            if (Identifiers.TryGetValue(identifierList, out string[] identifiers) && ExpressionLists.TryGetValue(context.expressionList(), out ExpressionInfo[] expressions))
            {
                StringBuilder statement = new StringBuilder();

                if (identifiers.Length != expressions.Length)
                {
                    if (identifiers.Length > expressions.Length && expressions.Length == 1)
                        identifiers = new[] { $"({string.Join(", ", identifiers)})" };
                    else
                        AddWarning(context, $"Encountered count mismatch for identifiers and expressions in short var declaration statement: {context.GetText()}");
                }

                for (int i = 0; i < identifiers.Length; i++)
                {
                    if (expressions.Length > i)
                        m_variableIdentifiers.Add(identifierList.IDENTIFIER(i), GetUniqueIdentifier(m_variableIdentifiers, identifiers[i]));
                }

                int length = Math.Min(identifiers.Length, expressions.Length);

                for (int i = 0; i < length; i++)
                {
                    string identifier = SanitizedIdentifier(identifiers[i]);
                    string expression = expressions[i].Text;
                    TypeInfo typeInfo = expressions[i].Type;
                    bool isPointer = typeInfo is PointerTypeInfo;
                    string typeName = typeInfo.TypeName ?? "var";
                    bool isInitialDeclaration = true;
                    VariableInfo variable = null;
                    bool heapAllocated = false;
                    string variableName = null;

                    statement.Append($"{Spacing()}");

                    // Determine if this is the initial declaration
                    if (InFunction && m_variableIdentifiers.TryGetValue(identifierList.IDENTIFIER(i), out variableName))
                        isInitialDeclaration = !variableName.Contains("@@");

                    if (isInitialDeclaration && !string.IsNullOrWhiteSpace(variableName))
                    {
                        m_variableTypes[variableName] = expressions[i].Type;
                        CurrentFunction.Variables.TryGetValue(variableName, out variable);
                    }

                    if (isInitialDeclaration)
                    {
                        if ((variable?.HeapAllocated ?? false) && !isPointer)
                        {
                            heapAllocated = true;
                            statement.Append($"ref {typeName} {identifier} = ref heap(");
                        }
                        else
                        {
                            statement.Append($"{typeName} {identifier} = ");
                        }
                    }
                    else
                    {
                        statement.Append($"{identifier} = ");
                    }

                    if (expression is not null)
                    {
                        if (typeInfo?.TypeClass == TypeClass.Interface)
                            statement.Append($"{typeInfo.TypeName}.As({expression})!");
                        else
                            statement.Append(expression);
                    }
                    else if (typeInfo?.TypeClass == TypeClass.Array && typeInfo is ArrayTypeInfo arrayTypeInfo)
                    {
                        statement.Append($"new {typeName}({arrayTypeInfo.Length.Text})");
                    }
                    else
                    {
                        AddWarning(context, $"Encountered short var declaration with no target expression: {context.GetText()}");
                        statement.Append("default");
                    }

                    if (heapAllocated)
                        statement.Append($", out ptr<{typeName}> {AddressPrefix}{identifier})");

                    statement.Append(";");

                    // Since multiple declarations can be on one line, only check for comments after last declaration
                    if (i < length - 1)
                    {
                        statement.AppendLine();
                    }
                    else
                    {
                        statement.Append(CheckForCommentsRight(context));

                        if (!WroteLineFeed)
                            statement.AppendLine();
                    }
                }

                m_simpleStatements[context.Parent] = statement.ToString();
            }
            else
            {
                AddWarning(context, $"Failed to find both identifier and expression lists for short var declaration statement: {context.GetText()}");
            }
        }
    }
}
