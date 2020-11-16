using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SimpleDomain.Db;
using SimpleDomain.Db.Model;

namespace SimpleDomain.Logic
{
    public class GetUsersOfDepartment
    {
        private readonly Context db;

        public GetUsersOfDepartment(Context db)
        {
            this.db = db;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public async Task<IEnumerable<User>> Execute(Guid departmentId)
        {
            var users = await db.Departments
                .Where(e => e.DepartmentId == departmentId)
                .SelectMany(e => e.Users)
                .Include(e=>e.Department)
                .ToArrayAsync()
                .ConfigureAwait(false);

            return users;
        }
    }
}