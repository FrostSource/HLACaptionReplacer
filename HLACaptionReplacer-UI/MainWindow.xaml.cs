using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace HLACaptionReplacer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string workingCCFileFormat = "CC_{0}.dat";
        public MainWindow()
        {
            SupportedLanguages = new ObservableCollection<string>(Steam.SteamData.GetSupportedLanguages());
            SelectedLanguage = "english";
            IsInitializingLanguage = false;
            Captions = new ObservableCollection<ClosedCaptionDependencyObject>();
            InitializeComponent();
        }
        Dictionary<uint, string> hashToName = new Dictionary<uint, string>();
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            AddOn = AddOnSelector.GetAddOn();
        }
        public static readonly DependencyProperty SupportedLanguagesProperty =
            DependencyProperty.Register(nameof(SupportedLanguages), typeof(ObservableCollection<string>),
            typeof(MainWindow));
        public ObservableCollection<string> SupportedLanguages
        {
            get
            {
                return (ObservableCollection<string>)GetValue(SupportedLanguagesProperty);
            }
            set
            {
                this.SetValue(SupportedLanguagesProperty, value);
            }
        }
        public static readonly DependencyProperty SelectedLanguageProperty =
            DependencyProperty.Register(nameof(SelectedLanguage), typeof(string),
            typeof(MainWindow), new PropertyMetadata(OnSelectedLanguageChanged));
        bool IsInitializingLanguage = true;
        private static void OnSelectedLanguageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MainWindow me = d as MainWindow;
            if (!me.IsInitializingLanguage)
            {
                if (me != null && e.OldValue != null && !string.IsNullOrEmpty(e.OldValue.ToString()))
                {
                    switch (MessageBox.Show("Save current captions?", "Captions", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                    {
                        case MessageBoxResult.Yes:
                            me.Save((string)e.OldValue, me.AddOn);
                            break;
                        case MessageBoxResult.No:
                            break;
                        case MessageBoxResult.Cancel:
                            me.IsInitializingLanguage = true;
                            me.SelectedLanguage = (string)e.OldValue;
                            me.IsInitializingLanguage = false;
                            return;
                    }
                }
                me.LoadCaptionData();
            }
        }

        public string SelectedLanguage
        {
            get
            {
                return (string)GetValue(SelectedLanguageProperty);
            }
            set
            {
                this.SetValue(SelectedLanguageProperty, value);
            }
        }

        public static readonly DependencyProperty SoundNamesProperty =
            DependencyProperty.Register(nameof(SoundNames), typeof(ObservableCollection<string>),
            typeof(MainWindow));
              public ObservableCollection<string> SoundNames
        {
            get
            {
                return (ObservableCollection<string>)GetValue(SoundNamesProperty);
            }
            set
            {
                this.SetValue(SoundNamesProperty, value);
            }
        }

        public static readonly DependencyProperty CaptionsProperty =
          DependencyProperty.Register(nameof(Captions), typeof(ObservableCollection<ClosedCaptionDependencyObject>),
          typeof(MainWindow));
        public ObservableCollection<ClosedCaptionDependencyObject> Captions
        {
            get
            {
                return (ObservableCollection<ClosedCaptionDependencyObject>)GetValue(CaptionsProperty);
            }
            set
            {
                this.SetValue(CaptionsProperty, value);
            }
        }
        void LoadCaptionData()
        {
            string addonFolder = Steam.SteamData.GetHLAAddOnFolder(AddOn);
            SoundNames = new ObservableCollection<string>();
            hashToName = new Dictionary<uint, string>();
            foreach (var eventFiles in new DirectoryInfo(addonFolder).GetFiles("*." + Steam.SteamData.SoundEventsExtension, SearchOption.AllDirectories))
            {
                foreach (var soundName in AAT.AddonHelper.DeserializeFile(eventFiles.FullName))
                {
                    SoundNames.Add(soundName.EventName);
                    hashToName.Add(ValveResourceFormat.Crc32.Compute(Encoding.UTF8.GetBytes(soundName.EventName)), soundName.EventName);
                }
            }

            //Look for and load any caption file.
            string targetPath = System.IO.Path.Combine(Steam.SteamData.GetHLAInstallFolder(), Steam.SteamData.HLAWIPAddonGamePath, AddOn, Steam.SteamData.CaptionFolder);
            if (Directory.Exists(targetPath))
            {
                foreach (var captionFiles in new DirectoryInfo(targetPath).GetFiles(string.Format(workingCCFileFormat, SelectedLanguage + "_custom"), SearchOption.AllDirectories))
                {
                    var closedCaptions = new ClosedCaptions();
                    using (var stream = captionFiles.OpenRead())
                    {
                        closedCaptions.Read(stream);
                    }
                    foreach (var caption in closedCaptions.Captions)
                    {
                        var cap = new ClosedCaptionDependencyObject(caption);
                        if (hashToName.ContainsKey(caption.SoundEventHash))
                        {
                            cap.Name = hashToName[caption.SoundEventHash];
                        }
                        Captions.Add(cap);
                    }
                }
            }

        }
        public static readonly DependencyProperty AddOnProperty =
            DependencyProperty.Register(nameof(AddOn), typeof(string),
            typeof(MainWindow), new PropertyMetadata(OnAddOnChanged));
        
        private static void OnAddOnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MainWindow me = d as MainWindow;
            if (me != null)
            {
                //Get list of soundevents to create captions from.

                
                me.LoadCaptionData();
            }
        }
        
        public string AddOn
        {
            get
            {
                return (string)GetValue(AddOnProperty);
            }
            set
            {
                this.SetValue(AddOnProperty, value);
            }
        }

        private void OnNewAddOn(object sender, RoutedEventArgs e)
        {
            if (Captions.Count > 0)
            {
                switch (MessageBox.Show("Save captions?", "Save", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                {
                    case MessageBoxResult.Yes:
                        Save(SelectedLanguage, AddOn);
                        break;
                    case MessageBoxResult.No:
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                }
            }
            AddOn = AddOnSelector.GetAddOn();
        }

        private void OnDeleteCaption(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                ClosedCaptionDependencyObject obj = btn.CommandParameter as ClosedCaptionDependencyObject;
                if (obj !=null)
                {
                    Captions.Remove(obj);
                }
            }
        }
        void Save(string language, string addOn)
        {
            string captionFile =
                System.IO.Path.Combine(Steam.SteamData.GetHLAInstallFolder(), Steam.SteamData.HLAWIPAddonGamePath,
                addOn, Steam.SteamData.CaptionFolder, string.Format(Steam.SteamData.CaptionFormat, language + "_custom"));

            ClosedCaptions captionsList = new ClosedCaptions();
            ClosedCaptions workingCCList = new ClosedCaptions();
            //First load in all the game captions for the selected language.


            string gameCaptionFile = System.IO.Path.Combine(Steam.SteamData.GetHLAInstallFolder(), @"game\hlvr",
                Steam.SteamData.CaptionFolder, string.Format(Steam.SteamData.CaptionFormat, language));
            using (var stream = new FileStream(gameCaptionFile, FileMode.Open, FileAccess.Read))
            {
                captionsList.Read(stream);
            }

            foreach (var caption in Captions)
            {
                workingCCList.Add(caption.Caption);
                captionsList.Add(caption.Caption);
            }
            captionsList.Write(captionFile);
            string workingCaptionFile = System.IO.Path.Combine(Steam.SteamData.GetHLAInstallFolder(), Steam.SteamData.HLAWIPAddonGamePath,
                addOn, Steam.SteamData.CaptionFolder, string.Format(workingCCFileFormat, language + "_custom"));
            workingCCList.Write(workingCaptionFile);

            MessageBox.Show("Captions saved as: " + captionFile + "\r\nWorking caption file: " + workingCaptionFile);
        }
        private void OnSave(object sender, RoutedEventArgs e)
        {
            Save(SelectedLanguage, AddOn);
        }

        private void OnAdd(object sender, RoutedEventArgs e)
        {
            Captions.Add(new ClosedCaptionDependencyObject());
        }
    }
}
