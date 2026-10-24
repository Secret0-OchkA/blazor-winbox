# Blazor Winbox

Multiple Windows Library that wraps [WinBox.js](https://github.com/nextapps-de/winbox) in Blazor Components.

*Windows management and rendering processes were inspired  by [MudBlazor](https://mudblazor.com) dialogs*
<br />
<br />

## Compatibility
Only Blazor WASM (client-side) .NET 6 applications are currently supported. This library is not tested on Blazor Server applications.
<br />
<br />

## Getting Started
### Installation:
The Nuget package page can be found [here](https://www.nuget.org/packages/Blazor.Winbox/)

To install `Blazor.Winbox` from Nuget Gallery simply search for its name and install the latest version

<br />

##### Or using Package Manager run the following command
```bash
Install-Package Blazor.Winbox
```
##### Or using .NET CLI run the following command
```bash
dotnet add package Blazor.Winbox
```

### Configurations:
* Import `BlazorWinbox` by adding the following in `_Imports.razor`
```
@using BlazorWinbox
```

* Register Service in `Program` Class:
 ```c#
    using BlazorWinbox; 


    builder.Services.AddBlazorWinbox();
 ```
 * Add script references to `index.html` after Blazor script
  ```html
  <script src="_content/Blazor.Winbox/winbox.bundle.min.js"></script>
  <script src="_content/Blazor.Winbox/BlazorWinbox.min.js"></script>
  ```
  * Add the following component to your `MainLayout.razor` 
  ```html
<WindowProvider />
  ```

### Open a window
 Create a `window` component. Let's use `Counter` template as example:
```html
<Window>

    <h1>Counter</h1>

    <p role="status">Current count: @currentCount</p>

    <button class="btn btn-primary" @onclick="IncrementCount">Click me</button>

</Window>

```
```c#
@code {

    private int currentCount = 0;

    private void IncrementCount()
    {
        currentCount++;
    }
}
```
**Note that whole window content should surrounded with `<Window>` Tag**

<br/>

To open window inject `IWindowManager` service and use `Open<TComponent>()` method:

```c#
//Use in razor
@inject IWindowManager windowManager
//OR in C# 
[Inject] public IWindowManager WindowManager { get; set; }

//Open a window
void OpenCounter(){
    IWindowReference windowReference = WindowManager.Open<Counter>();
}
```

![Preview](./Docs/BlazorWinbox.gif)

### Docking windows
Windows can dock (snap) to each other while they are dragged or resized. When the edge of the moved window gets close to the edge of another window, the window snaps to it and a dashed guide line shows the docking edge.

Docking is disabled by default. Enable it for a single window by assigning `WindowDockingOptions` to `WindowOptions.Docking`:

```c#
WindowOptions xWindowOptions = new()
{
    Width = "400px",
    Height = "300px",
    Docking = new WindowDockingOptions()
};

IWindowReference xWindowReference = WindowManager.Open<Counter>("Counter", windowOptions: xWindowOptions);
```

To enable docking for every window, assign it to the global options of `WindowProvider`:

```c#
_GlobalWindowOptions = new GlobalWindowOptions
{
    Docking = new WindowDockingOptions()
};
```

`WindowDockingOptions` properties:

| Property | Default | Description |
| --- | --- | --- |
| `Enabled` | `true` | Enables the docking behavior when the options object is assigned |
| `SnapDistance` | `10` | Max distance (px) between window edges at which the window snaps |
| `ShowGuides` | `true` | Shows dashed guide lines at the docked edges |
| `GuideColor` | light blue | Any css color of the guide lines, e.g. `"#22c55e"` or `"rgba(34, 197, 94, .8)"` |
| `GuideThickness` | `1` | Thickness of the guide lines in pixels |
| `Group` | `null` | Windows dock only to windows of the same group, so unrelated window sets can be kept apart |

Docking group example — blue and green windows dock only inside their own group:

```c#
WindowOptions xBlueOptions = new() { Docking = new WindowDockingOptions { Group = "blue" } };
WindowOptions xGreenOptions = new()
{
    Docking = new WindowDockingOptions { Group = "green", GuideColor = "#22c55e", GuideThickness = 2 }
};
```

If you have the `WinBox` instance at hand (e.g. from custom JavaScript), docking can also be changed after the window was created:

```js
const win = WinBoxWindowManager.OpenWindow(title, windowOptions); // or your own WinBox instance
WinBoxDocking.attach(win, { snapDistance: 20, group: "blue" });   // (re)enable with new settings
WinBoxDocking.detach(win);                                        // disable
```

