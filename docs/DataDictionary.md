# Data Dictionary for RelayRunner API

![License](https://img.shields.io/badge/license-MIT-green.svg)

## 1. Introduction

This document is meant to list and define the entities in use in RelayRunner.

### 1.1 Primary Entities Used

We have defined 5 types of data for the RelayRunner API (RRAPI).  Those are as follows:

| Type Name       |  Description    |  Notes                             |     RRAPI  | LodeRunner |
| :-------------- | :-------------- | :--------------------------------- | :----------| :----------|
| ClientStatus    | This object is used to convey the state of any given LodeRunner client that is configured to use the same data store. | Status documents will be placed in the dabase by LodeRunner and read by RRAPI.  A TTL of **1 minute will** be given to the records so that if the client doesn't regulary update status will not be visible to the RRAPI or the RelayRunner UI (RRUI). | xRxx | CRUD |
| LoadClient      | Information about the LodeRunner instance | | xRxx | CRUD |
| Config          | This is used to define certain aspects of a LodeRunner client's execution context. Including the location of the files used for testing. | | CRUD | xRxx |
| LoadTest        | This type will hold the target(s) of a test, the Config object to be used, and which clients should be used for executing the tests. | | CRUD | xRUx |
| TestRun         | This is the point in time copy of a load test that serves as a historical record.  It will contain a LoadResults object and have a reference to it's original LoadTest. | | CRUD | xRUx |
| LoadResults     | This is the summary information from each client of used in a TestRun and will be a subobject of TestRun || CRUD | CRUx |

`Table 01: Primary Relay Runner Entities`

## 2. Entity Definitions

### 2.1 BaseEntity

This entity is the parent of several objects and defines common fields

| Property        |      Type       | Description                        | Notes      |
| :-------------- | :-------------- | :--------------------------------- | :----------|
| PartitionKey    |     String      | Calculated value used for CosmosDB to determine how to allocate and use partitions  | Initial implementation will use `EntityType` to keep all objects of a similar type in the same partition |
| EntityType      |     String      | Entity type used for filtering  | |

`Table 02: Base Definition for Data Entities`

### 2.2 LoadClient and ClientStatus

#### 2.2.1 Example ClientStatus Flow
<!-- markdownlint-disable MD033 -->
<!-- couldn't get sizing to work in standard markdown -->
<img src="diagrams/out/ClientStatus Flow.svg" /> <!-- width="800" height="600"/> -->
<!-- ![ParsingController Sequence](images/sequence-ParsingController.png) -->

`Figure 01: LodeRunner Client Start-up Sequence`

#### 2.2.2 LoadClient

This is an object that represents an instance of LodeRunner and it's initial start-up configuration.

##### LoadClient

| Property        |      Type       | Description                        | Notes      |
| :-------------- | :-------------- | :--------------------------------- | :----------|
| PartitionKey    |     String      | | In the current model this value will always be empty for LoadClient and not stored |
| EntityType      |     String      | Entity type used for filtering  | |
| Version         |     String      | Version of LodeRunner being used   | | 
| Id              |     String      | Unique Id generated at start-up to differentiate clients located in the same Region and Zone | |
| Region          |     String      | The region in which the client is deployed | |
| Zone            |     String      | The zone in which the client is deployed | |
| Prometheus      |     Boolean     | Indicates whether or not this instance of LodeRunner is providing Prometheus metrics | |
| StartupArgs     |     String      | String of arguments passed to LodeRunner at start-up | |
| StartTime       |     DateTime    | The date and time this instance was started | |

`Table 03: Load Client Properties`

#### 2.2.3 ClientStatus Definition

This object is primarily for conveying the curent status, time of that status, and the `LoadClient` settings to consuming apps.  It inherits from `BaseEntity` and contains a `LoadClient` member.

##### ClientStatus

| Property        |      Type       | Description                        | Notes      |
| :-------------- | :-------------- | :--------------------------------- | :----------|
| PartitionKey    |     String      | | This value should be populated for `ClientStatus` objects and documents |
| EntityType      |     String      | Entity type used for filtering  | |
| LastUpdated     |     DateTime    | This shows the date and time the status was last updated | 
| StateDuration   |     Int         | The number of seconds since the last change in state for the client | |
| Message         |     string      | Additional information conveyed as part of the status update | |
| LoadClient      |   `LoadClient`  | A nested object holding the information about the particular client in this status message | |

`Table 04: ClientStatus Properties`

### 2.3 LoadTestConfig and TestRun

These are used for configuring a testing scenario.  `LoadTestConfig` will contain the settings that dictate what will be tested, with which files, and in what manner.  `TestRun` is used to schedule work with load clients and contains a `LoadTestConfg` and a list of LoadClients to use.