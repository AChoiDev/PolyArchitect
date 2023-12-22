namespace PolyArchitect.Core {
    public interface IHCommand {
        public string cliCmdName {get;}
        public void Apply();
        public void Undo();
    }

}