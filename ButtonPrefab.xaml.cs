using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace ShortcutOverlay
{
    /// <summary>
    /// Interaction logic for ButtonPrefab.xaml
    /// </summary>
    public partial class ButtonPrefab : Window
    {
        private string shortCut= "";
        private IntPtr applicationPointer;
        public string buttonName;
        private string windowName;
        private Keyboard.VirtualKeyShort extendedKey;
        private Keyboard.VirtualKeyShort extendedKey2;
        private Keyboard.VirtualKeyShort addKey;
        private bool hasSecondExtendedKeyOnly = false;
        private bool writeShortcut = false;
        private bool verbose = false;

        public ButtonPrefab()
        {
        }

        [DllImport("User32.dll")]

        static extern int SetForegroundWindow(IntPtr point);

        [DllImport("user32.dll", SetLastError = true)]


        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);


        private void HandleShortcut()
        {
            if (!Keyboard.ValidApplication(windowName))
            {
                MessageBox.Show("Application not found.", "Error!",MessageBoxButton.OK,MessageBoxImage.Error);
                return;
            }
            SetForegroundWindow(applicationPointer);


            if (writeShortcut || (extendedKey == Keyboard.VirtualKeyShort.NONCONVERT && extendedKey2 == Keyboard.VirtualKeyShort.NONCONVERT))
            {
                WriteText(shortCut);
            }
            else
            {
                Debug.WriteLine("addKey " + addKey + " exK " + extendedKey + " exK2 " + extendedKey2);
                if (hasSecondExtendedKeyOnly)
                {
                    PressOrReleaseKeyForShortcut(addKey, extendedKey,Keyboard.VirtualKeyShort.NONCONVERT);
                }
                else
                {
                    PressOrReleaseKeyForShortcut(addKey, extendedKey, extendedKey2);
                }
            }
           /* MessageBox.Show("add key " + addKey + " | eK  " + extendedKey);*/
            /*
              SetForegroundWindow(notepad);
                    SendKeys.Send("+{F11}");//shift+F11
                        for (int i = 0; i < shortCutData.Length; i++)
                        {

                        }*/
        }
        private static Keyboard.VirtualKeyShort GetKeyBySelectedIndex(int index)
        {
            short keyIn;
            switch (index)
            {
                case 0:
                    // Nothing
                    keyIn = (short)Keyboard.VirtualKeyShort.PACKET;
                    break;
                case 1:
                    // CTRL
                    keyIn = (short)Keyboard.VirtualKeyShort.CONTROL;
                    break;
                case 2:
                    // ALT
                    keyIn = (short)0x12;
                    break;
                case 3:
                    // SHIFT
                    keyIn =  (short)Keyboard.VirtualKeyShort.SHIFT;
                    break;
                default:
                    keyIn = (short)Keyboard.VirtualKeyShort.PACKET;
                    break;
            }

            return GetKeyByShort(keyIn);

        }

        public Rect GetWindowRect()
        {
         /*   Rect rect = new Rect();
            rect.Width = this.buttonForm.Width;
            rect.Height = this.buttonForm.Height;
            rect.Bottom = this.buttonForm.RestoreBounds.
            rect.X = this.buttonForm.*/
         return this.buttonForm.RestoreBounds;
        }

        public void SetPosAndScale(double[] position,double[] rect)
        {

            Rect windowRect = new Rect(rect[0],rect[1], rect[2], rect[3]);
            this.buttonForm.Height = windowRect.Height;
            this.buttonForm.Width = windowRect.Width; 
            this.buttonForm.Left = windowRect.X;
            this.buttonForm.Top = windowRect.Y;
            
        }
       
        public void SetPrefabInstance(string name, int selectedIndex, int selectedIndex2, string shortcut,string wn)
        {

            InitializeComponent();
            buttonForm.Focusable = false;
            prefabButton.Focusable = false;
            prefabButton.Content = name;
            buttonName = name;
            shortCut = shortcut;
            windowName = wn;
            addKey = ConvertSpecialKeysToKeyCode(shortcut);
            if (selectedIndex == 0 && selectedIndex2 == 0 && addKey == Keyboard.VirtualKeyShort.NONCONVERT)
                writeShortcut = true;
            if (addKey == Keyboard.VirtualKeyShort.NONCONVERT)
            {
                addKey = GetKeyByChar(shortcut.ToCharArray()[0]);
            }

            extendedKey = GetKeyBySelectedIndex(selectedIndex);
            extendedKey2 = GetKeyBySelectedIndex(selectedIndex2);
            prefabButton.Content = name;
            if (extendedKey == Keyboard.VirtualKeyShort.NONCONVERT)
            {
                MessageBox.Show("First shortcut could not be converted into a key!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                hasSecondExtendedKeyOnly = false; 
            }


            if (extendedKey2 == Keyboard.VirtualKeyShort.NONCONVERT)
            {
                MessageBox.Show("Second shortcut could not be converted into a key!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                hasSecondExtendedKeyOnly = true;
            }
            if (name.Trim().StartsWith("verbose"))
            {
                verbose = true;
                MessageBox.Show("Verbose enabled");
                MessageBox.Show("extended Key " + extendedKey + "extended2 Key " + extendedKey2 + " | add Key  " + addKey + "\nshortcut " + shortcut);
            }
            try
            {
                applicationPointer = FindWindow(windowName, null);
            }
            catch (Exception exe)
            {
                MessageBox.Show("Could not focus on window!","Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            prefabButton.Click += (sender, args) => { HandleShortcut(); };

        }

        private bool clicked = false;
        private Point lmAbs = new Point();

        private void prefabButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            clicked = true;
            this.lmAbs = e.GetPosition(this);
            this.lmAbs.Y = Convert.ToInt16(this.Top) + this.lmAbs.Y;
            this.lmAbs.X = Convert.ToInt16(this.Left) + this.lmAbs.X;
        }

        private void prefabButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            clicked = false;
        }

        // https://stackoverflow.com/questions/16608523/c-sharp-wpf-move-the-window
        private void prefabButton_MouseMove(object sender, MouseEventArgs e)
        {
            if (clicked)
            {
                Point MousePosition = e.GetPosition(this);
                Point MousePositionAbs = new Point();
                MousePositionAbs.X = Convert.ToInt16(this.Left) + MousePosition.X;
                MousePositionAbs.Y = Convert.ToInt16(this.Top) + MousePosition.Y;
                this.Left = this.Left + (MousePositionAbs.X - this.lmAbs.X);
                this.Top = this.Top + (MousePositionAbs.Y - this.lmAbs.Y);
                this.lmAbs = MousePositionAbs;
            }
        }
        private enum InputType
        {
            INPUT_MOUSE = 0,
            INPUT_KEYBOARD = 1,
            INPUT_HARDWARE = 2,
        }
        // https://github.com/kpreisser/MouseClickSimulator/blob/master/TTMouseclickSimulator/Core/Environment/AbstractWindowsEnvironment.cs
        public void WriteText(string characters)
        {
            var inputs = new Keyboard.INPUT[2 * characters.Length];
            for (int i = 0; i < inputs.Length; i++)
            {
                var ki = new Keyboard.KEYBDINPUT
                {
                    dwFlags = Keyboard.KEYEVENTF.UNICODE
                };
                if (i % 2 == 1)
                    ki.dwFlags |= Keyboard.KEYEVENTF.KEYUP;
                ki.wScan = (Keyboard.ScanCodeShort)(short)characters[i / 2];

                var input = new Keyboard.INPUT();
                input.type = (int)InputType.INPUT_KEYBOARD;
                input.U.ki = ki;

                inputs[i] = input;
            }

            if (Keyboard.SendInput((uint)inputs.Length, inputs, Keyboard.INPUT.Size) == 0)
                throw new Win32Exception();
        }

        private void PressOrReleaseKeyForShortcut(Keyboard.VirtualKeyShort additionalKey, Keyboard.VirtualKeyShort extendedKey,Keyboard.VirtualKeyShort extendedKey2)
        {
            //Press (and hold) extended key (Control, Shift,Alt)
            var ki = new Keyboard.KEYBDINPUT
            {
                wVk = extendedKey
            };
            ki.dwFlags = 0;

            var input = new Keyboard.INPUT();
            input.type = (int)InputType.INPUT_KEYBOARD;
            input.U.ki = ki;
            //Press (and hold) any other key
            var ki2 = new Keyboard.KEYBDINPUT
            {
                wVk = additionalKey
            };
            ki2.dwFlags = 0;

            var addKeyInput2 = new Keyboard.INPUT();
            addKeyInput2.type = (int)InputType.INPUT_KEYBOARD;
            addKeyInput2.U.ki = ki2;


            var addKeyInput3 = new Keyboard.INPUT();
            if (extendedKey2 != Keyboard.VirtualKeyShort.NONCONVERT)
            {
                //Press (and hold) any second key
                var ki3 = new Keyboard.KEYBDINPUT
                {
                    wVk = extendedKey2
                };
                ki3.dwFlags = 0;

                addKeyInput3.type = (int)InputType.INPUT_KEYBOARD;
                addKeyInput3.U.ki = ki3;
            }

            // Release extended key
            var kiRelease = new Keyboard.KEYBDINPUT
            {
                wVk = extendedKey
            };
            kiRelease.dwFlags = Keyboard.KEYEVENTF.KEYUP;

            var inputRelease = new Keyboard.INPUT();
            inputRelease.type = (int)InputType.INPUT_KEYBOARD;
            inputRelease.U.ki = kiRelease;

            //Release other key
            var kiaddKeyRelease = new Keyboard.KEYBDINPUT
            {
                wVk = additionalKey
            };
            kiaddKeyRelease.dwFlags = Keyboard.KEYEVENTF.KEYUP;

            var inputaddKeyRelease = new Keyboard.INPUT();
            inputaddKeyRelease.type = (int)InputType.INPUT_KEYBOARD;
            inputaddKeyRelease.U.ki = kiaddKeyRelease;

            var addKeyCode2Release = new Keyboard.INPUT();
            if (extendedKey2 != Keyboard.VirtualKeyShort.NONCONVERT)
            {
                // Release key second key
                var kiKeyCode2Release = new Keyboard.KEYBDINPUT
                {
                    wVk = extendedKey2
                };
                kiKeyCode2Release.dwFlags = Keyboard.KEYEVENTF.KEYUP;

                addKeyCode2Release.type = (int) InputType.INPUT_KEYBOARD;
                addKeyCode2Release.U.ki = kiKeyCode2Release;
            }

            // Send both at the same time
            if (extendedKey2 != Keyboard.VirtualKeyShort.NONCONVERT)
            {
                Keyboard.INPUT[] inputs = { input, addKeyInput2, addKeyInput3, inputRelease, inputaddKeyRelease, addKeyCode2Release};
                if (Keyboard.SendInput((uint)inputs.Length, inputs, Keyboard.INPUT.Size) == 0)
                    throw new Win32Exception();
            }
            else
            {
                Keyboard.INPUT[] inputs = { input, addKeyInput2, inputRelease, inputaddKeyRelease };
                if (Keyboard.SendInput((uint)inputs.Length, inputs, Keyboard.INPUT.Size) == 0)
                    throw new Win32Exception();
            }
        }
        // https://github.com/kpreisser/MouseClickSimulator/blob/master/TTMouseclickSimulator/Core/Environment/AbstractWindowsEnvironment.cs
        private void PressOrReleaseKey(Keyboard.VirtualKeyShort keyCode, bool down)
        {
            var ki = new Keyboard.KEYBDINPUT
            {
                wVk = keyCode
            };
            if (!down)
                ki.dwFlags = Keyboard.KEYEVENTF.KEYUP;

            var input = new Keyboard.INPUT();
            input.type = (int)InputType.INPUT_KEYBOARD;
            input.U.ki = ki;

            Keyboard.INPUT[] inputs = { input };

            if (Keyboard.SendInput((uint)inputs.Length, inputs, Keyboard.INPUT.Size) == 0)
                throw new Win32Exception();
        }

        private Keyboard.VirtualKeyShort GetKeyByChar(char c)
        {
            try
            {
                Keyboard.VirtualKeyShort k = (Keyboard.VirtualKeyShort) char.ToUpper(c);
                if (verbose)
                {
                    MessageBox.Show("GetKeyByChar is " + k + " input char is " + c,"Verbose");
                }
                return k;
            }
            catch (Exception exe)
            {
                MessageBox.Show("Could not convert shortcut to key. " + exe.Message,"Error",MessageBoxButton.OK,MessageBoxImage.Error);
                return Keyboard.VirtualKeyShort.KEY_S;
            }
        }

        private Keyboard.VirtualKeyShort ConvertSpecialKeysToKeyCode(string specialKey)
        {
            
            switch(specialKey.Trim().ToUpper())
            {
                case "F1":
                    return Keyboard.VirtualKeyShort.F1;
                case "F2":
                    return Keyboard.VirtualKeyShort.F2;
                case "F3":
                    return Keyboard.VirtualKeyShort.F3;
                case "F4":
                    return Keyboard.VirtualKeyShort.F4;
                case "F5":
                    return Keyboard.VirtualKeyShort.F5;
                case "F6":
                    return Keyboard.VirtualKeyShort.F6;
                case "F7":
                    return Keyboard.VirtualKeyShort.F7;
                case "F8":
                    return Keyboard.VirtualKeyShort.F8;
                case "F9":
                    return Keyboard.VirtualKeyShort.F9;
                case "F10":
                    return Keyboard.VirtualKeyShort.F10;
                case "F11":
                    return Keyboard.VirtualKeyShort.F11;
                case "F12":
                    return Keyboard.VirtualKeyShort.F12;
                case "DELETE":
                    return Keyboard.VirtualKeyShort.DELETE;
                case "PRINT":
                    return Keyboard.VirtualKeyShort.PRINT;
                case "TAB":
                    return Keyboard.VirtualKeyShort.TAB;
                case "UP":
                    return Keyboard.VirtualKeyShort.UP;
                case "DOWN":
                    return Keyboard.VirtualKeyShort.DOWN;
                case "RIGHT":
                    return Keyboard.VirtualKeyShort.RIGHT;
                case "LEFT":
                    return Keyboard.VirtualKeyShort.LEFT;
                case "BACKSPACE":
                    return Keyboard.VirtualKeyShort.BACK;
                default:
                return Keyboard.VirtualKeyShort.NONCONVERT;
            }
        }
        private static Keyboard.VirtualKeyShort GetKeyByShort(short c)
        {
            Keyboard.VirtualKeyShort k = (Keyboard.VirtualKeyShort)c;
            return k;
        }
    }
}
