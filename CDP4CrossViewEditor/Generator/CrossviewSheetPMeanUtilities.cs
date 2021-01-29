﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossviewSheetPMeanUtilities.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Adrian Chivu, Cozmin Velciu, Alex Vorobiev
//
//    This file is part of CDP4-Server-Administration-Tool.
//    The CDP4-Server-Administration-Tool is an ECSS-E-TM-10-25 Compliant tool
//    for advanced server administration.
//
//    The CDP4-Server-Administration-Tool is free software; you can redistribute it and/or modify
//    it under the terms of the GNU Affero General Public License as
//    published by the Free Software Foundation; either version 3 of the
//    License, or (at your option) any later version.
//
//    The CDP4-Server-Administration-Tool is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CrossViewEditor.Generator
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Helper class that is required for computing value of P_mean parameter
    /// </summary>
    internal static class CrossviewSheetPMeanUtilities
    {
        /// <summary>
        /// Hardcoded redundancy.scheme
        /// NOTE: not sure about this value
        /// </summary>
        private const string RedundancySchemeHot = "Active";

        /// <summary>
        /// Hardcoded redundancy.scheme
        /// NOTE: not sure about this value
        /// </summary>
        private const string RedundancySchemeCold = "Passive";

        /// <summary>
        /// Hardcoded redundancy.type
        /// </summary>
        private const string RedundancyTypeInternal = "Internal";

        /// <summary>
        /// Hardcoded redundancy.type
        /// </summary>
        private const string RedundancyTypeExternal = "External";

        /// <summary>
        /// Check if P_mean parameter calculation is possible
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>True if all power related parameters are present, False otherwise</returns>
        public static bool IsCalculationPossible(List<ParameterOrOverrideBase> parameters)
        {
            // Check if all power required parameters are present
            if (CrossviewSheetConstants.PowerParameters.Except(new[] { "P_peak" }).Except(parameters.Select(p => p.ParameterType.ShortName)).Any())
            {
                return false;
            }

            var pDutyCycle = parameters.Where(p => p != null)
                .FirstOrDefault(p => p.ParameterType.ShortName.Equals("P_duty_cyc", StringComparison.InvariantCultureIgnoreCase));
            var pMean = parameters.Where(p => p != null)
                .FirstOrDefault(p => p.ParameterType.ShortName.Equals("P_mean", StringComparison.InvariantCultureIgnoreCase));

            if (pDutyCycle == null || pMean == null)
            {
                return false;
            }

            // Check if P_duty_cyc && P_mean have the same option dependency

            if (pDutyCycle.IsOptionDependent != pMean.IsOptionDependent)
            {
                return false;
            }

            if (pDutyCycle.IsOptionDependent &&
                pMean.IsOptionDependent &&
                pDutyCycle.ValueSets.Select(vs => vs.ActualOption).Except(pMean.ValueSets.Select(vs => vs.ActualOption)).Any())
            {
                return true;
            }

            // Check if P_duty_cyc && P_mean have the same state dependency, both should be

            if (pDutyCycle.StateDependence == null || pMean.StateDependence == null)
            {
                return false;
            }

            if (pDutyCycle.StateDependence.ActualState.Count == 0 || pMean.StateDependence.ActualState.Count == 0)
            {
                return false;
            }

            return !pDutyCycle.StateDependence.ActualState.Except(pMean.StateDependence.ActualState).Any();
        }

        /// <summary>
        /// Calculate P_mean parameter value based on multiple parameter values:
        /// "P_stby", "P_on", "P_duty_cyc", "redundancy.scheme", "redundancy.type", "redundancy.k", "redundancy.n"
        /// </summary>
        public static double? ComputeCalculation(string standByValue, string onValue, string dutyCycleValue, string redundancyScheme, string redundancyType, string redundancyK, string redundancyN)
        {
            double.TryParse(standByValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var pStandByValue);
            double.TryParse(onValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var pOnValue);
            double.TryParse(dutyCycleValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var pDutyCycleValue);
            int.TryParse(redundancyK, NumberStyles.Any, CultureInfo.InvariantCulture, out var pRedundancyKValue);
            int.TryParse(redundancyN, NumberStyles.Any, CultureInfo.InvariantCulture, out var pRedundancyNValue);

            if (pDutyCycleValue == -1)
            {
                return 0;
            }

            if (pDutyCycleValue < 0 || pDutyCycleValue > 1)
            {
                return null;
            }

            switch (redundancyType)
            {
                case RedundancyTypeInternal:
                    return pDutyCycleValue * pOnValue + (1 - pDutyCycleValue) * pStandByValue;

                case RedundancyTypeExternal:
                    switch (redundancyScheme)
                    {
                        case RedundancySchemeCold:
                            return (pDutyCycleValue * pOnValue + (1 - pDutyCycleValue) * pStandByValue) * pRedundancyKValue / pRedundancyNValue;

                        case RedundancySchemeHot:
                            return pDutyCycleValue * pOnValue + (1 - pDutyCycleValue) * pStandByValue;

                        default:
                            return null;
                    }

                default:
                    return null;
            }
        }
    }
}