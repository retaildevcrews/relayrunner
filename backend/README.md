# RelayRunner Backend

## Usage

TODO

## Run the Application

### Using bash shell

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

### Testing the Application

Open a new bash shell

> Visual Studio Codespaces allows you to open multiple shells by clicking on the `Split Terminal` icon

```bash

# test the application

# test using httpie (installed automatically in Codespaces)
http localhost:8080/api/clients

# test using curl
curl localhost:8080/api/clients

```

Stop RelayRunner by typing Ctrl-C or the stop button if run via F5