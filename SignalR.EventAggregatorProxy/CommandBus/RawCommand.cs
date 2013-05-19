using System;

namespace SignalR.EventAggregatorProxy.CommandBus
{
    /// <summary>
    /// Defines data and metadata of a received command.
    /// </summary>
    public class RawCommand
    {
        public Guid Id { get { return Id; } }
        private readonly Guid guid;

        public Type Type { get; set; }
        
        public object Data { get; set; }
        public DateTime Timestamp { get; set; }
        
        public string ConnectionId { get; set; }
        public string UserName { get; set; }
        
        public RawCommand()
        {
            this.guid = Guid.NewGuid();
        }

        public RawCommand(Guid guid)
        {
            this.guid = guid;
        }

    }

}
