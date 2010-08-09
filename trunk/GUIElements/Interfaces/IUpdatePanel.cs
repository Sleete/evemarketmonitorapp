using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace EveMarketMonitorApp.GUIElements.Interfaces
{
    public delegate void APIUpdateEvent(object myObject, APIUpdateEventArgs args);

    /// <summary>
    /// interface which the various update panel implementations must implement
    /// </summary>
    public interface IUpdatePanel
    {
        int Width { get; set; }
        int Height { get; set; }
        Size Size { get; set; }
        Point Location { get; set; }
        AnchorStyles Anchor { get; set; }

        void UpdateData();
        void Dispose();

        event APIUpdateEvent UpdateEvent;
    }
}
