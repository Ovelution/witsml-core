﻿//----------------------------------------------------------------------- 
// PDS.Witsml, 2016.1
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
using System.Xml.Linq;
using PDS.Witsml.Data;

namespace PDS.Witsml
{
    /// <summary>
    /// Encapsulates common properties used for parsing WITSML elements.
    /// </summary>
    /// <seealso cref="PDS.Witsml.Data.DataObjectNavigationContext" />
    public abstract class WitsmlParserContext : DataObjectNavigationContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WitsmlParserContext"/> class.
        /// </summary>
        /// <param name="element">The XML element.</param>
        protected WitsmlParserContext(XElement element)
        {
            Element = element;
        }

        /// <summary>
        /// Gets the XML element.
        /// </summary>
        /// <value>The XML element.</value>
        public XElement Element { get; }

        /// <summary>
        /// Gets or sets a value indicating whether NaN elements should be removed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if NaN elements should be removed; otherwise, <c>false</c>.
        /// </value>
        public bool RemoveNaNElements { get; set; }
    }

    /// <summary>
    /// Encapsulates common properties used for parsing WITSML elements for a specific data object type.
    /// </summary>
    /// <typeparam name="T">The type of the data object.</typeparam>
    /// <seealso cref="PDS.Witsml.Data.DataObjectNavigationContext" />
    public class WitsmlParserContext<T> : WitsmlParserContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WitsmlParserContext{T}"/> class.
        /// </summary>
        /// <param name="element">The XML element.</param>
        public WitsmlParserContext(XElement element) : base(element)
        {
        }

        /// <summary>
        /// Gets the type of the data object.
        /// </summary>
        /// <value>The type of the data object.</value>
        public override Type DataObjectType => typeof(T);
    }
}
