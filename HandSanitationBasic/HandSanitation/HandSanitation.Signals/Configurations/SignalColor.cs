using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HandSanitation.Signals.Configurations
{
    public enum ECurrentAction
    {
        Complete,
        EnteredTheRoom,
        TagAttached,
        NotActive
    }
    public class SignalColor
    {
        private ECurrentAction CurrentAction { get; set; }
        private Color LedColor { get; set; } 
    }
}
