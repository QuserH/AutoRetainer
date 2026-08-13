using AutoRetainer.Modules.Voyage;
using AutoRetainer.UI.MainWindow.MultiModeTab;
using AutoRetainerAPI;
using AutoRetainerAPI.Configuration;
using Dalamud.Interface.Components;
using ECommons.Configuration;
using ECommons.Funding;
using NightmareUI;

namespace AutoRetainer.UI.MainWindow;

internal unsafe class AutoRetainerWindow : Window
{
    private TitleBarButton LockButton;

    public AutoRetainerWindow() : base($"")
    {
        PatreonBanner.IsOfficialPlugin = () => true;
        LockButton = new()
        {
            Click = OnLockButtonClick,
            Icon = C.PinWindow ? FontAwesomeIcon.Lock : FontAwesomeIcon.LockOpen,
            IconOffset = new(3, 2),
            ShowTooltip = () => ImGui.SetTooltip("锁定窗口位置和大小"),
        };
        SizeConstraints = new()
        {
            MinimumSize = new(250, 100),
            MaximumSize = new(9999, 9999)
        };
        P.WindowSystem.AddWindow(this);
        AllowPinning = false;
        TitleBarButtons.Add(new()
        {
            Click = (m) => { if(m == ImGuiMouseButton.Left) S.NeoWindow.IsOpen = true; },
            Icon = FontAwesomeIcon.Cog,
            IconOffset = new(2, 2),
            ShowTooltip = () => ImGui.SetTooltip("开启设置窗口"),
        });
        TitleBarButtons.Add(LockButton);
    }

    private Action<string> SomeAction;

    private void OnLockButtonClick(ImGuiMouseButton m)
    {
        SomeAction += (s) => { };
        SomeAction -= (s) => { };
        if(m == ImGuiMouseButton.Left)
        {
            C.PinWindow = !C.PinWindow;
            LockButton.Icon = C.PinWindow ? FontAwesomeIcon.Lock : FontAwesomeIcon.LockOpen;
        }
    }

    public override void PreDraw()
    {
        var prefix = SchedulerMain.PluginEnabled ? $" [{SchedulerMain.Reason}]" : "";
        var tokenRem = TimeSpan.FromMilliseconds(Utils.GetRemainingSessionMiliSeconds());
        WindowName = $"{P.Name} {P.GetType().Assembly.GetName().Version}{prefix} | {FormatToken(tokenRem)}###AutoRetainer";
        if(C.PinWindow)
        {
            ImGuiHelpers.SetNextWindowPosRelativeMainViewport(C.WindowPos);
            ImGui.SetNextWindowSize(C.WindowSize);
        }
    }

    private string FormatToken(TimeSpan time)
    {
        if(time.TotalMilliseconds > 0)
        {
            if(time.Days > 0)
            {
                return $"会话将在 {time.Days} 天后过期" + (time.Hours > 0 ? $"（还有 {time.Hours} 小时）" : "");
            }
            else
            {
                if(time.Hours > 0)
                {
                    return $"会话将在 {time.Hours} 小时后过期";
                }
                else
                {
                    return $"会话将在不到一小时后过期";
                }
            }
        }
        else
        {
            return "会话已过期";
        }
    }
    public override void Draw()
    {
        //ImGuiEx.Text(GradientColor.Get(EColor.RedBright, EColor.YellowBright), "This version MUST NOT BE RUNNING UNATTENDED.");
        try
        {
            if(!C.AcceptedDisclamer)
            {
                new NuiBuilder()
                    .Section("免责声明")
                    .TextWrapped(ImGuiColors.DalamudYellow, "请注意，严禁将 AutoRetainer 用于 RMT 目的")
                    .TextWrapped(ImGuiColors.DalamudRed, "为避免不必要的后果，使用 AutoRetainer 时请遵守以下规则：")
                    .TextWrapped("1. 不要在游戏聊天中提及你正在使用 AutoRetainer；")
                    .TextWrapped("2. 请勿长时间在无人看管的情况下执行 AutoRetainer；")
                    .TextWrapped("3. 每天实际运行游戏和 AutoRetainer 的时间不要超过 16 小时，并在雇员或潜水艇处理流程之间留出空闲时间；")
                    .TextWrapped("4. 不要回应通过交易或聊天进行所谓“机器人检测”的玩家；请立即将其加入黑名单；")
                    .TextWrapped("5. 如果GM询问，始终声称所有操作都是手动完成的，绝不承认使用插件。")
                    .TextWrapped("违反这些规则可能导致你的账号受到处罚。")
                    .TextWrapped(GradientColor.Get(ImGuiColors.DalamudYellow, ImGuiColors.DalamudRed), "不得将 AutoRetainer 用于 RMT 或其他商业用途。用于上述用途时，我们不会提供任何支持。")
                    .Widget(() =>
                    {
                        if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.Check, "接受并继续"))
                        {
                            C.AcceptedDisclamer = true;
                            EzConfig.Save();
                        }
                    })
                    .Draw();
                return;
            }
            var e = SchedulerMain.PluginEnabledInternal;
            var disabled = MultiMode.Active && !ImGui.GetIO().KeyCtrl;

            if(disabled)
            {
                ImGui.BeginDisabled();
            }
            if(ImGui.Checkbox($"启用 {P.Name}", ref e))
            {
                P.WasEnabled = false;
                if(e)
                {
                    SchedulerMain.EnablePlugin(PluginEnableReason.Auto);
                }
                else
                {
                    SchedulerMain.DisablePlugin();
                }
            }
            if(C.ShowDeployables && (VoyageUtils.Workshops.Contains(Svc.ClientState.TerritoryType) || VoyageScheduler.Enabled))
            {
                ImGui.SameLine();
                ImGui.Checkbox($"远航探索", ref VoyageScheduler.Enabled);
            }
            if(disabled)
            {
                ImGui.EndDisabled();
                ImGuiComponents.HelpMarker("此选项由多角色模式控制。按住 Ctrl 可强制覆盖。");
            }

            if(P.WasEnabled)
            {
                ImGui.SameLine();
                ImGuiEx.Text(GradientColor.Get(ImGuiColors.DalamudGrey, ImGuiColors.DalamudGrey3, 500), $"已暂停");
            }

            ImGui.SameLine();
            if(ImGui.Checkbox("多角色", ref MultiMode.Enabled))
            {
                MultiMode.OnMultiModeEnabled();
            }
            Utils.DrawLifestreamAvailabilityIndicator();
            if(C.ShowNightMode)
            {
                ImGui.SameLine();
                if(ImGui.Checkbox("夜间", ref C.NightMode))
                {
                    MultiMode.BailoutNightMode();
                }
            }
            if(C.DisplayMMType)
            {
                ImGui.SameLine();
                ImGuiEx.SetNextItemWidthScaled(100f);
                ImGuiEx.EnumCombo("##mode", ref C.MultiModeType, null, Lang.MultiModeTypeNames);
            }
            if(C.CharEqualize && MultiMode.Enabled)
            {
                ImGui.SameLine();
                if(ImGui.Button("重设计数器"))
                {
                    MultiMode.CharaCnt.Clear();
                }
            }

            Svc.PluginInterface.GetIpcProvider<object>(ApiConsts.OnMainControlsDraw).SendMessage();

            if(IPC.Suppressed)
            {
                ImGuiEx.Text(ImGuiColors.DalamudRed, $"插件操作正被其他插件抑制中");
                ImGui.SameLine();
                if(ImGui.SmallButton("取消"))
                {
                    IPC.Suppressed = false;
                }
            }

            if(P.TaskManager.IsBusy)
            {
                ImGui.SameLine();
                if(ImGui.Button($"中止 {P.TaskManager.NumQueuedTasks} 个任务"))
                {
                    P.TaskManager.Abort();
                }
            }

            PatreonBanner.DrawRight();
            ImGuiEx.EzTabBar("tabbar", PatreonBanner.Text,
                            ("雇员管理", MultiModeUI.Draw, null, true),
                            ("远航探索", WorkshopUI.Draw, null, true),
                            ("故障排除", TroubleshootingUI.Draw, null, true),
                            ("统计信息", DrawStats, null, true),
                            ("关于", CustomAboutTab.Draw, null, true)
                            );
            if(!C.PinWindow)
            {
                C.WindowPos = ImGui.GetWindowPos();
                C.WindowSize = ImGui.GetWindowSize();
            }
        }
        catch(Exception e)
        {
            ImGuiEx.TextWrapped(e.ToStringFull());
        }
    }

    private void DrawStats()
    {
            NuiTools.ButtonTabs([[C.RecordStats ? new("雇员派遣", S.VentureStats.DrawVentures) : null, new("金币", S.GilDisplay.Draw), new("部队信息", S.FCData.Draw)]]);
    }

    public override void OnClose()
    {
        EzConfig.Save();
        S.VentureStats.Data.Clear();
        MultiModeUI.JustRelogged = false;
    }

    public override void OnOpen()
    {
        MultiModeUI.JustRelogged = true;
    }
}
