using Dalamud.Interface.Components;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;

internal class SuperSecret : DebugSectionBase
{
    public override void Draw()
    {
        ImGuiEx.TextWrapped(ImGuiColors.ParsedOrange, "这里可能会发生任何状况");
        ImGui.Checkbox("旧版传唤铃感应", ref C.OldRetainerSense);
        ImGuiComponents.HelpMarker("检测并使用玩家有效距离内最近的传唤铃");
        ImGuiEx.TextWrapped(ImGuiColors.DalamudGrey, "在多角色模式执行期间，强制启用传唤铃感应");
        ImGui.Separator();
        ImGui.Checkbox($"不安全选项保护", ref C.UnsafeProtection);
        ImGui.SameLine();
        if(ImGui.Button($"写入登录档"))
        {
            Safety.Set(C.UnsafeProtection);
        }
        var g = Safety.Get();
        ImGuiEx.Text(g ? ImGuiColors.ParsedGreen : ImGuiColors.DalamudRed, $"Safety flag: {(g ? "Present" : "Absent")}");
        ImGui.Separator();
        ImGuiEx.Checkbox("忽略筹备任务时的大国防联军（GC）阶级检查", ref C.IgnoreGCRankCheck);
    }
}
