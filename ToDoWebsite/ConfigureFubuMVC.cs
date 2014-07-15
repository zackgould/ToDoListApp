using FubuMVC.Core;
using ToDoWebsite.ToDo;

namespace ToDoWebsite
{
    public class ConfigureFubuMVC : FubuRegistry
    {
        public ConfigureFubuMVC()
        {
            // As is, this will be using all the default conventions and policies
            Routes.HomeIs<LoginInputModel>();
            
        }
    }
}