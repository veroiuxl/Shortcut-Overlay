using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ShortcutOverlay
{
    [DataContract(Name = "Saved Layout", Namespace = "SaveableButton")]
    class SaveableButton : IExtensibleDataObject
    {
        public ButtonPrefab ButtonPrefab { get; set; }
        [DataMember()]
        public double[] windowsSize = new double[2];
        [DataMember()]
        public double[] windowsPos = new double[2];
        [DataMember()]
        public string buttonName;
        [DataMember()]
        public int extendedKey1;
        [DataMember()]
        public int extendedKey2;
        [DataMember()]
        public string shortcut;
        [DataMember()]
        public string procName;
        public SaveableButton(double[] newWindowsSize, double[] newWindowsPos, string newButtonName,int newExtendedKey1,int newExtendedKey2,string newShortcut,string newProcessName)
        {
            windowsSize = newWindowsSize;
            windowsPos = newWindowsPos;
            buttonName = newButtonName;
            extendedKey1 = newExtendedKey1;
            extendedKey2 = newExtendedKey2;
            shortcut = newShortcut;
            procName = newProcessName;
        }
        public ExtensionDataObject ExtensionData { get; set; }
    }
}
