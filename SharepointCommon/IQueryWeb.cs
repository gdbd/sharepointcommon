namespace SharepointCommon
{
    using System;

    using Microsoft.SharePoint;

    public interface IQueryWeb : IDisposable
    {
        SPSite Site { get; set; }
        SPWeb Web { get; set; }

        IQueryWeb Elevate();
        IQueryWeb Unsafe();

        IQueryList<T> GetByUrl<T>(string listUrl) where T : Item, new();
        IQueryList<T> GetByName<T>(string listName) where T : Item, new();
        IQueryList<T> GetById<T>(Guid id) where T : Item, new();
        IQueryList<T> CurrentList<T>() where T : Item, new();

        IQueryList<T> Create<T>(string listName) where T : Item, new();

        bool ExistsByUrl(string listUrl);
        bool ExistsByName(string listName);
        bool ExistsById(Guid id);
    }
}