using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Radios;

namespace Bluetooth
{
    internal class Program
    {
        internal static async Task Main(string[] args)
        {
            if (!await HasAccess().ConfigureAwait(false))
            {
                return;
            }

            RadioState newFixedState = ParseCommandlineArguments(args);

            if (newFixedState == RadioState.Unknown)
            {
                await ToggleState().ConfigureAwait(false);
            }
            else
            {
                await SetState(newFixedState).ConfigureAwait(false);
            }
        }

        private static async Task<bool> HasAccess()
        {
            RadioAccessStatus accessStatus = await Radio.RequestAccessAsync().AsTask().ConfigureAwait(false);
            if (accessStatus == RadioAccessStatus.Allowed)
            {
                return true;
            }

            await Console.Error.WriteLineAsync("Access to Bluetooth radio not allowed.").ConfigureAwait(false);
            return false;
        }

        private static RadioState ParseCommandlineArguments(string[] args)
        {
            return args.Any(s => s == "1" || string.Equals(s, "on", StringComparison.OrdinalIgnoreCase)) ? RadioState.On :
                args.Any(s => s == "0" || string.Equals(s, "off", StringComparison.OrdinalIgnoreCase)) ? RadioState.Off : RadioState.Unknown;
        }

        private static async Task SetState(RadioState newState)
        {
            var radios = await Radio.GetRadiosAsync().AsTask().ConfigureAwait(false);
            foreach (Radio radio in radios.Where(x => x.Kind == RadioKind.Bluetooth && x.State != newState))
            {
                await radio.SetStateAsync(newState).AsTask().ConfigureAwait(false);
                Console.WriteLine($"Bluetooth Radio New State: {newState}");
            }
        }

        private static async Task ToggleState()
        {
            var radios = await Radio.GetRadiosAsync().AsTask().ConfigureAwait(false);
            var allowedStates = new[] {RadioState.On, RadioState.Off};
            foreach (Radio radio in radios.Where(x => x.Kind == RadioKind.Bluetooth && allowedStates.Contains(x.State)))
            {
                RadioState toggleState = radio.State == RadioState.Off ? RadioState.On : RadioState.Off;
                await radio.SetStateAsync(toggleState).AsTask().ConfigureAwait(false);
                Console.WriteLine($"Bluetooth Radio New State: {toggleState}");
            }
        }
    }
}