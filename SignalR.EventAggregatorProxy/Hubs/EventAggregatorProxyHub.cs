using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using SignalR.EventAggregatorProxy.EventAggregation;
using SignalR.EventAggregatorProxy.CommandBus;

namespace SignalR.EventAggregatorProxy.Hubs
{
    public class EventAggregatorProxyHub : Hub
    {
        private static readonly EventProxy eventProxy;
        private static readonly CommandProxy commandProxy;

        static EventAggregatorProxyHub()
        {
            eventProxy = new EventProxy();
            try
            {
                commandProxy = GlobalHost.DependencyResolver.Resolve<CommandProxy>();
            }
            catch
            {
            }
            commandProxy = commandProxy ?? new CommandProxy();
        }

        #region Events

        public void Subscribe(string type, dynamic contraint)
        {
            eventProxy.Subscribe(Context, type, contraint);
        }

        public void Unsubscribe(IEnumerable<string> types)
        {
            eventProxy.Unsubscribe(Context.ConnectionId, types);
        }

        public override Task OnDisconnected()
        {
            eventProxy.UnsubscribeConnection(Context.ConnectionId);
            return base.OnDisconnected();
        }

        #endregion
        #region Commands
        
        public void Submit(string topic, object data)
        {
            commandProxy.Submit(Context, topic, data);
        }
        
        #endregion

    }
}
