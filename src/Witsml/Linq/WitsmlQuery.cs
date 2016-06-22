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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Energistics.DataAccess;
using log4net;
using Ast = LinqExtender.Ast;

namespace PDS.Witsml.Linq
{
    /// <summary>
    /// Default context to be queried.
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    /// <typeparam name="TList">List type</typeparam>
    public class WitsmlQuery<T, TList> : ExpressionVisitor, IWitsmlQuery<T>, IWitsmlQuery, LinqExtender.IQueryContext<T> where TList : IEnergisticsCollection
    {
        /// <summary>
        /// WitsmlQuery
        /// </summary>
        /// <param name="context"></param>
        public WitsmlQuery(WitsmlContext context)
        {
            Context = context;
            Logger = LogManager.GetLogger(GetType());
            Query = WITSMLWebServiceConnection.BuildEmptyQuery<TList>();
            Queryable = LinqExtender.Queryable.Select(this, x => x);
            Options = new Dictionary<string, string>();

            // update Version property modifed by BuildEmptyQuery
            Query.SetVersion(context.DataSchemaVersion);
        }

        /// <summary>
        /// WitsmlContext
        /// </summary>
        public WitsmlContext Context { get; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        public ILog Logger { get; }

        /// <summary>
        /// Query
        /// </summary>
        public TList Query { get; }

        /// <summary>
        /// Options
        /// </summary>
        public Dictionary<string, string> Options { get; }

        /// <summary>
        /// Invoked during execution of the query , with the
        /// pre populated expression tree.
        /// </summary>
        /// <param name="expression">Target expression block</param>
        /// <returns>Expected result</returns>
        public IEnumerable<T> Execute(Ast.Expression expression)
        {
            Visit(expression);

            var optionsIn = string.Join(";", Options.Select(x => $"{x.Key}={x.Value}"));
            var objectType = ObjectTypes.GetObjectType<T>();
            var xmlIn = WitsmlParser.ToXml(Query);

            Context.LogQuery(Functions.GetFromStore, objectType, xmlIn, optionsIn);

            using (var client = Context.Connection.CreateClientProxy())
            {
                var wmls = (IWitsmlClient)client;
                string suppMsgOut, xmlOut;

                var returnCode = wmls.WMLS_GetFromStore(objectType, xmlIn, optionsIn, null, out xmlOut, out suppMsgOut);
                var result = Enumerable.Empty<T>();

                try
                {
                    if (returnCode > 0)
                    {
                        var document = WitsmlParser.Parse(xmlOut);
                        var response = WitsmlParser.Parse<TList>(document.Root);
                        result = (IEnumerable<T>)response.Items;
                    }
                }
                catch (WitsmlException ex)
                {
                    Logger.ErrorFormat("Error parsing query response: {0}{2}{2}{1}", xmlOut, ex, Environment.NewLine);
                    returnCode = (short)ex.ErrorCode;
                    suppMsgOut = ex.Message + " " + ex.GetBaseException().Message;
                }

                Context.LogResponse(Functions.GetFromStore, objectType, xmlIn, optionsIn, xmlOut, returnCode, suppMsgOut);
                return result;
            }
        }

        /// <summary>
        /// Provides a callback that can be used to include specific elements in the query response.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public IWitsmlQuery<T> Include(Action<T> action)
        {
            action((T)Query.Items[0]);
            return this;
        }

        /// <summary>
        /// Sets the options that will be passed in to the GetFromStore query.
        /// </summary>
        /// <param name="optionsIn"></param>
        /// <returns></returns>
        public IWitsmlQuery<T> With(OptionsIn optionsIn)
        {
            Options[optionsIn.Key] = optionsIn.Value;
            return this;
        }

        /// <summary>
        /// Sets the options that will be passed in to the GetFromStore query.
        /// </summary>
        /// <param name="optionsIn"></param>
        /// <returns></returns>
        IWitsmlQuery IWitsmlQuery.With(OptionsIn optionsIn)
        {
            With(optionsIn);
            return this;
        }

        /// <summary>
        /// Sets the property value.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="value">The value.</param>
        protected override void SetPropertyValue(PropertyInfo property, object value)
        {
            property.SetValue(Query.Items[0], value);
        }

        #region IQueryable<T> Members

        private IQueryable<T> Queryable;

        /// <summary>
        /// Gets the expression tree that is associated with the instance of <see cref="T:System.Linq.IQueryable" />.
        /// </summary>
        public Expression Expression
        {
            get { return Queryable.Expression; }
        }

        /// <summary>
        /// Gets the type of the element(s) that are returned when the expression tree associated with this instance of <see cref="T:System.Linq.IQueryable" /> is executed.
        /// </summary>
        public Type ElementType
        {
            get { return Queryable.ElementType; }
        }

        /// <summary>
        /// Gets the query provider that is associated with this data source.
        /// </summary>
        public IQueryProvider Provider
        {
            get { return Queryable.Provider; }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return Queryable.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Queryable.GetEnumerator();
        }

        #endregion
    }
}
