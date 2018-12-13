using System.Threading.Tasks;
using Xenox.Command;

namespace Xenox.TestAppService {
	public class TestCommand : ICommand {
		public string DeviceName { get; set; }
		public string DeviceModelId { get; set; }
	}

	public class TestCommandHandler : ICommandHandler<TestCommand> {
		public Task HandleAsync(TestCommand command) {
			var thing = new object();
			return Task.FromResult<object>(null);
		}
	}
}
