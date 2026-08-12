namespace AutoRetainer.UI.NeoUI.Experiments;

internal class NightMode : ExperimentUIEntry
{
    public override string Name => "夜间模式";
    public override void Draw()
    {
        ImGuiEx.TextWrapped($"夜间模式:\n" +
                $"- 在登录画面等待选项将被强制启用\n" +
                $"- 将强制启用内置帧率限制器\n" +
                $"- 当窗口未聚焦且在等待时，游戏帧率将限制为 0.2\n" +
                $"- 游戏看起来可能会像当机，但在你重新激活游戏窗口后，请给它最多 5 秒的时间恢复运作。\n" +
                $"- 预设情况下，夜间模式仅启用潜艇自动化\n" +
                $"- 禁用夜间模式后，紧急恢复模块会启动并重新登录游戏。");
        if(ImGui.Checkbox("启用夜间模式", ref C.NightMode)) MultiMode.BailoutNightMode();
        ImGui.Checkbox("显示夜间模式勾选框", ref C.ShowNightMode);
        ImGui.Checkbox("在夜间模式下处理雇员", ref C.NightModeRetainers);
        ImGui.Checkbox("在夜间模式下处理派遣", ref C.NightModeDeployables);
        ImGui.Checkbox("使夜间模式状态持久化", ref C.NightModePersistent);
        ImGui.Checkbox("使关机指令改为启动夜间模式而非关闭游戏", ref C.ShutdownMakesNightMode);
    }
}
