//******************************************************************************************************
//  Converter_FunctionDecl.cs - Gbtc
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static go2cs.Common;

namespace go2cs
{
    public partial class Converter
    {
        public const string FunctionResultTypeMarker = ">>MARKER:FUNCTION_{0}_RESULTTYPE<<";
        public const string FunctionParametersMarker = ">>MARKER:FUNCTION_{0}_PARAMETERS<<";
        public const string FunctionExecContextMarker = ">>MARKER:FUNCTION_{0}_EXEC_CONTEXT<<";
        public const string FunctionBlockPrefixMarker = ">>MARKER:FUNCTION_{0}_BLOCK_PREFIX<<";

        private bool m_inFunction;
        private FunctionInfo m_currentFunction;
        private string m_originalFunctionName;
        private string m_currentFunctionName;
        private string m_functionResultTypeMarker;
        private string m_functionParametersMarker;
        private string m_functionExecContextMarker;

        public override void ExitUnaryExpr(GoParser.UnaryExprContext context)
        {
            base.ExitUnaryExpr(context);

            if (!m_inFunction || !(context.primaryExpr() is null) && PrimaryExpressions.ContainsKey(context.primaryExpr()) 
                || context.expression() is null || !Expressions.TryGetValue(context.expression(), out ExpressionInfo expression))
                return;

            ParameterInfo[] parameters = m_currentFunction.Signature.Signature.Parameters;
            string unaryOP = context.children[0].GetText();

            if (!unaryOP.Equals("*", StringComparison.Ordinal))
                return;

            ParameterInfo pointerParam = parameters.FirstOrDefault(parameter => parameter.Name.Equals(expression.Text));
                        
            if (!(pointerParam is null) && pointerParam.Type is PointerTypeInfo pointer && pointer.TargetTypeInfo.TypeClass == TypeClass.Array)
            {
                // Dereference array type parameters
                UnaryExpressions[context] = new ExpressionInfo
                {
                    Text = expression.Text,
                    Type = pointer.TargetTypeInfo
                };
            }
        }

        public override void EnterFunctionDecl(GoParser.FunctionDeclContext context)
        {
            m_inFunction = true; // May need to scope certain objects, like consts, to current function
            m_originalFunctionName = context.IDENTIFIER()?.GetText() ?? "_";
            m_currentFunctionName = SanitizedIdentifier(m_originalFunctionName);
            m_variableIdentifiers.Clear();
            m_variableTypes.Clear();

            string functionSignature = FunctionSignature.Generate(m_originalFunctionName);

            if (!Metadata.Functions.TryGetValue(functionSignature, out m_currentFunction))
                throw new InvalidOperationException($"Failed to find metadata for method function \"{functionSignature}\".");

            FunctionSignature function = m_currentFunction.Signature;

            if (function is null)
                throw new InvalidOperationException($"Failed to find signature metadata for function \"{m_currentFunctionName}\".");

            string scope = char.IsUpper(m_originalFunctionName[0]) ? "public" : "private";

            // Handle Go "main" function as a special case, in C# this should be capitalized "Main"
            if (m_currentFunctionName.Equals("main"))
            {
                m_currentFunctionName = "Main";
                
                // Track file names that contain main function in main package
                if (Package.Equals("main"))
                    s_mainPackageFiles.Add(TargetFileName);
            }

            // Function signature containing result type and parameters have not been visited yet,
            // so we mark their desired positions and replace once the visit has occurred
            m_functionResultTypeMarker = string.Format(FunctionResultTypeMarker, m_currentFunctionName);
            m_functionParametersMarker = string.Format(FunctionParametersMarker, m_currentFunctionName);
            m_functionExecContextMarker = string.Format(FunctionExecContextMarker, m_currentFunctionName);
            PushInnerBlockPrefix(string.Format(FunctionBlockPrefixMarker, m_currentFunctionName));

            m_targetFile.AppendLine($"{Spacing()}{scope} static {m_functionResultTypeMarker} {m_currentFunctionName}{m_functionParametersMarker}{m_functionExecContextMarker}");
        }

        public override void ExitFunctionDecl(GoParser.FunctionDeclContext context)
        {
            bool signatureOnly = context.block() is null;

            FunctionSignature function = m_currentFunction.Signature;
            bool hasDefer = m_currentFunction.HasDefer;
            bool hasPanic = m_currentFunction.HasPanic;
            bool hasRecover = m_currentFunction.HasRecover;
            bool useFuncExecutionContext = hasDefer || hasPanic || hasRecover;
            Signature signature = function.Signature;
            string parametersSignature = $"({signature.GenerateParametersSignature()})";
            string resultSignature = signature.GenerateResultSignature();
            string blockPrefix = "";

            if (!signatureOnly)
            {
                // TODO: Double check if any other types need clone-type copy operations
                // For any array parameters, Go copies the array by value
                StringBuilder arrayClones = new StringBuilder();
                StringBuilder arrayRefs = new StringBuilder();

                foreach (ParameterInfo parameter in signature.Parameters)
                {
                    if (parameter.Type.TypeClass == TypeClass.Array)
                        arrayClones.AppendLine($"{Spacing(1)}{parameter.Name} = {parameter.Name}.Clone();");

                    if (parameter.Type is PointerTypeInfo pointer && pointer.TargetTypeInfo.TypeClass == TypeClass.Array)
                        arrayRefs.AppendLine($"{Spacing(1)}ref {pointer.TargetTypeInfo.TypeName} {parameter.Name} = ref _addr_{parameter.Name}.val;");
                }

                if (arrayClones.Length > 0)
                {
                    arrayClones.Insert(0, Environment.NewLine);
                    blockPrefix += arrayClones.ToString();
                }

                if (arrayRefs.Length > 0)
                {
                    arrayRefs.Insert(0, Environment.NewLine);
                    blockPrefix += arrayRefs.ToString();

                    StringBuilder updatedSignature = new StringBuilder();
                    bool initialParam = true;

                    foreach (ParameterInfo parameter in signature.Parameters)
                    {
                        if (!initialParam)
                            updatedSignature.Append(", ");

                        initialParam = false;
                        updatedSignature.Append($"{(parameter.IsVariadic ? "params " : "")}{parameter.Type.TypeName} ");

                        if (parameter.Type is PointerTypeInfo pointer && pointer.TargetTypeInfo.TypeClass == TypeClass.Array)
                            updatedSignature.Append("_addr_");

                        updatedSignature.Append(parameter.Name);
                    }

                    parametersSignature = $"({updatedSignature})";
                }
            }

            // Replace function markers
            m_targetFile.Replace(m_functionResultTypeMarker, resultSignature);
            m_targetFile.Replace(m_functionParametersMarker, parametersSignature);

            if (useFuncExecutionContext)
            {
                Stack<string> unusedNames = new Stack<string>(new[] { "__", "_" });
                m_targetFile.Replace(m_functionExecContextMarker, $" => func(({(hasDefer ? "defer" : unusedNames.Pop())}, {(hasPanic ? "panic" : unusedNames.Pop())}, {(hasRecover ? "recover" : unusedNames.Pop())}) =>");
            }
            else
            {
                m_targetFile.Replace(m_functionExecContextMarker, "");
            }

            m_targetFile.Replace(string.Format(FunctionBlockPrefixMarker, m_currentFunctionName), blockPrefix);

            m_currentFunction = null;
            m_currentFunctionName = null;
            m_originalFunctionName = null;
            m_inFunction = false;

            if (useFuncExecutionContext)
                m_targetFile.Append(");");
            else if (signatureOnly)
                m_targetFile.Append(";");

            m_targetFile.Append(CheckForCommentsRight(context));
        }
    }
}
