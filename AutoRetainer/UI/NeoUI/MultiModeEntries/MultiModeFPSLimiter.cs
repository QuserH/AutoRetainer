namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeFPSLimiter : NeoUIEntry
{
    public override string Path => "多角色模式/帧率限制器";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("帧率限制器")
        .TextWrapped("帧率限制器仅在多角色模式启用时运行")
        .Widget("空闲时的目标帧率", (x) =>
        {
            ImGui.SetNextItemWidth(100f);
            UIUtils.SliderIntFrameTimeAsFPS(x, ref C.TargetMSPTIdle, C.ExtraFPSLockRange ? 1 : 10);
        })
        .Widget("空闲时的目标帧率", (x) =>
        {
            ImGui.SetNextItemWidth(100f);
            UIUtils.SliderIntFrameTimeAsFPS("操作时的目标帧率", ref C.TargetMSPTRunning, C.ExtraFPSLockRange ? 1 : 20);
        })
        .Checkbox("游戏处于活动状态时解除帧率限制", () => ref C.NoFPSLockWhenActive)
        .Checkbox($"允许更低的帧率限制值", () => ref C.ExtraFPSLockRange, "启用后如在多角色模式中遇到问题，将无法提供支持")
        .Checkbox($"仅在设置关闭计时器时启动限制器", () => ref C.FpsLockOnlyShutdownTimer);
}
