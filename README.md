# Etherna Gateway CLI

## Overview

A CLI interface to the Etherna Gateway

## Instructions
Download and extract binaries from [release page](https://github.com/Etherna/etherna-gateway-cli/releases).

Etherna Gateway CLI requires at least [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) and [ASP.NET Core 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) installed on local machine to run, or it needs the `selfcontained` version of package, that already contains framework dependencies.

### How to use

```
etherna
A CLI interface to the Etherna Gateway.

    Program distributed under AGPLv3 license. Copyright since 2024 by Etherna SA.
    You can find source code at: https://github.com/Etherna/etherna-gateway-cli

Usage:  etherna [ETHERNA_OPTIONS] COMMAND

Commands:
  chunk       Manage swarm chunks
  download    Download a resource from Swarm
  postage     Manage postage batches
  resource    Manage Swarm resources
  upload      Upload files and directories to Swarm

Options:
  -k, --api-key string        Api Key (optional)
      --bee                   Use bee API
      --gateway-url string    Custom gateway url
  -i, --ignore-update         Ignore new versions of EthernaGatewayCli

Run 'etherna -h' or 'etherna --help' to print help.
Run 'etherna COMMAND -h' or 'etherna COMMAND --help' for more information on a command.
```

## Issue reports
If you've discovered a bug, or have an idea for a new feature, please report it to our issue manager based on Jira https://etherna.atlassian.net/projects/EGC.

## Questions? Problems?

For questions or problems please write an email to [info@etherna.io](mailto:info@etherna.io).

## License

![AGPL Logo](https://www.gnu.org/graphics/agplv3-with-text-162x68.png)

We use the GNU Affero General Public License v3 (AGPL-3.0) for this project.
If you require a custom license, you can contact us at [license@etherna.io](mailto:license@etherna.io).
