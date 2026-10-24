using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorWinbox;

public class WinBoxWindowManager : IWindowManager
{
    private readonly IJSRuntime _js;
    private readonly Dictionary<Guid, IJSInProcessObjectReference> _windowsJsReferences;
    private readonly Dictionary<Guid, IWindowReference> _windowsBlzrReferences;
    private readonly Dictionary<Guid, List<Action<IJSInProcessObjectReference>>> _pendingToolbarActions;


    public event Action<IWindowReference> OnWindowInstanceAdded;
    public event Action<IWindowReference, WindowResult> OnWindowCloseRequested;

    public WinBoxWindowManager(IJSRuntime js)
    {
        _js = js;
        _windowsJsReferences = new Dictionary<Guid, IJSInProcessObjectReference>();
        _windowsBlzrReferences = new Dictionary<Guid, IWindowReference>();
        _pendingToolbarActions = new Dictionary<Guid, List<Action<IJSInProcessObjectReference>>>();
    }

    public IWindowReference Open<TComponent>(string title = null, WindowParameters windowParameters = null, WindowOptions windowOptions = null) where TComponent : ComponentBase
    {
        return Open(typeof(TComponent), title, windowParameters, windowOptions);
    }
    public IWindowReference Open(Type type, string title = null, WindowParameters windowParameters = null, WindowOptions windowOptions = null)
    {
        IWindowReference xWindowReference = CreateReference();
        RenderFragment xWindowContent = new(builder =>
        {
            var xAttributesCount = 0;
            builder.OpenComponent(xAttributesCount++, type);
            if (windowParameters is { Count: > 0 })
            {
                foreach (KeyValuePair<string, object> iParameter in windowParameters)
                {
                    builder.AddAttribute(xAttributesCount++, iParameter.Key, iParameter.Value);
                }
            }
            builder.CloseComponent();
        });

        RenderFragment xWindowInstance = new(builder =>
        {
            builder.OpenComponent<WindowInstance>(0);
            builder.SetKey(xWindowReference.Id);
            builder.AddAttribute(1, "Options", windowOptions);
            builder.AddAttribute(2, "Content", xWindowContent);
            builder.AddAttribute(3, "Id", xWindowReference.Id);
            builder.CloseComponent();
        });
        xWindowReference.InjectRenderFragment(xWindowInstance);
        _windowsBlzrReferences.Add(xWindowReference.Id, xWindowReference);
        OnWindowInstanceAdded?.Invoke(xWindowReference);

        return xWindowReference;
    }

    public async Task<object> OpenReValAsync<TComponent>(string title = null, WindowParameters windowParameters = null, WindowOptions windowOptions = null) where TComponent : ComponentBase
    {
        IWindowReference xWindowReference = Open<TComponent>(title, windowParameters, windowOptions);
        WindowResult xResult = await xWindowReference.Result;

        if (xResult?.Cancelled is not false)
        {
            return null;
        }
        else
        {
            return xResult.Data;
        }
    }

    public virtual IWindowReference CreateReference()
    {
        return new WinBoxWindowReference(Guid.NewGuid());
    }

    public async Task InjectComponentIntoWindow(string title, WindowOptions options, ElementReference mount, DotNetObjectReference<WindowInstance> windowInstanceReference)
    {
        if (!string.IsNullOrEmpty(title))
        {
            options.Title = title;
            title = null;
        }
        BasicWindowOptions xJsWindowOptions = BasicWindowOptions.CreateCopy(options);
        IJSInProcessRuntime xJsInProcess = (IJSInProcessRuntime)_js;
        IJSObjectReference xJsWindowReference = await xJsInProcess.InvokeAsync<IJSObjectReference>("WinBoxWindowManager.OpenWindow", title, xJsWindowOptions, windowInstanceReference, mount);
        Guid xWindowId = Guid.Parse(options.Id);

        // The window may have been closed while the javascript side was creating it,
        // in that case the window would stay orphaned and unreachable from blazor.
        if (!_windowsBlzrReferences.ContainsKey(xWindowId) || _windowsJsReferences.ContainsKey(xWindowId))
        {
            TryCloseJsWindow(xJsWindowReference);
            await xJsWindowReference.DisposeAsync();
            _pendingToolbarActions.Remove(xWindowId);
            return;
        }

        IJSInProcessObjectReference xWindowJsReference = xJsWindowReference as IJSInProcessObjectReference;
        _windowsJsReferences[xWindowId] = xWindowJsReference;
        FlushPendingToolbarActions(xWindowId, xWindowJsReference);
    }

    private static void TryCloseJsWindow(IJSObjectReference jsWindowReference)
    {
        if (jsWindowReference is not IJSInProcessObjectReference xInProcessReference)
        {
            return;
        }

        try
        {
            xInProcessReference.InvokeVoid("close", true);
        }
        catch (JSException)
        {
            // the window is being discarded anyway
        }
    }

    private static void DisposeJsReference(IJSInProcessObjectReference jsWindowReference)
    {
        if (jsWindowReference is null)
        {
            return;
        }

        try
        {
            jsWindowReference.Dispose();
        }
        catch (JSDisconnectedException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
    }

    public bool HasJsWindowReference(Guid id)
    {
        return _windowsJsReferences.ContainsKey(id);
    }

    public void CloseBlzr(Guid winId, WindowResult result)
    {

        _windowsBlzrReferences.TryGetValue(winId, out IWindowReference xWindowReference);
        if (xWindowReference is not null)
        {
            xWindowReference.Close(result);
            OnWindowCloseRequested?.Invoke(xWindowReference, result);
            _windowsBlzrReferences.Remove(winId);
        }

        if (_windowsJsReferences.Remove(winId, out IJSInProcessObjectReference xWindowJsReference))
        {
            DisposeJsReference(xWindowJsReference);
        }

        _pendingToolbarActions.Remove(winId);
    }

    public void Close(Guid winId, WindowResult result)
    {

        _windowsBlzrReferences.TryGetValue(winId, out IWindowReference xWindowReference);
        if (xWindowReference is not null)
        {
            xWindowReference.Close(result);
            OnWindowCloseRequested?.Invoke(xWindowReference, result);
            _windowsBlzrReferences.Remove(winId);
        }
        //to close  window in javascript
        if (_windowsJsReferences.Remove(winId, out IJSInProcessObjectReference xWindowJsReference))
        {
            try
            {
                xWindowJsReference?.InvokeVoid("close", true);
            }
            finally
            {
                DisposeJsReference(xWindowJsReference);
            }
        }

        _pendingToolbarActions.Remove(winId);
    }

    public void Fullscreen(Guid winId, bool? state = null)
    {
        _windowsJsReferences.TryGetValue(winId, out IJSInProcessObjectReference xWindowJsReference);
        if (xWindowJsReference is not null)
        {
            _windowsJsReferences[winId].InvokeVoid("fullscreen", state);
        }
    }

    public void Hide(Guid winId, bool? state = null)
    {
        _windowsJsReferences.TryGetValue(winId, out IJSInProcessObjectReference xWindowJsReference);
        if (xWindowJsReference is not null)
        {
            _windowsJsReferences[winId].InvokeVoid("hide", state);
        }
    }

    public void Maximize(Guid winId, bool? state = null)
    {
        _windowsJsReferences.TryGetValue(winId, out IJSInProcessObjectReference xWindowJsReference);
        if (xWindowJsReference is not null)
        {
            _windowsJsReferences[winId].InvokeVoid("maximize", state);
        }
    }

    public void Minimize(Guid winId, bool? state = null)
    {
        _windowsJsReferences.TryGetValue(winId, out IJSInProcessObjectReference xWindowJsReference);
        if (xWindowJsReference is not null)
        {
            _windowsJsReferences[winId].InvokeVoid("minimize", state);
        }
    }

    public void Move(Guid winId, string x, string y)
    {
        _windowsJsReferences.TryGetValue(winId, out IJSInProcessObjectReference xWindowJsReference);
        if (xWindowJsReference is not null)
        {
            _windowsJsReferences[winId].InvokeVoid("move", x, y);
        }
    }

    public void RemoveClass(Guid winId, string cssClass)
    {
        _windowsJsReferences.TryGetValue(winId, out IJSInProcessObjectReference xWindowJsReference);
        if (xWindowJsReference is not null)
        {
            _windowsJsReferences[winId].InvokeVoid("removeClass", cssClass);
        }
    }

    public void AddClass(Guid winId, string cssClass)
    {
        _windowsJsReferences.TryGetValue(winId, out IJSInProcessObjectReference xWindowJsReference);
        if (xWindowJsReference is not null)
        {
            _windowsJsReferences[winId].InvokeVoid("addClass", cssClass);
        }
    }

    public void Resize(Guid winId, string width, string height)
    {
        _windowsJsReferences.TryGetValue(winId, out IJSInProcessObjectReference xWindowJsReference);
        if (xWindowJsReference is not null)
        {
            _windowsJsReferences[winId].InvokeVoid("resize", width, height);
        }
    }

    public void Restore(Guid winId)
    {
        _windowsJsReferences.TryGetValue(winId, out IJSInProcessObjectReference xWindowJsReference);
        if (xWindowJsReference is not null)
        {
            _windowsJsReferences[winId].InvokeVoid("restore");
        }
    }

    public void SetBackground(Guid winId, string backgroundColor)
    {
        _windowsJsReferences.TryGetValue(winId, out IJSInProcessObjectReference xWindowJsReference);
        if (xWindowJsReference is not null)
        {
            _windowsJsReferences[winId].InvokeVoid("setBackground", backgroundColor);
        }
    }

    public void SetTitle(Guid winId, string title)
    {
        _windowsJsReferences.TryGetValue(winId, out IJSInProcessObjectReference xWindowJsReference);
        if (xWindowJsReference is not null)
        {
            _windowsJsReferences[winId].InvokeVoid("setTitle", title);
        }
    }

    public void Show(Guid winId, bool? state = null)
    {
        _windowsJsReferences.TryGetValue(winId, out IJSInProcessObjectReference xWindowJsReference);
        if (xWindowJsReference is not null)
        {
            _windowsJsReferences[winId].InvokeVoid("show", state);
        }
    }

    public void ToggleClass(Guid winId, string cssClass)
    {
        _windowsJsReferences.TryGetValue(winId, out IJSInProcessObjectReference xWindowJsReference);
        if (xWindowJsReference is not null)
        {
            _windowsJsReferences[winId].InvokeVoid("toggleClass", cssClass);
        }
    }

    public void Focus(Guid winId, bool? state = null)
    {
        _windowsJsReferences.TryGetValue(winId, out IJSInProcessObjectReference xWindowJsReference);
        if (xWindowJsReference is not null)
        {
            _windowsJsReferences[winId].InvokeVoid("focus", state);
        }
    }

    public void Blur(Guid winId, bool? state = null)
    {
        _windowsJsReferences.TryGetValue(winId, out IJSInProcessObjectReference xWindowJsReference);
        if (xWindowJsReference is not null)
        {
            _windowsJsReferences[winId].InvokeVoid("blur", state);
        }
    }

    public void Mount(Guid winId, ElementReference windowContent)
    {
        _windowsJsReferences.TryGetValue(winId, out IJSInProcessObjectReference xWindowJsReference);
        if (xWindowJsReference is not null)
        {
            _windowsJsReferences[winId].InvokeVoid("mount", windowContent);
        }
    }

    public void AddToolbarButton(Guid winId, ToolbarButton button)
    {
        InvokeTitleBarAction(winId, xWindowJsReference => xWindowJsReference.InvokeVoid("blazorAddToolbarButton", button));
    }

    public void UpdateToolbarButton(Guid winId, ToolbarButton button)
    {
        InvokeTitleBarAction(winId, xWindowJsReference => xWindowJsReference.InvokeVoid("blazorUpdateToolbarButton", button));
    }

    public void RemoveToolbarButton(Guid winId, string buttonId)
    {
        InvokeTitleBarAction(winId, xWindowJsReference => xWindowJsReference.InvokeVoid("blazorRemoveToolbarButton", buttonId));
    }

    public void SetToolbarButtons(Guid winId, IEnumerable<ToolbarButton> buttons)
    {
        List<ToolbarButton> xButtons = buttons?.ToList() ?? new List<ToolbarButton>();
        InvokeTitleBarAction(winId, xWindowJsReference => xWindowJsReference.InvokeVoid("blazorSetToolbarButtons", xButtons));
    }

    public void UpdateControlButton(Guid winId, WindowControlKind kind, WindowControlButton button)
    {
        InvokeTitleBarAction(winId, xWindowJsReference => xWindowJsReference.InvokeVoid("blazorUpdateControlButton", kind.ToString(), button));
    }

    /// <summary>
    /// Toolbar changes can be requested before the javascript window is created (e.g. right after opening it),
    /// so they are buffered and applied as soon as the window reference is available.
    /// </summary>
    private void InvokeTitleBarAction(Guid winId, Action<IJSInProcessObjectReference> toolbarAction)
    {
        if (TryGetJsWindowReference(winId, out IJSInProcessObjectReference xWindowJsReference))
        {
            toolbarAction(xWindowJsReference);
            return;
        }

        if (!_pendingToolbarActions.TryGetValue(winId, out List<Action<IJSInProcessObjectReference>> xPendingActions))
        {
            xPendingActions = new List<Action<IJSInProcessObjectReference>>();
            _pendingToolbarActions.Add(winId, xPendingActions);
        }

        xPendingActions.Add(toolbarAction);
    }

    private void FlushPendingToolbarActions(Guid winId, IJSInProcessObjectReference xWindowJsReference)
    {
        if (xWindowJsReference is null || !_pendingToolbarActions.Remove(winId, out List<Action<IJSInProcessObjectReference>> xPendingActions))
        {
            return;
        }

        foreach (Action<IJSInProcessObjectReference> iAction in xPendingActions)
        {
            iAction(xWindowJsReference);
        }
    }

    private bool TryGetJsWindowReference(Guid winId, out IJSInProcessObjectReference xWindowJsReference)
    {
        return _windowsJsReferences.TryGetValue(winId, out xWindowJsReference) && xWindowJsReference is not null;
    }
}
