using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using static InvBaseItem;
using PreserveAttribute = UnityEngine.Scripting.PreserveAttribute;

[Preserve]
public class XUiC_RepairableVehicleStackGrid : XUiController
{
    private WindowTypeEnum windowType = WindowTypeEnum.None;
    protected XUiController[] itemControllers;
    public ItemValue[] items;
    public readonly List<XUiC_RepairableVehicleStack> RepairableVehiclePartsList = new List<XUiC_RepairableVehicleStack>();
    private readonly bool slotsSetup;
    private bool bAwakeCalled;
    private XUiV_Grid slotGrid;

    private readonly ItemValue[] m_slots = new ItemValue[32];
    public bool SetSlotItem(int index, ItemValue value, bool isLocal = true)
    {
        bool flag = false;
        this.m_slots[index] = value;

        return flag;
    }

    public override void Init()
    {
        base.Init();
        this.itemControllers = this.Parent.GetChildrenByType<XUiC_RepairableVehicleStack>();
        this.slotGrid = base.Parent.GetChildById("slotGrid").ViewComponent as XUiV_Grid;
        this.bAwakeCalled = true;
        this.IsDirty = false;
    }

    public void SetSlotIndexForStack(int slot, RepairableVehicleSlotsEnum repairableVehicleSlots)
    {
        if(itemControllers[slot] is XUiC_RepairableVehicleStack && itemControllers[slot] != null)
        {
            XUiC_RepairableVehicleStack itemController = itemControllers[slot] as XUiC_RepairableVehicleStack;

            itemController.SlotIndices.Clear();
            itemController.SlotIndices.Add((int)repairableVehicleSlots);

            itemController.RepairableVehicleSlots = repairableVehicleSlots;
            this.RepairableVehiclePartsList.Add(itemController);
        }
    }

    public override void Update(float _dt)
    {
        if(GameManager.Instance == null || GameManager.Instance.World == null)
        {
            return;
        }

        if(this.IsDirty)
        {
            if(!this.slotsSetup)
            {
                if (windowType == WindowTypeEnum.None)
                {
                    XUiC_VehicleFrameWindowRebirth parentByType = GetParentByType<XUiC_VehicleFrameWindowRebirth>();
                    if (parentByType != null)
                    {
                        windowType = WindowTypeEnum.EntityVehicle;
                    }
                    else
                    {
                        windowType = WindowTypeEnum.BlockRepairableVehicle;
                    }
                }

                if (windowType == WindowTypeEnum.BlockRepairableVehicle)
                {
                    XUiC_RepairableVehicleWindow parentByType = GetParentByType<XUiC_RepairableVehicleWindow>();

                    if (parentByType != null)
                    {
                        if (parentByType.tileEntity != null)
                        {
                            string vehicleType = "V6CarRepair";
                            if (parentByType.tileEntity.blockValue.Block.Properties.Values.ContainsKey("VehicleType"))
                            {
                                vehicleType = parentByType.tileEntity.blockValue.Block.Properties.Values["VehicleType"];
                            }

                            foreach (string vehicleTypeKey in RebirthVariables.localVehicleTypes.Keys)
                            {
                                if (vehicleTypeKey == vehicleType)
                                {
                                    List<RepairableVehicleSlotsEnum> partsList = RebirthVariables.localVehicleTypes[vehicleTypeKey];

                                    for (int i = 0; i < partsList.Count; i++)
                                    {
                                        SetSlotIndexForStack(i, partsList[i]);
                                    }

                                    this.items = this.GetSlots();
                                    this.SetStacks(this.items);
                                    this.IsDirty = false;

                                    break;
                                }
                            }
                        }
                    }
                }
                else if (windowType == WindowTypeEnum.EntityVehicle)
                {
                    XUiC_VehicleFrameWindowRebirth parentByType = GetParentByType<XUiC_VehicleFrameWindowRebirth>();
                    if (parentByType != null)
                    {
                        string vehicleType = "V6CarRepair";

                        //Log.Out("XUiC_RepairableVehicleStackGrid-Update this.itemControllers.Length: " + this.itemControllers.Length);

                        if (parentByType.Vehicle.EntityClass.Properties.Values.ContainsKey("VehicleType"))
                        {
                            vehicleType = parentByType.Vehicle.EntityClass.Properties.Values["VehicleType"];
                        }

                        foreach (string vehicleTypeKey in RebirthVariables.localVehicleTypes.Keys)
                        {
                            if (vehicleTypeKey == vehicleType)
                            {
                                List<RepairableVehicleSlotsEnum> partsList = RebirthVariables.localVehicleTypes[vehicleTypeKey];

                                for (int i = 0; i < partsList.Count; i++)
                                {
                                    SetSlotIndexForStack(i, partsList[i]);
                                }

                                this.items = this.GetSlots();
                                this.SetStacks(this.items);
                                this.IsDirty = false;

                                break;
                            }
                        }
                    }
                }
                //Log.Error("XUiC_RepairableVehicleStackGrid-Update parentByType is NULL!!!");
            }
        }

        base.Update(_dt);
    }

    public virtual ItemValue[] GetSlots()
    {
        ItemValue[] itemValues = null;

        if (windowType == WindowTypeEnum.BlockRepairableVehicle)
        {
            XUiC_RepairableVehicleWindow parentByType = GetParentByType<XUiC_RepairableVehicleWindow>();
            if (parentByType != null)
            {
                itemValues = parentByType.tileEntity.GetRepairableVehicleParts();
            }
        }
        else if (windowType == WindowTypeEnum.EntityVehicle)
        {
            EntityVehicleRebirth vehicle = GetParentByType<XUiC_VehicleFrameWindowRebirth>().Vehicle;

            itemValues = vehicle.itemValues;
        }

        return itemValues;
    }

    protected virtual void SetStacks(ItemValue[] stackList)
    {
        if(stackList == null)
        {
            return;
        }

        XUiC_ItemInfoWindowRebirth parentByType = GetParentByType<XUiC_ItemInfoWindowRebirth>();
        for(int index = 0; index < stackList.Length && this.itemControllers.Length > index && stackList.Length > index; ++index)
        {
            XUiC_RepairableVehicleStack itemController = (XUiC_RepairableVehicleStack)this.itemControllers[index];
            itemController.SlotChangedEvent -= new XUiEvent_SlotChangedEventHandler(this.HandleSlotChangedEvent);
            itemController.ItemValue = stackList[index];
            itemController.SlotChangedEvent += new XUiEvent_SlotChangedEventHandler(this.HandleSlotChangedEvent);
            itemController.SlotNumber = index;
            itemController.InfoWindow = parentByType;
        }
    }

    public void HandleSlotChangedEvent(int slotNumber, ItemStack stack)
    {
        if (stack.IsEmpty())
        {
            for (int index = 0; index < this.RepairableVehiclePartsList[slotNumber].SlotIndices.Count; ++index)
            {
                SetSlotItem(this.RepairableVehiclePartsList[slotNumber].SlotIndices[index], ItemValue.None.Clone());
            }
        }

        else
        {
            if (this.items != null)
            {
                this.items[slotNumber] = stack.itemValue.Clone();
                SetSlotItem(slotNumber, stack.itemValue);
            }
            else
            {
                Log.Out("XUiC_RepairableVehicleStackGrid-HandleSlotChangedEvent this.items IS NULL");
            }
        }
    }

    public override void OnOpen()
    {
        if(this.ViewComponent != null && !this.ViewComponent.IsVisible)
        {
            this.ViewComponent.IsVisible = true;
        }

        this.IsDirty = true;
        this.IsDormant = false;
    }

    public override void OnClose()
    {
        for(int index = 0; index < this.itemControllers.Length; ++index)
        {
            this.itemControllers[index].Hovered(false);
        }

        if(this.ViewComponent != null && this.ViewComponent.IsVisible)
        {
            this.ViewComponent.IsVisible = false;
        }

        this.IsDormant = true;
    }
}
