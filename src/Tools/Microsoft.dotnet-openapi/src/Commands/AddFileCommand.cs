// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Tools.Internal;

namespace Microsoft.DotNet.OpenApi.Commands
{
    internal class AddFileCommand : BaseCommand
    {
        private const string CommandName = "file";

        private const string SourceFileArgName = "source-file";

        public AddFileCommand(AddCommand parent, HttpClient httpClient)
            : base(parent, CommandName, httpClient)
        {
            _sourceFileArg = Argument(SourceFileArgName, $"The OpenAPI file to add. This must be a path to local OpenAPI file(s)", multipleValues: true);
        }

        internal readonly CommandArgument _sourceFileArg;

        private readonly string[] ApprovedExtensions = new[] { ".json", ".yaml", ".yml" };

        protected override async Task<int> ExecuteCoreAsync()
        {
            var projectFilePath = ResolveProjectFile(ProjectFileOption);

            Ensure.NotNullOrEmpty(_sourceFileArg.Value, SourceFileArgName);

            foreach (var sourceFile in _sourceFileArg.Values)
            {
                var codeGenerator = CodeGenerator.NSwagCSharp;
                await EnsurePackagesInProjectAsync(projectFilePath, codeGenerator);
                if (IsLocalFile(sourceFile))
                {
                    if (!ApprovedExtensions.Any(e => sourceFile.EndsWith(e)))
                    {
                        await Warning.WriteLineAsync($"The extension for the given file '{sourceFile}' should have been one of: {string.Join(",", ApprovedExtensions)}.");
                        await Warning.WriteLineAsync($"The reference has been added, but may fail at build-time if the format is not correct.");
                    }
                    AddServiceReference(OpenApiReference, projectFilePath, sourceFile);
                }
                else
                {
                    throw new ArgumentException($"{SourceFileArgName} of '{sourceFile}' was not valid. Valid values are a JSON file or a YAML file");
                }
            }

            return 0;
        }

        private bool IsLocalFile(string file)
        {
            return File.Exists(GetFullPath(file));
        }

        protected override bool ValidateArguments()
        {
            Ensure.NotNullOrEmpty(_sourceFileArg.Value, SourceFileArgName);
            return true;
        }
    }
}