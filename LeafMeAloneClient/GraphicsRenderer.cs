﻿using System;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using Device = SlimDX.Direct3D11.Device;
using Resource = SlimDX.Direct3D11.Resource;
using System.Windows.Forms;
using AntTweakBar;
using Button = AntTweakBar.Button;

namespace Client
{
    static class GraphicsRenderer
    {

        /// <summary>
        /// The main form which is going to render to the screen.
        /// </summary>
        public static RenderForm Form;

        public static Form DebugForm;

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

        public static Context BarContext;


        #region FormConnections
        public static Panel Panel1;
        public static Label nicknameLabel;
        public static Label ipLabel;
        public static TextBox ipTextbox;
        public static TextBox nicknameTextbox;
        public static System.Windows.Forms.Button connectButton;
        #endregion


        #region Depth Buffer and Rasterizer
        private static Texture2DDescription depthBufferDesc;
        private static Texture2D depthBuffer;
        public static DepthStencilView DepthView;
        public static DepthStencilState DepthState;
        private static DepthStencilStateDescription dsStateDesc;

        public static DepthStencilState DepthStateOff;
        private static DepthStencilStateDescription dsStateDescOff;

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

            depthBuffer = new Texture2D(Device, depthBufferDesc);
            DepthView = new DepthStencilView(Device, depthBuffer);

            dsStateDesc = new DepthStencilStateDescription()
            {
                IsDepthEnabled = true,
                IsStencilEnabled = false,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.Less,
            };
            dsStateDescOff = new DepthStencilStateDescription()
            {
                IsDepthEnabled = true,
                IsStencilEnabled = false,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.Less,
            };
            DepthStateOff = DepthStencilState.FromDescription(Device, dsStateDescOff);
            DepthState = DepthStencilState.FromDescription(Device, dsStateDesc);

            DeviceContext.OutputMerger.DepthStencilState = DepthState;
        }

        static void InitializeBlending()
        {
            BlendStateDescription bs = new BlendStateDescription()
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false,
            };
            for (int i = 0; i < bs.RenderTargets.Length; i++)
            {
                bs.RenderTargets[i].BlendEnable = true;
                bs.RenderTargets[i].BlendOperation = BlendOperation.Add;
                bs.RenderTargets[i].SourceBlend = BlendOption.SourceAlpha;
                bs.RenderTargets[i].DestinationBlend = BlendOption.InverseSourceAlpha;


                bs.RenderTargets[i].BlendOperationAlpha = BlendOperation.Add;
                bs.RenderTargets[i].SourceBlendAlpha = BlendOption.Zero;
                bs.RenderTargets[i].DestinationBlendAlpha = BlendOption.Zero;



                bs.RenderTargets[i].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            }
            BlendState = BlendState.FromDescription(Device, bs);
        }

        private static void InitializeComponent(Form FormToShowOn)
        {
            Panel1 = new Panel();
            nicknameLabel = new Label();
            ipLabel = new Label();
            ipTextbox = new TextBox();
            nicknameTextbox = new TextBox();
            connectButton = new System.Windows.Forms.Button();
            // 
            // Panel1
            // 
            Panel1.Controls.Add(nicknameLabel);
            Panel1.Controls.Add(ipLabel);
            Panel1.Controls.Add(ipTextbox);
            Panel1.Controls.Add(nicknameTextbox);
            Panel1.Controls.Add(connectButton);
            Panel1.Dock = DockStyle.Top;
            Panel1.Location = new System.Drawing.Point(0, 0);
            Panel1.Name = "Panel1";
            Panel1.Size = new System.Drawing.Size(800, 37);
            Panel1.TabIndex = 1;
            // 
            // nicknameLabel
            // 
            nicknameLabel.AutoSize = true;
            nicknameLabel.Location = new System.Drawing.Point(313, 13);
            nicknameLabel.Name = "nicknameLabel";
            nicknameLabel.Size = new System.Drawing.Size(55, 13);
            nicknameLabel.TabIndex = 6;
            nicknameLabel.Text = "Nickname";
            // 
            // ipLabel
            // 
            ipLabel.AutoSize = true;
            ipLabel.Location = new System.Drawing.Point(7, 13);
            ipLabel.Name = "ipLabel";
            ipLabel.Size = new System.Drawing.Size(58, 13);
            ipLabel.TabIndex = 5;
            ipLabel.Text = "IP Address";
            // 
            // ipTextbox
            // 
            ipTextbox.Location = new System.Drawing.Point(71, 10);
            ipTextbox.Name = "ipTextbox";
            ipTextbox.Size = new System.Drawing.Size(225, 20);
            ipTextbox.TabIndex = 4;
            ipTextbox.Text = Properties.Settings.Default.IP;
            // 
            // nicknameTextbox
            // 
            nicknameTextbox.Location = new System.Drawing.Point(377, 10);
            nicknameTextbox.Name = "nicknameTextbox";
            nicknameTextbox.Size = new System.Drawing.Size(225, 20);
            nicknameTextbox.TabIndex = 3;
            
            // 
            // connectButton
            // 
            connectButton.Location = new System.Drawing.Point(608, 8);
            connectButton.Name = "connectButton";
            connectButton.Size = new System.Drawing.Size(180, 23);
            connectButton.TabIndex = 0;
            connectButton.Text = "Connect";
            connectButton.UseVisualStyleBackColor = true;


            nicknameTextbox.AcceptsReturn = true;
            connectButton.NotifyDefault(true);

            nicknameTextbox.KeyPress += (sender, args) =>
            {
                if (args.KeyChar == '\t')
                {
                    connectButton.Focus();
                }
            };

            ipTextbox.KeyPress += (sender, args) =>
            {
                if (args.KeyChar == '\t')
                {
                    nicknameTextbox.Focus();
                }
            };
            ipTextbox.TextChanged += (sender, args) =>
            {
                Properties.Settings.Default.IP = ipTextbox.Text; 
                Properties.Settings.Default.Save();
            };


            // 
            // Form1
            // 
            FormToShowOn.Controls.Add(Panel1);

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


            TextBox debugTextbox = new TextBox { Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical };
            DebugForm = new Form();
            DebugForm.Controls.Add(debugTextbox);
            DebugForm.Closing += (sender, args) =>
            {
                args.Cancel = true;
                DebugForm.Hide();
            };
            Debug.Init(debugTextbox);

            // handle alt+enter ourselves
            Form.KeyDown += (o, e) =>
            {
                if (e.Shift && e.KeyCode == Keys.Enter)
                    SwapChain.IsFullScreen = !SwapChain.IsFullScreen;
                if (e.KeyCode == Keys.Escape)
                    Application.Exit();
                if (e.Control && e.KeyCode == Keys.Enter)
                    DebugForm.Show();
            };

            BarContext = new Context(Tw.GraphicsAPI.D3D11, Device.ComPointer);
            BarContext.HandleResize(Form.ClientSize);
            
            InitializeComponent(Form);
        }
        /// <summary>
        /// Method called when the form is resized by the user.
        /// </summary>
        /// <param name="sender">sending form</param>
        /// <param name="eventArgs">event argument</param>
        private static void FormOnResize(object sender, EventArgs eventArgs)
        {
            RenderTarget.Dispose();
            DepthView.Dispose();
            depthBuffer.Dispose();


            DeviceContext = Device.ImmediateContext;

            DeviceContext.Rasterizer.State = RasterizerState.FromDescription(Device, Rasterizer);

            depthBufferDesc.Width = Form.ClientSize.Width;
            depthBufferDesc.Height = Form.ClientSize.Height;


            depthBuffer = new Texture2D(Device, depthBufferDesc);
            DepthView = new DepthStencilView(Device, depthBuffer);


            DepthState = DepthStencilState.FromDescription(Device, dsStateDesc);
            DeviceContext.OutputMerger.DepthStencilState = DepthState;



            SwapChain.ResizeBuffers(2, Form.ClientSize.Width, Form.ClientSize.Height, Format.R8G8B8A8_UNorm, SwapChainFlags.AllowModeSwitch);
            using (var resource = Resource.FromSwapChain<Texture2D>(SwapChain, 0))
                RenderTarget = new RenderTargetView(Device, resource);

            DeviceContext.OutputMerger.SetTargets(DepthView, RenderTarget);


            Viewport = new Viewport(0.0f, 0.0f, Form.ClientSize.Width, Form.ClientSize.Height);
            DeviceContext.Rasterizer.SetViewports(Viewport);
            ProjectionMatrix = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, Viewport.Width / Viewport.Height, .1f, 1000.0f);
            BarContext.HandleResize(Form.ClientSize);

            UIManagerSpriteRenderer.SpriteRenderer?.RefreshViewport();

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
