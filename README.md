# RelayRunner

RelayRunner is intended to facility testing in controlled environments by adding the capability to update load test configs without restarting load clients.

## Prerequisites

- Bash shell (tested on Visual Studio Codespaces, Mac, Ubuntu, Windows with WSL2)
  - Will not work with WSL1 or Cloud Shell
- Azure CLI ([download](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest))
- Docker CLI ([download](https://docs.docker.com/install/))
- .NET 5.0 ([download](https://docs.microsoft.com/en-us/dotnet/core/install/))
- Visual Studio Code (optional) ([download](https://code.visualstudio.com/download))

## Running the System

```bash
make all
```

This will...

- Use k3d to start a new cluster
- Create a local docker registry
- Start with the ngsa-app in in-memory mode as the load test application
- Start with loderunner as the load test client
- Start with a local build of relayrunner backend to manage load test configs and activities
- Start with a local build of relayrunner client to display load test configs and activites
- Start monitoring: prometheus, grafana (not configured yet)
- Start fluentbit
- Start a jumpbox
- Check the available endpoints

## Running the Backend Application

1. Clone the Repo:
      `git clone https://github.com/retaildevcrews/relayrunner.git`

2. Change into the backend directory:
      `cd backend`

3. Run the Application
      `dotnet run`

You should see the following response:
> Hosting environment: Production
Content root path: /workspaces/relayrunner/backend
Now listening on: http://[::]:8080
Application started. Press Ctrl+C to shut down.

## Testing the Backend Application

```bash
 
# test using httpie (installed automatically in Codespaces)
http localhost:8080/api/version
 
# test using curl
curl localhost:8080/api/version
 
```

Stop RelayRunner by typing Ctrl-C or the stop button if run via F5

## Client Application Scripts

In the project directory, you can run:

- `npm clean-install`: Installs npm dependecies.

- `npm start` : Runs the app in development mode

- `npm run lint` : Runs Linter and automatically fixes Linter recommended changes

- `npm test` : Launches the test runner, jest, in the interactive watch mode.

- `npm run build` : Builds the app for production to the `build` folder.

- `npm run eject` : Removes the single build dependency from project and copies all the configuration files and the transitive dependencies (webpack, Babel, ESLint, etc) right into your project so you have full control over them.
  - *Note: this is a one-way operation. Once you `eject`, you can’t go back!*

## Running the Client Application Locally

1. Clone the Repo `git clone https://github.com/retaildevcrews/relayrunner.git`
2. Change into the relayrunner directory `cd relayrunner`
3. Start the k3d cluster `make create`
4. Deploy a pod with relayrunner-backend `make rrapi`
5. Change into the client directory `cd client`
6. Install node dependencies `npm clean-install`
7. Start the client `npm start`

## Testing the Client Application

Run tests on Client components and functions using `npm test`

## Serve the Client Application on NGINX

```bash
// Use k3d to create a cluster
make create
// Build the client in production mode and serve on a pod running nginx
make rrui
```

## CosmosDB Change Feed

### Lease Container

Acts as state storage and coordinates processing the change feed across multiple workers. [Docs](https://docs.microsoft.com/en-us/azure/cosmos-db/change-feed-processor#components-of-the-change-feed-processor)

- Partion key definition must be `/id`. [Docs](https://docs.microsoft.com/en-us/azure/cosmos-db/change-feed-functions#requirements)
- The connection string to Azure Cosmos DB account with lease collection must have write permissions. [Docs](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-cosmosdb-v2-trigger?tabs=csharp#configuration)

## CosmosDB Collections

### clientStatus

Conveys the current status, time of that status, and the associated LoadClient's initial start-up configuration.

- TTL for container is set to no default (-1)
- TTL for clientStatus items is set to 60 seconds

```javascript
{
    "id": "5",
    "partitionKey": "ClientStatus",
    "entityType": "ClientStatus",
    "lastUpdated": "2021-08-17T14:36:37.0897032Z", // in UTC
    "statusDuration": 20, // seconds since last status change
    "status": "Ready", // Starting | Ready | Testing | Terminating
    "message": "Additional status update notes",
    "loadClient": {
        "id": "2",
        "entityType": "LoadClient",
        "version": "0.3.0-717-1030",
        "name": "Central-az-central-us-2",
        "region": "Central",
        "zone": "az-central-us",
        "prometheus": false,
        "startupArgs": "--delay-start -1 --secrets-volume secrets",
        "startTime": "2021-08-17T14:36:37.0897032Z" // in UTC
    }
}
```

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit <https://cla.opensource.microsoft.com>

When you submit a pull request, a CLA bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services.

Authorized use of Microsoft trademarks or logos is subject to and must follow [Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).

Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.

Any use of third-party trademarks or logos are subject to those third-party's policies.
