namespace Bot.Services
{
    public class ResourceReader
    {
        private readonly IConfigurationRoot _resource;

        public ResourceReader(string resourcePath)
        {
            _resource = InitConfigurationRoot(resourcePath);
        }

        public string this[string key] => _resource.GetSection(key).Value;

        private IConfigurationRoot InitConfigurationRoot(string resourcePath)
        {
            ConfigurationBuilder builder = new();

            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile(resourcePath);

            return builder.Build();
        }
    }
}
