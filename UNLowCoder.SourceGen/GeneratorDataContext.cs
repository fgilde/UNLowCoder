﻿using Microsoft.CodeAnalysis;
using UNLowCoder.Core.Data;

namespace UNLowCoder.SourceGen
{
    internal class GeneratorDataContext  
    {
        public string OriginFilePath { get; set; }
        public string Namespace { get; set; }
        public IReadOnlyList<UnLocodeCountry> Countries { get; set; }
        public string GeneratedFileName { get; set; }
        public string GeneratedClassName { get; set; }

        public GeneratorDataContext(AdditionalText originFile, List<UnLocodeCountry> countries, NamespaceResolver namespaceResolver)
        {            
            this.OriginFilePath = originFile.Path;
            this.Countries = countries;            
            this.GeneratedClassName = Path.GetFileNameWithoutExtension(originFile.Path);
            this.GeneratedFileName = $"{this.GeneratedClassName}.g.cs";
            this.Namespace = namespaceResolver.Resolve();
        }
    }
}
