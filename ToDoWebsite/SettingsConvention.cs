using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FubuCore;
using FubuCore.Configuration;
using StructureMap;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;
using StructureMap.Pipeline;

namespace ToDoWebsite
{
    public class SettingsConvention : IRegistrationConvention
    {
        public void Process(Type type, Registry registry)
        {
            if (type.Name.EndsWith("Settings") && type.IsConcreteWithDefaultCtor())
            {
                registry.For(type).Use(typeof(SettingsInstance<>).CloseAndBuildAs<Instance>(type));
            }
        }
    }

    public class SettingsInstance<T> : Instance where T : class, new()
    {
        protected override string getDescription()
        {
            return "{0} from {1}".ToFormat(typeof(T).Name, typeof(AppSettingsProvider).Name);
        }

        protected override object build(Type pluginType, BuildSession session)
        {
            return session.GetInstance<AppSettingsProvider>().SettingsFor<T>();
        }
    }
}