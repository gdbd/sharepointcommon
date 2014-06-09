using System;
using System.Linq;
using System.Linq.Expressions;
using SharepointCommon.Expressions;
using Microsoft.SharePoint;

// ReSharper disable once CheckNamespace
namespace SharepointCommon
{
    /// <summary>
    /// Class used to represent query to SharePoint list, such CAML query, ViewFields and etc.
    /// </summary>
    public class CamlQuery
    {
        /// <summary>
        /// Gets the default query, that return all items with all view fields
        /// </summary>
        public static CamlQuery Default
        {
            get
            {
                var option = new CamlQuery
                {
                    ViewFieldsStore = new string[0],
                    IsRecursive = false,
                    RowLimitStore = default(int),
                    FolderStore = null,
                };
                return option;
            }
        }

        internal string[] ViewFieldsStore { get; private set; }

        internal bool IsRecursive { get; private set; }

        internal int RowLimitStore { get; private set; }

        internal string FolderStore { get; private set; }

        internal string CamlStore { get; private set; }

        /// <summary>
        /// Sets ViewFields used in CAML query
        /// </summary>
        /// <param name="viewFields">The view field names (not xml tags!).</param>
        /// <returns>Fluent instance of that class</returns>
        public CamlQuery ViewFields(params string[] viewFields)
        {
            ViewFieldsStore = viewFields;
            return this;
        }

        /// <summary>
        /// Sets ViewFields used in CAML query
        /// </summary>
        /// <param name="selectors">Expressions indicates properties used as view fields</param>
        /// <returns>Fluent instance of that class</returns>
        public CamlQuery ViewFields<T>(params Expression<Func<T, object>>[] selectors) where T : Item
        {
            var memVisitor = new MemberAccessVisitor();
            ViewFieldsStore = selectors.Select(memVisitor.GetMemberName).ToArray();
            return this;
        }

        /// <summary>
        /// Indicates that CAML query affects items in subfolders
        /// </summary>
        /// <returns>Fluent instance of that class</returns>
        public CamlQuery Recursive()
        {
            IsRecursive = true;
            return this;
        }

        /// <summary>
        /// Sets limit of rows, returned by CAML query
        /// </summary>
        /// <param name="rowlimit"></param>
        /// <returns>Fluent instance of that class</returns>
        public CamlQuery RowLimit(int rowlimit)
        {
            RowLimitStore = rowlimit;
            return this;
        }

        /// <summary>
        /// Sets folder url, used in CAML query
        /// </summary>
        /// <param name="url"></param>
        /// <returns>Fluent instance of that class</returns>
        public CamlQuery Folder(string url)
        {
            FolderStore = url;
            return this;
        }

        /// <summary>
        /// Sets CAML query text. Use '<see cref="SharepointCommon.Q"/>' class to construct query
        /// </summary>
        /// <param name="caml">The caml.</param>
        /// <returns>Fluent instance of that class</returns>
        public CamlQuery Query(string caml)
        {
            CamlStore = caml;
            return this;
        }

        internal SPQuery GetSpQuery(SPWeb web)
        {
            var query = new SPQuery { };
            
            if (CamlStore != null) query.Query = CamlStore;
            if (ViewFieldsStore != null && ViewFieldsStore.Length > 0)
            {
                var sb = new System.Text.StringBuilder();
                foreach (string field in ViewFieldsStore)
                {
#pragma warning disable 612,618
                    sb.Append(Q.FieldRef(field));
#pragma warning restore 612,618
                }
                query.ViewFields = sb.ToString();
                query.ViewFieldsOnly = true;
            }

            if (RowLimitStore != default(int)) query.RowLimit = (uint)RowLimitStore;

            if (FolderStore != null)
            {
                var folder = web.GetFolder(FolderStore);
                query.Folder = folder;
            }

            if (IsRecursive)
            {
                query.ViewAttributes = "Scope=\"Recursive\"";
            }

            return query;
        }
    }
}