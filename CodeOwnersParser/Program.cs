﻿using System;
using System.Linq;
using CommandLine;
using static CommandLine.Parser;
using CodeOwnersParser;
using System.Collections.Generic;
using Octokit;
using System.Threading.Tasks;

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

    string output = String.Join(inputs.separator, ownersWithModifiedFiles);

    Console.WriteLine($"Owners with file changes: {output}");
    Environment.SetEnvironmentVariable("GITHUB_OUTPUT", Environment.GetEnvironmentVariable("GITHUB_OUTPUT") + "\n" + $"owners={output}");
    Console.WriteLine($"Output: {inputs.prefix + output + inputs.sufix}");
    Environment.SetEnvironmentVariable("GITHUB_OUTPUT", Environment.GetEnvironmentVariable("GITHUB_OUTPUT") + "\n" + $"owners-formatted={inputs.prefix + output + inputs.sufix}");
    Console.WriteLine("GITHUB_OUTPUT content: " + Environment.GetEnvironmentVariable("GITHUB_OUTPUT"));
}

