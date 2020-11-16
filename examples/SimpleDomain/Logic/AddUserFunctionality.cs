using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using SimpleDomain.Db;
using SimpleDomain.Db.Model;

namespace SimpleDomain.Logic
{
    public class AddUserFunctionality
    {
        private readonly Context db;

        public AddUserFunctionality(Context db)
        {
            this.db = db;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public async Task<User> CreateUser(string name, string sName, Guid departmentId, Guid? id = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Invalid parameter.", nameof(name));
            if (string.IsNullOrWhiteSpace(sName)) throw new ArgumentException("Invalid parameter.", nameof(sName));

            var task = await db.Users.AddAsync(new User
            {
                UserId = id ?? Guid.NewGuid(),
                Name = name,
                SName = sName,
                DepartmentId = departmentId
            });

            await db.SaveChangesAsync();

            return task.Entity;
        }
    }
}