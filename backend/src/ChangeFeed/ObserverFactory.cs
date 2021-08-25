// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing;

namespace RelayRunner.Application.ChangeFeed
{
    public class ObserverFactory : IChangeFeedObserverFactory
    {
        private IChangeFeedObserver CustomObserver { get; set; }

        public ObserverFactory(IChangeFeedObserver observer)
        {
            CustomObserver = observer;
        }

        public IChangeFeedObserver CreateObserver()
        {
            return CustomObserver;
        }
    }
}
