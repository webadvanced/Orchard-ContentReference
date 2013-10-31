using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage;
using Orchard.ContentManagement.Utilities;
using Orchard.Environment.Extensions;

namespace Contrib.ContentReference.Fields {
    [OrchardFeature("Contrib.ContentReference")]
    public class ContentReferenceField : ContentField {
        private static readonly char[] separator = new[] { '{', '}', ',' };
        private readonly LazyField<IEnumerable<IContent>> _contentItems = new LazyField<IEnumerable<IContent>>();

        public LazyField<IEnumerable<IContent>> ContentItemField { get { return _contentItems; } }

        public int[] ContentIds {
            get { return DecodeIds(Storage.Get<string>()); }
            set { Storage.Set(EncodeIds(value)); }
        }

        public IEnumerable<IContent> ContentItems {
            get { return _contentItems.Value; }
        }

        private string EncodeIds(ICollection<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return string.Empty;
            }

            // use {1},{2} format so it can be filtered with delimiters
            return "{" + string.Join("},{", ids.ToArray()) + "}";
        }

        private int[] DecodeIds(string ids)
        {
            if (String.IsNullOrWhiteSpace(ids))
            {
                return new int[0];
            }

            return ids.Split(separator, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
        }
    }
}
