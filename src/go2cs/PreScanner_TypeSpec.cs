//******************************************************************************************************
//  PreScanner_TypeSpec.cs - Gbtc
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
//  06/21/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using go2cs.Metadata;
using System.Collections.Generic;

namespace go2cs
{
    public partial class PreScanner
    {
        public override void ExitTypeSpec(GolangParser.TypeSpecContext context)
        {
            string identifier = context.IDENTIFIER().GetText();

            if (m_interfaceMethods.TryGetValue(context.type()?.typeLit()?.interfaceType(), out List<FunctionSignature> methods))
            {
                m_interfaces.Add(GetUniqueIdentifier(m_interfaces, identifier), new InterfaceInfo
                {
                    Name = identifier,
                    Methods = methods.ToArray()
                });
            }
            else if (m_structFields.TryGetValue(context.type()?.typeLit()?.structType(), out List<FieldInfo> fields))
            {
                m_structs.Add(GetUniqueIdentifier(m_structs, identifier), new StructInfo
                {
                    Name = identifier,
                    Fields = fields.ToArray()
                });
            }
        }
    }
}