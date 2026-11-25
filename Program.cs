using System.Text.Json;

try
{
    if (args.Length == 1 && args[0] == "--test") { LedgerTests.Run(); return 0; }
    if (args.Length != 1 || args[0] == "--help")
    {
        Console.WriteLine("Usage: dotnet run -- <transfers.json> | --test\nReplays atomic transfers in integer cents; identical event IDs are idempotent.");
        return args.Length == 1 ? 0 : 2;
    }
    var transfers = JsonSerializer.Deserialize<Transfer[]>(File.ReadAllText(args[0]), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
        ?? throw new ArgumentException("Expected a JSON array.");
    var ledger = new Ledger();
    ledger.Apply(transfers);
    Console.WriteLine(JsonSerializer.Serialize(new { applied = ledger.AppliedCount, balances = ledger.Balances.OrderBy(x => x.Key, StringComparer.Ordinal).ToDictionary() }, new JsonSerializerOptions { WriteIndented = true }));
    return 0;
}
catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException or ArgumentException or OverflowException)
{
    Console.Error.WriteLine(error.Message);
    return 1;
}
