using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;
using AntTweakBar;
using SlimDX.Windows;

namespace Client
{
    class UserInterface
    {

    }
}

/// <summary>
/// This form is just like a RenderForm, except it has an AntTweakBar
/// context and AntTweakBar events are handled transparently to the
/// user. It does nothing special until you assign a Context to it.
/// 
/// Feel free to use it in your own applications, it's quite handy.
/// </summary>
public class ATBRenderForm : RenderForm
{
    public Context Context { get; set; }

    public ATBRenderForm() : base() { }
    public ATBRenderForm(String text) : base(text) { }

    protected override void WndProc(ref System.Windows.Forms.Message m)
    {
        /* We could have handled all the mouse and keyboard events
         * separately, but AntTweakBar has a handy function that
         * can automatically hook into the Windows message pump.
        */

        if ((Context == null) || !Context.EventHandlerWin(m.HWnd, m.Msg, m.WParam, m.LParam))
        {
            base.WndProc(ref m);
        }
    }
};