//******************************************************************************************************
//  Ref.cs - Gbtc
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
//  06/13/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable CheckNamespace
// ReSharper disable UnusedMember.Global

namespace go
{
    /// <summary>
    /// Represents a heap allocated reference to an instance of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type used to create a heap based reference.</typeparam>
    /// <remarks>
    /// Unless otherwise marked with <see langword="stackalloc"/>, a .NET class will be allocated on
    /// the heap and registered for garbage collection. The "Ref" class is used to create a reference
    /// to a heap allocated instance of type <typeparamref name="T"/> so that the type can (1) have
    /// scope beyond the current stack, and (2) have the ability to create a pointer to the type,
    /// i.e., a reference to a reference of the type. Also see <see cref="Ptr{T}"/>.
    /// </remarks>
    public class Ref<T>
    {
        private T m_value;

        public ref T Value => ref m_value;

        public Ref()
        {
        }

        public Ref(T value) => m_value = value;

        public Ref(ref T value) => m_value = value;

        public override string ToString() => m_value?.ToString() ?? "nil";
    }
}