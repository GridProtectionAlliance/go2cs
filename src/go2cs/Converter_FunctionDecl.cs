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

using static go2cs.Common;

namespace go2cs
{
    public partial class Converter
    {
        public const string FunctionResultTypeMarker = ">>MARKER:FUNCTION_{0}_RESULTTYPE<<";
        public const string FunctionParametersMarker = ">>MARKER:FUNCTION_{0}_PARAMETERS<<";

        private bool m_inFunction;
        private string m_currentFunction;
        private string m_functionResultTypeMarker;
        private string m_functionParametersMarker;

        public override void EnterFunctionDecl(GolangParser.FunctionDeclContext context)
        {
            m_inFunction = true; // May need to scope certain objects, like consts, to current function
            m_currentFunction = context.IDENTIFIER().GetText();

            string scope = char.IsUpper(m_currentFunction[0]) ? "public" : "private";

            m_currentFunction = SanitizedIdentifier(m_currentFunction);

            // Handle Go "main" function as a special case, in C# this should be "Main"
            if (m_currentFunction.Equals("main"))
            {
                m_currentFunction = "Main";
                
                // Track file names that contain main function in main package
                if (m_package.Equals("main"))
                    s_mainPackageFiles.Add(TargetFileName);
            }

            // Function signature containing result type and parameters have not been visited yet,
            // so we mark their desired positions and replace once the visit has occurred
            m_functionResultTypeMarker = string.Format(FunctionResultTypeMarker, m_currentFunction);
            m_functionParametersMarker = string.Format(FunctionParametersMarker, m_currentFunction);

            if (!m_firstTopLevelDeclaration)
                m_targetFile.AppendLine();

            if (!string.IsNullOrWhiteSpace(m_nextDeclComments))
                m_targetFile.Append(FixForwardSpacing(m_nextDeclComments));

            m_targetFile.AppendLine($"{Spacing()}{scope} static {m_functionResultTypeMarker} {m_currentFunction}{m_functionParametersMarker} => func((defer, panic, recover) =>");
            m_targetFile.AppendLine($"{Spacing()}{{");

            m_indentLevel++;
        }

        public override void ExitFunctionDecl(GolangParser.FunctionDeclContext context)
        {
            m_indentLevel--;

            if (!m_signatures.TryGetValue(context.signature(), out (string parameters, string result) signature))
                m_signatures.TryGetValue(context.function().signature(), out signature);

            // Replace function markers
            m_targetFile.Replace(m_functionResultTypeMarker, signature.result);
            m_targetFile.Replace(m_functionParametersMarker, signature.parameters);

            m_currentFunction = null;
            m_inFunction = false;

            m_targetFile.AppendLine($"{Spacing()}}});");
        }

        public override void ExitFunction(GolangParser.FunctionContext context)
        {
            string tempBlock = RemoveSurrounding(context.block().GetText(), "{", "}");

            m_targetFile.AppendLine(FixForwardSpacing(tempBlock));
        }
    }
}
