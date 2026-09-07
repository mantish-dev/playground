# Mantish Playground

Sample applications used to demonstrate [Mantish](https://mantish.dev), an AI-powered
code review tool. The pull requests in this repository are opened on purpose and stay
open: each one shows how Mantish explains a particular kind of change, from a moved
method to an extracted service.

This is not the Mantish product itself. The code here exists to be reviewed, recorded
and linked to from the Mantish website and demos.

## Applications

| Directory | Stack                     | What it is                                     |
| --------- | ------------------------- | ---------------------------------------------- |
| `csharp/` | C# / ASP.NET Core, SQLite | **Stockroom**, a small warehouse inventory API |

Each application is self-contained. See its own `README.md` for how to build, run and
test it.

## Pull requests

Every demo PR is scoped to a single application and titled with its directory as a
prefix, for example `csharp: extract pricing into its own service`. The description of
each PR states which change type it demonstrates.
