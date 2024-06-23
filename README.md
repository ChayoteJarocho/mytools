# VPN Checker

Tool to detect if your VPN is connected or not.

## Requirements

- .NET 8 SDK: https://dotnet.microsoft.com/en-us/download/dotnet/8.0

## Build instructions

```
git clone https://github.com/ChayoteJarocho/vpn-checker
cd vpn-checker
dotnet build -C Release
```

### NordVPN

This is the only VPN supported at the moment.

Command:
```
dotnet run -c Release --project src/VpnChecker.csproj
```

If NordVPN is connected, the output is `True`, otherwise `False`.
