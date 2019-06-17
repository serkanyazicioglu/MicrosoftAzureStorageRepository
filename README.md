[![Build Status](https://dev.azure.com/serkanyazicioglu/serkanyazicioglu/_apis/build/status/serkanyazicioglu.MicrosoftAzureStorageRepository?branchName=master)](https://dev.azure.com/serkanyazicioglu/serkanyazicioglu/_build/latest?definitionId=4&branchName=master)
[![NuGet](https://img.shields.io/nuget/v/Nhea.Data.Repository.MicrosoftAzureStorageRepository.svg)](https://www.nuget.org/packages/Nhea.Data.Repository.MicrosoftAzureStorageRepository/)

# Nhea Microsoft Azure Storage Repository

Nhea base repository classes for Microsoft Azure Storage


## Getting Started

Nhea is on NuGet. You may install Nhea Microsoft Azure Storage Repository via NuGet Package manager.

https://www.nuget.org/packages/Nhea.Data.Repository.MicrosoftAzureStorageRepository/

```
Install-Package Nhea.Data.Repository.MicrosoftAzureStorageRepository
```

### Prerequisites

Project is built with .NET Framework 4.6.1.

This project references 
-	Nhea > 1.5.1
-	Microsoft.WindowsAzure.Storage > 9.3.2

I highly suggest you to use Azure Storage Explorer. Click the link below to download.

https://azure.microsoft.com/en-us/features/storage-explorer/

### Configuration

First of all creating a base repository class is a good idea to set basic properties like connection string.

```
public abstract class BaseTableRepository<T> : Nhea.Data.Repository.MicrosoftAzureStorageRepository.BaseTableStorageRepository<T> where T : TableEntity, new()
{
    protected override string StorageConnectionString => ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString;
}
```
You may remove the abstract modifier if you want to use generic repositories or you may create individual repository classes for your documents if you want to set specific properties.
```
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
```
Then in your code just initalize a new instance of your class and call appropriate methods for your needs.

```
string partitionKey = "SomeMembers";

string newItemRowKey = Guid.NewGuid().ToString();

using (MemberRepository memberRepository = new MemberRepository())
{
    memberRepository.CreateTable(); //Remove this code if you're sure your table exists!
    //memberRepository.DeleteTable(); //Execute to delete table!

    var member = memberRepository.CreateNew();
    member.PartitionKey = partitionKey;
    member.RowKey = newItemRowKey;
    member.Title = "Test Member";
    member.UserName = "username";
    member.Password = "password";
    member.Email = "test@test.com";
    memberRepository.Save();
}

using (MemberRepository memberRepository = new MemberRepository())
{
    var members = memberRepository.GetAll(query => query.PartitionKey == partitionKey && query.Timestamp >= DateTime.Today).ToList();

    foreach (var member in members)
    {
        member.Title += " Lastname";
    }

    memberRepository.Save();
}

using (MemberRepository memberRepository = new MemberRepository())
{
    var member = memberRepository.GetSingle(query => query.PartitionKey == partitionKey && query.RowKey == newItemRowKey);

    if (member != null)
    {
        member.Title = "Selected Member 2";
        memberRepository.Save();
    }
}

using (MemberRepository memberRepository = new MemberRepository())
{
    memberRepository.Delete(query => query.Title == "Selected Member 2");
    memberRepository.Save();
}

using (MemberRepository memberRepository = new MemberRepository())
{
    var member = memberRepository.CreateNew();
    bool isNew = memberRepository.IsNew(member);
}
```