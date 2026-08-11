using AutoRetainer.Modules.Voyage;
using AutoRetainer.Scheduler.Tasks;
using Dalamud.Utility;
using ECommons.Configuration;
using ECommons.Events;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.Interop;
using ECommons.IPC;
using ECommons.MathHelpers;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using ItemLevel = AutoRetainer.Helpers.ItemLevel;

namespace AutoRetainer.UI.NeoUI.AdvancedEntries.DebugSection;

internal unsafe class DebugMisc : DebugSectionBase
{
    public override void Draw()
    {
        if(ImGui.Button("EnableSingleMultiMode")) ECommonsIPC.AutoRetainer.EnableSingleMultiMode(null);
        if(ImGui.Button("EnableSingleMultiMode (subs)")) ECommonsIPC.AutoRetainer.EnableSingleMultiMode(ECommons.IPC.Subscribers.AutoRetainer.MultiModeType.Submersibles);
        if(ImGui.CollapsingHeader("移动卡死检测"))
        {
            ImGuiEx.Text($"""
                
                LastRefreshTime {BailoutManager.LastRefreshTime} ({Environment.TickCount64 - BailoutManager.LastRefreshTime})
                LastPosition {BailoutManager.LastPosition}
                ExtendIsMoving {EzThrottler.GetRemainingTime("ExtendIsMoving")}
                """);
        }
        if(ImGui.CollapsingHeader("AddonOpenedAt"))
        {
            foreach(var x in BailoutManager.AddonOpenedAt.Keys)
            {
                ImGuiEx.Text($"{x}: {BailoutManager.AddonOpenedAt[x]} ({Environment.TickCount64 - BailoutManager.AddonOpenedAt[x]} ago)");
                ImGui.SameLine();
                if(ImGui.SmallButton($"-1 min##{x}"))
                {
                    BailoutManager.AddonOpenedAt[x] -= 60 * 1000;
                }
            }
        }
        ImGui.Checkbox("渲染禁用详细记录", ref RenderDisableManager.Debug);
        if(ImGui.Button("执行外挂终止器"))
        {
            S.PluginTerminator.OnUpdate();
        }
        ImGuiEx.Text($"OfflineSubmarineData.Count {Data.OfflineSubmarineData.Count}, NumSubSlots {Data.NumSubSlots}");
        ImGuiEx.Text($"FC Rank: {Utils.FCRank}");
        if(ImGui.CollapsingHeader("ApiTest1"))
        {
            try
            {
                ImGuiEx.Text($"{P.API.Config.FCData}");
            }
            catch(Exception e)
            {
                ImGuiEx.Text($"{e}");
            }
        }
        if(ImGuiEx.Button("Telelport"))
        {
            MultiMode.RunTeleportLogic();
        }
        if(ImGui.CollapsingHeader("AskEligibility"))
        {
            ImGuiEx.Text($"""
                Current character: 
                SentVentures: {Data?.SentVenturesByDay.Sum(x => x.Value)}
                SentVoyages: {Data?.SentVoyagesByDay.Sum(x => x.Value)}
                Max enabled retainers: {Data?.GetEnabledRetainers(false).Length}
                SentVentures all: {C.OfflineData.Sum(x => x.SentVenturesByDay.Select(x => x.Value).Sum())}
                SentVoyages all: {C.OfflineData.Sum(x => x.SentVoyagesByDay.Select(x => x.Value).Sum())}
                Max enabled retainers global: {C.OfflineData.Select(x => x.GetEnabledRetainers().Length).MaxSafe()}
                Characters with enabled retainers: {C.OfflineData.Where(x => x.GetEnabledRetainers().Length > 0 && x.Enabled).Count()}
                Characters with enabled submarines: {C.OfflineData.Where(x => x.GetEnabledVesselsData(Internal.VoyageType.Submersible).Count > 0 && x.WorkshopEnabled).Count()}
                ---------
                By day:
                """);
            var days = C.OfflineData.Select(x => (long[])[..x.SentVenturesByDay.Keys, ..x.SentVoyagesByDay.Keys]).SelectNested(x => x).ToHashSet();
            ImGui.Indent();
            foreach(var x in days)
            {
                ImGuiEx.Text($"{x}: SentVentures: {C.OfflineData.Select(c => c.SentVenturesByDay.SafeSelect(x)).Sum()},  SentVoyages: {C.OfflineData.Select(c => c.SentVoyagesByDay.SafeSelect(x)).Sum()}");
            }
            ImGui.Unindent();
            ImGuiEx.Text($"""
                ---------
                按角色统计:
                """);
            foreach(var x in C.OfflineData)
            {
                ImGuiEx.Text($"{x.NameWithWorld}: SentVentures: {x.SentVenturesByDay.Sum(s => s.Value)}, SentVoyages: {x.SentVoyagesByDay.Sum(s => s.Value)}");
            }
        }
        if(ImGui.CollapsingHeader("FreeCompanyAction"))
        {
            ImGuiEx.Text($"Num: {TaskActivateSealSweetener.NumActions}");
            foreach(var x in TaskActivateSealSweetener.Actions)
            {
                ImGuiEx.Text($"{x} / {Svc.Data.GetExcelSheet<CompanyAction>().GetRowOrDefault((uint)x)?.Name}");
            }
            ImGuiEx.FilteringInputInt("Callback value 1", out var val1);
            ImGuiEx.FilteringInputInt("Callback value 2", out var val2);
            if(ImGui.Button("On FreeCompany"))
            {
                if(TryGetAddonByName<AtkUnitBase>("FreeCompany", out var addon) && addon->IsReady())
                {
                    Callback.Fire(addon, true, val1, (uint)val2);
                }
            }
            if(ImGui.Button("On FreeCompanyAction"))
            {
                if(TryGetAddonByName<AtkUnitBase>("FreeCompanyAction", out var addon) && addon->IsReady())
                {
                    Callback.Fire(addon, true, val1, (uint)val2);
                }
            }
            if(ImGui.Button("TaskActivateSealSweetener.Enqueue"))
            {
                TaskActivateSealSweetener.Enqueue();
            }
            if(ImGui.Button("TaskActivateSealSweetener.EnqueueThrottled"))
            {
                TaskActivateSealSweetener.EnqueueThrottled();
            }
        }
        if(ImGui.CollapsingHeader("618"))
        {
            var a = Svc.Data.GetExcelSheet<Lobby>().GetRow(618).Text.ToDalamudString();
            foreach(var pl in a.Payloads)
            {
                ImGuiEx.Text($"{pl.Type}: {pl.ToString()}");
            }
        }
        if(ImGui.CollapsingHeader("CMenu"))
        {
            if(TryGetAddonMaster<AddonMaster.ContextMenu>(out var m) && m.IsAddonReady)
            {
                foreach(var x in m.Entries)
                {
                    ImGuiEx.Text($"{x.Text}/{x.Enabled}");
                }
            }
        }
        if(ImGui.CollapsingHeader("雇员物品属性"))
        {
            var im = InventoryManager.Instance();
            var c = im->GetInventoryContainer(InventoryType.RetainerEquippedItems);
            for(var i = 0; i < c->Size; i++)
            {
                var slot = c->GetInventorySlot(i);
                ImGuiEx.Text($"{i} ({slot->GetItemId()}): {ExcelItemHelper.GetName(slot->GetItemId() % 1000000)}, gathering: {slot->GetStat(BaseParamEnum.Gathering)} [{slot->GetStatCap(BaseParamEnum.Gathering)}], perception: {slot->GetStat(BaseParamEnum.Perception)} [{slot->GetStatCap(BaseParamEnum.Perception)}]");
            }
        }
        if(ImGui.Button("测试 HaselTweaks"))
        {
            Utils.EnsureEnhancedLoginIsOff();
        }
        if(ImGui.Button("通过外部进程写入配置"))
        {
            ExternalWriter.PlaceWriteOrder(new(System.IO.Path.Combine(Svc.PluginInterface.ConfigDirectory.FullName, "WriterTest.json"), EzConfig.DefaultSerializationFactory.Serialize(C, true)));
        }
        ImGuiEx.Text($"FC points: {Utils.FCPoints}");
        if(ImGui.CollapsingHeader("房屋"))
        {
            var h = HousingManager.Instance();
            ImGuiEx.Text($"GetCurrentDivision {h->GetCurrentDivision()}");
            ImGuiEx.Text($"GetCurrentHouseId {h->GetCurrentIndoorHouseId()}");
            ImGuiEx.Text($"GetCurrentPlot {h->GetCurrentPlot()}");
            ImGuiEx.Text($"GetCurrentRoom {h->GetCurrentRoom()}");
            ImGuiEx.Text($"GetCurrentWard {h->GetCurrentWard()}");
            if(ImGui.Button("模拟登录"))
            {
                ProperOnLogin.FireArtificially();
            }
            if(h->OutdoorTerritory != null)
            {
                for(var i = 0; i < 30; i++)
                {
                    ImGuiEx.Text($"IsEstateResident {i}: {P.Memory.OutdoorTerritory_IsEstateResident((nint)h->OutdoorTerritory, (byte)i)}");
                }
            }
        }
        if(ImGui.Button("安装回调钩子")) Callback.InstallHook();
        if(ImGui.Button("禁用回调钩子")) Callback.UninstallHook();
        ImGuiEx.TextCopy($"{(nint)(&TargetSystem.Instance()->Target):X16}");
        ImGui.Checkbox($"记录操作码", ref P.LogOpcodes);
        ImGuiEx.Text($"CSFramework.Instance()->FrameCounter: {CSFramework.Instance()->FrameCounter}");
        if(ImGui.Button("测试重复存放"))
        {
            if(TryGetAddonByName<AtkUnitBase>("RetainerItemTransferList", out var addon))
            {
                Callback.Fire(addon, true, 0, (uint)29);
            }
        }
        ImGuiEx.Text($"Lockon: {*(byte*)((nint)TargetSystem.Instance() + 309)}");
        if(ImGui.Button("解除帧率锁定"))
        {
            FPSManager.LockChillFrames();
        }
        if(ImGui.Button("启用帧率锁定"))
        {
            FPSManager.UnlockChillFrames();
        }
        ImGui.Separator();
        ImGuiEx.Text($"CSFramework.Instance()->WindowInactive: {CSFramework.Instance()->WindowInactive}");
        ImGuiEx.Text($"IsKeyPressed(C.TempCollectB): {IsKeyPressed(C.TempCollectB)}");
        ImGuiEx.Text($"Bitmask.IsBitSet(User32.GetKeyState((int)C.TempCollectB), 15): {Bitmask.IsBitSet(TerraFX.Interop.Windows.Windows.GetKeyState((int)C.TempCollectB), 15)}");
        ImGuiEx.Text($"DontReassign: {C.DontReassign}, key {C.TempCollectB}/{(int)C.TempCollectB}");
        foreach(var x in C.OfflineData)
        {
            ImGuiEx.Text($"{x.Name}@{x.World}: {x.Gil + x.RetainerData.Sum(z => z.Gil)}");
        }
        var ocd = Data;
        if(ocd != null)
        {
            ImGuiEx.Text($"等级数组:");
            ImGuiEx.Text(ocd.ClassJobLevelArray.Print());
        }

        ImGuiEx.Text($"{Utils.TryGetCurrentRetainer(out var n)}/{n}");
        ImGuiEx.Text($"{ItemLevel.Calculate(out var g, out var p)}/{g}/{p}");
        if(ImGui.Button("重新生成屏蔽种子"))
        {
            C.CensorSeed = Guid.NewGuid().ToString();
        }
        var inv = Utils.GetActiveRetainerInventoryName();
        ImGuiEx.Text($"Utils.GetActiveRetainerInventoryName(): {inv.Name} {inv.EntrustDuplicatesIndex}");
        ImGuiEx.Text($"ConditionWasEnabled={P.ConditionWasEnabled}");
        if(ImGui.CollapsingHeader("任务调试"))
        {
            ImGuiEx.Text($"Busy: {P.TaskManager.IsBusy}, abort in {P.TaskManager.RemainingTimeMS}");
            if(ImGui.Button($"生成随机数 1/500"))
            {
                P.TaskManager.Enqueue(() => { var r = new Random().Next(0, 500); InternalLog.Verbose($"Gen 1/500: {r}"); return r == 0; });
            }
            if(ImGui.Button($"生成随机数 1/5000"))
            {
                P.TaskManager.Enqueue(() => { var r = new Random().Next(0, 5000); InternalLog.Verbose($"Gen 1/5000: {r}"); return r == 0; });
            }
            if(ImGui.Button($"生成随机数 1/100"))
            {
                P.TaskManager.Enqueue(() => { var r = new Random().Next(0, 100); InternalLog.Verbose($"Gen 1/100: {r}"); return r == 0; });
            }
        }
        ImGuiEx.Text($"QSI status: {P.quickSellItems?.openInventoryContextHook?.IsEnabled}");
        ImGuiEx.Text($"QuickSellItems.IsReadyToUse: {QuickSellItems.IsReadyToUse()}");

        foreach(var x in S.VentureStats.CharTotal)
        {
            ImGuiEx.Text($"{x.Key} : {x.Value}");
        }
        foreach(var x in S.VentureStats.RetTotal)
        {
            ImGuiEx.Text($"{x.Key} : {x.Value}");
        }

        ImGui.Separator();
        {
            if(ImGui.Button("触发") && TryGetAddonByName<AtkUnitBase>("GrandCompanySupplyList", out var addon) && IsAddonReady(addon) && addon->UldManager.NodeList[5]->IsVisible())
            {
                AutoGCHandin.InvokeHandin(addon, 0);
            }
        }

        {
            if(TryGetAddonByName<AtkUnitBase>("GrandCompanySupplyList", out var addon) && IsAddonReady(addon))
            {
                ImGuiEx.Text($"IsSelectedFilterValid: {AutoGCHandin.IsSelectedFilterValid(addon)}");
            }
        }

    }
}
