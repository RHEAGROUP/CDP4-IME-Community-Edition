﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DependentParameterTypeAssignmentRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Nathanael Smiechowski, Ahmed Ahmed.
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels.Dialogs.Rows
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;

    /// <summary>
    /// The row view model for the <see cref="DependentParameterTypeAssignment"/>
    /// </summary>
    public class DependentParameterTypeAssignmentRowViewModel : ParameterTypeAssignmentRowViewModel<DependentParameterTypeAssignment>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DependentParameterTypeAssignmentRowViewModel"/> class.
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/> represented by the row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The parent Row</param>
        public DependentParameterTypeAssignmentRowViewModel(DependentParameterTypeAssignment thing, ISession session, SampledFunctionParameterTypeDialogViewModel containerViewModel) : base(thing, session, containerViewModel)
        {
        }
    }
}
