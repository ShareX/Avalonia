using System;
using NWayland;
using NWayland.Interop;
using NWayland.Protocols.Wayland;
using NWayland.Protocols.XdgShell;

namespace Avalonia.Wayland.Server.Interop;

/// <summary>
/// Hand-written NWayland client proxy for <c>zwlr_layer_surface_v1</c> (v1..v4).
/// NWayland 0.11.0 does not ship generated bindings for <c>wlr-layer-shell-unstable-v1</c>.
/// </summary>
internal sealed class ZwlrLayerSurfaceV1 : WlProxy, IWlProxyTypeDescriptorProvider
{
    public static WlInterfaceDescription InterfaceDescription { get; } = WlInterfaceDescription
        .Create("zwlr_layer_surface_v1", 4)
        .AddMethod(WlMessageDescription.Create("set_size")
            .Add(WlMessageArgumentDescription.UInt32)
            .Add(WlMessageArgumentDescription.UInt32)
            .Build())
        .AddMethod(WlMessageDescription.Create("set_anchor")
            .Add(WlMessageArgumentDescription.UInt32)
            .Build())
        .AddMethod(WlMessageDescription.Create("set_exclusive_zone")
            .Add(WlMessageArgumentDescription.Int32)
            .Build())
        .AddMethod(WlMessageDescription.Create("set_margin")
            .Add(WlMessageArgumentDescription.Int32)
            .Add(WlMessageArgumentDescription.Int32)
            .Add(WlMessageArgumentDescription.Int32)
            .Add(WlMessageArgumentDescription.Int32)
            .Build())
        .AddMethod(WlMessageDescription.Create("set_keyboard_interactivity")
            .Add(WlMessageArgumentDescription.UInt32)
            .Build())
        .AddMethod(WlMessageDescription.Create("get_popup")
            .Add(WlMessageArgumentDescription.Object(XdgPopup.ProxyType))
            .Build())
        .AddMethod(WlMessageDescription.Create("ack_configure")
            .Add(WlMessageArgumentDescription.UInt32)
            .Build())
        .AddMethod(WlMessageDescription.Create("destroy")
            .IsDestructor()
            .Build())
        .AddMethod(WlMessageDescription.Create("set_layer")
            .SinceVersion(2)
            .Add(WlMessageArgumentDescription.UInt32)
            .Build())
        .AddEvent(WlMessageDescription.Create("configure")
            .Add(WlMessageArgumentDescription.UInt32)
            .Add(WlMessageArgumentDescription.UInt32)
            .Add(WlMessageArgumentDescription.UInt32)
            .Build())
        .AddEvent(WlMessageDescription.Create("closed")
            .Build())
        .Build();

    public static WlProxyTypeDescriptor ProxyType { get; } = new(
        InterfaceDescription,
        typeof(ZwlrLayerSurfaceV1),
        static ctx => new ZwlrLayerSurfaceV1(ctx),
        Frozen: false,
        ServerResourceType: null!,
        ServerFactory: null!);

    static WlProxyTypeDescriptor IWlProxyTypeDescriptorProvider.ProxyType => ProxyType;

    public ZwlrLayerSurfaceV1(WlProxyCreationContext context) : base(context)
    {
    }

    public bool IsSetLayerAvailable => Version >= 2;

    public void SetSize(uint width, uint height)
    {
        using var call = WaylandCallBuilder.Create(this, 0);
        call.Arg(width);
        call.Arg(height);
        call.Invoke();
    }

    public void SetAnchor(uint anchor)
    {
        using var call = WaylandCallBuilder.Create(this, 1);
        call.Arg(anchor);
        call.Invoke();
    }

    public void SetExclusiveZone(int zone)
    {
        using var call = WaylandCallBuilder.Create(this, 2);
        call.Arg(zone);
        call.Invoke();
    }

    public void SetMargin(int top, int right, int bottom, int left)
    {
        using var call = WaylandCallBuilder.Create(this, 3);
        call.Arg(top);
        call.Arg(right);
        call.Arg(bottom);
        call.Arg(left);
        call.Invoke();
    }

    public void SetKeyboardInteractivity(uint keyboardInteractivity)
    {
        using var call = WaylandCallBuilder.Create(this, 4);
        call.Arg(keyboardInteractivity);
        call.Invoke();
    }

    public void GetPopup(XdgPopup popup)
    {
        using var call = WaylandCallBuilder.Create(this, 5);
        call.Arg(popup);
        call.Invoke();
    }

    public void AckConfigure(uint serial)
    {
        using var call = WaylandCallBuilder.Create(this, 6);
        call.Arg(serial);
        call.Invoke();
    }

    public void Destroy()
    {
        using var call = WaylandCallBuilder.Create(this, 7);
        call.Invoke();
    }

    public void SetLayer(ZwlrLayerShellV1.LayerEnum layer)
    {
        if (!IsSetLayerAvailable)
            return;

        using var call = WaylandCallBuilder.Create(this, 8);
        call.Arg((uint)layer);
        call.Invoke();
    }

    internal abstract class Listener : IWlEventsListener
    {
        protected virtual void Configure(ZwlrLayerSurfaceV1 eventSender, uint serial, uint width, uint height) { }
        protected virtual void Closed(ZwlrLayerSurfaceV1 eventSender) { }

        void IWlEventsListener.DispatchEvent(WlEventArgs args)
        {
            switch (args.Opcode)
            {
                case 0:
                    Configure((ZwlrLayerSurfaceV1)args.Sender, args.GetUInt32(0), args.GetUInt32(1), args.GetUInt32(2));
                    break;
                case 1:
                    Closed((ZwlrLayerSurfaceV1)args.Sender);
                    break;
            }
        }
    }
}

/// <summary>
/// Hand-written NWayland client proxy for <c>zwlr_layer_shell_v1</c> (v1..v4).
/// </summary>
internal sealed class ZwlrLayerShellV1 : WlProxy, IWlProxyTypeDescriptorProvider
{
    public enum LayerEnum : uint
    {
        Background = 0,
        Bottom = 1,
        Top = 2,
        Overlay = 3,
    }

    public static WlInterfaceDescription InterfaceDescription { get; } = WlInterfaceDescription
        .Create("zwlr_layer_shell_v1", 4)
        .AddMethod(WlMessageDescription.Create("get_layer_surface")
            .Add(WlMessageArgumentDescription.NewId(ZwlrLayerSurfaceV1.ProxyType))
            .Add(WlMessageArgumentDescription.Object(WlSurface.ProxyType))
            .Add(WlMessageArgumentDescription.Object(WlOutput.ProxyType).AsNullable())
            .Add(WlMessageArgumentDescription.UInt32)
            .Add(WlMessageArgumentDescription.String)
            .Build())
        .AddMethod(WlMessageDescription.Create("destroy")
            .IsDestructor()
            .SinceVersion(3)
            .Build())
        .Build();

    public static WlProxyTypeDescriptor ProxyType { get; } = new(
        InterfaceDescription,
        typeof(ZwlrLayerShellV1),
        static ctx => new ZwlrLayerShellV1(ctx),
        Frozen: false,
        ServerResourceType: null!,
        ServerFactory: null!);

    static WlProxyTypeDescriptor IWlProxyTypeDescriptorProvider.ProxyType => ProxyType;

    public ZwlrLayerShellV1(WlProxyCreationContext context) : base(context)
    {
    }

    public ZwlrLayerSurfaceV1 GetLayerSurface(
        WlSurface surface,
        WlOutput? output,
        LayerEnum layer,
        string @namespace,
        ZwlrLayerSurfaceV1.Listener? eventsListener = null,
        IWlTargetQueue? dispatchOnQueue = null)
    {
        using var call = WaylandCallBuilder.Create(this, 0);
        call.ArgNewId();
        call.Arg(surface);
        call.Arg(output!);
        call.Arg((uint)layer);
        call.Arg(@namespace);
        return call.InvokeNewId<ZwlrLayerSurfaceV1>(eventsListener, dispatchOnQueue, null);
    }

    public void Destroy()
    {
        if (Version < 3)
            return;

        using var call = WaylandCallBuilder.Create(this, 1);
        call.Invoke();
    }
}
