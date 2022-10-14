using System;
using CommandLine;

namespace CodeOwnersParser
{
    public class ActionInputs
    {
        string _repositoryName = null!;

        public ActionInputs()
        {

        }

        [Option('o', "owner",
            Required = true,
            HelpText = "The owner, for example: \"dotnet\". Assign from `github.repository_owner`.")]
        public string Owner { get; set; } = null!;

        [Option('n', "name",
            Required = true,
            HelpText = "The repository name, for example: \"samples\". Assign from `github.repository`.")]
        public string Name
        {
            get => _repositoryName;
            set => ParseAndAssign(value, str => _repositoryName = str);
        }

        [Option('w', "workspace",
            Required = false,
            HelpText = "The workspace directory, or repository root directory. Use `/github/workspace`.",
            Default = "/github/workspace")]
        public string WorkspaceDirectory { get; set; } = null!;

        [Option('t', "timeout",
            Required = false,
            HelpText = "Timeout for Github API calls in milliseconds.",
            Default = 10000)]
        public int timeout { get; set; } = 0;

        [Option('l', "fileLimit",
            Required = false,
            HelpText = "Maximum amount of files to process. PRs with more files will be skipped.",
            Default = 1000)]
        public int fileLimit { get; set; } = 0;

        [Option('k', "token",
            Required = false,
            HelpText = "Token used for Github API calls.")]
        public string token { get; set; } = null!;

        [Option('f', "file",
            Required = false,
            HelpText = "Relative path to the codeowners file.",
            Default = "/.github/CODEOWNERS")]
        public string file { get; set; } = null!;

        [Option('i', "pullID",
           Required = true,
           HelpText = "ID of the PR. Assign from `github.event_path.pull_request.number`.")]
        public int pullID { get; set; } = 0;

        [Option('d', "separator",
           Required = false,
           HelpText = "String used to seperate multiple owners in the output.",
           Default = " ")]
        public string separator { get; set; } = null!;

        [Option('p', "prefix",
           Required = false,
           HelpText = "Will be prefixed to the output, useful if output is used in comments action. Also used for finding existing mentions.",
           Default = "")]
        public string prefix { get; set; } = null!;

        [Option('s', "sufix",
           Required = false,
           HelpText = "Will be prefixed to the output, useful if output is used in comments action. Also used for finding existing mentions.",
           Default = "")]
        public string sufix { get; set; } = null!;

        [Option('b', "botname",
           Required = false,
           HelpText = "If set existing comments of this user will be parsed to find already mentioned users.")]
        public string botname { get; set; } = null!;

        static void ParseAndAssign(string? value, Action<string> assign)
        {
            if (value is { Length: > 0 } && assign is not null)
            {
                assign(value.Split("/")[^1]);
            }
        }
    }
}
