using System.Linq;

namespace Examples.From.Documentation.Basics.Domain
{
    public class GetUserById : IGetUserById
    {
        private readonly IUserRepository userRepository;

        public GetUserById(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public User Execute(int id)
        {
            return userRepository.SingleOrDefault(e => e.Id == id);
        }
    }
}