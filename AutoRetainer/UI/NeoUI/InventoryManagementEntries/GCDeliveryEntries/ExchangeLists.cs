using AutoRetainerAPI.Configuration;
using ECommons.Configuration;
using ECommons.ExcelServices;
using ECommons.GameHelpers;
using Lumina.Excel.Sheets;
using System.Numerics;
using GrandCompany = ECommons.ExcelServices.GrandCompany;
using GrandCompanyRank = Lumina.Excel.Sheets.GrandCompanyRank;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.GCDeliveryEntries;
public sealed unsafe class ExchangeLists : InventoryManagementBase
{
    private ImGuiEx.RealtimeDragDrop<GCExchangeItem> DragDrop = new("GCELDD", x => x.ID);
    public override string Name { get; } = "大国防联军 - 军票交换清单";
    private GCExchangeCategoryTab? SelectedCategory = null;
    private GCExchangeCategoryTab? SelectedCategory2 = null;
    private GCExchangeRankTab? SelectedRank = null;
    private Guid SelectedPlanGuid = Guid.Empty;

    public override int DisplayPriority => -5;

    public override void Draw()
    {
        C.AdditionalGCExchangePlans.Where(x => x.GUID == Guid.Empty).Each(x => x.GUID = Guid.NewGuid());
        ImGuiEx.TextWrapped($"""
            选择在大国防联军专家交付操作期间自动购买的物品。
            购买逻辑:
            - 系统将尝试从列表中购买第一个可用的物品。
            - 购买将持续到该物品在你的背包中的数量达到指定的目标数量。
            如果没有可购买的列表物品，或它们无法放入你的背包:
            - 系统将改为购买冒险委托。
            - 委托购买将持续到你的委托数量达到 65,000。
            一旦达到委托上限且无法再进行其他购买:
            - 多余的军票将被丢弃。
            """);

        var selectedPlan = C.AdditionalGCExchangePlans.FirstOrDefault(x => x.GUID == SelectedPlanGuid);
        ImGuiEx.InputWithRightButtonsArea(() =>
        {
            if(ImGui.BeginCombo("##selplan", selectedPlan?.DisplayName ?? "预设计划"))
            {
                if(ImGui.Selectable("预设计划", selectedPlan == null)) SelectedPlanGuid = Guid.Empty;
                ImGui.Separator();
                foreach(var x in C.AdditionalGCExchangePlans)
                {
                    ImGui.PushID(x.ID);
                    if(ImGui.Selectable(x.DisplayName)) SelectedPlanGuid = x.GUID;
                    ImGui.PopID();
                }
                ImGui.EndCombo();
            }
        }, () =>
        {
            if(ImGuiEx.IconButton(FontAwesomeIcon.Plus))
            {
                var newPlan = new GCExchangePlan();
                C.AdditionalGCExchangePlans.Add(newPlan);
                SelectedPlanGuid = newPlan.GUID;
            }
            ImGuiEx.Tooltip("添加新的计划");
            ImGui.SameLine(0, 1);
            if(ImGuiEx.IconButton(FontAwesomeIcon.Copy))
            {
                var clone = (selectedPlan ?? C.DefaultGCExchangePlan).DSFClone();
                clone.GUID = Guid.Empty;
                Copy(EzConfig.DefaultSerializationFactory.Serialize(clone));
            }
            ImGuiEx.Tooltip("复制");
            ImGui.SameLine(0, 1);
            if(ImGuiEx.IconButton(FontAwesomeIcon.Paste))
            {
                try
                {
                    var newPlan = EzConfig.DefaultSerializationFactory.Deserialize<GCExchangePlan>(Paste()) ?? throw new NullReferenceException();
                    newPlan.GUID.Regenerate();  
                    C.AdditionalGCExchangePlans.Add(newPlan);
                    SelectedPlanGuid = newPlan.GUID;
                }
                catch(Exception e)
                {
                    e.Log();
                    Notify.Error(e.Message);
                }
            }
            ImGuiEx.Tooltip("贴上");
            if(selectedPlan != null)
            {
                ImGui.SameLine(0, 1);
                if(ImGuiEx.IconButton(FontAwesomeIcon.ArrowsUpToLine, enabled: ImGuiEx.Ctrl && selectedPlan != null))
                {
                    C.DefaultGCExchangePlan = selectedPlan.DSFClone();
                    C.DefaultGCExchangePlan.Name = "";
                    C.DefaultGCExchangePlan.GUID.Regenerate();
                    new TickScheduler(() => C.AdditionalGCExchangePlans.Remove(selectedPlan));
                }
                ImGuiEx.Tooltip("将此计划设为预设计划，当前预设计划将会被覆盖。按住CTRL + 左键");
                ImGui.SameLine(0, 1);
                if(ImGuiEx.IconButton(FontAwesomeIcon.Trash, enabled: ImGuiEx.Ctrl && selectedPlan != null))
                {
                    new TickScheduler(() => C.AdditionalGCExchangePlans.Remove(selectedPlan));
                }
                ImGuiEx.Tooltip("删除此计划。按住CTRL + 左键");
            }
        });

        if(SelectedPlanGuid == Guid.Empty)
        {
            DrawGCEchangeList(C.DefaultGCExchangePlan);
        }
        else
        {
            if(Data != null)
            {
                if(Data.ExchangePlan == SelectedPlanGuid)
                {
                    ImGuiEx.Text(ImGuiColors.ParsedGreen, UiBuilder.IconFont, FontAwesomeIcon.Check.ToIconString());
                    ImGui.SameLine();
                    ImGuiEx.Text(ImGuiColors.ParsedGreen, $"当前角色使用");
                    ImGui.SameLine();
                    if(ImGui.SmallButton("取消分配"))
                    {
                        Data.ExchangePlan = Guid.Empty;
                    }
                }
                else
                {
                    ImGuiEx.Text(ImGuiColors.DalamudOrange, UiBuilder.IconFont, FontAwesomeIcon.ExclamationTriangle.ToIconString());
                    ImGui.SameLine();
                    ImGuiEx.Text(ImGuiColors.DalamudOrange, $"非当前角色使用");
                    ImGui.SameLine();
                    if(ImGui.SmallButton("分配"))
                    {
                        Data.ExchangePlan = selectedPlan.GUID;
                    }
                }
                ImGui.SameLine();
            }

            var charas = C.OfflineData.Where(x => x.ExchangePlan == selectedPlan.GUID).ToArray();
            if(charas.Length > 0)
            {
                ImGuiEx.Text($"共有 {charas.Length} 个角色使用");
                ImGuiEx.Tooltip($"{charas.Select(x => x.NameWithWorldCensored).Print("\n")}");
            }
            else
            {
                ImGuiEx.Text($"没有任何角色使用");
            }

                var planIndex = C.AdditionalGCExchangePlans.IndexOf(x => x.GUID == SelectedPlanGuid);
            if(planIndex == -1)
            {
                SelectedPlanGuid = Guid.Empty;
            }
            else
            {
                DrawGCEchangeList(C.AdditionalGCExchangePlans[planIndex]);
            }
        }
    }

    public void DrawGCEchangeList(GCExchangePlan plan)
    {
        ref string getFilter() => ref Ref<string>.Get($"{plan.ID}filter");
        ref bool onlySelected() => ref Ref<bool>.Get($"{plan.ID}onlySel");
        ref string getFilter2() => ref Ref<string>.Get($"{plan.ID}filter2");

        ImGui.PushID(plan.ID);
        plan.Validate();

        ImGuiEx.InputWithRightButtonsArea("GCPlanSettings", () =>
        {
            if(ReferenceEquals(plan, C.DefaultGCExchangePlan))
            {
                ImGui.BeginDisabled();
                var s = "Default exchange plan can not be renamed";
                ImGui.InputText("##name", ref s, 1);
                ImGui.EndDisabled();
            }
            else
            {
                ImGui.InputTextWithHint($"##name", "名称", ref plan.Name, 100);
                ImGuiEx.Tooltip("交换计划名称");
            }
        }, () =>
        {
            ImGui.SetNextItemWidth(100f);
            ImGui.InputInt("保留军票数量", ref plan.RemainingSeals.ValidateRange(0, 70000), 0, 0);
            ImGuiEx.HelpMarker($"This amount of seals will be kept after purchase list is executed. However, this value will be capped to be no more than 20000 seals less than maximum possible, according to character's rank. ");
            ImGui.SameLine();
            ImGui.Checkbox("筹备后交换", ref plan.FinalizeByPurchasing);
            ImGuiEx.HelpMarker("勾选后将会在筹备稀有品完成后交换物品；未勾选则在军票到达上限后才交换");
        });

        ImGuiEx.SetNextItemFullWidth();
        if(ImGui.BeginCombo("##Add Items", "添加物品", ImGuiComboFlags.HeightLarge))
        {
            ImGuiEx.InputWithRightButtonsArea(() =>
            {
                ImGui.InputTextWithHint("##filter2", "搜索...", ref getFilter2(), 100);
            }, () =>
            {
                ImGui.SetNextItemWidth(100f);
                ImGuiEx.EnumCombo("##cat2", ref SelectedCategory2, nullName: "所有类别");
                ImGuiEx.Tooltip("类别");
            });
            foreach(var x in Utils.SharedGCExchangeListings)
            {
                if(getFilter2().Length > 0
                    && !x.Value.Data.Name.GetText().Contains(getFilter2(), StringComparison.OrdinalIgnoreCase)
                    && !x.Value.Category.ToString().Equals(getFilter2(), StringComparison.OrdinalIgnoreCase)
                    && !Utils.GCRanks[x.Value.MinPurchaseRank].Equals(getFilter2(), StringComparison.OrdinalIgnoreCase)
                    ) continue;
                if(SelectedCategory2 != null && x.Value.Category != SelectedCategory2.Value) continue;
                var cont = plan.Items.Select(s => s.ItemID).ToArray();
                if(ThreadLoadImageHandler.TryGetIconTextureWrap(x.Value.Data.Icon, false, out var t))
                {
                    ImGui.Image(t.Handle, new(ImGui.GetTextLineHeight()));
                    ImGui.SameLine();
                }
                if(ImGui.Selectable(x.Value.Data.GetName() + $"##{x.Key}", cont.Contains(x.Key), ImGuiSelectableFlags.DontClosePopups))
                {
                    plan.Items.Add(new(x.Key, 0));
                }
            }
            ImGui.EndCombo();
        }
        if(ImGui.BeginPopup("Ex"))
        {
            if(ImGui.Selectable("自动交换军票武器与装备，最佳化以获取额外部队点数。(FC Point)"))
            {
                List<GCExchangeItem> items = [];
                var qualifyingItems = Utils.SharedGCExchangeListings.Where(x => (x.Value.Category == GCExchangeCategoryTab.Weapons || x.Value.Category == GCExchangeCategoryTab.Armor) && x.Value.Data.GetRarity() == ItemRarity.Green).ToDictionary();
                plan.Items.RemoveAll(x => qualifyingItems.ContainsKey(x.ItemID));
                foreach(var item in qualifyingItems)
                {
                    items.Add(new(item.Key, 0));
                }
                items = items.OrderByDescending(x => (double)Svc.Data.GetExcelSheet<GCSupplyDutyReward>().GetRow(x.Data.Value.LevelItem.RowId).SealsExpertDelivery / (double)Utils.SharedGCExchangeListings[x.ItemID].Seals).ToList();
                foreach(var x in items)
                {
                    plan.Items.Add(x);
                    x.Quantity = Utils.SharedGCExchangeListings[x.ItemID].Data.IsUnique ? 1 : 999;
                }
            }
            ImGuiEx.Tooltip("选择此选项将自动填入所有可交换的武器与装备；交换的武器与装备将会立即筹备至军队，以最大化产生部队点数。这些物品将会被放在清单末端，且仅在没有其他可购买物品时才会购买。");
            if(ImGui.Selectable("加入所有缺少的物品"))
            {
                foreach(var x in Utils.SharedGCExchangeListings)
                {
                    if(!plan.Items.Any(i => i.ItemID == x.Key))
                    {
                        plan.Items.Add(new(x.Key, 0));
                    }
                }
            }
            if(ImGui.Selectable("将数量重设为 0"))
            {
                plan.Items.Each(x => x.Quantity = 0);
                plan.Items.Each(x => x.QuantitySingleTime = 0);
            }
            if(ImGui.Selectable("移除数量为 0 的物品"))
            {
                plan.Items.RemoveAll(x => x.Quantity == 0 && x.QuantitySingleTime == 0);
            }
            if(ImGuiEx.Selectable("清除清单(按住 CTRL + 左键)", enabled: ImGuiEx.Ctrl))
            {
                plan.Items.Clear();
            }
            ImGui.EndPopup();
        }
        if(ImGuiEx.IconButtonWithText(FontAwesomeIcon.AngleDoubleDown, "操作"))
        {
            ImGui.OpenPopup("Ex");
        }
        ImGui.SameLine();
        ImGuiEx.InputWithRightButtonsArea("Fltr2", () =>
        {
            ImGui.InputTextWithHint("##filter", "搜索...", ref getFilter(), 100);
        }, () =>
        {
            ImGui.Checkbox("仅显示已选择", ref onlySelected());
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100f);
            ImGuiEx.EnumCombo("##cat", ref SelectedCategory, nullName: "所有类别");
            ImGuiEx.Tooltip("类别");
        });



        DragDrop.Begin();
        if(ImGuiEx.BeginDefaultTable("GCDeliveryList", ["##dragDrop", "~物品", "军票", "等级", "价格", "类别", "保留数量", "单次数量", "##controls"]))
        {
            for(var i = 0; i < plan.Items.Count; i++)
            {
                var currentItem = plan.Items[i];
                var meta = Utils.SharedGCExchangeListings[currentItem.ItemID];
                if(onlySelected() && currentItem.Quantity == 0) continue;
                if(getFilter().Length > 0
                    && !meta.Data.Name.GetText().Contains(getFilter(), StringComparison.OrdinalIgnoreCase)
                    && !meta.Category.ToString().Equals(getFilter(), StringComparison.OrdinalIgnoreCase)
                    && !Utils.GCRanks[meta.MinPurchaseRank].Equals(getFilter(), StringComparison.OrdinalIgnoreCase)
                    ) continue;
                if(SelectedCategory != null && meta.Category != SelectedCategory.Value) continue;
                ImGui.PushID(currentItem.ID);
                ImGui.TableNextRow();
                DragDrop.SetRowColor(currentItem);
                ImGui.TableNextColumn();
                DragDrop.NextRow();
                if(ImGuiEx.IconButton(FontAwesomeIcon.AngleDoubleUp))
                {
                    new TickScheduler(() =>
                    {
                        plan.Items.Remove(currentItem);
                        plan.Items.Insert(0, currentItem);
                    });
                }
                ImGui.SameLine(0, 1);
                ImGuiEx.Tooltip("移至顶部");
                DragDrop.DrawButtonDummy(currentItem, plan.Items, i);
                ImGui.TableNextColumn();
                if(ThreadLoadImageHandler.TryGetIconTextureWrap(meta.Data.Icon, false, out var t))
                {
                    ImGui.Image(t.Handle, new(ImGui.GetFrameHeight()));
                    ImGui.SameLine();
                }
                ImGuiEx.TextV($"{meta.Data.Name.GetText()}");
                ImGui.TableNextColumn();
                foreach(var c in Enum.GetValues<GrandCompany>().Where(x => x != GrandCompany.Unemployed))
                {
                    if(ThreadLoadImageHandler.TryGetIconTextureWrap(60870 + (int)c, false, out var ctex))
                    {
                        var trans = !meta.Companies.Contains(c);
                        if(trans) ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.2f);
                        ImGui.Image(ctex.Handle, new(ImGui.GetFrameHeight()));
                        if(trans) ImGui.PopStyleVar();
                        ImGuiEx.Tooltip($"{c}" + (trans ? " (unavailable)" : ""));
                        ImGui.SameLine(0, 1);
                    }
                }
                ImGui.TableNextColumn();
                ImGuiEx.TextV($"{meta.Data.LevelItem.RowId}");
                ImGui.TableNextColumn();
                if(Svc.Data.GetExcelSheet<GrandCompanyRank>().TryGetRow(meta.MinPurchaseRank, out var rank) && ThreadLoadImageHandler.TryGetIconTextureWrap(rank.IconFlames, false, out var tex))
                {
                    ImGui.Image(tex.Handle, new(ImGui.GetFrameHeight()));
                    var rankName = Utils.GCRanks[meta.MinPurchaseRank];
                    ImGuiEx.Tooltip(rankName);
                    if(ImGuiEx.HoveredAndClicked()) getFilter() = rankName;
                    ImGui.SameLine();
                }
                ImGuiEx.TextV($"{meta.Seals}");
                ImGui.TableNextColumn();
                ImGuiEx.TextV($"{meta.Category}");
                if(ImGuiEx.HoveredAndClicked()) getFilter() = meta.Category.ToString();
                ImGui.TableNextColumn();
                if(currentItem.Data.Value.IsUnique)
                {
                    ImGuiEx.Checkbox("独特的", ref currentItem.Quantity);
                }
                else
                {
                    ImGui.SetNextItemWidth(100f.Scale());
                    ImGui.InputInt("##qty", ref currentItem.Quantity.ValidateRange(0, int.MaxValue), 0, 0);
                }
                ImGuiEx.Tooltip("选择背包中要维持的数量");
                ImGui.TableNextColumn();
                ImGui.SetNextItemWidth(100f.Scale());
                ImGui.InputInt("##qtyonetime", ref currentItem.QuantitySingleTime.ValidateRange(0, currentItem.Data.Value.IsUnique ? 1 : int.MaxValue), 0, 0);
                ImGuiEx.Tooltip("每次交换的数量：当使用此方案在任意角色上进行交换时，该数量将从此值中扣除。当数值降至 0 时，将自动恢复为\"维持\"数量。");
                ImGui.TableNextColumn();
                if(ImGuiEx.IconButton(FontAwesomeIcon.Clone))
                {
                    plan.Items.Insert(i + 1, currentItem.JSONClone());
                }
                ImGuiEx.Tooltip("复制此列表");
                ImGui.SameLine(0, 1);
                if(ImGuiEx.IconButton(FontAwesomeIcon.Trash))
                {
                    new TickScheduler(() => plan.Items.Remove(currentItem));
                }
                ImGuiEx.Tooltip($"如果清单中存在多个相同的物品，则将其从清单中删除；如果清单中只有一个相同的物品，则将其数量设为 0。");
                ImGui.PopID();
            }
            ImGui.EndTable();
        }
        DragDrop.End();
        ImGui.PopID();
    }
}
