
using System;
namespace SignalR.EventAggregatorProxy.CommandBus
{
    /// <summary>
    /// Implements a way of handling incoming commands with associated issues.
    /// </summary>
    public interface IRawCommandHandler
    {
        /// <summary>
        /// Handles the command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>true, if the command was handled successfully. Otherwise false.</returns>
        bool Handle(RawCommand command);
    }

    /// <summary>
    /// Lambda-based raw command handler.
    /// </summary>
    public class RawCommandHandler : IRawCommandHandler
    {
        private Action<RawCommand> action;
        public RawCommandHandler(Action<RawCommand> action) { this.action = action; }
        public bool Handle(RawCommand command)
        {
            action(command);
            return true;
        }
    }

}
