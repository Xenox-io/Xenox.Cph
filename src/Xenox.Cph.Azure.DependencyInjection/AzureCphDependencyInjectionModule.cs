using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Xenox.Command.Dispatcher;
using Xenox.DependencyInjection.Configuration;
using Xenox.DependencyInjection.Module;
using Xenox.Host.Providers.DependencyInjection;
using Xenox.Serialization.CommandMessage;
using Xenox.Serialization.JsonNet.DependencyInjection;

namespace Xenox.Cph.Azure.DependencyInjection {
	public class AzureCphDependencyInjectionModule : IDependencyInjectionModule {
		public void ConfigureServices(IServiceCollection serviceCollection) {
			IConfiguration configuration = ConfigurationService.GetConfigurationJson("azurecphsettings-copy-to-cph-host-bin.json");
			serviceCollection
				.AddTransient<ICph, AzureCph>()
				.AddSingleton<ICommandDispatcher, CommandDispatcher>()
				.AddSingleton<ICommandMessageDeserializer, CommandMessageDeserializer>()
				.AddHostProviders()
				.AddJsonNetSerialization()
				.AddSingleton<CloudStorageAccount>(CloudStorageAccount.Parse(configuration["AzureQueue:AzureStorageConnectionString"]))
				.AddTransient<CloudQueueClient>(sp => {
					CloudStorageAccount azureAccount = sp.GetService<CloudStorageAccount>();
					return azureAccount.CreateCloudQueueClient();
				})
				.AddTransient<CloudQueue>(sp => {
					CloudQueueClient azureQueueClient = sp.GetService<CloudQueueClient>();
					CloudQueue azureQueue = azureQueueClient.GetQueueReference(configuration["AzureQueue:AzureQueueName"]);
					azureQueue.CreateIfNotExistsAsync().GetAwaiter().GetResult();
					return azureQueue;
				})
			;
		}
	}
}
