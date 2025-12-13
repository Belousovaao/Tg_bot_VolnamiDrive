using VolnamiDriveBot.Commands;

namespace VolnamiDriveBot.Services.Commands
{
    public interface ICommandFactory
    {
        ICommand CreateCommand(string input, string userState);
        void RegisterCommand(string key, Type commandType);
        bool IsCommandExists(string input);
    }
}
