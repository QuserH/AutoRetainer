namespace AutoRetainer.Modules.GcHandin;

internal static class AutoGCHandinUI
{
    internal static void Draw()
    {
        ImGui.Checkbox("筹备稀有品完成时发送托盘通知（需要 NotificationMaster 插件）", ref C.GCHandinNotify);
    }
}
