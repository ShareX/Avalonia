using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.UnitTests;
using Avalonia.Wayland.Server.Interop;
using Xunit;

namespace Avalonia.Wayland.UnitTests;

public class WaylandLayerShellTests
{
    private const string LayerNamespace = "avalonia-layer-test";

    [Fact]
    public void WaylandProperties_LayerDefaultsAndRoundTrip()
    {
        using var app = UnitTestApplication.Start(TestServices.MockWindowingPlatform);
        var window = new Window();

        Assert.Null(WaylandProperties.GetLayer(window));
        Assert.Equal(WaylandLayerAnchor.None, WaylandProperties.GetLayerAnchor(window));
        Assert.Equal(0, WaylandProperties.GetLayerExclusiveZone(window));
        Assert.Equal(default, WaylandProperties.GetLayerMargin(window));
        Assert.Equal(WaylandLayerKeyboardInteractivity.OnDemand, WaylandProperties.GetLayerKeyboardInteractivity(window));
        Assert.Null(WaylandProperties.GetLayerNamespace(window));
        Assert.Null(WaylandProperties.GetLayerOutputName(window));

        var margin = new Thickness(4, 8, 12, 16);
        WaylandProperties.SetLayer(window, WaylandLayer.Overlay);
        WaylandProperties.SetLayerAnchor(window, WaylandLayerAnchor.All);
        WaylandProperties.SetLayerExclusiveZone(window, -1);
        WaylandProperties.SetLayerMargin(window, margin);
        WaylandProperties.SetLayerKeyboardInteractivity(window, WaylandLayerKeyboardInteractivity.Exclusive);
        WaylandProperties.SetLayerNamespace(window, LayerNamespace);
        WaylandProperties.SetLayerOutputName(window, "DP-1");

        Assert.Equal(WaylandLayer.Overlay, WaylandProperties.GetLayer(window));
        Assert.Equal(WaylandLayerAnchor.All, WaylandProperties.GetLayerAnchor(window));
        Assert.Equal(-1, WaylandProperties.GetLayerExclusiveZone(window));
        Assert.Equal(margin, WaylandProperties.GetLayerMargin(window));
        Assert.Equal(WaylandLayerKeyboardInteractivity.Exclusive, WaylandProperties.GetLayerKeyboardInteractivity(window));
        Assert.Equal(LayerNamespace, WaylandProperties.GetLayerNamespace(window));
        Assert.Equal("DP-1", WaylandProperties.GetLayerOutputName(window));
    }

    [Fact]
    public void LayerShellProtocolDescriptors_MatchWaylandSpec()
    {
        var shellIface = ZwlrLayerShellV1.ProxyType.Interface;
        Assert.Equal("zwlr_layer_shell_v1", shellIface.Name);
        Assert.Equal(4, shellIface.Version);
        Assert.Equal(2, shellIface.Methods.Count);
        Assert.Empty(shellIface.Events);
        Assert.Equal("get_layer_surface", shellIface.Methods[0].Name);
        Assert.Equal("no?ous", shellIface.Methods[0].Signature);
        Assert.Equal("destroy", shellIface.Methods[1].Name);
        Assert.True(shellIface.Methods[1].IsDestructor);
        Assert.Equal(3, shellIface.Methods[1].SinceVersion);

        var surfaceIface = ZwlrLayerSurfaceV1.ProxyType.Interface;
        Assert.Equal("zwlr_layer_surface_v1", surfaceIface.Name);
        Assert.Equal(4, surfaceIface.Version);
        Assert.Equal(9, surfaceIface.Methods.Count);
        Assert.Equal(2, surfaceIface.Events.Count);
        Assert.Equal("configure", surfaceIface.Events[0].Name);
        Assert.Equal("uuu", surfaceIface.Events[0].Signature);
        Assert.Equal("closed", surfaceIface.Events[1].Name);
        Assert.Equal("set_layer", surfaceIface.Methods[8].Name);
        Assert.Equal(2, surfaceIface.Methods[8].SinceVersion);
    }

    [Fact]
    public void LiveHyprland_LayerShellWindowMapsOnOverlayLayer()
    {
        if (!OperatingSystem.IsLinux() ||
            Environment.GetEnvironmentVariable("AVALONIA_WAYLAND_LAYER_TEST") != "1" ||
            string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY")))
        {
            return;
        }

        var lifetime = new ClassicDesktopStyleApplicationLifetime
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown
        };

        var app = AppBuilder.Configure<Application>()
            .UseWayland()
            .UseSkia()
            .UseHarfBuzz()
            .SetupWithLifetime(lifetime);
        _ = app;

        var window = new Window
        {
            Background = new SolidColorBrush(Color.FromArgb(64, 24, 96, 192)),
            Content = new Border
            {
                BorderBrush = Brushes.Lime,
                BorderThickness = new Thickness(2),
            }
        };

        Assert.True(WaylandProperties.IsWaylandBackend(window));
        Assert.True(WaylandProperties.IsLayerShellSupported(window));

        WaylandProperties.SetLayer(window, WaylandLayer.Overlay);
        WaylandProperties.SetLayerAnchor(window, WaylandLayerAnchor.All);
        WaylandProperties.SetLayerExclusiveZone(window, -1);
        WaylandProperties.SetLayerKeyboardInteractivity(window, WaylandLayerKeyboardInteractivity.Exclusive);
        WaylandProperties.SetLayerNamespace(window, LayerNamespace);

        bool? mappedOnOverlayLayer = null;
        Exception? failure = null;

        window.Opened += (_, _) =>
        {
            DispatcherTimer.RunOnce(() =>
            {
                try
                {
                    mappedOnOverlayLayer = IsHyprlandLayerMapped(LayerNamespace, expectedLevel: 3);
                }
                catch (Exception ex)
                {
                    failure = ex;
                }
                finally
                {
                    window.Close();
                }
            }, TimeSpan.FromMilliseconds(200));
        };

        window.Closed += (_, _) => Dispatcher.UIThread.Post(() => lifetime.Shutdown());

        DispatcherTimer.RunOnce(() =>
        {
            if (!window.IsVisible)
                return;
            failure ??= new TimeoutException("Timed out waiting for live Hyprland layer-shell test to complete.");
            window.Close();
        }, TimeSpan.FromSeconds(5));

        window.Show();
        lifetime.Start(Array.Empty<string>());

        if (failure != null)
            throw failure;

        Assert.True(mappedOnOverlayLayer, $"Expected '{LayerNamespace}' to map on Hyprland overlay layer 3.");

        Thread.Sleep(120);
        Assert.False(IsHyprlandLayerMapped(LayerNamespace, expectedLevel: 3),
            $"'{LayerNamespace}' remained mapped on Hyprland overlay layer 3 after Window.Close().");
    }

    private static bool IsHyprlandLayerMapped(string ns, int expectedLevel)
    {
        using var proc = Process.Start(new ProcessStartInfo("hyprctl", "layers -j")
        {
            UseShellExecute = false,
            RedirectStandardOutput = true
        }) ?? throw new InvalidOperationException("Failed to start hyprctl.");

        var json = proc.StandardOutput.ReadToEnd();
        proc.WaitForExit();
        if (proc.ExitCode != 0)
            return false;

        using var doc = JsonDocument.Parse(json);
        foreach (var monitor in doc.RootElement.EnumerateObject())
        {
            if (!monitor.Value.TryGetProperty("levels", out var levels))
                continue;

            if (!levels.TryGetProperty(expectedLevel.ToString(), out var levelArr) ||
                levelArr.ValueKind != JsonValueKind.Array)
                continue;

            foreach (var surface in levelArr.EnumerateArray())
            {
                if (surface.TryGetProperty("namespace", out var nsProp) &&
                    string.Equals(nsProp.GetString(), ns, StringComparison.Ordinal))
                    return true;
            }
        }

        return false;
    }
}
