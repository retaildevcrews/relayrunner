# Data Dictionary for RelayRunner API

![License](https://img.shields.io/badge/license-MIT-green.svg)

## 1. Introduction

TODO: Place short description with namespace and project where class definitions exist.

### 1.1 Base Entity Design

We have defined 5 types of data for the RelayRunner API (RRAPI).  Those are as follows:

| Type Name       |  Description    |  Notes                             |     RRAPI  | LodeRunner |
| :-------------- | :-------------- | :--------------------------------- | :----------| :----------|
| ClientStatus    | This object is used to convey the state of any given LodeRunner client that is configured to use the same data store. | Status documents will be placed in the dabase by LodeRunner and read by RRAPI.  A TTL of **1 minute will** be given to the records so that if the client doesn't regulary update status will not be visible to the RRAPI or the RelayRunner UI (RRUI). | xRxx | CRUD |
| Config          | This is used to define certain aspects of a LodeRunner client's execution context. Including the location of the files used for testing. | CRUD | xRxx |
| LoadTest        | This type will hold the target(s) of a test, the Config object to be used, and which clients should be used for executing the tests. | CRUD | xRUx |
| TestRun         | This is the point in time copy of a load test that serves as a historical record.  It will contain a LoadResults object and have a reference to it's original LoadTest. | CRUD | xRUx |
| LoadResults     | This is the summary information from each client of used in a TestRun and will be a subobject of TestRun | CRUD | CRUx |


<!-- markdownlint-disable MD033 -->
<!-- couldn't get sizing to work in standard markdown -->
<img src="images/sequence-ParsingController.png" width="800" height="600"/>
<!-- ![ParsingController Sequence](images/sequence-ParsingController.png) -->

`Figure 01: ParsingController Sequence`

There are 3 types of adapters used by the ParsingController:

- **IRecordIdentifier** - reads an individual record and returns a record type identifier if the text is recognized by the component
- **IRecordParser** - parses a single record for a given type identifier
- **ILogSerializer** - responsible for converting output from a parser to an outbound format
  
Upon instantiation, the ParsingController will attempt to configure an App Insights TelemetryClient, read its configuration, and pre-load the configured RecordIdentifiers. Parsers and Serializers are not pre-loaded as there is no certainty that all of the ones configured will be needed during the execution of any given instance.  Instead a cache-aside mechanism is used which may be enabled or disabled via the `enableAdapterCache` setting.  The ParsingController has 3 different constructors: one for construction without App Insights integration and two for App Insights integration.  The parameters are as follows:

| Arg             |      Type       | Description                        | Constructor|
| :-------------- | :-------------- | :--------------------------------- | :----------|
| logger          |     ILogger     | Used to report log information based on system conditions. | all
| configFile      |     string      | The path to the configuration file used by ParsingController (ParsingControllerConfig.json).  It will look in the local folder if not specified at run-time. | all
| adapterConfig   |     string      | The root directory to look for all adapter configs.  This is passed to adapters during construction. | all
| deviceId        |     string      | Used to identify the host that is processing the records | all
| correlationId   |     string      | Used to associate events, metrics, and logs that are reported in a given execution | all
| telemetryClient | TelemetryClient | Used to report custom metrics from the ParsingController | TelemetryClient
| appInsightsKey  |     string      | Used to instantiate a TelemetryClient | Telemetry config|

`Table 01: ParsingController Constructor Arguments`