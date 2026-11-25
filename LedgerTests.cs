using System.Numerics;

public static class LedgerTests
{
    private static int count;
    private static void Check(bool ok) { count++; if (!ok) throw new Exception($"Check {count} failed"); }
    private static void Reject(Action action) { bool rejected = false; try { action(); } catch (Exception e) when (e is ArgumentException or OverflowException) { rejected = true; } Check(rejected); }
    public static void Run()
    {
        var ledger = new Ledger();
        var first = new Transfer("a", "cash", "merchant", 1250);
        ledger.Apply([first, new("b", "merchant", "fees", 350), first]);
        Check(ledger.AppliedCount == 2);
        Check(ledger.Balances["cash"] == -1250 && ledger.Balances["merchant"] == 900 && ledger.Balances["fees"] == 350);
        Check(ledger.Balances.Values.Aggregate(BigInteger.Zero, (a,b) => a + b) == 0);
        Reject(() => ledger.Apply([new("c", "fees", "cash", 1), first with { Cents = 10 }]));
        Check(ledger.AppliedCount == 2 && ledger.Balances["fees"] == 350);
        Reject(() => ledger.Apply([first with { Id = "", Cents = 2 }]));
        Reject(() => ledger.Apply([new("c", "cash", "cash", 2)]));
        Reject(() => ledger.Apply([new("c", "cash", "fees", 0)]));
        Reject(() => ledger.Apply([new("c", "cash", "fees", -2)]));
        var large = new Ledger(); large.Apply([new("1", "a", "b", long.MaxValue)]);
        Reject(() => large.Apply([new("2", "a", "b", 1)]));
        Check(large.AppliedCount == 1 && large.Balances["b"] == long.MaxValue);
        var copy = (Dictionary<string,long>)ledger.Balances; copy["cash"] = 0;
        Check(ledger.Balances["cash"] == -1250);
        ledger.Apply([]); Check(ledger.AppliedCount == 2);
        Console.WriteLine($"{count} ledger checks passed");
    }
}
