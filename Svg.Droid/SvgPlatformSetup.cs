using Android.Content;
using Svg.Interfaces;
using Svg.Platform;

namespace Svg
{
    public class SvgAndroidPlatformOptions : SvgPlatformOptions
    {
        public Context Context { get; set; }

        public SvgAndroidPlatformOptions(Context context)
        {
            Context = context;
        }

        public bool EnableFastTextRendering { get; set; } = true;
    }

    public class SvgPlatformSetup : SvgPlatformSetupBase
    {
        private static bool _isInitialized = false;

        protected override void Initialize(SvgPlatformOptions options)
        {

            base.Initialize(options);
            
            Engine.Register<IFactory, Factory>(() => new Factory());

            var ops = (SvgAndroidPlatformOptions)options;
            if (ops.EnableFastTextRendering)
            {
                Engine.Register<IAlternativeSvgTextRenderer, AndroidTextRenderer>(() => new AndroidTextRenderer());
            }

        }

        public static void Init(SvgAndroidPlatformOptions options)
        {
            if (_isInitialized)
                return;

            new SvgPlatformSetup().Initialize(options);

            _isInitialized = true;
        }
    }
}