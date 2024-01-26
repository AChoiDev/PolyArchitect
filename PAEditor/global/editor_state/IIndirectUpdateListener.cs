using PolyArchitect.TransferDefinitions;
namespace PolyArchitect.Editor
{	
    public interface IIndirectUpdateListener<C, E> {
		public E WithUpdate(C update);
	}
}