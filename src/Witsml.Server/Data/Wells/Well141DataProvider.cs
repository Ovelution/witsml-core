﻿//----------------------------------------------------------------------- 
// PDS.Witsml.Server, 2016.1
//
// Copyright 2016 Petrotechnical Data Systems
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
//-----------------------------------------------------------------------

using System;
using Energistics.DataAccess.WITSML141;
using Energistics.Datatypes;

namespace PDS.Witsml.Server.Data.Wells
{
    /// <summary>
    /// Data provider that implements support for WITSML API functions for <see cref="Well"/>.
    /// </summary>
    public partial class Well141DataProvider
    {
        /// <summary>
        /// Sets the additional default values.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        partial void SetAdditionalDefaultValues(Well dataObject)
        {
            if (string.IsNullOrWhiteSpace(dataObject.TimeZone))
                dataObject.TimeZone = GetTimeZoneOffset();
        }

        /// <summary>
        /// Sets additional default values for the specified data object and URI.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="uri">The data object URI.</param>
        partial void SetAdditionalDefaultValues(Well dataObject, EtpUri uri)
        {
            dataObject.TimeZone = GetTimeZoneOffset();
        }

        /// <summary>
        /// Gets the local time zone offset.
        /// </summary>
        /// <returns>The local time zone offset.</returns>
        private string GetTimeZoneOffset()
        {
            var offset = TimeZoneInfo.Local.GetUtcOffset(DateTimeOffset.Now).ToString();
            return offset.Substring(0, offset.LastIndexOf(':'));
        }
    }
}