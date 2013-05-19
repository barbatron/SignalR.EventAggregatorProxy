using System;
using System.Web.Routing;
using Microsoft.AspNet.SignalR;
using SignalR.EventAggregatorProxy.Event;

namespace SignalR.EventAggregatorProxy.SystemWeb
{
    public static class RouteCollectionExtensions
    {        
        public static void MapEventProxy<TEvent>(this RouteCollection routes, 
            string overrideUrl = null,
            RouteValueDictionary overrideDefaults = null, 
            IRouteHandler overrideHandler = null,
            RouteValueDictionary overrideConstraints = null)
        {
            var url = overrideUrl ?? "eventAggregation/events";
            var defaults = overrideDefaults ?? new RouteValueDictionary();
            var routeHandler = overrideHandler ?? new EventScriptRouteHandler<TEvent>();
            var constraints = overrideConstraints ?? new RouteValueDictionary() { { "controller", string.Empty } };

            routes.Add(new Route(
                url: "eventAggregation/events", 
                defaults: defaults,
                constraints: constraints,
                routeHandler: routeHandler));
        }
    }
}
