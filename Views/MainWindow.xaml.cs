using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WallpaperClient.ViewModels;

namespace WallpaperClient.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetService(typeof(MainViewModel)) as MainViewModel;

            // 初始化加载动画
            InitializeLoadingAnimation();

            // 窗口加载完成后初始化ViewModel
            Loaded += MainWindow_Loaded;
        }

        /// <summary>
        /// 窗口加载完成事件
        /// </summary>
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                await viewModel.InitializeAsync();
            }
        }

        /// <summary>
        /// 初始化加载动画
        /// </summary>
        private void InitializeLoadingAnimation()
        {
            var rotateTransform = FindName("LoadingRotateTransform") as RotateTransform;
            if (rotateTransform != null)
            {
                var animation = new DoubleAnimation
                {
                    From = 0,
                    To = 360,
                    Duration = new Duration(TimeSpan.FromSeconds(1)),
                    RepeatBehavior = RepeatBehavior.Forever
                };
                rotateTransform.BeginAnimation(RotateTransform.AngleProperty, animation);
            }
        }

        /// <summary>
        /// 最小化按钮点击事件
        /// </summary>
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// 最大化按钮点击事件
        /// </summary>
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        /// <summary>
        /// 关闭按钮点击事件
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// 自定义标题栏拖拽移动
        /// </summary>
        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                MaximizeButton_Click(sender, new RoutedEventArgs());
                return;
            }

            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
