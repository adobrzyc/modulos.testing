using System;

namespace SimpleDomain.Db.Model
{
    public class User
    {
		public Guid UserId { get; set; }
		public string Name { get; set; }
		public string SName { get; set; }

        public Guid DepartmentId { get; set; }
        public Department Department { get; set; }
    }
}