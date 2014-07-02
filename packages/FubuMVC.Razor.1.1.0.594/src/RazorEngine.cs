﻿using FubuMVC.Core;
using FubuMVC.Core.Registration;
using FubuMVC.Core.UI;
using FubuMVC.Core.View;
using FubuMVC.Core.View.Model;
using FubuMVC.Core.View.Model.Sharing;
using FubuMVC.Core.View.Rendering;
using FubuMVC.Razor.Rendering;
using FubuMVC.Razor.RazorModel;

namespace FubuMVC.Razor
{
    public class RazorEngineRegistry : IFubuRegistryExtension
    {
        private readonly RazorParsings _parsings = new RazorParsings();
        private readonly TemplateRegistry<IRazorTemplate> _templateRegistry = new TemplateRegistry<IRazorTemplate>();

        void IFubuRegistryExtension.Configure(FubuRegistry registry)
        {
            registry.ViewFacility(new RazorViewFacility(_templateRegistry, _parsings));
            registry.Services(configureServices);

            registry.AlterSettings<CommonViewNamespaces>(x =>
            {
                x.AddForType<RazorViewFacility>(); // FubuMVC.Razor
                x.AddForType<IPartialInvoker>(); // FubuMVC.Core.UI
            });
        }

        private void configureServices(ServiceRegistry services)
        {
            services.ReplaceService<ITemplateRegistry<IRazorTemplate>>(_templateRegistry);
            services.SetServiceIfNone<IRazorTemplateGenerator, RazorTemplateGenerator>();
            services.SetServiceIfNone<ITemplateCompiler, TemplateCompiler>();
            services.SetServiceIfNone<ITemplateFactory, TemplateFactoryCache>();
            services.ReplaceService<IParsingRegistrations<IRazorTemplate>>(_parsings);
            services.SetServiceIfNone<ITemplateDirectoryProvider<IRazorTemplate>, TemplateDirectoryProvider<IRazorTemplate>>();
            services.SetServiceIfNone<ISharedPathBuilder>(new SharedPathBuilder());
            services.SetServiceIfNone<IPartialRenderer, PartialRenderer>();

            var graph = new SharingGraph();
            services.SetServiceIfNone(graph);
            services.SetServiceIfNone<ISharingGraph>(graph);

            services.FillType<ISharedTemplateLocator<IRazorTemplate>, SharedTemplateLocator<IRazorTemplate>>();
            services.FillType<ISharingAttacher<IRazorTemplate>, MasterAttacher<IRazorTemplate>>();
            services.FillType<ITemplateSelector<IRazorTemplate>, RazorTemplateSelector>();
            services.FillType<Bottles.IActivator, SharingAttacherActivator<IRazorTemplate>>();
            services.FillType<IRenderStrategy, AjaxRenderStrategy>();
            services.FillType<IRenderStrategy, DefaultRenderStrategy>();

            services.SetServiceIfNone<IViewModifierService<IFubuRazorView>, ViewModifierService<IFubuRazorView>>();

            services.FillType<IViewModifier<IFubuRazorView>, FubuPartialRendering>();
        }
    }

}