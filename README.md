# Etherna Gateway CLI

## Overview

A CLI interface to the Etherna Gateway

## Instructions
Download and extract binaries from [release page](https://github.com/Etherna/etherna-gateway-cli/releases).

Etherna Gateway CLI requires at least [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) and [ASP.NET Core 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) installed on local machine to run, or it needs the `selfcontained` version of package, that already contains framework dependencies.

### How to use

```
Usage:  etherna [OPTIONS] COMMAND

Commands:
  download    Download a resource from Swarm
  postage     Manage postage batches
  resource    Manage resources on Gateway
  upload      Upload a resource to Swarm

General Options:
  -k, --api-key           Api Key (optional)
  -i, --ignore-update     Ignore new version of EthernaGatewayCli

Run 'etherna -h' or 'etherna --help' to print help.
Run 'etherna COMMAND -h' or 'etherna COMMAND --help' for more information on a command.
```

# Issue reports
If you've discovered a bug, or have an idea for a new feature, please report it to our issue manager based on Jira https://etherna.atlassian.net/projects/EGC.

# Questions? Problems?

For questions or problems please write an email to [info@etherna.io](mailto:info@etherna.io).
