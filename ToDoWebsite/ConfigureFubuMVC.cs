using FubuMVC.Core;
using FubuPersistence.RavenDb;
using StructureMap.Configuration.DSL;
using ToDoWebsite.Behaviors;
using ToDoWebsite.ToDo;

namespace ToDoWebsite
{
    public class ConfigureFubuMVC : FubuRegistry
    {
        public ConfigureFubuMVC()
        {
            // As is, this will be using all the default conventions and policies
            Routes.HomeIs<LoginInputModel>();
            
            Policies.Add<RavenSessionPolicy>();

            
        }
    }

    public class StructureMapRegistry : Registry
    {
        public StructureMapRegistry()
        {
            Scan(s =>
            {
                s.TheCallingAssembly();
                s.AssemblyContainingType<RavenDbSettings>();
                s.Convention<SettingsConvention>();
            });
        }
    }
}