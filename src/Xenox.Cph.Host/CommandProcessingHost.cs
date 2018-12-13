using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xenox.Cph.DependencyInjection;
using Xenox.DependencyInjection.Configuration;

namespace Xenox.Cph.Host {
	public static class CommandProcessingHost {
		public static void Main(string[] args) {
			MainAsync(args).GetAwaiter().GetResult();
		}

		public static async Task MainAsync(string[] args) {
			IServiceProvider serviceProvider = (new ServiceCollection())
				.AddCph(ConfigurationService.GetConfigurationJson("hostsettings.json"))
				.BuildServiceProvider()
			;
			ICph cph = serviceProvider.GetService<ICph>();
			if (cph == null) {
				throw new Exception("No CPH was found in configured Host Assemblies.");
			}
			for (;;) {
				if (Console.KeyAvailable) {
					break;
				}
				await cph.ProcessOnceAsync();
			}
		}
	}
}
