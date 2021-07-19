using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShortcutOverlay
{ 
    class ButtonInformation
    {
        public ButtonPrefab ButtonPrefab { get; set; }
        public int selectedIndex { get; set; }
        public int selectedIndex2 { get; set; }
        public string ShortCut { get; set; }

        public string buttonName;
        public string ProcName { get; set; }
    }
}
