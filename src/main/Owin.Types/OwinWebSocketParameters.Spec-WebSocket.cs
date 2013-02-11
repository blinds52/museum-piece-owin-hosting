// <copyright file="OwinWebSocketParameters.Spec-WebSocket.cs" company="Microsoft Open Technologies, Inc.">
// Copyright 2013 Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
namespace Owin.Types
{
    public partial struct OwinWebSocketParameters
    {
        public string SubProtocol
        {
            get { return Get<string>(OwinConstants.WebSocket.SubProtocol); }
            set { Set(OwinConstants.WebSocket.SubProtocol, value); }
        }
    }
}
