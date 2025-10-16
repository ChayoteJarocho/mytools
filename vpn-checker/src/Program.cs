using System.CommandLine;

namespace VpnChecker;

public enum VpnProvider
{
    NordVpn,
    Surfshark
}

public class Program
{
    public static async Task Main(string[] args)
    {
        Option<VpnProvider> vpnOption = new(["-vpn", "--vpn"], $"The VPN provider to check.")
        {
            IsRequired = true
        };

        Option<bool> verboseOption = new(["-v", "--verbose"], getDefaultValue: () => false, "Enable verbose output.")
        {
            IsRequired = false
        };

        RootCommand rootCommand = new("VPN Connection Checker");

        rootCommand.AddOption(vpnOption);
        rootCommand.AddOption(verboseOption);

        rootCommand.SetHandler(async (vpn, verbose) =>
        {
            VpnChecker checker = new();
            switch (vpn)
            {
                case VpnProvider.NordVpn:
                    await checker.CheckNordVpnAsync(verbose);
                    break;
                case VpnProvider.Surfshark:
                    await checker.CheckSurfsharkAsync(verbose);
                    break;
                default:
                    Console.WriteLine("Invalid VPN.");
                    Environment.Exit(1);
                    break;
            }

        }, vpnOption, verboseOption);

        await rootCommand.InvokeAsync(args);
    }
}