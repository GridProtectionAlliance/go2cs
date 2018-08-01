//******************************************************************************************************
//  Ptr.cs - Gbtc
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
    /// Represents a pointer to a heap allocated reference of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type used to create a reference to a heap based reference.</typeparam>
    /// <remarks>
    /// Unless otherwise marked with <see langword="stackalloc"/>, a .NET class will be allocated on
    /// the heap and registered for garbage collection. The "Ref" class is used to create a reference
    /// to a heap allocated instance of type <typeparamref name="T"/> so that the type can (1) have
    /// scope beyond the current stack, and (2) have the ability to create a pointer to the type,
    /// i.e., a reference to a reference of the type. Also see <see cref="Ref{T}"/>.
    /// </remarks>
    public class Ptr<T> : Ref<Ref<T>>
    {
        public ref T Deref => ref Value.Value;

        public Ptr()
        {
        }

        public Ptr(T value) => Value = new Ref<T>(value);

        public Ptr(ref T value) => Value = new Ref<T>(ref value);

        public Ptr(Ref<T> value) => Value = value;

        public Ptr(ref Ref<T> value) => Value = value;

        public Ptr(Ptr<T> value) => Value = value.Value;

        public Ptr(ref Ptr<T> value) => Value = value.Value;

        public Ptr(NilType nil) => Value = new Ref<T>(nil);

        public Ptr(ref NilType nil) => Value = new Ref<T>(ref nil);

        public override string ToString() => $"->{base.ToString()}";
    }
}