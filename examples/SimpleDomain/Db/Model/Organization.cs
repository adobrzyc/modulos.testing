using System;
using System.Collections.Generic;

namespace SimpleDomain.Db.Model
{
    public class Organization
    {
        public Guid OrganizationId { get; set; }
        public string Name { get; set; }

        public ICollection<Department> Departments { get; set; }
    }
}