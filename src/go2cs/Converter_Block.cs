//******************************************************************************************************
//  Converter_Block.cs - Gbtc
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
//  07/12/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.Text;

namespace go2cs
{
    public partial class Converter
    {
        private readonly Stack<StringBuilder> m_blocks = new Stack<StringBuilder>();
        private bool m_firstStatementIsReturn;

        private void PushBlock()
        {
            m_blocks.Push(m_targetFile);
            m_targetFile = new StringBuilder();
        }

        private string PopBlock(bool appendToPrevious = true, string prefix = null, string suffix = null)
        {
            StringBuilder lastTarget = m_blocks.Pop();
            string block = m_targetFile.ToString();

            if (!string.IsNullOrEmpty(prefix))
                lastTarget.Append(prefix);

            if (appendToPrevious)
                lastTarget.Append(block);

            if (!string.IsNullOrEmpty(suffix))
                lastTarget.Append(suffix);

            m_targetFile = lastTarget;

            return block;
        }

        public override void EnterBlock(GolangParser.BlockContext context)
        {
            // block
            //     : '{' statementList '}'

            // statementList
            //     : (statement eos )*

            PushBlock();
            m_targetFile.AppendLine($"{Spacing()}{{");
            m_firstStatementIsReturn = false;

            IndentLevel++;
        }

        public override void ExitBlock(GolangParser.BlockContext context)
        {
            IndentLevel--;

            GolangParser.StatementListContext statementListContext = context.statementList();

            if (statementListContext.statement().Length > 0)
                m_firstStatementIsReturn = statementListContext.statement(0).returnStmt() != null;

            m_targetFile.Append($"{Spacing()}}}");
            PopBlock();
        }
    }
}
