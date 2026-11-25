# Atomic ledger replay

Replay transfers in integer cents with atomic batches, idempotent event IDs and checked arithmetic.

Language: **C#**.

[![Validation](https://github.com/Michaelhoang8/csharp-ledger-replay/actions/workflows/ci.yml/badge.svg)](https://github.com/Michaelhoang8/csharp-ledger-replay/actions/workflows/ci.yml)

## Run

```sh
dotnet run -- fixtures/transfers.json
```

## Behavior

- A conflicting duplicate event rejects the entire batch without changing balances.
- Identical retries do not create duplicate transfers.
- Balance snapshots are copies; callers cannot mutate internal state.

## Validation

```sh
dotnet run -- --test
```

The workflow executes the checked-in fixtures and boundary tests. See the workflow log for the actual result; tests are included with the source.

## Scope

An in-memory replay study: no database, network service, authentication or multi-currency conversion. Negative balances are permitted; this is not an overdraft policy.

Code: [MIT](LICENSE). Fixtures are constructed examples.
