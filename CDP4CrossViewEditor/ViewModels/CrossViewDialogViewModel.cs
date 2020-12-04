﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossViewDialogViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
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

namespace CDP4CrossViewEditor.ViewModels
{
    using System;
    using System.Windows.Input;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using ReactiveUI;

    /// <summary>
    /// The view-model to managing cross view workbook output.
    /// </summary>
    public class CrossViewDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Gets the dialog box title
        /// </summary>
        public string DialogTitle { get; private set; }

        /// <summary>
        /// Gets the <see cref="IterationSetup"/>
        /// </summary>
        public Iteration Iteration { get; private set; }

        /// <summary>
        /// Gets the Select <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<object> OkCommand { get; private set; }

        /// <summary>
        /// Gets the Cancel <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<object> CancelCommand { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public ThingSelectorViewModel ElementSelectorViewModel { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public ThingSelectorViewModel ParameterSelectorViewModel { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CrossViewDialogViewModel"/> class.
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that is currently opened
        /// </param>
        public CrossViewDialogViewModel(Iteration iteration)
        {
            this.DialogTitle = "Select equipments and parameters";
            this.Iteration = iteration;

            this.ElementSelectorViewModel = new ThingSelectorViewModel(this.Iteration, ClassKind.ElementBase);
            this.ParameterSelectorViewModel = new ThingSelectorViewModel(this.Iteration, ClassKind.ParameterBase);

            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(_ => this.ExecuteCancel());

            this.OkCommand = ReactiveCommand.Create();
            this.OkCommand.Subscribe(_ => this.ExecuteOk());
        }

        /// <summary>
        /// Executes the <see cref="CancelCommand"/>
        /// </summary>
        private void ExecuteCancel()
        {
            this.DialogResult = new BaseDialogResult(false);
        }

        /// <summary>
        /// Executes the <see cref="OkCommand"/>
        /// </summary>
        private void ExecuteOk()
        {
        }
    }
}
