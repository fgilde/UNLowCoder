namespace UNLowCoder.SourceGen
{
    internal class NamespaceResolver(
        string originFilePath,
        string fallBackRootNamespace,
        NamespaceResolver.OptionsTryGetFunc optionsGetterFunc)
    {
        public delegate bool OptionsTryGetFunc(string key, out string? value);

        public string? Resolve()
        {
            if (!optionsGetterFunc("build_property.rootnamespace", out var rootNamespace))
                rootNamespace = fallBackRootNamespace;

            if (optionsGetterFunc("build_property.projectdir", out var projectDir) && projectDir != null)
            {
                var fromPath = this.EnsurePathEndsWithDirectorySeparator(projectDir);
                var toPath = this.EnsurePathEndsWithDirectorySeparator(Path.GetDirectoryName(originFilePath));
                var relativPath = this.GetRelativePath(fromPath, toPath);

                return $"{rootNamespace}.{relativPath.Replace(Path.DirectorySeparatorChar, '.')}";
            }

            return rootNamespace;
        }

        private string GetRelativePath(string fromPath, string toPath)
        {
            var relativeUri = new Uri(fromPath).MakeRelativeUri(new(toPath));
            return Uri.UnescapeDataString(relativeUri.ToString())
                .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        private string EnsurePathEndsWithDirectorySeparator(string path) 
            => path.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
    }
}
