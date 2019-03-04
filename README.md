# Altwalker 

AltWalker is an open source, Model-based testing framework for automating your test execution. You design your tests as a directional graph and AltWalker executes them. It relies on [Graphwalker](http://graphwalker.github.io/) to generate paths through your tests graph.

To execute your dotnet tests using altwalker you can use Altwalker.Executor package. You need to register your models and start the webservice.

# Quickstart

* Create a simple console application

`dotnet new console`

* Add Altwalker.Executor  package reference

`dotnet add package Altwalker.Executor`

* Register your models in `Program.cs`

`ExecutorService service = new ExecutorService();`  
`service.RegisterModel<ExampleModel>();`

* Start the service 

`service.Start(args);` - args param

* Run altwalker online:

`altwalker online -x dotnet path/to/console/project/ -m path/to/model.json "random(edge_coverage(100))"`

`altwalker online -x dotnet path/to/console/project/app.dll -m path/to/model.json "random(edge_coverage(100))"`
