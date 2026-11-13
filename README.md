# DWIS Blackboard Rig Edition: contracts

Part of DWIS Blackboard Rig Edition (GitHub topic `dwis-blackboard-rig-edition`); design and decisions in [DWIS-Blackboard-Design](https://github.com/D-WIS/DWIS-Blackboard-Design).

**Holds:** the gRPC protobuf files, the HTTP DTOs, the report model, the namespace translation function, and the configuration library `DWIS.Blackboard.Configuration`.

**Produces:** NuGet packages `DWIS.Blackboard.Contracts` and `DWIS.Blackboard.Configuration`, `1.0.0-edge.<run>` from every merge to `main`, versioned from tags.

**Depends on:** nothing.

## Layout

One solution, `DWIS.Blackboard.Contracts.slnx`, .NET 10, with shared settings in `Directory.Build.props` (nullable, analyzers, warnings as errors) and `Directory.Packages.props` (central package versions). Tests use xUnit v3 on the Microsoft Testing Platform (opt-in in `global.json`) with FluentAssertions 7.

| Project | Responsibility |
|---|---|
| `src/DWIS.Blackboard.Configuration` | the five-layer configuration of both hosts (specification section 8) and the effective-configuration report with sources and masked secrets |
| `src/DWIS.Blackboard.Contracts` (milestone 3) | the gRPC protobuf files, the HTTP DTOs, the report model, the namespace translation function |

## DWIS.Blackboard.Configuration

Five layers, lowest precedence first: defaults in code; `appsettings.json` and `appsettings.{Environment}.json` next to the application; a shared file named by the environment variable `DWIS_CONFIG_FILE` (default `/config/dwis.json`), read once at start-up; environment variables `DWIS__Section__Key`; command-line arguments `--Section:Key=value`, with today's short names `--port`, `--useHub`, `--hubURL` and `--hubGroup` still accepted.

```csharp
var options = new DwisConfigurationOptions { CommandLineArguments = args, EnvironmentName = builder.Environment.EnvironmentName };
options.Defaults["Semantic:Address"] = "http://semantic:8080";
builder.Configuration.AddDwisLayers(options);
logger.LogInformation("{Report}", EffectiveConfiguration.Render(builder.Configuration));
```

The report lists every value with the layer it came from and prints secrets (keys ending in `Password`, `Secret`, `Token`, `ApiKey`, `PrivateKey`, `ConnectionString`, `Credential`) as `***`. Secrets come from files named by settings, never from arguments.

## Building and testing

```bash
dotnet build DWIS.Blackboard.Contracts.slnx -c Release
```

```bash
dotnet test DWIS.Blackboard.Contracts.slnx -c Release --no-build --report-trx --results-directory TestResults
```


## How to work here

Trunk-based (specification 10.2 in the design repository): branch from `main` as `feature/…`, `fix/…` or `chore/…`, open a pull request early, one approval and green CI, squash merge. Releases are git tags `vX.Y.Z[-alpha.N]` on `main`. The Blackboard API stays at version 2; Rig Edition artefacts start at 1.0.0.

Work items live as issues with milestones M1 to M6 and are tracked on the organisation project [Rig Edition](https://github.com/orgs/D-WIS/projects/5).

## Licence

MIT, see [LICENSE](LICENSE).
