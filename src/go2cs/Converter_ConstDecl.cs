//******************************************************************************************************
//  Converter_ConstDecl.cs - Gbtc
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

using System;
using System.Globalization;

namespace go2cs
{
    public partial class Converter
    {
        private int m_iota;

        public override void EnterConstDecl(GolangParser.ConstDeclContext context)
        {
            m_iota = 0;
        }

        public override void ExitConstSpec(GolangParser.ConstSpecContext context)
        {
            m_identifiers.TryGetValue(context.identifierList(), out string[] identifiers);

            m_expressionLists.TryGetValue(context.expressionList(), out string[] expressions);

            if (identifiers == null)
                throw new DataMisalignedException("No identifier specified in constant expression.");

            if (expressions != null && identifiers.Length != expressions.Length)
                return; // throw new DataMisalignedException("Missing value in const declaration"); 

            GolangParser.TypeContext typeContext = context.type();
            string lastType = null;

            for (int i = 0; i < identifiers.Length; i++)
            {
                string identifier = identifiers[i];
                string expression = expressions?[i] ?? $"{m_iota}";
                string type;

                if (typeContext == null)
                    type = DerviveType(expression, lastType) ?? "double";
                else
                    type = m_types[typeContext].PrimitiveName;

                lastType = type;

                if (m_inFunction)
                {
                    //if (type == "Complex")
                    //    m_targetFile.Append($"{Spacing()}Complex {identifier} = new Complex;");
                    //else
                    //    m_targetFile.Append($"{Spacing()}const {type} {identifier} = {ApplyIota(type, expression)};");
                }
                else
                {
                    string scope = char.IsUpper(identifier[0]) ? "public" : "private";

                    //if (type == "Complex")
                    //    m_targetFile.Append($"{Spacing()}{scope} readonly Complex {identifier} = new {ApplyIota("Complex", type)};");
                    //else
                    //    m_targetFile.Append($"{Spacing()}{scope} const {type} {identifier} = {ApplyIota(type, expression)};");
                }

                m_targetFile.Append(CheckForCommentsRight(context));

                if (!m_wroteCommentWithLineFeed)
                    m_targetFile.AppendLine();

                m_iota++;
            }
        }

        private string DerviveType(string expression, string lastType)
        {
            if (expression == "iota")
                return lastType ?? "int";

            // TODO: replace all of the following code with an expression parser for better constant type evaluation

            if (expression.StartsWith("\"") || expression.StartsWith("`"))
                return "string";

            if (expression.StartsWith("'"))
                return "char";

            if (expression == "true" || expression == "false")
                return "bool";

            if (expression.StartsWith("0x"))
            {
                expression = expression.Substring(2);

                if (byte.TryParse(expression, NumberStyles.HexNumber, null, out byte _))
                    return "byte";

                if (ushort.TryParse(expression, NumberStyles.HexNumber, null, out ushort _))
                    return "ushort";

                if (uint.TryParse(expression, NumberStyles.HexNumber, null, out uint _))
                    return "uint";

                return "ulong";
            }

            if (expression.Contains("i(") || expression.Contains("complex("))
            {
                m_requiredUsings.Add("System.Numerics");
                return "Complex";
            }

            if (expression.Contains("."))
            {
                if (float.TryParse(expression, out float _))
                    return "float";

                if (double.TryParse(expression, out double _))
                    return "double";
                
                return "double";
            }

            if (byte.TryParse(expression, out byte _))
                return "byte";

            if (short.TryParse(expression, out short _))
                return "short";

            if (ushort.TryParse(expression, out ushort _))
                return "ushort";

            if (int.TryParse(expression, out int _))
                return "int";

            if (uint.TryParse(expression, out uint _))
                return "uint";

            if (long.TryParse(expression, out long _))
                return "long";

            return "ulong";
        }
    }
}