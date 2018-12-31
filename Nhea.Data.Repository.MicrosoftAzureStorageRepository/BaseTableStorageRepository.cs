using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nhea.Data.Repository.MicrosoftAzureStorageRepository
{
    public abstract class BaseTableStorageRepository<T> : BaseRepository<T>, IDisposable where T : class, ITableEntity, new()
    {
        protected abstract string StorageConnectionString { get; }

        protected CloudStorageAccount CurrentStorageAccount => CloudStorageAccount.Parse(StorageConnectionString);

        protected virtual string TableName
        {
            get
            {
                return typeof(T).Name;
            }
        }

        private CloudTable CurrentCloudTable
        {
            get
            {
                return CurrentCloudTableClient.GetTableReference(TableName);
            }
        }

        private CloudTableClient currentCloudTableClient;
        public CloudTableClient CurrentCloudTableClient
        {
            get
            {
                if (currentCloudTableClient == null)
                {
                    currentCloudTableClient = new CloudTableClient(CurrentStorageAccount.TableStorageUri, CurrentStorageAccount.Credentials);
                }

                return currentCloudTableClient;
            }
        }

        private TableBatchOperation currentBatchOperation;
        private TableBatchOperation CurrentBatchOperation
        {
            get
            {
                if (currentBatchOperation == null)
                {
                    currentBatchOperation = new TableBatchOperation();
                }

                return currentBatchOperation;
            }
        }

        public bool CreateTable()
        {
            var table = CurrentCloudTableClient.GetTableReference(TableName);
            return table.CreateIfNotExists();
        }

        public bool DeleteTable()
        {
            var table = CurrentCloudTableClient.GetTableReference(TableName);
            return table.DeleteIfExists();
        }

        private List<T> NewItems = new List<T>();

        public override T CreateNew()
        {
            var entity = new T();
            NewItems.Add(entity);
            return entity;
        }

        public override void Add(T entity)
        {
            if (entity != null)
            {
                CurrentBatchOperation.InsertOrReplace(entity);
            }
        }

        public override void Add(List<T> entities)
        {
            foreach (T entity in entities)
            {
                CurrentBatchOperation.InsertOrReplace(entity);
            }
        }

        protected override int CountCore(System.Linq.Expressions.Expression<Func<T, bool>> filter, bool getDefaultFilter)
        {
            if (getDefaultFilter && DefaultFilter != null)
            {
                filter = filter.And(DefaultFilter);
            }

            if (filter == null)
            {
                filter = query => true;
            }

            return CurrentCloudTable.CreateQuery<T>().Count(filter);
        }

        protected override bool AnyCore(System.Linq.Expressions.Expression<Func<T, bool>> filter, bool getDefaultFilter)
        {
            if (getDefaultFilter && DefaultFilter != null)
            {
                filter = filter.And(DefaultFilter);
            }

            if (filter == null)
            {
                filter = query => true;
            }

            return CurrentCloudTable.CreateQuery<T>().Any(filter);
        }

        public override void Delete(T entity)
        {
            if (entity != null)
            {
                CurrentBatchOperation.Delete(entity);
            }
        }

        public override void Delete(System.Linq.Expressions.Expression<Func<T, bool>> filter)
        {
            var returnList = CurrentCloudTable.CreateQuery<T>().Where(filter).ToList();

            if (returnList != null && returnList.Any())
            {
                foreach (var entity in returnList)
                {
                    CurrentBatchOperation.Delete(entity);
                }
            }
        }

        public override void Dispose()
        {
            this.NewItems = null;
            this.CurrentBatchOperation.Clear();
            this.currentCloudTableClient = null;
        }

        protected override IQueryable<T> GetAllCore(System.Linq.Expressions.Expression<Func<T, bool>> filter, bool getDefaultFilter, bool getDefaultSorter, string sortColumn, SortDirection? sortDirection, bool allowPaging, int pageSize, int pageIndex, ref int totalCount)
        {
            if (getDefaultFilter && DefaultFilter != null)
            {
                filter = filter.And(DefaultFilter);
            }

            if (filter == null)
            {
                filter = query => true;
            }

            IQueryable<T> returnList = CurrentCloudTable.CreateQuery<T>().Where(filter).ToList().AsQueryable();

            if (!String.IsNullOrEmpty(sortColumn))
            {
                returnList = returnList.Sort(sortColumn, sortDirection);
            }
            else if (getDefaultSorter && DefaultSorter != null)
            {
                if (DefaultSortType == SortDirection.Ascending)
                {
                    returnList = returnList.OrderBy(DefaultSorter);
                }
                else
                {
                    returnList = returnList.OrderByDescending(DefaultSorter);
                }
            }

            if (allowPaging && pageSize > 0)
            {
                if (totalCount == 0)
                {
                    totalCount = returnList.Count();
                }

                int skipCount = pageSize * pageIndex;

                returnList = returnList.Skip(skipCount).Take(pageSize);
            }

            return returnList;
        }

        public override T GetById(object id)
        {
            throw new NotImplementedException();
        }

        protected override T GetSingleCore(System.Linq.Expressions.Expression<Func<T, bool>> filter, bool getDefaultFilter)
        {
            if (DefaultFilter != null)
            {
                filter = filter.And(DefaultFilter);
            }

            return CurrentCloudTable.CreateQuery<T>().Where(filter).SingleOrDefault();
        }

        public override bool IsNew(T entity)
        {
            return NewItems.Contains(entity);
        }

        public override void Refresh(T entity)
        {
            throw new NotImplementedException();
        }

        private const int PageCount = 100;

        public override void Save()
        {
            if (NewItems.Any())
            {
                foreach (T entity in NewItems)
                {
                    CurrentBatchOperation.InsertOrReplace(entity);
                }

                NewItems = new List<T>();
            }

            if (CurrentBatchOperation.Any())
            {
                if (CurrentBatchOperation.Count > PageCount)
                {
                    int pager = 0;
                    int currentListCount = 0;

                    do
                    {
                        var items = CurrentBatchOperation.Skip(pager * PageCount).Take(PageCount).ToList();
                        currentListCount = items.Count();

                        var pagedBatchOperation = new TableBatchOperation();

                        foreach (var item in items)
                        {
                            pagedBatchOperation.Add(item);
                        }

                        CurrentCloudTable.ExecuteBatch(pagedBatchOperation);
                        pager++;
                    }
                    while (currentListCount == PageCount);
                }
                else
                {
                    CurrentCloudTable.ExecuteBatch(CurrentBatchOperation);
                }

                CurrentBatchOperation.Clear();
            }
        }
    }
}
