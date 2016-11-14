﻿using System.Reactive.Concurrency;

namespace Svg.Editor.Services
{
    public class SchedulerProvider
    {
        public IScheduler MainScheduer { get; }
        public IScheduler BackgroundScheduler { get; }

        public SchedulerProvider(IScheduler mainScheduler, IScheduler backgroundScheduler)
        {
            MainScheduer = mainScheduler;
            BackgroundScheduler = backgroundScheduler;
        }
    }
}