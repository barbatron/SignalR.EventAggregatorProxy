using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using SignalR.EventAggregatorProxy.Event;
using SignalR.EventAggregatorProxy.Hubs;
using SignalR.EventAggregatorProxy.EventAggregation;
using System.Diagnostics;

namespace SignalR.EventAggregatorProxy.CommandBus
{
    /// <summary>
    /// Maintains server-side subscriptions for commands issued by client.
    /// </summary>
    public class CommandProxy
    {
        public IRawCommandHandler UnrecognizedTypeHandler { get; set; }
        public IRawCommandHandler CommandIgnoredHandler { get; set; }

        private readonly ITypeFinder typeFinder;
        private readonly IDictionary<Type, List<CommandSubscription>> subscriptions;
        
        
        /* Auto resolve via SignalR dep.resolver */
        public CommandProxy() 
            : this(GlobalHost.DependencyResolver.Resolve<ITypeFinder>())
        { }
        
        /* Single typefinder */
        public CommandProxy(ITypeFinder typeFinder) 
        { 
            this.typeFinder = typeFinder;            
            this.subscriptions = typeFinder
                .ListEventTypes()
                .ToDictionary(t => t, t => new List<CommandSubscription>());
        }
        
        /// <summary>
        /// Accepts an client command and hands it to any subscribers.
        /// registered subscribers.
        /// </summary>
        public void Submit(HubCallerContext Context, string typeName, object data)
        {
            if (!ValidateCommand(Context, typeName, data))
                return;

            var type = typeFinder.GetType(typeName);
            var typeSubs = subscriptions[type];
            var rc = BuildRawCommand(Context, typeName, data);

            // Notify subscribers:
            typeSubs.ForEach(ts => ts.Handle(rc));            
        }

        public IDisposable Subscribe<TCommand>(Action<RawCommand> callback)
        {
            return Subscribe(typeof(TCommand).FullName, callback);
        }

        public IDisposable Subscribe(string topic, Action<RawCommand> callback)
        {
            var type = typeFinder.GetType(topic);
            
            var subscription = new CommandSubscription(new RawCommandHandler(callback), type);

            subscriptions[type].Add(subscription);
            subscription.Disposing += OnSubscriptionDisposing;

            return subscription;
        }

        private void OnSubscriptionDisposing(object sender, EventArgs e)
        {
            var cs = sender as CommandSubscription;
            if (cs == null)
                throw new ArgumentException("Invalid sender");
            Unsubscribe(cs, false);
        }

        public void Unsubscribe(CommandSubscription subscription, bool dispose = true)
        {            
            var typeSubs = subscriptions[subscription.Type];
            typeSubs.RemoveAll(s => object.ReferenceEquals(s, subscription));            
            if (dispose && !subscription.Disposed) subscription.Dispose();
        }

        private RawCommand BuildRawCommand(HubCallerContext Context, string typeName, object data)
        {           
            return new RawCommand
            {
                Type = typeFinder.KnowsType(typeName) ? typeFinder.GetType(typeName) : null,
                Data = data,
                ConnectionId = Context.ConnectionId,
                UserName = Context.User.Identity.Name,
                Timestamp = DateTime.Now
            };
        }

        private bool ValidateCommand(HubCallerContext Context, string typeName, object data)
        {
            Lazy<RawCommand> cmdFactory = 
                new Lazy<RawCommand>(() => BuildRawCommand(Context, typeName, data));

            if (typeFinder.KnowsType(typeName) == false)
            {
#if DEBUG
                Trace.TraceWarning("Client command type \"{0}\" was unrecognized.", typeName);
#endif
                if (UnrecognizedTypeHandler != null)
                {
                    bool handled = UnrecognizedTypeHandler.Handle(cmdFactory.Value);
                    if (handled) Trace.TraceInformation("Item was handled by fallback.");
                    return false;
                }
            }

            if (subscriptions.ContainsKey(typeFinder.GetType(typeName)) == false)
            {
#if DEBUG
                Trace.TraceWarning("Client command type \"{0}\" was ignored (no subscribers).", typeName);
#endif
                if (CommandIgnoredHandler != null)
                {
                    bool handled = CommandIgnoredHandler.Handle(cmdFactory.Value);
                    if (handled) Trace.TraceInformation("Item was handled by fallback.");
                    return false;
                }
            }

            return true;
        }
    }
}
