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
            _commandRegistry = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            InitializeDefaultCommands();
        }

        private void InitializeDefaultCommands()
        {
            _commandRegistry["/start"] = typeof(StartCommand);
            _commandRegistry["/help"] = typeof(HelpCommand);
            _commandRegistry["/contact"] = typeof(ContactCommand);
            _commandRegistry["go_back"] = typeof(StartCommand);
        }

        public ICommand CreateCommand(string input, string userState)
        {
            var cleanInput = input.Split(' ')[0].ToLower();

            // Ищем команду в реестре
            if (_commandRegistry.TryGetValue(cleanInput, out var commandType))
            {
                return (ICommand)_serviceProvider.GetRequiredService(commandType);
            }

            return null;
        }

        public void RegisterCommand(string key, Type commandType)
        {
            if (!typeof(ICommand).IsAssignableFrom(commandType))
            {
                throw new ArgumentException($"Тип {commandType.Name} должен реализовывать ICommand");
            }

            _commandRegistry[key] = commandType;
        }

        public bool IsCommandExists(string input)
        {
            return _commandRegistry.ContainsKey(input.Split(' ')[0].ToLower());
        }
    }
}
