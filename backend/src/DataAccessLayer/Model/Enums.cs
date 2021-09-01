// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace RelayRunner.Model.Enum
{
    public enum EntityType
    {
        Client,
        LoadClient,
        ClientStatus,
        LoadTestConfig,
        TestRun,
    }

    public enum Status
    {
        Starting,
        Ready,
        Testing,
        Terminating,
    }
}
