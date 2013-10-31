﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Environment.Extensions;

namespace Contrib.ContentReference.Settings {
    [OrchardFeature("Contrib.ContentReference")]
    public class ContentReferenceFieldEditorEvents : ContentDefinitionEditorEventsBase {
        private readonly IContentManager _contentManager;

        public ContentReferenceFieldEditorEvents(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (definition.FieldDefinition.Name == "ContentReferenceField") {
                var model = definition.Settings.GetModel<ContentReferenceFieldSettings>();
                model.QueryList = _contentManager.Query("Query").List()
                    .Select(c => {
                        var contentItemMetadata = _contentManager.GetItemMetadata(c);

                        return new SelectListItem {
                            Text = contentItemMetadata.DisplayText,
                            Value = c.Id.ToString(),
                            Selected = c.Id == model.QueryId
                        };
                    }).ToList();
                yield return DefinitionTemplate(model);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.FieldType != "ContentReferenceField") {
                yield break;
            }

            var model = new ContentReferenceFieldSettings();
            if (updateModel.TryUpdateModel(model, "ContentReferenceFieldSettings", null, null)) {
                if (model.QueryId != null)
                {
                    var queryIdentifier = _contentManager.GetItemMetadata(_contentManager.Get(model.QueryId.Value)).Identity.ToString();
                    builder.WithSetting("ContentReferenceFieldSettings.QueryIdentifier", queryIdentifier);
                }

                builder.WithSetting("ContentReferenceFieldSettings.DisplayAsLink", model.DisplayAsLink.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("ContentReferenceFieldSettings.Required", model.Required.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("ContentReferenceFieldSettings.Multiple", model.Multiple.ToString(CultureInfo.InvariantCulture));
            }

            yield return DefinitionTemplate(model);
        }
    }
}
