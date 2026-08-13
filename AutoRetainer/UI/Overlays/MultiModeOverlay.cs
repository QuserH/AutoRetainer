using AutoRetainer.Modules.Voyage;
using Dalamud.Game.ClientState.Conditions;
using System.IO;

namespace AutoRetainer.UI.Overlays;

internal class MultiModeOverlay : Window
{
    public MultiModeOverlay() : base("AutoRetainer 提醒", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoBackground, true)
    {
        P.WindowSystem.AddWindow(this);
        IsOpen = true;
        ShowCloseButton = false;
        RespectCloseHotkey = false;
    }

    private bool DisplayNotify => C.NotifyEnableOverlay && NotificationHandler.CurrentState && !NotificationHandler.IsHidden && (!C.NotifyCombatDutyNoDisplay || !(Svc.Condition[ConditionFlag.BoundByDuty56] || Svc.Condition[ConditionFlag.InCombat]));

    public override bool DrawConditions()
    {
        return !C.HideOverlayIcons && (P.TaskManager.IsBusy || P.IsNextToBell || MultiMode.Enabled || SchedulerMain.PluginEnabled || DisplayNotify || VoyageScheduler.Enabled || Shutdown.Active || SchedulerMain.CharacterPostProcessLocked);
    }

    private Vector2 StatusPanelSize => new(C.StatusBarIconWidth);

    public override void Draw()
    {
        var displayed = false;
        bool ShouldDisplay() => !displayed || !C.StatusBarMSI;
        CImGui.igBringWindowToDisplayBack(CImGui.igGetCurrentWindow());
        if(BailoutManager.IsLogOnTitleEnabled && ShouldDisplay())
        {
            displayed = true;
            if(ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "bailoutTitleRestart.png"), out var t))
            {
                ImGui.Image(t.Handle, StatusPanelSize);
                if(ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    if(ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        Svc.Commands.ProcessCommand("/ays");
                    }
                    if(ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    {
                        BailoutManager.IsLogOnTitleEnabled = false;
                    }
                    ImGui.SetTooltip("AutoRetainer 正在登录界面等待可用角色。\n左键点击 - 打开 AutoRetainer\n右键点击 - 中止等待");
                }
            }
            else
            {
                ImGuiEx.Text("正在加载界面…");
            }
            ImGui.SameLine();
        }
        if(Shutdown.Active && ShouldDisplay())
        {
            displayed = true;
            if(ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "timer.png"), out var t))
            {
                ImGui.Image(t.Handle, StatusPanelSize);
                if(ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    if(ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        Svc.Commands.ProcessCommand("/ays");
                    }
                    if(ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    {
                        Shutdown.ForceShutdownAt = 0;
                        Shutdown.ShutdownAt = 0;
                    }
                    ImGui.SetTooltip($"已设置关机计时器。\n将在 {TimeSpan.FromMilliseconds(Shutdown.ShutdownAt - Environment.TickCount64)} 后关机\n将在 {TimeSpan.FromMilliseconds(Shutdown.ForceShutdownAt - Environment.TickCount64)} 后强制关机\n左键点击 - 打开 AutoRetainer\n右键点击 - 清除计时器");
                }
            }
            else
            {
                ImGuiEx.Text("正在加载界面…");
            }
            ImGui.SameLine();
        }
        if(SchedulerMain.CharacterPostProcessLocked && ShouldDisplay())
        {
            displayed = true;
            if(ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "processing.png"), out var t))
            {
                ImGui.Image(t.Handle, StatusPanelSize);
                if(ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    if(ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        Svc.Commands.ProcessCommand("/ays");
                    }
                    if(ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    {
                        SchedulerMain.CharacterPostProcessLocked = false;
                    }
                    ImGui.SetTooltip("AutoRetainer 正在进行后处理阶段\n左键点击 - 开启 AutoRetainer\n右键点击 - 中止");
                }
            }
            else
            {
                ImGuiEx.Text("正在加载界面…");
            }
            ImGui.SameLine();
        }
        if(P.TaskManager.IsBusy && ShouldDisplay())
        {
            displayed = true;
            if(ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "processing.png"), out var t))
            {
                ImGui.Image(t.Handle, StatusPanelSize);
                if(ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    if(ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        Svc.Commands.ProcessCommand("/ays");
                    }
                    if(ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    {
                        P.TaskManager.Abort();
                    }
                    ImGui.SetTooltip("AutoRetainer 正在处理任务中\n左键点击 - 开启 AutoRetainer\n右键点击 - 中止");
                }
            }
            else
            {
                ImGuiEx.Text("正在加载界面…");
            }
            ImGui.SameLine();
        }
        if(P.IsNextToBell && ShouldDisplay())
        {
            displayed = true;
            if(ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "bellalert.png"), out var t))
            {
                ImGui.Image(t.Handle, StatusPanelSize);
                if(ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    if(ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        Svc.Commands.ProcessCommand("/ays");
                    }
                    ImGui.SetTooltip("传唤铃感应已启用\n左键点击 - 开启 AutoRetainer");
                }
                var f = (float)(Environment.TickCount64 - P.LastMovementAt) / (float)C.RetainerSenseThreshold;
                ImGui.ProgressBar(f, new(128, 10), "");
            }
            else
            {
                ImGuiEx.Text("正在加载界面…");
            }
            ImGui.SameLine();
        }
        if(MultiMode.Enabled && ShouldDisplay())
        {
            displayed = true;
            if(ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "multi.png"), out var t))
            {
                ImGui.Image(t.Handle, StatusPanelSize);
                if(ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    if(ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        Svc.Commands.ProcessCommand("/ays");
                    }
                    if(ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    {
                        MultiMode.Enabled = false;
                        MultiMode.SingleMultiMode = null;
                    }
                    ImGui.SetTooltip("多角色模式已启用\n左键点击 - 开启 AutoRetainer\n右键点击 - 禁用多角色模式。");
                }
            }
            else
            {
                ImGuiEx.Text("正在加载界面…");
            }
            ImGui.SameLine();
        }
        if(MultiMode.Enabled && MultiMode.SingleMultiMode != null && ShouldDisplay())
        {
            displayed = true;
            if(ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "single.png"), out var t))
            {
                ImGui.Image(t.Handle, StatusPanelSize);
                if(ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    if(ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        Svc.Commands.ProcessCommand("/ays");
                    }
                    if(ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    {
                        MultiMode.SingleMultiMode = null;
                    }
                    ImGui.SetTooltip($"单轮多角色模式已启用。\n待处理角色：\n{MultiMode.SingleMultiMode?.Select(x => $"- {Censor.Character(x)}").Print("\n")}\n\n左键点击 - 打开 AutoRetainer\n右键点击 - 转为常规多角色模式");
                }
            }
            else
            {
                ImGuiEx.Text("正在加载界面…");
            }
            ImGui.SameLine();
        }
        if(C.NightMode && MultiMode.Enabled && ShouldDisplay())
        {
            displayed = true;
            if(ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "Night.png"), out var t))
            {
                ImGui.Image(t.Handle, StatusPanelSize);
                if(ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    if(ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        Svc.Commands.ProcessCommand("/ays");
                    }
                    if(ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    {
                        C.NightMode = false;
                        MultiMode.BailoutNightMode();
                    }
                    ImGui.SetTooltip("夜间模式已启用。\n左键点击 - 打开 AutoRetainer\n右键点击 - 禁用夜间模式");
                }
            }
            else
            {
                ImGuiEx.Text("正在加载界面…");
            }
            ImGui.SameLine();
        }
        if(VoyageScheduler.Enabled && ShouldDisplay())
        {
            displayed = true;
            if(ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "submarine.png"), out var t))
            {
                ImGui.Image(t.Handle, StatusPanelSize);
                if(ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    if(ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        Svc.Commands.ProcessCommand("/ays");
                    }
                    if(ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    {
                        VoyageScheduler.Enabled = false;
                    }
                    ImGui.SetTooltip("潜水艇模块已启用\n左键点击 - 开启 AutoRetainer\n右键点击 - 禁用潜水艇模块");
                }
            }
            else
            {
                ImGuiEx.Text("正在加载界面…");
            }
            ImGui.SameLine();
        }
        if(SchedulerMain.PluginEnabled && ShouldDisplay())
        {
            displayed = true;
            if(ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", Utils.GetReachableRetainerBell(false) == null ? "bellcrossed.png" : "bell.png"), out var t))
            {
                ImGui.Image(t.Handle, StatusPanelSize);
                if(ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    if(ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        Svc.Commands.ProcessCommand("/ays");
                    }
                    if(ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    {
                        SchedulerMain.DisablePlugin();
                    }
                    ImGui.SetTooltip("AutoRetainer 已启用\n左键点击 - 开启 AutoRetainer\n右键点击 - 禁用 AutoRetainer");
                }
            }
            else
            {
                ImGuiEx.Text("正在加载界面…");
            }
            ImGui.SameLine();
        }
        if(DisplayNotify && ShouldDisplay())
        {
            displayed = true;
            if(ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "notify.png"), out var t))
            {
                ImGui.Image(t.Handle, StatusPanelSize);
                if(ImGui.IsItemHovered())
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                    if(ImGui.IsItemClicked(ImGuiMouseButton.Left))
                    {
                        NotificationHandler.IsHidden = true;
                        Svc.Commands.ProcessCommand("/ays");
                    }
                    if(ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    {
                        NotificationHandler.IsHidden = true;
                    }
                    ImGui.SetTooltip("部分雇员已完成探险任务。\n左键点击 - 开启 AutoRetainer\n右键点击 - 关闭提示");
                }
            }
            else
            {
                ImGuiEx.Text("正在加载界面…");
            }
            ImGui.SameLine();
        }
        ImGui.Dummy(Vector2.One);
        if(Data != null && !C.OldStatusIcons)
        {
            ImGuiEx.LineCentered("状态", delegate
            {
                if(C.MultiModeWorkshopConfiguration.MultiWaitForAll)
                {
                    if(ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "wait.png"), out var t))
                    {
                        ImGui.Image(t.Handle, StatusPanelSize / 2);
                        ImGuiEx.Tooltip("已全局启用等待所有远航探索功能");
                    }
                    else
                    {
                        ImGuiEx.Text("正在加载界面…");
                    }
                    ImGui.SameLine();
                }
                else if(Data.MultiWaitForAllDeployables)
                {
                    if(ThreadLoadImageHandler.TryGetTextureWrap(Path.Combine(Svc.PluginInterface.AssemblyLocation.DirectoryName, "res", "wait.png"), out var t))
                    {
                        ImGui.Image(t.Handle, StatusPanelSize / 2);
                        ImGuiEx.Tooltip("已为此角色启用等待所有远航探索功能");
                    }
                    else
                    {
                        ImGuiEx.Text("正在加载界面…");
                    }
                    ImGui.SameLine();
                }
            });
        }

        Position = new(ImGuiHelpers.MainViewport.Size.X / 2 - ImGui.GetWindowSize().X / 2, 20);
    }
}
