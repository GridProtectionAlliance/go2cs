//******************************************************************************************************
//  ScannerBase_Expression.cs - Gbtc
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
    public partial class ScannerBase
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
        protected readonly ParseTreeValues<string> Expressions = new ParseTreeValues<string>();
        protected readonly ParseTreeValues<string> UnaryExpressions = new ParseTreeValues<string>();

        // TODO: Focus on getting primaryExpr working, and build out from there
        public override void ExitPrimaryExpr(GolangParser.PrimaryExprContext context)
        {
        }

        public override void ExitExpression(GolangParser.ExpressionContext context)
        {
            if (context.expression()?.Length == 2)
            {                
                string binaryOP = context.children[1].GetText();
                binaryOP = binaryOP.Equals("&^") ? " & ~" : $" {binaryOP} ";
                Expressions[context] = $"{Expressions[context.expression(0)]}{binaryOP}{Expressions[context.expression(1)]}";
            }
            else
            {
                Expressions[context] = UnaryExpressions[context.unaryExpr()];
            }
        }

        public override void ExitUnaryExpr(GolangParser.UnaryExprContext context)
        {
            if (context.unaryExpr() != null)
            {
                string unaryOP = context.children[0].GetText();

                if (unaryOP.Equals("^"))
                {
                    unaryOP = "~";
                }
                else if (unaryOP.Equals("<-"))
                {
                    // TODO: Handle channel value access (update when channel class is created):
                    unaryOP = null;
                    UnaryExpressions[context] = $"{UnaryExpressions[context.unaryExpr()]}.Receive()";
                }
                else if (unaryOP.Equals("&"))
                {
                    // TODO: Handle pointer acquisition context - may need to augment pre-scan metadata for heap allocation notation
                }
                else if (unaryOP.Equals("*"))
                {
                    // TODO: Handle pointer dereference context
                }

                if ((object)unaryOP != null)
                    UnaryExpressions[context] = $"{unaryOP}{UnaryExpressions[context.unaryExpr()]}";
            }
            else if (!UnaryExpressions.ContainsKey(context))
            {
                // TODO: This is an expression fall back until all sub-tree paths are populated as needed
                UnaryExpressions[context] = context.GetText();
            }
        }

        public override void ExitArguments(GolangParser.ArgumentsContext context)
        {
            // Unary expression path = unaryExpr->primaryExpr->arguments:
            if (!(context.Parent.Parent is GolangParser.UnaryExprContext unaryExprContext))
            {
                AddWarning(context, $"Could not derive parent unary expression context from arguments: \"{context.GetText()}\"");
                return;
            }
        }

        public override void ExitBasicLit(GolangParser.BasicLitContext context)
        {
            // Unary expression path = unaryExpr->primaryExpr->operand->literal->basicLit:
            if (!(context.Parent.Parent.Parent.Parent is GolangParser.UnaryExprContext unaryExprContext))
            {
                AddWarning(context, $"Could not derive parent unary expression context from basic literal: \"{context.GetText()}\"");
                return;
            }

            string expressionValue;

            //basicLit
            //    : INT_LIT
            //    | FLOAT_LIT
            //    | IMAGINARY_LIT
            //    | RUNE_LIT
            //    | STRING_LIT
            //    ;

            if (context.IMAGINARY_LIT() != null)
            {
                string value = context.IMAGINARY_LIT().GetText();
                expressionValue = value.EndsWith("i") ? $"i({value.Substring(0, value.Length - 1)})" : value;
            }
            else if (context.RUNE_LIT() != null)
            {
                expressionValue = ReplaceOctalBytes(context.RUNE_LIT().GetText());
            }
            else if (context.STRING_LIT() != null)
            {
                expressionValue = ToStringLiteral(ReplaceOctalBytes(context.STRING_LIT().GetText()));
            }
            else
            {
                expressionValue = context.GetText();
            }

            UnaryExpressions[unaryExprContext] = expressionValue;
        }
    }
}