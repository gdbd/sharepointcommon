using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.SharePoint;
using SharepointCommon.Events;

// ReSharper disable once CheckNamespace
namespace SharepointCommon
{
    /// <summary>
    /// Represents wrapper on a SharePoint list or library
    /// </summary>
    /// <typeparam name="T">Item or ContentType of list item</typeparam>
    public interface IQueryList<T> where T : Item, new()
    {
        /// <summary>
        /// Gets reference of parent <see cref="IQueryWeb" object />
        /// </summary>
        IQueryWeb ParentWeb { get; }

        /// <summary>
        /// Gets underlying list instance
        /// </summary>
        SPList List { get; }

        /// <summary>
        /// Gets the SPList id.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the SPWeb id.
        /// </summary>
        Guid WebId { get; }

        /// <summary>
        /// Gets the SPSite id.
        /// </summary>
        Guid SiteId { get; }

        /// <summary>
        /// Gets or sets the SPList title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        string Title { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this list is versioning enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this list is versioning enabled; otherwise, <c>false</c>.
        /// </value>
        bool IsVersioningEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this list is folder creation allowed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this list is folder creation allowed; otherwise, <c>false</c>.
        /// </value>
        bool IsFolderCreationAllowed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether list allows to manage content types.
        /// </summary>
        /// <value>
        /// <c>true</c> if list allows manage content types; otherwise, <c>false</c>.
        /// </value>
        bool AllowManageContentTypes { get; set; }

        /// <summary>
        /// Gets the full url of list. Ex: http://server/lists/list1
        /// </summary>
        string Url { get; }

        /// <summary>
        /// Gets the relative url of list. Ex: /lists/list1
        /// </summary>
        string RelativeUrl { get; }

        /// <summary>
        /// Registers list event receiver
        /// </summary>
        /// <typeparam name="TEventReceiver">Type inherited from <see cref="ListEventReceiver{T}"/></typeparam>
        void AddEventReceiver<TEventReceiver>() where TEventReceiver : ListEventReceiver<T>;


        /// <summary>
        /// Removes list event receiver
        /// </summary>
        /// <typeparam name="TEventReceiver">Type inherited from <see cref="ListEventReceiver{T}"/></typeparam>
        void RemoveEventReceiver<TEventReceiver>() where TEventReceiver : ListEventReceiver<T>;

        /// <summary>
        /// Gets the url of specific list form
        /// </summary>
        /// <param name="pageType">Type of the page.</param>
        /// <param name="id">The id of item</param>
        /// <param name="isDlg">Add 'isDlg=1' to form url</param>
        /// <returns>Url of list form with item id</returns>
        string FormUrl(PageType pageType, int id = 0, bool isDlg = false);

        /// <summary>
        /// Adds new list item to the list
        /// </summary>
        /// <param name="entity">instance of entity that represents new item</param>
        void Add(T entity);

        /// <summary>
        /// Updates specified field of existing item by data of entity
        /// </summary>
        /// <param name="entity">The entity instance.</param>
        /// <param name="incrementVersion">if set to <c>true</c>increments item version.</param>
        /// <param name="selectors">Expressions used to enumerate fields been updated. Ex: e => e.Title</param>
        void Update(T entity, bool incrementVersion, params Expression<Func<T, object>>[] selectors);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="fieldSelector"></param>
        /// <param name="valueToSet"></param>
        /// <param name="incrementVersion"></param>
        void UpdateField(T entity, Expression<Func<T, object>> fieldSelector, object valueToSet, bool incrementVersion = true);

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="recycle">if set to <c>true</c> to move in recycle bin.</param>
        void Delete(T entity, bool recycle);

        /// <summary>
        /// Deletes item by the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="recycle">if set to <c>true</c> to move recycle bin.</param>
        void Delete(int id, bool recycle);

        /// <summary>
        /// Gets item by specified id.
        /// </summary>
        /// <param name="id">The item id.</param>
        /// <returns>List item instance.</returns>
        T ById(int id);

        /// <summary>
        /// Gets item of specified content type by id
        /// </summary>
        /// <typeparam name="TCt">Type of entity, represents content type (marked with [ContentType])</typeparam>
        /// <param name="id">The item id.</param>
        /// <returns>List item instance.</returns>
        TCt ById<TCt>(int id) where TCt : Item, new();

        /// <summary>
        /// Gets item by specified Guid (field 'GUID' not equals to 'UniqueId')
        /// </summary>
        /// <param name="id"></param>
        /// <returns>List item instance.</returns>
        T ByGuid(Guid id);

        /// <summary>
        /// Gets item by specified content type Guid (field 'GUID' not equals to 'UniqueId')
        /// </summary>
        /// <typeparam name="TCt">Type of entity, represents content type (marked with [ContentType])</typeparam>
        /// <param name="id">the item id.</param>
        /// <returns>List item instance.</returns>
        TCt ByGuid<TCt>(Guid id) where TCt : Item, new();

        /// <summary>
        /// Gets items by value of specified field.
        /// </summary>
        /// <typeparam name="TR">type of value</typeparam>
        /// <param name="selector">Expression used to point needed field. Ex: e=>e.Title</param>
        /// <param name="value">value to filter items</param>
        /// <returns>items, which have specified value in specified field.</returns>
        IEnumerable<T> ByField<TR>(Expression<Func<T, TR>> selector, TR value);

        /// <summary>
        /// Gets items filtered by specified options
        /// </summary>
        /// <param name="option">The option used to filter items.</param>
        /// <returns>items by query</returns>
        IEnumerable<T> Items(CamlQuery option);

        /// <summary>
        /// Gets items of specified content type, filtered by specified options.
        /// </summary>
        /// <typeparam name="TCt">Type of entity, represents content type (marked with [ContentType])</typeparam>
        /// <param name="option">The option used to filter items.</param>
        /// <returns>items by query.</returns>
        IEnumerable<TCt> Items<TCt>(CamlQuery option) where TCt : Item, new();

        /// <summary>
        /// Deletes the list.
        /// </summary>
        /// <param name="recycle">if set to <c>true</c> to recycle list.</param>
        void DeleteList(bool recycle);

        /// <summary>
        /// Check that all fields of '<typeparamref name="T"/>' are exists in list.
        /// If at least one of fields not exists, throw exception.
        /// </summary>
        /// <exception cref="SharepointCommonException"></exception>
        void CheckFields();

        /// <summary>
        /// Check that all fields of '<typeparamref name="T"/>' are exists in list.
        /// If some of fields not exists in list, creates it.
        /// </summary>
        void EnsureFields();

        /// <summary>
        /// Ensure field in list.(Skip if exists, Create if not exists)
        /// </summary>
        /// <param name="selector">Expression used to point needed field. Ex: e=>e.Title</param>
        void EnsureField(Expression<Func<T, object>> selector);

        /// <summary>
        /// Determines whether the specified field contains in list.
        /// </summary>
        /// <param name="selector">Expression used to point needed field. Ex: e=>e.Title</param>
        /// <returns>
        /// <c>true</c> if the specified selector contains field; otherwise, <c>false</c>.
        /// </returns>
        bool ContainsField(Expression<Func<T, object>> selector);

        /// <summary>
        /// Gets the wrapper for SPField.
        /// </summary>
        /// <param name="selector">Expression used to point needed field. Ex: e=>e.Title</param>
        /// <returns>field wrapper</returns>
        Field GetField(Expression<Func<T, object>> selector);

        /// <summary>
        /// Gets the wrapper collection for list fields.
        /// </summary>
        /// <param name="onlyCustom">if set to <c>true</c> only custom fields; otherwise all fields.</param>
        /// <returns></returns>
        IEnumerable<Field> GetFields(bool onlyCustom);

        /// <summary>
        /// Adds the specified content type to list.
        /// </summary>
        /// <typeparam name="TCt">Type of entity, represents content type (marked with [ContentType]).</typeparam>
        void AddContentType<TCt>() where TCt : Item, new();

        /// <summary>
        /// Determines whether list contains specified content type.
        /// </summary>
        /// <typeparam name="TCt">Type of entity, represents content type (marked with [ContentType])</typeparam>
        /// <returns>
        /// <c>true</c> if [contains content type]; otherwise, <c>false</c>.
        /// </returns>
        bool ContainsContentType<TCt>() where TCt : Item, new();

        /// <summary>
        /// Removes the specified content type from list.
        /// </summary>
        /// <typeparam name="TCt">Type of entity, represents content type (marked with [ContentType])</typeparam>
        void RemoveContentType<TCt>() where TCt : Item, new();   
    }

    public class QueryEvent<T>
    {
        public event Action<T> Added;
    }
}