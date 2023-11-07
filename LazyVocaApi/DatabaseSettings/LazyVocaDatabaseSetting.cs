namespace LazyVocaApi.DatabaseSettings
{
    public class LazyVocaDatabaseSetting
    {
        public string ConnectionString { get; set; } = null!;

        public string DatabaseName { get; set; } = null!;

        public string UsersCollectionName { get; set; } = null!;

        public string VocabulariesCollectionName { get; set; } = null!;
    }
}
