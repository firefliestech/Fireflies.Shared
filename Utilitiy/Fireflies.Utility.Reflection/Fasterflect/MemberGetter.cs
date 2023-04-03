#region License

// Copyright © 2010 Buu Nguyen, Morten Mertner
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://fasterflect.codeplex.com/

#endregion

namespace Fireflies.Utility.Reflection.Fasterflect; 

/// <summary>
///     A delegate to retrieve the value of an instance field or property of an object.
/// </summary>
/// <param name="obj">
///     The object whose field's or property's value is to be retrieved.
///     Use <see langword="null" /> for static field or property.
/// </param>
/// <returns>The value of the instance field or property.</returns>
public delegate object MemberGetter(object obj);