using System;
using System.Collections;
using System.Collections.Generic;

using Audio;

using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Scripting;
using static vp_Timer;

[Preserve]
public class XUiC_RepairableVehicleWindow : XUiController
{
    private XUiV_Button btnSiphon_Background;
    private readonly XUiV_Button btnRepair_Background;
    private XUiV_Button btnHotwire_Background;

    public override void Init()
    {
        base.Init();
        XUiController childById = base.GetChildById("btnSiphon");
        if(childById != null)
        {
            this.btnSiphon_Background = (XUiV_Button)childById.GetChildById("clickable").ViewComponent;
            this.btnSiphon_Background.Controller.OnPress += this.BtnSiphon_OnPress;
        }

        XUiController childById2 = base.GetChildById("btnHotwire");
        if(childById2 != null)
        {
            this.btnHotwire_Background = (XUiV_Button)childById2.GetChildById("clickable").ViewComponent;
            this.btnHotwire_Background.Controller.OnPress += this.BtnHotwire_OnPress;
        }

        XUiController childById3 = base.GetChildById("btnRepair");
        if(childById3 != null)
        {
            this.btnHotwire_Background = (XUiV_Button)childById3.GetChildById("clickable").ViewComponent;
            this.btnHotwire_Background.Controller.OnPress += this.BtnRepair_OnPress;
        }
    }

    private void BtnSiphon_OnPress(XUiController _sender, int _mouseButton)
    {
        ItemStack gas = new ItemStack(ItemClass.GetItem("ammoGasCan"), Mathf.RoundToInt(tileEntity.GasPerc * 500));
        LocalPlayerUI.GetUIForPlayer(_sender.xui.playerUI.entityPlayer).entityPlayer.AddUIHarvestingItem(gas, true);
        _sender.xui.playerUI.entityPlayer.inventory.AddItem(gas);
        this.tileEntity.GasPerc = 0f;
        base.RefreshBindings(true);
        IsDirty = true;
    }

    private void BtnHotwire_OnPress(XUiController _sender, int _mouseButton)
    {
        base.RefreshBindings(false);
        Block block = null;
        bool foundEmpty = false;
        ItemValue[] itemValues = { ItemValue.None };

        if(tileEntity != null)
        {
            //Log.Out("XUiC_RepairableVehicleWindow-BtnHotwire_OnPress tileEntity != null");

            block = tileEntity.blockValue.Block;
            itemValues = tileEntity.itemValues;
            Vector3i blockPos = tileEntity.ToWorldPos();

            if(itemValues != null)
            {
                //Log.Out("XUiC_RepairableVehicleWindow-BtnHotwire_OnPress itemValues != null");
                foundEmpty = false;

                foreach(ItemValue item in itemValues)
                {
                    if(item == null || item == ItemValue.None || item.ItemClass == null)
                    {
                        //Log.Warning($"Contains: NULL OR EMPTY! {item}");
                        foundEmpty = true;
                    }
                    else
                    {
                        //Log.Out("XUiC_RepairableVehicleWindow-BtnHotwire_OnPress item: " + item.ItemClass.GetItemName());
                        Log.Warning($"Contains: {item.ItemClass.Name}");
                    }
                }
            }
            else
            {
                //Log.Out("XUiC_RepairableVehicleWindow-BtnHotwire_OnPress itemValues == null");
                foundEmpty = true;
            }

            if(!foundEmpty)
            {
                if(RebirthUtilities.useInventoryBagItem(_sender.xui.playerUI.entityPlayer, "HotwireKit", 1))
                {
                    Manager.PlayInsidePlayerHead("crafting/craft_repair_item", -1, 0f, false);

                    float durability = tileEntity.vehicleHealth; // tileEntity.baseDurability;
                    float maxDurability = tileEntity.vehicleHealth;

                    List<RepairableVehicleSlotsEnum> itemValueEnums = RebirthVariables.localVehicleTypes[block.Properties.Values["VehicleType"]];

                    foreach (ItemValue item in itemValues)
                    {
                        if (item != ItemValue.None)
                        {
                            string itemName = item.ItemClass.GetItemName();
                            foreach (RepairableVehicleSlotsEnum part in itemValueEnums)
                            {
                                if (RebirthVariables.localVehicleParts[part].itemName == itemName)
                                {
                                    //durability += ((item.MaxUseTimes - item.UseTimes) / item.MaxUseTimes) * RebirthVariables.localVehicleParts[part].durabilityPerQuality * item.Quality;
                                    /*Log.Out("XUiC_RepairableVehicleWindow-BtnHotwire_OnPress itemName: " + itemName);
                                    Log.Out("XUiC_RepairableVehicleWindow-BtnHotwire_OnPress PercUsed: " + ((item.MaxUseTimes - item.UseTimes) / item.MaxUseTimes));
                                    Log.Out("XUiC_RepairableVehicleWindow-BtnHotwire_OnPress Quality: " + item.Quality);
                                    Log.Out("XUiC_RepairableVehicleWindow-BtnHotwire_OnPress durabilityPerQuality: " + RebirthVariables.localVehicleParts[part].durabilityPerQuality);
                                    Log.Out("XUiC_RepairableVehicleWindow-BtnHotwire_OnPress durability: " + durability);*/
                                    break;
                                }
                            }
                        }
                    }
                    //Log.Out("XUiC_RepairableVehicleWindow-BtnHotwire_OnPress blockPos: " + blockPos);
                    //Log.Out("XUiC_RepairableVehicleWindow-BtnHotwire_OnPress isMultiBlock: " + block.isMultiBlock);
                    //Log.Out("XUiC_RepairableVehicleWindow-BtnHotwire_OnPress tileEntity.blockValue.ischild: " + tileEntity.blockValue.ischild);

                    //Log.Out("XUiC_RepairableVehicleWindow-BtnHotwire_OnPress tileEntity.vehicleHealth: " + tileEntity.vehicleHealth);
                    //Log.Out("XUiC_RepairableVehicleWindow-BtnHotwire_OnPress tileEntity.maxDurability: " + tileEntity.maxDurability);

                    if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
                    {
                        SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageSpawnVehicleRebirth>().Setup(block.Properties.Values["vehicle_entity_class"],
                                                                                                                                                            tileEntity.itemValues,
                                                                                                                                                            blockPos.x,
                                                                                                                                                            blockPos.y,
                                                                                                                                                            blockPos.z,
                                                                                                                                                            durability,
                                                                                                                                                            _sender.xui.playerUI.entityPlayer.entityId
                                                                                                                                                            ));
                    }
                    else
                    {
                        GameManager.Instance.StartCoroutine(RebirthUtilities.SpawnVehicleCoroutine(
                                                                                blockPos,
                                                                                tileEntity.itemValues,
                                                                                tileEntity.vehicleHealth,
                                                                                tileEntity.GasPerc,
                                                                                tileEntity.OilPerc,
                                                                                tileEntity.vehicleHealth,
                                                                                tileEntity.vehicleHealth,
                                                                                RebirthUtilities.GetBlockAngle(tileEntity.blockValue.rotation),
                                                                                block.Properties.Values["vehicle_entity_class"],
                                                                                _sender.xui.playerUI.entityPlayer.entityId
                                                                                ));
                    }

                    OnClose();
                }

                //else
                //{
                //    XUiC_PopupToolTip.QueueTooltip(_sender.xui, "Missing HotwireKit!", null, "misc/missingitemtorepair", new ToolTipEvent());
                //}
            }
            else
            {
                XUiC_PopupToolTip.QueueTooltip(_sender.xui, Localization.Get("ttMissingOrDamagedParts"), null, "misc/missingitemtorepair", new ToolTipEvent());
            }
        }

        else
        {
            Log.Error("onHotwire tileEntity is null!");
        }
    }

    private void BtnRepair_OnPress(XUiController _sender, int _mouseButton)
    {
        base.RefreshBindings(false);
        Block block = null;
        bool foundEmpty = false;
        ItemValue[] itemValues = { ItemValue.None };

        if(tileEntity != null)
        {
            block = tileEntity.blockValue.Block;
            itemValues = tileEntity.itemValues;
            Vector3i blockPos = tileEntity.ToWorldPos();

            if(itemValues != null)
            {
                foundEmpty = false;

                foreach(ItemValue item in itemValues)
                {
                    if(item == null || item == ItemValue.None || item.ItemClass == null)
                    {
                        Log.Warning($"Contains: NULL OR EMPTY! {item}");
                        foundEmpty = true;

                    }

                    else
                    {
                        Log.Warning($"Contains: {item.ItemClass.Name}");
                    }
                }
            }

            else
            {
                foundEmpty = true;
            }

            if(!foundEmpty)
            {
                if(RebirthUtilities.useInventoryBagItem(_sender.xui.playerUI.entityPlayer, "FuriousRamsayBikeRepairKit", 1))
                {
                    Manager.PlayInsidePlayerHead("crafting/craft_repair_item", -1, 0f, false);

                    Chunk chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(blockPos);

                    float gasPerc = tileEntity.GasPerc;
                    float durability = tileEntity.Durability;

                    List<RepairableVehicleSlotsEnum> itemValueEnums = RebirthVariables.localVehicleTypes[block.Properties.Values["VehicleType"]];

                    //Log.Out("XUiC_RepairableVehicleWindow-BtnRepair_OnPress INITIAL durability: " + durability);

                    foreach (ItemValue item in itemValues)
                    {
                        if (item != ItemValue.None)
                        {
                            string itemName = item.ItemClass.GetItemName();
                            foreach (RepairableVehicleSlotsEnum part in itemValueEnums)
                            {
                                if (RebirthVariables.localVehicleParts[part].itemName == itemName)
                                {
                                    //durability += ((item.MaxUseTimes - item.UseTimes) / item.MaxUseTimes) * RebirthVariables.localVehicleParts[part].durabilityPerQuality * item.Quality;
                                    /*Log.Out("XUiC_RepairableVehicleWindow-BtnRepair_OnPress itemName: " + itemName);
                                    Log.Out("XUiC_RepairableVehicleWindow-BtnRepair_OnPress PercUsed: " + ((item.MaxUseTimes - item.UseTimes) / item.MaxUseTimes));
                                    Log.Out("XUiC_RepairableVehicleWindow-BtnRepair_OnPress Quality: " + item.Quality);
                                    Log.Out("XUiC_RepairableVehicleWindow-BtnRepair_OnPress durabilityPerQuality: " + RebirthVariables.localVehicleParts[part].durabilityPerQuality);
                                    Log.Out("XUiC_RepairableVehicleWindow-BtnRepair_OnPress durability: " + durability);*/
                                    break;
                                }
                            }
                        }
                    }

                    //Log.Out("XUiC_RepairableVehicleWindow-BtnRepair_OnPress TOTAL durability: " + durability);

                    if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
                    {
                        SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageSpawnVehicleRebirth>().Setup(block.Properties.Values["vehicle_entity_class"],
                                                                                                                                                            tileEntity.itemValues,
                                                                                                                                                            blockPos.x,
                                                                                                                                                            blockPos.y,
                                                                                                                                                            blockPos.z,
                                                                                                                                                            durability,
                                                                                                                                                            _sender.xui.playerUI.entityPlayer.entityId
                                                                                                                                                            ));
                    }
                    else
                    {
                        /*GameManager.Instance.StartCoroutine(RebirthUtilities.SpawnVehicleCoroutine(
                                                                                blockPos,
                                                                                tileEntity.itemValues,
                                                                                tileEntity.baseDurability,
                                                                                tileEntity.GasPerc,
                                                                                tileEntity.OilPerc,
                                                                                durability,
                                                                                tileEntity.maxDurability,
                                                                                RebirthUtilities.GetBlockAngle(tileEntity.blockValue.rotation),
                                                                                block.Properties.Values["vehicle_entity_class"],
                                                                                _sender.xui.playerUI.entityPlayer.entityId
                                                                                ));*/
                        GameManager.Instance.StartCoroutine(RebirthUtilities.SpawnVehicleCoroutine(
                                                                                blockPos,
                                                                                tileEntity.itemValues,
                                                                                tileEntity.vehicleHealth,
                                                                                tileEntity.GasPerc,
                                                                                tileEntity.OilPerc,
                                                                                tileEntity.vehicleHealth,
                                                                                tileEntity.vehicleHealth,
                                                                                RebirthUtilities.GetBlockAngle(tileEntity.blockValue.rotation),
                                                                                block.Properties.Values["vehicle_entity_class"],
                                                                                _sender.xui.playerUI.entityPlayer.entityId
                                                                                ));
                    }

                    OnClose();
                }
                else
                {
                    XUiC_PopupToolTip.QueueTooltip(_sender.xui, Localization.Get("ttMissingRepairKit"), null, "misc/missingitemtorepair", new ToolTipEvent());
                }
            }

            else
            {
                XUiC_PopupToolTip.QueueTooltip(_sender.xui, Localization.Get("ttMissingOrDamagedParts"), null, "misc/missingitemtorepair", new ToolTipEvent());
            }
        }
    }

    public override bool GetBindingValue(ref string value, string bindingName)
    {
        if (tileEntity != null)
        {
            EntityClass entityClass = null;
            string entityClassName = "";
            Block block = tileEntity.blockValue.Block;
            Dictionary<string, DynamicProperties> propertyMap = Vehicle.PropertyMap;
            float velocityMax = 0f;

            if (block is BlockVehicleRebirth)
            {
                BlockVehicleRebirth blockVehicle = (BlockVehicleRebirth)block;
                entityClassName = blockVehicle.vehicle_entity_class;
                var properties = propertyMap[entityClassName.ToLowerInvariant()];
                velocityMax = Convert.ToSingle(properties.Values["velocityMax_turbo"].Split(',')[2]);
                entityClass = EntityClass.GetEntityClass(EntityClass.FromString(entityClassName));
            }

            switch (bindingName)
            {
                case "oil":
                    value = this.fuelFormatter.Format(Mathf.RoundToInt(tileEntity.OilPerc * 100f));
                    return true;

                case "oiltitle":
                    value = Localization.Get("xuiOil");
                    return true;

                case "fuel":
                    value = this.fuelFormatter.Format(Mathf.RoundToInt(tileEntity.GasPerc * 100f));
                    return true;

                case "fueltitle":
                    value = Localization.Get("xuiGas");
                    return true;

                case "speed":
                    value = speedFormatter.Format(Mathf.RoundToInt(velocityMax));
                    return true;

                case "speedtext":
                    if(velocityMax <= 0f)
                    {
                        value = Localization.Get("xuiVehicleSpeedNone");
                    }
                    else if(velocityMax <= 10f)
                    {
                        value = Localization.Get("xuiVehicleSpeedSlow");
                    }
                    else if(velocityMax <= 20f)
                    {
                        value = Localization.Get("xuiVehicleSpeedNormal");
                    }
                    else
                    {
                        value = Localization.Get("xuiVehicleSpeedFast");
                    }

                    return true;

                case "speedtitle":
                    value = Localization.Get("xuiSpeed");
                    return true;

                case "bodydurability":

                    int health = (int)tileEntity.vehicleHealth;

                    value = vehicleDurabilityFormatter.Format(Mathf.RoundToInt(tileEntity.Durability), health);
                    return true;

                case "bodydurabilitytitle":
                    value = Localization.Get("xuiBodyDurability");
                    return true;

                case "vehicleicon":
                    if(entityClassName != "")
                    {
                        value = "ui_game_symbol_4x4";

                        if(entityClass != null)
                        {
                            if(entityClass.Properties.Values.ContainsKey("MapIcon"))
                            {
                                value = entityClass.Properties.Values["MapIcon"];
                            }
                        }
                    }
                    else
                    {
                        value = "ui_game_symbol_4x4";
                    }

                    return true;

                case "vehiclename":
                    if(entityClassName != "")
                    {
                        value = "Vehicle";

                        if(entityClass != null)
                        {
                            value = Localization.Get(entityClass.entityClassName);
                        }
                    }
                    else
                    {
                        value = "Vehicle";
                    }

                    return true;

                default:
                    return false;
            }
        }

        return true;
    }

    public override void Update(float _dt)
    {
        if(GameManager.Instance == null || GameManager.Instance.World == null)
        {
            return;
        }

        base.Update(_dt);
        if(this.windowGroup.isShowing)
        {
            if(!base.xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
            {
                this.wasReleased = true;
            }

            if(this.wasReleased)
            {
                if(base.xui.playerUI.playerInput.PermanentActions.Activate.IsPressed)
                {
                    this.activeKeyDown = true;
                }

                if(base.xui.playerUI.playerInput.PermanentActions.Activate.WasReleased && this.activeKeyDown)
                {
                    this.activeKeyDown = false;
                    base.xui.playerUI.windowManager.CloseAllOpenWindows(null, false);
                }
            }
        }
    }

    public void SetTileEntity(TileEntityDriveableLootContainer _te)
    {
        //Log.Out("XUiC_RepairableVehicleWindow-SetTileEntity START");
        this.tileEntity = _te;

        if(this.tileEntity != null)
        {
            base.RefreshBindings(true);
        }
    }

    public override void OnClose()
    {
        //Log.Out("XUiC_RepairableVehicleWindow-OnClose START");

        this.wasReleased = false;
        this.activeKeyDown = false;

        if(this.tileEntity != null)
        {
            Vector3i blockPos = this.tileEntity.ToWorldPos();

            if(SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && !GameManager.IsDedicatedServer)
            {
                RebirthUtilities.removeTileEntityAccess(blockPos);
            }
            else
            {
                if(SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
                {
                    SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageCloseTileEntityRebirth>().Setup(blockPos.x, blockPos.y, blockPos.z), false);
                }
            }

            this.tileEntity.SetUserAccessing(false);
            this.tileEntity.SetModified();
            this.SetTileEntity(null);
        }

        base.OnClose();
    }

    public TileEntityDriveableLootContainer tileEntity;
    private readonly bool isClosing;
    private bool activeKeyDown;
    private bool wasReleased;

    private readonly CachedStringFormatterInt speedFormatter = new CachedStringFormatterInt();
    private readonly CachedStringFormatterInt fuelFormatter = new CachedStringFormatterInt();
    private readonly CachedStringFormatter<int, int> vehicleDurabilityFormatter = new CachedStringFormatter<int, int>((int _i1, int _i2) => string.Format("{0}/{1}", _i1, _i2));
}
