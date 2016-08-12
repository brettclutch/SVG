using System;
#if ANDROID
using Android.Content;
using Javax.Crypto;
#endif
using Svg.Interfaces;
using Svg.Platform;

namespace Svg
{
    public class SvgSkiaPlatformOptions : SvgPlatformOptions
    {
        public bool EnableFastTextRendering { get; set; } = true;
    }

    public class SvgPlatformSetup : SvgPlatformSetupBase
    {
        private static bool _isInitialized = false;

        protected override void Initialize(SvgPlatformOptions options)
        {
            base.Initialize(options);

#if ANDROID
            Engine.Register<IFactory, IFactory>(() => new Factory());

            var ops = (SvgAndroidPlatformOptions)options;
            if (ops.EnableFastTextRendering)
            {
                Engine.Register<IAlternativeSvgTextRenderer, AndroidTextRenderer>(() => new AndroidTextRenderer());
            }
#else
            Engine.Register<IFactory, IFactory>(() => new SKFactory());
            var ops = (SvgSkiaPlatformOptions)options;
            if (ops.EnableFastTextRendering)
            {
                Engine.Register<IAlternativeSvgTextRenderer, SkiaTextRenderer>(() => new SkiaTextRenderer());
            }
#endif

        }

        public static void Init(SvgSkiaPlatformOptions options)
        {
            if (_isInitialized)
                return;

            new SvgPlatformSetup().Initialize(options);

            _isInitialized = true;
        }
    }
}