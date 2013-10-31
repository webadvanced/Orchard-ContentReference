using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Contrib.ContentReference.Fields;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Tokens;

namespace Contrib.ContentReference.Providers {
    [OrchardFeature("Contrib.ContentReference")]
    public class ContentReferenceToken : ITokenProvider {
        private readonly IWorkContextAccessor _workContextAccessor;

        private Localizer T { get; set; }

        public ContentReferenceToken(IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;

            T = NullLocalizer.Instance;
        }

        public void Describe(DescribeContext context) {
            context.For("ContentReferenceField", T("Content Reference Field"), T("Tokens for Content Reference Fields"))
                   .Token("ContentItems", T("Content Items"), T("The content items referenced."))
                   .Token("ContentItems[:*]", T("Content Items"), T("A content items referenced by its index. Can be chained with Content tokens."));
        }

        public void Evaluate(EvaluateContext context) {
            context.For<ContentReferenceField>("ContentReferenceField")
                .Token(
                       token => token.StartsWith("ContentItems:", StringComparison.OrdinalIgnoreCase) ? token.Substring("ContentItems:".Length) : null,
                       (token, t) =>
                       {
                           var index = Convert.ToInt32(token);
                           return index + 1 > t.ContentIds.Count() ? null : t.ContentItems.ElementAt(index);
                       })
                .Chain("ContentItems", "Content", field => field.ContentItems)
                .Chain("ContentItems:0", "Content", t => t.ContentItems.ElementAt(0))
                .Chain("ContentItems:1", "Content", t => t.ContentItems.ElementAt(1))
                .Chain("ContentItems:2", "Content", t => t.ContentItems.ElementAt(2))
                .Chain("ContentItems:3", "Content", t => t.ContentItems.ElementAt(3));
        }
    }
}
