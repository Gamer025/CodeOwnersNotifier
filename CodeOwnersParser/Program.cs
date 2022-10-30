using System;
using System.Linq;
using CommandLine;
using static CommandLine.Parser;
using CodeOwnersParser;
using System.Collections.Generic;
using Octokit;
using System.IO;

var parser = Default.ParseArguments<ActionInputs>(() => new(), args);
parser.WithNotParsed(
    errors =>
    {
        Console.Write(errors);
        Environment.Exit(2);
    });

parser.WithParsed(options => NotifyOwners(options));

static void NotifyOwners(ActionInputs inputs)
{
    Console.WriteLine($"CodeOwnersParser Version {System.Reflection.Assembly.GetEntryAssembly().GetName().Version}");
    Console.WriteLine($"Parsing codeowner file at: {inputs.WorkspaceDirectory}{inputs.file}");
    Dictionary<string, List<string>> codeowners = Helpers.ParseCodeownersFile(inputs.WorkspaceDirectory + inputs.file);

    GitHubClient ghclient = new GitHubClient(new ProductHeaderValue("CodeOwnersParser"));
    ghclient.SetRequestTimeout(TimeSpan.FromMilliseconds(inputs.timeout));
    if (!String.IsNullOrEmpty(inputs.token))
        ghclient.Credentials = new Credentials(inputs.token);

    PullRequest PR = ghclient.PullRequest.Get(inputs.Owner, inputs.Name, inputs.pullID).Result;
    if (PR.ChangedFiles > inputs.fileLimit)
    {
        Console.WriteLine($"PR exceeded file limit. Limit: {inputs.fileLimit} files, PR files: {PR.ChangedFiles}");
        Environment.Exit(1);
    }

    Console.WriteLine($"Getting PR files for PR with ID {inputs.pullID}");
    var batchPagination = new ApiOptions
    {
        PageSize = 100
    };
    var modifiedFilesTask = ghclient.PullRequest.Files(inputs.Owner, inputs.Name, inputs.pullID, batchPagination);
    List<Octokit.PullRequestFile> modifiedFiles;
    if (modifiedFilesTask.Wait(inputs.timeout))
    {
        modifiedFiles = new List<PullRequestFile>(modifiedFilesTask.Result);
    }
    else
    {
        throw new TimeoutException("Timeout while getting PR files");
    }

    List<string> ownersWithModifiedFiles = Helpers.GetOwnersWithModifiedFiles(codeowners, modifiedFiles);
    //If we were provided a botname parse its comments and find already notifed owners
    if (inputs.botname is not null)
    {
        var PRcommentsTask = ghclient.Issue.Comment.GetAllForIssue(inputs.Owner, inputs.Name, inputs.pullID);
        List<Octokit.IssueComment> PRcomments;
        if (PRcommentsTask.Wait(inputs.timeout))
        {
            PRcomments = new List<IssueComment>(PRcommentsTask.Result);
        }
        else
        {
            throw new TimeoutException("Timeout while getting PR comments");
        }

        List<string> notifiedOwners = Helpers.GetMentionedOwners(PRcomments, inputs.botname, inputs.prefix, inputs.sufix, inputs.separator);
        ownersWithModifiedFiles = ownersWithModifiedFiles.Except(notifiedOwners).ToList();
    }

    string owners = String.Join(inputs.separator, ownersWithModifiedFiles);
    string[] output = { $"owners={owners}", $"owners-formatted={inputs.prefix + owners + inputs.sufix}"};
    Console.WriteLine($"Owners: {output[0]}");
    Console.WriteLine($"Owners-formatted: {output[1]}");
    File.WriteAllLines(Environment.GetEnvironmentVariable("GITHUB_OUTPUT"), output);
}

