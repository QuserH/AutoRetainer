namespace AutoRetainer.UI.NeoUI;
public class MiscTab : NeoUIEntry
{
    public override string Path => "杂项";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("统计信息")
        .Checkbox($"记录雇员探险统计", () => ref C.RecordStats)

        .Section("自动筹备稀有品")
        .Checkbox("筹备稀有品完成时发送托盘通知（需要NotificationMaster插件）", () => ref C.GCHandinNotify)

        .Section("效能")

        .If(() => Utils.IsBusy)
        .Widget("", (x) => ImGui.BeginDisabled())
        .EndIf()

        .Checkbox($"插件运行时解除最小化帧率限制", () => ref C.UnlockFPS)
        .Checkbox($"- 同时解除常规帧率限制", () => ref C.UnlockFPSUnlimited)
        .Checkbox($"- 同时暂停 ChillFrames 插件", () => ref C.UnlockFPSChillFrames)
        .Checkbox($"插件运行时提高FFXIV进程优先权", () => ref C.ManipulatePriority, "可能导致其他程式变慢")

        .If(() => Utils.IsBusy)
        .Widget("", (x) => ImGui.EndDisabled())
        .EndIf();
}
