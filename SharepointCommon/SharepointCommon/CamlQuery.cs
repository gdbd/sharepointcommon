namespace SharepointCommon
{
    using Microsoft.SharePoint;

    using SharepointCommon.Common;

    public class CamlQuery
    {
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

        public CamlQuery ViewFields(params string[] viewFields)
        {
            ViewFieldsStore = viewFields;
            return this;
        }

        public CamlQuery Recursive()
        {
            IsRecursive = true;
            return this;
        }

        public CamlQuery RowLimit(int rowlimit)
        {
            RowLimitStore = rowlimit;
            return this;
        }

        public CamlQuery Folder(string url)
        {
            FolderStore = url;
            return this;
        }

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
                    sb.Append(Q.FieldRef(field));
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