using System;
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
            string title = field.Identifier != null ?
                Services.ContentManager.GetItemMetadata(field.ContentItem).DisplayText :
                string.Empty;

            return ContentShape("Fields_ContentReference", GetDifferentiator(field, part),
                () => shapeHelper.Fields_ContentReference(DisplayAsLink: settings.DisplayAsLink, ContentField: field, Title: title));
        }

        protected override DriverResult Editor(ContentPart part, Fields.ContentReferenceField field, dynamic shapeHelper)
        {
            var settings = field.PartFieldDefinition.Settings.GetModel<ContentReferenceFieldSettings>();
            var query = _contentManager.ResolveIdentity(new ContentIdentity(settings.QueryIdentifier));
            var contentItems = _projectionManager.GetContentItems(query.Id)
                .Select(c =>
                {
                    var contentItemMetadata = Services.ContentManager.GetItemMetadata(c);
                    var contentIdentity = contentItemMetadata.Identity.ToString();

                    return new SelectListItem
                    {
                        Text = contentItemMetadata.DisplayText,
                        Value = contentIdentity,
                        Selected = field.Identifier == contentIdentity
                    };
                })
                .ToList();

            contentItems.Insert(0, new SelectListItem { Text = "None", Value = "" });

            var model = new ContentReferenceFieldViewModel
            {
                Field = field,
                ItemList = new SelectList(contentItems, "Value", "Text", field.Identifier)
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

                if (settings.Required && viewModel.Identifier == null)
                {
                    updater.AddModelError(GetPrefix(field, part), T("The field {0} is mandatory.", T(field.DisplayName)));
                }

                field.Identifier = viewModel.Identifier;
            }

            return Editor(part, field, shapeHelper);
        }

        protected override void Importing(ContentPart part, Fields.ContentReferenceField field, ImportContentContext context)
        {
            context.ImportAttribute(field.FieldDefinition.Name + "." + field.Name, "Identifier", v => field.Identifier = v);
        }

        protected override void Exporting(ContentPart part, Fields.ContentReferenceField field, ExportContentContext context)
        {
            context.Element(field.FieldDefinition.Name + "." + field.Name)
                .SetAttributeValue("Identifier", field.Identifier ?? String.Empty);
        }

        protected override void Describe(DescribeMembersContext context)
        {
            context.Member(null, typeof(int), T("Identifier"), T("The Identifier referenced by this field."));
        }
    }
}
