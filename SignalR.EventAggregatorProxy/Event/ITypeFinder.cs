using System;
using System.Collections.Generic;
using SignalR.EventAggregatorProxy.EventAggregation;

namespace SignalR.EventAggregatorProxy.Event
{
    public interface ITypeFinder
    {
        IEnumerable<Type> ListEventTypes();
        bool KnowsType(string type);
        Type GetType(string type);
        Type GetConstraintHandlerType(Type type);
    }
}