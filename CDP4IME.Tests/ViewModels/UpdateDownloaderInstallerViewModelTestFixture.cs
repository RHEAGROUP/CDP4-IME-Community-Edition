// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginInstallerViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4IME.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net.Http;
    using System.Reactive.Concurrency;
    using System.Reflection;
    using System.Security.AccessControl;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using CDP4Composition.Modularity;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services.AppSettingService;
    using CDP4Composition.Utilities;

    using CDP4IME.Behaviors;
    using CDP4IME.Services;
    using CDP4IME.Settings;
    using CDP4IME.ViewModels;

    using CDP4UpdateServerDal;
    using CDP4UpdateServerDal.Enumerators;

    using DevExpress.Mvvm.Native;

    using Microsoft.Practices.ServiceLocation;

    using Moq;

    using Newtonsoft.Json;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture, Apartment(ApartmentState.STA)]
    public class UpdateDownloaderInstallerViewModelTestFixture : UpdateDownloaderInstallerDataSetup
    {
        private const string PluginName0 = "plugin0";
        private const string PluginName1 = "plugin1";
        private const string PluginName2 = "plugin2";
        private const string Version0 = "0.1.0.0";
        private const string Version1 = "0.2.0.0";
        private IEnumerable<(FileInfo cdp4ckFile, Manifest manifest)> updatablePlugins;
        private Mock<IUpdateDownloaderInstallerBehavior> behavior;
        private Mock<IAssemblyInformationService> assemblyInformationService;
        private Mock<IProcessRunnerService> processRunner;
        private Mock<IDialogNavigationService> dialogNavigation;
        private Mock<IAppSettingsService<ImeAppSettings>> appSettingsService;
        private Mock<IUpdateServerClient> updateServerClient;
        private UpdateDownloaderInstallerViewModel viewModel;

        private Mock<IServiceLocator> serviceLocator;
        private List<Manifest> installedManifest;
        private ProcessorArchitecture processorArchitecture;
        private Platform platform;
        private ImeAppSettings appSettings;
        private List<(string ThingName, string Version)> updateResultFomApi;
        private FileInfo alreadyDownloadedMsi;
        private FileInfo realAlreadyDownloadedplugin0;
        private FileInfo realAlreadyDownloadedplugin1;

        [SetUp]
        public override void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            base.Setup();

            this.updatablePlugins = new List<(FileInfo cdp4ckFile, Manifest manifest)>()
            {
                this.Plugin
            };

            this.behavior = new Mock<IUpdateDownloaderInstallerBehavior>();
            this.behavior.Setup(x => x.Close());

            this.viewModel = new UpdateDownloaderInstallerViewModel(this.updatablePlugins);
            this.appSettings = new ImeAppSettings();

            this.serviceLocator = new Mock<IServiceLocator>();

            this.updateServerClient = new Mock<IUpdateServerClient>();

            this.installedManifest = new List<Manifest>()
            {
                new Manifest() { Name = PluginName0, Version = Version0 },
                new Manifest() { Name = PluginName1, Version = Version0 },
                new Manifest() { Name = PluginName2, Version = Version0 }
            };

            this.processorArchitecture = ProcessorArchitecture.Amd64;
            this.platform = this.processorArchitecture == ProcessorArchitecture.Amd64 ? Platform.X64 : Platform.X86;

            this.updateResultFomApi = new List<(string ThingName, string Version)>()
            {
                (UpdateServerClient.ImeKey, Version1),
                (PluginName0, Version1),
                (PluginName1, Version1),
            };

            this.updateServerClient.
                Setup(x => x.CheckForUpdate(It.IsAny<List<Manifest>>(), It.IsAny<Version>(), It.IsAny<ProcessorArchitecture>())).
                Returns(Task.FromResult<IEnumerable<(string ThingName, string Version)>>(this.updateResultFomApi));
            
            this.updateServerClient.Setup(x => x.DownloadIme(Version1, this.platform)).Returns(Task.FromResult<Stream>(new MemoryStream(Encoding.UTF8.GetBytes("msi"))));
            this.updateServerClient.Setup(x => x.DownloadPlugin(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult<Stream>(new MemoryStream(Encoding.UTF8.GetBytes("plugin"))));

            this.assemblyInformationService = new Mock<IAssemblyInformationService>();
            this.assemblyInformationService.Setup(x => x.GetVersion()).Returns(new Version(Version0));
            this.assemblyInformationService.Setup(x => x.GetProcessorArchitecture()).Returns(this.processorArchitecture);
            this.assemblyInformationService.Setup(x => x.GetLocation()).Returns(this.BasePath);

            this.processRunner = new Mock<IProcessRunnerService>();
            this.processRunner.Setup(x => x.Restart());

            this.dialogNavigation = new Mock<IDialogNavigationService>();
            this.dialogNavigation.Setup(x => x.NavigateModal(It.IsAny<IDialogViewModel>())).Returns(new BaseDialogResult(true));

            this.appSettingsService = new Mock<IAppSettingsService<ImeAppSettings>>();
            this.appSettingsService.Setup(x => x.AppSettings).Returns(this.appSettings);

            this.appSettingsService.Setup(x => x.Save());

            this.serviceLocator.Setup(x => x.GetInstance<IUpdateServerClient>()).Returns(this.updateServerClient.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IAssemblyInformationService>()).Returns(this.assemblyInformationService.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IProcessRunnerService>()).Returns(this.processRunner.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IDialogNavigationService>()).Returns(this.dialogNavigation.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IAppSettingsService<ImeAppSettings>>()).Returns(this.appSettingsService.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IUpdateFileSystemService>()).Returns(this.UpdateFileSystem);

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
        }

        [TearDown]
        public void Teardown()
        {
            if (this.realAlreadyDownloadedplugin0?.Directory?.Exists == true)
            {
                this.realAlreadyDownloadedplugin0.Directory.Delete(true);
            }

            if (this.realAlreadyDownloadedplugin1?.Directory?.Exists == true)
            {
                this.realAlreadyDownloadedplugin1.Directory.Delete(true);
            }

            if (Directory.Exists(this.BasePath))
            {
                try
                {
                    Directory.Delete(this.BasePath, true);
                }
                catch (Exception)
                {
                    File.SetAttributes(this.UpdateFileSystem.UpdateCdp4CkFileInfo.FullName, FileAttributes.Temporary);
                }
            }
        }

        [Test]
        public void VerifyPropertiesAreSet()
        {
            var vm = new UpdateDownloaderInstallerViewModel(this.updatablePlugins);
            Assert.IsNotEmpty(vm.AvailablePlugins);
            Assert.AreSame(vm.UpdatablePlugins, this.updatablePlugins);
            Assert.IsFalse(vm.IsInstallationOrDownloadInProgress);
            Assert.IsNull(vm.CancellationTokenSource);
            Assert.IsFalse(vm.IsInDownloadMode);
            Assert.IsFalse(vm.IsCheckingApi);
            Assert.IsNull(vm.DialogResult);
            Assert.IsNull(vm.LoadingMessage);
            Assert.False(vm.IsBusy);
        }

        [Test]
        public async Task VerifyCheckApi()
        {
            var vm = new UpdateDownloaderInstallerViewModel(false);
            await vm.CheckApiForUpdate().ConfigureAwait(true);
            Assert.AreEqual(vm.DownloadableThings, this.updateResultFomApi);

            this.updateServerClient.Setup(x => x.CheckForUpdate(It.IsAny<List<Manifest>>(), It.IsAny<Version>(), It.IsAny<ProcessorArchitecture>())).Throws(new HttpRequestException(string.Empty));
            await vm.CheckApiForUpdate().ConfigureAwait(true);
            this.dialogNavigation.Verify(x => x.NavigateModal(It.IsAny<IDialogViewModel>()), Times.Once);
            
            this.updateServerClient.Setup(x => x.CheckForUpdate(It.IsAny<List<Manifest>>(), It.IsAny<Version>(), It.IsAny<ProcessorArchitecture>())).Throws(new Exception(string.Empty));
            Assert.DoesNotThrowAsync(() => vm.CheckApiForUpdate()); 
            this.dialogNavigation.Verify(x => x.NavigateModal(It.IsAny<IDialogViewModel>()), Times.Once);
        }

        [Test]
        public async Task VerifyDownload()
        {
            var vm = new UpdateDownloaderInstallerViewModel(true);
            Assert.AreEqual(this.updateResultFomApi, vm.DownloadableThings);
            _ = await vm.DownloadCommand.ExecuteAsyncTask(null);

            vm.AvailablePlugins.ForEach(p => p.IsSelected = true);
            vm.AvailableIme.ForEach(i => i.IsSelected = true);
            Assert.IsTrue(vm.DownloadCommand.CanExecute(null));

            _ = await vm.DownloadCommand.ExecuteAsyncTask(null);
            Thread.Sleep(100);
            var basePath = new DirectoryInfo(this.BasePath);
            var allCdp4Ck = basePath.EnumerateFiles("*.cdp4ck", SearchOption.AllDirectories).Select(f => f.Name).ToList();
            Assert.AreEqual(3, allCdp4Ck.Count());
            Assert.IsNotNull(allCdp4Ck.SingleOrDefault(p => p.Contains(PluginName1)));
            Assert.IsNotNull(allCdp4Ck.SingleOrDefault(p => p.Contains(PluginName0)));
            var msi = basePath.EnumerateFiles("*.msi", SearchOption.AllDirectories).Single();
            Assert.IsTrue(msi.Name.Contains(UpdateServerClient.ImeKey));
            Assert.IsTrue(msi.Name.Contains(Version1));
        }

        [Test]
        public void VerifySelectAllUpdateCheckBoxCommand()
        {
            var vm = new UpdateDownloaderInstallerViewModel(this.updatablePlugins);
            Assert.IsTrue(vm.SelectAllUpdateCheckBoxCommand.CanExecute(null));

            vm.SelectAllUpdateCheckBoxCommand.Execute(null);
            Assert.IsTrue(vm.AvailablePlugins.All(p => p.IsSelected));

            vm.SelectAllUpdateCheckBoxCommand.Execute(null);
            Assert.IsTrue(vm.AvailablePlugins.All(p => !p.IsSelected));

            vm = new UpdateDownloaderInstallerViewModel(true);

            vm.SelectAllUpdateCheckBoxCommand.Execute(null);
            Assert.IsTrue(vm.AvailablePlugins.All(p => p.IsSelected));
        }
        
        [Test]
        public async Task VerifyCancelCommand()
        {
            this.viewModel.Behavior = this.behavior.Object;

            this.viewModel.IsInstallationOrDownloadInProgress = true;
            Assert.IsTrue(this.viewModel.CancelCommand.CanExecute(null));
            await this.viewModel.CancelCommand.ExecuteAsyncTask(null);
            this.behavior.Verify(x => x.Close(), Times.Once);
            this.viewModel.IsInstallationOrDownloadInProgress = false;
        }

        [Test]
        public async Task VerifyInstallCommand()
        {
            this.viewModel.Behavior = this.behavior.Object;
            this.viewModel.AvailablePlugins.First().FileSystem = this.UpdateFileSystem;
            Assert.IsTrue(this.viewModel.InstallCommand.CanExecute(null));
            this.viewModel.AvailablePlugins.First().IsSelected = false;
            await this.viewModel.InstallCommand.ExecuteAsyncTask(null);
            this.behavior.Verify(x => x.Close(), Times.Never);

            this.viewModel.AvailablePlugins.First().IsSelected = true;
            await this.viewModel.InstallCommand.ExecuteAsyncTask(null);
        }
        
        [Test]
        public async Task VerifyFailingInstallCommand()
        {
            this.UpdateFileSystem.UpdateCdp4CkFileInfo.Delete();
            this.viewModel.Behavior = this.behavior.Object;
            this.viewModel.AvailablePlugins.First().FileSystem = this.UpdateFileSystem;
            Assert.IsTrue(this.viewModel.InstallCommand.CanExecute(null));
            this.viewModel.AvailablePlugins.First().IsSelected = true;
            await this.viewModel.InstallCommand.ExecuteAsyncTask(null);
            this.behavior.Verify(x => x.Close(), Times.Never);
        }

        /// <summary>
        /// This test is creating a large file (100MB) so it can have a chance to Invoke the cancellation on the CancellationToken
        /// The Task may fail on some system.
        /// </summary>
        /// <returns>The Task may fail on some system</returns>
        [Test]
        public async Task VerifyCancellationToken()
        {
            this.SetupTestContentForInstallationCancellationPurpose(this.UpdateFileSystem.InstallationPath.FullName);

            using (var largeFile = File.OpenWrite(this.UpdateFileSystem.UpdateCdp4CkFileInfo.FullName))
            {
                largeFile.Seek(100L * 1024 * 1024, SeekOrigin.Begin);
                largeFile.WriteByte(0);
                largeFile.Close();
            }

            this.viewModel.Behavior = this.behavior.Object;
            this.viewModel.AvailablePlugins.First().FileSystem = this.UpdateFileSystem;
            Assert.IsTrue(this.viewModel.InstallCommand.CanExecute(null));
            this.viewModel.AvailablePlugins.First().IsSelected = true;

            await Task.WhenAll(
                this.viewModel.InstallCommand.ExecuteAsyncTask(null),
                this.viewModel.CancelCommand.ExecuteAsyncTask(null));
            
            this.AssertInstalledTestFileHasBeenRestored();
        }

        [Test]
        public void VerifyRestartAfterDownloadCommand()
        {
            var vm = new UpdateDownloaderInstallerViewModel(false);
            Assert.IsTrue(vm.RestartAfterDownloadCommand.CanExecute(null));
            Assert.IsFalse(vm.HasToRestartClientAfterDownload);
            vm.IsInstallationOrDownloadInProgress = false;
            var downloadText = vm.DownloadButtonText;
            vm.RestartAfterDownloadCommand.Execute(null);
            Assert.AreNotEqual(downloadText, vm.DownloadButtonText);
            Assert.IsTrue(vm.HasToRestartClientAfterDownload);
        }

        [Test]
        public async Task VerifyCancelDownload()
        {
            var vm = new UpdateDownloaderInstallerViewModel(true);
            vm.AvailablePlugins.First().IsSelected = true;
            vm.AvailableIme.First().IsSelected = true;
            Assert.IsTrue(vm.DownloadCommand.CanExecute(null));
            await vm.DownloadCommand.ExecuteAsyncTask(null);
            Assert.IsTrue(vm.CancelCommand.CanExecute(null));
            vm.CancelCommand.Execute(null);

            Assert.False(
                this.UpdateFileSystem.PluginDownloadPath.EnumerateFiles().Any(
                    f => f.Name == PluginName0 || f.Name == PluginName1));
        }
        
        [Test]
        public async Task VerifyCheckApiWhenAllHasBeenDownloadedAlready()
        {
            this.alreadyDownloadedMsi = new FileInfo(Path.Combine(this.UpdateFileSystem.ImeDownloadPath.FullName, $"CDP4{UpdateServerClient.ImeKey}-CE.x64-{Version1}.msi"));
            File.Create(this.alreadyDownloadedMsi.FullName).Dispose();

            this.realAlreadyDownloadedplugin0 = this.CreateRealCdp4Ck(PluginName0);
            this.realAlreadyDownloadedplugin1 = this.CreateRealCdp4Ck(PluginName1);
            
            var vm = new UpdateDownloaderInstallerViewModel(false);
            await vm.CheckApiForUpdate().ConfigureAwait(true);
            Assert.IsEmpty(vm.DownloadableThings);
        }

        private FileInfo CreateRealCdp4Ck(string pluginName)
        {
            var cdp4CkFile = new FileInfo(Path.Combine(PluginUtilities.GetDownloadDirectory(true, pluginName).FullName, $"{pluginName}.cdp4ck"));
            
            var manifestBytes = Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(
                    new Manifest() { Name = pluginName, Version = Version1 }));

            using (var plugin0Cdp4CkFile = new FileStream(cdp4CkFile.FullName, FileMode.CreateNew))
            {
                using (var archive = new ZipArchive(plugin0Cdp4CkFile, ZipArchiveMode.Create, true))
                {
                    var entry = archive.CreateEntry($"{pluginName}.plugin.manifest", CompressionLevel.Fastest);

                    using (var entryStream = entry.Open())
                    {
                        entryStream.Write(manifestBytes, 0, manifestBytes.Length);
                    }
                }
            }

            return cdp4CkFile;
        }
    }
}
