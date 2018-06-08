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
        public static Panel Panel1;
        public static SplitContainer splitContainer1;
        public static Label nicknameLabel;
        public static CheckBox networkedCheckbox;
        public static TextBox nicknameTextbox;
        public static TextBox ipTextbox;
        public static Label ipLabel;
        public static Button connectButton;
        public static PictureBox pictureBox1;
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
            Panel1 = new Panel();
            splitContainer1 = new SplitContainer();
            networkedCheckbox = new CheckBox();
            nicknameLabel = new Label();
            nicknameTextbox = new TextBox();
            ipTextbox = new TextBox();
            ipLabel = new Label();
            connectButton = new Button();
            pictureBox1 = new PictureBox();
            // 
            // Panel1
            // 
            Panel1.Controls.Add(splitContainer1);
            Panel1.Dock = DockStyle.Top;
            Panel1.Location = new Point(0, 0);
            Panel1.Name = "Panel1";
            Panel1.Size = new Size(784, 45);
            Panel1.TabIndex = 0;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(nicknameTextbox);
            splitContainer1.Panel1.Controls.Add(nicknameLabel);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(connectButton);
            splitContainer1.Panel2.Controls.Add(networkedCheckbox);
            splitContainer1.Panel2.Controls.Add(ipTextbox);
            splitContainer1.Panel2.Controls.Add(ipLabel);
            splitContainer1.Size = new Size(784, 45);
            splitContainer1.SplitterDistance = 224;
            splitContainer1.TabIndex = 0;
            // 
            // networkedCheckbox
            // 
            networkedCheckbox.AutoSize = true;
            networkedCheckbox.Font = new Font("Dimbo", 15.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            networkedCheckbox.Location = new Point(240, 9);
            networkedCheckbox.Name = "networkedCheckbox";
            networkedCheckbox.Size = new Size(109, 29);
            networkedCheckbox.TabIndex = 0;
            networkedCheckbox.Text = "Networked";
            networkedCheckbox.UseVisualStyleBackColor = true;
            // 
            // nicknameLabel
            // 
            nicknameLabel.AutoSize = true;
            nicknameLabel.Font = new Font("Dimbo", 15.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            nicknameLabel.Location = new Point(3, 9);
            nicknameLabel.Name = "nicknameLabel";
            nicknameLabel.Size = new Size(76, 25);
            nicknameLabel.TabIndex = 0;
            nicknameLabel.Text = "Nickname";
            // 
            // nicknameTextbox
            // 
            nicknameTextbox.Location = new Point(77, 13);
            nicknameTextbox.Name = "nicknameTextbox";
            nicknameTextbox.Size = new Size(142, 20);
            nicknameTextbox.TabIndex = 1;
            // 
            // ipTextbox
            // 
            ipTextbox.Location = new Point(89, 13);
            ipTextbox.Name = "ipTextbox";
            ipTextbox.Size = new Size(142, 20);
            ipTextbox.TabIndex = 3;
            // 
            // ipLabel
            // 
            ipLabel.AutoSize = true;
            ipLabel.Font = new Font("Dimbo", 15.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            ipLabel.Location = new Point(3, 9);
            ipLabel.Name = "ipLabel";
            ipLabel.Size = new Size(87, 25);
            ipLabel.TabIndex = 2;
            ipLabel.Text = "IP Address";
            // 
            // connectButton
            // 
            connectButton.Font = new Font("Dimbo", 15.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            connectButton.Location = new Point(352, 3);
            connectButton.Name = "connectButton";
            connectButton.Size = new Size(199, 39);
            connectButton.TabIndex = 4;
            connectButton.Text = "Connect";
            connectButton.UseVisualStyleBackColor = true;

            pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBox1.Image = new Bitmap(Constants.Logo);
            pictureBox1.Location = new System.Drawing.Point(0, 45);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(784, 516);
            pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;


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
            FormToShowOn.Controls.Add(Panel1);
            FormToShowOn.Controls.Add(pictureBox1);
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
