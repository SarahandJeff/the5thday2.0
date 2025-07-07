using System;
using System.Collections.Generic;
using System.Security.Principal;
using Audio;

using InControl;

using UnityEngine;
using static InvGameItem;

//[Preserve]
public class XUiC_RepairableVehicleStack : XUiC_SelectableEntry
{
    private WindowTypeEnum windowType = WindowTypeEnum.None;
    private ItemStack itemStack = ItemStack.Empty.Clone();
    public ItemValue itemValue = new ItemValue();
    private bool isDirty = true;
    private bool isOver;
    private Color32 selectedBorderColor = new Color32(222, 206, 163, byte.MaxValue);
    private Color32 hoverBorderColor = new Color32(182, 166, 123, byte.MaxValue);
    private Color32 normalBorderColor = new Color32(96, 96, 96, byte.MaxValue);
    private Color32 normalBackgroundColor = new Color32(96, 96, 96, 96);
    private bool lastClicked;
    private string emptySpriteName = "";
    private string emptyTooltipName = "";
    private AudioClip pickupSound;
    public AudioClip placeSound;
    private RepairableVehicleSlotsEnum repairableVehicleSlots;
    public int PickupSnapDistance = 4;
    public static Color32 finalPressedColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
    public static Color32 backgroundColor = new Color32(96, 96, 96, byte.MaxValue);
    public static Color32 highlightColor = new Color32(222, 206, 163, byte.MaxValue);
    public Color32 holdingColor = new Color32(byte.MaxValue, 128, 0, byte.MaxValue);
    private readonly XUiController timer;
    private XUiController stackValue;
    private XUiController itemIcon;
    private XUiController durability;
    private XUiController durabilityBackground;
    private XUiController lockTypeIcon;
    private XUiController tintedOverlay;
    private XUiController highlightOverlay;
    private XUiController overlay;
    private XUiController background;
    public List<int> SlotIndices = new List<int>();
    private TweenScale tweenScale;
    private Vector3 startMousePos = Vector3.zero;

    public RepairableVehicleSlotsEnum RepairableVehicleSlots
    {
        get
        {
            return this.repairableVehicleSlots;
        }

        set
        {
            this.repairableVehicleSlots = value;
            this.SetEmptySpriteNameAndTooltip();
        }
    }

    public int SlotNumber
    {
        get; set;
    }

    public event XUiEvent_SlotChangedEventHandler SlotChangedEvent;

    public float HoverIconGrow
    {
        get; private set;
    }

    public ItemValue ItemValue
    {
        get
        {
            return this.itemValue;
        }

        set
        {
            if(this.itemValue != value)
            {
                this.itemValue = value;
                this.itemStack.itemValue = this.itemValue;
                if(!this.itemStack.itemValue.IsEmpty())
                {
                    this.itemStack.count = 1;
                }

                if(value.IsEmpty() && this.Selected)
                {
                    this.Selected = false;
                    this.InfoWindow?.SetItemStack(null, true);
                }

                this.SlotChangedEvent?.Invoke(this.SlotNumber, this.itemStack);
            }

            this.isDirty = true;
        }
    }

    public ItemStack ItemStack
    {
        get
        {
            return this.itemStack;
        }

        set
        {
            if(this.itemStack == value)
            {
                return;
            }

            this.itemStack = value.Clone();
            this.ItemValue = this.itemStack.itemValue.Clone();
            this.isDirty = true;
        }
    }

    public XUiC_ItemInfoWindowRebirth InfoWindow
    {
        get; set;
    }

    public override void SelectedChanged(bool isSelected)
    {
        this.SetBorderColor(isSelected ? this.selectedBorderColor : this.normalBorderColor);
    }

    private void SetBorderColor(Color32 color)
    {
        ((XUiV_Sprite)this.background.ViewComponent).Color = (Color)color;
    }

    private void SetEmptySpriteNameAndTooltip()
    {
        foreach (RepairableVehicleSlotsEnum key in RebirthVariables.localVehicleParts.Keys)
        {
            if(key == this.repairableVehicleSlots)
            {
                this.emptySpriteName = RebirthVariables.localVehicleParts[key].emptySpriteName;
                this.emptyTooltipName = Localization.Get(RebirthVariables.localVehicleParts[key].emptyTooltipName);

                //Log.Out("XUiC_RepairableVehicleStack-SetEmptySpriteNameAndTooltip emptySpriteName: " + this.emptySpriteName);
                //Log.Out("XUiC_RepairableVehicleStack-SetEmptySpriteNameAndTooltip emptyTooltipName: " + this.emptyTooltipName);

                break;
            }
        }

        if(this.emptyTooltipName == null)
        {
            return;
        }

        this.emptyTooltipName = this.emptyTooltipName.ToUpper();
    }

    public override void Init()
    {
        base.Init();
        this.stackValue = this.GetChildById("stackValue");
        this.background = this.GetChildById("background");
        this.SetBorderColor(this.normalBorderColor);
        this.itemIcon = this.GetChildById("itemIcon");
        this.durabilityBackground = this.GetChildById("durabilityBackground");
        this.durability = this.GetChildById("durability");
        this.tintedOverlay = this.GetChildById("tintedOverlay");
        this.highlightOverlay = this.GetChildById("highlightOverlay");
        this.lockTypeIcon = this.GetChildById("lockTypeIcon");
        this.overlay = this.GetChildById("overlay");
        this.itemStack.count = 1;
        this.tweenScale = this.itemIcon.ViewComponent.UiTransform.gameObject.AddComponent<TweenScale>();
    }

    public override void Update(float _dt)
    {
        base.Update(_dt);
        if(this.WindowGroup.isShowing)
        {
            PlayerActionsGUI guiActions = this.xui.playerUI.playerInput.GUIActions;
            CursorControllerAbs cursorController = this.xui.playerUI.CursorController;
            Vector3 screenPosition = (Vector3)cursorController.GetScreenPosition();
            bool mouseButtonUp = cursorController.GetMouseButtonUp(UICamera.MouseButton.LeftButton);
            bool mouseButtonDown = cursorController.GetMouseButtonDown(UICamera.MouseButton.LeftButton);
            bool mouseButton = cursorController.GetMouseButton(UICamera.MouseButton.LeftButton);

            if (windowType == WindowTypeEnum.None)
            {
                XUiC_VehicleFrameWindowRebirth parentByType = GetParentByType<XUiC_VehicleFrameWindowRebirth>();
                if (parentByType != null)
                {
                    //Log.Out("XUiC_RepairableVehicleStack-Update windowType: EntityVehicle");
                    windowType = WindowTypeEnum.EntityVehicle;
                }
                else
                {
                    //Log.Out("XUiC_RepairableVehicleStack-Update windowType: BlockRepairableVehicle");
                    //Log.Out("XUiC_RepairableVehicleStack-Update windowType: BlockRepairableVehicle");
                    windowType = WindowTypeEnum.BlockRepairableVehicle;
                }
            }

            if (this.isOver && UICamera.hoveredObject == ViewComponent.UiTransform.gameObject && this.ViewComponent.EventOnPress)
            {
                ////Log.Out("XUiC_RepairableVehicleStack-Update 1");
                if (guiActions.LastInputType == BindingSourceType.DeviceBindingSource)
                {
                    //Log.Out("XUiC_RepairableVehicleStack-Update 2");
                    bool wasReleased1 = guiActions.Submit.WasReleased;
                    bool wasReleased2 = guiActions.Inspect.WasReleased;
                    bool wasReleased3 = guiActions.RightStick.WasReleased;
                    if(this.xui.dragAndDrop.CurrentStack.IsEmpty() && !this.ItemStack.IsEmpty())
                    {
                        //Log.Out("XUiC_RepairableVehicleStack-Update 3");
                        if (wasReleased1)
                        {
                            //Log.Out("XUiC_RepairableVehicleStack-Update 4");
                            this.SwapItem();
                        }
                        else if(wasReleased3)
                        {
                            //Log.Out("XUiC_RepairableVehicleStack-Update 5");
                            this.HandleMoveToPreferredLocation();
                        }
                        else if(wasReleased2)
                        {
                            //Log.Out("XUiC_RepairableVehicleStack-Update 6");
                            this.HandleItemInspect();
                        }

                        if(this.itemStack.IsEmpty())
                        {
                            //Log.Out("XUiC_RepairableVehicleStack-Update 7");
                            ((XUiV_Sprite)this.background.ViewComponent).Color = (Color)XUiC_RepairableVehicleStack.backgroundColor;
                        }
                    }
                    else if(wasReleased1)
                    {
                        //Log.Out("XUiC_RepairableVehicleStack-Update 8");
                        this.HandleStackSwap();
                    }
                }
                else if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    //Log.Out("XUiC_RepairableVehicleStack-Update 9");
                    if (mouseButtonUp)
                    {
                        //Log.Out("XUiC_RepairableVehicleStack-Update 0");
                        this.HandleMoveToPreferredLocation();
                    }
                }
                else if(mouseButton)
                {
                    //Log.Out("XUiC_RepairableVehicleStack-Update 10");
                    if (this.xui.dragAndDrop.CurrentStack.IsEmpty() && !this.ItemStack.IsEmpty())
                    {
                        //Log.Out("XUiC_RepairableVehicleStack-Update 11");
                        if (!this.lastClicked)
                        {
                            //Log.Out("XUiC_RepairableVehicleStack-Update 12");
                            this.startMousePos = screenPosition;
                        }
                        else if((double)Mathf.Abs((screenPosition - this.startMousePos).magnitude) > PickupSnapDistance)
                        {
                            //Log.Out("XUiC_RepairableVehicleStack-Update 13");
                            this.SwapItem();
                        }

                        //Log.Out("XUiC_RepairableVehicleStack-Update 14");
                        this.SetBorderColor(this.normalBorderColor);
                    }

                    if(mouseButtonDown)
                    {
                        //Log.Out("XUiC_RepairableVehicleStack-Update 15");
                        this.lastClicked = true;
                    }
                }
                else if(mouseButtonUp)
                {
                    //Log.Out("XUiC_RepairableVehicleStack-Update 16");
                    if (this.xui.dragAndDrop.CurrentStack.IsEmpty())
                    {
                        //Log.Out("XUiC_RepairableVehicleStack-Update 17");
                        this.HandleItemInspect();
                    }
                    else if(this.lastClicked)
                    {
                        //Log.Out("XUiC_RepairableVehicleStack-Update 18");
                        this.HandleStackSwap();
                    }
                }
                else
                {
                    ////Log.Out("XUiC_RepairableVehicleStack-Update 19");
                    this.lastClicked = false;
                }
            }
            else
            {
                this.lastClicked = false;
                if(this.isOver || this.itemIcon.ViewComponent.UiTransform.localScale != Vector3.one)
                {
                    if(this.tweenScale.value != Vector3.one && !this.itemStack.IsEmpty())
                    {
                        this.tweenScale.from = Vector3.one * 1.5f;
                        this.tweenScale.to = Vector3.one;
                        this.tweenScale.enabled = true;
                        this.tweenScale.duration = 0.5f;
                    }

                    this.isOver = false;
                }
            }
        }

        if (this.isDirty)
        {
            bool flag = !this.itemValue.IsEmpty();
            ItemClass itemClass = null;
            if(flag)
            {
                itemClass = ItemClass.GetForId(this.itemValue.type);
            }

            if(this.itemIcon != null)
            {
                ((XUiV_Sprite)this.itemIcon.ViewComponent).SpriteName = flag ? this.itemStack.itemValue.GetPropertyOverride("CustomIcon", itemClass.GetIconName()) : this.emptySpriteName;
                ((XUiV_Sprite)this.itemIcon.ViewComponent).UIAtlas = flag ? "ItemIconAtlas" : "ItemIconAtlasGreyscale";
                ((XUiV_Sprite)this.itemIcon.ViewComponent).Color = flag ? Color.white : new Color(1f, 1f, 1f, 0.7f);
                string str = string.Empty;
                if(flag)
                {
                    str = itemClass.GetLocalizedItemName();
                }

                this.ViewComponent.ToolTip = flag ? str : this.emptyTooltipName;
            }

            if(itemClass != null)
            {
                ((XUiV_Sprite)this.itemIcon.ViewComponent).Color = this.itemStack.itemValue.ItemClass.GetIconTint(this.itemStack.itemValue);
                if(itemClass.ShowQualityBar)
                {
                    if(this.durability != null)
                    {
                        this.durability.ViewComponent.IsVisible = true;
                        this.durabilityBackground.ViewComponent.IsVisible = true;
                        XUiV_Sprite viewComponent = (XUiV_Sprite)this.durability.ViewComponent;
                        viewComponent.Color = QualityInfo.GetQualityColor(this.itemValue.Quality);
                        viewComponent.Fill = this.itemValue.PercentUsesLeft;
                    }

                    if(this.stackValue != null)
                    {
                        XUiV_Label viewComponent = (XUiV_Label)this.stackValue.ViewComponent;
                        viewComponent.Alignment = NGUIText.Alignment.Center;
                        viewComponent.Text = this.itemStack.itemValue.Quality > 0 ? this.itemStack.itemValue.Quality.ToString() : (this.itemStack.itemValue.IsMod ? "*" : "");
                    }
                }
                else if(this.durability != null)
                {
                    this.durability.ViewComponent.IsVisible = false;
                    this.durabilityBackground.ViewComponent.IsVisible = false;
                }

                if(this.lockTypeIcon != null)
                {
                    if(this.itemStack.itemValue.HasMods())
                    {
                        (this.lockTypeIcon.ViewComponent as XUiV_Sprite).SpriteName = "ui_game_symbol_modded";
                    }
                    else
                    {
                        (this.lockTypeIcon.ViewComponent as XUiV_Sprite).SpriteName = "";
                    }
                }
            }
            else
            {
                if(this.durability != null)
                {
                    this.durability.ViewComponent.IsVisible = false;
                }

                if(this.durabilityBackground != null)
                {
                    this.durabilityBackground.ViewComponent.IsVisible = false;
                }

                if(this.stackValue != null)
                {
                    ((XUiV_Label)this.stackValue.ViewComponent).Text = "";
                }

                if(this.lockTypeIcon != null)
                {
                    (this.lockTypeIcon.ViewComponent as XUiV_Sprite).SpriteName = "";
                }
            }

            this.isDirty = false;
        }

      ((XUiV_Label)this.stackValue.ViewComponent).Alignment = this.itemStack.itemValue.HasQuality || this.itemStack.itemValue.Modifications.Length != 0 ? NGUIText.Alignment.Center : NGUIText.Alignment.Right;
    }

    private void HandleItemInspect()
    {
        if(!this.ItemStack.IsEmpty() && this.InfoWindow != null)
        {
            this.Selected = true;
            this.InfoWindow.SetItemStack(this, true);
        }

        this.HandleClickComplete();
    }

    private void HandleStackSwap()
    {
        //Log.Out("StackTrace: '{0}'", Environment.StackTrace);
        //Log.Out("XUiC_RepairableVehicleStack-HandleStackSwap START");
        ItemClass itemClass = this.xui.dragAndDrop.CurrentStack.itemValue.ItemClass;
        if(itemClass == null)
        {
            //Log.Out("XUiC_RepairableVehicleStack-HandleStackSwap 1");
            return;
        }

        RepairableVehicleSlotsEnum slotEnum = RepairableVehicleSlotsEnum.None;
        if(itemClass.Properties.Values.ContainsKey("VehicleSlot"))
        {
            //Log.Out("XUiC_RepairableVehicleStack-HandleStackSwap 2");
            slotEnum = (RepairableVehicleSlotsEnum)Enum.Parse(typeof(RepairableVehicleSlotsEnum), itemClass.Properties.Values["VehicleSlot"]);
        }
        else
        {
            //Log.Out("XUiC_RepairableVehicleStack-HandleStackSwap 3");
            return;
        }

        for(int index = 0; index < this.SlotIndices.Count; ++index)
        {
            if(RepairableVehicleSlots == slotEnum)
            {
                //Log.Out("XUiC_RepairableVehicleStack-HandleStackSwap 4");
                this.SwapItem();
                this.Selected = false;
            }
        }

        this.Selected = false;
        this.HandleClickComplete();
    }

    private void HandleClickComplete()
    {
        this.lastClicked = false;
        if(!this.itemValue.IsEmpty())
        {
            return;
        }

        this.SetBorderColor(this.normalBorderColor);
    }

    public override void OnHovered(bool _isOver)
    {
        this.isOver = _isOver;
        if(_isOver)
        {
            if(!this.Selected)
            {
                this.SetBorderColor(this.hoverBorderColor);
            }

            if(!this.itemStack.IsEmpty())
            {
                this.tweenScale.from = Vector3.one;
                this.tweenScale.to = Vector3.one * 1.5f;
                this.tweenScale.enabled = true;
                this.tweenScale.duration = 0.5f;
            }
        }
        else
        {
            if(!this.Selected)
            {
                this.SetBorderColor(this.normalBorderColor);
            }

            this.tweenScale.from = Vector3.one * 1.5f;
            this.tweenScale.to = Vector3.one;
            this.tweenScale.enabled = true;
            this.tweenScale.duration = 0.5f;
        }

        if(!_isOver && this.tweenScale.value != Vector3.one && !this.itemStack.IsEmpty())
        {
            this.tweenScale.from = Vector3.one * 1.5f;
            this.tweenScale.to = Vector3.one;
            this.tweenScale.enabled = true;
            this.tweenScale.duration = 0.5f;
        }

        base.OnHovered(_isOver);
    }

    private void SwapItem()
    {
        ItemStack currentStack = this.xui.dragAndDrop.CurrentStack;

        if (this.itemStack.IsEmpty())
        {
            if(placeSound != null)
            {
                Manager.PlayXUiSound(this.placeSound, 0.75f);
            }
        }
        else if(pickupSound != null)
        {
            Manager.PlayXUiSound(this.pickupSound, 0.75f);
        }

        this.xui.dragAndDrop.CurrentStack = this.itemStack.Clone();
        this.ItemStack = currentStack.Clone();
        if(this.SlotChangedEvent == null)
        {
            return;
        }

        this.SlotChangedEvent(this.SlotNumber, this.itemStack);

        ItemValue[] TEItems = null;

        //Log.Out("XUiC_RepairableVehicleStack-SwapItem windowType: " + windowType);

        if (windowType == WindowTypeEnum.BlockRepairableVehicle)
        {
            //Log.Out("XUiC_RepairableVehicleStack-SwapItem BlockRepairableVehicle");

            TileEntityDriveableLootContainer tileEntity = GetParentByType<XUiC_RepairableVehicleWindow>().tileEntity;
            TEItems = tileEntity.GetRepairableVehicleParts();

            TEItems[SlotNumber] = ItemStack.itemValue;
            tileEntity.SetRepairableVehicleParts(TEItems);

            float increase = 0;

            List<RepairableVehicleSlotsEnum> itemValueEnums = RebirthVariables.localVehicleTypes[tileEntity.blockValue.Block.Properties.Values["VehicleType"]];

            string itemName = "";

            foreach (ItemValue item in TEItems)
            {
                if (item.type != ItemValue.None.type)
                {
                    itemName = item.ItemClass.GetItemName();
                    foreach (RepairableVehicleSlotsEnum part in itemValueEnums)
                    {
                        if (RebirthVariables.localVehicleParts[part].itemName == itemName)
                        {
                            //increase += ((item.MaxUseTimes - item.UseTimes) / item.MaxUseTimes) * RebirthVariables.localVehicleParts[part].durabilityPerQuality * item.Quality;
                            /*//Log.Out("XUiC_RepairableVehicleWindow-SwapItem itemName: " + itemName);
                            //Log.Out("XUiC_RepairableVehicleWindow-SwapItem PercUsed: " + ((item.MaxUseTimes - item.UseTimes) / item.MaxUseTimes));
                            //Log.Out("XUiC_RepairableVehicleWindow-SwapItem Quality: " + item.Quality);
                            //Log.Out("XUiC_RepairableVehicleWindow-SwapItem durabilityPerQuality: " + RebirthVariables.localVehicleParts[part].durabilityPerQuality);
                            //Log.Out("XUiC_RepairableVehicleWindow-SwapItem durability: " + durability);*/
                            break;
                        }
                    }
                }
            }

            tileEntity.Durability = tileEntity.baseDurability + increase;
            tileEntity.vehicleHealth = tileEntity.baseVehicleHealth + increase;

            tileEntity.SetModified();

            XUiC_RepairableVehicleWindow window = GetParentByType<XUiC_RepairableVehicleWindow>();

            if (window != null)
            {
                window.RefreshBindings(true);
            }
        }
        else if (windowType == WindowTypeEnum.EntityVehicle)
        {
            //Log.Out("XUiC_RepairableVehicleStack-SwapItem EntityVehicle");

            EntityVehicleRebirth vehicle = GetParentByType<XUiC_VehicleFrameWindowRebirth>().Vehicle;
            TEItems = vehicle.itemValues;

            ItemValue tempItemValue = TEItems[SlotNumber];

            TEItems[SlotNumber] = ItemStack.itemValue;
            vehicle.itemValues = TEItems;

            float increase = 0;
            string itemName = "";

            List<RepairableVehicleSlotsEnum> itemValueEnums = RebirthVariables.localVehicleTypes[vehicle.EntityClass.Properties.Values["VehicleType"]];

            foreach (ItemValue item in TEItems)
            {
                if (item.type != ItemValue.None.type)
                {
                    itemName = item.ItemClass.GetItemName();
                    foreach (RepairableVehicleSlotsEnum part in itemValueEnums)
                    {
                        if (RebirthVariables.localVehicleParts[part].itemName == itemName)
                        {
                            //increase += ((item.MaxUseTimes - item.UseTimes) / item.MaxUseTimes) * RebirthVariables.localVehicleParts[part].durabilityPerQuality * item.Quality;
                            /*//Log.Out("XUiC_RepairableVehicleWindow-SwapItem itemName: " + itemName);
                            //Log.Out("XUiC_RepairableVehicleWindow-SwapItem PercUsed: " + ((item.MaxUseTimes - item.UseTimes) / item.MaxUseTimes));
                            //Log.Out("XUiC_RepairableVehicleWindow-SwapItem Quality: " + item.Quality);
                            //Log.Out("XUiC_RepairableVehicleWindow-SwapItem durabilityPerQuality: " + RebirthVariables.localVehicleParts[part].durabilityPerQuality);
                            //Log.Out("XUiC_RepairableVehicleWindow-SwapItem durability: " + durability);*/
                            break;
                        }
                    }
                }
            }

            float change = 0;

            if (this.itemStack.IsEmpty())
            {
                itemName = tempItemValue.ItemClass.GetItemName();
                foreach (RepairableVehicleSlotsEnum part in itemValueEnums)
                {
                    if (RebirthVariables.localVehicleParts[part].itemName == itemName)
                    {
                        //change = ((tempItemValue.MaxUseTimes - tempItemValue.UseTimes) / tempItemValue.MaxUseTimes) * RebirthVariables.localVehicleParts[part].durabilityPerQuality * tempItemValue.Quality;
                        break;
                    }
                }

                vehicle.Health = vehicle.Health - (int)change;
                vehicle.maxBodyDurability = vehicle.maxBodyDurability - (int)change;
                vehicle.bodyDurability = vehicle.bodyDurability - (int)change;
            }
            else
            {
                //Log.Out("ItemStack: " + ItemStack.itemValue.ItemClass.GetItemName());

                itemName = this.itemStack.itemValue.ItemClass.GetItemName();
                foreach (RepairableVehicleSlotsEnum part in itemValueEnums)
                {
                    if (RebirthVariables.localVehicleParts[part].itemName == itemName)
                    {
                        //change = ((this.itemStack.itemValue.MaxUseTimes - this.itemStack.itemValue.UseTimes) / this.itemStack.itemValue.MaxUseTimes) * RebirthVariables.localVehicleParts[part].durabilityPerQuality * this.itemStack.itemValue.Quality;
                        break;
                    }
                }

                vehicle.Health = vehicle.Health + (int)change;
                vehicle.maxBodyDurability = vehicle.maxBodyDurability + (int)change;
                vehicle.bodyDurability = vehicle.bodyDurability + (int)change;
            }

            if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            {
                int playerID = this.xui.playerUI.entityPlayer.entityId;
                int currentVehicleID = vehicle.entityId;
                float currentItemUseTimes = this.itemStack.itemValue.UseTimes;
                string currentItemName = "";

                if (this.itemStack.itemValue.type != ItemValue.None.type)
                {
                    currentItemName = this.itemStack.itemValue.ItemClass.GetItemName();
                }

                //Log.Out("XUiC_RepairableVehicleWindow-SwapItem currentItemName: " + currentItemName);
                //Log.Out("XUiC_RepairableVehicleWindow-SwapItem Degradation: " + currentItemUseTimes + "/" + this.itemStack.itemValue.MaxUseTimes);
                //Log.Out("XUiC_RepairableVehicleWindow-SwapItem Quality: " + this.itemStack.itemValue.Quality);
                //Log.Out("XUiC_RepairableVehicleWindow-SwapItem SlotNumber: " + SlotNumber);

                NetPackageVehicleUpdatePartRebirth package = NetPackageManager.GetPackage<NetPackageVehicleUpdatePartRebirth>().Setup(playerID, currentVehicleID, SlotNumber, currentItemUseTimes, this.itemStack.itemValue.Quality, currentItemName);
                SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package, false);
            }

            //Log.Out("XUiC_RepairableVehicleWindow-SwapItem vehicle.baseBodyDurability: " + vehicle.baseBodyDurability);
            //Log.Out("XUiC_RepairableVehicleWindow-SwapItem vehicle.bodyDurability: " + vehicle.bodyDurability);
            //Log.Out("XUiC_RepairableVehicleWindow-SwapItem increase: " + increase);

            XUiC_VehicleFrameWindowRebirth window = GetParentByType<XUiC_VehicleFrameWindowRebirth>();

            if (window != null)
            {
                window.RefreshBindings(true);
            }
        }
    }

    public void HandleMoveToPreferredLocation()
    {
        //Log.Out("XUiC_RepairableVehicleWindow-HandleMoveToPreferredLocation START");
        ItemValue[] TEItems = null; 
        
        ItemStack _itemStack = this.ItemStack.Clone();
        if(this.xui.PlayerInventory.AddItemToBackpack(_itemStack))
        {
            //Log.Out("XUiC_RepairableVehicleWindow-HandleMoveToPreferredLocation 1");
            this.ItemValue = ItemStack.Empty.itemValue.Clone();
            if(placeSound != null)
            {
                //Log.Out("XUiC_RepairableVehicleWindow-HandleMoveToPreferredLocation 2");
                Manager.PlayXUiSound(this.placeSound, 0.75f);
            }

            this.SlotChangedEvent?.Invoke(this.SlotNumber, this.itemStack);

            if (windowType == WindowTypeEnum.BlockRepairableVehicle)
            {
                //Log.Out("XUiC_RepairableVehicleWindow-HandleMoveToPreferredLocation 3");
                TileEntityDriveableLootContainer tileEntity = GetParentByType<XUiC_RepairableVehicleWindow>().tileEntity;
                TEItems = tileEntity.GetRepairableVehicleParts();
                TEItems[SlotNumber] = this.itemValue;
                tileEntity.SetRepairableVehicleParts(TEItems);
                tileEntity.SetModified();
                this.ItemStack = ItemStack.Empty;


            }
            else if (windowType == WindowTypeEnum.EntityVehicle)
            {
                //Log.Out("XUiC_RepairableVehicleWindow-HandleMoveToPreferredLocation 4");
                EntityVehicleRebirth vehicle = GetParentByType<XUiC_VehicleFrameWindowRebirth>().Vehicle;
                TEItems = vehicle.itemValues;
                TEItems[SlotNumber] = ItemStack.itemValue;
                vehicle.itemValues = TEItems;
                this.ItemStack = ItemStack.Empty;

                if (!this.itemStack.IsEmpty() || !this.Selected)
                {
                    //Log.Out("XUiC_RepairableVehicleWindow-HandleMoveToPreferredLocation 5");

                    if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
                    {
                        int playerID = this.xui.playerUI.entityPlayer.entityId;
                        int currentVehicleID = vehicle.entityId;
                        float currentItemUseTimes = this.itemStack.itemValue.UseTimes;
                        string currentItemName = "";

                        if (this.itemStack.itemValue.type != ItemValue.None.type)
                        {
                            currentItemName = this.itemStack.itemValue.ItemClass.GetItemName();
                        }

                        //Log.Out("XUiC_RepairableVehicleWindow-SwapItem currentItemName: " + currentItemName);
                        //Log.Out("XUiC_RepairableVehicleWindow-SwapItem Degradation: " + currentItemUseTimes + "/" + this.itemStack.itemValue.MaxUseTimes);
                        //Log.Out("XUiC_RepairableVehicleWindow-SwapItem Quality: " + this.itemStack.itemValue.Quality);
                        //Log.Out("XUiC_RepairableVehicleWindow-SwapItem SlotNumber: " + SlotNumber);

                        NetPackageVehicleUpdatePartRebirth package = NetPackageManager.GetPackage<NetPackageVehicleUpdatePartRebirth>().Setup(playerID, currentVehicleID, SlotNumber, currentItemUseTimes, this.itemStack.itemValue.Quality, currentItemName);
                        SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package, false);
                    }

                    return;
                }
            }

            this.Selected = false;
        }
        else
        {
            if(!this.xui.PlayerInventory.AddItemToToolbelt(_itemStack))
            {
                //Log.Out("XUiC_RepairableVehicleWindow-HandleMoveToPreferredLocation 6");
                return;
            }

            this.ItemValue = ItemStack.Empty.itemValue.Clone();
            if(placeSound != null)
            {
                //Log.Out("XUiC_RepairableVehicleWindow-HandleMoveToPreferredLocation 7");
                Manager.PlayXUiSound(this.placeSound, 0.75f);
            }

            this.SlotChangedEvent?.Invoke(this.SlotNumber, this.itemStack);

            if (windowType == WindowTypeEnum.BlockRepairableVehicle)
            {
                //Log.Out("XUiC_RepairableVehicleWindow-HandleMoveToPreferredLocation 8");
                TileEntityDriveableLootContainer tileEntity = GetParentByType<XUiC_RepairableVehicleWindow>().tileEntity;
                TEItems = tileEntity.GetRepairableVehicleParts();
                TEItems[SlotNumber] = ItemValue;
                tileEntity.SetRepairableVehicleParts(TEItems);
                tileEntity.SetModified();
            }
            else if (windowType == WindowTypeEnum.EntityVehicle)
            {
                //Log.Out("XUiC_RepairableVehicleWindow-HandleMoveToPreferredLocation 9");
                EntityVehicleRebirth vehicle = GetParentByType<XUiC_VehicleFrameWindowRebirth>().Vehicle;
                TEItems = vehicle.itemValues;
                TEItems[SlotNumber] = ItemStack.itemValue;
                vehicle.itemValues = TEItems;
            }

            if (!this.itemStack.IsEmpty() || !this.Selected)
            {
                //Log.Out("XUiC_RepairableVehicleWindow-HandleMoveToPreferredLocation 10");
                return;
            }

            this.Selected = false;
        }
    }

    public override bool ParseAttribute(string name, string value, XUiController _parent)
    {
        bool attribute = base.ParseAttribute(name, value, _parent);
        if(attribute)
        {
            return attribute;
        }

        switch(name)
        {
            case "hover_border_color":
                this.hoverBorderColor = (Color32)StringParsers.ParseColor32(value);
                break;
            case "hover_icon_grow":
                this.HoverIconGrow = StringParsers.ParseFloat(value);
                break;
            case "normal_background_color":
                XUiC_RepairableVehicleStack.finalPressedColor = (Color32)StringParsers.ParseColor32(value);
                break;
            case "normal_border_color":
                this.normalBorderColor = (Color32)StringParsers.ParseColor32(value);
                break;
            case "normal_color":
                this.normalBackgroundColor = (Color32)StringParsers.ParseColor32(value);
                break;
            case "pickup_sound":
                this.xui.LoadData<AudioClip>(value, o => this.pickupSound = o);
                break;
            case "place_sound":
                this.xui.LoadData<AudioClip>(value, o => this.placeSound = o);
                break;
            case "selected_border_color":
                this.selectedBorderColor = (Color32)StringParsers.ParseColor32(value);
                break;
            default:
                return false;
        }

        return true;
    }

    public override void OnOpen()
    {
        base.OnOpen();
        this.isDirty = true;
    }
}
