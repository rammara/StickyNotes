using System.Text.Json.Serialization;
using System.Windows.Input;

namespace StickyNotes.Models
{
    public class SettingsModel
    {
        public DoubleClickAction DoubleClickAction { get; set; } = DoubleClickAction.EditWindow;

        [JsonIgnore]
        public Hotkey MainHotkey { get; set; } = new Hotkey { Key = System.Windows.Forms.Keys.F4, Modifiers = HotkeyModifiers.Shift };

        public string MainHotkeyString
        {
            get => MainHotkey.ToString();
            set => MainHotkey = Hotkey.Parse(value);
        }

        [JsonIgnore]
        public Hotkey SaveHotkey { get; set; } = new Hotkey { Key = System.Windows.Forms.Keys.F2 };

        public string SaveHotkeyString
        {
            get => SaveHotkey.ToString();
            set => SaveHotkey = Hotkey.Parse(value);
        }

        public bool StartWithWindows { get; set; } = false;
        public string DefaultSaveFolder { get; set; } =
            System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Notes");
        public FontSettings DefaultFont { get; set; } = new FontSettings();
    } // SettingsModel

    public enum DoubleClickAction
    {
        EditWindow,
        SettingsWindow
    } // DoubleClickAction

    public class Hotkey
    {
        public System.Windows.Forms.Keys Key { get; set; }
        public HotkeyModifiers Modifiers { get; set; }

        public override string ToString()
        {
            var parts = new List<string>();
            if ((Modifiers & HotkeyModifiers.Control) != 0) parts.Add("Ctrl");
            if ((Modifiers & HotkeyModifiers.Shift) != 0) parts.Add("Shift");
            if ((Modifiers & HotkeyModifiers.Alt) != 0) parts.Add("Alt");
            if ((Modifiers & HotkeyModifiers.Win) != 0) parts.Add("Win");
            parts.Add(Key.ToString());
            return string.Join("+", parts);
        } // ToString

        public bool IsPressed()
        {
            try
            {
                bool ctrlPressed = (Modifiers & HotkeyModifiers.Control) != 0;
                bool shiftPressed = (Modifiers & HotkeyModifiers.Shift) != 0;
                bool altPressed = (Modifiers & HotkeyModifiers.Alt) != 0;
                bool winPressed = (Modifiers & HotkeyModifiers.Win) != 0;

                bool ctrlActual =  Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl) ||
                                   Keyboard.IsKeyDown(System.Windows.Input.Key.RightCtrl);
                bool shiftActual = Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift) ||
                                   Keyboard.IsKeyDown(System.Windows.Input.Key.RightShift);
                bool altActual =   Keyboard.IsKeyDown(System.Windows.Input.Key.LeftAlt) ||
                                   Keyboard.IsKeyDown(System.Windows.Input.Key.RightAlt);
                bool winActual =   Keyboard.IsKeyDown(System.Windows.Input.Key.LWin) ||
                                   Keyboard.IsKeyDown(System.Windows.Input.Key.RWin);
                
                Key targetKey = KeyInterop.KeyFromVirtualKey((int)Key);
                bool keyPressed =Keyboard.IsKeyDown(targetKey);

                bool ctrlMatch = (ctrlPressed && ctrlActual) || (!ctrlPressed && !ctrlActual);
                bool shiftMatch = (shiftPressed && shiftActual) || (!shiftPressed && !shiftActual);
                bool altMatch = (altPressed && altActual) || (!altPressed && !altActual);
                bool winMatch = (winPressed && winActual) || (!winPressed && !winActual);

                return ctrlMatch && shiftMatch && altMatch && winMatch && keyPressed;
            }
            catch
            {
                return false;
            }
        } // IsPressed

        
        public bool IsPressed(KeyEventArgs e)
        {
            if (e == null) return false;

            try
            {
                bool ctrlPressed = (Modifiers & HotkeyModifiers.Control) != 0;
                bool shiftPressed = (Modifiers & HotkeyModifiers.Shift) != 0;
                bool altPressed = (Modifiers & HotkeyModifiers.Alt) != 0;
                bool winPressed = (Modifiers & HotkeyModifiers.Win) != 0;

                bool ctrlActual = (e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
                bool shiftActual = (e.KeyboardDevice.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
                bool altActual = (e.KeyboardDevice.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt;

                // Separate check for WinKey
                bool winActual = Keyboard.IsKeyDown(System.Windows.Input.Key.LWin) ||
                                 Keyboard.IsKeyDown(System.Windows.Input.Key.RWin);

                
                Key targetKey = KeyInterop.KeyFromVirtualKey((int)Key);
                bool ctrlMatch = (ctrlPressed && ctrlActual) || (!ctrlPressed && !ctrlActual);
                bool shiftMatch = (shiftPressed && shiftActual) || (!shiftPressed && !shiftActual);
                bool altMatch = (altPressed && altActual) || (!altPressed && !altActual);
                bool winMatch = (winPressed && winActual) || (!winPressed && !winActual);
                bool keyMatch = e.Key == targetKey;

                return ctrlMatch && shiftMatch && altMatch && winMatch && keyMatch;
            }
            catch
            {
                return false;
            }
        } // IsPressed



        public static Hotkey Parse(string value)
        {
            var hotkey = new Hotkey();
            var parts = value.Split('+');

            foreach (var part in parts)
            {
                var trimmed = part.Trim();

                if (trimmed.Equals("Ctrl", StringComparison.OrdinalIgnoreCase))
                    hotkey.Modifiers |= HotkeyModifiers.Control;
                else if (trimmed.Equals("Shift", StringComparison.OrdinalIgnoreCase))
                    hotkey.Modifiers |= HotkeyModifiers.Shift;
                else if (trimmed.Equals("Alt", StringComparison.OrdinalIgnoreCase))
                    hotkey.Modifiers |= HotkeyModifiers.Alt;
                else if (trimmed.Equals("Win", StringComparison.OrdinalIgnoreCase))
                    hotkey.Modifiers |= HotkeyModifiers.Win;
                else if (Enum.TryParse<System.Windows.Forms.Keys>(trimmed, out var key))
                    hotkey.Key = key;
            }

            return hotkey;
        } // Parse
    } // Hotkey

    [Flags]
    public enum HotkeyModifiers
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Win = 8
    } // HotkeyModifiers

    public class FontSettings
    {
        public string FontFamily { get; set; } = "Consolas";
        public float Size { get; set; } = 12;
        public System.Drawing.FontStyle Style { get; set; } = System.Drawing.FontStyle.Regular;

        public System.Drawing.Font ToFont()
        {
            return new System.Drawing.Font(FontFamily, Size, Style);
        } // ToFont
    } // FontSettings
}