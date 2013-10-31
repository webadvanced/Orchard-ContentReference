using System.Collections.Generic;
using System.Web.Mvc;

namespace Contrib.ContentReference.Settings {
    public class ContentReferenceFieldSettings {
        public bool DisplayAsLink { get; set; }
        public bool Required { get; set; }
        public bool Multiple { get; set; }

        public string QueryIdentifier { get; set; }

        public IList<SelectListItem> QueryList { get; set; }
    }
}
