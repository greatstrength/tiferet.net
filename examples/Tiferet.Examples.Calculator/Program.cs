using System.CommandLine;
using Tiferet.Blueprints;
using Tiferet.Contexts;
using Tiferet.Domain;
using Tiferet.Events;
using Tiferet.Domain.Error;

// Config directory relative to the binary output location.
var configDir = Path.Combine(AppContext.BaseDirectory, "app", "configs");

// Demo mode: run hardcoded test cases (mirrors Python basic_calc.py).
if (args.Length == 0 || (args.Length == 1 && args[0] == "--demo"))
{
    RunDemo(configDir);
    return;
}

// CLI mode: route all other args through CliBlueprint (mirrors Python calc_cli.py).
var cli = CliBlueprint.BuildCli(
    "basic_calc",
    configDir: configDir,
    description: "Basic Calculator CLI");
await cli.InvokeAsync(args);

// *** Demo runner

static void RunDemo(string configDir)
{
    var app = AppBlueprint.BuildApp("basic_calc", configDir);

    Console.WriteLine("=== Basic Calculator Demo ===\n");

    Execute(app, "calc.add",      [("a", "1"),  ("b", "2")],  r => $"1 + 2 = {r}");
    Execute(app, "calc.subtract", [("a", "5"),  ("b", "3")],  r => $"5 - 3 = {r}");
    Execute(app, "calc.multiply", [("a", "4"),  ("b", "3")],  r => $"4 * 3 = {r}");
    Execute(app, "calc.divide",   [("a", "8"),  ("b", "2")],  r => $"8 / 2 = {r}");
    Execute(app, "calc.divide",   [("a", "8"),  ("b", "0")],  r => $"8 / 0 = {r}");  // error
    Execute(app, "calc.exp",      [("a", "2"),  ("b", "3")],  r => $"2 ** 3 = {r}");
    Execute(app, "calc.sqrt",     [("a", "16")],              r => $"√16 = {r}");
}

static void Execute(
    AppInterfaceContext app,
    string featureId,
    (string k, string v)[] data,
    Func<object?, string> format)
{
    try
    {
        var dict = data.ToDictionary(kv => kv.k, kv => (object?)kv.v);
        var result = app.Run(featureId, data: dict);
        Console.WriteLine(format(result));
    }
    catch (TiferetApiException ex)
    {
        Console.WriteLine($"ErrorConfiguration: {ex.Message}");
    }
}
