using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.ComponentModel;

namespace BlazorWinbox;

public partial class WindowInstance
{
    [Inject] public IWindowManager WindowManager { get; set; }
    [Parameter] public Guid Id { get; set; }
    [Parameter] public WindowOptions Options { get; set; }
    //[Parameter] public string Title { get; set; }
    [Parameter] public RenderFragment Content { get; set; }
    [CascadingParameter] public GlobalWindowOptions GlobalWindowOptions { get; set; }

    WindowOptions _Options;
    protected override void OnInitialized()
    {
        ConfigureInstance();
    }

    private void ConfigureInstance()
    {
        Options ??= new WindowOptions();

        SetClass();
        SetIndex();
        SetRoot();
        SetBackground();
        SetBorder();
        SetModal();
        SetWidth();
        SetHeight();
        SetMinWidth();
        SetMinHeight();
        SetMaxWidth();
        SetMaxHeight();
        SetAutoSize();
        SetX();
        SetY();
        SetTop();
        SetBottom();
        SetRight();
        SetLeft();
        Options.Id = Id.ToString("N");
        Options.XCloseHandlerName = nameof(OnXClose);
        Options.BlazorWindowInstanceReference = DotNetObjectReference.Create(this);
        Options.OnCloseHandlerName = nameof(CloseAsync);

        Options.OnMoveHandlerName = nameof(OnMove);
        Options.OnShowHandlerName = nameof(OnShow);
        Options.OnHideHandlerName = nameof(OnHide);
        Options.OnFocusHandlerName = nameof(OnFocus);
        Options.OnBlurHandlerName = nameof(OnBlur);
        Options.OnResizeHandlerName = nameof(OnResize);
        Options.OnFullScreenHandlerName = nameof(OnFullscreen);
        Options.OnMaximizeHandlerName = nameof(OnMaximize);
        Options.OnMinimizeHandlerName = nameof(OnMinimize);
        Options.OnRestoreHandlerName = nameof(OnRestore);

        _Options = Options;
    }


    #region Configure instance methods

    private void SetClass()
    {
        Options.Class = (GlobalWindowOptions.Class ?? "") + (Options.Class ?? "");
    }

    private void SetIndex()
    {
        if (Options.Index.HasValue)
        {
            return;
        }
        else if (GlobalWindowOptions.Index.HasValue)
        {
            Options.Index = GlobalWindowOptions.Index.Value;
        }
    }

    private void SetRoot()
    {
        if (Options.Root.HasValue)
        {
            return;
        }
        else if (GlobalWindowOptions.Root.HasValue)
        {
            Options.Root = GlobalWindowOptions.Root.Value;
        }
    }

    private void SetBackground()
    {
        if (!string.IsNullOrEmpty(Options.Background))
        {
            return;
        }
        else if (!string.IsNullOrEmpty(GlobalWindowOptions.Background))
        {
            Options.Background = GlobalWindowOptions.Background;
        }
    }

    private void SetBorder()
    {
        if (!string.IsNullOrEmpty(Options.Border))
        {
            return;
        }
        else if (!string.IsNullOrEmpty(GlobalWindowOptions.Border))
        {
            Options.Border = GlobalWindowOptions.Border;
        }
    }

    private void SetModal()
    {
        if (Options.Modal.HasValue)
        {
            return;
        }
        else if (GlobalWindowOptions.Modal.HasValue)
        {
            Options.Modal = GlobalWindowOptions.Modal.Value;
        }
        else
        {
            Options.Modal = false;
        }
    }

    private void SetWidth()
    {
        if (!string.IsNullOrEmpty(Options.Width))
        {
            return;
        }
        else if (!string.IsNullOrEmpty(GlobalWindowOptions.Width))
        {
            Options.Width = GlobalWindowOptions.Width;
        }
    }

    private void SetHeight()
    {
        if (!string.IsNullOrEmpty(Options.Height))
        {
            return;
        }
        else if (!string.IsNullOrEmpty(GlobalWindowOptions.Height))
        {
            Options.Height = GlobalWindowOptions.Height;
        }
    }

    private void SetMinWidth()
    {
        if (!string.IsNullOrEmpty(Options.MinWidth))
        {
            return;
        }
        else if (!string.IsNullOrEmpty(GlobalWindowOptions.MinWidth))
        {
            Options.MinWidth = GlobalWindowOptions.MinWidth;
        }
    }

    private void SetMinHeight()
    {
        if (!string.IsNullOrEmpty(Options.MinHeight))
        {
            return;
        }
        else if (!string.IsNullOrEmpty(GlobalWindowOptions.MinHeight))
        {
            Options.MinHeight = GlobalWindowOptions.MinHeight;
        }
    }

    private void SetMaxWidth()
    {
        if (!string.IsNullOrEmpty(Options.MaxWidth))
        {
            return;
        }
        else if (!string.IsNullOrEmpty(GlobalWindowOptions.MaxWidth))
        {
            Options.MaxWidth = GlobalWindowOptions.MaxWidth;
        }
    }

    private void SetMaxHeight()
    {
        if (!string.IsNullOrEmpty(Options.MaxHeight))
        {
            return;
        }
        else if (!string.IsNullOrEmpty(GlobalWindowOptions.MaxHeight))
        {
            Options.MaxHeight = GlobalWindowOptions.MaxHeight;
        }
    }

    private void SetAutoSize()
    {
        if (Options.AutoSize.HasValue)
        {
            return;
        }
        else if (GlobalWindowOptions.AutoSize.HasValue)
        {
            Options.AutoSize = GlobalWindowOptions.AutoSize.Value;
        }
    }

    private void SetX()
    {
        if (!string.IsNullOrEmpty(Options.X))
        {
            return;
        }
        else if (!string.IsNullOrEmpty(GlobalWindowOptions.X))
        {
            Options.X = GlobalWindowOptions.X;
        }
        else
        {
            Options.X = "center";
        }
    }

    private void SetY()
    {
        if (!string.IsNullOrEmpty(Options.Y))
        {
            return;
        }
        else if (!string.IsNullOrEmpty(GlobalWindowOptions.Y))
        {
            Options.Y = GlobalWindowOptions.Y;
        }
        else
        {
            Options.Y = "center";
        }
    }

    private void SetTop()
    {
        if (!string.IsNullOrEmpty(Options.Top))
        {
            return;
        }
        else if (!string.IsNullOrEmpty(GlobalWindowOptions.Top))
        {
            Options.Top = GlobalWindowOptions.Top;
        }
    }

    private void SetBottom()
    {
        if (!string.IsNullOrEmpty(Options.Bottom))
        {
            return;
        }
        else if (!string.IsNullOrEmpty(GlobalWindowOptions.Bottom))
        {
            Options.Bottom = GlobalWindowOptions.Bottom;
        }
    }

    private void SetLeft()
    {
        if (!string.IsNullOrEmpty(Options.Left))
        {
            return;
        }
        else if (!string.IsNullOrEmpty(GlobalWindowOptions.Left))
        {
            Options.Left = GlobalWindowOptions.Left;
        }
    }

    private void SetRight()
    {
        if (!string.IsNullOrEmpty(Options.Right))
        {
            return;
        }
        else if (!string.IsNullOrEmpty(GlobalWindowOptions.Right))
        {
            Options.Right = GlobalWindowOptions.Right;
        }
    }

    #endregion

    [Obsolete("Do not use this in your code, it is allowed to be used internally by Window")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [JSInvokable]
    public void OnXClose()
    {
        WindowManager.CloseBlzr(Id, WindowResult.Cancel());
    }

    public async Task CloseAsync(WindowResult result)
    {
        var xCompleteClose = _Options.OnClose?.Invoke();
        bool? xCompleteCloseAsync = null;
        if (_Options.OnCloseAsync != null)
        {
            xCompleteCloseAsync = await _Options.OnCloseAsync?.Invoke();
        }
        if (xCompleteClose is not false && xCompleteCloseAsync is not false)
        {
            ForceClose(result);
        }
    }

    [JSInvokable]
    public async Task CloseAsync()
    {
        await CloseAsync(WindowResult.Cancel());
    }

    public void ForceClose()
    {
        ForceClose(WindowResult.Cancel());
    }

    public void ForceClose(WindowResult result)
    {
        WindowManager.Close(Id, result);
    }

    /// <summary>
    /// Set the fullscreen state of a window
    /// </summary>
    /// <param name="state"></param>
    public async Task Fullscreen(bool? state = null)
    {
        WindowManager.Fullscreen(Id, state);
        if (_Options.OnFullScreen is null) return;
        await _Options.OnFullScreen.Invoke(state);
    }
    [JSInvokable]
    public async Task OnFullscreen(bool? state = null)
    {
        if (_Options.OnFullScreen is null) return;
        await _Options.OnFullScreen.Invoke(state);
    }

    /// <summary>
    /// Hide a specific window
    /// </summary>
    /// <param name="state"></param>
    public async Task Hide(bool? state = null)
    {
        WindowManager.Hide(Id, state);
        if (_Options.OnHide is null) return;
        await _Options.OnHide.Invoke(state);
    }
    [JSInvokable]
    public async Task OnHide(bool? state = null)
    {
        if (_Options.OnHide is null) return;
        await _Options.OnHide.Invoke(state);
    }

    /// <summary>
    /// Set the maximized state of a window
    /// </summary>
    /// <param name="state"></param>
    public async Task Maximize(bool? state = null)
    {
        WindowManager.Maximize(Id, state);
        if (_Options.OnMaximize is null) return;
        await _Options.OnMaximize.Invoke(state);
    }
    [JSInvokable]
    public async Task OnMaximize(bool? state = null)
    {
        if (_Options.OnMaximize is null) return;
        await _Options.OnMaximize.Invoke(state);
    }

    /// <summary>
    /// Set the minimized state of a window
    /// </summary>
    /// <param name="state"></param>
    public async Task Minimize(bool? state = null)
    {
        WindowManager.Minimize(Id, state);
        if (_Options.OnMinimize is null) return;
        await _Options.OnMinimize.Invoke(state);
    }
    [JSInvokable]
    public async Task OnMinimize(bool? state = null)
    {
        if (_Options.OnMinimize is null) return;
        await _Options.OnMinimize.Invoke(state);
    }

    /// <summary>
    /// Supports keywords "right" for x-axis, "bottom" for y-axis, "center" for both, units px and % also for both.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public async Task Move(string x, string y)
    {
        WindowManager.Move(Id, x, y);
        if (_Options.OnMove is null) return;
        await _Options.OnMove.Invoke(x, y);
    }

    [JSInvokable]
    public async Task OnMove(long x, long y)
    {
        if (_Options.OnMove is null) return;
        await _Options.OnMove.Invoke(x.ToString(), y.ToString());
    }

    /// <summary>
    /// You can remove all control classes from above along the window's lifetime
    /// </summary>
    /// <param name="cssClass"></param>
    public void RemoveClass(string cssClass)
    {
        WindowManager.RemoveClass(Id, cssClass);
    }

    /// <summary>
    /// You can add all control classes from above along the window's lifetime
    /// </summary>
    /// <param name="cssClass"></param>
    public void AddClass(string cssClass)
    {
        WindowManager.AddClass(Id, cssClass);
    }

    /// <summary>
    /// Supports units px and % also for both.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public async Task Resize(string width, string height)
    {
        WindowManager.Resize(Id, width, height);
        if (_Options.OnResize is null) return;
        await _Options.OnResize.Invoke(width, height);
    }
    [JSInvokable]
    public async Task OnResize(long w, long h)
    {
        if (_Options.OnResize is null) return;
        await _Options.OnResize.Invoke(w.ToString(), h.ToString());
    }

    /// <summary>
    /// Restore the state of a window
    /// </summary>
    public async Task Restore()
    {
        WindowManager.Restore(Id);
        if (_Options.OnRestore is null) return;
        await _Options.OnRestore.Invoke();
    }
    [JSInvokable]
    public async Task OnRestore()
    {
        if (_Options.OnRestore is null) return;
        await _Options.OnRestore.Invoke();
    }

    public void SetBackground(string backgroundColor)
    {
        WindowManager.SetBackground(Id, backgroundColor);
    }

    public void SetTitle(string title)
    {
        WindowManager.SetTitle(Id, title);
    }

    /// <summary>
    /// Show a specific hidden window
    /// </summary>
    /// <param name="state"></param>
    public async Task Show(bool? state = null)
    {
        WindowManager.Show(Id, state);
        if (_Options.OnShow is null) return;
        await _Options.OnShow.Invoke(state);
    }
    [JSInvokable]
    public async Task OnShow(bool? state = null)
    {
        if (_Options.OnShow is null) return;
        await _Options.OnShow.Invoke(state);
    }

    /// <summary>
    /// You can toggle all control classes from above along the window's lifetime
    /// </summary>
    /// <param name="cssClass"></param>
    public void ToggleClass(string cssClass)
    {
        WindowManager.ToggleClass(Id, cssClass);
    }

    /// <summary>
    /// Focus a window (bring up to front)
    /// </summary>
    /// <param name="state"></param>
    public async Task Focus(bool? state = null)
    {
        WindowManager.Focus(Id);
        if (_Options.OnFocus is null) return;
        await _Options.OnFocus.Invoke(state);
    }
    [JSInvokable]
    public async Task OnFocus(bool? state = null)
    {
        if (_Options.OnFocus is null) return;
        await _Options.OnFocus.Invoke(state);
    }

    /// <summary>
    /// Blur a focused window
    /// </summary>
    /// <param name="state"></param>
    public async Task Blur(bool? state = null)
    {
        WindowManager.Blur(Id, state);
        if (_Options.OnBlur is null) return;
        await _Options.OnBlur.Invoke(state);
    }
    [JSInvokable]
    public async Task OnBlur(bool? state = null)
    {
        if (_Options.OnBlur is null) return;
        await _Options.OnBlur.Invoke(state);
    }

    /// <summary>
    /// Mount an HTML element to the window body.
    /// </summary>
    /// <param name="windowContent">HTML element (widget, template, etc.)</param>
    public void Mount(ElementReference windowContent)
    {
        WindowManager.Mount(Id, windowContent);
    }
}
