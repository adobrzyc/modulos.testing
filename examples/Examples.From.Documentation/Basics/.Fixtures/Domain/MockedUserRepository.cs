using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Examples.From.Documentation.Basics.Domain
{
    public class MockedUserRepository : IUserRepository
    {
        private readonly User[] users = {
            new User {Id = 0, Name = "Tom"},
            new User {Id = 1, Name = "John"},
            new User {Id = 2, Name = "Amber"},
            new User {Id = 3, Name = "Grace"}
        };

        public IEnumerator<User> GetEnumerator()
        {
            return users.AsEnumerable()
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return users.GetEnumerator();
        }
    }

    public class Database
    {

    }
}