﻿using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.TeamServices.Samples.Client.Git
{
    [ClientSample(GitWebApiConstants.AreaName, "reverts")]
    public class RevertsSample : ClientSample
    {
        [ClientSampleMethod]
        public GitRevert CreateRevert()
        {
            VssConnection connection = this.Context.Connection;
            GitHttpClient gitClient = connection.GetClient<GitHttpClient>();

            Guid projectId = ClientSampleHelpers.FindAnyProject(this.Context).Id;
            GitRepository repo = GitSampleHelpers.FindAnyRepository(this.Context, projectId);

            // find the latest commit on master
            GitCommitRef latestCommitOnMaster = gitClient.GetCommitsAsync(repo.Id, new GitQueryCommitsCriteria()
            {
                ItemVersion = new GitVersionDescriptor()
                {
                    Version = "master",
                    VersionType = GitVersionType.Branch
                },
                Top = 1
            }).Result.First();

            // generate a unique name to suggest for the branch
            string suggestedBranchName = "refs/heads/vsts-dotnet-samples/" + GitSampleHelpers.ChooseRefsafeName();

            // revert it relative to master
            GitRevert revert = gitClient.CreateRevertAsync(
                new GitAsyncRefOperationParameters()
                {
                    OntoRefName = "refs/heads/master",
                    GeneratedRefName = suggestedBranchName,
                    Repository = repo,
                    Source = new GitAsyncRefOperationSource()
                    {
                        CommitList = new GitCommitRef[] { new GitCommitRef() { CommitId = latestCommitOnMaster.CommitId } }
                    },
                },
                projectId,
                repo.Id).Result;

            Console.WriteLine("Revert {0} created", revert.RevertId);

            // typically, the next thing you'd do is create a PR for this revert
            return revert;
        }
    }
}
