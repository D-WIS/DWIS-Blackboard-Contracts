# DWIS Blackboard Rig Edition: contracts

Part of DWIS Blackboard Rig Edition (GitHub topic `dwis-blackboard-rig-edition`); design and decisions in [DWIS-Blackboard-Design](https://github.com/D-WIS/DWIS-Blackboard-Design).

**Holds:** the gRPC protobuf files, the HTTP DTOs, the report model, the namespace translation function, and the configuration library `DWIS.Blackboard.Configuration`.

**Produces:** NuGet packages `DWIS.Blackboard.Contracts` and `DWIS.Blackboard.Configuration`, `1.0.0-edge.<run>` from every merge to `main`, versioned from tags.

**Depends on:** nothing.


## How to work here

Trunk-based (specification 10.2 in the design repository): branch from `main` as `feature/…`, `fix/…` or `chore/…`, open a pull request early, one approval and green CI, squash merge. Releases are git tags `vX.Y.Z[-alpha.N]` on `main`. The Blackboard API stays at version 2; Rig Edition artefacts start at 1.0.0.

Work items live as issues with milestones M1 to M6 and are tracked on the organisation project [Rig Edition](https://github.com/orgs/D-WIS/projects/5).

## Licence

MIT, see [LICENSE](LICENSE).
