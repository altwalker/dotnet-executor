# Altwalker 

AltWalker is an open source, Model-based testing framework for automating your test execution. You design your tests as a directional graph and AltWalker executes them. It relies on [Graphwalker](http://graphwalker.github.io/) to generate paths through your tests graph.

Use Altwalker.Executor to execute your dotnet tests with altwalker. Follow [altwalker C# quickstart](https://altom.gitlab.io/altwalker/altwalker/quickstart.html#c-quickstart) tutorial to get started.


# Run Altwalker.Executor locally

Run tests

`dotnet test AltwalkerExecutor.Tests/`


Start web service

`dotnet run --project AltwalkerExecutor.Example/altwalkerexecutor.example.csproj --server.urls=http://localhost:5001`


`curl -sv http://localhost:5001/altwalker/hasModel?name=WalletModel`
`curl -sv http://localhost:5001/altwalker/hasStep?modelName=WalletModel&name=setUpModel`

