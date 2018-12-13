using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xenox.Command;
using Xenox.DependencyInjection.Module;
using Xenox.DependencyInjection.Reflection;
using Xenox.Reflection;

namespace Xenox.Cph.DependencyInjection {
	public static class CphServiceCollectionExtensions {
		public static IServiceCollection AddCph(this IServiceCollection serviceCollection, IConfiguration configuration) {
			if (serviceCollection == null) {
				throw new ArgumentNullException(nameof(serviceCollection));
			}
			DynamicAssemblyLoadContext dynamicAssemblyLoadContext = new DynamicAssemblyLoadContext(
				configuration.GetSection("SharedAssemblies").GetChildren().Select(c => c.Value),
				configuration.GetSection("AssemblyProbingPaths").GetChildren().Select(c => c.Value)
			);
			List<Type> hostDiModuleTypes = new List<Type>();
			List<string> hostAssemblyNames = configuration.GetSection("Host:Assemblies").GetChildren().Select(c => c.Value).ToList();
			foreach (string hostAssemblyName in hostAssemblyNames) {
				Assembly hostAssembly = dynamicAssemblyLoadContext.LoadFromDirectory(hostAssemblyName);
				if (hostAssembly == null) {
					continue;
				}
				hostDiModuleTypes.AddRange(hostAssembly.GetTypes(t => t.IsConcreteClass() && t.ImplementsInterface(typeof(IDependencyInjectionModule))));
			}
			foreach (Type hostDiModuleType in hostDiModuleTypes) {
				IDependencyInjectionModule hostDiModule = Activator.CreateInstance(hostDiModuleType) as IDependencyInjectionModule;
				hostDiModule?.ConfigureServices(serviceCollection);
			}
			return serviceCollection.AddApplication(configuration, dynamicAssemblyLoadContext);
		}

		private static IServiceCollection AddApplication(this IServiceCollection serviceCollection, IConfiguration configuration, DynamicAssemblyLoadContext dynamicAssemblyLoadContext) {
			if (serviceCollection == null) {
				throw new ArgumentNullException(nameof(serviceCollection));
			}
			List<Type> commandTypes = new List<Type>();
			List<Type> applicationDiModuleTypes = new List<Type>();
			List<string> applicationAssemblyNames = configuration.GetSection("Application:Assemblies").GetChildren().Select(c => c.Value).ToList();
			foreach (string applicationAssemblyName in applicationAssemblyNames) {
				Assembly applicationAssembly = dynamicAssemblyLoadContext.LoadFromDirectory(applicationAssemblyName);
				if (applicationAssembly == null) {
					continue;
				}
				applicationDiModuleTypes.AddRange(applicationAssembly.GetTypes(t => t.IsConcreteClass() && t.ImplementsInterface(typeof(IDependencyInjectionModule))));
				commandTypes.AddRange(applicationAssembly.GetTypes(t => t.IsConcreteClass() && t.ImplementsInterface(typeof(ICommand))));
				serviceCollection.AddTransientGenericTypeDefinition(typeof(ICommandHandler<>), applicationAssembly);
			}
			foreach (Type applicationDiModuleType in applicationDiModuleTypes) {
				IDependencyInjectionModule module = Activator.CreateInstance(applicationDiModuleType) as IDependencyInjectionModule;
				module?.ConfigureServices(serviceCollection);
			}
			return serviceCollection.AddSingleton<ITypeProvider>(sp => new TypeProvider(commandTypes));
		}
	}
}
