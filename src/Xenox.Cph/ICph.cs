using System.Threading.Tasks;

namespace Xenox.Cph {
	public interface ICph {
		Task<bool> ProcessOnceAsync();
	}
}
