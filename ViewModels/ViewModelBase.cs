using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WallpaperClient.ViewModels
{
    /// <summary>
    /// ViewModel基类，实现INotifyPropertyChanged接口
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// 属性更改事件
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 触发属性更改事件
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 设置属性值并触发更改事件
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="field">字段引用</param>
        /// <param name="value">新值</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns>是否发生更改</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// 设置属性值并触发更改事件（带回调）
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="field">字段引用</param>
        /// <param name="value">新值</param>
        /// <param name="onChanged">更改回调</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns>是否发生更改</returns>
        protected bool SetProperty<T>(ref T field, T value, Action onChanged, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// 设置属性值并触发多个更改事件
        /// </summary>
        /// <typeparam name="T">属性类型</typeparam>
        /// <param name="field">字段引用</param>
        /// <param name="value">新值</param>
        /// <param name="propertyNames">要触发的属性名称数组</param>
        /// <returns>是否发生更改</returns>
        protected bool SetProperty<T>(ref T field, T value, params string[] propertyNames)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            foreach (var propertyName in propertyNames)
            {
                OnPropertyChanged(propertyName);
            }
            return true;
        }
    }

    /// <summary>
    /// 异步命令实现
    /// </summary>
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool>? _canExecute;
        private bool _isExecuting;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="execute">执行函数</param>
        /// <param name="canExecute">是否可执行函数</param>
        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 是否正在执行
        /// </summary>
        public bool IsExecuting
        {
            get => _isExecuting;
            private set
            {
                if (_isExecuting != value)
                {
                    _isExecuting = value;
                    RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// 是否可执行更改事件
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// 是否可执行
        /// </summary>
        /// <param name="parameter">参数</param>
        /// <returns>是否可执行</returns>
        public bool CanExecute(object? parameter)
        {
            return !IsExecuting && (_canExecute?.Invoke() ?? true);
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="parameter">参数</param>
        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
            {
                return;
            }

            try
            {
                IsExecuting = true;
                await _execute();
            }
            finally
            {
                IsExecuting = false;
            }
        }

        /// <summary>
        /// 触发是否可执行更改事件
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// 异步命令实现（带参数）
    /// </summary>
    /// <typeparam name="T">参数类型</typeparam>
    public class AsyncRelayCommand<T> : ICommand
    {
        private readonly Func<T?, Task> _execute;
        private readonly Func<T?, bool>? _canExecute;
        private bool _isExecuting;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="execute">执行函数</param>
        /// <param name="canExecute">是否可执行函数</param>
        public AsyncRelayCommand(Func<T?, Task> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 是否正在执行
        /// </summary>
        public bool IsExecuting
        {
            get => _isExecuting;
            private set
            {
                if (_isExecuting != value)
                {
                    _isExecuting = value;
                    RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// 是否可执行更改事件
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// 是否可执行
        /// </summary>
        /// <param name="parameter">参数</param>
        /// <returns>是否可执行</returns>
        public bool CanExecute(object? parameter)
        {
            return !IsExecuting && (_canExecute?.Invoke((T?)parameter) ?? true);
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="parameter">参数</param>
        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
            {
                return;
            }

            try
            {
                IsExecuting = true;
                await _execute((T?)parameter);
            }
            finally
            {
                IsExecuting = false;
            }
        }

        /// <summary>
        /// 触发是否可执行更改事件
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// 同步命令实现
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="execute">执行函数</param>
        /// <param name="canExecute">是否可执行函数</param>
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 是否可执行更改事件
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// 是否可执行
        /// </summary>
        /// <param name="parameter">参数</param>
        /// <returns>是否可执行</returns>
        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="parameter">参数</param>
        public void Execute(object? parameter)
        {
            _execute();
        }

        /// <summary>
        /// 触发是否可执行更改事件
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// 同步命令实现（带参数）
    /// </summary>
    /// <typeparam name="T">参数类型</typeparam>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="execute">执行函数</param>
        /// <param name="canExecute">是否可执行函数</param>
        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 是否可执行更改事件
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// 是否可执行
        /// </summary>
        /// <param name="parameter">参数</param>
        /// <returns>是否可执行</returns>
        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke((T?)parameter) ?? true;
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="parameter">参数</param>
        public void Execute(object? parameter)
        {
            _execute((T?)parameter);
        }

        /// <summary>
        /// 触发是否可执行更改事件
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
