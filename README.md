# RelayRunner App

RelayRunner App is intended to faciliate testing in controlled environments by adding the capability to update configs and load tests without restarting load clients.

## Prerequisites

- Bash shell (tested on Visual Studio Codespaces, Mac, Ubuntu, Windows with WSL2)
  - Will not work with WSL1 or Cloud Shell
- Azure CLI ([download](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest))
- Docker CLI ([download](https://docs.docker.com/install/))
- .NET 5.0 ([download](https://docs.microsoft.com/en-us/dotnet/core/install/))
- Visual Studio Code (optional) ([download](https://code.visualstudio.com/download))

## Backend

### Backend Usage

TODO

### Run the Backend Application

#### Using bash shell for the Backend

> This will work from a terminal in Visual Studio Codespaces as well

1. Clone the Repo:
      `git clone https://github.com/retaildevcrews/relayrunner.git`

2. Change into the backend directory:
      `cd backend`

3. Run the Application

> The default is to run in memory mode, which allows us to run the application without setting up the rest of the supporting infrastrucutre

```bash

# run the application
dotnet run
```

You should see the following response:
> Hosting environment: Production
Content root path: /workspaces/relayrunner/backend
Now listening on: http://[::]:8080
Application started. Press Ctrl+C to shut down.

#### Testing the Backend Application

Open a new bash shell

> Visual Studio Codespaces allows you to open multiple shells by clicking on the `Split Terminal` icon

```bash

# test the application

# test using httpie (installed automatically in Codespaces)
http localhost:8080/api/loadClients

# test using curl
curl localhost:8080/api/loadClients

```

Stop RelayRunner by typing Ctrl-C or the stop button if run via F5

## Client

### Client Usage

#### Available Scripts

In the project directory, you can run:

- `npm clean-install`: Installs npm dependecies.

- `npm start` : Runs the app in development mode

- `npm run lint` : Runs Linter and automatically fixes Linter recommended changes

- `npm test` : Launches the test runner, jest, in the interactive watch mode.

- `npm run build` : Builds the app for production to the `build` folder.

- `npm run eject` : Removes the single build dependency from project and copies all the configuration files and the transitive dependencies (webpack, Babel, ESLint, etc) right into your project so you have full control over them.
  - *Note: this is a one-way operation. Once you `eject`, you can’t go back!*

### Run the Client UI

#### Using bash shell for Client

> This will work from a terminal in Visual Studio Codespaces as well

1. Clone the Repo
      `git clone https://github.com/retaildevcrews/relayrunner.git`

2. Change into the client directory
      `cd client`

3. Install node dependencies
      `npm clean-install`

4. Run the client UI
      `npm start`

#### Testing the Client

Run tests on Client components and functions using `npm test`

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit <https://cla.opensource.microsoft.com>

When you submit a pull request, a CLA bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services.

Authorized use of Microsoft trademarks or logos is subject to and must follow [Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).

Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.

Any use of third-party trademarks or logos are subject to those third-party's policies.