﻿// -------------------------------------------------------------------------------------------------
// <copyright file="HtmlReportGenerator.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels.HtmlReport
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Requirements.Utils;
    using DotLiquid;
    using DotLiquid.NamingConventions;

    /// <summary>
    /// the purpose of the <see cref="HtmlReportGenerator"/> is to generated an HTML report
    /// </summary>
    public class HtmlReportGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlReportGenerator"/> class.
        /// </summary>
        public HtmlReportGenerator()
        {
            Template.NamingConvention = new CSharpNamingConvention();
            this.RegisterTypes();
        }

        /// <summary>
        /// Creates a <see cref="List{Thing}"/> that contains <see cref="RequirementsSpecification"/> and its containing 
        /// <see cref="RequirementsGroup"/> and <see cref="Requirement"/>
        /// </summary>
        /// <param name="requirementsSpecification"></param>
        /// <returns>
        /// A <see cref="List{Thing}"/> ordered by <see cref="IBreadCrumb"/>
        /// </returns>
        private IEnumerable<Thing> CreateSortedContent(RequirementsSpecification requirementsSpecification)
        {
            var things = new List<Thing>();

            things.Add(requirementsSpecification);
            var allGroups = requirementsSpecification.GetAllContainedGroups();
            things.AddRange(allGroups);
            things.AddRange(requirementsSpecification.Requirement);

            var contentComparer = new RequirementsSpecificationContentComparer();
            things.Sort(contentComparer);

            return things;
        }

        /// <summary>
        /// Renders a <see cref="RequirementsSpecification"/> as HTML report
        /// </summary>
        /// <returns>
        /// an HTML string that represents the <see cref="RequirementsSpecification"/> in HTML
        /// </returns>
        public string Render(RequirementsSpecification requirementsSpecification)
        {
            if (requirementsSpecification == null)
            {
                throw new ArgumentNullException("requirementsSpecification", "The RequirementsSpecification may not be null");
            }

            var htmlTemplate = this.ReadEmbeddedTemplate();
            var template = Template.Parse(htmlTemplate);
            
            var sortedContent = this.CreateSortedContent(requirementsSpecification);

            var htmlReport = template.Render(DotLiquid.Hash.FromAnonymousObject(new { content = sortedContent }));

            return htmlReport;
        }

        /// <summary>
        /// Writes the report to disk
        /// </summary>
        /// <param name="report">
        /// The HTML report as a string
        /// </param>
        /// <param name="path">
        /// the path where the HTML report needs to be written to
        /// </param>
        public void Write(string report, string path)
        {
            System.IO.File.WriteAllText(path, report, Encoding.UTF8);
        }

        /// <summary>
        /// Registeres the required types with the <see cref="Template"/> as safe types
        /// </summary>
        private void RegisterTypes()
        {
            Template.RegisterSafeType(typeof(IterationSetup), new string[] { "Description", "IterationNumber", "ClassKind", "Iid" } );
            Template.RegisterSafeType(typeof(Iteration), new string[] { "Relationship", "IterationSetup", "RuleVerificationList", "RevisionNumber", "ClassKind", "Iid" });
            Template.RegisterSafeType(typeof(BinaryRelationship), new string[] { "Category", "ClassKind", "Container", "Target", "Source", "Iid" });
            Template.RegisterSafeType(typeof(MultiRelationship), new string[] { "Category", "ClassKind", "Container", "RelatedThing", "Iid" });
            Template.RegisterSafeType(typeof(RequirementsSpecification), new string[] { "Category", "Owner", "ShortName", "Name", "Requirement", "Group", "RevisionNumber", "Container", "ClassKind", "Iid" });
            Template.RegisterSafeType(typeof(Requirement), new string[] { "Category", "Owner", "ShortName", "Name", "Definition", "Group", "Container", "ClassKind", "Iid", "IsDeprecated" });
            Template.RegisterSafeType(typeof(RequirementsGroup), new string[] { "Category", "Owner", "ShortName", "Name", "Definition", "Group", "Container", "ClassKind" });
            Template.RegisterSafeType(typeof(Category), new string[] { "ShortName", "Name", "Container", "ClassKind" });
            Template.RegisterSafeType(typeof(Definition), new string[] { "Content", "LanguageCode", "Container", "ClassKind" });
            Template.RegisterSafeType(typeof(DomainOfExpertise), new string[] { "ShortName", "Name", "Container", "ClassKind" });
            Template.RegisterSafeType(typeof(ElementDefinition), new string[] { "ShortName", "Name", "Container", "ClassKind" });
            Template.RegisterSafeType(typeof(ElementUsage), new string[] { "ShortName", "Name", "Container", "ClassKind" });
            Template.RegisterSafeType(typeof(ClassKind), ck => ck.ToString());
        }

        /// <summary>
        /// reads the embedded HTML template from a resource
        /// </summary>
        /// <returns>
        /// A string with the contents of the html template
        /// </returns>
        private string ReadEmbeddedTemplate()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "CDP4Requirements.Resources.DotLiquidTemplate.RequirementsSpecificationReportTemplate.liquid";

            var stream = assembly.GetManifestResourceStream(resourceName);
            
            using (StreamReader reader = new StreamReader(stream))
            {
                var result = reader.ReadToEnd();
                return result;
            }   
        }
    }
}
