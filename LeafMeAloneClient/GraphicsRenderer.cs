using System;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using Device = SlimDX.Direct3D11.Device;
using Resource = SlimDX.Direct3D11.Resource;
using System.Windows.Forms;
using Client.UI;
using Shared;
using Button = System.Windows.Forms.Button;
using Screen = System.Windows.Forms.Screen;

namespace Client
{
    internal static class GraphicsRenderer
    {

        /// <summary>
        /// The main form which is going to render to the screen.
        /// </summary>
        public static RenderForm Form;

#if DEBUG
        public static Form DebugForm;
#endif

        /// <summary>
        /// Device is an adapter used to render.
        /// </summary>
        public static Device Device;
        public static DeviceContext DeviceContext;

        /// <summary>
        /// Swap chain holds surfaces which contain render data before putting them on screen.
        /// </summary>
        public static SwapChain SwapChain;

        //Render Target
        public static RenderTargetView RenderTarget;

        //Viewport for screen view.
        public static Viewport Viewport;

        public static Matrix ProjectionMatrix;
        
        #region FormConnections
        public static SplitContainer splitContainer1;
        public static Label nicknameLabel;
        public static CheckBox networkedCheckbox;
        public static TextBox nicknameTextbox;
        public static TextBox ipTextbox;
        public static Label ipLabel;
        public static Button connectButton;
        public static Panel panel1;
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


        private static void InitializeRasterizer()
        {
            Rasterizer = new RasterizerStateDescription()
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.Back,
                IsFrontCounterclockwise = false,
                IsDepthClipEnabled = true
            };
            DeviceContext.Rasterizer.State = RasterizerState.FromDescription(Device, Rasterizer);
        }

        private static void InitializeDepthBuffer()
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

        private static void InitializeBlending()
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
           
            panel1 = new System.Windows.Forms.Panel();
            connectButton = new System.Windows.Forms.Button();
            networkedCheckbox = new System.Windows.Forms.CheckBox();
            ipTextbox = new System.Windows.Forms.TextBox();
            ipLabel = new System.Windows.Forms.Label();
            nicknameTextbox = new System.Windows.Forms.TextBox();
            nicknameLabel = new System.Windows.Forms.Label();

            // panel1
            // 
            panel1.BackgroundImage = new Bitmap(Constants.Logo);
            panel1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            panel1.Controls.Add(connectButton);
            panel1.Controls.Add(networkedCheckbox);
            panel1.Controls.Add(ipTextbox);
            panel1.Controls.Add(ipLabel);
            panel1.Controls.Add(nicknameTextbox);
            panel1.Controls.Add(nicknameLabel);
            panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            panel1.Location = new System.Drawing.Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(784, 561);
            panel1.TabIndex = 9;
            // 
            // connectButton
            // 
            connectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            connectButton.BackColor = System.Drawing.Color.LightGray;
            connectButton.Font = new System.Drawing.Font("Dimbo", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            connectButton.Location = new System.Drawing.Point(585, 4);
            connectButton.Name = "connectButton";
            connectButton.Size = new System.Drawing.Size(196, 39);
            connectButton.TabIndex = 14;
            connectButton.Text = "Connect";
            connectButton.UseVisualStyleBackColor = false;
            // 
            // networkedCheckbox
            // 
            networkedCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            networkedCheckbox.AutoSize = true;
            networkedCheckbox.BackColor = System.Drawing.Color.Transparent;
            networkedCheckbox.Font = new System.Drawing.Font("Dimbo", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            networkedCheckbox.ForeColor = System.Drawing.Color.White;
            networkedCheckbox.Location = new System.Drawing.Point(480, 9);
            networkedCheckbox.Name = "networkedCheckbox";
            networkedCheckbox.Size = new System.Drawing.Size(109, 29);
            networkedCheckbox.TabIndex = 11;
            networkedCheckbox.Text = "Networked";
            networkedCheckbox.UseVisualStyleBackColor = false;
            // 
            // ipTextbox
            // 
            ipTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            ipTextbox.Location = new System.Drawing.Point(332, 13);
            ipTextbox.Name = "ipTextbox";
            ipTextbox.Size = new System.Drawing.Size(142, 20);
            ipTextbox.TabIndex = 13;
            // 
            // ipLabel
            // 
            ipLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            ipLabel.AutoSize = true;
            ipLabel.BackColor = System.Drawing.Color.Transparent;
            ipLabel.Font = new System.Drawing.Font("Dimbo", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            ipLabel.ForeColor = System.Drawing.Color.White;
            ipLabel.Location = new System.Drawing.Point(239, 11);
            ipLabel.Name = "ipLabel";
            ipLabel.Size = new System.Drawing.Size(87, 25);
            ipLabel.TabIndex = 12;
            ipLabel.Text = "IP Address";
            // 
            // nicknameTextbox
            // 
            nicknameTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            nicknameTextbox.Location = new System.Drawing.Point(91, 13);
            nicknameTextbox.Name = "nicknameTextbox";
            nicknameTextbox.Size = new System.Drawing.Size(142, 20);
            nicknameTextbox.TabIndex = 10;
            // 
            // nicknameLabel
            // 
            nicknameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            nicknameLabel.AutoSize = true;
            nicknameLabel.BackColor = System.Drawing.Color.Transparent;
            nicknameLabel.Font = new System.Drawing.Font("Dimbo", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            nicknameLabel.ForeColor = System.Drawing.Color.White;
            nicknameLabel.Location = new System.Drawing.Point(9, 11);
            nicknameLabel.Name = "nicknameLabel";
            nicknameLabel.Size = new System.Drawing.Size(76, 25);
            nicknameLabel.TabIndex = 9;
            nicknameLabel.Text = "Nickname";



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
            networkedCheckbox.CheckedChanged += (sender, args) => { ipTextbox.Enabled = networkedCheckbox.Checked; };

            if (ipTextbox.Text == "" && networkedCheckbox.Checked)
            {
                var ip = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ipAddress in ip.AddressList)
                {
                    if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                        ipTextbox.Text = ipAddress.ToString();
                }
            }

            ipTextbox.Enabled = networkedCheckbox.Checked;

            // 
            // Form1
            // 
            FormToShowOn.Controls.Add(panel1);
            int pnum = Process.GetProcessesByName("LeafMeAloneClient").Length;
            var positions = new[] {new Vector2(0, 0), new Vector2(800, 0), new Vector2(0, 600), new Vector2(800, 600) };
            FormToShowOn.SetDesktopLocation((int)positions[(pnum-1) % 4].X, (int)positions[(pnum-1) % 4].Y);

        }
        /// <summary>
        /// Initialize graphics properties and create the main window.
        /// </summary>
        public static void Init()
        {

            //Make the Form
            Form = new RenderForm("LeafMeAlone");

            Device = new Device(DriverType.Hardware,DeviceCreationFlags.None);
            var msaa = Device.CheckMultisampleQualityLevels(Format.R8G8B8A8_UNorm, 4);
            
            SwapChainDescription description = new SwapChainDescription()
            {
                BufferCount = 1,
                Usage = Usage.RenderTargetOutput,
                OutputHandle = Form.Handle,
                IsWindowed = true,
                ModeDescription = new ModeDescription(0,0, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                SampleDescription = /*msaa != 0 ? new SampleDescription(4,msaa) : */new SampleDescription(1, 0),
                Flags = SwapChainFlags.None,
                SwapEffect = SwapEffect.Discard
            };


            SwapChain = new SwapChain(Device.Factory,Device,description);

            //create new device (with directx) which can be used throughout the project.
           // Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, description, out Device, out SwapChain);



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

#if DEBUG
            TextBox debugTextbox = new TextBox { Multiline = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical };
            DebugForm = new Form();
            DebugForm.Controls.Add(debugTextbox);
            DebugForm.Closing += (sender, args) =>
            {
                args.Cancel = true;
                DebugForm.Hide();
            };
            Debug.Init(debugTextbox);
#endif

            // handle alt+enter ourselves
            Form.KeyDown += (o, e) =>
            {
                if (e.Shift && e.KeyCode == Keys.Enter)
                {
                    Form.Size =  new Size(Screen.PrimaryScreen.WorkingArea.Width,Screen.PrimaryScreen.WorkingArea.Height);
                    SwapChain.IsFullScreen = !SwapChain.IsFullScreen;
                }

                if (e.KeyCode == Keys.Escape)
                    Application.Exit();
#if DEBUG
                if (e.Control && e.KeyCode == Keys.Enter)
                    DebugForm.Show();
#endif
            };
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
