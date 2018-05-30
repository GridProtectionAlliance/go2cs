//******************************************************************************************************
//  Converter_Expression.cs - Gbtc
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

using System.Diagnostics;
using static go2cs.Common;

namespace go2cs
{   
    public partial class Converter
    {
        // Stack handlers:
        //  expressionList (required)
        //  expressionStmt (required)
        //  sendStmt (required)
        //  incDecStmt (required)
        //  defer (required)
        //  ifStmt (required)
        //  exprSwitchStmt (optional)
        //  recvStmt (required)
        //  forClause (optional)
        //  rangeClause (required)
        //  goStmt (required)
        //  arrayLength (required)
        //  operand (optional)
        //  key (optional)
        //  element (optional)
        //  index (required)
        //  slice (optional)
        //  expression (optional)
        //  conversion (required)
        private readonly ParseTreeValues<string> m_expressions = new ParseTreeValues<string>();

        public override void EnterExpression(GolangParser.ExpressionContext context)
        {
            m_expressions[context] = ToStringLiteral(context.GetText().Replace("&^", "& ~"));
        }

        public override void ExitExpression(GolangParser.ExpressionContext context)
        {
            if (context.expression()?.Length == 2)
            {
                Debug.WriteLine(context.GetText());
                //Debug.WriteLine(context.GetTokens(1));
                //Debug.WriteLine(context.GetTokens(2));
            }
        }

        public override void ExitPrimaryExpr(GolangParser.PrimaryExprContext context)
        {
        }

        public override void ExitBasicLit(GolangParser.BasicLitContext context)
        {
            if (context.IMAGINARY_LIT() != null)
            {
            }
        }
    }
}