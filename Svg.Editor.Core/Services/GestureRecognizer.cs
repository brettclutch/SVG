﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Svg.Core.Events;
using Svg.Core.Extensions;
using Svg.Core.Gestures;
using Svg.Interfaces;

namespace Svg.Core.Services
{
    public class GestureRecognizer : IDisposable
    {
        #region Private fields

        private readonly Subject<UserGesture> _recognizedGestures = new Subject<UserGesture>();
        private IObservable<UserInputEvent> DetectedInputEvents { get; }

        public IObservable<UserGesture> RecognizedGestures => _recognizedGestures.AsObservable();

        private readonly IDictionary<string, IDisposable> _subscriptions = new Dictionary<string, IDisposable>();

        #endregion

        #region Public properties

        /// <summary>
        /// Minimum distance in pixels that is required to start a drag gesture.
        /// </summary>
        public double DragMinDistance { get; set; } = 10.0;

        /// <summary>
        /// Time in seconds which the pointer has to stay down until a long press gesture will be triggered.
        /// </summary>
        public double LongPressDuration { get; set; } = 0.66;

        /// <summary>
        /// Maximum distance in pixels that is ignored when tapping, to accomodate tiny shivers.
        /// </summary>
        public double TouchThreshold { get; set; } = 20.0;

        /// <summary>
        /// Maximum time in seconds for a valid tap gesture.
        /// </summary>
        public double TapTimeout { get; set; } = 0.33;

        /// <summary>
        /// Maximum time in seconds for a valid double tap gesture.
        /// </summary>
        public double DoubleTapTimeout { get; set; } = 0.2;

        #endregion

        public GestureRecognizer(IObservable<UserInputEvent> detectedInputEvents, IScheduler mainScheduler, IScheduler backgroundScheduler)
        {
            DetectedInputEvents = detectedInputEvents;

            var pointerEvents = DetectedInputEvents.OfType<PointerEvent>();
            var enterEvents = pointerEvents.Where(pe => pe.EventType == EventType.PointerDown);
            var exitEvents = pointerEvents.Where(pe => pe.EventType == EventType.PointerUp || pe.EventType == EventType.Cancel);
            var interactionWindows = pointerEvents.Window(enterEvents, _ => exitEvents);

            #region tap gestures

            _subscriptions["tap"] = interactionWindows
            .SelectMany(window =>
            {
                return window
                .Where(
                    pe =>
                        pe.EventType == EventType.PointerUp &&
                        PositionEquals(pe.Pointer1Down, pe.Pointer1Position, TouchThreshold))
                .Buffer(TimeSpan.FromSeconds(TapTimeout), 1, backgroundScheduler)
                .Take(1);
            })
            .Where(l => l.Any())
            .Select(l => new TapGesture(l.First().Pointer1Position))
            .Timestamp()
            .PairWithPrevious()
            .Throttle(TimeSpan.FromSeconds(DoubleTapTimeout), backgroundScheduler)
            .ObserveOn(mainScheduler)
            .Select
            (
                tup => 
                    tup.Item1.Value == null ||
                    tup.Item2.Timestamp - tup.Item1.Timestamp > TimeSpan.FromSeconds(DoubleTapTimeout) ||
                    !PositionEquals(tup.Item1.Value.Position, tup.Item2.Value.Position, TouchThreshold)
                        ? tup.Item2.Value
                        : new DoubleTapGesture(tup.Item2.Value.Position)
            )
            .Subscribe(tg => _recognizedGestures.OnNext(tg));

            #endregion

            #region long press gesture

            _subscriptions["longpress"] = interactionWindows
            .SelectMany(window =>
            {
                return window
                .Where(pe => pe.EventType == EventType.PointerDown || pe.EventType == EventType.PointerUp ||
                    !PositionEquals(pe.Pointer1Down, pe.Pointer1Position, TouchThreshold))
                .Buffer(TimeSpan.FromSeconds(LongPressDuration), 2)
                .Take(1);
            })
            .ObserveOn(mainScheduler)
            .Subscribe
            (
                l =>
                {
                    if (l.Count == 1) _recognizedGestures.OnNext(new LongPressGesture(l.First().Pointer1Position));
                },
                ex => Debug.WriteLine(ex.Message)
            );

            #endregion

            #region drag gesture

            _subscriptions["drag"] = interactionWindows.Subscribe(window =>
            {
                // create this subject for controlling lifetime of the subscription
                var dragLifetime = new Subject<Unit>();
                var dragSubscription = window
                    .Where(pe => pe.EventType == EventType.Move && pe.PointerCount == 1)
                    // if we get a window without move events, we want to dispose subscription entirely,
                    // else we would propagate an unneccessary DragGesture.Exit gesture
                    .DefaultIfEmpty(new PointerEvent(EventType.Cancel, PointF.Empty, PointF.Empty, PointF.Empty, 0))
                    .Select((pe, i) =>
                    {
                        if (i == 0 && pe.EventType != EventType.Cancel)
                            _recognizedGestures.OnNext(DragGesture.Enter(pe.Pointer1Down));
                        // if we had an empty window, it defaults to EventType.Cancel and we dispose subscription
                        if (pe.EventType == EventType.Cancel) dragLifetime.OnCompleted();
                        return pe;
                    })
                    .Subscribe
                    (
                        pe =>
                        {
                            var deltaPoint = pe.Pointer1Position - pe.Pointer1Down;
                            var delta = SizeF.Create(deltaPoint.X, deltaPoint.Y);

                            // selection only counts if width and height are not too small
                            var dist = Math.Sqrt(Math.Pow(delta.Width, 2) + Math.Pow(delta.Height, 2));

                            if (dist > DragMinDistance)
                            {
                                _recognizedGestures.OnNext(new DragGesture(pe.Pointer1Position, pe.Pointer1Down,
                                    delta, dist));
                            }
                        },
                        ex => { },
                        () => _recognizedGestures.OnNext(DragGesture.Exit)
                    );

                // bind completion of dragLifetime to disposing of our subscription
                Observable.Using(() => dragSubscription, _ => dragLifetime).Subscribe();
            });

            #endregion
        }

        private static bool PositionEquals(PointF start, PointF position, double threshold = 0)
        {
            var delta = position - start;
            return Math.Abs(delta.X) <= threshold && Math.Abs(delta.Y) <= threshold;
        }

        public void Dispose()
        {
            foreach (var disposable in _subscriptions.Values) disposable.Dispose();
        }
    }

    public interface IGestureDetector
    {
        IObservable<UserInputEvent> DetectedGestures { get; }
    }
}