using Microsoft.WindowsAzure.Storage.Table;
using Nhea.Data;
using Nhea.Enumeration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SampleApp.Repositories
{
    public partial class Member : TableEntity
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public int Status { get; set; }

        public string Email { get; set; }
    }

    public class MemberRepository : BaseTableRepository<Member>
    {
        //You may give a custom name for your table. Otherwise repository creates a collection by using object name.
        protected override string TableName => base.TableName;

        public override Member CreateNew()
        {
            var entity = base.CreateNew();
            entity.Id = Guid.NewGuid();
            entity.Status = (int)StatusType.Available;

            return entity;
        }

        public override Expression<Func<Member, object>> DefaultSorter => query => new { query.Timestamp.DateTime };

        protected override SortDirection DefaultSortType => SortDirection.Descending;

        public override Expression<Func<Member, bool>> DefaultFilter => query => query.Status == (int)StatusType.Available;
    }
}
