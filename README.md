# .NET Executor for AltWalker

The .NET Executor provides a programming interface for exposing and executing your C# tests with AltWalker, to use the .NET Executor you need to create a console application and run the `ExecutorService` to expose your tests to AltWalker.

Use `Altwalker.Executor` to execute your .NET tests with AltWalker. Follow [AltWalker C# Quickstart](https://altwalker.github.io/altwalker/quickstart.html) tutorial to get started.

Read the full documentation on https://altwalker.github.io/altwalker.

## Usage

You need to create a console application and run the `ExecutorService` to expose your tests to AltWalker.

Your console application needs to have a `Main` that registers your models and starts the executor service:

```c#
public class Program {

    public static void Main(string[] args) {
        ExecutorService service = new ExecutorService();

        // You need to register every model
        service.RegisterModel<WalletModel>();
        service.RegisterSetup<Setup>();

        // Start the executor service
        service.Run(args);
    }
}
```

For a more detailed example you can check the example from `AltwalkerExecutor.Example/`.

## Setting Up a Development Environment

### Run Tests

```
$ dotnet test AltwalkerExecutor.Tests/
```

### Run the service locally

To start the web service locally run:

```
$ dotnet run --project path/to/project.csproj --server.urls=http://localhost:5000
```

You can start the web server with the example from `AltwalkerExecutor.Exampele/`:

```
$ dotnet run --project AltwalkerExecutor.Example/altwalkerexecutor.example.csproj --server.urls=http://localhost:5000
```

You can run the following commands to check that the service started:

```
$ curl -sv http://localhost:5000/altwalker/hasModel?name=WalletModel
```

```
$ curl -sv http://localhost:5000/altwalker/hasStep?modelName=WalletModel&name=setUpModel
```

## Support

Join our [Gitter chat room](https://gitter.im/altwalker/community) or our [Google Group](https://groups.google.com/g/altwalker) to chat with us or with other members of the community.

## License

This project is licensed under the [GNU General Public License v3.0](LICENSE).
