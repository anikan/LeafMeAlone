﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Client;
using Shared;
using SlimDX;
using SlimDX.DXGI;
using SlimDX.Windows;


namespace Client
{
    static class GameClient
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            GraphicsRenderer.Init();
            MessagePump.Run(GraphicsRenderer.Form, DoGameLoop);
            GraphicsRenderer.Dispose();
        }

        private static void DoGameLoop()
        {
            GraphicsRenderer.DeviceContext.ClearRenderTargetView(GraphicsRenderer.RenderTarget, new Color4(0.5f, 0.5f, 1.0f));
            GraphicsRenderer.SwapChain.Present(0, PresentFlags.None);
        }

    }
}
