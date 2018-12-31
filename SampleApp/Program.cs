using SampleApp.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            TestTableStorage();
        }

        private static void TestTableStorage()
        {
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
        }
    }
}
