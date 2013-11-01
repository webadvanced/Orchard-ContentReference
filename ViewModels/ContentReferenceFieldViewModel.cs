using System.Collections.Generic;
using System.Web.Mvc;
using Contrib.ContentReference.Fields;

namespace Contrib.ContentReference.ViewModels {
    public class ContentReferenceFieldViewModel {
        public ContentReferenceField Field { get; set; }

        public int[] SelectedContentIds { get; set; }
        public int? SelectedContentId { get; set; }

        public List<SelectListItem> SelectionList { get; set; }
    }
}
