﻿/*
 * Copyright 2007-2012 JetBrains s.r.o.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

namespace JetBrains.Annotations {
    /// <summary> Indicates that the value of marked element could be <c>null</c> sometimes,
    /// so the check for <c>null</c> is necessary before its usage. </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter |
                    AttributeTargets.Property | AttributeTargets.Delegate | AttributeTargets.Field)]
    public sealed class CanBeNullAttribute : Attribute {}


    /// <summary> Indicates that the value of marked element could never be <c>null</c>. </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter |
                    AttributeTargets.Property | AttributeTargets.Delegate | AttributeTargets.Field)]
    public sealed class NotNullAttribute : Attribute {}


    /// <summary> Indicates that method doesn't contain observable side effects. </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class PureAttribute : Attribute {}
}
