using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Queue;
using Xenox.Command;
using Xenox.Command.Dispatcher;
using Xenox.Serialization.CommandMessage;

namespace Xenox.Cph.Azure {
	public class AzureCph : ICph {
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly ICommandMessageDeserializer _commandDeserializer;
		private readonly CloudQueue _queue;

		public AzureCph(
			ICommandDispatcher commandDispatcher,
			ICommandMessageDeserializer commandDeserializer,
			CloudQueue queue
		) {
			_commandDispatcher = commandDispatcher;
			_commandDeserializer = commandDeserializer;
			_queue = queue;
		}

		public async Task<bool> ProcessOnceAsync() {
			CloudQueueMessage message = await _queue.GetMessageAsync();
			if (message == null) {
				return false;
			}
			ICommand command = _commandDeserializer.DeserializeCommandMessage(message.AsBytes);
			await _commandDispatcher.DispatchAsync(command);
			await _queue.DeleteMessageAsync(message);
			return true;
		}
	}
}
