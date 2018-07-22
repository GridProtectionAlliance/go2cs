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
using static go2cs.Common;

namespace go2cs
{
    public partial class Converter
    {
        public const string FunctionResultTypeMarker = ">>MARKER:FUNCTION_{0}_RESULTTYPE<<";
        public const string FunctionParametersMarker = ">>MARKER:FUNCTION_{0}_PARAMETERS<<";
        public const string FunctionExecContextMarker = ">>MARKER:FUNCTION_{0}_EXEC_CONTEXT<<";

        private bool m_inFunction;
        private FunctionInfo m_currentFunction;
        private string m_originalFunctionName;
        private string m_currentFunctionName;
        private string m_functionResultTypeMarker;
        private string m_functionParametersMarker;
        private string m_functionExecContextMarker;

        public override void EnterFunctionDecl(GolangParser.FunctionDeclContext context)
        {
            m_inFunction = true; // May need to scope certain objects, like consts, to current function
            m_originalFunctionName = context.IDENTIFIER().GetText();
            m_currentFunctionName = SanitizedIdentifier(m_originalFunctionName);

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

            m_targetFile.AppendLine($"{Spacing()}{scope} static {m_functionResultTypeMarker} {m_currentFunctionName}{m_functionParametersMarker}{m_functionExecContextMarker}");
        }

        public override void ExitFunctionDecl(GolangParser.FunctionDeclContext context)
        {
            bool signatureOnly = false;

            if (Parameters.TryGetValue(context.signature()?.parameters(), out List<ParameterInfo> parameters) && (object)parameters != null)
                signatureOnly = true;
            else if (!Parameters.TryGetValue(context.function()?.signature()?.parameters(), out parameters) || (object)parameters == null)
                parameters = new List<ParameterInfo>();

            string functionSignature = FunctionSignature.Generate(m_originalFunctionName, parameters);

            if (!Metadata.Functions.TryGetValue(functionSignature, out m_currentFunction))
                throw new InvalidOperationException($"Failed to find metadata for method function \"{functionSignature}\".");

            FunctionSignature function = m_currentFunction.Signature;

            if ((object)function == null)
                throw new InvalidOperationException($"Failed to find signature metadata for function \"{m_currentFunctionName}\".");

            bool hasDefer = m_currentFunction.HasDefer;
            bool hasPanic = m_currentFunction.HasPanic;
            bool hasRecover = m_currentFunction.HasRecover;
            bool useFuncExecutionContext = hasDefer || hasPanic || hasRecover;
            Signature signature = function.Signature;
            string parametersSignature = $"({signature.GenerateParametersSignature(useFuncExecutionContext)})";
            string resultSignature = signature.GenerateResultSignature();

            // Replace function markers
            m_targetFile.Replace(m_functionResultTypeMarker, resultSignature);
            m_targetFile.Replace(m_functionParametersMarker, parametersSignature);

            if (useFuncExecutionContext)
            {
                string[] funcExecContextByRefParams = signature.GetByRefParameters(false).ToArray();

                if (funcExecContextByRefParams.Length > 0)
                {
                    string[] lambdaByRefParameters = signature.GetByRefParameters(true).ToArray();

                    m_targetFile.Replace(m_functionExecContextMarker, $" => func({string.Join(", ", funcExecContextByRefParams)}, ({string.Join(", ", lambdaByRefParameters)}, Defer {(hasDefer ? "defer" : "_")}, Panic {(hasPanic ? "panic" : "_")}, Recover {(hasRecover ? "recover" : "_")}) =>");
                }
                else
                {
                    m_targetFile.Replace(m_functionExecContextMarker, $" => func(({(hasDefer ? "defer" : "_")}, {(hasPanic ? "panic" : "_")}, {(hasRecover ? "recover" : "_")}) =>");
                }
            }
            else
            {
                m_targetFile.Replace(m_functionExecContextMarker, "");
            }

            m_currentFunction = null;
            m_currentFunctionName = null;
            m_originalFunctionName = null;
            m_inFunction = false;

            if (useFuncExecutionContext)
                m_targetFile.Append(");");
            else if (signatureOnly)
                m_targetFile.Append(";");

            m_targetFile.Append(CheckForBodyCommentsRight(context));
        }

        //public override void ExitFunction(GolangParser.FunctionContext context)
        //{
        //    string tempBlock = RemoveSurrounding(context.block().GetText(), "{", "}");

        //    m_targetFile.AppendLine(FixForwardSpacing(tempBlock));
        //}
    }
}
