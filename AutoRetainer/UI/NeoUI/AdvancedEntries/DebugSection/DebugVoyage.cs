using AutoRetainer.Internal;
using AutoRetainer.Modules.Voyage;
using AutoRetainer.Modules.Voyage.Tasks;
using AutoRetainer.Modules.Voyage.VoyageCalculator;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Diagnostics;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;

internal unsafe class DebugVoyage : DebugSectionBase
{
    private static string data1 = "";
    private static VoyageType data2 = default;
    private static int r1, r2, r3, r4, r5 = -1;
    public override void Draw()
    {
        if(ImGui.CollapsingHeader("Debug"))
        {
            try
            {
                var h = HousingManager.Instance()->WorkshopTerritory;
                if(h != null)
                {
                    foreach(var x in h->Submersible.DataPointers)
                    {
                        if(x.Value != null)
                        {
                            ImGuiEx.Text($"{x.Value->Name.Read()}/{x.Value->ReturnTime}/{x.Value->CurrentExplorationPoints.ToArray().Print()}");
                        }
                        else
                        {
                            ImGuiEx.Text("Null");
                        }
                    }
                }
                if(ImGui.Button("清除离线数据"))
                {
                    Data.OfflineAirshipData.Clear();
                    Data.OfflineSubmarineData.Clear();
                }
                if(ImGui.Button("修理 1")) VoyageScheduler.TryRepair(0);
                if(ImGui.Button("修理 2")) VoyageScheduler.TryRepair(1);
                if(ImGui.Button("修理 3")) VoyageScheduler.TryRepair(2);
                if(ImGui.Button("修理 4")) VoyageScheduler.TryRepair(3);
                if(ImGui.Button("关闭维修界面")) VoyageScheduler.CloseRepair();
                //if (ImGui.Button("Trigger auto repair")) TaskRepairAll.EnqueueImmediate();
                ImGui.InputText("data1", ref data1, 50);
                ImGuiEx.EnumCombo("data2", ref data2);
                if(CurrentSubmarine.Get() != null)
                {
                    ImGuiEx.Text($"{CurrentSubmarine.Get()->CurrentExp}/{CurrentSubmarine.Get()->NextLevelExp}");
                }
                ImGuiEx.Text($"Is voyage panel: {VoyageUtils.IsInVoyagePanel()}, {Lang.PanelName}");
                if(ImGui.Button("IsVesselNeedsRepair"))
                {
                    try
                    {
                        DuoLog.Information($"{VoyageUtils.GetIsVesselNeedsRepair(data1, data2, out var log).Print()}\n{log.Join("\n")}");
                    }
                    catch(Exception e)
                    {
                        e.LogDuo();
                    }
                }
                if(ImGui.Button("GetSubmarineIndexByName"))
                {
                    try
                    {
                        DuoLog.Information($"{VoyageUtils.GetVesselIndexByName(data1, VoyageType.Submersible)}");
                    }
                    catch(Exception e)
                    {
                        e.LogDuo();
                    }
                }
                ImGuiEx.Text($"Bell: {Utils.GetReachableRetainerBell(false)}");
                ImGuiEx.Text($"Bell(true): {Utils.GetReachableRetainerBell(true)}");
                ImGuiEx.TextWrapped($"Enabled subs: {Data.GetVesselData(VoyageType.Submersible).Select(x => $"{x.Name}, {x.GetRemainingSeconds()}").Print()}");
                ImGuiEx.Text($"AnyEnabledVesselsAvailable: {Data.AnyEnabledVesselsAvailable()}");
                ImGuiEx.Text($"Panel type: {VoyageUtils.GetCurrentWorkshopPanelType()}");
                if(TryGetAddonByName<AtkUnitBase>("AirShipExplorationResult", out var addon) && IsAddonReady(addon))
                {
                    var button = addon->UldManager.NodeList[3]->GetAsAtkComponentButton();
                    ImGuiEx.Text($"Button: {button->IsEnabled}");
                }
                if(ImGui.Button("与最近的界面互动"))
                {
                    TaskInteractWithNearestPanel.Enqueue();
                }
            }
            catch(Exception e)
            {
                ImGuiEx.TextWrapped(e.ToString());
            }
        }
        ImGuiEx.Text($"IsRetainerBlockedByVoyage: {VoyageUtils.IsRetainerBlockedByVoyage()}");
        if(ImGui.CollapsingHeader("data"))
        {
            try
            {
                ImGuiEx.Text($"Curnet: {(nint)CurrentSubmarine.Get()}");
                if(CurrentSubmarine.Get() != null)
                {
                    ImGuiEx.Text($"Name: {CurrentSubmarine.Get()->Name.Read()}");
                    ImGuiEx.Text($"Hull: {CurrentSubmarine.Get()->HullId}");
                    ImGuiEx.Text($"->SternId: {CurrentSubmarine.Get()->SternId}");
                    ImGuiEx.Text($"BridgeId: {CurrentSubmarine.Get()->BridgeId}");
                    ImGuiEx.Text($"BowId: {CurrentSubmarine.Get()->BowId}");
                    ImGuiEx.Text($"RankId: {CurrentSubmarine.Get()->RankId}");
                    if(ImGui.Button("打印最佳经验"))
                    {
                        CurrentSubmarine.GetBestExps();
                    }
                    if(ImGui.Button("选择最佳航线"))
                    {
                        TaskCalculateAndPickBestExpRoute.Enqueue();
                    }
                    ImGuiEx.Text($"点数: {CurrentSubmarine.Get()->CurrentExplorationPoints.ToArray().Print()}");
                    ImGuiEx.Text($"点数: {CurrentSubmarine.Get()->CurrentExplorationPoints.ToArray().Select(x => VoyageUtils.GetSubmarineExplorationName(x)).Print()}");
                }
            }
            catch(Exception e)
            {
                ImGuiEx.TextWrapped(e.ToString());
            }
            var curPlotId = (long*)(Process.GetCurrentProcess().MainModule.BaseAddress + 0x215FB68);
            ImGuiEx.TextCopy($"Plot ID: {*curPlotId:X16}");
            ImGuiEx.Text($"HID: {HousingManager.Instance()->GetCurrentIndoorHouseId()}");
            if(HousingManager.Instance()->WorkshopTerritory != null)
            {
                ImGuiEx.Text($"Num air: {HousingManager.Instance()->WorkshopTerritory->Airship.AirshipCount}");
                //ImGuiEx.Text($"Num w: {HousingManager.Instance()->WorkshopTerritory->Submersible.DataList}");
                {
                    var data = HousingManager.Instance()->WorkshopTerritory->Airship.Data;
                    for(var i = 0; i < data.Length; i++)
                    {
                        var d = data[i];
                        ImGuiEx.Text($"Air: {d.Name.Read()}, returns at {d.GetReturnTime()}, current: {d.CurrentExp}");
                    }
                }
                {
                    var data = HousingManager.Instance()->WorkshopTerritory->Submersible.DataPointers;
                    for(var i = 0; i < data.Length; i++)
                    {
                        var d = data[i];
                        ImGuiEx.Text($"Sub: {d.Value->Name.Read()}, returns at {d.Value->GetReturnTime()}, current: {d.Value->CurrentExp}");
                    }
                }
            }
        }
        if(ImGui.CollapsingHeader("工具"))
        {
            ImGui.InputInt("r1", ref r1);
            if(ImGui.Button("选择"))
            {
                P.Memory.SelectRoutePointUnsafe(r1);
            }
        }
        if(ImGui.CollapsingHeader("控制"))
        {
            if(ImGui.Button($"{nameof(VoyageScheduler.Lockon)}")) DuoLog.Information($"{VoyageScheduler.Lockon()}");
            if(ImGui.Button($"{nameof(VoyageScheduler.Approach)}")) DuoLog.Information($"{VoyageScheduler.Approach()}");
            if(ImGui.Button($"{nameof(VoyageScheduler.AutomoveOffPanel)}")) DuoLog.Information($"{VoyageScheduler.AutomoveOffPanel()}");
            if(ImGui.Button($"{nameof(VoyageScheduler.InteractWithVoyagePanel)}")) DuoLog.Information($"{VoyageScheduler.InteractWithVoyagePanel()}");
            if(ImGui.Button($"{nameof(VoyageScheduler.SelectAirshipManagement)}")) DuoLog.Information($"{VoyageScheduler.SelectAirshipManagement()}");
            if(ImGui.Button($"{nameof(VoyageScheduler.SelectSubManagement)}")) DuoLog.Information($"{VoyageScheduler.SelectSubManagement()}");
            ImGui.InputText("主题名称", ref data1, 100);
            if(ImGui.Button($"{nameof(VoyageScheduler.SelectVesselByName)}")) DuoLog.Information($"{VoyageScheduler.SelectVesselByName(data1, VoyageType.Submersible)}");
            if(ImGui.Button($"{nameof(VoyageScheduler.RedeployVessel)}")) DuoLog.Information($"{VoyageScheduler.RedeployVessel()}");
            if(ImGui.Button($"{nameof(VoyageScheduler.DeployVessel)}")) DuoLog.Information($"{VoyageScheduler.DeployVessel()}");
            if(ImGui.Button($"{nameof(TaskDeployOnBestExpVoyage.Deploy)}")) DuoLog.Information($"{TaskDeployOnBestExpVoyage.Deploy()}");
            //if (ImGui.Button($"{nameof(TaskDeployOnBestExpVoyage)}")) TaskDeployOnBestExpVoyage.Enqueue();
            if(ImGui.Button($"{nameof(VoyageScheduler.Approach)}")) DuoLog.Information($"{VoyageScheduler.Approach}");
        }
        if(ImGui.CollapsingHeader("测试任务管理器"))
        {
            if(ImGui.Button("测试重新部署飞空艇"))
            {
                P.TaskManager.Enqueue(VoyageScheduler.Lockon);
                P.TaskManager.Enqueue(VoyageScheduler.Approach);
                P.TaskManager.Enqueue(VoyageScheduler.AutomoveOffPanel);
                P.TaskManager.Enqueue(VoyageScheduler.InteractWithVoyagePanel);
                P.TaskManager.Enqueue(VoyageScheduler.SelectAirshipManagement);
                P.TaskManager.Enqueue(() => VoyageScheduler.SelectVesselByName(data1, VoyageType.Airship));
                P.TaskManager.Enqueue(VoyageScheduler.WaitUntilFinalizeDeployAddonExists);
                P.TaskManager.Enqueue(VoyageScheduler.RedeployVessel);
                P.TaskManager.Enqueue(VoyageScheduler.DeployVessel);
            }
        }
    }
}
