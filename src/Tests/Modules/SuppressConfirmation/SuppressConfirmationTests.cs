﻿using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AppDomainToolkit;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.SystemModule;
using Shouldly;
using Tests.Artifacts;
using Tests.Modules.SuppressConfirmation.BOModel;
using Xpand.Source.Extensions.XAF.XafApplication;
using Xpand.XAF.Modules.MasterDetail;
using Xpand.XAF.Modules.SuppressConfirmation;
using Xunit;

namespace Tests.Modules.SuppressConfirmation{
//    [Collection(nameof(XafTypesInfo))]
    public class SuppressConfirmationTests : BaseTest{

        [Theory]
        [InlineData(typeof(ListView),Platform.Win)]
        [InlineData(typeof(DetailView),Platform.Win)]
        [InlineData(typeof(ListView),Platform.Web)]
        [InlineData(typeof(DetailView),Platform.Web)]
        internal async Task Signal_When_Window_with_SupressConfirmation_Enabled_ObjectView_changed(Type viewType,Platform platform){
            await RemoteFuncAsync.InvokeAsync(Domain, viewType, platform, async (t, p) => {
                var application = DefaultSuppressConfirmationModule(p).Application;
                var windows = application.WhenSuppressConfirmationWindows().Replay();
                windows.Connect();
                var window = application.CreateWindow(TemplateContext.View, null, true);
                var objectView = application.CreateObjectView(t, typeof(SC));

                window.SetView(objectView);

                await windows.FirstAsync();

                return Unit.Default;
            });

        }

        [Theory]
        [InlineData(Platform.Win)]
        [InlineData(Platform.Web)]
        internal async Task Signal_When_DashboardView_with_SupressConfirmation_Enabled_ObjectView_changed(Platform platform){
            await RemoteFuncAsync.InvokeAsync(Domain, platform, async p => {
                var application = DefaultSuppressConfirmationModule(p).Application;
                var windows = application.WhenSuppressConfirmationWindows().Replay();
                windows.Connect();
                var modelDashboardView = application.Model.NewModelDashboardView(typeof(SC));
                var dashboardView =
                    application.CreateDashboardView(application.CreateObjectSpace(), modelDashboardView.Id, true);
                dashboardView.MockCreateControls();

                var frame = await windows.Take(1);
                frame.ShouldBeOfType<NestedFrame>();
                frame = await windows.Take(1);
                frame.ShouldBeOfType<NestedFrame>();
                return Unit.Default;
            });
        }

        [Theory]
        [InlineData(typeof(ListView),Platform.Win)]
        [InlineData(typeof(DetailView),Platform.Win)]
        [InlineData(typeof(ListView),Platform.Web)]
        [InlineData(typeof(DetailView),Platform.Web)]
        internal void Change_Modification_Handling_Mode(Type viewType,Platform platform){
            RemoteFunc.Invoke(Domain, viewType,platform, (t, p) => {
                var application = DefaultSuppressConfirmationModule(p).Application;
                var windows = application.WhenSuppressConfirmationWindows().Replay();
                windows.Connect();
                var window = application.CreateWindow(TemplateContext.View, null,true);
                var objectView = application.CreateObjectView(t,typeof(SC));
                objectView.CurrentObject = objectView.ObjectSpace.CreateObject(typeof(SC));
                window.SetView(objectView);
                objectView.ObjectSpace.CommitChanges();

                window.GetController<ModificationsController>().ModificationsHandlingMode.ShouldBe((ModificationsHandlingMode) (-1));
                return Unit.Default;
            });
        }


        private static SuppressConfirmationModule DefaultSuppressConfirmationModule(Platform platform){
            var application = platform.NewApplication();
            application.Title = "AutoCommitModule";
            var supressConfirmationModule = new SuppressConfirmationModule();
            supressConfirmationModule.AdditionalExportedTypes.AddRange(new[]{typeof(SC)});
            application.SetupDefaults(supressConfirmationModule);
            
            var modelClassSupressConfirmation = (IModelClassSupressConfirmation) application.Model.BOModel.GetClass(typeof(SC));
            modelClassSupressConfirmation.SupressConfirmation = true;
            return supressConfirmationModule;
        }
    }
}