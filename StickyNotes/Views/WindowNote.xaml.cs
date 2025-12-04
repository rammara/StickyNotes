using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace StickyNotes.Views
{
    public partial class WindowNote : Window
    {
        public WindowNote()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        } // WindowNote

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            NoteTextBox.Focus();
        } // OnLoaded

        protected override void OnLocationChanged(System.EventArgs e)
        {
            base.OnLocationChanged(e);
            var viewModel = DataContext as ViewModels.WindowNoteViewModel;
            viewModel?.SaveWindowPosition();
        } // OnLocationChanged

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            var viewModel = DataContext as ViewModels.WindowNoteViewModel;
            viewModel?.SaveWindowPosition();
        } // OnRenderSizeChanged

        // Обновим метод Window_KeyDown в WindowNote.xaml.cs
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is not ViewModels.WindowNoteViewModel viewModel) return;

            var mainViewModel = Application.Current.MainWindow?.DataContext as ViewModels.MainViewModel;

            // Ctrl+C или Ctrl+Insert - копировать весь текст
            if ((e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control) ||
                (e.Key == Key.Insert && Keyboard.Modifiers == ModifierKeys.Control))
            {
                viewModel.CopyToClipboardCommand.Execute(null);
                e.Handled = true;
            }
            // Ctrl+S или F2 - сохранить
            else if ((e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control) ||
                     (mainViewModel != null && mainViewModel.IsSaveHotkeyPressed(e)))
            {
                viewModel.SaveCommand.Execute(null);
                e.Handled = true;
            }
        } // Window_KeyDown

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            var point = e.GetPosition(this);

            // Проверяем, находится ли курсор в зоне ресайза
            if (point.X <= ResizeBorderThickness || point.X >= ActualWidth - ResizeBorderThickness ||
                point.Y <= ResizeBorderThickness || point.Y >= ActualHeight - ResizeBorderThickness)
            {
                // Начинаем изменение размера
                if (point.X <= ResizeBorderThickness && point.Y <= ResizeBorderThickness)
                    DragResize(WindowResizeEdge.TopLeft);
                else if (point.X >= ActualWidth - ResizeBorderThickness && point.Y <= ResizeBorderThickness)
                    DragResize(WindowResizeEdge.TopRight);
                else if (point.X <= ResizeBorderThickness && point.Y >= ActualHeight - ResizeBorderThickness)
                    DragResize(WindowResizeEdge.BottomLeft);
                else if (point.X >= ActualWidth - ResizeBorderThickness && point.Y >= ActualHeight - ResizeBorderThickness)
                    DragResize(WindowResizeEdge.BottomRight);
                else if (point.X <= ResizeBorderThickness)
                    DragResize(WindowResizeEdge.Left);
                else if (point.X >= ActualWidth - ResizeBorderThickness)
                    DragResize(WindowResizeEdge.Right);
                else if (point.Y <= ResizeBorderThickness)
                    DragResize(WindowResizeEdge.Top);
                else if (point.Y >= ActualHeight - ResizeBorderThickness)
                    DragResize(WindowResizeEdge.Bottom);
            }
        } // OnMouseLeftButtonDown

        private static bool IsInTitleBar(FrameworkElement element)
        {
            // Проверяем, является ли элемент частью заголовка
            if (element.Name == "TitleBar")
                return true;

            // Проверяем родителей элемента
            var parent = VisualTreeHelper.GetParent(element) as FrameworkElement;
            while (parent != null)
            {
                if (parent.Name == "TitleBar")
                    return true;
                parent = VisualTreeHelper.GetParent(parent) as FrameworkElement;
            }

            return false;
        } // IsInTitleBar

        // В WindowNote.xaml.cs добавим методы для ресайза
        private const int ResizeBorderThickness = 6;

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // Определяем курсор для изменения размера
            var point = e.GetPosition(this);
            var cursor = Cursors.Arrow;

            if (point.X <= ResizeBorderThickness)
            {
                cursor = point.Y <= ResizeBorderThickness ? Cursors.SizeNWSE :
                         point.Y >= ActualHeight - ResizeBorderThickness ? Cursors.SizeNESW :
                         Cursors.SizeWE;
            }
            else if (point.X >= ActualWidth - ResizeBorderThickness)
            {
                cursor = point.Y <= ResizeBorderThickness ? Cursors.SizeNESW :
                         point.Y >= ActualHeight - ResizeBorderThickness ? Cursors.SizeNWSE :
                         Cursors.SizeWE;
            }
            else if (point.Y <= ResizeBorderThickness)
            {
                cursor = Cursors.SizeNS;
            }
            else if (point.Y >= ActualHeight - ResizeBorderThickness)
            {
                cursor = Cursors.SizeNS;
            }

            Cursor = cursor;
        } // OnMouseMove

        private enum WindowResizeEdge
        {
            Left,
            Right,
            Top,
            Bottom,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        } // WindowResizeEdge

        private void DragResize(WindowResizeEdge edge)
        {
            // Используем Win32 API для изменения размера
            var wpfPoint = Mouse.GetPosition(this);
            var screenPoint = PointToScreen(wpfPoint);

            switch (edge)
            {
                case WindowResizeEdge.Left:
                    ResizeWindow(screenPoint.X, 0, 0, 0);
                    break;
                case WindowResizeEdge.Right:
                    ResizeWindow(0, 0, screenPoint.X, 0);
                    break;
                case WindowResizeEdge.Top:
                    ResizeWindow(0, screenPoint.Y, 0, 0);
                    break;
                case WindowResizeEdge.Bottom:
                    ResizeWindow(0, 0, 0, screenPoint.Y);
                    break;
                case WindowResizeEdge.TopLeft:
                    ResizeWindow(screenPoint.X, screenPoint.Y, 0, 0);
                    break;
                case WindowResizeEdge.TopRight:
                    ResizeWindow(0, screenPoint.Y, screenPoint.X, 0);
                    break;
                case WindowResizeEdge.BottomLeft:
                    ResizeWindow(screenPoint.X, 0, 0, screenPoint.Y);
                    break;
                case WindowResizeEdge.BottomRight:
                    ResizeWindow(0, 0, screenPoint.X, screenPoint.Y);
                    break;
            }
        } // DragResize

        private void ResizeWindow(double left, double top, double right, double bottom)
        {
            // Сохраняем текущие значения
            var currentLeft = Left;
            var currentTop = Top;
            var currentWidth = Width;
            var currentHeight = Height;

            // Изменяем размеры
            if (left > 0)
            {
                var newWidth = currentWidth + (currentLeft - left);
                if (newWidth > MinWidth)
                {
                    Left = left;
                    Width = newWidth;
                }
            }

            if (top > 0)
            {
                var newHeight = currentHeight + (currentTop - top);
                if (newHeight > MinHeight)
                {
                    Top = top;
                    Height = newHeight;
                }
            }

            if (right > 0)
            {
                var newWidth = right - Left;
                if (newWidth > MinWidth)
                    Width = newWidth;
            }

            if (bottom > 0)
            {
                var newHeight = bottom - Top;
                if (newHeight > MinHeight)
                    Height = newHeight;
            }

            // Сохраняем позицию
            var viewModel = DataContext as ViewModels.WindowNoteViewModel;
            viewModel?.SaveWindowPosition();
        } // ResizeWindow

    } // WindowNote
} // namespace