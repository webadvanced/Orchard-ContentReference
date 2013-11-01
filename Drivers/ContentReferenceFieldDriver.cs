using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Contrib.ContentReference.Settings;
using Contrib.ContentReference.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Projections.Services;

namespace Contrib.ContentReference.Drivers
{
    [OrchardFeature("Contrib.ContentReference")]
    public class ContentReferenceFieldDriver : ContentFieldDriver<Fields.ContentReferenceField>
    {
        private readonly IProjectionManager _projectionManager;
        private readonly IContentManager _contentManager;
        public IOrchardServices Services { get; set; }
        private const string TemplateName = "Fields/ContentReference.Edit";

        public ContentReferenceFieldDriver(
            IOrchardServices services,
            IProjectionManager projectionManager,
            IContentManager contentManager
            )
        {
            _projectionManager = projectionManager;
            _contentManager = contentManager;
            Services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private static string GetPrefix(ContentField field, ContentPart part)
        {
            return part.PartDefinition.Name + "." + field.Name;
        }

        private static string GetDifferentiator(Fields.ContentReferenceField field, ContentPart part)
        {
            return field.Name;
        }

        protected override DriverResult Display(ContentPart part, Fields.ContentReferenceField field, string displayType, dynamic shapeHelper)
        {
            var settings = field.PartFieldDefinition.Settings.GetModel<ContentReferenceFieldSettings>();

            var contentItems =
                field.ContentItems
                .Select(content => 
                    new Tuple<string, IContent>(Services.ContentManager.GetItemMetadata(content).DisplayText, content))
                .ToArray();

            return ContentShape("Fields_ContentReference", GetDifferentiator(field, part),
                () => shapeHelper.Fields_ContentReference(DisplayAsLink: settings.DisplayAsLink, ContentField: field, ContentItems: contentItems));
        }

        protected override DriverResult Editor(ContentPart part, Fields.ContentReferenceField field, dynamic shapeHelper)
        {
            var settings = field.PartFieldDefinition.Settings.GetModel<ContentReferenceFieldSettings>();

            var query = _contentManager.ResolveIdentity(new ContentIdentity(settings.QueryIdentifier));

            List<SelectListItem> selectionList = (query == null)
                                    ? new List<SelectListItem>()
                                    : _projectionManager.GetContentItems(query.Id)
                                                        .Select(c => new SelectListItem
                                                            {
                                                                Text = Services.ContentManager.GetItemMetadata(c).DisplayText,
                                                                Value = c.Id.ToString(),
                                                                Selected = field.ContentIds.Contains(c.Id)
                                                            })
                                                        .ToList();

            selectionList.Insert(0, new SelectListItem { Text = "None", Value = "" });

            var model = new ContentReferenceFieldViewModel
            {
                Field = field,
                SelectedContentIds = field.ContentIds,
                SelectedContentId = field.ContentIds.FirstOrDefault(),
                SelectionList = selectionList
            };

            return ContentShape("Fields_ContentReference_Edit", GetDifferentiator(field, part),
                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: GetPrefix(field, part)));
        }

        protected override DriverResult Editor(ContentPart part, Fields.ContentReferenceField field, IUpdateModel updater, dynamic shapeHelper)
        {
            var viewModel = new ContentReferenceFieldViewModel();

            if (updater.TryUpdateModel(viewModel, GetPrefix(field, part), null, null))
            {
                var settings = field.PartFieldDefinition.Settings.GetModel<ContentReferenceFieldSettings>();

                if (viewModel.SelectedContentIds == null || !viewModel.SelectedContentIds.Any())
                {
                    field.ContentIds = viewModel.SelectedContentId.HasValue
                        ? new[] { viewModel.SelectedContentId.Value }
                        : new int[0];
                }
                else
                {
                    field.ContentIds = viewModel.SelectedContentIds;
                }

                if (settings.Required && field.ContentIds.Length == 0)
                {
                    updater.AddModelError("Id", T("The field {0} is mandatory", field.DisplayName));
                }
            }

            return Editor(part, field, shapeHelper);
        }

        protected override void Importing(ContentPart part, Fields.ContentReferenceField field, ImportContentContext context) {
            context.ImportAttribute(field.FieldDefinition.Name + "." + field.Name, "Identifier", identity => {
                field.ContentIds = 
                    identity.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => _contentManager.ResolveIdentity(new ContentIdentity(s)).Id).ToArray();
            });
        }

        protected override void Exporting(ContentPart part, Fields.ContentReferenceField field, ExportContentContext context)
        {
            context.Element(field.FieldDefinition.Name + "." + field.Name)
                .SetAttributeValue("Identifier", string.Join(",", field.ContentIds.Select(i => _contentManager.GetItemMetadata(_contentManager.Get(i)).Identity)));
        }

        protected override void Describe(DescribeMembersContext context)
        {
            context.Member(null, typeof(string), T("ContentIds"), T("The Content Ids referenced by this field."));
        }
    }
}
