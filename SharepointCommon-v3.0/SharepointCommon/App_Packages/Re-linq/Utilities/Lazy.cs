﻿// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 

using Remotion.Utilities;

#if !NET_4_5
// ReSharper disable once CheckNamespace
namespace System
{
  internal class Lazy<T>
    where T: class
  {
    private readonly Func<T> _creator;
    private readonly object _syncObject = new object();
    private volatile T _value;

    public Lazy (Func<T> creator)
    {
      ArgumentUtility.CheckNotNull ("creator", creator);
      _creator = creator;
    }

    public T Value
    {
      get
      {
        if (_value == null)
        {
          lock (_syncObject)
          {
            _value = _creator();
          }
        }
        return _value;
      }
    }
  }
}
#endif