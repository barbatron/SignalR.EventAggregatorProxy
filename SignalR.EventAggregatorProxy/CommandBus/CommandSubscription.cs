using System;
using System.Threading;

namespace SignalR.EventAggregatorProxy.CommandBus
{
    public class CommandSubscription : IRawCommandHandler, IDisposable
    {
        public Type Type { get; private set; }        
        public IRawCommandHandler Handler { get; set; }
                
        public event EventHandler Disposing;
        public bool Disposed { get; private set; }
        private bool disposed = false;

        public CommandSubscription(Action<RawCommand> handler, Type topic)
            : this(new RawCommandHandler(handler), topic)
        { }

        public CommandSubscription(IRawCommandHandler handler, Type topic)
        {
            Handler = handler;
            Type = topic;
        }

        public bool Handle(RawCommand command)
        {
            return Handler.Handle(command);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private readonly AutoResetEvent disposeLocker = new AutoResetEvent(true);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed && disposeLocker.WaitOne(0, true))
            {
                if (disposing)
                {
                    Disposed = true;

                    // notify disposing
                    var handler = Disposing;
                    if (handler != null) handler.Invoke(this, EventArgs.Empty);
                    
                    // ditch references
                    Disposing = null;
                    Handler = null;
                    Type = null;
                }
            }
            disposed = true;
        }

        ~CommandSubscription()
        {
            Dispose(false);
        }

    }
}

