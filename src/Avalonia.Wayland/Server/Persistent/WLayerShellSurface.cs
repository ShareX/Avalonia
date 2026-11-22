using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Wayland.Server.Interop;
using Avalonia.Wayland.Server.Transient;
using NWayland.Protocols.Wayland;
using NWayland.Protocols.XdgShell;

namespace Avalonia.Wayland.Server.Persistent;

internal sealed class WaylandLayerShellParams
{
    public WaylandLayer Layer { get; set; }
    public WaylandLayerAnchor Anchor { get; set; }
    public int ExclusiveZone { get; set; }
    public Thickness Margin { get; set; }
    public WaylandLayerKeyboardInteractivity KeyboardInteractivity { get; set; } = WaylandLayerKeyboardInteractivity.OnDemand;
    public string? Namespace { get; set; }
    public string? OutputName { get; set; }
    public Size DesiredSize { get; set; }
}

internal sealed class WLayerShellSurface : WXdgShellSurface, IWLayerSurface
{
    private readonly WLayerSurfaceEventSinkProxy _layerEventSink;
    private readonly WaylandLayerShellParams _params;
    private readonly TaskCompletionSource<XdgConfigureBatch> _basicInitCompletedTcs =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
    private ZwlrLayerSurfaceV1? _layerSurface;

    public WLayerShellSurface(
        WaylandWorker worker,
        WLayerSurfaceEventSinkProxy eventSink,
        WaylandLayerShellParams initialParams)
        : base(worker, eventSink)
    {
        _layerEventSink = eventSink;
        _params = new WaylandLayerShellParams
        {
            Layer = initialParams.Layer,
            Anchor = initialParams.Anchor,
            ExclusiveZone = initialParams.ExclusiveZone,
            Margin = initialParams.Margin,
            KeyboardInteractivity = initialParams.KeyboardInteractivity,
            Namespace = initialParams.Namespace,
            OutputName = initialParams.OutputName,
            DesiredSize = initialParams.DesiredSize,
        };
    }

    public Task<XdgConfigureBatch> BasicInitCompleted => _basicInitCompletedTcs.Task;

    protected override bool UsesXdgSurface => false;

    internal override bool CanAttachChildPopup => IsMapped && _layerSurface != null;

    internal override XdgPopup AttachChildPopup(XdgSurface childXdgSurface, XdgPositioner positioner, XdgPopup.Listener listener)
    {
        var popup = childXdgSurface.GetPopup(null!, positioner, listener);
        _layerSurface!.GetPopup(popup);
        return popup;
    }

    public override void OnConnected(WaylandConnection connection, WaylandGlobals globals)
    {
        base.OnConnected(connection, globals);

        if (globals.LayerShell is not { } layerShell)
            throw new AvaloniaWaylandException("Compositor does not support zwlr_layer_shell_v1.");

        WlOutput? targetOutput = null;
        if (!string.IsNullOrWhiteSpace(_params.OutputName))
        {
            targetOutput = globals.Outputs.Outputs.FirstOrDefault(o =>
                string.Equals(o.XdgName, _params.OutputName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(o.OutputName, _params.OutputName, StringComparison.OrdinalIgnoreCase))?.WlOutput;
        }

        var ns = string.IsNullOrWhiteSpace(_params.Namespace) ? "avalonia-layer" : _params.Namespace!;
        _layerSurface = layerShell.GetLayerSurface(
            WlSurface!,
            targetOutput,
            (ZwlrLayerShellV1.LayerEnum)_params.Layer,
            ns,
            new LayerSurfaceListener(this),
            connection.Queue);

        ApplyAllLayerState();
        WlSurface!.Commit();
    }

    private void ApplyAllLayerState()
    {
        if (_layerSurface == null)
            return;

        _layerSurface.SetAnchor((uint)_params.Anchor);
        _layerSurface.SetExclusiveZone(_params.ExclusiveZone);
        _layerSurface.SetMargin(
            (int)Math.Round(_params.Margin.Top),
            (int)Math.Round(_params.Margin.Right),
            (int)Math.Round(_params.Margin.Bottom),
            (int)Math.Round(_params.Margin.Left));
        _layerSurface.SetKeyboardInteractivity((uint)_params.KeyboardInteractivity);
        ApplyRequestedSize();
    }

    private void ApplyRequestedSize()
    {
        if (_layerSurface == null)
            return;

        var stretchHoriz = (_params.Anchor & (WaylandLayerAnchor.Left | WaylandLayerAnchor.Right))
                           == (WaylandLayerAnchor.Left | WaylandLayerAnchor.Right);
        var stretchVert = (_params.Anchor & (WaylandLayerAnchor.Top | WaylandLayerAnchor.Bottom))
                          == (WaylandLayerAnchor.Top | WaylandLayerAnchor.Bottom);

        var w = stretchHoriz
            ? 0u
            : (uint)Math.Max(1, (int)Math.Round(_params.DesiredSize.Width));
        var h = stretchVert
            ? 0u
            : (uint)Math.Max(1, (int)Math.Round(_params.DesiredSize.Height));

        _layerSurface.SetSize(w, h);
    }

    public void SetLayer(WaylandLayer layer)
    {
        _params.Layer = layer;
        if (_layerSurface != null && WlSurface != null)
        {
            _layerSurface.SetLayer((ZwlrLayerShellV1.LayerEnum)layer);
            WlSurface.Commit();
        }
    }

    public void SetAnchor(WaylandLayerAnchor anchor)
    {
        _params.Anchor = anchor;
        if (_layerSurface != null && WlSurface != null)
        {
            _layerSurface.SetAnchor((uint)anchor);
            ApplyRequestedSize();
            WlSurface.Commit();
        }
    }

    public void SetExclusiveZone(int exclusiveZone)
    {
        _params.ExclusiveZone = exclusiveZone;
        if (_layerSurface != null && WlSurface != null)
        {
            _layerSurface.SetExclusiveZone(exclusiveZone);
            WlSurface.Commit();
        }
    }

    public void SetMargin(Thickness margin)
    {
        _params.Margin = margin;
        if (_layerSurface != null && WlSurface != null)
        {
            _layerSurface.SetMargin(
                (int)Math.Round(margin.Top),
                (int)Math.Round(margin.Right),
                (int)Math.Round(margin.Bottom),
                (int)Math.Round(margin.Left));
            WlSurface.Commit();
        }
    }

    public void SetKeyboardInteractivity(WaylandLayerKeyboardInteractivity interactivity)
    {
        _params.KeyboardInteractivity = interactivity;
        if (_layerSurface != null && WlSurface != null)
        {
            _layerSurface.SetKeyboardInteractivity((uint)interactivity);
            WlSurface.Commit();
        }
    }

    public void SetDesiredSize(Size size)
    {
        _params.DesiredSize = size;
        if (_layerSurface != null && WlSurface != null)
        {
            ApplyRequestedSize();
            WlSurface.Commit();
        }
    }

    protected override void AckConfigureCore(uint serial) => _layerSurface?.AckConfigure(serial);

    protected override void MaybeEmitWindowGeometry(PixelSize bufferPixelSize, double bufferScale, Size logicalSize)
    {
        int surfaceWidth;
        int surfaceHeight;
        if (HasFractionalScaling && logicalSize.Width > 0 && logicalSize.Height > 0)
        {
            surfaceWidth = Math.Max(1, (int)Math.Round(logicalSize.Width));
            surfaceHeight = Math.Max(1, (int)Math.Round(logicalSize.Height));
        }
        else
        {
            var intScale = Math.Max(1, (int)Math.Ceiling(bufferScale));
            surfaceWidth = Math.Max(1, (bufferPixelSize.Width + intScale - 1) / intScale);
            surfaceHeight = Math.Max(1, (bufferPixelSize.Height + intScale - 1) / intScale);
        }

        LastWindowGeometry = (0, 0, surfaceWidth, surfaceHeight);
    }

    private Size CalculateFallbackMaxSize()
    {
        var outputs = Outputs.Count > 0 ? Outputs : Globals?.Outputs.Outputs;
        if (outputs == null || outputs.Count == 0)
            return new Size(800, 600);

        Size? maxSize = null;
        foreach (var o in outputs)
        {
            var size = o.LogicalSize.ToSize(1);
            maxSize = maxSize == null
                ? size
                : new Size(Math.Min(maxSize.Value.Width, size.Width), Math.Min(maxSize.Value.Height, size.Height));
        }

        return maxSize ?? new Size(800, 600);
    }

    public override void OnDisconnected()
    {
        EventSink.OnKeyboardLeave();
        _layerSurface?.Destroy();
        _layerSurface?.Dispose();
        _layerSurface = null;
        base.OnDisconnected();
    }

    private sealed class LayerSurfaceListener(WLayerShellSurface p) : ZwlrLayerSurfaceV1.Listener
    {
        protected override void Configure(ZwlrLayerSurfaceV1 eventSender, uint serial, uint width, uint height)
        {
            var batch = new XdgConfigureBatch
            {
                Serial = serial,
                Size = new PixelSize((int)width, (int)height),
                States = XdgToplevelStates.Activated,
                MaxSize = width > 0 && height > 0 ? new Size(width, height) : p.CalculateFallbackMaxSize(),
            };

            p._layerEventSink.OnConfigure(batch);
            p._basicInitCompletedTcs.TrySetResult(batch);
            p.OnConfigureBatchComplete(serial);
        }

        protected override void Closed(ZwlrLayerSurfaceV1 eventSender) =>
            p._layerEventSink.OnClose();
    }
}
