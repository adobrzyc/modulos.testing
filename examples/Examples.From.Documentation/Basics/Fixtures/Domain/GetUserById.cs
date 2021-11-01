namespace Examples.From.Documentation.Basics.Domain
{
    using System.Linq;

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