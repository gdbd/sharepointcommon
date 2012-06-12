namespace SharepointCommon
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public interface IQueryList<T> where T : Item, new()
    {
        Guid Id { get; }
        Guid WebId { get; }
        Guid SiteId { get; }
        string Title { get; set; }
        bool IsVersioningEnabled { get; set; }
        bool IsFolderCreationAllowed { get; set; }
        bool AllowManageContentTypes { get; set; }
        string Url { get; }
        string RelativeUrl { get; }
        string FormUrl(PageType pageType, int id);

        void Add(T entity);
        void Update(T entity, bool incrementVersion);
        void Update(T entity, bool incrementVersion, params Expression<Func<T, object>>[] selectors);
        void Delete(T entity, bool recycle);
        void Delete(int id, bool recycle);

        T ById(int id);
        TCt ById<TCt>(int id) where TCt : Item, new();

        T ByGuid(Guid id);
        TCt ByGuid<TCt>(Guid id) where TCt : Item, new();

        IEnumerable<T> ByField<TR>(Expression<Func<T, TR>> selector, TR value);
        IEnumerable<T> Items(CamlQuery option);
        IEnumerable<TCt> Items<TCt>(CamlQuery option) where TCt : Item, new();

        void DeleteList(bool recycle);
        void CheckFields();
        void EnsureFields();
        void EnsureField(Expression<Func<T, object>> selector);
        bool ContainsField(Expression<Func<T, object>> selector);

        Field GetField(Expression<Func<T, object>> selector);
        IEnumerable<Field> GetFields(bool onlyCustom);

        void AddContentType<TCt>() where TCt : Item, new();
        bool ContainsContentType<TCt>() where TCt : Item, new();
        void RemoveContentType<TCt>() where TCt : Item, new();   
    }
}