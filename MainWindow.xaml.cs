using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace ShortcutOverlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Dictionary<ButtonInformation,Label> buttonPrefabs = new Dictionary<ButtonInformation, Label>();
        private FileLayoutManager fileLayoutManager = new FileLayoutManager();
        private string selectedButtonName = "";
        public MainWindow()
        {
            InitializeComponent();
        }

       

        private ButtonPrefab AddButton(string buttonName,int selectedIndex, int selectedIndex2, string shortCut,string procName)
        {
            ButtonInformation buttonInformation = new ButtonInformation();
            ButtonPrefab newButtonInstance = new ButtonPrefab();
            Label label = new Label();
            label.Content = textBoxButtonName.Text;
            listBoxButtons.Items.Add(label);
            buttonInformation.ButtonPrefab = newButtonInstance;
            buttonInformation.buttonName = textBoxButtonName.Text;
            buttonInformation.selectedIndex = selectedIndex;
            buttonInformation.selectedIndex2 = selectedIndex2;
            buttonInformation.ShortCut = shortCut;
            buttonInformation.ProcName = procName;
            buttonPrefabs.Add(buttonInformation,label);
            newButtonInstance.SetPrefabInstance(buttonName, selectedIndex,selectedIndex2, shortCut, procName);
            newButtonInstance.Show();
            return newButtonInstance;
        }

        
        private bool RemoveButtonByName(string buttonName)
        {
            ButtonInformation prefab = FindButtonInformationByName(buttonName);
            if (prefab == null)
            {
                return false;
            }
            try
            {
                var value = buttonPrefabs.FirstOrDefault(x => x.Key.ButtonPrefab == prefab.ButtonPrefab);
                prefab.ButtonPrefab.Close();
                listBoxButtons.Items.RemoveAt(listBoxButtons.Items.IndexOf(listBoxButtons.SelectedItem));
                buttonPrefabs.Remove(prefab);
                return true;
            }
            catch (Exception exe)
            {
                MessageBox.Show(exe.Message,"Error");
                return false;
            }
        }

        private ButtonInformation FindButtonInformationByName(string buttonName)
        {
            foreach (ButtonInformation bp in buttonPrefabs.Keys)
            {
                if (bp.ButtonPrefab.buttonName == buttonName)
                {
                    return bp;
                }
            }
            return null;
        }
        private ButtonInformation FindButtonInformationByPrefab(ButtonPrefab prefab)
        {
            foreach (ButtonInformation bp in buttonPrefabs.Keys)
            {
                if (bp.ButtonPrefab == prefab)
                {
                    return bp;
                }
            }
            return null;
        }
        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectedButtonName == null)
            {
                MessageBox.Show("Nothing selected!","Error!");
                return;
            }

            RemoveButtonByName(this.selectedButtonName);
            GC.Collect();
        }

        private void listBoxButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBoxButtons.SelectedItem != null)
                this.selectedButtonName = (string)((Label)listBoxButtons.SelectedValue).Content;
        }

        private List<SaveableButton> GetAllSaveableButtons()
        {
            List<SaveableButton> saveButtons = new List<SaveableButton>();
            foreach (ButtonInformation button in buttonPrefabs.Keys)
            {
                saveButtons.Add(GetSavableButtonFromPrefab(button.ButtonPrefab));
            }

            return saveButtons;
        }

        private void ThrowErrorMessage(string content)
        {
            MessageBox.Show(content, "Error",MessageBoxButton.OK,MessageBoxImage.Error);
        }
        private SaveableButton GetSavableButtonFromPrefab(ButtonPrefab prefab)
        {
            if (!Keyboard.ValidApplication(textBoxProcessName.Text))
            {
                ThrowErrorMessage("Application not found.");
                return null;
            }

            ButtonInformation buttonInfo = FindButtonInformationByPrefab(prefab);
            if (buttonInfo == null)
            {
                ThrowErrorMessage("Button not found.");
                return null;
            }
            Rect buttonRect = prefab.GetWindowRect();
            double[] windowRect = new[] {buttonRect.Top,buttonRect.Bottom,buttonRect.Left,buttonRect.Right };
            double[] windowPosition = new[] {buttonRect.X,buttonRect.Y};
            
            return new SaveableButton(windowRect, windowPosition, prefab.buttonName, buttonInfo.selectedIndex, buttonInfo.selectedIndex2, buttonInfo.ShortCut, buttonInfo.ProcName);
        }
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxButtonName.Text))
            {
                ThrowErrorMessage("Button name can't be nothing.");
                return;
            }
            else if (string.IsNullOrEmpty(textBoxShortcut.Text))
            {
                ThrowErrorMessage("Button shortcut can't be nothing.");
                return;
            }

            if (!Keyboard.ValidApplication(textBoxProcessName.Text))
            {
                ThrowErrorMessage("Application not found.");
                return;
            }
            AddButton(textBoxButtonName.Text, comboBoxExtendedKeys.SelectedIndex, comboBoxExtended2Keys.SelectedIndex,textBoxShortcut.Text, textBoxProcessName.Text);
        }

        private void themeSelection_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Theme");
        }

        private void loadButton_Click(object sender, RoutedEventArgs e)
        {
            LoadLayoutFile();
        }

        private void LoadLayoutFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Open Layout";
            openFileDialog.Filter = "XML |*.xml";
            openFileDialog.ShowDialog();
            if (openFileDialog.FileName != "")
            {
                foreach (ButtonInformation prefab in buttonPrefabs.Keys)
                {
                    prefab.ButtonPrefab.Close();
                }
                List<SaveableButton> saveableButtonList = fileLayoutManager.DeserializeXML((FileStream) openFileDialog.OpenFile());
                /*MessageBox.Show("Read " + saveableButton.Count + " buttons");*/
                foreach (SaveableButton button in saveableButtonList)
                {
                    ButtonPrefab newButtonPrefab = AddButton(button.buttonName, button.extendedKey1, button.extendedKey2, button.shortcut, button.procName);
                    Rect rect = new Rect(button.windowsPos[0], button.windowsPos[1], button.windowsRect[2], button.windowsRect[0]);
                    newButtonPrefab.SetPosAndScale(rect);
                }
            }
        }

        private void SaveLayoutToFile()
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Title = "Save Layout";
            fileDialog.AddExtension = true;
            fileDialog.Filter = "XML |*.xml";
            fileDialog.ShowDialog();
            if (!string.IsNullOrEmpty(fileDialog.FileName))
            {
                fileLayoutManager.SerializeXml(GetAllSaveableButtons(), (FileStream)fileDialog.OpenFile());
                MessageBox.Show("Saved layout " + fileDialog.FileName + "!","Shortcut Overlay",MessageBoxButton.OK,MessageBoxImage.Information);
            }
        }
        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveLayoutToFile();
        }

        private void form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            foreach (ButtonInformation prefab in buttonPrefabs.Keys)
            {
                prefab.ButtonPrefab.Close();
            }
        }
    }
}
