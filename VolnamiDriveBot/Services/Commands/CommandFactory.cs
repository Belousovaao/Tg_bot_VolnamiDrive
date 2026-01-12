using Microsoft.Extensions.DependencyInjection;
using VolnamiDriveBot.Commands;
using VolnamiDriveBot.Commands.Concrete;

namespace VolnamiDriveBot.Services.Commands
{
    public class CommandFactory : ICommandFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Type> _commandRegistry;

        public CommandFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _commandRegistry = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                {"/start",  typeof(StartCommand)},
                {"/help", typeof(HelpCommand)},
                {"/contact",  typeof(ContactCommand)},
                {"go_back",  typeof(StartCommand)}
            };
        }

        public ICommand CreateCommand(string input, string userState)
        {
            string cleanInput = input.Split(' ')[0].ToLower();

            // Ищем команду в словаре
            if (_commandRegistry.TryGetValue(cleanInput, out var commandType))
            {
                return (ICommand)_serviceProvider.GetRequiredService(commandType);
            }

            return null;
        }
    }
}
