using System;
using System.Collections.Generic;

namespace SimpleDomain.Db.Model
{
    public class Department
    {
        public Guid DepartmentId { get; set; }
        public string Name { get; set; }
        public Guid? OrganizationId { get; set; }
        public Organization Organization { get; set; }
        public ICollection<User> Users { get; set; }
    }
}