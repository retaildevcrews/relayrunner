# Data Dictionary for RelayRunner API

![License](https://img.shields.io/badge/license-MIT-green.svg)

## 1. Introduction

This document is meant to list and define the entities in use in RelayRunner.

### 1.1 Primary Entities Used

We have defined 5 types of data for the RelayRunner API (RRAPI).  Those are as follows:

| Type Name       |  Description    |  Notes                             |     RRAPI  | LodeRunner |
| :-------------- | :-------------- | :--------------------------------- | :----------| :----------|
| ClientStatus    | This object is used to convey the state of any given LodeRunner client that is configured to use the same data store. | Status documents will be placed in the dabase by LodeRunner and read by RRAPI.  A TTL of **1 minute will** be given to the records so that if the client doesn't regulary update status will not be visible to the RRAPI or the RelayRunner UI (RRUI). | xRxx | CRUD |
| Config          | This is used to define certain aspects of a LodeRunner client's execution context. Including the location of the files used for testing. | | CRUD | xRxx |
| LoadTest        | This type will hold the target(s) of a test, the Config object to be used, and which clients should be used for executing the tests. | | CRUD | xRUx |
| TestRun         | This is the point in time copy of a load test that serves as a historical record.  It will contain a LoadResults object and have a reference to it's original LoadTest. | | CRUD | xRUx |
| LoadResults     | This is the summary information from each client of used in a TestRun and will be a subobject of TestRun || CRUD | CRUx |
`Table 01: Primary Relay Runner Entities`

## 2. Entity Definitions

### 2.1 ClientStatus

#### 2.1.1 Example ClientStatus Flow
<!-- markdownlint-disable MD033 -->
<!-- couldn't get sizing to work in standard markdown -->
<img src="diagrams/out/ClientStatus Flow.svg" /> <!-- width="800" height="600"/> -->
<!-- ![ParsingController Sequence](images/sequence-ParsingController.png) -->

`Figure 01: LodeRunner Client Start-up Sequence`

#### 2.1.2 ClientStatus Definition

TODO: Add any necessary description

| **ClientStatus** | | | | 
| Property        |      Type       | Description                        | Constructor|
| :-------------- | :-------------- | :--------------------------------- | :----------|
| logger          |     ILogger     | Used to report log information based on system conditions. | all
| configFile      |     string      | The path to the configuration file used by ParsingController (ParsingControllerConfig.json).  It will look in the local folder if not specified at run-time. | all
| adapterConfig   |     string      | The root directory to look for all adapter configs.  This is passed to adapters during construction. | all
| deviceId        |     string      | Used to identify the host that is processing the records | all
| correlationId   |     string      | Used to associate events, metrics, and logs that are reported in a given execution | all
| telemetryClient | TelemetryClient | Used to report custom metrics from the ParsingController | TelemetryClient
| appInsightsKey  |     string      | Used to instantiate a TelemetryClient | Telemetry config|

