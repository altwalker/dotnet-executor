# .NET Executor for AltWalker


The .NET Executor offers a convenient programming interface for exposing and executing your C# tests with AltWalker. To utilize the .NET Executor effectively, you'll need to create a console application and run the `ExecutorService` to make your tests accessible to AltWalker.

For executing your .NET tests with AltWalker, use the `Altwalker.Executor`. To get started, follow the [AltWalker C# Quickstart](https://altwalker.github.io/altwalker/quickstart.html) tutorial.

Read the full documentation on <https://altwalker.github.io/altwalker>.

## Usage

To use the .NET Executor, you must create a console application and initiate the `ExecutorService` to expose your tests to AltWalker. Your console application should contain a `Main` method that registers your models and launches the executor service, like so:

```csharp
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

For a more in-depth example, you can explore the sample in [`AltwalkerExecutor.Example/`](https://github.com/altwalker/dotnet-executor/tree/main/AltwalkerExecutor.Example).

## Setting Up a Development Environment

### Run Tests

```bash
dotnet test AltwalkerExecutor.Tests/
```

### Run the service locally

To start the web service on your local machine, execute the following command:

```bash
dotnet run --project path/to/project.csproj --server.urls=http://localhost:5000
```

To start the web server from the example found in `AltwalkerExecutor.Example/`:

```bash
dotnet run --project AltwalkerExecutor.Example/altwalkerexecutor.example.csproj --server.urls=http://localhost:5000
```

You can run the following commands to check that the service started:

```bash
curl -sv http://localhost:5000/altwalker/hasModel?name=WalletModel
```

```bash
curl -sv http://localhost:5000/altwalker/hasStep?modelName=WalletModel&name=setUpModel
```

## Support

Join our [Gitter chat room](https://gitter.im/altwalker/community) or our [Google Group](https://groups.google.com/g/altwalker) to chat with us or with other members of the community.

## License

This project is licensed under the [GNU General Public License v3.0](LICENSE).
