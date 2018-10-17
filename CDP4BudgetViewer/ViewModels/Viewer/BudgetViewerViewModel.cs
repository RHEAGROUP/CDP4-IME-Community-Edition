﻿// -------------------------------------------------------------------------------------------------
// <copyright file="BudgetViewerViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Budget.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Events;
    using Config;
    using ConfigFile;
    using Microsoft.Practices.ServiceLocation;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using NLog;
    using ReactiveUI;

    public class BudgetViewerViewModel : ModellingThingBrowserViewModelBase, IPanelViewModel
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Backing field for <see cref="CurrentModel"/>
        /// </summary>
        private string currentModel;

        /// <summary>
        /// Backing field for <see cref="CurrentIteration"/>
        /// </summary>
        private int currentIteration;

        /// <summary>
        /// Backing field for <see cref="DomainOfExpertise"/>
        /// </summary>
        private string domainOfExpertise;

        /// <summary>
        /// Backing field for <see cref="BudgetConfig"/>
        /// </summary>
        private BudgetConfig budgetConfig;

        /// <summary>
        /// Backing field for <see cref="ComputationError"/>
        /// </summary>
        private string computationError;

        /// <summary>
        /// The <see cref="IOpenSaveFileDialogService"/> that is used to navigate to the File Open/Save dialog
        /// </summary>
        private readonly IOpenSaveFileDialogService openSaveFileDialogService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BudgetViewerViewModel"/> class
        /// </summary>
        /// <param name="iteration">The associated <see cref="Iteration"/></param>
        /// <param name="session">The session</param>
        /// <param name="thingDialogNavigationService">The thing Dialog Navigation Service</param>
        /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        public BudgetViewerViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService)
            : base(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService)
        {
            this.openSaveFileDialogService = ServiceLocator.Current.GetInstance<IOpenSaveFileDialogService>();

            this.Caption = "Budget Viewer";
            this.ToolTip = string.Format("{0}\n{1}\n{2}", ((EngineeringModel)this.Thing.Container).EngineeringModelSetup.Name, this.Thing.IDalUri, this.Session.ActivePerson.Name);

            this.CurrentModel = this.CurrentEngineeringModelSetup.Name;
            this.CurrentIteration = this.Thing.IterationSetup.IterationNumber;

            var currentDomainOfExpertise = this.QueryCurrentDomainOfExpertise();
            this.DomainOfExpertise = currentDomainOfExpertise == null ? "None" : $"{currentDomainOfExpertise.Name} [{currentDomainOfExpertise.ShortName}]";

            this.OptionOverviewViewModel = new OptionOverviewViewModel();
            this.BudgetViewModels = new ReactiveList<OptionBudgetViewModel>();

            this.OpenConfigCommand = ReactiveCommand.Create();
            this.OpenConfigCommand.Subscribe(_ => this.ExecuteOpenConfigCommand());

            var confObs = this.WhenAnyValue(x => x.BudgetConfig).Select(x => x != null);
            this.RefreshBudgetCommand = ReactiveCommand.Create(confObs);
            this.RefreshBudgetCommand.CanExecuteObservable.Subscribe(_ => this.ComputeBudgets());

            this.SaveConfigCommand = ReactiveCommand.Create(confObs);
            this.SaveConfigCommand.Subscribe(_ => this.ExecuteSaveConfigCommand());

            this.LoadConfigCommand = ReactiveCommand.Create();
            this.LoadConfigCommand.Subscribe(_ => this.ExecuteLoadConfigCommand());
            this.ComputeBudgets();
        }

        #region Header Properties
        /// <summary>
        /// Gets the view model current <see cref="EngineeringModelSetup"/>
        /// </summary>
        public EngineeringModelSetup CurrentEngineeringModelSetup
        {
            get { return this.Thing.IterationSetup.GetContainerOfType<EngineeringModelSetup>(); }
        }

        /// <summary>
        /// Gets the current model caption to be displayed in the browser
        /// </summary>
        public string CurrentModel
        {
            get { return this.currentModel; }
            private set { this.RaiseAndSetIfChanged(ref this.currentModel, value); }
        }

        /// <summary>
        /// Gets the current iteration caption to be displayed in the browser
        /// </summary>
        public int CurrentIteration
        {
            get { return this.currentIteration; }
            private set { this.RaiseAndSetIfChanged(ref this.currentIteration, value); }
        }

        /// <summary>
        /// Gets the current <see cref="DomainOfExpertise"/> name
        /// </summary>
        public string DomainOfExpertise
        {
            get { return this.domainOfExpertise; }
            private set { this.RaiseAndSetIfChanged(ref this.domainOfExpertise, value); }
        }
#endregion

        /// <summary>
        /// Gets the <see cref="BudgetConfig"/>
        /// </summary>
        public BudgetConfig BudgetConfig
        {
            get { return this.budgetConfig; }
            private set { this.RaiseAndSetIfChanged(ref this.budgetConfig, value); }
        }

        /// <summary>
        /// Gets the list of budget view-models which depend on the number of <see cref="Option"/> in the current <see cref="Iteration"/>
        /// </summary>
        public ReactiveList<OptionBudgetViewModel> BudgetViewModels { get; private set; }

        /// <summary>
        /// Gets the <see cref="OptionOverviewViewModel"/>
        /// </summary>
        public OptionOverviewViewModel OptionOverviewViewModel { get; private set; }

        /// <summary>
        /// Gets the open-config <see cref="IReactiveCommand"/>
        /// </summary>
        public ReactiveCommand<object> OpenConfigCommand { get; private set; }

        /// <summary>
        /// Gets the refresh <see cref="IReactiveCommand"/>
        /// </summary>
        public ReactiveCommand<object> RefreshBudgetCommand { get; private set; }

        /// <summary>
        /// Gets the save config <see cref="IReactiveCommand"/>
        /// </summary>
        public ReactiveCommand<object> SaveConfigCommand { get; private set; }

        /// <summary>
        /// Gets the load config <see cref="IReactiveCommand"/>
        /// </summary>
        public ReactiveCommand<object> LoadConfigCommand { get; private set; }

        /// <summary>
        /// Gets the computation error message
        /// </summary>
        public string ComputationError
        {
            get { return this.computationError; }
            private set { this.RaiseAndSetIfChanged(ref this.computationError, value); }
        }

        protected override void ExecuteOpenAnnotationWindow(ModellingAnnotationItem annotation)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Handle the <see cref="ObjectChangedEvent"/>
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);

            if (this.BudgetConfig != null)
            {
                this.ComputeBudgets();
            }
        }

        /// <summary>
        /// Computes the budget
        /// </summary>
        private void ComputeBudgets()
        {
            this.BudgetViewModels.Clear();

            if (this.BudgetConfig == null)
            {
                return;
            }

            foreach (Option option in this.Thing.Option)
            {
                var budgetViewModel = new OptionBudgetViewModel(option, this.BudgetConfig, this.Session);
                this.BudgetViewModels.Add(budgetViewModel);

                foreach (var budgetSummaryViewModel in budgetViewModel.BudgetSummary)
                {
                    budgetSummaryViewModel.WhenAnyValue(x => x.SummaryTotal).Skip(1).Subscribe(_ => this.PopulateOptionOverview());
                }
            }

            this.PopulateOptionOverview();
        }

        /// <summary>
        /// Populate the option-overview
        /// </summary>
        private void PopulateOptionOverview()
        {
            this.OptionOverviewViewModel.ClearView();
            this.OptionOverviewViewModel.GenerateColumn(this.BudgetConfig.Elements);
            foreach (var optionBudgetViewModel in this.BudgetViewModels)
            {
                this.OptionOverviewViewModel.AddRecord(optionBudgetViewModel.Option, optionBudgetViewModel.BudgetSummary.ToDictionary(x => x.RootElement, x => x.SummaryTotal));
            }
        }

        /// <summary>
        /// Executes the <see cref="OpenConfigCommand"/>
        /// </summary>
        private void ExecuteOpenConfigCommand()
        {
            var vm = new BudgetConfigViewModel(this.Thing, this.BudgetConfig);
            var result = (BudgetConfigDialogResult)this.DialogNavigationService.NavigateModal(vm);

            if (result != null && result.Result.HasValue && result.Result.Value)
            {
                this.BudgetConfig = result.BudgetConfig;
            }
        }

        /// <summary>
        /// Executes the <see cref="SaveConfigCommand"/>
        /// </summary>
        private void ExecuteSaveConfigCommand()
        {
            var configFilePath = this.openSaveFileDialogService.GetSaveFileDialog("budget-config", "json", "JSON (.json)|*.json", "", 1);
            if (string.IsNullOrWhiteSpace(configFilePath))
            {
                return;
            }

            using (var outputFile = new StreamWriter(configFilePath))
            {
                var serializer = new JsonSerializer
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Include
                };
                serializer.Serialize(outputFile, this.BudgetConfig.ToDto());
            }
        }

        /// <summary>
        /// Executes the <see cref="LoadConfigCommand"/>
        /// </summary>
        private void ExecuteLoadConfigCommand()
        {
            var result = this.openSaveFileDialogService.GetOpenFileDialog(true, true, false, "JSON(.json) | *.json", ".json", "", 1);
            if (result == null || result.Length != 1)
            {
                return;
            }

            var filePath = result.Single();

            try
            {
                BudgetConfigDto fileconfig;

                using (var filestream = new FileStream(filePath, FileMode.Open))
                using (var sr = new StreamReader(filestream))
                using (var reader = new JsonTextReader(sr))
                {
                    var serializer = new JsonSerializer
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        NullValueHandling = NullValueHandling.Include
                    };
                    serializer.Converters.Add(new BudgetConfigJsonConverter());
                    fileconfig = serializer.Deserialize<BudgetConfigDto>(reader);
                }

                var vm = new BudgetConfigViewModel(this.Thing, fileconfig);
                var configResult = (BudgetConfigDialogResult)this.DialogNavigationService.NavigateModal(vm);

                if (configResult != null && configResult.Result.HasValue && configResult.Result.Value)
                {
                    this.BudgetConfig = configResult.BudgetConfig;
                }
            }
            catch (Exception e)
            {
                logger.Error("An exception occurred when reading the configuration file {0}", e.Message);
            }
        }

        /// <summary>
        /// Queries the current <see cref="DomainOfExpertise"/> from the session for the current <see cref="Iteration"/>
        /// </summary>
        /// <returns>
        /// The <see cref="DomainOfExpertise"/> if selected, null otherwise.
        /// </returns>
        private DomainOfExpertise QueryCurrentDomainOfExpertise()
        {
            var iterationDomainPair = this.Session.OpenIterations.SingleOrDefault(x => x.Key == this.Thing);
            if (iterationDomainPair.Equals(default(KeyValuePair<Iteration, Tuple<DomainOfExpertise, Participant>>)))
            {
                return null;
            }

            return iterationDomainPair.Value?.Item1;
        }
    }
}
