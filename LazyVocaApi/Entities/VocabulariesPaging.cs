using LazyVocaApi.Models;

namespace LazyVocaApi.Entities
{
    public class VocabulariesPaging
    {
        public long TotalRecords { get; set; }

        public int TotalPages { get; set; }

        public int CurrentPage { get; set; }

        public int PageSize {get;set;}
        
        public IEnumerable<Vocabulary >? Vocabularies { get; set; }
    }
}
