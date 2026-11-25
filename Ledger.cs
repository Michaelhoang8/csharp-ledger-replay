public sealed record Transfer(string Id, string From, string To, long Cents);

public sealed class Ledger
{
    private Dictionary<string, long> balances = new(StringComparer.Ordinal);
    private Dictionary<string, Transfer> journal = new(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, long> Balances => new Dictionary<string, long>(balances);
    public int AppliedCount => journal.Count;

    // Validate and replay against a private candidate. Publish only a complete batch.
    public void Apply(IEnumerable<Transfer> batch)
    {
        ArgumentNullException.ThrowIfNull(batch);
        var candidate = new Dictionary<string, long>(balances, StringComparer.Ordinal);
        var events = new Dictionary<string, Transfer>(journal, StringComparer.Ordinal);
        foreach (var transfer in batch)
        {
            if (transfer is null || string.IsNullOrWhiteSpace(transfer.Id) ||
                string.IsNullOrWhiteSpace(transfer.From) || string.IsNullOrWhiteSpace(transfer.To) ||
                transfer.From == transfer.To || transfer.Cents <= 0)
                throw new ArgumentException("Transfer requires an ID, distinct accounts and positive integer cents.");
            if (events.TryGetValue(transfer.Id, out var previous))
            {
                if (previous != transfer) throw new ArgumentException($"Conflicting event ID: {transfer.Id}");
                continue;
            }
            checked
            {
                candidate[transfer.From] = candidate.GetValueOrDefault(transfer.From) - transfer.Cents;
                candidate[transfer.To] = candidate.GetValueOrDefault(transfer.To) + transfer.Cents;
            }
            events.Add(transfer.Id, transfer);
        }
        balances = candidate;
        journal = events;
    }
}
