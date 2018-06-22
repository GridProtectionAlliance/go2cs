//******************************************************************************************************
//  ScannerBase_IdentifierList.cs - Gbtc
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

namespace go2cs
{
    public partial class ScannerBase
    {
        // Stack handlers:
        //  constDecl (required)
        //  varDecl (required)
        //  shortVarDecl (required)
        //  recvStmt (optional)
        //  rangeClause (optional)
        //  parameterDecl (optional)
        //  fieldDecl (optional)
        protected readonly ParseTreeValues<string[]> Identifiers = new ParseTreeValues<string[]>();

        public override void EnterIdentifierList(GolangParser.IdentifierListContext context)
        {
            List<string> identifers = new List<string>();

            for (int i = 0; i < context.IDENTIFIER().Length; i++)
                identifers.Add(context.IDENTIFIER(i).GetText());

            Identifiers[context] = identifers.ToArray();
        }
    }
}