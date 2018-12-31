using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp.Repositories
{
    public abstract class BaseTableRepository<T> : Nhea.Data.Repository.MicrosoftAzureStorageRepository.BaseTableStorageRepository<T> where T : TableEntity, new()
    {
        protected override string StorageConnectionString => ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
    }
}
