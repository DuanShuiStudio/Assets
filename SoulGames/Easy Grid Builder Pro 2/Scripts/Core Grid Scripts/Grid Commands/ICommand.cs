namespace SoulGames.EasyGridBuilderPro
{
    public interface ICommand
    {
        public void Execute();
        public void Undo();
        public void Redo();
    }
}