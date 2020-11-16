using Autofac;
using SimpleDomain.Logic;

namespace SimpleDomain.Modules
{
    public class RegisterDependencies : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder.RegisterType<GetUsersOfDepartment>().InstancePerDependency();
            builder.RegisterType<AddUserFunctionality>().InstancePerDependency();
            builder.RegisterType<GetUsersFromOrganization>().InstancePerDependency();
        }
    }
}