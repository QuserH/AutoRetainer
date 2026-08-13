namespace AutoRetainer.UI.NeoUI;
public class MainSettings : NeoUIEntry
{
    public override string Path => "一般";

    public override NuiBuilder Builder { get; init; } = new NuiBuilder()
        .Section("延迟设置")
        .Widget(100f, "时间不同步补偿", (x) => ImGuiEx.SliderInt(x, ref C.UnsyncCompensation.ValidateRange(-60, 0), -10, 0), "从探险结束时间额外扣除的秒数。这有助于减缓游戏服务器与你电脑之间时间不同步所产生的问题。")
        .Widget(100f, "额外交互延迟（帧）", (x) => ImGuiEx.SliderInt(x, ref C.ExtraFrameDelay.ValidateRange(-10, 100), 0, 50), "数值越低，操作越快。游戏帧率较低或网络延迟较高时，建议提高此值；想加快操作可适当降低。")
        .Widget("额外日志", (x) => ImGui.Checkbox(x, ref C.ExtraDebug), "启用详细调试日志。开启后会产生大量日志并影响性能；插件重载或游戏重启时会自动关闭。")

            .Section("操作模式")
        .Widget("指派 + 重新指派", (x) =>
        {
            if(ImGui.RadioButton(x, C.EnableAssigningQuickExploration && !C._dontReassign))
            {
                C.EnableAssigningQuickExploration = true;
                C.DontReassign = false;
            }
        }, "若雇员当前没有任务，将自动分派\"自由探索\"，并在完成后自动重新派遣相同的任务。")
        .Widget("领取回报", (x) =>
        {
            if(ImGui.RadioButton(x, !C.EnableAssigningQuickExploration && C._dontReassign))
            {
                C.EnableAssigningQuickExploration = false;
                C.DontReassign = true;
            }
        }, "仅领取雇员的探险奖励，不会重新派遣。与传唤铃互动时按住 CTRL 可临时使用此模式。")
        .Widget("重新指派", (x) =>
        {
            if(ImGui.RadioButton("重新指派", !C.EnableAssigningQuickExploration && !C._dontReassign))
            {
                C.EnableAssigningQuickExploration = false;
                C.DontReassign = false;
            }
        }, "仅重新派遣雇员目前正在进行的相同任务")
        .Widget("雇员感官", (x) => ImGui.Checkbox(x, ref C.RetainerSense), "当玩家进入雇员铃的互动范围内时，AutoRetainer 将自动启用。期间你必须保持静止，否则会取消启用。")
        .Widget(200f, "启动时间", (x) => ImGuiEx.SliderIntAsFloat(x, ref C.RetainerSenseThreshold, 1000, 100000));


}
