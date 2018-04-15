using System;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using Device = SlimDX.Direct3D11.Device;
using Resource = SlimDX.Direct3D11.Resource;
using System.Windows.Forms;

namespace LeafMeAloneClient
{
    static class GraphicsRenderer
    {
        public static RenderForm Form;
        public static Device Device;
        public static DeviceContext DeviceContext;

        public static SwapChain SwapChain;

        public static RenderTargetView RenderTarget;

        public static Viewport Viewport;

        public static Matrix ProjectionMatrix;

        public static void Init()
        {
            Form = new RenderForm("LeafMeAlone");
            var description = new SwapChainDescription()
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


            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, description, out Device, out SwapChain);

            // create a view of our render target, which is the backbuffer of the swap chain we just created
            using (var resource = Resource.FromSwapChain<Texture2D>(SwapChain, 0))
                RenderTarget = new RenderTargetView(Device, resource);

            //get device context
            DeviceContext = Device.ImmediateContext;

            Viewport = new Viewport(0.0f, 0.0f, Form.ClientSize.Width, Form.ClientSize.Height);
            DeviceContext.OutputMerger.SetTargets(RenderTarget);
            DeviceContext.Rasterizer.SetViewports(Viewport);

            Form.Resize += FormOnResize;

            ProjectionMatrix = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, Viewport.Width / Viewport.Height, .1f, 1000.0f);

            // prevent DXGI handling of alt+enter, which doesn't work properly with Winforms
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

        private static void FormOnResize(object sender, EventArgs eventArgs)
        {
            Viewport.Height = Form.ClientSize.Height;
            Viewport.Width = Form.ClientSize.Width;
            ProjectionMatrix = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, Viewport.Width / Viewport.Height, .1f, 1000.0f);
            DeviceContext.Rasterizer.SetViewports(Viewport);
        }

        public static void Dispose()
        {
            RenderTarget.Dispose();
            SwapChain.Dispose();
            Device.Dispose();
        }
    }
}
