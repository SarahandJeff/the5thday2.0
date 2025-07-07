using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_VehicleWindowGroupRebirth : XUiController
{
	public EntityVehicle CurrentVehicleEntity
	{
		get
		{
			return this.currentVehicleEntity;
		}
		set
		{
			base.xui.vehicle = value;
			this.currentVehicleEntity = value;
			this.frameWindow.Vehicle = (EntityVehicleRebirth)value;
			Vehicle vehicle = value.GetVehicle();
			ItemValue updatedItemValue = vehicle.GetUpdatedItemValue();
			ItemStack currentItem = new ItemStack(updatedItemValue, 1);
			this.partGrid.AssembleWindow = this.frameWindow;
			this.partGrid.CurrentVehicle = vehicle;
			this.partGrid.CurrentItem = currentItem;
			this.partGrid.SetMods(updatedItemValue.Modifications);
			this.cosmeticGrid.AssembleWindow = this.frameWindow;
			this.cosmeticGrid.CurrentItem = currentItem;
			this.cosmeticGrid.SetParts(updatedItemValue.CosmeticMods);
			XUiM_AssembleItem assembleItem = base.xui.AssembleItem;
			assembleItem.AssembleWindow = this.frameWindow;
			assembleItem.CurrentItem = currentItem;
			assembleItem.CurrentItemStackController = null;
		}
	}

	public override void Init()
	{
		base.Init();
		this.nonPagingHeaderWindow = base.GetChildByType<XUiC_WindowNonPagingHeader>();
		this.headerName = Localization.Get("xuiVehicle");
		this.frameWindow = base.GetChildByType<XUiC_VehicleFrameWindowRebirth>();
		this.partGrid = base.GetChildByType<XUiC_VehiclePartStackGrid>();
		this.cosmeticGrid = base.GetChildByType<XUiC_ItemCosmeticStackGrid>();
	}

	public override void Update(float _dt)
	{
		if (this.windowGroup.isShowing)
		{
			if (!base.xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
			{
				this.wasReleased = true;
			}
			if (this.wasReleased)
			{
				if (base.xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
				{
					this.activeKeyDown = true;
				}
				if (base.xui.playerUI.playerInput.PermanentActions.Activate.WasReleased && this.activeKeyDown)
				{
					this.activeKeyDown = false;
					if (!base.xui.playerUI.windowManager.IsInputActive())
					{
						base.xui.playerUI.windowManager.CloseAllOpenWindows(null, false);
					}
				}
			}
		}
		if (this.currentVehicleEntity != null && !this.currentVehicleEntity.CheckUIInteraction())
		{
			base.xui.playerUI.windowManager.Close(XUiC_VehicleWindowGroupRebirth.ID);
		}
		base.Update(_dt);
	}

	public override void OnOpen()
	{
		base.OnOpen();
        if (this.nonPagingHeaderWindow != null)
		{
			this.nonPagingHeaderWindow.SetHeader(this.headerName);
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		XUiM_AssembleItem assembleItem = base.xui.AssembleItem;
		assembleItem.AssembleWindow = null;
		if (assembleItem.CurrentItem.itemValue == base.xui.vehicle.GetVehicle().GetUpdatedItemValue())
		{
			assembleItem.CurrentItem = null;
			assembleItem.CurrentItemStackController = null;
		}
		this.wasReleased = false;
		this.activeKeyDown = false;
		this.CurrentVehicleEntity.StopUIInteraction();
		base.xui.vehicle = null;
	}

	public void OnItemChanged(ItemStack itemStack)
	{
		this.partGrid.CurrentItem = itemStack;
		this.partGrid.SetMods(itemStack.itemValue.Modifications);
		this.cosmeticGrid.CurrentItem = itemStack;
		this.cosmeticGrid.SetParts(itemStack.itemValue.CosmeticMods);
	}

	public static string ID = "vehicle";
	private XUiC_VehicleFrameWindowRebirth frameWindow;
	private XUiC_WindowNonPagingHeader nonPagingHeaderWindow;
	private XUiC_VehiclePartStackGrid partGrid;
	private XUiC_ItemCosmeticStackGrid cosmeticGrid;
	private string headerName;
	private EntityVehicle currentVehicleEntity;
	private bool activeKeyDown;
	private bool wasReleased;
}
