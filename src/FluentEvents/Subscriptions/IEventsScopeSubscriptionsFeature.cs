﻿using System.Collections.Generic;

namespace FluentEvents.Subscriptions
{
    internal interface IEventsScopeSubscriptionsFeature
    {
        IEnumerable<Subscription> GetSubscriptions(IScopedSubscriptionsService scopedSubscriptionsService);
    }
}