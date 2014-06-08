using System;
using Microsoft.SharePoint;

// ReSharper disable once CheckNamespace
namespace SharepointCommon
{
    /// <summary>
    /// Represents an abstract wrapper for SPWeb and SPSite objects
    /// </summary>
    public interface IQueryWeb : IDisposable
    {
        /// <summary>
        /// Gets the associated SPSite instance.
        /// </summary>
        SPSite Site { get; }

        /// <summary>
        /// Gets the associated SPWeb instance.
        /// </summary>
        SPWeb Web { get;  }

        /// <summary>
        /// Open Site and Web with a 'system account'
        /// </summary>
        /// <returns></returns>
        IQueryWeb Elevate();

        /// <summary>
        /// Set AllowUnsafeUpdates on a 'Web'
        /// </summary>
        /// <returns></returns>
        IQueryWeb Unsafe();

        /// <summary>
        /// Gets the SPList wrapper by URL.
        /// </summary>
        /// <typeparam name="T">Item or ContentType entity</typeparam>
        /// <param name="listUrl">The list URL.</param>
        /// <returns>SPList wrapper</returns>
        IQueryList<T> GetByUrl<T>(string listUrl) where T : Item, new();

        /// <summary>
        /// Gets the SPList wrapper by NAME.
        /// </summary>
        /// <typeparam name="T">Item or ContentType entity</typeparam>
        /// <param name="listName">Name of the list.</param>
        /// <returns>SPList wrapper</returns>
        IQueryList<T> GetByName<T>(string listName) where T : Item, new();

        /// <summary>
        /// Gets the SPList wrapper by ID.
        /// </summary>
        /// <typeparam name="T">Item or ContentType entity</typeparam>
        /// <param name="id">The ID.</param>
        /// <returns>SPList wrapper</returns>
        IQueryList<T> GetById<T>(Guid id) where T : Item, new();

        /// <summary>
        /// Gets the SPList wrapper for a SPContext.Current.List
        /// </summary>
        /// <typeparam name="T">Item or ContentType entity</typeparam>
        /// <returns>SPList wrapper</returns>
        IQueryList<T> CurrentList<T>() where T : Item, new();

        /// <summary>
        /// Creates new List or Library with specified Name
        /// </summary>
        /// <typeparam name="T">Item or ContentType entity</typeparam>
        /// <param name="listName">Name of the list.</param>
        /// <returns>SPList wrapper</returns>
        IQueryList<T> Create<T>(string listName) where T : Item, new();

        /// <summary>
        /// Checks that list with specified URL exists
        /// </summary>
        /// <param name="listUrl">The list URL.</param>
        /// <returns>true if exists</returns>
        bool ExistsByUrl(string listUrl);

        /// <summary>
        /// Checks that list with specified NAME exists
        /// </summary>
        /// <param name="listName">Name of the list.</param>
        /// <returns>true if exists</returns>
        bool ExistsByName(string listName);

        /// <summary>
        /// Checks that list with specified ID exists
        /// </summary>
        /// <param name="id">The list id.</param>
        /// <returns>true if exists</returns>
        bool ExistsById(Guid id);
    }
}