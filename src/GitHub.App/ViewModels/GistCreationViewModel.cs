﻿using System;
using GitHub.Exports;
using System.ComponentModel.Composition;
using System.Reactive;
using System.Reactive.Linq;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Extensions.Reactive;
using GitHub.Factories;
using GitHub.Primitives;
using GitHub.Services;
using Microsoft.VisualStudio.TextManager.Interop;
using Octokit;
using ReactiveUI;

namespace GitHub.ViewModels
{
    [ExportViewModel(ViewType=UIViewType.Gist)]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class GistCreationViewModel : BaseViewModel, IGistCreationViewModel
    {
        readonly ISelectedTextProvider selectedTextProvider;
        readonly IApiClient apiClient;

        [ImportingConstructor]
        GistCreationViewModel(ISelectedTextProvider selectedTextProvider, IApiClientFactory apiClientFactory)
        {
            Title = Resources.CreateGistTitle;
            this.selectedTextProvider = selectedTextProvider;
            this.apiClient = apiClientFactory.Create(HostAddress.GitHubDotComHostAddress);

            var canCreateGist = this.WhenAny(
                x => x.FileName,
                x => x.Content,
                (x, y) => x.Value.IsNotNullOrEmpty() && y.Value.IsNotNullOrEmpty());

            CreatePublicGist = ReactiveCommand.CreateAsyncObservable(canCreateGist, _ => OnCreateGist(true));
            CreatePrivateGist = ReactiveCommand.CreateAsyncObservable(canCreateGist, _ => OnCreateGist(false));
        }

        private IObservable<Unit> OnCreateGist(bool isPublic)
        {
            selectedTextProvider.GetSelectedText().Select(async selectedText =>
            {
                var newGist = new NewGist
                {
                    Description = Description,
                    Public = isPublic
                };
                newGist.Files.Add(FileName, selectedText);
                await apiClient.CreateGist(newGist);
            });

            return Observable.Return(Unit.Default);
        }

        public IReactiveCommand<Unit> CreatePublicGist { get; }
        public IReactiveCommand<Unit> CreatePrivateGist { get; }
        public string Description { get; }
        public string Content { get; }
        public string FileName { get; }
    }
}
