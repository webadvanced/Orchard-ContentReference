using System.Linq;
using Contrib.ContentReference.Fields;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;

namespace Contrib.ContentReference.Handlers {
    [OrchardFeature("Contrib.ContentReference")]
    public class ContentReferenceHandler : ContentHandler {
        private readonly IAuthenticationService _authenticationService;
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentReferenceHandler(
            IAuthenticationService authenticationService,
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager) {

            T = NullLocalizer.Instance;

            _authenticationService = authenticationService;
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;

            //OnActivated<ContentPart>(PropertySetHandlers);
            //OnLoading<ContentPart>((context, part) => LazyLoadHandlers(part));
            //OnVersioning<ContentPart>((context, part, newVersionPart) => LazyLoadHandlers(newVersionPart));
        }

        public Localizer T { get; set; }

        protected override void Activated(ActivatedContentContext context) {
            foreach(var part in context.ContentItem.Parts) {
                PropertySetHandlers(context, part, _contentManager);
            }
        }

        protected override void Loading(LoadContentContext context) {
            foreach (var part in context.ContentItem.Parts) {
                LazyLoadHandlers(part);
            }
        }

        protected override void Versioning(VersionContentContext context) {
            foreach (var part in context.BuildingContentItem.Parts) {
                LazyLoadHandlers(part);
            }
        }

        protected void LazyLoadHandlers(ContentPart part) {
            // add handlers that will load content for id's just-in-time
            foreach (ContentReferenceField field in part.Fields.Where(f => f.FieldDefinition.Name.Equals("ContentReferenceField"))) {
                var field1 = field;
                field.ContentItemField.Loader(item => field1.ContentIds.Select(i => _contentManager.Get(i)));   
            }
        }

        protected static void PropertySetHandlers(ActivatedContentContext context, ContentPart part, IContentManager contentManager) {
            // add handlers that will update ID when ContentItem is assigned
            foreach (ContentReferenceField field in part.Fields.Where(f => f.FieldDefinition.Name.Equals("ContentReferenceField"))) {
                var field1 = field;
                field1.ContentItemField.Setter(contentItems => {
                    var contentItemsArray = contentItems.ToArray();
                    field1.ContentIds = contentItemsArray.Select(content => content.Id).ToArray();
                    return contentItemsArray;
                });

                // Force call to setter if we had already set a value
                if (field1.ContentItemField.Value != null)
                    field1.ContentItemField.Value = field1.ContentItemField.Value;
            }
        }
    }
}
