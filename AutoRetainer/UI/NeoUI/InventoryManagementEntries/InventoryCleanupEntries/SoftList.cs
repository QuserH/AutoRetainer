using AutoRetainerAPI.Configuration;

namespace AutoRetainer.UI.NeoUI.InventoryManagementEntries.InventoryCleanupEntries;
public class SoftList : InventoryManagementBase
{
    public override string Name => "背包清理/快速雇员出售清单";
    private InventoryManagementCommon InventoryManagementCommon = new();
    private SoftList()
    {
        Builder = InventoryCleanupCommon.CreateCleanupHeaderBuilder()
            .Section(Name)
            .TextWrapped("通过快速探险获得的这些物品会自动出售，除非背包中已有同类物品。")
            .Widget(() => InventoryManagementCommon.DrawListNew(
                itemId => InventoryCleanupCommon.SelectedPlan.AddItemToList(IMListKind.SoftSell, itemId, out _),
                itemId => InventoryCleanupCommon.SelectedPlan.IMAutoVendorSoft.Remove(itemId), InventoryCleanupCommon.SelectedPlan.IMAutoVendorSoft,
                filter: item => item.PriceLow != 0))
            .Widget(() =>
            {
                InventoryManagementCommon.ImportFromArDiscard(InventoryCleanupCommon.SelectedPlan.IMAutoVendorSoft);
            });
    }
}
