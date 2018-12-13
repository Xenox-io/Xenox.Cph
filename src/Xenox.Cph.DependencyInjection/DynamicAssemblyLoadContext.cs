using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Xenox.Cph.DependencyInjection {
	public class DynamicAssemblyLoadContext : AssemblyLoadContext {
		private readonly List<string> _sharedAssemblies;
		private readonly List<string> _assemblyProbingDirectories;

		public DynamicAssemblyLoadContext(IEnumerable<string> sharedAssemblies, IEnumerable<string> assemblyProbingDirectories) {
			_sharedAssemblies = sharedAssemblies.ToList();
			_assemblyProbingDirectories = assemblyProbingDirectories.ToList();
			_assemblyProbingDirectories.Add(Directory.GetCurrentDirectory());
		}

		protected override Assembly Load(AssemblyName assemblyName) {
			if (_sharedAssemblies.Contains(assemblyName.Name)) {
				return Default.LoadFromAssemblyName(assemblyName);
			}
			return LoadFromDirectory(assemblyName);
		}

		public Assembly LoadFromDirectory(AssemblyName assemblyName) {
			return LoadFromDirectory(assemblyName.Name);
		}

		public Assembly LoadFromDirectory(string assembly) {
			try {
				foreach (string assemblyProbingDirectory in _assemblyProbingDirectories) {
					string assemblyPath = Path.Combine(assemblyProbingDirectory, assembly) + ".dll";
					if (File.Exists(assemblyPath)) {
						return LoadFromAssemblyPath(assemblyPath);
					}
				}
			} catch {
				// Ignored.
			}
			return null;
		}
	}
}
