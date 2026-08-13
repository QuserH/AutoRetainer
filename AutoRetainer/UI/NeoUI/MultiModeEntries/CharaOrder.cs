using AutoRetainerAPI.Configuration;

namespace AutoRetainer.UI.NeoUI.MultiModeEntries;
public class CharaOrder : NeoUIEntry
{
    public override string Path => "多角色模式/排除与排序";

    private static string Search = "";
    private static ImGuiEx.RealtimeDragDrop<OfflineCharacterData> DragDrop = new("CharaOrder", x => x.Identity);

    public override bool NoFrame { get; set; } = true;

    public override void Draw()
    {
        C.OfflineData.RemoveAll(x => C.Blacklist.Any(z => z.CID == x.CID));
        var b = new NuiBuilder()
        .Section("角色排序")
        .Widget("在此处可对角色进行排序。这将影响多角色模式处理它们的顺序，以及它们在插件界面和登录覆盖层中的显示顺序。", (x) =>
        {
            ImGuiEx.TextWrapped($"在此处可对角色进行排序。这将影响多角色模式处理它们的顺序，以及它们在插件界面和登录覆盖层中的显示顺序。");
            ImGui.SetNextItemWidth(150f);
            ImGui.InputText($"搜索", ref Search, 50);
            DragDrop.Begin();
            if(ImGui.BeginTable("角色顺序表", 5, ImGuiTableFlags.Borders | ImGuiTableFlags.NoSavedSettings | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit))
            {
                ImGui.TableSetupColumn("##ctrl");
                ImGui.TableSetupColumn("角色", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupColumn("Functions");
                ImGui.TableSetupColumn("Exclusions");
                ImGui.TableSetupColumn("删除数据");
                ImGui.TableHeadersRow();

                for(var index = 0; index < C.OfflineData.Count; index++)
                {
                    var chr = C.OfflineData[index];
                    ImGui.PushID(chr.Identity);
                    ImGui.TableNextRow();
                    DragDrop.SetRowColor(chr.Identity);
                    ImGui.TableNextColumn();
                    DragDrop.NextRow();
                    DragDrop.DrawButtonDummy(chr, C.OfflineData, index);
                    ImGui.TableNextColumn();
                    ImGuiEx.TextV((Search != "" && ($"{chr.Name}@{chr.World}").Contains(Search, StringComparison.OrdinalIgnoreCase)) ? ImGuiColors.ParsedGreen : (Search == "" ? null : ImGuiColors.DalamudGrey3), Censor.Character(chr.Name, chr.World));

                    ImGui.TableNextColumn();

                    ImGuiEx.ButtonCheckbox(FontAwesomeIcon.GasPump, ref chr.AutoFuelPurchase, color: ImGuiColors.TankBlue);
                    ImGuiEx.Tooltip("允许此角色在工作坊购买燃料");
                    ImGuiEx.DragDropRepopulate("EnFuel", chr.AutoFuelPurchase, ref chr.AutoFuelPurchase);

                    ImGui.SameLine();

                    ImGuiEx.ButtonCheckbox(FontAwesomeIcon.BuildingFlag, ref chr.NoFcBuffUse, color: !C.FullAutoGCDeliveryUseBuffFCAction ? ImGuiColors.DalamudRed : ImGuiColors.TankBlue, inverted: true);
                    ImGuiEx.Tooltip("允许此角色使用部队增益");
                    if(!C.FullAutoGCDeliveryUseBuffFCAction) ImGuiEx.Tooltip(EColor.RedBright, $"你必须同时在全局设置中启用此功能，它才能生效。");
                    ImGuiEx.DragDropRepopulate("EnFcBuf", chr.NoFcBuffUse, ref chr.NoFcBuffUse);

                    ImGui.SameLine();

                    ImGuiEx.ButtonCheckbox(FontAwesomeIcon.Ticket, ref chr.NoItemBuffUse, color: !C.FullAutoGCDeliveryUseBuffItem ? ImGuiColors.DalamudRed : ImGuiColors.TankBlue, inverted: true);
                    ImGuiEx.Tooltip("允许此角色使用优先军票配额");
                    if(!C.FullAutoGCDeliveryUseBuffItem) ImGuiEx.Tooltip(EColor.RedBright, $"你必须同时在全局设置中启用此功能，它才能生效。");
                    ImGuiEx.DragDropRepopulate("EnGiBuf", chr.NoItemBuffUse, ref chr.NoItemBuffUse);

                    ImGui.TableNextColumn();
                    if(ImGuiEx.ButtonCheckbox(FontAwesomeIcon.Users, ref chr.ExcludeRetainer, inverted: true))
                    {
                        chr.Enabled = false;
                        C.SelectedRetainers.Remove(chr.CID);
                    }
                    ImGuiEx.Tooltip("启用雇员自动化");
                    ImGuiEx.DragDropRepopulate("EnRet", chr.ExcludeRetainer, ref chr.ExcludeRetainer);
                    ImGui.SameLine();
                    if(ImGuiEx.ButtonCheckbox(FontAwesomeIcon.Ship, ref chr.ExcludeWorkshop, inverted: true))
                    {
                        chr.WorkshopEnabled = false;
                        chr.EnabledSubs.Clear();
                        chr.EnabledAirships.Clear();
                    }
                    ImGuiEx.Tooltip("启用潜艇自动化");
                    ImGuiEx.DragDropRepopulate("EnDep", chr.ExcludeWorkshop, x =>
                    {
                        chr.ExcludeWorkshop = x;
                        if(!x)
                        {
                            chr.EnabledSubs.Clear();
                            chr.EnabledAirships.Clear();
                        }
                    });
                    ImGui.SameLine();
                    ImGuiEx.ButtonCheckbox(FontAwesomeIcon.DoorOpen, ref chr.ExcludeOverlay, inverted: true);
                    ImGuiEx.Tooltip("在登录覆盖窗口显示");
                    ImGuiEx.DragDropRepopulate("EnLog", chr.ExcludeOverlay, ref chr.ExcludeOverlay);
                    ImGui.SameLine();
                    ImGuiEx.ButtonCheckbox(FontAwesomeIcon.Coins, ref chr.NoGilTrack, inverted: true);
                    ImGuiEx.Tooltip("将此角色的金币计入总额");
                    ImGuiEx.DragDropRepopulate("EnGil", chr.NoGilTrack, ref chr.NoGilTrack);

                    ImGui.TableNextColumn();
                    if(ImGuiEx.IconButton(FontAwesomeIcon.UserMinus))
                    {
                        chr.ClearFCData();
                    }
                    ImGuiEx.Tooltip("重置此角色的部队和潜艇数据。登录并访问管制面板后会重新生成。");
                    ImGui.SameLine();
                    if(ImGuiEx.IconButton(FontAwesomeIcon.Trash, enabled: ImGuiEx.Ctrl))
                    {
                        new TickScheduler(() => C.OfflineData.Remove(chr));
                    }
                    ImGuiEx.Tooltip($"按住 CTRL 并左键点击可删除保存的角色数据。重新登录后会自动重建。");
                    ImGui.SameLine();
                    if(ImGuiEx.IconButton("\uf057", enabled: ImGuiEx.Ctrl))
                    {
                        C.Blacklist.Add((chr.CID, chr.Name));
                    }
                    ImGuiEx.Tooltip($"按住 CTRL 并左键点击可永久删除角色数据，并将该角色完全排除在 AutoRetainer 的处理范围外。");

                    ImGui.PopID();
                }

                ImGui.EndTable();
            }
            DragDrop.End();
        });


        if(C.Blacklist.Count != 0)
        {
            b = b.Section("已排除角色")
                .Widget(() =>
                {
                    for(var i = 0; i < C.Blacklist.Count; i++)
                    {
                        var d = C.Blacklist[i];
                        ImGuiEx.TextV($"{d.Name} ({d.CID:X16})");
                        ImGui.SameLine();
                        if(ImGui.Button($"删除##bl{i}"))
                        {
                            C.Blacklist.RemoveAt(i);
                            C.SelectedRetainers.Remove(d.CID);
                            break;
                        }
                    }
                });
        }

        b.Draw();
    }
}
