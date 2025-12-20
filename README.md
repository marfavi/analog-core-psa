# Analysing Analog Core
Practical Software Analysis 2025

Code coverage reports: https://marfavi.github.io/analog-core-psa/

## How to set up
This project is configured to work within a vscode devcontainer, automatically installing all dependencies in a container which can be easily all removed from the host machine.

To set it up the host machine needs to have docker installed, and we have accessed the containers with vscode and the "Dev Containers" extension.

With docker deamon running, in vscode open command palette and select "Dev Containers: Reopen in Container". The initial open takes a while, it starts the database container and the development container with the fuzzing book library and dotnet runtime. This can use up to 10GB of storage on the host machine (all deleted with the containers).

## Run Fuzzing
First start the Analog Core server in terminal:
```
cd analog-core/coffeecard/CoffeeCard.WebApi/
dotnet run
```
now open fuzz/_swagger_fuzzer.ipynb in vscode and run the cells (if needed select global python kernel), the fuzzer is running!

To collect coverage do the same but use `dotnet-coverage collect -f cobertura -if bin/Debug/net8.0/CoffeeCard.Library.dll -if bin/Debug/net8.0/CoffeeCard.WebApi.dll dotnet bin/Debug/net8.0/CoffeeCard.WebApi.dll` instead of `dotnet run`. After process is stopped the coverage report can be generated with `reportgenerator -reports:output.cobertura.xml -targetdir:/workspace/coverage-report -classfilters:"-CoffeeCard.Library.Migrations.*;-*Mock;-*CoffeeCardContext" -riskhotspotassemblyfilters:"-*"`

To reload database seed without rebuilding container this command can be run (FROM THE HOST MACHINE): `docker exec mssql /usr/local/bin/docker-entrypoint.sh`