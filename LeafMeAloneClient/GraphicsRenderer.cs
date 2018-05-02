using System;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using Device = SlimDX.Direct3D11.Device;
using Resource = SlimDX.Direct3D11.Resource;
using System.Windows.Forms;

namespace Client
{
    static class GraphicsRenderer
    {

        /// <summary>
        /// The main form which is going to render to the screen.
        /// </summary>
        public static RenderForm Form;

        /// <summary>
        /// Device is an adapter used to render.
        /// </summary>
        public static Device Device;
        public static DeviceContext DeviceContext;

        /// <summary>
        /// Swap chain holds surfaces which contain render data before putting them on screen.
        /// </summary>
        public static SwapChain SwapChain;

        /// <summary>
        /// 
        /// </summary>
        public static RenderTargetView RenderTarget;

        public static Viewport Viewport;

        public static Matrix ProjectionMatrix;

        #region Depth Buffer and Rasterizer
        private static Texture2DDescription depthBufferDesc;
        private static Texture2D DepthBuffer;
        public static DepthStencilView DepthView;
        public static DepthStencilState DepthState;
        private static DepthStencilStateDescription dsStateDesc;
        public static RasterizerStateDescription Rasterizer;

        public static BlendState BlendState;

        #endregion


        static void InitializeRasterizer()
        {
            Rasterizer = new RasterizerStateDescription()
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.None,
                IsFrontCounterclockwise = false,
                IsDepthClipEnabled = true
            };
            DeviceContext.Rasterizer.State = RasterizerState.FromDescription(Device, Rasterizer);
        }

        static void InitializeDepthBuffer()
        {
            Format depthFormat = Format.D32_Float;
            depthBufferDesc = new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = depthFormat,
                Height = Form.Height,
                Width = Form.Width,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default
            };

            DepthBuffer = new Texture2D(Device, depthBufferDesc);
            DepthView = new DepthStencilView(Device, DepthBuffer);

            dsStateDesc = new DepthStencilStateDescription()
            {
                IsDepthEnabled = true,
                IsStencilEnabled = false,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.Less,
            };

            DepthState = DepthStencilState.FromDescription(Device, dsStateDesc);
            DeviceContext.OutputMerger.DepthStencilState = DepthState;
        }

        static void InitializeBlending()
        {
            BlendStateDescription bs = new BlendStateDescription()
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false
            };
            bs.RenderTargets[0].BlendEnable = true;
            bs.RenderTargets[0].SourceBlend = BlendOption.SourceAlpha;
            bs.RenderTargets[0].DestinationBlend = BlendOption.Zero;
            bs.RenderTargets[0].BlendOperation = BlendOperation.Minimum;
            bs.RenderTargets[0].SourceBlendAlpha = BlendOption.Zero;
            bs.RenderTargets[0].DestinationBlendAlpha = BlendOption.Zero;
            bs.RenderTargets[0].BlendOperationAlpha = BlendOperation.Minimum;
            bs.RenderTargets[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            BlendState = BlendState.FromDescription(Device, bs);
        }


        /// <summary>
        /// Initialize graphics properties and create the main window.
        /// </summary>
        public static void Init()
        {

            //Make the Form
            Form = new RenderForm("LeafMeAlone");

            SwapChainDescription description = new SwapChainDescription()
            {
                BufferCount = 1,
                Usage = Usage.RenderTargetOutput,
                OutputHandle = Form.Handle,
                IsWindowed = true,
                ModeDescription = new ModeDescription(0, 0, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                SampleDescription = new SampleDescription(1, 0),
                Flags = SwapChainFlags.AllowModeSwitch,
                SwapEffect = SwapEffect.Discard
            };

            //create new device (with directx) which can be used throughout the project.
            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, description, out Device, out SwapChain);



            // create a view of our render target, which is the backbuffer of the swap chain we just created
            using (var resource = Resource.FromSwapChain<Texture2D>(SwapChain, 0))
                RenderTarget = new RenderTargetView(Device, resource);

            //get device context so we can render to it.
            DeviceContext = Device.ImmediateContext;

            InitializeRasterizer();
            InitializeDepthBuffer();
            InitializeBlending();

            Viewport = new Viewport(0.0f, 0.0f, Form.ClientSize.Width, Form.ClientSize.Height);
            //DeviceContext.OutputMerger.SetTargets(RenderTarget);
            DeviceContext.OutputMerger.SetTargets(DepthView, RenderTarget);
            DeviceContext.Rasterizer.SetViewports(Viewport);

            Form.Resize += FormOnResize;

            ProjectionMatrix = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, Viewport.Width / Viewport.Height, .1f, 1000.0f);

            // dont use default alt enter because it doesn't work.
            using (var factory = SwapChain.GetParent<Factory>())
                factory.SetWindowAssociation(Form.Handle, WindowAssociationFlags.IgnoreAltEnter);

            // handle alt+enter ourselves
            Form.KeyDown += (o, e) =>
            {
                //if (e.Alt && e.KeyCode == Keys.Enter)
                //    SwapChain.IsFullScreen = !SwapChain.IsFullScreen;
                //else 
                if (e.KeyCode == Keys.Escape)
                    Application.Exit();
            };
        }

        /// <summary>
        /// Method called when the form is resized by the user.
        /// </summary>
        /// <param name="sender">sending form</param>
        /// <param name="eventArgs">event argument</param>
        private static void FormOnResize(object sender, EventArgs eventArgs)
        {
            Viewport.Height = Form.ClientSize.Height;
            Viewport.Width = Form.ClientSize.Width;
            ProjectionMatrix = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, Viewport.Width / Viewport.Height, .1f, 1000.0f);
            DeviceContext.Rasterizer.SetViewports(Viewport);
        }


        /// <summary>
        /// Dispose of the window and other things once the app is closing to prevent memory leaks.
        /// </summary>
        public static void Dispose()
        {
            RenderTarget.Dispose();
            SwapChain.Dispose();
            Device.Dispose();
            Form.Dispose();
        }
    }
}
