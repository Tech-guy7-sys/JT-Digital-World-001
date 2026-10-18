using System.Globalization;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Controls;

namespace AgentJeff;

public partial class MainWindow : Window
{
    private readonly SpeechSynthesizer _synth = new();
    private SpeechRecognitionEngine? _recognizer;
    private bool _listening;
    private readonly List<InstalledVoice> _voices = new();
    private VoiceCompanionWindow? _companion;
    private SecurityCenterWindow? _securityCenter;

    public MainWindow()
    {
        InitializeComponent();
        LoadVoices();
        LoadRecognizer();
        Closed += (_, _) => { StopListening(); _synth.Dispose(); _recognizer?.Dispose(); };
    }

    private void LoadVoices()
    {
        try
        {
            foreach (var voice in _synth.GetInstalledVoices().Where(v => v.Enabled))
            {
                _voices.Add(voice);
                VoicePicker.Items.Add($"{voice.VoiceInfo.Name} · {voice.VoiceInfo.Culture.Name}");
            }
            if (VoicePicker.Items.Count > 0) VoicePicker.SelectedIndex = 0;
            else SetStatus("NO WINDOWS TTS VOICE FOUND");
        }
        catch (Exception ex) { SetStatus("VOICE ERROR"); TranscriptBox.Text = ex.Message; }
    }

    private void LoadRecognizer()
    {
        try
        {
            var installed = SpeechRecognitionEngine.InstalledRecognizers();
            var recognizerInfo = installed.FirstOrDefault(r => r.Culture.Name.Equals("en-NG", StringComparison.OrdinalIgnoreCase))
                ?? installed.FirstOrDefault(r => r.Culture.Name.Equals("en-US", StringComparison.OrdinalIgnoreCase))
                ?? installed.FirstOrDefault(r => r.Culture.TwoLetterISOLanguageName == "en");
            if (recognizerInfo is null)
            {
                ListenButton.IsEnabled = false;
                SetStatus("WINDOWS SPEECH RECOGNIZER NOT INSTALLED");
                TranscriptBox.Text = "Windows has no compatible installed speech recognizer. Install a supported Windows speech language in Settings, then reopen Agent Jeff.";
                return;
            }
            _recognizer = new SpeechRecognitionEngine(recognizerInfo);
            _recognizer.LoadGrammar(new DictationGrammar());
            _recognizer.SetInputToDefaultAudioDevice();
            _recognizer.SpeechRecognized += (_, e) =>
            {
                if (e.Result.Confidence < 0.45 || string.IsNullOrWhiteSpace(e.Result.Text)) return;
                Dispatcher.Invoke(() =>
                {
                    var existing = TranscriptBox.Text.Trim();
                    if (existing.StartsWith("Your recognized speech")) existing = "";
                    TranscriptBox.Text = string.IsNullOrEmpty(existing) ? e.Result.Text : existing + " " + e.Result.Text;
                    TranscriptBox.ScrollToEnd();
                });
            };
            _recognizer.RecognizeCompleted += (_, _) => Dispatcher.Invoke(() =>
            {
                if (_listening) { _listening = false; ListenButton.Content = "Start listening"; SetStatus("READY · MICROPHONE OFF"); }
            });
        }
        catch (Exception ex)
        {
            ListenButton.IsEnabled = false;
            SetStatus("SPEECH INITIALIZATION FAILED");
            TranscriptBox.Text = $"Could not initialize Windows speech recognition: {ex.Message}";
        }
    }

    private void Listen_Click(object sender, RoutedEventArgs e)
    {
        if (_listening) { StopListening(); return; }
        if (_recognizer is null) { SetStatus("SPEECH RECOGNITION UNAVAILABLE"); return; }
        try
        {
            _recognizer.RecognizeAsync(RecognizeMode.Multiple);
            _listening = true;
            ListenButton.Content = "Listening… (click to stop)";
            SetStatus("LISTENING · MICROPHONE ACTIVE");
        }
        catch (Exception ex) { SetStatus("MICROPHONE ERROR"); MessageBox.Show(this, ex.Message, "Agent Jeff", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }

    private void Stop_Click(object sender, RoutedEventArgs e) => StopListening();
    private void StopListening()
    {
        if (!_listening) return;
        _listening = false;
        try { _recognizer?.RecognizeAsyncCancel(); } catch { }
        ListenButton.Content = "Start listening";
        SetStatus("READY · MICROPHONE OFF");
    }

    private void VoicePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var index = VoicePicker.SelectedIndex;
        if (index >= 0 && index < _voices.Count)
        {
            try { _synth.SelectVoice(_voices[index].VoiceInfo.Name); } catch { }
        }
    }

    private void PreviewVoice_Click(object sender, RoutedEventArgs e) => Speak("Hello. I'm Jeff, your desktop assistant from JT Digital World.");
    private void SpeakTranscript_Click(object sender, RoutedEventArgs e)
    {
        var text = TranscriptBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(text) || text.StartsWith("Your recognized speech")) return;
        Speak(text);
    }
    private void Speak(string text)
    {
        try { _synth.SpeakAsyncCancelAll(); _synth.SpeakAsync(text); SetStatus("SPEAKING"); }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Text-to-speech error", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }
    private void StopSpeaking_Click(object sender, RoutedEventArgs e) { _synth.SpeakAsyncCancelAll(); SetStatus(_listening ? "LISTENING · MICROPHONE ACTIVE" : "READY · MICROPHONE OFF"); }
    private void Clear_Click(object sender, RoutedEventArgs e) => TranscriptBox.Clear();

    private void ShowCompanion_Click(object sender, RoutedEventArgs e)
    {
        if (_companion is null || !_companion.IsVisible)
        {
            _companion = new VoiceCompanionWindow();
            _companion.Closed += (_, _) => _companion = null;
            _companion.Show();
        }
        else
        {
            _companion.Activate();
        }
    }
    private void OpenSecurity_Click(object sender, RoutedEventArgs e)
    {
        if (_securityCenter is null || !_securityCenter.IsVisible)
        {
            _securityCenter = new SecurityCenterWindow { Owner = this };
            _securityCenter.Closed += (_, _) => _securityCenter = null;
            _securityCenter.Show();
        }
        else _securityCenter.Activate();
    }

    private void SetStatus(string status) => StatusText.Text = status.ToUpperInvariant();
}