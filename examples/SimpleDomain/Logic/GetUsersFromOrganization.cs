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
    public class GetUsersFromOrganization
    {
        private readonly Context db;

        public GetUsersFromOrganization(Context db)
        {
            this.db = db;
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public async Task<IEnumerable<User>> Execute(Guid organizationId)
        {
            var users = await db.Departments
                .Where(e => e.OrganizationId == organizationId)
                .SelectMany(e => e.Users)
                .Include(e=>e.Department)
                .Include(e=>e.Department.Organization)
                .ToArrayAsync()
                .ConfigureAwait(false);

            return users;
        }
    }
}