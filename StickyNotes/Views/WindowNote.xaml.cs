using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace StickyNotes.Views
{
    public partial class WindowNote : Window
    {
        private const int ResizeBorderThickness = 6;
        private bool _isResizing = false;
        private WindowResizeEdge _resizeEdge;
        private Point _resizeStartPoint;
        private bool _isDragging = false;
        private Point _dragStartPoint;

        public WindowNote()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        } // WindowNote

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            NoteTextBox.Focus();
        } // OnLoaded

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement element && IsInTitleBar(element))
            {
                _isDragging = true;
                _dragStartPoint = e.GetPosition(this);
                CaptureMouse();
                e.Handled = true;
            }
        } // TitleBar_MouseLeftButtonDown

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (_isDragging)
            {
                _isDragging = false;
                ReleaseMouseCapture();
                SaveWindowPosition();
            }

            if (_isResizing)
            {
                _isResizing = false;
                ReleaseMouseCapture();
                SaveWindowPosition();
            }
        } // OnMouseLeftButtonUp

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition(this);
                Vector offset = currentPosition - _dragStartPoint;

                Left += offset.X;
                Top += offset.Y;
            }
            else if (_isResizing && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition(this);
                HandleResize(currentPosition);
            }
            else
            {
                UpdateCursor(e.GetPosition(this));
            }
        } // OnMouseMove

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            SaveWindowPosition();
        } // OnLocationChanged

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            SaveWindowPosition();
        } // OnRenderSizeChanged

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            var point = e.GetPosition(this);

            // Проверяем, находится ли курсор в зоне ресайза
            if (IsInResizeZone(point))
            {
                _isResizing = true;
                _resizeStartPoint = point;
                _resizeEdge = GetResizeEdge(point);
                CaptureMouse();
                e.Handled = true;
            }
        } // OnMouseLeftButtonDown

        private bool IsInResizeZone(Point point)
        {
            return point.X <= ResizeBorderThickness ||
                   point.X >= ActualWidth - ResizeBorderThickness ||
                   point.Y <= ResizeBorderThickness ||
                   point.Y >= ActualHeight - ResizeBorderThickness;
        } // IsInResizeZone

        private WindowResizeEdge GetResizeEdge(Point point)
        {
            if (point.X <= ResizeBorderThickness)
            {
                if (point.Y <= ResizeBorderThickness) return WindowResizeEdge.TopLeft;
                if (point.Y >= ActualHeight - ResizeBorderThickness) return WindowResizeEdge.BottomLeft;
                return WindowResizeEdge.Left;
            }

            if (point.X >= ActualWidth - ResizeBorderThickness)
            {
                if (point.Y <= ResizeBorderThickness) return WindowResizeEdge.TopRight;
                if (point.Y >= ActualHeight - ResizeBorderThickness) return WindowResizeEdge.BottomRight;
                return WindowResizeEdge.Right;
            }

            if (point.Y <= ResizeBorderThickness) return WindowResizeEdge.Top;
            return WindowResizeEdge.Bottom;
        } // GetResizeEdge

        private void HandleResize(Point currentPosition)
        {
            double deltaX = currentPosition.X - _resizeStartPoint.X;
            double deltaY = currentPosition.Y - _resizeStartPoint.Y;

            switch (_resizeEdge)
            {
                case WindowResizeEdge.Left:
                    Left += deltaX;
                    Width = Math.Max(MinWidth, Width - deltaX);
                    break;
                case WindowResizeEdge.Right:
                    Width = Math.Max(MinWidth, Width + deltaX);
                    break;
                case WindowResizeEdge.Top:
                    Top += deltaY;
                    Height = Math.Max(MinHeight, Height - deltaY);
                    break;
                case WindowResizeEdge.Bottom:
                    Height = Math.Max(MinHeight, Height + deltaY);
                    break;
                case WindowResizeEdge.TopLeft:
                    Left += deltaX;
                    Top += deltaY;
                    Width = Math.Max(MinWidth, Width - deltaX);
                    Height = Math.Max(MinHeight, Height - deltaY);
                    break;
                case WindowResizeEdge.TopRight:
                    Top += deltaY;
                    Width = Math.Max(MinWidth, Width + deltaX);
                    Height = Math.Max(MinHeight, Height - deltaY);
                    break;
                case WindowResizeEdge.BottomLeft:
                    Left += deltaX;
                    Width = Math.Max(MinWidth, Width - deltaX);
                    Height = Math.Max(MinHeight, Height + deltaY);
                    break;
                case WindowResizeEdge.BottomRight:
                    Width = Math.Max(MinWidth, Width + deltaX);
                    Height = Math.Max(MinHeight, Height + deltaY);
                    break;
            }

            _resizeStartPoint = currentPosition;
        } // HandleResize

        private void UpdateCursor(Point point)
        {
            if (IsInResizeZone(point))
            {
                var edge = GetResizeEdge(point);
                Cursor = GetCursorForEdge(edge);
            }
            else
            {
                Cursor = Cursors.Arrow;
            }
        } // UpdateCursor

        private static Cursor GetCursorForEdge(WindowResizeEdge edge)
        {
            return edge switch
            {
                WindowResizeEdge.Left or WindowResizeEdge.Right => Cursors.SizeWE,
                WindowResizeEdge.Top or WindowResizeEdge.Bottom => Cursors.SizeNS,
                WindowResizeEdge.TopLeft or WindowResizeEdge.BottomRight => Cursors.SizeNWSE,
                WindowResizeEdge.TopRight or WindowResizeEdge.BottomLeft => Cursors.SizeNESW,
                _ => Cursors.Arrow
            };
        } // GetCursorForEdge

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

        private void SaveWindowPosition()
        {
            var viewModel = DataContext as ViewModels.WindowNoteViewModel;
            viewModel?.SaveWindowPosition();
        } // SaveWindowPosition

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is not ViewModels.WindowNoteViewModel viewModel) return;

            // Ctrl+C или Ctrl+Insert - копировать весь текст
            if ((e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control) ||
                (e.Key == Key.Insert && Keyboard.Modifiers == ModifierKeys.Control))
            {
                viewModel.CopyToClipboardCommand.Execute(null);
                e.Handled = true;
            }
            // Ctrl+S или F2 - сохранить
            else if ((e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control) ||
                     e.Key == Key.F2)
            {
                viewModel.SaveCommand.Execute(null);
                e.Handled = true;
            }
        } // Window_KeyDown

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
    } // WindowNote
} // namespace