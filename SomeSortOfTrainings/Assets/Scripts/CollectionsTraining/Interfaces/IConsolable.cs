namespace Consoleable
{
    public interface IConsolable
    {
        public abstract void SendCommand(string command);
        public abstract void SendResponseToCommand(string response);
        public abstract string GetCommandList();
    }

}