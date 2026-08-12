namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class MultiModeRetainers : NeoUIEntry
{
    public override string Path => "多角色模式/雇员设置";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("多角色模式 - 雇员设置")
        .Checkbox("等待探险完成", () => ref C.MultiModeRetainerConfiguration.MultiWaitForAll, "在多角色模式下，AutoRetainer 会等到所有雇员都回归后才切换至下一个角色。")
        .Widget(() =>
        {
            ImGuiEx.TextWrapped("雇员操作与切换角色的额外随机延迟（单位：秒，保留两位小数；最小值和最大值均为 0 表示不延迟）");
            ImGui.SetNextItemWidth(120f);
            ImGui.InputFloat("雇员操作延迟最小值", ref C.MultiRetainerOperationDelayMinSeconds, 0.01f, 0.10f, "%.2f");
            ImGui.SetNextItemWidth(120f);
            ImGui.InputFloat("雇员操作延迟最大值", ref C.MultiRetainerOperationDelayMaxSeconds, 0.01f, 0.10f, "%.2f");
            ImGui.SetNextItemWidth(120f);
            ImGui.InputFloat("关闭雇员界面并切换角色延迟最小值", ref C.MultiRetainerExitDelayMinSeconds, 0.01f, 0.10f, "%.2f");
            ImGui.SetNextItemWidth(120f);
            ImGui.InputFloat("关闭雇员界面并切换角色延迟最大值", ref C.MultiRetainerExitDelayMaxSeconds, 0.01f, 0.10f, "%.2f");
            C.MultiRetainerOperationDelayMinSeconds = NormalizeDelay(C.MultiRetainerOperationDelayMinSeconds);
            C.MultiRetainerOperationDelayMaxSeconds = Math.Max(C.MultiRetainerOperationDelayMinSeconds, NormalizeDelay(C.MultiRetainerOperationDelayMaxSeconds));
            C.MultiRetainerExitDelayMinSeconds = NormalizeDelay(C.MultiRetainerExitDelayMinSeconds);
            C.MultiRetainerExitDelayMaxSeconds = Math.Max(C.MultiRetainerExitDelayMinSeconds, NormalizeDelay(C.MultiRetainerExitDelayMaxSeconds));
        })
        .DragInt(60f, "提前重登阈值(秒)", () => ref C.MultiModeRetainerConfiguration.AdvanceTimer.ValidateRange(0, 300), 0.1f, 0, 300)
        .SliderInt(100f, "继续操作所需的最小背包空格", () => ref C.MultiMinInventorySlots.ValidateRange(2, 9999), 2, 30)
        .Checkbox("同步雇员状态（一次性）", () => ref MultiMode.Synchronize, "AutoRetainer 会等待直到所有启用的雇员都完成探险。之后此设置将自动禁用，并开始处理所有角色。")
        .Checkbox($"强制执行完整角色轮换", () => ref C.CharEqualize, "推荐给拥有超过 15 个角色的用户。强制多角色模式按顺序处理所有角色的探险，然后才回到循环起点。")
        .Indent()
        .Checkbox("依探险完成时间排序角色", () => ref C.LongestVentureFirst, "优先检查那些很久以前就已完成探险的角色")
        .Checkbox("依雇员等级与上限排序角色", () => ref C.CappedLevelsLast, "优先处理有雇员可升级的角色；其次是雇员满级的角色；最后是雇员未满级且达到当前等级上限的角色。")
        .Unindent();

    private static float NormalizeDelay(float seconds)
    {
        if(!float.IsFinite(seconds)) return 0f;
        return Math.Clamp(MathF.Round(seconds, 2, MidpointRounding.AwayFromZero), 0f, 3600f);
    }
}
