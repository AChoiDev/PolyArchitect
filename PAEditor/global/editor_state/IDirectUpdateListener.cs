using PolyArchitect.TransferDefinitions;
namespace PolyArchitect.Editor
{
    public interface IDirectUpdateListener<C, E> {
		public static abstract E FromUpdate(C update);
	}
}