﻿using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

using Random = UnityEngine.Random;

public static class SaveLoadManager
{
    private static readonly string SaveVersion = "1";


    public static void WriteSave(SaveCampaignData saveData)
    {
        try
        {
            RecreateSaveDirectory();

            using (var fs = new FileStream(GenerateSaveFileName(saveData.SaveId), FileMode.Create, FileAccess.Write))
            {
                using (var bw = new BinaryWriter(fs))
                {
                    #region Estate

                    bw.Write(SaveVersion);
                    bw.Write(saveData.IsFirstStart);
                    bw.Write(saveData.HamletTitle);
                    bw.Write(saveData.LocationName);
                    bw.Write(saveData.QuestsCompleted);
                    bw.Write(saveData.CurrentWeek);
                    bw.Write(saveData.SaveId);
                    
                    bw.Write(saveData.GoldAmount);
                    bw.Write(saveData.BustsAmount);
                    bw.Write(saveData.DeedsAmount);
                    bw.Write(saveData.PortraitsAmount);
                    bw.Write(saveData.CrestsAmount);
                    
                    saveData.RosterHeroes.Write(bw);
                    saveData.StageCoachHeroes.Write(bw);
                    saveData.StageEventHeroes.Write(bw);
                    saveData.DeathRecords.Write(bw);
                    saveData.DeathEventData.Write(bw);

                    saveData.InventoryTrinkets.Write(bw);
                    saveData.WagonTrinkets.Write(bw);
                    saveData.DungeonProgress.Write(bw);
                    saveData.BuildingUpgrades.Write(bw);
                    saveData.InstancedPurchases.Write(bw);
                    saveData.ActivityLog.Write(bw);

                    saveData.CompletedPlot.Write(bw);
                    saveData.GeneratedQuests.Write(bw);

                    saveData.AbbeyActivitySlots.Write(bw);
                    saveData.TavernActivitySlots.Write(bw);
                    saveData.SanitariumActivitySlots.Write(bw);

                    bw.Write(saveData.CurrentEvent ?? "");
                    bw.Write(saveData.GuaranteedEvent ?? "");
                    saveData.EventData.Write(bw);
                    saveData.EventModifers.Write(bw);

                    saveData.TownNarrations.Write(bw);
                    saveData.RaidNarrations.Write(bw);
                    saveData.CampaignNarrations.Write(bw);

                    #endregion

                    #region Raid

                    bw.Write(saveData.InRaid);
                    if (!saveData.InRaid)
                        return;

                    bw.Write(saveData.QuestCompleted);
                    saveData.Quest.Write(bw);
                   
                    #region Dungeon
                    bw.Write(saveData.Dungeon.Name);
                    bw.Write(saveData.Dungeon.GridSizeX);
                    bw.Write(saveData.Dungeon.GridSizeY);
                    bw.Write(saveData.Dungeon.StartingRoomId);

                    #region Rooms
                    bw.Write(saveData.Dungeon.Rooms.Count);
                    foreach(var room in saveData.Dungeon.Rooms.Values)
                    {
                        bw.Write(room.Id);
                        bw.Write(room.GridX);
                        bw.Write(room.GridY);
                        bw.Write(room.Doors.Count);
                        for(int j = 0; j < room.Doors.Count; j++)
                        {
                            bw.Write(room.Doors[j].TargetArea);
                            bw.Write((int)room.Doors[j].Direction);
                        }
                        bw.Write(room.TextureId);
                        bw.Write((int)room.Type);
                        bw.Write((int)room.Knowledge);
                        bw.Write(room.Prop != null ? true : false);
                        if(room.Prop != null)
                        {
                            bw.Write((int)room.Prop.Type);
                            switch(room.Prop.Type)
                            {
                                case AreaType.Door:
                                    var door = room.Prop as Door;
                                    bw.Write(door.TargetArea);
                                    bw.Write((int)door.Direction);
                                    break;
                                case AreaType.Curio:
                                    bw.Write((room.Prop as Curio).IsQuestCurio);
                                    bw.Write(room.Prop.StringId);
                                    break;
                                case AreaType.Obstacle:
                                case AreaType.Trap:
                                    bw.Write(room.Prop.StringId);
                                    break;
                            }
                        }
                        bw.Write(room.BattleEncounter != null ? true : false);
                        if (room.BattleEncounter != null)
                        {
                            bw.Write(room.BattleEncounter.Monsters.Count);
                            for (int j = 0; j < room.BattleEncounter.Monsters.Count; j++)
                                bw.Write(room.BattleEncounter.Monsters[j].Data.StringId);
                            bw.Write(room.BattleEncounter.Cleared);
                        }
                    }
                    #endregion

                    #region Hallways
                    bw.Write(saveData.Dungeon.Hallways.Count);
                    foreach (var hallway in saveData.Dungeon.Hallways.Values)
                    {
                        bw.Write(hallway.Id);
                        bw.Write(hallway.RoomA.Id);
                        bw.Write(hallway.RoomB.Id);
                        bw.Write(hallway.Halls.Count);
                        for (int j = 0; j < hallway.Halls.Count; j++)
                        {
                            bw.Write(hallway.Halls[j].Id);
                            bw.Write(hallway.Halls[j].GridX);
                            bw.Write(hallway.Halls[j].GridY);
                            bw.Write(hallway.Halls[j].TextureId);
                            bw.Write((int)hallway.Halls[j].Type);
                            bw.Write((int)hallway.Halls[j].Knowledge);
                            bw.Write(hallway.Halls[j].Prop != null ? true : false);
                            if (hallway.Halls[j].Prop != null)
                            {
                                bw.Write((int)hallway.Halls[j].Prop.Type);
                                switch (hallway.Halls[j].Prop.Type)
                                {
                                    case AreaType.Door:
                                        var door = hallway.Halls[j].Prop as Door;
                                        bw.Write(door.TargetArea);
                                        bw.Write((int)door.Direction);
                                        break;
                                    case AreaType.Curio:
                                        bw.Write((hallway.Halls[j].Prop as Curio).IsQuestCurio);
                                        bw.Write(hallway.Halls[j].Prop.StringId);
                                        break;
                                    case AreaType.Obstacle:
                                    case AreaType.Trap:
                                        bw.Write(hallway.Halls[j].Prop.StringId);
                                        break;
                                }
                            }
                            bw.Write(hallway.Halls[j].BattleEncounter != null ? true : false);
                            if (hallway.Halls[j].BattleEncounter != null)
                            {
                                bw.Write(hallway.Halls[j].BattleEncounter.Monsters.Count);
                                for (int i = 0; i < hallway.Halls[j].BattleEncounter.Monsters.Count; i++)
                                    bw.Write(hallway.Halls[j].BattleEncounter.Monsters[i].Data.StringId);
                                bw.Write(hallway.Halls[j].BattleEncounter.Cleared);
                            }
                        }
                    }
                    #endregion

                    bw.Write(saveData.Dungeon.SharedMashExecutionIds.Count);
                    for (int i = 0; i < saveData.Dungeon.SharedMashExecutionIds.Count; i++)
                        bw.Write(saveData.Dungeon.SharedMashExecutionIds[i]);
                    #endregion

                    #region Raid Party
                    bw.Write(saveData.RaidParty.IsMovingLeft);
                    bw.Write(saveData.RaidParty.HeroInfo.Count);
                    for (int i = 0; i < saveData.RaidParty.HeroInfo.Count; i++)
                    {
                        bw.Write(saveData.RaidParty.HeroInfo[i].HeroRosterId);
                        bw.Write(saveData.RaidParty.HeroInfo[i].IsAlive);
                        if (saveData.RaidParty.HeroInfo[i].IsAlive == false)
                        {
                            bw.Write((int)saveData.RaidParty.HeroInfo[i].Factor);
                            bw.Write(saveData.RaidParty.HeroInfo[i].Killer);
                        }
                    }
                    #endregion

                    #region Raid Info
                    bw.Write((int)saveData.CampingPhase);
                    bw.Write(saveData.CampingTimeLeft);
                    bw.Write(saveData.NightAmbushReduced);
                    bw.Write(saveData.HungerCooldown);
                    bw.Write(saveData.AncestorTalk);

                    bw.Write(saveData.ExploredRoomCount);
                    bw.Write(saveData.CurrentLocation);
                    bw.Write(saveData.LastRoom);
                    bw.Write(saveData.PreviousLastSector);
                    bw.Write(saveData.LastSector);

                    bw.Write(saveData.KilledMonsters.Count);
                    for (int i = 0; i < saveData.KilledMonsters.Count; i++)
                        bw.Write(saveData.KilledMonsters[i]);

                    bw.Write(saveData.InvestigatedCurios.Count);
                    for (int i = 0; i < saveData.InvestigatedCurios.Count; i++)
                        bw.Write(saveData.InvestigatedCurios[i]);

                    bw.Write(saveData.TorchAmount);
                    bw.Write(saveData.MaxTorchAmount);
                    bw.Write(saveData.ModifiedMinTorch);
                    bw.Write(saveData.ModifiedMaxTorch);
                    #endregion

                    #region Hero Formation
                    saveData.HeroFormationData.WriteFormationData(bw);
                    #endregion

                    #region Inventory
                    bw.Write(saveData.InventoryItems.Count);
                    for (int i = 0; i < saveData.InventoryItems.Count; i++)
                    {
                        bw.Write(saveData.InventoryItems[i] != null);
                        if (saveData.InventoryItems[i] != null)
                        {
                            bw.Write(saveData.InventoryItems[i].Item.Id);
                            bw.Write(saveData.InventoryItems[i].Item.Type);
                            bw.Write(saveData.InventoryItems[i].Item.Amount);
                        }
                    }
                    #endregion

                    #region Battle
                    bw.Write(saveData.InBattle);
                    if (saveData.InBattle == false)
                        return;

                    saveData.BattleGroundSaveData.WriteBattlegroundData(bw);
                    #endregion
                    #endregion
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error while writing save slot " + saveData.SaveId + "! " + ex.Message +
                "Inner: " + ex.InnerException != null ? ex.InnerException.Message : "None");
        }
    }

    public static void DeleteSave(int slotId)
    {
        File.Delete(GenerateSaveFileName(slotId));
    }

    public static SaveCampaignData ReadSave(int slotId)
    {
        if (!File.Exists(GenerateSaveFileName(slotId)))
            return null;

        try
        {
            RecreateSaveDirectory();

            SaveCampaignData saveData = new SaveCampaignData();
            using (var fs = new FileStream(GenerateSaveFileName(slotId), FileMode.Open, FileAccess.Read))
            {
                using (var br = new BinaryReader(fs))
                {
                    #region Estate

                    if (br.ReadString() != SaveVersion)
                        throw new NotImplementedException("Updater for old save files is not implemented!");

                    saveData.IsFirstStart = br.ReadBoolean();
                    saveData.HamletTitle = br.ReadString();
                    saveData.LocationName = br.ReadString();
                    saveData.QuestsCompleted = br.ReadInt32();
                    saveData.CurrentWeek = br.ReadInt32();
                    saveData.SaveId = br.ReadInt32();

                    saveData.GoldAmount = br.ReadInt32();
                    saveData.BustsAmount = br.ReadInt32();
                    saveData.DeedsAmount = br.ReadInt32();
                    saveData.PortraitsAmount = br.ReadInt32();
                    saveData.CrestsAmount = br.ReadInt32();

                    saveData.RosterHeroes.Read(br);
                    saveData.StageCoachHeroes.Read(br);
                    saveData.StageEventHeroes.Read(br);
                    saveData.DeathRecords.Read(br);
                    saveData.DeathEventData.Read(br);

                    saveData.InventoryTrinkets.Read(br);
                    saveData.WagonTrinkets.Read(br);
                    saveData.DungeonProgress.Read(item => item.DungeonName, br);
                    saveData.BuildingUpgrades.Read(item => item.TreeId, br);
                    saveData.InstancedPurchases.Read(item => item.TreeId, br);
                    saveData.ActivityLog.Read(br);

                    saveData.CompletedPlot.Read(br);
                    saveData.GeneratedQuests.Read(br);

                    saveData.AbbeyActivitySlots.Read(br);
                    saveData.TavernActivitySlots.Read(br);
                    saveData.SanitariumActivitySlots.Read(br);

                    saveData.CurrentEvent = br.ReadString();
                    saveData.GuaranteedEvent = br.ReadString();
                    saveData.EventData.Read(br);
                    saveData.EventModifers.Read(br);

                    saveData.TownNarrations.Read(br);
                    saveData.RaidNarrations.Read(br);
                    saveData.CampaignNarrations.Read(br);

                    #endregion

                    #region Raid

                    saveData.InRaid = br.ReadBoolean();
                    if (!saveData.InRaid)
                        return saveData;

                    saveData.QuestCompleted = br.ReadBoolean();
                    saveData.Quest = BinarySaveDataHelper.Create<Quest>(br);

                    #region Dungeon
                    saveData.Dungeon = new Dungeon();
                    saveData.Dungeon.Name = br.ReadString();
                    saveData.Dungeon.GridSizeX = br.ReadInt32();
                    saveData.Dungeon.GridSizeY = br.ReadInt32();
                    saveData.Dungeon.StartingRoomId = br.ReadString();

                    #region Rooms
                    int roomCount = br.ReadInt32();
                    for (int i = 0; i < roomCount; i++)
                    {
                        var room = new DungeonRoom(br.ReadString(), br.ReadInt32(), br.ReadInt32());
                        int doorCount = br.ReadInt32();
                        for (int j = 0; j < doorCount; j++)
                        {
                            room.Doors.Add(new Door("to", br.ReadString(), (Direction)br.ReadInt32()));
                        }
                        room.TextureId = br.ReadString();
                        room.Type = (AreaType)br.ReadInt32();
                        room.Knowledge = (Knowledge)br.ReadInt32();
                        bool hasProp = br.ReadBoolean();
                        if (hasProp)
                        {
                            AreaType propType = (AreaType)br.ReadInt32();

                            switch (propType)
                            {
                                case AreaType.Door:
                                    room.Prop = new Door("to", br.ReadString(), (Direction)br.ReadInt32());
                                    break;
                                case AreaType.Curio:
                                    bool isQuestCurio = br.ReadBoolean();
                                    if (isQuestCurio)
                                    {
                                        if (saveData.Quest.Goal.Type == "activate")
                                        {
                                            if (saveData.Quest.Goal.QuestData is QuestActivateData)
                                            {
                                                var activateData = saveData.Quest.Goal.QuestData as QuestActivateData;
                                                var curio = new Curio(activateData.CurioName); br.ReadString();
                                                curio.IsQuestCurio = true;

                                                if (saveData.Quest.Goal.StartingItems.Count > 0)
                                                {
                                                    curio.ItemInteractions.Add(new ItemInteraction()
                                                    {
                                                        Chance = 1,
                                                        ItemId = saveData.Quest.Goal.StartingItems[0].Id,
                                                        ResultType = "loot",
                                                        Results = new List<CurioResult>(),
                                                    });
                                                }
                                                else
                                                {
                                                    curio.Results.Add(new CurioInteraction()
                                                    {
                                                        Chance = 1,
                                                        ResultType = "loot",
                                                        Results = new List<CurioResult>(),
                                                    });
                                                }
                                                room.Prop = curio;
                                            }
                                        }
                                        else if (saveData.Quest.Goal.Type == "gather")
                                        {
                                            if (saveData.Quest.Goal.QuestData is QuestGatherData)
                                            {
                                                var gatherData = saveData.Quest.Goal.QuestData as QuestGatherData;
                                                var curio = new Curio(gatherData.CurioName); br.ReadString();
                                                curio.IsQuestCurio = true;

                                                var curioInteraction = new CurioInteraction();
                                                curioInteraction.Chance = 1;
                                                curioInteraction.ResultType = "loot";
                                                curioInteraction.Results = new List<CurioResult>();
                                                curioInteraction.Results.Add(new CurioResult()
                                                {
                                                    Chance = 1,
                                                    Draws = 1,
                                                    Item = gatherData.Item.Id,
                                                });
                                                curio.Results.Add(curioInteraction);
                                                room.Prop = curio;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Curio newCurio = DarkestDungeonManager.Data.Curios[br.ReadString()];
                                        newCurio.IsQuestCurio = isQuestCurio;
                                        room.Prop = newCurio;
                                    }
                                    break;
                                case AreaType.Obstacle:
                                    room.Prop = DarkestDungeonManager.Data.Obstacles[br.ReadString()];
                                    break;
                                case AreaType.Trap:
                                    room.Prop = DarkestDungeonManager.Data.Traps[br.ReadString()];
                                    break;
                            }
                        }
                        bool hasBattle = br.ReadBoolean();
                        if (hasBattle)
                        {
                            room.BattleEncounter = new BattleEncounter();
                            int monsterCount = br.ReadInt32();
                            for (int j = 0; j < monsterCount; j++)
                                room.BattleEncounter.Monsters.Add(new Monster(DarkestDungeonManager.Data.Monsters[br.ReadString()]));
                            room.BattleEncounter.Cleared = br.ReadBoolean();
                        }
                        saveData.Dungeon.Rooms.Add(room.Id, room);
                    }
                    #endregion

                    #region Hallways
                    int hallwayCount = br.ReadInt32();
                    for (int i = 0; i < hallwayCount; i++)
                    {
                        var hallway = new Hallway(br.ReadString());
                        hallway.RoomA = saveData.Dungeon.Rooms[br.ReadString()];
                        hallway.RoomB = saveData.Dungeon.Rooms[br.ReadString()];
                        int hallsCount = br.ReadInt32();
                        for (int j = 0; j < hallsCount; j++)
                        {
                            var hallSector = new HallSector(br.ReadString(), br.ReadInt32(), br.ReadInt32(), hallway);
                            hallSector.TextureId = br.ReadString();
                            hallSector.Type = (AreaType)br.ReadInt32();
                            hallSector.Knowledge = (Knowledge)br.ReadInt32();
                            bool hasProp = br.ReadBoolean();
                            if (hasProp)
                            {
                                AreaType propType = (AreaType)br.ReadInt32();

                                switch (propType)
                                {
                                    case AreaType.Door:
                                        hallSector.Prop = new Door("to", br.ReadString(), (Direction)br.ReadInt32());
                                        break;
                                    case AreaType.Curio:
                                        bool isQuestCurio = br.ReadBoolean();
                                        Curio newCurio = DarkestDungeonManager.Data.Curios[br.ReadString()];
                                        newCurio.IsQuestCurio = isQuestCurio;
                                        hallSector.Prop = newCurio;
                                        break;
                                    case AreaType.Obstacle:
                                        hallSector.Prop = DarkestDungeonManager.Data.Obstacles[br.ReadString()];
                                        break;
                                    case AreaType.Trap:
                                        hallSector.Prop = DarkestDungeonManager.Data.Traps[br.ReadString()];
                                        break;
                                }
                            }
                            bool hasBattle = br.ReadBoolean();
                            if (hasBattle)
                            {
                                hallSector.BattleEncounter = new BattleEncounter();
                                int monsterCount = br.ReadInt32();
                                for (int k = 0; k < monsterCount; k++)
                                    hallSector.BattleEncounter.Monsters.Add(new Monster(DarkestDungeonManager.Data.Monsters[br.ReadString()]));
                                hallSector.BattleEncounter.Cleared = br.ReadBoolean();
                            }
                            hallway.Halls.Add(hallSector);
                        }
                        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
                    }
                    #endregion

                    var envData = DarkestDungeonManager.Data.DungeonEnviromentData[saveData.Quest.Dungeon];
                    saveData.Dungeon.SharedMash = DarkestDungeonManager.Data.DungeonEnviromentData["shared"].
                        BattleMashes.Find(mash => mash.MashId == saveData.Quest.Difficulty);
                    saveData.Dungeon.DungeonMash = envData.BattleMashes.Find(mash => mash.MashId == saveData.Quest.Difficulty);

                    int sharedExecIdsCount = br.ReadInt32();
                    for (int i = 0; i < sharedExecIdsCount; i++)
                        saveData.Dungeon.SharedMashExecutionIds.Add(br.ReadInt32());
                    #endregion

                    #region Raid Party
                    saveData.RaidParty = new RaidPartySaveData();
                    saveData.RaidParty.IsMovingLeft = br.ReadBoolean();
                    int raidHeroCount = br.ReadInt32();

                    for (int i = 0; i < raidHeroCount; i++)
                    {
                        var heroInfo = new RaidPartyHeroInfoSaveData();
                        heroInfo.HeroRosterId = br.ReadInt32();
                        heroInfo.IsAlive = br.ReadBoolean();
                        if (heroInfo.IsAlive == false)
                        {
                            heroInfo.Factor = (DeathFactor)br.ReadInt32();
                            heroInfo.Killer = br.ReadString();
                        }
                        saveData.RaidParty.HeroInfo.Add(heroInfo);
                    }
                    #endregion

                    #region Raid Info
                    saveData.CampingPhase = (CampingPhase)br.ReadInt32();
                    saveData.CampingTimeLeft = br.ReadInt32();
                    saveData.NightAmbushReduced = br.ReadSingle();
                    saveData.HungerCooldown = br.ReadInt32();
                    saveData.AncestorTalk = br.ReadInt32();

                    saveData.ExploredRoomCount = br.ReadInt32();
                    saveData.CurrentLocation = br.ReadString();
                    saveData.LastRoom = br.ReadString();
                    saveData.PreviousLastSector = br.ReadString();
                    saveData.LastSector = br.ReadString();

                    int monsterKilledCount = br.ReadInt32();
                    for (int i = 0; i < monsterKilledCount; i++)
                        saveData.KilledMonsters.Add(br.ReadString());

                    int investigatedCount = br.ReadInt32();
                    for (int i = 0; i < investigatedCount; i++)
                        saveData.InvestigatedCurios.Add(br.ReadString());

                    saveData.TorchAmount = br.ReadInt32();
                    saveData.MaxTorchAmount = br.ReadInt32();
                    saveData.ModifiedMinTorch = br.ReadInt32();
                    saveData.ModifiedMaxTorch = br.ReadInt32();

                    #endregion

                    #region HeroFormationData
                    saveData.HeroFormationData = new BattleFormationSaveData();
                    saveData.HeroFormationData.ReadFormationData(br);
                    #endregion

                    #region Raid Party Inventory
                    int inventorySlotCount = br.ReadInt32();
                    saveData.InventoryItems = new List<InventorySlotData>();
                    for (int i = 0; i < inventorySlotCount; i++)
                    {
                        bool hasItem = br.ReadBoolean();
                        if (hasItem)
                        {
                            var newItem = new ItemDefinition()
                            {
                                Id = br.ReadString(),
                                Type = br.ReadString(),
                                Amount = br.ReadInt32(),
                            };
                            saveData.InventoryItems.Add(new InventorySlotData(newItem));
                        }
                        else
                            saveData.InventoryItems.Add(null);
                        
                    }
                    #endregion

                    #region Battle
                    saveData.InBattle = br.ReadBoolean();
                    if (saveData.InBattle == false)
                        return saveData;

                    saveData.BattleGroundSaveData.ReadBattlegroundData(br);
                    #endregion

                    #endregion

                    return saveData;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error while reading save slot " + slotId + "! " + ex.Message +
                "Inner: " + ex.InnerException != null ? ex.InnerException.Message : "None");
            DeleteSave(slotId);
            return null;
        }
    }


    public static void WriteDungeonMap(SaveCampaignData saveData, string mapName)
    {
        RecreateMapDirectory();

        using (var fs = new FileStream(GenerateMapFileName(mapName), FileMode.Create, FileAccess.Write))
        {
            using (var bw = new BinaryWriter(fs))
            {
                bw.Write(saveData.Dungeon.Name);
                bw.Write(saveData.Dungeon.GridSizeX);
                bw.Write(saveData.Dungeon.GridSizeY);
                bw.Write(saveData.Dungeon.StartingRoomId);

                #region Rooms
                bw.Write(saveData.Dungeon.Rooms.Count);
                foreach (var room in saveData.Dungeon.Rooms.Values)
                {
                    bw.Write(room.Id);
                    bw.Write(room.GridX);
                    bw.Write(room.GridY);
                    bw.Write(room.Doors.Count);
                    for (int j = 0; j < room.Doors.Count; j++)
                    {
                        bw.Write(room.Doors[j].TargetArea);
                        bw.Write((int)room.Doors[j].Direction);
                    }
                    bw.Write(room.TextureId);
                    bw.Write((int)room.Type);
                    bw.Write((int)room.Knowledge);
                    bw.Write(room.Prop != null ? true : false);
                    if (room.Prop != null)
                    {
                        bw.Write((int)room.Prop.Type);
                        switch (room.Prop.Type)
                        {
                            case AreaType.Door:
                                var door = room.Prop as Door;
                                bw.Write(door.TargetArea);
                                bw.Write((int)door.Direction);
                                break;
                            case AreaType.Curio:
                                bw.Write((room.Prop as Curio).IsQuestCurio);
                                bw.Write(room.Prop.StringId);
                                break;
                            case AreaType.Obstacle:
                            case AreaType.Trap:
                                bw.Write(room.Prop.StringId);
                                break;
                        }
                    }
                    bw.Write(room.BattleEncounter != null ? true : false);
                    if (room.BattleEncounter != null)
                    {
                        bw.Write(room.BattleEncounter.Monsters.Count);
                        for (int j = 0; j < room.BattleEncounter.Monsters.Count; j++)
                            bw.Write(room.BattleEncounter.Monsters[j].Data.StringId);
                        bw.Write(room.BattleEncounter.Cleared);
                    }
                }
                #endregion

                #region Hallways
                bw.Write(saveData.Dungeon.Hallways.Count);
                foreach (var hallway in saveData.Dungeon.Hallways.Values)
                {
                    bw.Write(hallway.Id);
                    bw.Write(hallway.RoomA.Id);
                    bw.Write(hallway.RoomB.Id);
                    bw.Write(hallway.Halls.Count);
                    for (int j = 0; j < hallway.Halls.Count; j++)
                    {
                        bw.Write(hallway.Halls[j].Id);
                        bw.Write(hallway.Halls[j].GridX);
                        bw.Write(hallway.Halls[j].GridY);
                        bw.Write(hallway.Halls[j].TextureId);
                        bw.Write((int)hallway.Halls[j].Type);
                        bw.Write((int)hallway.Halls[j].Knowledge);
                        bw.Write(hallway.Halls[j].Prop != null ? true : false);
                        if (hallway.Halls[j].Prop != null)
                        {
                            bw.Write((int)hallway.Halls[j].Prop.Type);
                            switch (hallway.Halls[j].Prop.Type)
                            {
                                case AreaType.Door:
                                    var door = hallway.Halls[j].Prop as Door;
                                    bw.Write(door.TargetArea);
                                    bw.Write((int)door.Direction);
                                    break;
                                case AreaType.Curio:
                                    bw.Write((hallway.Halls[j].Prop as Curio).IsQuestCurio);
                                    bw.Write(hallway.Halls[j].Prop.StringId);
                                    break;
                                case AreaType.Obstacle:
                                case AreaType.Trap:
                                    bw.Write(hallway.Halls[j].Prop.StringId);
                                    break;
                            }
                        }
                        bw.Write(hallway.Halls[j].BattleEncounter != null ? true : false);
                        if (hallway.Halls[j].BattleEncounter != null)
                        {
                            bw.Write(hallway.Halls[j].BattleEncounter.Monsters.Count);
                            for (int i = 0; i < hallway.Halls[j].BattleEncounter.Monsters.Count; i++)
                                bw.Write(hallway.Halls[j].BattleEncounter.Monsters[i].Data.StringId);
                            bw.Write(hallway.Halls[j].BattleEncounter.Cleared);
                        }
                    }
                }
                #endregion
            }
        }
    }

    public static void ReadDungeonMap(SaveCampaignData saveData, string mapName)
    {
        using (var fs = new FileStream(GenerateSaveFileName(saveData.SaveId), FileMode.Open, FileAccess.Read))
        {
            using (var br = new BinaryReader(fs))
            {
                saveData.Dungeon = new Dungeon();
                saveData.Dungeon.Name = br.ReadString();
                saveData.Dungeon.GridSizeX = br.ReadInt32();
                saveData.Dungeon.GridSizeY = br.ReadInt32();
                saveData.Dungeon.StartingRoomId = br.ReadString();

                #region Rooms
                int roomCount = br.ReadInt32();
                for (int i = 0; i < roomCount; i++)
                {
                    var room = new DungeonRoom(br.ReadString(), br.ReadInt32(), br.ReadInt32());
                    int doorCount = br.ReadInt32();
                    for (int j = 0; j < doorCount; j++)
                    {
                        room.Doors.Add(new Door("to", br.ReadString(), (Direction)br.ReadInt32()));
                    }
                    room.TextureId = br.ReadString();
                    room.Type = (AreaType)br.ReadInt32();
                    room.Knowledge = (Knowledge)br.ReadInt32();
                    bool hasProp = br.ReadBoolean();
                    if (hasProp)
                    {
                        AreaType propType = (AreaType)br.ReadInt32();

                        switch (propType)
                        {
                            case AreaType.Door:
                                room.Prop = new Door("to", br.ReadString(), (Direction)br.ReadInt32());
                                break;
                            case AreaType.Curio:
                                bool isQuestCurio = br.ReadBoolean();
                                if (isQuestCurio)
                                {
                                    if (saveData.Quest.Goal.Type == "activate")
                                    {
                                        if (saveData.Quest.Goal.QuestData is QuestActivateData)
                                        {
                                            var activateData = saveData.Quest.Goal.QuestData as QuestActivateData;
                                            var curio = new Curio(activateData.CurioName); br.ReadString();
                                            curio.IsQuestCurio = true;

                                            if (saveData.Quest.Goal.StartingItems.Count > 0)
                                            {
                                                curio.ItemInteractions.Add(new ItemInteraction()
                                                {
                                                    Chance = 1,
                                                    ItemId = saveData.Quest.Goal.StartingItems[0].Id,
                                                    ResultType = "loot",
                                                    Results = new List<CurioResult>(),
                                                });
                                            }
                                            room.Prop = curio;
                                        }
                                    }
                                    else if (saveData.Quest.Goal.Type == "gather")
                                    {
                                        if (saveData.Quest.Goal.QuestData is QuestGatherData)
                                        {
                                            var gatherData = saveData.Quest.Goal.QuestData as QuestGatherData;
                                            var curio = new Curio(gatherData.CurioName); br.ReadString();
                                            curio.IsQuestCurio = true;

                                            var curioInteraction = new CurioInteraction();
                                            curioInteraction.Chance = 1;
                                            curioInteraction.ResultType = "loot";
                                            curioInteraction.Results = new List<CurioResult>();
                                            curioInteraction.Results.Add(new CurioResult()
                                            {
                                                Chance = 1,
                                                Draws = 1,
                                                Item = gatherData.Item.Id,
                                            });
                                            curio.Results.Add(curioInteraction);
                                            room.Prop = curio;
                                        }
                                    }
                                }
                                else
                                {
                                    Curio newCurio = DarkestDungeonManager.Data.Curios[br.ReadString()];
                                    newCurio.IsQuestCurio = isQuestCurio;
                                    room.Prop = newCurio;
                                }
                                break;
                            case AreaType.Obstacle:
                                room.Prop = DarkestDungeonManager.Data.Obstacles[br.ReadString()];
                                break;
                            case AreaType.Trap:
                                room.Prop = DarkestDungeonManager.Data.Traps[br.ReadString()];
                                break;
                        }
                    }
                    bool hasBattle = br.ReadBoolean();
                    if (hasBattle)
                    {
                        room.BattleEncounter = new BattleEncounter();
                        int monsterCount = br.ReadInt32();
                        for (int j = 0; j < monsterCount; j++)
                            room.BattleEncounter.Monsters.Add(new Monster(DarkestDungeonManager.Data.Monsters[br.ReadString()]));
                        room.BattleEncounter.Cleared = br.ReadBoolean();
                    }
                    saveData.Dungeon.Rooms.Add(room.Id, room);
                }
                #endregion

                #region Hallways
                int hallwayCount = br.ReadInt32();
                for (int i = 0; i < hallwayCount; i++)
                {
                    var hallway = new Hallway(br.ReadString());
                    hallway.RoomA = saveData.Dungeon.Rooms[br.ReadString()];
                    hallway.RoomB = saveData.Dungeon.Rooms[br.ReadString()];
                    int hallsCount = br.ReadInt32();
                    for (int j = 0; j < hallsCount; j++)
                    {
                        var hallSector = new HallSector(br.ReadString(), br.ReadInt32(), br.ReadInt32(), hallway);
                        hallSector.TextureId = br.ReadString();
                        hallSector.Type = (AreaType)br.ReadInt32();
                        hallSector.Knowledge = (Knowledge)br.ReadInt32();
                        bool hasProp = br.ReadBoolean();
                        if (hasProp)
                        {
                            AreaType propType = (AreaType)br.ReadInt32();

                            switch (propType)
                            {
                                case AreaType.Door:
                                    hallSector.Prop = new Door("to", br.ReadString(), (Direction)br.ReadInt32());
                                    break;
                                case AreaType.Curio:
                                    bool isQuestCurio = br.ReadBoolean();
                                    Curio newCurio = DarkestDungeonManager.Data.Curios[br.ReadString()];
                                    newCurio.IsQuestCurio = isQuestCurio;
                                    hallSector.Prop = newCurio;
                                    break;
                                case AreaType.Obstacle:
                                    hallSector.Prop = DarkestDungeonManager.Data.Obstacles[br.ReadString()];
                                    break;
                                case AreaType.Trap:
                                    hallSector.Prop = DarkestDungeonManager.Data.Traps[br.ReadString()];
                                    break;
                            }
                        }
                        bool hasBattle = br.ReadBoolean();
                        if (hasBattle)
                        {
                            hallSector.BattleEncounter = new BattleEncounter();
                            int monsterCount = br.ReadInt32();
                            for (int k = 0; k < monsterCount; k++)
                                hallSector.BattleEncounter.Monsters.Add(new Monster(DarkestDungeonManager.Data.Monsters[br.ReadString()]));
                            hallSector.BattleEncounter.Cleared = br.ReadBoolean();
                        }
                        hallway.Halls.Add(hallSector);
                    }
                    saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
                }
                #endregion
            }
        }
    }

    public static Dungeon LoadDungeonMap(string mapName, Quest quest)
    {
        TextAsset mapAsset = Resources.Load("Data/Maps/" + mapName) as TextAsset;
        using (BinaryReader br = new BinaryReader(new MemoryStream(mapAsset.bytes)))
        {
            Dungeon loadedDungeon;
            loadedDungeon = new Dungeon();
            loadedDungeon.Name = br.ReadString();
            loadedDungeon.GridSizeX = br.ReadInt32();
            loadedDungeon.GridSizeY = br.ReadInt32();
            loadedDungeon.StartingRoomId = br.ReadString();

            #region Rooms
            int roomCount = br.ReadInt32();
            for (int i = 0; i < roomCount; i++)
            {
                var room = new DungeonRoom(br.ReadString(), br.ReadInt32(), br.ReadInt32());
                int doorCount = br.ReadInt32();
                for (int j = 0; j < doorCount; j++)
                {
                    room.Doors.Add(new Door("to", br.ReadString(), (Direction)br.ReadInt32()));
                }
                room.TextureId = br.ReadString();
                room.Type = (AreaType)br.ReadInt32();
                room.Knowledge = (Knowledge)br.ReadInt32();
                bool hasProp = br.ReadBoolean();
                if (hasProp)
                {
                    AreaType propType = (AreaType)br.ReadInt32();

                    switch (propType)
                    {
                        case AreaType.Door:
                            room.Prop = new Door("to", br.ReadString(), (Direction)br.ReadInt32());
                            break;
                        case AreaType.Curio:
                            bool isQuestCurio = br.ReadBoolean();
                            if (isQuestCurio)
                            {
                                if (quest.Goal.Type == "activate")
                                {
                                    if (quest.Goal.QuestData is QuestActivateData)
                                    {
                                        var activateData = quest.Goal.QuestData as QuestActivateData;
                                        var curio = new Curio(activateData.CurioName); br.ReadString();
                                        curio.IsQuestCurio = true;

                                        if (quest.Goal.StartingItems.Count > 0)
                                        {
                                            curio.ItemInteractions.Add(new ItemInteraction()
                                            {
                                                Chance = 1,
                                                ItemId = quest.Goal.StartingItems[0].Id,
                                                ResultType = "loot",
                                                Results = new List<CurioResult>(),
                                            });
                                        }
                                        room.Prop = curio;
                                    }
                                }
                                else if (quest.Goal.Type == "gather")
                                {
                                    if (quest.Goal.QuestData is QuestGatherData)
                                    {
                                        var gatherData = quest.Goal.QuestData as QuestGatherData;
                                        var curio = new Curio(gatherData.CurioName); br.ReadString();
                                        curio.IsQuestCurio = true;

                                        var curioInteraction = new CurioInteraction();
                                        curioInteraction.Chance = 1;
                                        curioInteraction.ResultType = "loot";
                                        curioInteraction.Results = new List<CurioResult>();
                                        curioInteraction.Results.Add(new CurioResult()
                                        {
                                            Chance = 1,
                                            Draws = 1,
                                            Item = gatherData.Item.Id,
                                        });
                                        curio.Results.Add(curioInteraction);
                                        room.Prop = curio;
                                    }
                                }
                            }
                            else
                            {
                                Curio newCurio = DarkestDungeonManager.Data.Curios[br.ReadString()];
                                newCurio.IsQuestCurio = isQuestCurio;
                                room.Prop = newCurio;
                            }
                            break;
                        case AreaType.Obstacle:
                            room.Prop = DarkestDungeonManager.Data.Obstacles[br.ReadString()];
                            break;
                        case AreaType.Trap:
                            room.Prop = DarkestDungeonManager.Data.Traps[br.ReadString()];
                            break;
                    }
                }
                bool hasBattle = br.ReadBoolean();
                if (hasBattle)
                {
                    room.BattleEncounter = new BattleEncounter();
                    int monsterCount = br.ReadInt32();
                    for (int j = 0; j < monsterCount; j++)
                        room.BattleEncounter.Monsters.Add(new Monster(DarkestDungeonManager.Data.Monsters[br.ReadString()]));
                    room.BattleEncounter.Cleared = br.ReadBoolean();
                }
                loadedDungeon.Rooms.Add(room.Id, room);
            }
            #endregion
            #region Hallways
            int hallwayCount = br.ReadInt32();
            for (int i = 0; i < hallwayCount; i++)
            {
                var hallway = new Hallway(br.ReadString());
                hallway.RoomA = loadedDungeon.Rooms[br.ReadString()];
                hallway.RoomB = loadedDungeon.Rooms[br.ReadString()];
                int hallsCount = br.ReadInt32();
                for (int j = 0; j < hallsCount; j++)
                {
                    var hallSector = new HallSector(br.ReadString(), br.ReadInt32(), br.ReadInt32(), hallway);
                    hallSector.TextureId = br.ReadString();
                    hallSector.Type = (AreaType)br.ReadInt32();
                    hallSector.Knowledge = (Knowledge)br.ReadInt32();
                    bool hasProp = br.ReadBoolean();
                    if (hasProp)
                    {
                        AreaType propType = (AreaType)br.ReadInt32();

                        switch (propType)
                        {
                            case AreaType.Door:
                                hallSector.Prop = new Door("to", br.ReadString(), (Direction)br.ReadInt32());
                                break;
                            case AreaType.Curio:
                                bool isQuestCurio = br.ReadBoolean();
                                Curio newCurio = DarkestDungeonManager.Data.Curios[br.ReadString()];
                                newCurio.IsQuestCurio = isQuestCurio;
                                hallSector.Prop = newCurio;
                                break;
                            case AreaType.Obstacle:
                                hallSector.Prop = DarkestDungeonManager.Data.Obstacles[br.ReadString()];
                                break;
                            case AreaType.Trap:
                                hallSector.Prop = DarkestDungeonManager.Data.Traps[br.ReadString()];
                                break;
                        }
                    }
                    bool hasBattle = br.ReadBoolean();
                    if (hasBattle)
                    {
                        hallSector.BattleEncounter = new BattleEncounter();
                        int monsterCount = br.ReadInt32();
                        for (int k = 0; k < monsterCount; k++)
                            hallSector.BattleEncounter.Monsters.Add(new Monster(DarkestDungeonManager.Data.Monsters[br.ReadString()]));
                        hallSector.BattleEncounter.Cleared = br.ReadBoolean();
                    }
                    hallway.Halls.Add(hallSector);
                }
                loadedDungeon.Hallways.Add(hallway.Id, hallway);
            }
            #endregion

            return loadedDungeon;
        }
    }


    public static SaveCampaignData WriteStartingSave(SaveCampaignData saveData)
    {
        saveData.IsFirstStart = true;
        saveData.LocationName = "Raid";
        saveData.QuestsCompleted = 0;
        saveData.CurrentWeek = 1;

        saveData.GoldAmount = 1000;
        saveData.BustsAmount = 10;
        saveData.DeedsAmount = 10;
        saveData.PortraitsAmount = 10;
        saveData.CrestsAmount = 10;

        #region Initial Heroes
        saveData.RosterHeroes = new List<SaveHeroData>
        {
            #region Hero 1
            new SaveHeroData()
            {
                RosterId = 1,
                Name = "Reynald",
                HeroClass = "crusader",
                ResolveLevel = 0,
                ResolveXP = 0,
                StressLevel = 10,
                WeaponLevel = 1,
                ArmorLevel = 1,
                LeftTrinketId = "",
                RightTrinketId = "",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("warrior_of_light"),
                    new QuirkInfo("kleptomaniac"),
                    new QuirkInfo("god_fearing"),
                },
            },
            #endregion
            #region Hero 2
            new SaveHeroData()
            {
                RosterId = 2,
                Name = "Dismas",
                HeroClass = "highwayman",
                ResolveLevel = 0,
                ResolveXP = 0,
                StressLevel = 10,
                WeaponLevel = 1,
                ArmorLevel = 1,
                LeftTrinketId = "",
                RightTrinketId = "",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("hard_noggin"),
                    new QuirkInfo("known_cheat"),
                    new QuirkInfo("quick_reflexes"),
                },
            },
            #endregion
        };
        #endregion

        #region StageCoach Heroes
        saveData.StageCoachHeroes = new List<SaveHeroData>();
        #endregion

        #region Initial Hero Purchases
        saveData.InstancedPurchases.Clear();
        for (int i = 0; i < saveData.RosterHeroes.Count; i++)
        {
            var newHeroPurchases = new Dictionary<string, UpgradePurchases>();
            saveData.InstancedPurchases.Add(saveData.RosterHeroes[i].RosterId, newHeroPurchases);
            newHeroPurchases.Add(saveData.RosterHeroes[i].HeroClass + ".weapon",
                new UpgradePurchases(saveData.RosterHeroes[i].HeroClass + ".weapon", new string[0]));
            newHeroPurchases.Add(saveData.RosterHeroes[i].HeroClass + ".armour",
                new UpgradePurchases(saveData.RosterHeroes[i].HeroClass + ".armour", new string[0]));
            var heroClass = DarkestDungeonManager.Data.HeroClasses[saveData.RosterHeroes[i].HeroClass];
            for (int j = 0; j < heroClass.CombatSkills.Count; j++)
                newHeroPurchases.Add(saveData.RosterHeroes[i].HeroClass + "." + heroClass.CombatSkills[j].Id, new UpgradePurchases(
                    saveData.RosterHeroes[i].HeroClass + "." + heroClass.CombatSkills[j].Id, new string[0]));
            for (int j = 0; j < heroClass.CampingSkills.Count; j++)
                newHeroPurchases.Add(heroClass.CampingSkills[j].Id, new UpgradePurchases(
                    heroClass.CampingSkills[j].Id, new string[0]));
        }
        saveData.InstancedPurchases[1]["crusader.smite"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["crusader.zealous_accusation"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["crusader.stunning_blow"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["crusader.bulwark_of_faith"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(0);
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(1);
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(2);
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(3);
        saveData.InstancedPurchases[1]["encourage"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["stand_tall"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["zealous_speech"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[0].SelectedCampingSkillIndexes.Add(0);
        saveData.RosterHeroes[0].SelectedCampingSkillIndexes.Add(4);
        saveData.RosterHeroes[0].SelectedCampingSkillIndexes.Add(5);

        saveData.InstancedPurchases[2]["highwayman.opened_vein"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["highwayman.pistol_shot"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["highwayman.grape_shot_blast"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["highwayman.take_aim"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(6);
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(1);
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(3);
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(4);
        saveData.InstancedPurchases[2]["first_aid"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["clean_guns"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["bandits_sense"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[1].SelectedCampingSkillIndexes.Add(1);
        saveData.RosterHeroes[1].SelectedCampingSkillIndexes.Add(5);
        saveData.RosterHeroes[1].SelectedCampingSkillIndexes.Add(6);
        #endregion

        #region Activity Log
        saveData.ActivityLog = new List<WeekActivityLog>();
        saveData.CompletedPlot = new List<string>();
        saveData.GeneratedQuests = new List<Quest>();
        #endregion

        #region Estate Misc
        saveData.InventoryTrinkets.Clear();

        saveData.WagonTrinkets.Clear();

        saveData.DungeonProgress.Clear();
        saveData.DungeonProgress.Add("crypts", new DungeonProgress("crypts", 0, 0, true, false));
        saveData.DungeonProgress.Add("warrens", new DungeonProgress("warrens", 0, 0, true, false));
        saveData.DungeonProgress.Add("weald", new DungeonProgress("weald", 0, 0, true, false));
        saveData.DungeonProgress.Add("cove", new DungeonProgress("cove", 0, 0, true, false));
        saveData.DungeonProgress.Add("darkestdungeon", new DungeonProgress("darkestdungeon", 1, 0, true, false));
        saveData.DungeonProgress.Add("town", new DungeonProgress("town", 1, 0, true, true));

        saveData.DeathRecords = new List<DeathRecord>();

        saveData.BuildingUpgrades.Clear();

        saveData.BuildingUpgrades.Add("abbey.meditation", new UpgradePurchases("abbey.meditation", new string[0]));
        saveData.BuildingUpgrades.Add("abbey.prayer", new UpgradePurchases("abbey.prayer", new string[0]));
        saveData.BuildingUpgrades.Add("abbey.flagellation", new UpgradePurchases("abbey.flagellation", new string[0]));
        saveData.BuildingUpgrades.Add("tavern.bar", new UpgradePurchases("tavern.bar", new string[0]));
        saveData.BuildingUpgrades.Add("tavern.gambling", new UpgradePurchases("tavern.gambling", new string[0]));
        saveData.BuildingUpgrades.Add("tavern.brothel", new UpgradePurchases("tavern.brothel", new string[0]));
        saveData.BuildingUpgrades.Add("sanitarium.cost", new UpgradePurchases("sanitarium.cost", new string[0]));
        saveData.BuildingUpgrades.Add("sanitarium.disease_quirk_cost", new UpgradePurchases("sanitarium.disease_quirk_cost", new string[0]));
        saveData.BuildingUpgrades.Add("sanitarium.slots", new UpgradePurchases("sanitarium.slots", new string[0]));
        saveData.BuildingUpgrades.Add("blacksmith.weapon", new UpgradePurchases("blacksmith.weapon", new string[0]));
        saveData.BuildingUpgrades.Add("blacksmith.armour", new UpgradePurchases("blacksmith.armour", new string[0]));
        saveData.BuildingUpgrades.Add("blacksmith.cost", new UpgradePurchases("blacksmith.cost", new string[0]));
        saveData.BuildingUpgrades.Add("guild.skill_levels", new UpgradePurchases("guild.skill_levels", new string[0]));
        saveData.BuildingUpgrades.Add("guild.cost", new UpgradePurchases("guild.cost", new string[0]));
        saveData.BuildingUpgrades.Add("camping_trainer.cost", new UpgradePurchases("camping_trainer.cost", new string[0]));
        saveData.BuildingUpgrades.Add("nomad_wagon.numitems", new UpgradePurchases("nomad_wagon.numitems", new string[0]));
        saveData.BuildingUpgrades.Add("nomad_wagon.cost", new UpgradePurchases("nomad_wagon.cost", new string[0]));
        saveData.BuildingUpgrades.Add("stage_coach.numrecruits", new UpgradePurchases("stage_coach.numrecruits", new string[0]));
        saveData.BuildingUpgrades.Add("stage_coach.rostersize", new UpgradePurchases("stage_coach.rostersize", new string[0]));
        saveData.BuildingUpgrades.Add("stage_coach.upgraded_recruits", new UpgradePurchases("stage_coach.upgraded_recruits", new string[0]));
        #endregion

        #region ActivitySlots
        saveData.AbbeyActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        saveData.TavernActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        saveData.SanitariumActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        #endregion

        saveData.InRaid = true;

        #region Quest
        saveData.QuestCompleted = false;
        saveData.Quest = new PlotQuest()
        {
            IsPlotQuest = true,
            Id = "tutorial",
            Difficulty = 1,
            Type = "tutorial_room",
            Dungeon = "weald",
            DungeonLevel = 1,
            Goal = DarkestDungeonManager.Data.QuestDatabase.QuestGoals["tutorial_final_room"],
            Length = 1,
            PlotTrinket = new PlotTrinketReward() {  Amount = 0, Rarity = "very_common"},
            Reward = new CompletionReward()
            {
                 ResolveXP = 2,
                 ItemDefinitions = new List<ItemDefinition>()
                 {
                     new ItemDefinition("gold", "", 5000),
                 }
            },

            CanRetreat = false,
            CompletionDungeonXp = false,
        };
        #endregion

        #region Dungeon
        saveData.Dungeon = new Dungeon();
        saveData.Dungeon.Name = "weald";
        saveData.Dungeon.GridSizeX = 9;
        saveData.Dungeon.GridSizeY = 1;
        saveData.Dungeon.StartingRoomId = "room1_1";

        DungeonRoom room = new DungeonRoom("room1_1", 1, 1)
        {
            Knowledge = Knowledge.Completed,
            Type = AreaType.Entrance,
            MashId = 1,
            Prop = null,
            BattleEncounter = null,
            Doors = new List<Door>(),
            TextureId = "effigy_0",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);

        room = new DungeonRoom("room2_1", 8, 1)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.BattleTresure,
            MashId = 1,
            Prop = DarkestDungeonManager.Data.Curios["bandits_trapped_chest"],
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["weald"].BattleMashes.
                    Find(mash => mash.MashId == 1).NamedEncounters["tutorial_2"][0].MonsterSet),
            Doors = new List<Door>(),
            TextureId = "effigy_1",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);

        Hallway hallway = new Hallway("hallroom2_1_room1_1");
        hallway.RoomA = saveData.Dungeon.Rooms["room2_1"];
        hallway.RoomB = saveData.Dungeon.Rooms["room1_1"];
        hallway.RoomA.Doors.Add(new Door("room2_1", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room1_1", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", 7, 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", 6, 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "7",
                Type = AreaType.Curio,
                Prop = DarkestDungeonManager.Data.Curios["travellers_tent_tutorial"],
            },
            new HallSector("2", 5, 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "8",
                Type = AreaType.Empty,
            },
            new HallSector("3", 4, 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "2",
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["weald"].BattleMashes.
                    Find(mash => mash.MashId == 1).NamedEncounters["tutorial_1"][0].MonsterSet),
            },
            new HallSector("4", 3, 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Empty,
            },
            new HallSector("5", 2, 1, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion

        #region Party
        saveData.RaidParty = new RaidPartySaveData()
        {
            IsMovingLeft = false,
            HeroInfo = new List<RaidPartyHeroInfoSaveData>()
            {
                new RaidPartyHeroInfoSaveData()
                {
                Factor = DeathFactor.AttackMonster,
                Killer = "",
                IsAlive = true,
                HeroRosterId = 1,
                },
                new RaidPartyHeroInfoSaveData()
                {
                Factor = DeathFactor.AttackMonster,
                Killer = "",
                IsAlive = true,
                HeroRosterId = 2,
                },
            }
        };
        #endregion

        #region Data
        saveData.ExploredRoomCount = 1;
        saveData.CurrentLocation = "room1_1";
        saveData.LastRoom = "room1_1";
        saveData.PreviousLastSector = "";
        saveData.LastSector = "";
        saveData.KilledMonsters = new List<string>();
        saveData.InvestigatedCurios = new List<string>();

        saveData.TorchAmount = 100;
        saveData.MaxTorchAmount = 100;
        saveData.ModifiedMinTorch = -1;
        saveData.ModifiedMaxTorch = -1;
        #endregion

        #region Formation
        saveData.HeroFormationData = new BattleFormationSaveData();
        saveData.HeroFormationData.unitData.Add(new FormationUnitSaveData()
            {
                IsHero = true,
                RosterId = 1,
                Rank = 1,
                CombatInfo = new FormationUnitInfo()
                {
                     CombatId = 1,
                }
            });
        saveData.HeroFormationData.unitData.Add(new FormationUnitSaveData()
        {
            IsHero = true,
            RosterId = 2,
            Rank = 2,
            CombatInfo = new FormationUnitInfo()
            {
                CombatId = 2,
            }
        });
        #endregion

        #region Inventory
        saveData.InventoryItems = new List<InventorySlotData>();
        #endregion

        WriteSave(saveData);
        return saveData;
    }

    public static SaveCampaignData WriteTestingSave(SaveCampaignData saveData)
    {
        saveData.IsFirstStart = true;
        saveData.LocationName = "Hamlet";
        saveData.QuestsCompleted = 50;
        saveData.CurrentWeek = 2;

        saveData.GoldAmount = 120000;
        saveData.BustsAmount = 123;
        saveData.DeedsAmount = 128;
        saveData.PortraitsAmount = 274;
        saveData.CrestsAmount = 492;

        #region Initial Heroes
        saveData.RosterHeroes = new List<SaveHeroData>
        {
            #region Hero 1
            new SaveHeroData()
            {
                RosterId = 1,
                Name = "Reynald",
                HeroClass = "arbalest",
                ResolveLevel = 6,
                ResolveXP = 94,
                StressLevel = 30,
                WeaponLevel = 5,
                ArmorLevel = 4,
                LeftTrinketId = "ancestors_coat",
                RightTrinketId = "ancestors_musket_ball",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("slugger"),
                    new QuirkInfo("robust"),
                    new QuirkInfo("second_wind"),
                    new QuirkInfo("satanophobia"),
                    new QuirkInfo("meditator"),
                    new QuirkInfo("tapeworm"),
                },
            },
            #endregion
            #region Hero 2
            new SaveHeroData()
            {
                RosterId = 2,
                Name = "Dismas",
                HeroClass = "occultist",
                ResolveLevel = 5,
                ResolveXP = 60,
                StressLevel = 15,
                WeaponLevel = 3,
                ArmorLevel = 3,
                LeftTrinketId = "ancestors_pistol",
                RightTrinketId = "ancestors_pen",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("slugger"),
                    new QuirkInfo("robust"),
                    new QuirkInfo("warrior_of_light"),
                    new QuirkInfo("second_wind"),
                    new QuirkInfo("satanophobia"),
                },
            },
            #endregion
            #region Hero 3
            new SaveHeroData()
            {
                RosterId = 3,
                Name = "Renold",
                HeroClass = "man_at_arms",
                ResolveLevel = 4,
                ResolveXP = 10,
                StressLevel = 0,
                WeaponLevel = 1,
                ArmorLevel = 1,
                LeftTrinketId = "ancestors_handkerchief",
                RightTrinketId = "ancestors_lantern",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("hard_skinned"),
                    new QuirkInfo("quick_reflexes"),
                    new QuirkInfo("quickdraw"),
                    new QuirkInfo("robust"),
                    new QuirkInfo("quickdraw"),
                },
            },
            #endregion
            #region Hero 4
            new SaveHeroData()
            {
                RosterId = 4,
                Name = "Maudit",
                HeroClass = "abomination",
                ResolveLevel = 5,
                ResolveXP = 79,
                StressLevel = 60,
                WeaponLevel = 5,
                ArmorLevel = 2,
                LeftTrinketId = "ancestors_signet_ring",
                RightTrinketId = "ancestors_moustache_cream",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("tough"),
                    new QuirkInfo("slow_reflexes"),
                    new QuirkInfo("slugger"),
                    new QuirkInfo("robust"),
                    new QuirkInfo("warrior_of_light"),
                    new QuirkInfo("tapeworm"),
                },
            },
            #endregion
            #region Hero 5
            new SaveHeroData()
            {
                RosterId = 5,
                Name = "Fairfax",
                HeroClass = "hellion",
                ResolveLevel = 0,
                ResolveXP = 1,
                StressLevel = 64,
                WeaponLevel = 1,
                ArmorLevel = 1,
                LeftTrinketId = "",
                RightTrinketId = "ancestors_map",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("warrior_of_light"),
                    new QuirkInfo("second_wind"),
                    new QuirkInfo("satanophobia"),
                },
            },
            #endregion
            #region Hero 6
            new SaveHeroData()
            {
                RosterId = 6,
                Name = "Bernard",
                HeroClass = "jester",
                ResolveLevel = 1,
                ResolveXP = 7,
                StressLevel = 99,
                WeaponLevel = 1,
                ArmorLevel = 1,
                LeftTrinketId = "ancestors_candle",
                RightTrinketId = "",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("tough"),
                    new QuirkInfo("robust"),
                    new QuirkInfo("warrior_of_light"),
                },
            },
            #endregion
            #region Hero 7
            new SaveHeroData()
            {
                RosterId = 7,
                Name = "Trixy",
                HeroClass = "grave_robber",
                ResolveLevel = 4,
                ResolveXP = 5,
                StressLevel = 15,
                WeaponLevel = 2,
                ArmorLevel = 2,
                LeftTrinketId = "ancestors_tentacle_idol",
                RightTrinketId = "ancestors_bottle",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("tough"),
                    new QuirkInfo("hard_skinned"),
                    new QuirkInfo("satanophobia"),
                },
            },
            #endregion
            #region Hero 8
            new SaveHeroData()
            {
                RosterId = 8,
                Name = "Leonel",
                HeroClass = "occultist",
                ResolveLevel = 4,
                ResolveXP = 21,
                StressLevel = 27,
                WeaponLevel = 3,
                ArmorLevel = 3,
                LeftTrinketId = "legendary_bracer",
                RightTrinketId = "focus_ring",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("meditator"),
                },
            },
            #endregion
            #region Hero 9
            new SaveHeroData()
            {
                RosterId = 9,
                Name = "Daston",
                HeroClass = "plague_doctor",
                ResolveLevel = 6,
                ResolveXP = 90,
                StressLevel = 87,
                WeaponLevel = 4,
                ArmorLevel = 4,
                LeftTrinketId = "tough_ring",
                RightTrinketId = "cleansing_crystal",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("slow_reflexes"),
                    new QuirkInfo("slugger"),
                    new QuirkInfo("satanophobia"),
                },
            },
            #endregion
            #region Hero 10
            new SaveHeroData()
            {
                RosterId = 10,
                Name = "Murax",
                HeroClass = "leper",
                ResolveLevel = 6,
                ResolveXP = 15,
                StressLevel = 62,
                WeaponLevel = 1,
                ArmorLevel = 1,
                LeftTrinketId = "",
                RightTrinketId = "",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("slow_reflexes"),
                    new QuirkInfo("slugger"),
                    new QuirkInfo("robust"),
                    new QuirkInfo("satanophobia"),
                },
            },
            #endregion
            #region Hero 11
            new SaveHeroData()
            {
                RosterId = 11,
                Name = "John",
                HeroClass = "crusader",
                ResolveLevel = 3,
                ResolveXP = 10,
                StressLevel = 10,
                WeaponLevel = 3,
                ArmorLevel = 3,
                LeftTrinketId = "",
                RightTrinketId = "",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("slow_reflexes"),
                    new QuirkInfo("warrior_of_light"),
                    new QuirkInfo("second_wind"),
                    new QuirkInfo("satanophobia"),
                },
            },
            #endregion
            #region Hero 12
            new SaveHeroData()
            {
                RosterId = 12,
                Name = "Tulio",
                HeroClass = "bounty_hunter",
                ResolveLevel = 2,
                ResolveXP = 12,
                StressLevel = 15,
                WeaponLevel = 2,
                ArmorLevel = 2,
                LeftTrinketId = "spiked_collar",
                RightTrinketId = "poisoning_buckle",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("slow_reflexes"),
                },
            },
            #endregion
            #region Hero 13
            new SaveHeroData()
            {
                RosterId = 13,
                Name = "Axe",
                HeroClass = "leper",
                ResolveLevel = 5,
                ResolveXP = 20,
                StressLevel = 0,
                WeaponLevel = 2,
                ArmorLevel = 3,
                LeftTrinketId = "berserk_mask",
                RightTrinketId = "martyrs_seal",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("tough"),
                },
            },
            #endregion
            #region Hero 14
            new SaveHeroData()
            {
                RosterId = 14,
                Name = "Jaraxus",
                HeroClass = "highwayman",
                ResolveLevel = 6,
                ResolveXP = 78,
                StressLevel = 100,
                WeaponLevel = 5,
                ArmorLevel = 2,
                LeftTrinketId = "sun_cloak",
                RightTrinketId = "",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("slow_reflexes"),
                    new QuirkInfo("robust"),
                },
            },
            #endregion
            #region Hero 15
            new SaveHeroData()
            {
                RosterId = 15,
                Name = "Kirk",
                HeroClass = "crusader",
                ResolveLevel = 4,
                ResolveXP = 31,
                StressLevel = 64,
                WeaponLevel = 3,
                ArmorLevel = 4,
                LeftTrinketId = "holy_orders",
                RightTrinketId = "",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("tough"),
                },
            },
            #endregion
            #region Hero 16
            new SaveHeroData()
            {
                RosterId = 16,
                Name = "William",
                HeroClass = "jester",
                ResolveLevel = 0,
                ResolveXP = 0,
                StressLevel = 99,
                WeaponLevel = 1,
                ArmorLevel = 1,
                LeftTrinketId = "berserk_charm",
                RightTrinketId = "",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("tough"),
                    new QuirkInfo("satanophobia"),
                },
            },
            #endregion
            #region Hero 17
            new SaveHeroData()
            {
                RosterId = 17,
                Name = "Jorgen",
                HeroClass = "occultist",
                ResolveLevel = 1,
                ResolveXP = 2,
                StressLevel = 15,
                WeaponLevel = 2,
                ArmorLevel = 2,
                LeftTrinketId = "",
                RightTrinketId = "feather_crystal",
                Quirks = new List<QuirkInfo>()
                {
                },
            },
            #endregion
            #region Hero 18
            new SaveHeroData()
            {
                RosterId = 18,
                Name = "Robert",
                HeroClass = "plague_doctor",
                ResolveLevel = 5,
                ResolveXP = 40,
                StressLevel = 27,
                WeaponLevel = 3,
                ArmorLevel = 3,
                LeftTrinketId = "brawlers_gloves",
                RightTrinketId = "life_crystal",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("tough"),
                },
            },
            #endregion
            #region Hero 19
            new SaveHeroData()
            {
                RosterId = 19,
                Name = "Quasim",
                HeroClass = "highwayman",
                ResolveLevel = 5,
                ResolveXP = 60,
                StressLevel = 87,
                WeaponLevel = 4,
                ArmorLevel = 4,
                LeftTrinketId = "snipers_ring",
                RightTrinketId = "quick_draw_charm",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("warrior_of_light"),
                    new QuirkInfo("second_wind"),
                },
            },
            #endregion
            #region Hero 20
            new SaveHeroData()
            {
                RosterId = 20,
                Name = "Lion",
                HeroClass = "crusader",
                ResolveLevel = 6,
                ResolveXP = 94,
                StressLevel = 62,
                WeaponLevel = 1,
                ArmorLevel = 1,
                LeftTrinketId = "bulls_eye_bandana",
                RightTrinketId = "protective_collar",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("slow_reflexes"),
                    new QuirkInfo("satanophobia"),
                },
            },
            #endregion
        };
        #endregion

        #region StageCoach Heroes
        saveData.StageCoachHeroes = new List<SaveHeroData>
        {
            #region Hero 1
            new SaveHeroData()
            {
                RosterId = 21,
                Name = "Recald",
                HeroClass = "crusader",
                ResolveLevel = 0,
                ResolveXP = 0,
                StressLevel = 0,
                WeaponLevel = 1,
                ArmorLevel = 1,
                LeftTrinketId = "",
                RightTrinketId = "",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("meditator"),
                },
            },
            #endregion
            #region Hero 2
            new SaveHeroData()
            {
                RosterId = 22,
                Name = "Resmas",
                HeroClass = "highwayman",
                ResolveLevel = 0,
                ResolveXP = 0,
                StressLevel = 0,
                WeaponLevel = 0,
                ArmorLevel = 0,
                LeftTrinketId = "",
                RightTrinketId = "",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("slugger"),
                    new QuirkInfo("warrior_of_light"),
                },
            },
            #endregion
            #region Hero 3
            new SaveHeroData()
            {
                RosterId = 23,
                Name = "Recold",
                HeroClass = "bounty_hunter",
                ResolveLevel = 0,
                ResolveXP = 0,
                StressLevel = 0,
                WeaponLevel = 1,
                ArmorLevel = 1,
                LeftTrinketId = "",
                RightTrinketId = "",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("hard_skinned"),
                    new QuirkInfo("slugger"),
                },
            },
            #endregion
            #region Hero 4
            new SaveHeroData()
            {
                RosterId = 24,
                Name = "Reudit",
                HeroClass = "vestal",
                ResolveLevel = 0,
                ResolveXP = 0,
                StressLevel = 0,
                WeaponLevel = 1,
                ArmorLevel = 1,
                LeftTrinketId = "",
                RightTrinketId = "",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("tough"),
                    new QuirkInfo("slow_reflexes"),
                },
            },
            #endregion
        };
        #endregion

        #region Initial Hero Purchases
        saveData.InstancedPurchases.Clear();
        List<string> equipCodes = new List<string>() { "0", "1", "2", "3", "4" };
        for (int i = 0; i < saveData.RosterHeroes.Count; i++)
        {
            var newHeroPurchases = new Dictionary<string, UpgradePurchases>();
            saveData.InstancedPurchases.Add(saveData.RosterHeroes[i].RosterId, newHeroPurchases);
            newHeroPurchases.Add(saveData.RosterHeroes[i].HeroClass + ".weapon",
                new UpgradePurchases(saveData.RosterHeroes[i].HeroClass + ".weapon", equipCodes.GetRange(0,
                Mathf.Clamp(saveData.RosterHeroes[i].WeaponLevel - 1, 0, 4)).ToArray()));
            newHeroPurchases.Add(saveData.RosterHeroes[i].HeroClass + ".armour",
                new UpgradePurchases(saveData.RosterHeroes[i].HeroClass + ".armour", equipCodes.GetRange(0,
                Mathf.Clamp(saveData.RosterHeroes[i].ArmorLevel - 1, 0, 4)).ToArray()));
            var heroClass = DarkestDungeonManager.Data.HeroClasses[saveData.RosterHeroes[i].HeroClass];
            for (int j = 0; j < heroClass.CombatSkills.Count; j++)
            {
                    newHeroPurchases.Add(saveData.RosterHeroes[i].HeroClass + "." + heroClass.CombatSkills[j].Id, new UpgradePurchases(
                        saveData.RosterHeroes[i].HeroClass + "." + heroClass.CombatSkills[j].Id,equipCodes.GetRange(0,
                            Mathf.Clamp(saveData.RosterHeroes[i].ArmorLevel, 0, 5)).ToArray()));
            }
            for (int j = 0; j < heroClass.CampingSkills.Count; j++)
            {
                newHeroPurchases.Add(heroClass.CampingSkills[j].Id,
                    new UpgradePurchases(heroClass.CampingSkills[j].Id, new string[] { "0" }));
            }

            if (saveData.RosterHeroes[i].HeroClass != "abomination")
            {
                saveData.RosterHeroes[i].SelectedCombatSkillIndexes.AddRange(new int[] { 0, 1, 2, 3 });
                saveData.RosterHeroes[i].SelectedCampingSkillIndexes.AddRange(new int[] { 0, 3, 5, 6 });
            }
            else
            {
                saveData.RosterHeroes[i].SelectedCombatSkillIndexes.AddRange(new int[] { 0, 1, 2, 3, 4, 5, 6 });
                saveData.RosterHeroes[i].SelectedCampingSkillIndexes.AddRange(new int[] { 0, 3, 5, 6 });
            }

        }
        for (int i = 0; i < saveData.StageCoachHeroes.Count; i++)
        {
            var newHeroPurchases = new Dictionary<string, UpgradePurchases>();
            saveData.InstancedPurchases.Add(saveData.StageCoachHeroes[i].RosterId, newHeroPurchases);
            newHeroPurchases.Add(saveData.StageCoachHeroes[i].HeroClass + ".weapon",
                new UpgradePurchases(saveData.StageCoachHeroes[i].HeroClass + ".weapon", new string[0]));
            newHeroPurchases.Add(saveData.StageCoachHeroes[i].HeroClass + ".armour",
                new UpgradePurchases(saveData.StageCoachHeroes[i].HeroClass + ".armour", new string[0]));
            var heroClass = DarkestDungeonManager.Data.HeroClasses[saveData.StageCoachHeroes[i].HeroClass];
            for (int j = 0; j < heroClass.CombatSkills.Count; j++)
            {
                if (j == 4 || j == 5 || j == 6 )
                    newHeroPurchases.Add(saveData.StageCoachHeroes[i].HeroClass + "." + heroClass.CombatSkills[j].Id, new UpgradePurchases(
                        saveData.StageCoachHeroes[i].HeroClass + "." + heroClass.CombatSkills[j].Id, new string[0]));
                else
                    newHeroPurchases.Add(saveData.StageCoachHeroes[i].HeroClass + "." + heroClass.CombatSkills[j].Id, new UpgradePurchases(
                        saveData.StageCoachHeroes[i].HeroClass + "." + heroClass.CombatSkills[j].Id, new string[1] { "0" }));

            }
            for (int j = 0; j < heroClass.CampingSkills.Count; j++)
            {
                if (j == 0 || j == 2 || j == 4 || j == 5)
                    newHeroPurchases.Add(heroClass.CampingSkills[j].Id,
                        new UpgradePurchases(heroClass.CampingSkills[j].Id, new string[] { "0" }));
                else
                    newHeroPurchases.Add(heroClass.CampingSkills[j].Id,
                        new UpgradePurchases(heroClass.CampingSkills[j].Id, new string[] { }));
            }
        }
        #endregion

        #region Activity Log
        saveData.ActivityLog = new List<WeekActivityLog>()
        {
            new WeekActivityLog(1)
            {
                 ReturnRecord = new PartyActivityRecord()
                 {
                      PartyActionType = PartyActionType.Tutorial,
                      QuestType = "explore",
                      QuestDifficulty = "1",
                      QuestLength = "1",
                      Dungeon = "weald",
                      Names = new List<string>(new string[2] { "Reynald", "Dismas", }),
                      Classes = new List<string>(new string[2] { "crusader", "highwayman"}),
                      Alive = new List<bool>(new bool[2] { true, true}),
                      IsSuccessfull = true,
                 },
                 EmbarkRecord = new PartyActivityRecord()
                 {
                      PartyActionType = PartyActionType.Embark,
                      QuestType = "gather",
                      QuestDifficulty = "1",
                      QuestLength = "2",
                      Dungeon = "crypts",
                      Names = new List<string>(new string[4] { "Reynald", "Dismas", "Renold", "Maudit" }),
                      Classes = new List<string>(new string[4] { "crusader", "highwayman", "bounty_hunter", "vestal" }),
                      Alive = new List<bool>(new bool[4] { true, true, true, true }),
                      IsSuccessfull = true,
                 },
            },
            new WeekActivityLog(2)
            {
                ReturnRecord = new PartyActivityRecord()
                 {
                      PartyActionType = PartyActionType.Result,
                      QuestType = "gather",
                      QuestDifficulty = "1",
                      QuestLength = "2",
                      Dungeon = "crypts",
                      Names = new List<string>(new string[4] { "Reynald", "Dismas", "Renold", "Maudit" }),
                      Classes = new List<string>(new string[4] { "crusader", "highwayman", "bounty_hunter", "vestal" }),
                      Alive = new List<bool>(new bool[4] { true, true, true, true }),
                      IsSuccessfull = true,
                 },
            },
        };
        #endregion

        #region Completed Plot
        saveData.CompletedPlot = new List<string>()
        {
            "leper", "crusader", "plot_tutorial_crypts",
            "plot_darkest_dungeon_1", "plot_darkest_dungeon_2",
            "plot_darkest_dungeon_3", "plot_kill_brigand_cannon_1",
            "plot_kill_brigand_cannon_2", "plot_kill_brigand_cannon_3",
            "plot_kill_drowned_crew_1", "plot_kill_drowned_crew_2", "plot_kill_drowned_crew_3",
            "plot_kill_formless_flesh_1", "plot_kill_formless_flesh_2", "plot_kill_formless_flesh_3",
            "plot_kill_hag_1", "plot_kill_hag_2", "plot_kill_hag_3",
            "plot_kill_necromancer_1", "plot_kill_necromancer_2", "plot_kill_necromancer_3",
            "plot_kill_prophet_1", "plot_kill_prophet_2", "plot_kill_prophet_3",
            "plot_kill_siren_1", "plot_kill_siren_2", "plot_kill_siren_3",
            "plot_kill_swine_prince_1", "plot_kill_swine_prince_2", "plot_kill_swine_prince_3",
        };
        #endregion

        saveData.InventoryTrinkets = new List<string>()
        {
            "sun_ring","sun_cloak", "tough_ring", "heavy_boots","focus_ring","defenders_seal", "deteriorating_bracer",
            "demons_cauldron","sacred_scroll", "cleansing_crystal","ancestors_musket_ball", "ancestors_coat",
            "ancestors_bottle","ancestors_scroll", "poisoning_buckle", "spiked_collar","bloodied_fetish",
            "sacrificial_cauldron","witchs_vial", "sharpening_sheath","immunity_mask", "bright_tambourine", "agility_whistle",
        };

        saveData.WagonTrinkets = new List<string>()
        {
            "demons_cauldron", "sacred_scroll", "cleansing_crystal", "ancestors_musket_ball", "ancestors_coat", 
        };

        saveData.DungeonProgress.Clear();
        saveData.DungeonProgress.Add("crypts", new DungeonProgress("crypts", 7, 42, true, false));
        saveData.DungeonProgress.Add("warrens", new DungeonProgress("warrens", 6, 16, true, false));
        saveData.DungeonProgress.Add("weald", new DungeonProgress("weald", 5, 32, true, false));
        saveData.DungeonProgress.Add("cove", new DungeonProgress("cove", 3, 1, true, false));
        saveData.DungeonProgress.Add("darkestdungeon", new DungeonProgress("darkestdungeon", 1, 0, true, false));
        saveData.DungeonProgress.Add("town", new DungeonProgress("town", 1, 0, true, true));

        saveData.DeathRecords = new List<DeathRecord>()
        {
            new DeathRecord() { HeroName = "Ronald", HeroClassIndex = 2,
                Factor = DeathFactor.Hunger, KillerName = "", ResolveLevel = 2, },
            new DeathRecord() { HeroName = "Losla", HeroClassIndex = 7,
                Factor = DeathFactor.AttackMonster, KillerName = "necromancer_C", ResolveLevel = 5, },
            new DeathRecord() { HeroName = "Qweoas", HeroClassIndex = 2,
                Factor = DeathFactor.AttackMonster, KillerName = "necromancer_C", ResolveLevel = 5, },
            new DeathRecord() { HeroName = "Klosopas", HeroClassIndex = 1,
                Factor = DeathFactor.AttackMonster, KillerName = "necromancer_C", ResolveLevel = 6, },
            new DeathRecord() { HeroName = "Klosopas", HeroClassIndex = 1,
                Factor = DeathFactor.AttackMonster, KillerName = "necromancer_C", ResolveLevel = 6, },
            new DeathRecord() { HeroName = "Klosopas", HeroClassIndex = 1,
                Factor = DeathFactor.AttackMonster, KillerName = "necromancer_C", ResolveLevel = 6, },
            new DeathRecord() { HeroName = "Klosopas", HeroClassIndex = 1,
                Factor = DeathFactor.AttackMonster, KillerName = "necromancer_C", ResolveLevel = 6, },
            new DeathRecord() { HeroName = "Trexos", HeroClassIndex = 0,
                Factor = DeathFactor.AttackMonster, KillerName = "necromancer_C", ResolveLevel = 4, },
            new DeathRecord() { HeroName = "Oloks", HeroClassIndex = 9,
                Factor = DeathFactor.BleedMonster, KillerName = "necromancer_C", ResolveLevel = 1, },
            new DeathRecord() { HeroName = "Dismas", HeroClassIndex = 10,
                Factor = DeathFactor.AttackMonster, KillerName = "necromancer_C", ResolveLevel = 5, },
        };

        saveData.BuildingUpgrades.Clear();

        saveData.BuildingUpgrades.Add("abbey.meditation",
            new UpgradePurchases("abbey.meditation", new string[6] { "a", "b", "c", "d", "e", "f" }));
        saveData.BuildingUpgrades.Add("abbey.prayer",
            new UpgradePurchases("abbey.prayer", new string[3] { "a", "b", "c" }));
        saveData.BuildingUpgrades.Add("abbey.flagellation", 
            new UpgradePurchases("abbey.flagellation", new string[0]));
        saveData.BuildingUpgrades.Add("tavern.bar",
            new UpgradePurchases("tavern.bar", new string[2] { "a", "b" }));
        saveData.BuildingUpgrades.Add("tavern.gambling",
            new UpgradePurchases("tavern.gambling", new string[5] { "a", "b", "c", "d", "e" }));
        saveData.BuildingUpgrades.Add("tavern.brothel",
            new UpgradePurchases("tavern.brothel", new string[1] { "a" }));
        saveData.BuildingUpgrades.Add("sanitarium.cost",
            new UpgradePurchases("sanitarium.cost", new string[3] { "a", "b", "c" }));
        saveData.BuildingUpgrades.Add("sanitarium.disease_quirk_cost",
            new UpgradePurchases("sanitarium.disease_quirk_cost", new string[1] { "a" }));
        saveData.BuildingUpgrades.Add("sanitarium.slots",
            new UpgradePurchases("sanitarium.slots", new string[2] { "a", "b" }));
        saveData.BuildingUpgrades.Add("blacksmith.weapon",
            new UpgradePurchases("blacksmith.weapon", new string[1] { "a" }));
        saveData.BuildingUpgrades.Add("blacksmith.armour",
            new UpgradePurchases("blacksmith.armour", new string[2] { "a", "b" }));
        saveData.BuildingUpgrades.Add("blacksmith.cost", 
            new UpgradePurchases("blacksmith.cost", new string[4] { "a", "b", "c", "d" }));
        saveData.BuildingUpgrades.Add("guild.skill_levels", 
            new UpgradePurchases("guild.skill_levels", new string[1] { "a" }));
        saveData.BuildingUpgrades.Add("guild.cost", 
            new UpgradePurchases("guild.cost", new string[2] { "a", "b" }));
        saveData.BuildingUpgrades.Add("camping_trainer.cost",
            new UpgradePurchases("camping_trainer.cost", new string[2] { "a", "b" }));
        saveData.BuildingUpgrades.Add("nomad_wagon.numitems",
            new UpgradePurchases("nomad_wagon.numitems", new string[4] { "a", "b", "c", "d" }));
        saveData.BuildingUpgrades.Add("nomad_wagon.cost",
            new UpgradePurchases("nomad_wagon.cost", new string[0]));
        saveData.BuildingUpgrades.Add("stage_coach.numrecruits",
            new UpgradePurchases("stage_coach.numrecruits", new string[4] { "a", "b", "c", "d" }));
        saveData.BuildingUpgrades.Add("stage_coach.rostersize",
            new UpgradePurchases("stage_coach.rostersize", new string[4] { "a", "b", "c", "d" }));
        saveData.BuildingUpgrades.Add("stage_coach.upgraded_recruits",
            new UpgradePurchases("stage_coach.upgraded_recruits", new string[1] { "a" }));

        #region ActivitySlots
        saveData.AbbeyActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        saveData.TavernActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        saveData.SanitariumActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        #endregion

        WriteSave(saveData);
        return saveData;
    }

    public static SaveCampaignData WriteDarkestQuestOneSave(SaveCampaignData saveData)
    {
        saveData.IsFirstStart = true;
        saveData.LocationName = "Raid";
        saveData.QuestsCompleted = 0;
        saveData.CurrentWeek = 1;

        saveData.GoldAmount = 1000;
        saveData.BustsAmount = 10;
        saveData.DeedsAmount = 10;
        saveData.PortraitsAmount = 10;
        saveData.CrestsAmount = 10;

        #region Initial Heroes
        saveData.RosterHeroes = new List<SaveHeroData>
        {
            #region Hero 1
            new SaveHeroData()
            {
                RosterId = 1,
                Name = "Reynald",
                HeroClass = "crusader",
                ResolveLevel = 0,
                ResolveXP = 0,
                StressLevel = 10,
                WeaponLevel = 1,
                ArmorLevel = 1,
                LeftTrinketId = "",
                RightTrinketId = "",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("warrior_of_light"),
                    new QuirkInfo("kleptomaniac"),
                    new QuirkInfo("god_fearing"),
                },
            },
            #endregion
            #region Hero 2
            new SaveHeroData()
            {
                RosterId = 2,
                Name = "Dismas",
                HeroClass = "highwayman",
                ResolveLevel = 0,
                ResolveXP = 0,
                StressLevel = 10,
                WeaponLevel = 1,
                ArmorLevel = 1,
                LeftTrinketId = "",
                RightTrinketId = "",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("hard_noggin"),
                    new QuirkInfo("known_cheat"),
                    new QuirkInfo("quick_reflexes"),
                },
            },
            #endregion
        };
        #endregion

        #region StageCoach Heroes
        saveData.StageCoachHeroes = new List<SaveHeroData>();
        #endregion

        #region Initial Hero Purchases
        saveData.InstancedPurchases.Clear();
        for (int i = 0; i < saveData.RosterHeroes.Count; i++)
        {
            var newHeroPurchases = new Dictionary<string, UpgradePurchases>();
            saveData.InstancedPurchases.Add(saveData.RosterHeroes[i].RosterId, newHeroPurchases);
            newHeroPurchases.Add(saveData.RosterHeroes[i].HeroClass + ".weapon",
                new UpgradePurchases(saveData.RosterHeroes[i].HeroClass + ".weapon", new string[0]));
            newHeroPurchases.Add(saveData.RosterHeroes[i].HeroClass + ".armour",
                new UpgradePurchases(saveData.RosterHeroes[i].HeroClass + ".armour", new string[0]));
            var heroClass = DarkestDungeonManager.Data.HeroClasses[saveData.RosterHeroes[i].HeroClass];
            for (int j = 0; j < heroClass.CombatSkills.Count; j++)
                newHeroPurchases.Add(saveData.RosterHeroes[i].HeroClass + "." + heroClass.CombatSkills[j].Id, new UpgradePurchases(
                    saveData.RosterHeroes[i].HeroClass + "." + heroClass.CombatSkills[j].Id, new string[0]));
            for (int j = 0; j < heroClass.CampingSkills.Count; j++)
                newHeroPurchases.Add(heroClass.CampingSkills[j].Id, new UpgradePurchases(
                    heroClass.CampingSkills[j].Id, new string[0]));
        }
        saveData.InstancedPurchases[1]["crusader.smite"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["crusader.zealous_accusation"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["crusader.stunning_blow"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["crusader.bulwark_of_faith"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(0);
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(1);
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(2);
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(3);
        saveData.InstancedPurchases[1]["encourage"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["stand_tall"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["zealous_speech"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[0].SelectedCampingSkillIndexes.Add(0);
        saveData.RosterHeroes[0].SelectedCampingSkillIndexes.Add(4);
        saveData.RosterHeroes[0].SelectedCampingSkillIndexes.Add(5);


        saveData.InstancedPurchases[2]["highwayman.opened_vein"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["highwayman.pistol_shot"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["highwayman.grape_shot_blast"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["highwayman.take_aim"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(6);
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(1);
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(3);
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(4);
        saveData.InstancedPurchases[2]["first_aid"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["clean_guns"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["bandits_sense"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[1].SelectedCampingSkillIndexes.Add(1);
        saveData.RosterHeroes[1].SelectedCampingSkillIndexes.Add(5);
        saveData.RosterHeroes[1].SelectedCampingSkillIndexes.Add(6);
        #endregion

        #region Activity Log
        saveData.ActivityLog = new List<WeekActivityLog>();
        saveData.CompletedPlot = new List<string>();
        saveData.GeneratedQuests = new List<Quest>();
        #endregion

        #region Estate Misc
        saveData.InventoryTrinkets.Clear();

        saveData.WagonTrinkets.Clear();

        saveData.DungeonProgress.Clear();
        saveData.DungeonProgress.Add("crypts", new DungeonProgress("crypts", 0, 0, true, false));
        saveData.DungeonProgress.Add("warrens", new DungeonProgress("warrens", 0, 0, true, false));
        saveData.DungeonProgress.Add("weald", new DungeonProgress("weald", 0, 0, true, false));
        saveData.DungeonProgress.Add("cove", new DungeonProgress("cove", 0, 0, true, false));
        saveData.DungeonProgress.Add("darkestdungeon", new DungeonProgress("darkestdungeon", 1, 0, true, false));
        saveData.DungeonProgress.Add("town", new DungeonProgress("town", 1, 0, true, true));

        saveData.DeathRecords = new List<DeathRecord>();

        saveData.BuildingUpgrades.Clear();

        saveData.BuildingUpgrades.Add("abbey.meditation", new UpgradePurchases("abbey.meditation", new string[0]));
        saveData.BuildingUpgrades.Add("abbey.prayer", new UpgradePurchases("abbey.prayer", new string[0]));
        saveData.BuildingUpgrades.Add("abbey.flagellation", new UpgradePurchases("abbey.flagellation", new string[0]));
        saveData.BuildingUpgrades.Add("tavern.bar", new UpgradePurchases("tavern.bar", new string[0]));
        saveData.BuildingUpgrades.Add("tavern.gambling", new UpgradePurchases("tavern.gambling", new string[0]));
        saveData.BuildingUpgrades.Add("tavern.brothel", new UpgradePurchases("tavern.brothel", new string[0]));
        saveData.BuildingUpgrades.Add("sanitarium.cost", new UpgradePurchases("sanitarium.cost", new string[0]));
        saveData.BuildingUpgrades.Add("sanitarium.disease_quirk_cost", new UpgradePurchases("sanitarium.disease_quirk_cost", new string[0]));
        saveData.BuildingUpgrades.Add("sanitarium.slots", new UpgradePurchases("sanitarium.slots", new string[0]));
        saveData.BuildingUpgrades.Add("blacksmith.weapon", new UpgradePurchases("blacksmith.weapon", new string[0]));
        saveData.BuildingUpgrades.Add("blacksmith.armour", new UpgradePurchases("blacksmith.armour", new string[0]));
        saveData.BuildingUpgrades.Add("blacksmith.cost", new UpgradePurchases("blacksmith.cost", new string[0]));
        saveData.BuildingUpgrades.Add("guild.skill_levels", new UpgradePurchases("guild.skill_levels", new string[0]));
        saveData.BuildingUpgrades.Add("guild.cost", new UpgradePurchases("guild.cost", new string[0]));
        saveData.BuildingUpgrades.Add("camping_trainer.cost", new UpgradePurchases("camping_trainer.cost", new string[0]));
        saveData.BuildingUpgrades.Add("nomad_wagon.numitems", new UpgradePurchases("nomad_wagon.numitems", new string[0]));
        saveData.BuildingUpgrades.Add("nomad_wagon.cost", new UpgradePurchases("nomad_wagon.cost", new string[0]));
        saveData.BuildingUpgrades.Add("stage_coach.numrecruits", new UpgradePurchases("stage_coach.numrecruits", new string[0]));
        saveData.BuildingUpgrades.Add("stage_coach.rostersize", new UpgradePurchases("stage_coach.rostersize", new string[0]));
        saveData.BuildingUpgrades.Add("stage_coach.upgraded_recruits", new UpgradePurchases("stage_coach.upgraded_recruits", new string[0]));
        #endregion

        #region ActivitySlots
        saveData.AbbeyActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        saveData.TavernActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        saveData.SanitariumActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        #endregion

        saveData.InRaid = true;

        #region Quest
        saveData.QuestCompleted = false;
        var currentPlotQuest = DarkestDungeonManager.Data.QuestDatabase.PlotQuests.Find(quest => quest.Id == "plot_darkest_dungeon_1").Copy();
        currentPlotQuest.PlotTrinket = new PlotTrinketReward() { Amount = 1, Rarity = "very_rare" };
        saveData.Quest = currentPlotQuest;
        #endregion

        #region Dungeon
        saveData.Dungeon = new Dungeon();
        saveData.Dungeon.Name = saveData.Quest.Dungeon;
        saveData.Dungeon.GridSizeX = 49;
        saveData.Dungeon.GridSizeY = 13;
        saveData.Dungeon.StartingRoomId = "entry";

        #region Rooms
        #region Entry 1/7
        DungeonRoom room = new DungeonRoom("entry", 1, 7)
        {
            Knowledge = Knowledge.Completed,
            Type = AreaType.Entrance,
            MashId = 1,
            Prop = null,
            BattleEncounter = null,
            Doors = new List<Door>(),
            TextureId = "plot_darkest_dungeon_1_enter",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 2 Mid 14/7
        room = new DungeonRoom("room2_mid", 14, 7)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Battle,
            MashId = 6,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_1_mash_03"][0].MonsterSet),
            Doors = new List<Door>(),
            TextureId = "random2",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 3 Top 18/11
        room = new DungeonRoom("room3_top", 18, 11)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 1,
            Doors = new List<Door>(),
            TextureId = "random4",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 3 Bot 18/3
        room = new DungeonRoom("room3_bot", 18, 3)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 1,
            Doors = new List<Door>(),
            TextureId = "random1",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 4 Mid 22/7
        room = new DungeonRoom("room4_mid", 22, 7)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 1,
            Doors = new List<Door>(),
            TextureId = "random1",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 5 Mid 28/7
        room = new DungeonRoom("room5_mid", 28, 7)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 1,
            Doors = new List<Door>(),
            TextureId = "random2",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 5 Top 28/13
        room = new DungeonRoom("room5_top", 28, 13)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Battle,
            MashId = 1,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_1_mash_07"][0].MonsterSet),
            Doors = new List<Door>(),
            TextureId = "random2",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 5 Bot 28/1
        room = new DungeonRoom("room5_bot", 28, 1)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 1,
            Doors = new List<Door>(),
            TextureId = "random2",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 6 Mid 34/7
        room = new DungeonRoom("room6_mid", 34, 7)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.BattleTresure,
            Prop = DarkestDungeonManager.Data.Curios["ancestors_knapsack"],
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_1_mash_09"][0].MonsterSet),
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "random2",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 6 Top 34/13
        room = new DungeonRoom("room6_top", 34, 13)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 1,
            Doors = new List<Door>(),
            TextureId = "random2",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 6 Bot 34/1
        room = new DungeonRoom("room6_bot", 34, 1)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 1,
            Doors = new List<Door>(),
            TextureId = "random2",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 7 Mid 40/7
        room = new DungeonRoom("room7_mid", 40, 7)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Battle,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_1_mash_10"][0].MonsterSet),
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "random2",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 7 Top 40/13
        room = new DungeonRoom("room7_top", 40, 13)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 1,
            Doors = new List<Door>(),
            TextureId = "random2",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 7 Bot 40/1
        room = new DungeonRoom("room7_bot", 40, 1)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 1,
            Doors = new List<Door>(),
            TextureId = "random2",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 8 Mid 49/7
        room = new DungeonRoom("room8_mid", 49, 7)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Boss,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).BossEncounters.Find(enc => enc.MonsterSet.Contains("shuffler_D")).MonsterSet),
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "plot_darkest_dungeon_1_final",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #endregion

        #region Hallways
        #region 14/7 to 1/7
        Hallway hallway = new Hallway("room2_mid_to_entry");
        hallway.RoomA = saveData.Dungeon.Rooms["room2_mid"];
        hallway.RoomB = saveData.Dungeon.Rooms["entry"];
        hallway.RoomA.Doors.Add(new Door("room2_mid", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("entry", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX - 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX - 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_1_mash_02"][0].MonsterSet),
            },
            new HallSector("2", hallway.RoomA.GridX - 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX - 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX - 5, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "2",
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX - 6, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_1_mash_01"][0].MonsterSet),
            },
            new HallSector("6", hallway.RoomA.GridX - 7, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("7", hallway.RoomA.GridX - 8, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("8", hallway.RoomA.GridX - 9, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("9", hallway.RoomA.GridX - 10, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "2",
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(new List<string>() { "formless_melee_A", "formless_guard_A", "formless_weak_A", "formless_ranged_A" }),
            },
            new HallSector("10", hallway.RoomA.GridX - 11, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Empty,
            },
            new HallSector("11", hallway.RoomA.GridX - 12, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 18/11 to 14/7
        hallway = new Hallway("room3_top_to_room2_mid");
        hallway.RoomA = saveData.Dungeon.Rooms["room3_top"];
        hallway.RoomB = saveData.Dungeon.Rooms["room2_mid"];
        hallway.RoomA.Doors.Add(new Door("room3_top", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room2_mid", hallway.Id, Direction.Top));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX - 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX - 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_1_mash_04"][0].MonsterSet),
            },
            new HallSector("2", hallway.RoomA.GridX - 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX - 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX - 4, hallway.RoomA.GridY - 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "2",
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX - 4, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Empty,
            },
            new HallSector("6", hallway.RoomA.GridX - 4, hallway.RoomA.GridY - 3, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 18/3 to 14/7
        hallway = new Hallway("room3_bot_to_room2_mid");
        hallway.RoomA = saveData.Dungeon.Rooms["room3_bot"];
        hallway.RoomB = saveData.Dungeon.Rooms["room2_mid"];
        hallway.RoomA.Doors.Add(new Door("room3_bot", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room2_mid", hallway.Id, Direction.Top));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX - 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX - 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_1_mash_05"][0].MonsterSet),
            },
            new HallSector("2", hallway.RoomA.GridX - 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX - 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX - 4, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "2",
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX - 4, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Empty,
            },
            new HallSector("6", hallway.RoomA.GridX - 4, hallway.RoomA.GridY + 3, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 22/7 to 18/11
        hallway = new Hallway("room4_mid_to_room3_top");
        hallway.RoomA = saveData.Dungeon.Rooms["room4_mid"];
        hallway.RoomB = saveData.Dungeon.Rooms["room3_top"];
        hallway.RoomA.Doors.Add(new Door("room4_mid", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room3_top", hallway.Id, Direction.Top));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY + 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY + 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX, hallway.RoomA.GridY + 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX - 1, hallway.RoomA.GridY + 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "2",
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX - 2, hallway.RoomA.GridY + 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Empty,
            },
            new HallSector("6", hallway.RoomA.GridX - 3, hallway.RoomA.GridY + 4, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 22/7 to 18/3
        hallway = new Hallway("room4_mid_to_room3_bot");
        hallway.RoomA = saveData.Dungeon.Rooms["room4_mid"];
        hallway.RoomB = saveData.Dungeon.Rooms["room3_bot"];
        hallway.RoomA.Doors.Add(new Door("room4_mid", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room3_bot", hallway.Id, Direction.Top));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY - 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX, hallway.RoomA.GridY - 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX - 1, hallway.RoomA.GridY - 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "2",
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX - 2, hallway.RoomA.GridY - 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Empty,
            },
            new HallSector("6", hallway.RoomA.GridX - 3, hallway.RoomA.GridY - 4, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 28/7 to 22/7
        hallway = new Hallway("room5_mid_to_room4_mid");
        hallway.RoomA = saveData.Dungeon.Rooms["room5_mid"];
        hallway.RoomB = saveData.Dungeon.Rooms["room4_mid"];
        hallway.RoomA.Doors.Add(new Door("room5_mid", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room4_mid", hallway.Id, Direction.Top));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX - 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX - 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX - 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_1_mash_06"][0].MonsterSet),
            },
            new HallSector("3", hallway.RoomA.GridX - 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX - 5, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 28/13 to 28/7
        hallway = new Hallway("room5_top_to_room5_mid");
        hallway.RoomA = saveData.Dungeon.Rooms["room5_top"];
        hallway.RoomB = saveData.Dungeon.Rooms["room5_mid"];
        hallway.RoomA.Doors.Add(new Door("room5_top", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room5_mid", hallway.Id, Direction.Top));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY - 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX, hallway.RoomA.GridY - 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX, hallway.RoomA.GridY - 5, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 28/1 to 28/7
        hallway = new Hallway("room5_bot_to_room5_mid");
        hallway.RoomA = saveData.Dungeon.Rooms["room5_bot"];
        hallway.RoomB = saveData.Dungeon.Rooms["room5_mid"];
        hallway.RoomA.Doors.Add(new Door("room5_bot", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room5_mid", hallway.Id, Direction.Top));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY + 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_1_mash_08"][0].MonsterSet),
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY + 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX, hallway.RoomA.GridY + 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX, hallway.RoomA.GridY + 5, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 34/1 to 28/1
        hallway = new Hallway("room6_bot_to_room5_bot");
        hallway.RoomA = saveData.Dungeon.Rooms["room6_bot"];
        hallway.RoomB = saveData.Dungeon.Rooms["room5_bot"];
        hallway.RoomA.Doors.Add(new Door("room6_bot", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room5_bot", hallway.Id, Direction.Top));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX - 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX - 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX - 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX - 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX - 5, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 34/13 to 28/13
        hallway = new Hallway("room6_top_to_room5_top");
        hallway.RoomA = saveData.Dungeon.Rooms["room6_top"];
        hallway.RoomB = saveData.Dungeon.Rooms["room5_top"];
        hallway.RoomA.Doors.Add(new Door("room6_top", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room5_top", hallway.Id, Direction.Top));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX - 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX - 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX - 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX - 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX - 5, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 34/1 to 34/7
        hallway = new Hallway("room6_bot_to_room6_mid");
        hallway.RoomA = saveData.Dungeon.Rooms["room6_bot"];
        hallway.RoomB = saveData.Dungeon.Rooms["room6_mid"];
        hallway.RoomA.Doors.Add(new Door("room6_bot", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room6_mid", hallway.Id, Direction.Top));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY + 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY + 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX, hallway.RoomA.GridY + 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX, hallway.RoomA.GridY + 5, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 34/13 to 34/7
        hallway = new Hallway("room6_top_to_room6_mid");
        hallway.RoomA = saveData.Dungeon.Rooms["room6_top"];
        hallway.RoomB = saveData.Dungeon.Rooms["room6_mid"];
        hallway.RoomA.Doors.Add(new Door("room6_top", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room6_mid", hallway.Id, Direction.Top));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY - 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_1_mash_08"][0].MonsterSet),
            },
            new HallSector("3", hallway.RoomA.GridX, hallway.RoomA.GridY - 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX, hallway.RoomA.GridY - 5, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 40/7 to 34/7
        hallway = new Hallway("room7_mid_to_room6_mid");
        hallway.RoomA = saveData.Dungeon.Rooms["room7_mid"];
        hallway.RoomB = saveData.Dungeon.Rooms["room6_mid"];
        hallway.RoomA.Doors.Add(new Door("room7_mid", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room6_mid", hallway.Id, Direction.Top));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX - 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX - 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX - 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX - 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX - 5, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 40/13 to 34/13
        hallway = new Hallway("room7_top_to_room6_top");
        hallway.RoomA = saveData.Dungeon.Rooms["room7_top"];
        hallway.RoomB = saveData.Dungeon.Rooms["room6_top"];
        hallway.RoomA.Doors.Add(new Door("room7_top", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room6_top", hallway.Id, Direction.Top));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX - 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX - 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX - 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX - 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX - 5, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 40/1 to 34/1
        hallway = new Hallway("room7_bot_to_room6_bot");
        hallway.RoomA = saveData.Dungeon.Rooms["room7_bot"];
        hallway.RoomB = saveData.Dungeon.Rooms["room6_bot"];
        hallway.RoomA.Doors.Add(new Door("room7_bot", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room6_bot", hallway.Id, Direction.Top));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX - 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX - 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX - 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_1_mash_07"][0].MonsterSet),
            },
            new HallSector("3", hallway.RoomA.GridX - 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX - 5, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 40/1 to 40/7
        hallway = new Hallway("room7_bot_to_room7_mid");
        hallway.RoomA = saveData.Dungeon.Rooms["room7_bot"];
        hallway.RoomB = saveData.Dungeon.Rooms["room7_mid"];
        hallway.RoomA.Doors.Add(new Door("room7_bot", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room7_mid", hallway.Id, Direction.Top));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY + 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY + 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX, hallway.RoomA.GridY + 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX, hallway.RoomA.GridY + 5, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 40/13 to 40/7
        hallway = new Hallway("room7_top_to_room7_mid");
        hallway.RoomA = saveData.Dungeon.Rooms["room7_top"];
        hallway.RoomB = saveData.Dungeon.Rooms["room7_mid"];
        hallway.RoomA.Doors.Add(new Door("room7_top", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room7_mid", hallway.Id, Direction.Top));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY - 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX, hallway.RoomA.GridY - 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX, hallway.RoomA.GridY - 5, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 49/7 to 40/7
        hallway = new Hallway("room8_mid_to_room7_mid");
        hallway.RoomA = saveData.Dungeon.Rooms["room8_mid"];
        hallway.RoomB = saveData.Dungeon.Rooms["room7_mid"];
        hallway.RoomA.Doors.Add(new Door("room8_mid", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room7_mid", hallway.Id, Direction.Top));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX - 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX - 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX - 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_1_mash_11"][0].MonsterSet),
            },
            new HallSector("3", hallway.RoomA.GridX - 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX - 5, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX - 6, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("6", hallway.RoomA.GridX - 7, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("7", hallway.RoomA.GridX - 8, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #endregion
        #endregion

        #region Party
        saveData.RaidParty = new RaidPartySaveData()
        {
            IsMovingLeft = false,
            HeroInfo = new List<RaidPartyHeroInfoSaveData>()
            {
                new RaidPartyHeroInfoSaveData()
                {
                Factor = DeathFactor.AttackMonster,
                Killer = "",
                IsAlive = true,
                HeroRosterId = 1,
                },
                new RaidPartyHeroInfoSaveData()
                {
                Factor = DeathFactor.AttackMonster,
                Killer = "",
                IsAlive = true,
                HeroRosterId = 2,
                },
            }
        };
        #endregion

        #region Data
        saveData.ExploredRoomCount = 1;
        saveData.CurrentLocation = "entry";
        saveData.LastRoom = "entry";
        saveData.PreviousLastSector = "";
        saveData.LastSector = "";
        saveData.KilledMonsters = new List<string>();
        saveData.InvestigatedCurios = new List<string>();

        saveData.TorchAmount = 100;
        saveData.MaxTorchAmount = 100;
        saveData.ModifiedMinTorch = -1;
        saveData.ModifiedMaxTorch = -1;
        #endregion

        #region Formation
        saveData.HeroFormationData = new BattleFormationSaveData();
        saveData.HeroFormationData.unitData.Add(new FormationUnitSaveData()
        {
            IsHero = true,
            RosterId = 1,
            Rank = 1,
            CombatInfo = new FormationUnitInfo()
            {
                CombatId = 1,
            }
        });
        saveData.HeroFormationData.unitData.Add(new FormationUnitSaveData()
        {
            IsHero = true,
            RosterId = 2,
            Rank = 2,
            CombatInfo = new FormationUnitInfo()
            {
                CombatId = 2,
            }
        });
        #endregion

        #region Inventory
        saveData.InventoryItems = new List<InventorySlotData>();
        #endregion

        WriteSave(saveData);
        return saveData;
    }

    public static SaveCampaignData WriteDarkestQuestTwoSave(SaveCampaignData saveData)
    {
        saveData.IsFirstStart = true;
        saveData.LocationName = "Raid";
        saveData.QuestsCompleted = 0;
        saveData.CurrentWeek = 1;

        saveData.GoldAmount = 1000;
        saveData.BustsAmount = 10;
        saveData.DeedsAmount = 10;
        saveData.PortraitsAmount = 10;
        saveData.CrestsAmount = 10;

        #region Initial Heroes
        saveData.RosterHeroes = new List<SaveHeroData>
        {
            #region Hero 1
            new SaveHeroData()
            {
                RosterId = 1,
                Name = "Reynald",
                HeroClass = "crusader",
                ResolveLevel = 0,
                ResolveXP = 0,
                StressLevel = 10,
                WeaponLevel = 1,
                ArmorLevel = 1,
                LeftTrinketId = "",
                RightTrinketId = "dd_trinket",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("warrior_of_light"),
                    new QuirkInfo("kleptomaniac"),
                    new QuirkInfo("god_fearing"),
                },
            },
            #endregion
            #region Hero 2
            new SaveHeroData()
            {
                RosterId = 2,
                Name = "Dismas",
                HeroClass = "highwayman",
                ResolveLevel = 0,
                ResolveXP = 0,
                StressLevel = 10,
                WeaponLevel = 1,
                ArmorLevel = 1,
                LeftTrinketId = "",
                RightTrinketId = "dd_trinket",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("hard_noggin"),
                    new QuirkInfo("known_cheat"),
                    new QuirkInfo("quick_reflexes"),
                },
            },
            #endregion
        };
        #endregion

        #region StageCoach Heroes
        saveData.StageCoachHeroes = new List<SaveHeroData>();
        #endregion

        #region Initial Hero Purchases
        saveData.InstancedPurchases.Clear();
        for (int i = 0; i < saveData.RosterHeroes.Count; i++)
        {
            var newHeroPurchases = new Dictionary<string, UpgradePurchases>();
            saveData.InstancedPurchases.Add(saveData.RosterHeroes[i].RosterId, newHeroPurchases);
            newHeroPurchases.Add(saveData.RosterHeroes[i].HeroClass + ".weapon",
                new UpgradePurchases(saveData.RosterHeroes[i].HeroClass + ".weapon", new string[0]));
            newHeroPurchases.Add(saveData.RosterHeroes[i].HeroClass + ".armour",
                new UpgradePurchases(saveData.RosterHeroes[i].HeroClass + ".armour", new string[0]));
            var heroClass = DarkestDungeonManager.Data.HeroClasses[saveData.RosterHeroes[i].HeroClass];
            for (int j = 0; j < heroClass.CombatSkills.Count; j++)
                newHeroPurchases.Add(saveData.RosterHeroes[i].HeroClass + "." + heroClass.CombatSkills[j].Id, new UpgradePurchases(
                    saveData.RosterHeroes[i].HeroClass + "." + heroClass.CombatSkills[j].Id, new string[0]));
            for (int j = 0; j < heroClass.CampingSkills.Count; j++)
                newHeroPurchases.Add(heroClass.CampingSkills[j].Id, new UpgradePurchases(
                    heroClass.CampingSkills[j].Id, new string[0]));
        }
        saveData.InstancedPurchases[1]["crusader.smite"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["crusader.zealous_accusation"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["crusader.stunning_blow"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["crusader.bulwark_of_faith"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(0);
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(1);
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(2);
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(3);
        saveData.InstancedPurchases[1]["encourage"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["stand_tall"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["zealous_speech"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[0].SelectedCampingSkillIndexes.Add(0);
        saveData.RosterHeroes[0].SelectedCampingSkillIndexes.Add(4);
        saveData.RosterHeroes[0].SelectedCampingSkillIndexes.Add(5);


        saveData.InstancedPurchases[2]["highwayman.opened_vein"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["highwayman.pistol_shot"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["highwayman.grape_shot_blast"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["highwayman.take_aim"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(6);
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(1);
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(3);
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(4);
        saveData.InstancedPurchases[2]["first_aid"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["clean_guns"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["bandits_sense"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[1].SelectedCampingSkillIndexes.Add(1);
        saveData.RosterHeroes[1].SelectedCampingSkillIndexes.Add(5);
        saveData.RosterHeroes[1].SelectedCampingSkillIndexes.Add(6);
        #endregion

        #region Activity Log
        saveData.ActivityLog = new List<WeekActivityLog>();
        saveData.CompletedPlot = new List<string>();
        saveData.GeneratedQuests = new List<Quest>();
        #endregion

        #region Estate Misc
        saveData.InventoryTrinkets.Clear();

        saveData.WagonTrinkets.Clear();

        saveData.DungeonProgress.Clear();
        saveData.DungeonProgress.Add("crypts", new DungeonProgress("crypts", 0, 0, true, false));
        saveData.DungeonProgress.Add("warrens", new DungeonProgress("warrens", 0, 0, true, false));
        saveData.DungeonProgress.Add("weald", new DungeonProgress("weald", 0, 0, true, false));
        saveData.DungeonProgress.Add("cove", new DungeonProgress("cove", 0, 0, true, false));
        saveData.DungeonProgress.Add("darkestdungeon", new DungeonProgress("darkestdungeon", 1, 0, true, false));
        saveData.DungeonProgress.Add("town", new DungeonProgress("town", 1, 0, true, true));

        saveData.DeathRecords = new List<DeathRecord>();

        saveData.BuildingUpgrades.Clear();

        saveData.BuildingUpgrades.Add("abbey.meditation", new UpgradePurchases("abbey.meditation", new string[0]));
        saveData.BuildingUpgrades.Add("abbey.prayer", new UpgradePurchases("abbey.prayer", new string[0]));
        saveData.BuildingUpgrades.Add("abbey.flagellation", new UpgradePurchases("abbey.flagellation", new string[0]));
        saveData.BuildingUpgrades.Add("tavern.bar", new UpgradePurchases("tavern.bar", new string[0]));
        saveData.BuildingUpgrades.Add("tavern.gambling", new UpgradePurchases("tavern.gambling", new string[0]));
        saveData.BuildingUpgrades.Add("tavern.brothel", new UpgradePurchases("tavern.brothel", new string[0]));
        saveData.BuildingUpgrades.Add("sanitarium.cost", new UpgradePurchases("sanitarium.cost", new string[0]));
        saveData.BuildingUpgrades.Add("sanitarium.disease_quirk_cost", new UpgradePurchases("sanitarium.disease_quirk_cost", new string[0]));
        saveData.BuildingUpgrades.Add("sanitarium.slots", new UpgradePurchases("sanitarium.slots", new string[0]));
        saveData.BuildingUpgrades.Add("blacksmith.weapon", new UpgradePurchases("blacksmith.weapon", new string[0]));
        saveData.BuildingUpgrades.Add("blacksmith.armour", new UpgradePurchases("blacksmith.armour", new string[0]));
        saveData.BuildingUpgrades.Add("blacksmith.cost", new UpgradePurchases("blacksmith.cost", new string[0]));
        saveData.BuildingUpgrades.Add("guild.skill_levels", new UpgradePurchases("guild.skill_levels", new string[0]));
        saveData.BuildingUpgrades.Add("guild.cost", new UpgradePurchases("guild.cost", new string[0]));
        saveData.BuildingUpgrades.Add("camping_trainer.cost", new UpgradePurchases("camping_trainer.cost", new string[0]));
        saveData.BuildingUpgrades.Add("nomad_wagon.numitems", new UpgradePurchases("nomad_wagon.numitems", new string[0]));
        saveData.BuildingUpgrades.Add("nomad_wagon.cost", new UpgradePurchases("nomad_wagon.cost", new string[0]));
        saveData.BuildingUpgrades.Add("stage_coach.numrecruits", new UpgradePurchases("stage_coach.numrecruits", new string[0]));
        saveData.BuildingUpgrades.Add("stage_coach.rostersize", new UpgradePurchases("stage_coach.rostersize", new string[0]));
        saveData.BuildingUpgrades.Add("stage_coach.upgraded_recruits", new UpgradePurchases("stage_coach.upgraded_recruits", new string[0]));
        #endregion

        #region ActivitySlots
        saveData.AbbeyActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        saveData.TavernActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        saveData.SanitariumActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        #endregion

        saveData.InRaid = true;

        #region Quest
        saveData.QuestCompleted = false;
        saveData.Quest = DarkestDungeonManager.Data.QuestDatabase.PlotQuests.Find(quest =>
            quest.Id == "plot_darkest_dungeon_2").Copy();
        #endregion

        #region Dungeon
        saveData.Dungeon = new Dungeon();
        saveData.Dungeon.Name = saveData.Quest.Dungeon;
        saveData.Dungeon.GridSizeX = 31;
        saveData.Dungeon.GridSizeY = 32;
        saveData.Dungeon.StartingRoomId = "room:16/11";

        #region Rooms
        #region Room (1,2) 1/11
        DungeonRoom room = new DungeonRoom("room:1/11", 1, 11)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 6,
            Prop = null,
            BattleEncounter = null,
            Doors = new List<Door>(),
            TextureId = "random1",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room (2,2) 5/7
        room = new DungeonRoom("room:5/7", 5, 7)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Battle,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_2_mash_03"][0].MonsterSet),
            MashId = 6,
            Prop = null,
            Doors = new List<Door>(),
            TextureId = "random2",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room (2,1) 5/1 Boss Beacon
        room = new DungeonRoom("room:5/1", 5, 1)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Boss,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_2_miniboss_1"][0].MonsterSet),
            MashId = 6,
            Prop = new Curio("beacon") { IsQuestCurio = true, },
            Doors = new List<Door>(),
            TextureId = "plot_darkest_dungeon_2_final",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room (3,3) 9/11
        room = new DungeonRoom("room:9/11", 9, 11)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 6,
            Prop = null,
            BattleEncounter = null,
            Doors = new List<Door>(),
            TextureId = "random3",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room (2,4) 5/15
        room = new DungeonRoom("room:5/15", 5, 15)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 6,
            Prop = null,
            BattleEncounter = null,
            Doors = new List<Door>(),
            TextureId = "random1",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room (2,5) 5/22
        room = new DungeonRoom("room:5/22", 5, 22)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Battle,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_2_mash_10"][0].MonsterSet),
            MashId = 6,
            Prop = null,
            Doors = new List<Door>(),
            TextureId = "random2",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room (5,3) 16/11 Entry
        room = new DungeonRoom("room:16/11", 16, 11)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Entrance,
            MashId = 6,
            Prop = null,
            BattleEncounter = null,
            Doors = new List<Door>(),
            TextureId = "plot_darkest_dungeon_2_enter",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room (5,4) 16/18
        room = new DungeonRoom("room:16/18", 16, 18)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 6,
            Prop = null,
            BattleEncounter = null,
            Doors = new List<Door>(),
            TextureId = "random1",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room (4,5) 12/22
        room = new DungeonRoom("room:12/22", 12, 22)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 6,
            Prop = null,
            BattleEncounter = null,
            Doors = new List<Door>(),
            TextureId = "random2",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room (6,5) 20/22
        room = new DungeonRoom("room:20/22", 20, 22)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 6,
            Prop = null,
            BattleEncounter = null,
            Doors = new List<Door>(),
            TextureId = "random3",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room (5,6) 16/26
        room = new DungeonRoom("room:16/26", 16, 26)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Battle,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_2_mash_07"][0].MonsterSet),
            MashId = 6,
            Prop = null,
            Doors = new List<Door>(),
            TextureId = "random1",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room (5,7) 16/32 Boss Beacon
        room = new DungeonRoom("room:16/32", 16, 32)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Boss,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_2_miniboss_2"][0].MonsterSet),
            MashId = 6,
            Prop = new Curio("beacon") { IsQuestCurio = true, },
            Doors = new List<Door>(),
            TextureId = "plot_darkest_dungeon_2_final",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room (7,3) 23/11
        room = new DungeonRoom("room:23/11", 23, 11)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 6,
            Prop = null,
            BattleEncounter = null,
            Doors = new List<Door>(),
            TextureId = "random2",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room (9,3) 31/11
        room = new DungeonRoom("room:31/11", 31, 11)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 6,
            Prop = null,
            BattleEncounter = null,
            Doors = new List<Door>(),
            TextureId = "random3",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room (8,1) 27/1 Boss Beacon
        room = new DungeonRoom("room:27/1", 27, 1)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Boss,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_2_miniboss_3"][0].MonsterSet),
            MashId = 6,
            Prop = new Curio("beacon") { IsQuestCurio = true, },
            Doors = new List<Door>(),
            TextureId = "plot_darkest_dungeon_2_final",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room (8,2) 27/7
        room = new DungeonRoom("room:27/7", 27, 7)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 6,
            Prop = null,
            Doors = new List<Door>(),
            TextureId = "random1",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room (8,4) 27/15
        room = new DungeonRoom("room:27/15", 27, 15)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 6,
            Prop = null,
            Doors = new List<Door>(),
            TextureId = "random2",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room (8,5) 27/22
        room = new DungeonRoom("room:27/22", 27, 22)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.BattleTresure,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_2_mash_10"][0].MonsterSet),
            MashId = 6,
            Prop = DarkestDungeonManager.Data.Curios["ancestors_knapsack"],
            Doors = new List<Door>(),
            TextureId = "random3",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #endregion

        #region Hallways
        // Left Section
        #region 5/15 to 1/11
        Hallway hallway = new Hallway("5/15_to_1/11");
        hallway.RoomA = saveData.Dungeon.Rooms["room:5/15"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:1/11"];
        hallway.RoomA.Doors.Add(new Door("room:5/15", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:1/11", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX - 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX - 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX - 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX - 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_2_mash_02"][0].MonsterSet),
            },
            new HallSector("4", hallway.RoomA.GridX - 4, hallway.RoomA.GridY - 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX - 4, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("6", hallway.RoomA.GridX - 4, hallway.RoomA.GridY - 3, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 5/15 to 9/11
        hallway = new Hallway("5/15_to_9/11");
        hallway.RoomA = saveData.Dungeon.Rooms["room:5/15"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:9/11"];
        hallway.RoomA.Doors.Add(new Door("room:5/15", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:9/11", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_2_mash_06"][0].MonsterSet),
            },
            new HallSector("4", hallway.RoomA.GridX + 4, hallway.RoomA.GridY - 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX + 4, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("6", hallway.RoomA.GridX + 4, hallway.RoomA.GridY - 3, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 5/15 to 5/22
        hallway = new Hallway("5/15_to_5/22");
        hallway.RoomA = saveData.Dungeon.Rooms["room:5/15"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:5/22"];
        hallway.RoomA.Doors.Add(new Door("room:5/15", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:5/22", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY + 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY + 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX, hallway.RoomA.GridY + 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX, hallway.RoomA.GridY + 5, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX, hallway.RoomA.GridY + 6, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 12/22 to 5/22
        hallway = new Hallway("12/22_to_5/22");
        hallway.RoomA = saveData.Dungeon.Rooms["room:12/22"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:5/22"];
        hallway.RoomA.Doors.Add(new Door("room:12/22", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:5/22", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX - 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX - 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX - 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX - 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX - 5, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX - 6, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 5/7 to 1/11
        hallway = new Hallway("5/7_to_1/11");
        hallway.RoomA = saveData.Dungeon.Rooms["room:5/7"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:1/11"];
        hallway.RoomA.Doors.Add(new Door("room:5/7", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:1/11", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX - 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX - 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX - 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX - 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX - 4, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX - 4, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("6", hallway.RoomA.GridX - 4, hallway.RoomA.GridY + 3, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 5/7 to 9/11
        hallway = new Hallway("5/7_to_9/11");
        hallway.RoomA = saveData.Dungeon.Rooms["room:5/7"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:9/11"];
        hallway.RoomA.Doors.Add(new Door("room:5/7", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:9/11", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 4, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX + 4, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("6", hallway.RoomA.GridX + 4, hallway.RoomA.GridY + 3, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 5/7 to 5/1
        hallway = new Hallway("5/7_to_5/1");
        hallway.RoomA = saveData.Dungeon.Rooms["room:5/7"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:5/1"];
        hallway.RoomA.Doors.Add(new Door("room:5/7", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:5/1", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY - 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX, hallway.RoomA.GridY - 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX, hallway.RoomA.GridY - 5, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        // Middle Section
        #region 16/11 to 9/11
        hallway = new Hallway("16/11_to_9/11");
        hallway.RoomA = saveData.Dungeon.Rooms["room:16/11"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:9/11"];
        hallway.RoomA.Doors.Add(new Door("room:16/11", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:9/11", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX - 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX - 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX - 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX - 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_2_mash_01"][0].MonsterSet),
            },
            new HallSector("4", hallway.RoomA.GridX - 5, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX - 6, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 16/11 to 16/18
        hallway = new Hallway("16/11_to_16/18");
        hallway.RoomA = saveData.Dungeon.Rooms["room:16/11"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:16/18"];
        hallway.RoomA.Doors.Add(new Door("room:16/11", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:16/18", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY + 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY + 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX, hallway.RoomA.GridY + 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX, hallway.RoomA.GridY + 5, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_2_mash_02"][0].MonsterSet),
            },
            new HallSector("5", hallway.RoomA.GridX - 6, hallway.RoomA.GridY + 6, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 16/11 to 23/11
        hallway = new Hallway("16/11_to_23/11");
        hallway.RoomA = saveData.Dungeon.Rooms["room:16/11"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:23/11"];
        hallway.RoomA.Doors.Add(new Door("room:16/11", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:23/11", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_2_mash_03"][0].MonsterSet),
            },
            new HallSector("3", hallway.RoomA.GridX + 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 5, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX + 6, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 16/18 to 12/22
        hallway = new Hallway("16/18_to_12/22");
        hallway.RoomA = saveData.Dungeon.Rooms["room:16/18"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:12/22"];
        hallway.RoomA.Doors.Add(new Door("room:16/18", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:12/22", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX - 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX - 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX - 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX - 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX - 4, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX - 4, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("6", hallway.RoomA.GridX - 4, hallway.RoomA.GridY + 3, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 16/18 to 20/22
        hallway = new Hallway("16/18_to_20/22");
        hallway.RoomA = saveData.Dungeon.Rooms["room:16/18"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:20/22"];
        hallway.RoomA.Doors.Add(new Door("room:16/18", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:20/22", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_2_mash_04"][0].MonsterSet),
            },
            new HallSector("4", hallway.RoomA.GridX + 4, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX + 4, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("6", hallway.RoomA.GridX + 4, hallway.RoomA.GridY + 3, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 16/26 to 20/22
        hallway = new Hallway("16/26_to_20/22");
        hallway.RoomA = saveData.Dungeon.Rooms["room:16/26"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:20/22"];
        hallway.RoomA.Doors.Add(new Door("room:16/26", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:20/22", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 4, hallway.RoomA.GridY - 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX + 4, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("6", hallway.RoomA.GridX + 4, hallway.RoomA.GridY - 3, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 16/26 to 12/22
        hallway = new Hallway("16/26_to_12/22");
        hallway.RoomA = saveData.Dungeon.Rooms["room:16/26"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:12/22"];
        hallway.RoomA.Doors.Add(new Door("room:16/26", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:12/22", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX - 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX - 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX - 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX - 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX - 4, hallway.RoomA.GridY - 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX - 4, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("6", hallway.RoomA.GridX - 4, hallway.RoomA.GridY - 3, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 16/26 to 16/32
        hallway = new Hallway("16/26_to_16/32");
        hallway.RoomA = saveData.Dungeon.Rooms["room:16/26"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:16/32"];
        hallway.RoomA.Doors.Add(new Door("room:16/26", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:16/32", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY + 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY + 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX, hallway.RoomA.GridY + 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX, hallway.RoomA.GridY + 5, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        // Right Section
        #region 27/15 to 23/11
        hallway = new Hallway("27/15_to_23/11");
        hallway.RoomA = saveData.Dungeon.Rooms["room:27/15"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:23/11"];
        hallway.RoomA.Doors.Add(new Door("room:27/15", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:23/11", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX - 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX - 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_2_mash_09"][0].MonsterSet),
            },
            new HallSector("2", hallway.RoomA.GridX - 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX - 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX - 4, hallway.RoomA.GridY - 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX - 4, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("6", hallway.RoomA.GridX - 4, hallway.RoomA.GridY - 3, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 27/15 to 31/11
        hallway = new Hallway("27/15_to_31/11");
        hallway.RoomA = saveData.Dungeon.Rooms["room:27/15"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:31/11"];
        hallway.RoomA.Doors.Add(new Door("room:27/15", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:31/11", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_2_mash_08"][0].MonsterSet),
            },
            new HallSector("4", hallway.RoomA.GridX + 4, hallway.RoomA.GridY - 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX + 4, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("6", hallway.RoomA.GridX + 4, hallway.RoomA.GridY - 3, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 27/15 to 27/22
        hallway = new Hallway("27/15_to_27/22");
        hallway.RoomA = saveData.Dungeon.Rooms["room:27/15"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:27/22"];
        hallway.RoomA.Doors.Add(new Door("room:27/15", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:27/22", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY + 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY + 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX, hallway.RoomA.GridY + 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX, hallway.RoomA.GridY + 5, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX, hallway.RoomA.GridY + 6, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 20/22 to 27/22
        hallway = new Hallway("20/22_to_27/22");
        hallway.RoomA = saveData.Dungeon.Rooms["room:20/22"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:27/22"];
        hallway.RoomA.Doors.Add(new Door("room:20/22", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:27/22", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 5, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX + 6, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 27/7 to 23/11
        hallway = new Hallway("27/7_to_23/11");
        hallway.RoomA = saveData.Dungeon.Rooms["room:27/7"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:23/11"];
        hallway.RoomA.Doors.Add(new Door("room:27/7", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:23/11", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX - 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX - 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX - 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_2_mash_05"][0].MonsterSet),
            },
            new HallSector("3", hallway.RoomA.GridX - 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            
            new HallSector("4", hallway.RoomA.GridX - 4, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX - 4, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("6", hallway.RoomA.GridX - 4, hallway.RoomA.GridY + 3, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 27/7 to 31/11
        hallway = new Hallway("27/7_to_31/11");
        hallway.RoomA = saveData.Dungeon.Rooms["room:27/7"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:31/11"];
        hallway.RoomA.Doors.Add(new Door("room:27/7", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:31/11", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 4, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX + 4, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "3",
                Type = AreaType.Empty,
            },
            new HallSector("6", hallway.RoomA.GridX + 4, hallway.RoomA.GridY + 3, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 27/7 to 27/1
        hallway = new Hallway("27/7_to_27/1");
        hallway.RoomA = saveData.Dungeon.Rooms["room:27/7"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:27/1"];
        hallway.RoomA.Doors.Add(new Door("room:5/7", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:5/1", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY - 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "5",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX, hallway.RoomA.GridY - 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "4",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX, hallway.RoomA.GridY - 5, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #endregion
        #endregion

        #region Party
        saveData.RaidParty = new RaidPartySaveData()
        {
            IsMovingLeft = false,
            HeroInfo = new List<RaidPartyHeroInfoSaveData>()
            {
                new RaidPartyHeroInfoSaveData()
                {
                Factor = DeathFactor.AttackMonster,
                Killer = "",
                IsAlive = true,
                HeroRosterId = 1,
                },
                new RaidPartyHeroInfoSaveData()
                {
                Factor = DeathFactor.AttackMonster,
                Killer = "",
                IsAlive = true,
                HeroRosterId = 2,
                },
            }
        };
        #endregion

        #region Data
        saveData.ExploredRoomCount = 1;
        saveData.CurrentLocation = "room:27/22";
        saveData.LastRoom = "room:27/22";
        saveData.PreviousLastSector = "";
        saveData.LastSector = "";
        saveData.KilledMonsters = new List<string>();
        saveData.InvestigatedCurios = new List<string>();

        saveData.TorchAmount = 100;
        saveData.MaxTorchAmount = 100;
        saveData.ModifiedMinTorch = -1;
        saveData.ModifiedMaxTorch = -1;
        #endregion

        #region Formation
        saveData.HeroFormationData = new BattleFormationSaveData();
        saveData.HeroFormationData.unitData.Add(new FormationUnitSaveData()
        {
            IsHero = true,
            RosterId = 1,
            Rank = 1,
            CombatInfo = new FormationUnitInfo()
            {
                CombatId = 1,
            }
        });
        saveData.HeroFormationData.unitData.Add(new FormationUnitSaveData()
        {
            IsHero = true,
            RosterId = 2,
            Rank = 2,
            CombatInfo = new FormationUnitInfo()
            {
                CombatId = 2,
            }
        });
        #endregion

        #region Inventory
        saveData.InventoryItems = new List<InventorySlotData>();
        #endregion

        WriteSave(saveData);
        return saveData;
    }

    public static SaveCampaignData WriteDarkestQuestThreeSave(SaveCampaignData saveData)
    {
        saveData.IsFirstStart = true;
        saveData.LocationName = "Raid";
        saveData.QuestsCompleted = 0;
        saveData.CurrentWeek = 1;

        saveData.GoldAmount = 1000;
        saveData.BustsAmount = 10;
        saveData.DeedsAmount = 10;
        saveData.PortraitsAmount = 10;
        saveData.CrestsAmount = 10;

        #region Initial Heroes
        saveData.RosterHeroes = new List<SaveHeroData>
        {
            #region Hero 1
            new SaveHeroData()
            {
                RosterId = 1,
                Name = "Reynald",
                HeroClass = "crusader",
                ResolveLevel = 0,
                ResolveXP = 0,
                StressLevel = 10,
                WeaponLevel = 1,
                ArmorLevel = 1,
                LeftTrinketId = "",
                RightTrinketId = "dd_trinket",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("warrior_of_light"),
                    new QuirkInfo("kleptomaniac"),
                    new QuirkInfo("god_fearing"),
                },
            },
            #endregion
            #region Hero 2
            new SaveHeroData()
            {
                RosterId = 2,
                Name = "Dismas",
                HeroClass = "highwayman",
                ResolveLevel = 0,
                ResolveXP = 0,
                StressLevel = 10,
                WeaponLevel = 1,
                ArmorLevel = 1,
                LeftTrinketId = "",
                RightTrinketId = "dd_trinket",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("hard_noggin"),
                    new QuirkInfo("known_cheat"),
                    new QuirkInfo("quick_reflexes"),
                },
            },
            #endregion
        };
        #endregion

        #region StageCoach Heroes
        saveData.StageCoachHeroes = new List<SaveHeroData>();
        #endregion

        #region Initial Hero Purchases
        saveData.InstancedPurchases.Clear();
        for (int i = 0; i < saveData.RosterHeroes.Count; i++)
        {
            var newHeroPurchases = new Dictionary<string, UpgradePurchases>();
            saveData.InstancedPurchases.Add(saveData.RosterHeroes[i].RosterId, newHeroPurchases);
            newHeroPurchases.Add(saveData.RosterHeroes[i].HeroClass + ".weapon",
                new UpgradePurchases(saveData.RosterHeroes[i].HeroClass + ".weapon", new string[0]));
            newHeroPurchases.Add(saveData.RosterHeroes[i].HeroClass + ".armour",
                new UpgradePurchases(saveData.RosterHeroes[i].HeroClass + ".armour", new string[0]));
            var heroClass = DarkestDungeonManager.Data.HeroClasses[saveData.RosterHeroes[i].HeroClass];
            for (int j = 0; j < heroClass.CombatSkills.Count; j++)
                newHeroPurchases.Add(saveData.RosterHeroes[i].HeroClass + "." + heroClass.CombatSkills[j].Id, new UpgradePurchases(
                    saveData.RosterHeroes[i].HeroClass + "." + heroClass.CombatSkills[j].Id, new string[0]));
            for (int j = 0; j < heroClass.CampingSkills.Count; j++)
                newHeroPurchases.Add(heroClass.CampingSkills[j].Id, new UpgradePurchases(
                    heroClass.CampingSkills[j].Id, new string[0]));
        }
        saveData.InstancedPurchases[1]["crusader.smite"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["crusader.zealous_accusation"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["crusader.stunning_blow"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["crusader.bulwark_of_faith"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(0);
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(1);
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(2);
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(3);
        saveData.InstancedPurchases[1]["encourage"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["stand_tall"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["zealous_speech"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[0].SelectedCampingSkillIndexes.Add(0);
        saveData.RosterHeroes[0].SelectedCampingSkillIndexes.Add(4);
        saveData.RosterHeroes[0].SelectedCampingSkillIndexes.Add(5);


        saveData.InstancedPurchases[2]["highwayman.opened_vein"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["highwayman.pistol_shot"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["highwayman.grape_shot_blast"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["highwayman.take_aim"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(6);
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(1);
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(3);
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(4);
        saveData.InstancedPurchases[2]["first_aid"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["clean_guns"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["bandits_sense"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[1].SelectedCampingSkillIndexes.Add(1);
        saveData.RosterHeroes[1].SelectedCampingSkillIndexes.Add(5);
        saveData.RosterHeroes[1].SelectedCampingSkillIndexes.Add(6);
        #endregion

        #region Activity Log
        saveData.ActivityLog = new List<WeekActivityLog>();
        saveData.CompletedPlot = new List<string>();
        saveData.GeneratedQuests = new List<Quest>();
        #endregion

        #region Estate Misc
        saveData.InventoryTrinkets.Clear();

        saveData.WagonTrinkets.Clear();

        saveData.DungeonProgress.Clear();
        saveData.DungeonProgress.Add("crypts", new DungeonProgress("crypts", 0, 0, true, false));
        saveData.DungeonProgress.Add("warrens", new DungeonProgress("warrens", 0, 0, true, false));
        saveData.DungeonProgress.Add("weald", new DungeonProgress("weald", 0, 0, true, false));
        saveData.DungeonProgress.Add("cove", new DungeonProgress("cove", 0, 0, true, false));
        saveData.DungeonProgress.Add("darkestdungeon", new DungeonProgress("darkestdungeon", 1, 0, true, false));
        saveData.DungeonProgress.Add("town", new DungeonProgress("town", 1, 0, true, true));

        saveData.DeathRecords = new List<DeathRecord>();

        saveData.BuildingUpgrades.Clear();

        saveData.BuildingUpgrades.Add("abbey.meditation", new UpgradePurchases("abbey.meditation", new string[0]));
        saveData.BuildingUpgrades.Add("abbey.prayer", new UpgradePurchases("abbey.prayer", new string[0]));
        saveData.BuildingUpgrades.Add("abbey.flagellation", new UpgradePurchases("abbey.flagellation", new string[0]));
        saveData.BuildingUpgrades.Add("tavern.bar", new UpgradePurchases("tavern.bar", new string[0]));
        saveData.BuildingUpgrades.Add("tavern.gambling", new UpgradePurchases("tavern.gambling", new string[0]));
        saveData.BuildingUpgrades.Add("tavern.brothel", new UpgradePurchases("tavern.brothel", new string[0]));
        saveData.BuildingUpgrades.Add("sanitarium.cost", new UpgradePurchases("sanitarium.cost", new string[0]));
        saveData.BuildingUpgrades.Add("sanitarium.disease_quirk_cost", new UpgradePurchases("sanitarium.disease_quirk_cost", new string[0]));
        saveData.BuildingUpgrades.Add("sanitarium.slots", new UpgradePurchases("sanitarium.slots", new string[0]));
        saveData.BuildingUpgrades.Add("blacksmith.weapon", new UpgradePurchases("blacksmith.weapon", new string[0]));
        saveData.BuildingUpgrades.Add("blacksmith.armour", new UpgradePurchases("blacksmith.armour", new string[0]));
        saveData.BuildingUpgrades.Add("blacksmith.cost", new UpgradePurchases("blacksmith.cost", new string[0]));
        saveData.BuildingUpgrades.Add("guild.skill_levels", new UpgradePurchases("guild.skill_levels", new string[0]));
        saveData.BuildingUpgrades.Add("guild.cost", new UpgradePurchases("guild.cost", new string[0]));
        saveData.BuildingUpgrades.Add("camping_trainer.cost", new UpgradePurchases("camping_trainer.cost", new string[0]));
        saveData.BuildingUpgrades.Add("nomad_wagon.numitems", new UpgradePurchases("nomad_wagon.numitems", new string[0]));
        saveData.BuildingUpgrades.Add("nomad_wagon.cost", new UpgradePurchases("nomad_wagon.cost", new string[0]));
        saveData.BuildingUpgrades.Add("stage_coach.numrecruits", new UpgradePurchases("stage_coach.numrecruits", new string[0]));
        saveData.BuildingUpgrades.Add("stage_coach.rostersize", new UpgradePurchases("stage_coach.rostersize", new string[0]));
        saveData.BuildingUpgrades.Add("stage_coach.upgraded_recruits", new UpgradePurchases("stage_coach.upgraded_recruits", new string[0]));
        #endregion

        #region ActivitySlots
        saveData.AbbeyActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        saveData.TavernActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        saveData.SanitariumActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        #endregion

        saveData.InRaid = true;

        #region Quest
        saveData.QuestCompleted = false;
        saveData.Quest = DarkestDungeonManager.Data.QuestDatabase.PlotQuests.Find(quest =>
            quest.Id == "plot_darkest_dungeon_3").Copy();
        #endregion

        #region Dungeon
        saveData.Dungeon = new Dungeon();
        saveData.Dungeon.Name = saveData.Quest.Dungeon;
        saveData.Dungeon.GridSizeX = 45;
        saveData.Dungeon.GridSizeY = 30;
        saveData.Dungeon.StartingRoomId = "room:2/29";

        #region Rooms
        // Top 26-29
        #region Room 2/29
        DungeonRoom room = new DungeonRoom("room:2/29", 2, 29)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Entrance,
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "plot_darkest_dungeon_3_enter",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 11/29
        room = new DungeonRoom("room:11/29", 11, 29)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "random1",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 31/29
        room = new DungeonRoom("room:31/29", 31, 29)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 6,
            Prop = null,
            Doors = new List<Door>(),
            TextureId = "random1",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 37/29
        room = new DungeonRoom("room:37/29", 37, 29)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.BattleTresure,
            MashId = 6,
            Prop = DarkestDungeonManager.Data.Curios["ancestors_knapsack"],
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_mash_06"][0].MonsterSet),
            Doors = new List<Door>(),
            TextureId = "random2",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 17/28
        room = new DungeonRoom("room:17/28", 17, 28)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Battle,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_mash_02"][0].MonsterSet),
            MashId = 6,
            Prop = null,
            Doors = new List<Door>(),
            TextureId = "random2",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 43/26
        room = new DungeonRoom("room:43/26", 43, 26)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Battle,
            MashId = 6,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_mash_01"][0].MonsterSet),
            Doors = new List<Door>(),
            TextureId = "random3",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 21/26
        room = new DungeonRoom("room:21/26", 21, 26)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Battle,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_mash_02"][0].MonsterSet),
            MashId = 6,
            Prop = null,
            Doors = new List<Door>(),
            TextureId = "random3",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        // Top 20-25
        #region Room 10/23
        room = new DungeonRoom("room:10/23", 10, 23)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "random1",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 18/22
        room = new DungeonRoom("room:18/22", 18, 22)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "random2",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 24/23
        room = new DungeonRoom("room:24/23", 24, 23)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Battle,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_mash_03"][0].MonsterSet),
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "random3",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 30/24
        room = new DungeonRoom("room:30/24", 30, 24)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "random1",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 35/21
        room = new DungeonRoom("room:35/21", 35, 21)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Battle,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_mash_10"][0].MonsterSet),
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "random2",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 40/23
        room = new DungeonRoom("room:40/23", 40, 23)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "random3",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        // Mid 15-19
        #region Room 5/17
        room = new DungeonRoom("room:5/17", 5, 17)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Battle,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_mash_01"][0].MonsterSet),
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "random1",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 11/17
        room = new DungeonRoom("room:11/17", 11, 17)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "random2",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 18/16
        room = new DungeonRoom("room:18/16", 18, 16)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Battle,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_mash_09"][0].MonsterSet),
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "random3",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 24/17
        room = new DungeonRoom("room:24/17", 24, 17)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "random1",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 30/18
        room = new DungeonRoom("room:30/18", 30, 18)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "random2",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 41/17
        room = new DungeonRoom("room:41/17", 41, 17)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Battle,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_mash_05"][0].MonsterSet),
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "random3",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        // Bot 8-14
        #region Room 12/11
        room = new DungeonRoom("room:12/11", 12, 11)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Battle,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_mash_07"][0].MonsterSet),
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "random1",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 19/10
        room = new DungeonRoom("room:19/10", 19, 10)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "random2",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion 
        #region Room 26/10 Boss Teleport
        room = new DungeonRoom("room:26/10", 26, 10)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.BattleCurio,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_teleport"][0].MonsterSet),
            MashId = 6,
            Prop = new Curio("teleporter") { IsQuestCurio = true, },
            Doors = new List<Door>(),
            TextureId = "plot_darkest_dungeon_3_final",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 32/12
        room = new DungeonRoom("room:32/12", 32, 12)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "random3",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 38/13
        room = new DungeonRoom("room:38/13", 38, 13)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "random1",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 44/13
        room = new DungeonRoom("room:44/13", 44, 13)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Battle,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_mash_04"][0].MonsterSet),
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "random2",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        // Bot 1-7
        #region Room 16/1
        room = new DungeonRoom("room:16/1", 16, 1)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Battle,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_mash_02"][0].MonsterSet),
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "random1",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 19/4
        room = new DungeonRoom("room:19/4", 19, 4)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "random2",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 25/4
        room = new DungeonRoom("room:25/4", 25, 4)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "random3",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 32/6
        room = new DungeonRoom("room:32/6", 32, 6)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "random1",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 35/3
        room = new DungeonRoom("room:35/3", 35, 3)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Battle,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_mash_03"][0].MonsterSet),
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "random2",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 38/6
        room = new DungeonRoom("room:38/6", 38, 6)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Battle,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_mash_07"][0].MonsterSet),
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "random3",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #endregion

        #region Hallways
        // Left Part
        #region 2/29 to 11/29
        Hallway hallway = new Hallway("2/29_to_11/29");
        hallway.RoomA = saveData.Dungeon.Rooms["room:2/29"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:11/29"];
        hallway.RoomA.Doors.Add(new Door("room:2/29", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:11/29", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 5, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX + 6, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("6", hallway.RoomA.GridX + 7, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("7", hallway.RoomA.GridX + 8, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 11/29 to 17/28
        hallway = new Hallway("11/29_to_17/28");
        hallway.RoomA = saveData.Dungeon.Rooms["room:11/29"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:17/28"];
        hallway.RoomA.Doors.Add(new Door("room:11/29", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:17/28", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 3, hallway.RoomA.GridY - 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 4, hallway.RoomA.GridY - 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 5, hallway.RoomA.GridY - 1, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 11/29 to 10/23
        hallway = new Hallway("11/29_to_10/23");
        hallway.RoomA = saveData.Dungeon.Rooms["room:11/29"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:10/23"];
        hallway.RoomA.Doors.Add(new Door("room:11/29", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:10/23", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY - 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_mash_06"][0].MonsterSet),
            },
            new HallSector("3", hallway.RoomA.GridX - 1, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX - 1, hallway.RoomA.GridY - 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX - 1, hallway.RoomA.GridY - 5, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 10/23 to 11/17
        hallway = new Hallway("10/23_to_11/17");
        hallway.RoomA = saveData.Dungeon.Rooms["room:10/23"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:11/17"];
        hallway.RoomA.Doors.Add(new Door("room:10/23", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:11/17", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY - 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_teleport"][0].MonsterSet),
            },
            new HallSector("3", hallway.RoomA.GridX + 1, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 1, hallway.RoomA.GridY - 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 1, hallway.RoomA.GridY - 5, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 10/23 to 18/22
        hallway = new Hallway("10/23_to_18/22");
        hallway.RoomA = saveData.Dungeon.Rooms["room:10/23"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:18/22"];
        hallway.RoomA.Doors.Add(new Door("room:10/23", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:18/22", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_mash_04"][0].MonsterSet),
            },
            new HallSector("3", hallway.RoomA.GridX + 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 4, hallway.RoomA.GridY - 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX + 5, hallway.RoomA.GridY - 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("6", hallway.RoomA.GridX + 6, hallway.RoomA.GridY - 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("7", hallway.RoomA.GridX + 7, hallway.RoomA.GridY - 1, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 17/28 to 18/22
        hallway = new Hallway("17/28_to_18/22");
        hallway.RoomA = saveData.Dungeon.Rooms["room:17/28"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:18/22"];
        hallway.RoomA.Doors.Add(new Door("room:17/28", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:18/22", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY - 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 1, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 1, hallway.RoomA.GridY - 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 1, hallway.RoomA.GridY - 5, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 11/17 to 12/11
        hallway = new Hallway("11/17_to_12/11");
        hallway.RoomA = saveData.Dungeon.Rooms["room:11/17"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:12/11"];
        hallway.RoomA.Doors.Add(new Door("room:11/17", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:12/11", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY - 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 1, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 1, hallway.RoomA.GridY - 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 1, hallway.RoomA.GridY - 5, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 11/17 to 5/17
        hallway = new Hallway("11/17_to_5/17");
        hallway.RoomA = saveData.Dungeon.Rooms["room:11/17"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:5/17"];
        hallway.RoomA.Doors.Add(new Door("room:11/17", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:5/17", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX - 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX - 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX - 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX - 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX - 5, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 11/17 to 18/16
        hallway = new Hallway("11/17_to_18/16");
        hallway.RoomA = saveData.Dungeon.Rooms["room:11/17"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:18/16"];
        hallway.RoomA.Doors.Add(new Door("room:11/17", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:18/16", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 3, hallway.RoomA.GridY - 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 4, hallway.RoomA.GridY - 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_mash_08"][0].MonsterSet),
            },
            new HallSector("5", hallway.RoomA.GridX + 5, hallway.RoomA.GridY - 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 6, hallway.RoomA.GridY - 1, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 12/11 to 18/16
        hallway = new Hallway("12/11_to_19/10");
        hallway.RoomA = saveData.Dungeon.Rooms["room:12/11"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:19/10"];
        hallway.RoomA.Doors.Add(new Door("room:12/11", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:19/10", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 3, hallway.RoomA.GridY - 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 4, hallway.RoomA.GridY - 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_teleport"][0].MonsterSet),
            },
            new HallSector("5", hallway.RoomA.GridX + 5, hallway.RoomA.GridY - 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 6, hallway.RoomA.GridY - 1, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 18/16 to 18/22
        hallway = new Hallway("18/16_to_19/10");
        hallway.RoomA = saveData.Dungeon.Rooms["room:18/16"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:19/10"];
        hallway.RoomA.Doors.Add(new Door("room:18/16", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:19/10", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY - 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 1, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 1, hallway.RoomA.GridY - 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 1, hallway.RoomA.GridY - 5, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 18/22 to 18/16
        hallway = new Hallway("18/22_to_18/16");
        hallway.RoomA = saveData.Dungeon.Rooms["room:18/22"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:18/16"];
        hallway.RoomA.Doors.Add(new Door("room:18/22", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:18/16", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY - 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_teleport"][0].MonsterSet),
            },
            new HallSector("3", hallway.RoomA.GridX, hallway.RoomA.GridY - 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX, hallway.RoomA.GridY - 5, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 19/10 to 19/4
        hallway = new Hallway("19/10_to_19/4");
        hallway.RoomA = saveData.Dungeon.Rooms["room:19/10"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:19/4"];
        hallway.RoomA.Doors.Add(new Door("room:19/10", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:19/4", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY - 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX, hallway.RoomA.GridY - 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX, hallway.RoomA.GridY - 5, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 19/4 to 16/1
        hallway = new Hallway("19/4_to_16/1");
        hallway.RoomA = saveData.Dungeon.Rooms["room:19/4"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:16/1"];
        hallway.RoomA.Doors.Add(new Door("room:19/4", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:16/1", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY - 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX - 1, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX - 2, hallway.RoomA.GridY - 3, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        // Middle Part
        #region 25/4 to 19/4
        hallway = new Hallway("25/4_to_19/4");
        hallway.RoomA = saveData.Dungeon.Rooms["room:25/4"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:19/4"];
        hallway.RoomA.Doors.Add(new Door("room:25/4", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:19/4", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX - 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX - 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX - 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_teleport"][0].MonsterSet),
            },
            new HallSector("3", hallway.RoomA.GridX - 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX - 5, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 26/10 to 25/4
        hallway = new Hallway("26/10_to_25/4");
        hallway.RoomA = saveData.Dungeon.Rooms["room:26/10"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:25/4"];
        hallway.RoomA.Doors.Add(new Door("room:26/10", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:25/4", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY - 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_mash_08"][0].MonsterSet),
            },
            new HallSector("3", hallway.RoomA.GridX - 1, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX - 1, hallway.RoomA.GridY - 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX - 1, hallway.RoomA.GridY - 5, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 19/10 to 26/10
        hallway = new Hallway("19/10_to_26/10");
        hallway.RoomA = saveData.Dungeon.Rooms["room:19/10"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:26/10"];
        hallway.RoomA.Doors.Add(new Door("room:19/10", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:26/10", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_mash_templar_melee"][0].MonsterSet),
            },
            new HallSector("4", hallway.RoomA.GridX + 5, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX + 6, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 18/16 to 24/17
        hallway = new Hallway("18/16_to_24/17");
        hallway.RoomA = saveData.Dungeon.Rooms["room:18/16"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:24/17"];
        hallway.RoomA.Doors.Add(new Door("room:18/16", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:24/17", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 3, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_teleport"][0].MonsterSet),
            },
            new HallSector("4", hallway.RoomA.GridX + 4, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX + 5, hallway.RoomA.GridY + 1, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 18/22 to 24/23
        hallway = new Hallway("18/22_to_24/23");
        hallway.RoomA = saveData.Dungeon.Rooms["room:18/22"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:24/23"];
        hallway.RoomA.Doors.Add(new Door("room:18/22", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:24/23", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 3, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_teleport"][0].MonsterSet),
            },
            new HallSector("4", hallway.RoomA.GridX + 4, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX + 5, hallway.RoomA.GridY + 1, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 24/23 to 30/24
        hallway = new Hallway("24/23_to_30/24");
        hallway.RoomA = saveData.Dungeon.Rooms["room:24/23"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:30/24"];
        hallway.RoomA.Doors.Add(new Door("room:24/23", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:30/24", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_teleport"][0].MonsterSet),
            },
            new HallSector("3", hallway.RoomA.GridX + 3, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 4, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX + 5, hallway.RoomA.GridY + 1, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 24/17 to 30/24
        hallway = new Hallway("24/17_to_30/18");
        hallway.RoomA = saveData.Dungeon.Rooms["room:24/17"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:30/18"];
        hallway.RoomA.Doors.Add(new Door("room:24/17", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:30/18", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 3, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 4, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX + 5, hallway.RoomA.GridY + 1, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 24/23 to 21/26
        hallway = new Hallway("24/23_to_21/26");
        hallway.RoomA = saveData.Dungeon.Rooms["room:24/23"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:21/26"];
        hallway.RoomA.Doors.Add(new Door("room:24/23", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:21/26", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY + 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY + 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX - 1, hallway.RoomA.GridY + 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX - 2, hallway.RoomA.GridY + 3, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 24/23 to 24/17
        hallway = new Hallway("24/23_to_24/17");
        hallway.RoomA = saveData.Dungeon.Rooms["room:24/23"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:24/17"];
        hallway.RoomA.Doors.Add(new Door("room:24/23", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:24/17", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY - 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX, hallway.RoomA.GridY - 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX, hallway.RoomA.GridY - 5, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 24/17 to 26/10
        hallway = new Hallway("24/17_to_26/10");
        hallway.RoomA = saveData.Dungeon.Rooms["room:24/17"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:26/10"];
        hallway.RoomA.Doors.Add(new Door("room:24/17", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:26/10", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY - 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 1, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 2, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX + 2, hallway.RoomA.GridY - 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_mash_templar_ranged"][0].MonsterSet),
            },
            new HallSector("6", hallway.RoomA.GridX + 2, hallway.RoomA.GridY - 5, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("7", hallway.RoomA.GridX + 2, hallway.RoomA.GridY - 6, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 26/10 to 32/12
        hallway = new Hallway("26/10_to_32/12");
        hallway.RoomA = saveData.Dungeon.Rooms["room:26/10"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:32/12"];
        hallway.RoomA.Doors.Add(new Door("room:26/10", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:32/12", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 3, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 3, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_mash_09"][0].MonsterSet),
            },
            new HallSector("5", hallway.RoomA.GridX + 4, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("6", hallway.RoomA.GridX + 5, hallway.RoomA.GridY + 2, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 25/4 to 32/6
        hallway = new Hallway("25/4_to_32/6");
        hallway.RoomA = saveData.Dungeon.Rooms["room:25/4"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:32/6"];
        hallway.RoomA.Doors.Add(new Door("room:25/4", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:32/6", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 4, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX + 4, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_mash_10"][0].MonsterSet),
            },
            new HallSector("6", hallway.RoomA.GridX + 5, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("7", hallway.RoomA.GridX + 6, hallway.RoomA.GridY + 2, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 32/6 to 35/3
        hallway = new Hallway("32/6_to_35/3");
        hallway.RoomA = saveData.Dungeon.Rooms["room:32/6"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:35/3"];
        hallway.RoomA.Doors.Add(new Door("room:32/6", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:35/3", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY - 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 1, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 2, hallway.RoomA.GridY - 3, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 32/12 to 32/6
        hallway = new Hallway("32/12_to_32/6");
        hallway.RoomA = saveData.Dungeon.Rooms["room:32/12"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:32/6"];
        hallway.RoomA.Doors.Add(new Door("room:32/12", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:32/6", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY - 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_teleport"][0].MonsterSet),
            },
            new HallSector("3", hallway.RoomA.GridX, hallway.RoomA.GridY - 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX, hallway.RoomA.GridY - 5, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 30/24 to 30/18
        hallway = new Hallway("30/24_to_30/18");
        hallway.RoomA = saveData.Dungeon.Rooms["room:30/24"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:30/18"];
        hallway.RoomA.Doors.Add(new Door("room:30/24", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:30/18", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY - 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_mash_06"][0].MonsterSet),
            },
            new HallSector("3", hallway.RoomA.GridX, hallway.RoomA.GridY - 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX, hallway.RoomA.GridY - 5, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 30/24 to 31/29
        hallway = new Hallway("30/24_to_31/29");
        hallway.RoomA = saveData.Dungeon.Rooms["room:30/24"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:31/29"];
        hallway.RoomA.Doors.Add(new Door("room:30/24", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:31/29", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY + 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX + 1, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 1, hallway.RoomA.GridY + 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_mash_templar_melee"][0].MonsterSet),
            },
            new HallSector("4", hallway.RoomA.GridX + 1, hallway.RoomA.GridY + 4, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 30/18 to 32/12
        hallway = new Hallway("30/18_to_32/12");
        hallway.RoomA = saveData.Dungeon.Rooms["room:30/18"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:32/12"];
        hallway.RoomA.Doors.Add(new Door("room:30/18", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:32/12", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY - 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 1, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_teleport"][0].MonsterSet),
            },
            new HallSector("4", hallway.RoomA.GridX + 2, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX + 2, hallway.RoomA.GridY - 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("6", hallway.RoomA.GridX + 2, hallway.RoomA.GridY - 5, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        // Right Part
        #region 32/12 to 38/13
        hallway = new Hallway("32/12_to_38/13");
        hallway.RoomA = saveData.Dungeon.Rooms["room:32/12"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:38/13"];
        hallway.RoomA.Doors.Add(new Door("room:32/12", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:38/13", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 3, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_teleport"][0].MonsterSet),
            },
            new HallSector("4", hallway.RoomA.GridX + 4, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX + 5, hallway.RoomA.GridY + 1, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 38/6 to 32/6
        hallway = new Hallway("38/6_to_32/6");
        hallway.RoomA = saveData.Dungeon.Rooms["room:38/6"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:32/6"];
        hallway.RoomA.Doors.Add(new Door("room:38/6", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:32/6", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX - 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX - 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX - 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX - 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX - 5, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 38/13 to 38/6
        hallway = new Hallway("38/13_to_38/6");
        hallway.RoomA = saveData.Dungeon.Rooms["room:38/13"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:38/6"];
        hallway.RoomA.Doors.Add(new Door("room:38/13", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:38/6", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY - 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_mash_05"][0].MonsterSet),
            },
            new HallSector("3", hallway.RoomA.GridX, hallway.RoomA.GridY - 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX, hallway.RoomA.GridY - 5, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX, hallway.RoomA.GridY - 6, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 38/13 to 44/13
        hallway = new Hallway("38/13_to_44/13");
        hallway.RoomA = saveData.Dungeon.Rooms["room:38/13"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:44/13"];
        hallway.RoomA.Doors.Add(new Door("room:38/13", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:44/13", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 5, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 31/29 to 37/29
        hallway = new Hallway("31/29_to_37/29");
        hallway.RoomA = saveData.Dungeon.Rooms["room:31/29"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:37/29"];
        hallway.RoomA.Doors.Add(new Door("room:31/29", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:37/29", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 5, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 43/26 to 40/23
        hallway = new Hallway("43/26_to_40/23");
        hallway.RoomA = saveData.Dungeon.Rooms["room:43/26"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:40/23"];
        hallway.RoomA.Doors.Add(new Door("room:43/26", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:40/23", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY - 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX - 1, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX - 2, hallway.RoomA.GridY - 3, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 30/24 to 35/21
        hallway = new Hallway("30/24_to_35/21");
        hallway.RoomA = saveData.Dungeon.Rooms["room:30/24"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:35/21"];
        hallway.RoomA.Doors.Add(new Door("room:30/24", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:35/21", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 3, hallway.RoomA.GridY - 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 3, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX + 3, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("6", hallway.RoomA.GridX + 4, hallway.RoomA.GridY - 3, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 30/18 to 35/21
        hallway = new Hallway("30/18_to_35/21");
        hallway.RoomA = saveData.Dungeon.Rooms["room:30/18"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:35/21"];
        hallway.RoomA.Doors.Add(new Door("room:30/18", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:35/21", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_mash_04"][0].MonsterSet),
            },
            new HallSector("3", hallway.RoomA.GridX + 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 5, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX + 5, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("6", hallway.RoomA.GridX + 5, hallway.RoomA.GridY + 2, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 35/21 to 40/23
        hallway = new Hallway("35/21_to_40/23");
        hallway.RoomA = saveData.Dungeon.Rooms["room:35/21"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:40/23"];
        hallway.RoomA.Doors.Add(new Door("room:35/21", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:40/23", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_teleport"][0].MonsterSet),
            },
            new HallSector("2", hallway.RoomA.GridX + 2, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 2, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 3, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX + 4, hallway.RoomA.GridY + 2, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 37/29 to 40/23
        hallway = new Hallway("37/29_to_40/23");
        hallway.RoomA = saveData.Dungeon.Rooms["room:37/29"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:40/23"];
        hallway.RoomA.Doors.Add(new Door("room:37/29", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:40/23", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY - 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 1, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_mash_templar_ranged"][0].MonsterSet),
            },
            new HallSector("4", hallway.RoomA.GridX + 2, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX + 3, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("6", hallway.RoomA.GridX + 3, hallway.RoomA.GridY - 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("7", hallway.RoomA.GridX + 3, hallway.RoomA.GridY - 5, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 40/23 to 41/17
        hallway = new Hallway("40/23_to_41/17");
        hallway.RoomA = saveData.Dungeon.Rooms["room:40/23"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:41/17"];
        hallway.RoomA.Doors.Add(new Door("room:40/23", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:41/17", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY - 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 1, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 1, hallway.RoomA.GridY - 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 1, hallway.RoomA.GridY - 5, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 38/13 to 41/17
        hallway = new Hallway("38/13_to_41/17");
        hallway.RoomA = saveData.Dungeon.Rooms["room:38/13"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:41/17"];
        hallway.RoomA.Doors.Add(new Door("room:38/13", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:41/17", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY + 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY + 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["dd_quest_3_mash_01"][0].MonsterSet),
            },
            new HallSector("3", hallway.RoomA.GridX, hallway.RoomA.GridY + 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 1, hallway.RoomA.GridY + 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 2, hallway.RoomA.GridY + 4, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #endregion
        #endregion

        #region Party
        saveData.RaidParty = new RaidPartySaveData()
        {
            IsMovingLeft = false,
            HeroInfo = new List<RaidPartyHeroInfoSaveData>()
            {
                new RaidPartyHeroInfoSaveData()
                {
                Factor = DeathFactor.AttackMonster,
                Killer = "",
                IsAlive = true,
                HeroRosterId = 1,
                },
                new RaidPartyHeroInfoSaveData()
                {
                Factor = DeathFactor.AttackMonster,
                Killer = "",
                IsAlive = true,
                HeroRosterId = 2,
                },
            }
        };
        #endregion

        #region Data
        saveData.ExploredRoomCount = 1;
        saveData.CurrentLocation = "room:2/29";
        saveData.LastRoom = "room:2/29";
        saveData.PreviousLastSector = "";
        saveData.LastSector = "";
        saveData.KilledMonsters = new List<string>();
        saveData.InvestigatedCurios = new List<string>();

        saveData.TorchAmount = 100;
        saveData.MaxTorchAmount = 100;
        saveData.ModifiedMinTorch = -1;
        saveData.ModifiedMaxTorch = -1;
        #endregion

        #region Formation
        saveData.HeroFormationData = new BattleFormationSaveData();
        saveData.HeroFormationData.unitData.Add(new FormationUnitSaveData()
        {
            IsHero = true,
            RosterId = 1,
            Rank = 1,
            CombatInfo = new FormationUnitInfo()
            {
                CombatId = 1,
            }
        });
        saveData.HeroFormationData.unitData.Add(new FormationUnitSaveData()
        {
            IsHero = true,
            RosterId = 2,
            Rank = 2,
            CombatInfo = new FormationUnitInfo()
            {
                CombatId = 2,
            }
        });
        #endregion

        #region Inventory
        saveData.InventoryItems = new List<InventorySlotData>();
        #endregion

        WriteSave(saveData);
        return saveData;
    }

    public static SaveCampaignData WriteDarkestQuestFourSave(SaveCampaignData saveData)
    {
        saveData.IsFirstStart = true;
        saveData.LocationName = "Raid";
        saveData.QuestsCompleted = 0;
        saveData.CurrentWeek = 1;

        saveData.GoldAmount = 1000;
        saveData.BustsAmount = 10;
        saveData.DeedsAmount = 10;
        saveData.PortraitsAmount = 10;
        saveData.CrestsAmount = 10;

        #region Initial Heroes
        saveData.RosterHeroes = new List<SaveHeroData>
        {
            #region Hero 1
            new SaveHeroData()
            {
                RosterId = 1,
                Name = "Reynald",
                HeroClass = "crusader",
                ResolveLevel = 0,
                ResolveXP = 0,
                StressLevel = 10,
                WeaponLevel = 1,
                ArmorLevel = 1,
                LeftTrinketId = "",
                RightTrinketId = "dd_trinket",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("warrior_of_light"),
                    new QuirkInfo("kleptomaniac"),
                    new QuirkInfo("god_fearing"),
                },
            },
            #endregion
            #region Hero 2
            new SaveHeroData()
            {
                RosterId = 2,
                Name = "Dismas",
                HeroClass = "highwayman",
                ResolveLevel = 0,
                ResolveXP = 0,
                StressLevel = 10,
                WeaponLevel = 1,
                ArmorLevel = 1,
                LeftTrinketId = "",
                RightTrinketId = "dd_trinket",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("hard_noggin"),
                    new QuirkInfo("known_cheat"),
                    new QuirkInfo("quick_reflexes"),
                },
            },
            #endregion
        };
        #endregion

        #region StageCoach Heroes
        saveData.StageCoachHeroes = new List<SaveHeroData>();
        #endregion

        #region Initial Hero Purchases
        saveData.InstancedPurchases.Clear();
        for (int i = 0; i < saveData.RosterHeroes.Count; i++)
        {
            var newHeroPurchases = new Dictionary<string, UpgradePurchases>();
            saveData.InstancedPurchases.Add(saveData.RosterHeroes[i].RosterId, newHeroPurchases);
            newHeroPurchases.Add(saveData.RosterHeroes[i].HeroClass + ".weapon",
                new UpgradePurchases(saveData.RosterHeroes[i].HeroClass + ".weapon", new string[0]));
            newHeroPurchases.Add(saveData.RosterHeroes[i].HeroClass + ".armour",
                new UpgradePurchases(saveData.RosterHeroes[i].HeroClass + ".armour", new string[0]));
            var heroClass = DarkestDungeonManager.Data.HeroClasses[saveData.RosterHeroes[i].HeroClass];
            for (int j = 0; j < heroClass.CombatSkills.Count; j++)
                newHeroPurchases.Add(saveData.RosterHeroes[i].HeroClass + "." + heroClass.CombatSkills[j].Id, new UpgradePurchases(
                    saveData.RosterHeroes[i].HeroClass + "." + heroClass.CombatSkills[j].Id, new string[0]));
            for (int j = 0; j < heroClass.CampingSkills.Count; j++)
                newHeroPurchases.Add(heroClass.CampingSkills[j].Id, new UpgradePurchases(
                    heroClass.CampingSkills[j].Id, new string[0]));
        }
        saveData.InstancedPurchases[1]["crusader.smite"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["crusader.zealous_accusation"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["crusader.stunning_blow"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["crusader.bulwark_of_faith"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(0);
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(1);
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(2);
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(3);
        saveData.InstancedPurchases[1]["encourage"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["stand_tall"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["zealous_speech"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[0].SelectedCampingSkillIndexes.Add(0);
        saveData.RosterHeroes[0].SelectedCampingSkillIndexes.Add(4);
        saveData.RosterHeroes[0].SelectedCampingSkillIndexes.Add(5);


        saveData.InstancedPurchases[2]["highwayman.opened_vein"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["highwayman.pistol_shot"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["highwayman.grape_shot_blast"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["highwayman.take_aim"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(6);
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(1);
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(3);
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(4);
        saveData.InstancedPurchases[2]["first_aid"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["clean_guns"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["bandits_sense"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[1].SelectedCampingSkillIndexes.Add(1);
        saveData.RosterHeroes[1].SelectedCampingSkillIndexes.Add(5);
        saveData.RosterHeroes[1].SelectedCampingSkillIndexes.Add(6);
        #endregion

        #region Activity Log
        saveData.ActivityLog = new List<WeekActivityLog>();
        saveData.CompletedPlot = new List<string>();
        saveData.GeneratedQuests = new List<Quest>();
        #endregion

        #region Estate Misc
        saveData.InventoryTrinkets.Clear();

        saveData.WagonTrinkets.Clear();

        saveData.DungeonProgress.Clear();
        saveData.DungeonProgress.Add("crypts", new DungeonProgress("crypts", 0, 0, true, false));
        saveData.DungeonProgress.Add("warrens", new DungeonProgress("warrens", 0, 0, true, false));
        saveData.DungeonProgress.Add("weald", new DungeonProgress("weald", 0, 0, true, false));
        saveData.DungeonProgress.Add("cove", new DungeonProgress("cove", 0, 0, true, false));
        saveData.DungeonProgress.Add("darkestdungeon", new DungeonProgress("darkestdungeon", 1, 0, true, false));
        saveData.DungeonProgress.Add("town", new DungeonProgress("town", 1, 0, true, true));

        saveData.DeathRecords = new List<DeathRecord>();

        saveData.BuildingUpgrades.Clear();

        saveData.BuildingUpgrades.Add("abbey.meditation", new UpgradePurchases("abbey.meditation", new string[0]));
        saveData.BuildingUpgrades.Add("abbey.prayer", new UpgradePurchases("abbey.prayer", new string[0]));
        saveData.BuildingUpgrades.Add("abbey.flagellation", new UpgradePurchases("abbey.flagellation", new string[0]));
        saveData.BuildingUpgrades.Add("tavern.bar", new UpgradePurchases("tavern.bar", new string[0]));
        saveData.BuildingUpgrades.Add("tavern.gambling", new UpgradePurchases("tavern.gambling", new string[0]));
        saveData.BuildingUpgrades.Add("tavern.brothel", new UpgradePurchases("tavern.brothel", new string[0]));
        saveData.BuildingUpgrades.Add("sanitarium.cost", new UpgradePurchases("sanitarium.cost", new string[0]));
        saveData.BuildingUpgrades.Add("sanitarium.disease_quirk_cost", new UpgradePurchases("sanitarium.disease_quirk_cost", new string[0]));
        saveData.BuildingUpgrades.Add("sanitarium.slots", new UpgradePurchases("sanitarium.slots", new string[0]));
        saveData.BuildingUpgrades.Add("blacksmith.weapon", new UpgradePurchases("blacksmith.weapon", new string[0]));
        saveData.BuildingUpgrades.Add("blacksmith.armour", new UpgradePurchases("blacksmith.armour", new string[0]));
        saveData.BuildingUpgrades.Add("blacksmith.cost", new UpgradePurchases("blacksmith.cost", new string[0]));
        saveData.BuildingUpgrades.Add("guild.skill_levels", new UpgradePurchases("guild.skill_levels", new string[0]));
        saveData.BuildingUpgrades.Add("guild.cost", new UpgradePurchases("guild.cost", new string[0]));
        saveData.BuildingUpgrades.Add("camping_trainer.cost", new UpgradePurchases("camping_trainer.cost", new string[0]));
        saveData.BuildingUpgrades.Add("nomad_wagon.numitems", new UpgradePurchases("nomad_wagon.numitems", new string[0]));
        saveData.BuildingUpgrades.Add("nomad_wagon.cost", new UpgradePurchases("nomad_wagon.cost", new string[0]));
        saveData.BuildingUpgrades.Add("stage_coach.numrecruits", new UpgradePurchases("stage_coach.numrecruits", new string[0]));
        saveData.BuildingUpgrades.Add("stage_coach.rostersize", new UpgradePurchases("stage_coach.rostersize", new string[0]));
        saveData.BuildingUpgrades.Add("stage_coach.upgraded_recruits", new UpgradePurchases("stage_coach.upgraded_recruits", new string[0]));
        #endregion

        #region ActivitySlots
        saveData.AbbeyActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        saveData.TavernActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        saveData.SanitariumActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        #endregion

        saveData.InRaid = true;

        #region Quest
        saveData.QuestCompleted = false;
        saveData.Quest = DarkestDungeonManager.Data.QuestDatabase.PlotQuests.Find(quest =>
            quest.Id == "plot_darkest_dungeon_4").Copy();
        #endregion

        #region Dungeon
        saveData.Dungeon = new Dungeon();
        saveData.Dungeon.Name = saveData.Quest.Dungeon;
        saveData.Dungeon.GridSizeX = 21;
        saveData.Dungeon.GridSizeY = 5;
        saveData.Dungeon.StartingRoomId = "room:2/4";

        #region Rooms
        #region Room 2/4
        DungeonRoom room = new DungeonRoom("room:2/4", 2, 4)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Entrance,
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "plot_darkest_dungeon_4_enter",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 21/4
        room = new DungeonRoom("room:21/4", 21, 4)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Boss,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["darkestdungeon"].BattleMashes.
                    Find(mash => mash.MashId == 6).BossEncounters.Find(enc => enc.MonsterSet.Contains("ancestor_small_D")).MonsterSet),
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "plot_darkest_dungeon_4_final",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #endregion

        #region Hallways
        #region 2/4 to 21/4
        Hallway hallway = new Hallway("2/4_to_21/4");
        hallway.RoomA = saveData.Dungeon.Rooms["room:2/4"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:21/4"];
        hallway.RoomA.Doors.Add(new Door("room:2/4", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:21/4", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 3, hallway.RoomA.GridY - 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 3, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX + 4, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Obstacle,
                Prop = DarkestDungeonManager.Data.Obstacles["ancestor"],
            },
            new HallSector("6", hallway.RoomA.GridX + 5, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Empty,
            },
            new HallSector("7", hallway.RoomA.GridX + 5, hallway.RoomA.GridY - 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Empty,
            },
            new HallSector("8", hallway.RoomA.GridX + 6, hallway.RoomA.GridY - 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Empty,
            },
            new HallSector("9", hallway.RoomA.GridX + 7, hallway.RoomA.GridY - 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Empty,
            },
            new HallSector("10", hallway.RoomA.GridX + 7, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Empty,
            },
            new HallSector("11", hallway.RoomA.GridX + 7, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Empty,
            },
            new HallSector("12", hallway.RoomA.GridX + 8, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Obstacle,
                Prop = DarkestDungeonManager.Data.Obstacles["ancestor"],
            },
            new HallSector("13", hallway.RoomA.GridX + 9, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Empty,
            },
            new HallSector("14", hallway.RoomA.GridX + 10, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Empty,
            },
            new HallSector("15", hallway.RoomA.GridX + 10, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Empty,
            },
            new HallSector("16", hallway.RoomA.GridX + 10, hallway.RoomA.GridY - 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Empty,
            },
            new HallSector("17", hallway.RoomA.GridX + 11, hallway.RoomA.GridY - 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Empty,
            },
            new HallSector("18", hallway.RoomA.GridX + 12, hallway.RoomA.GridY - 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Empty,
            },
            new HallSector("19", hallway.RoomA.GridX + 12, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Empty,
            },
            new HallSector("20", hallway.RoomA.GridX + 12, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Empty,
            },
            new HallSector("21", hallway.RoomA.GridX + 13, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Obstacle,
                Prop = DarkestDungeonManager.Data.Obstacles["ancestor"],
            },
            new HallSector("22", hallway.RoomA.GridX + 14, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Empty,
            },
            new HallSector("23", hallway.RoomA.GridX + 14, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Empty,
            },
            new HallSector("24", hallway.RoomA.GridX + 15, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Empty,
            },
            new HallSector("25", hallway.RoomA.GridX + 16, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Empty,
            },
            new HallSector("26", hallway.RoomA.GridX + 17, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Empty,
            },
            new HallSector("27", hallway.RoomA.GridX + 18, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #endregion
        #endregion

        #region Party
        saveData.RaidParty = new RaidPartySaveData()
        {
            IsMovingLeft = false,
            HeroInfo = new List<RaidPartyHeroInfoSaveData>()
            {
                new RaidPartyHeroInfoSaveData()
                {
                Factor = DeathFactor.AttackMonster,
                Killer = "",
                IsAlive = true,
                HeroRosterId = 1,
                },
                new RaidPartyHeroInfoSaveData()
                {
                Factor = DeathFactor.AttackMonster,
                Killer = "",
                IsAlive = true,
                HeroRosterId = 2,
                },
            }
        };
        #endregion

        #region Data
        saveData.ExploredRoomCount = 1;
        saveData.CurrentLocation = "room:2/4";
        saveData.LastRoom = "room:2/4";
        saveData.PreviousLastSector = "";
        saveData.LastSector = "";
        saveData.KilledMonsters = new List<string>();
        saveData.InvestigatedCurios = new List<string>();

        saveData.TorchAmount = 100;
        saveData.MaxTorchAmount = 100;
        saveData.ModifiedMinTorch = -1;
        saveData.ModifiedMaxTorch = -1;
        #endregion

        #region Formation
        saveData.HeroFormationData = new BattleFormationSaveData();
        saveData.HeroFormationData.unitData.Add(new FormationUnitSaveData()
        {
            IsHero = true,
            RosterId = 1,
            Rank = 1,
            CombatInfo = new FormationUnitInfo()
            {
                CombatId = 1,
            }
        });
        saveData.HeroFormationData.unitData.Add(new FormationUnitSaveData()
        {
            IsHero = true,
            RosterId = 2,
            Rank = 2,
            CombatInfo = new FormationUnitInfo()
            {
                CombatId = 2,
            }
        });
        #endregion

        #region Inventory
        saveData.InventoryItems = new List<InventorySlotData>();
        #endregion

        WriteSave(saveData);
        return saveData;
    }

    public static SaveCampaignData WriteStartingPlusSave(SaveCampaignData saveData)
    {
        saveData.IsFirstStart = true;
        saveData.LocationName = "Raid";
        saveData.QuestsCompleted = 0;
        saveData.CurrentWeek = 1;

        saveData.GoldAmount = 1000;
        saveData.BustsAmount = 10;
        saveData.DeedsAmount = 10;
        saveData.PortraitsAmount = 10;
        saveData.CrestsAmount = 10;

        #region Initial Heroes
        saveData.RosterHeroes = new List<SaveHeroData>
        {
            #region Hero 1
            new SaveHeroData()
            {
                RosterId = 1,
                Name = "Reynald",
                HeroClass = "crusader",
                ResolveLevel = 0,
                ResolveXP = 0,
                StressLevel = 10,
                WeaponLevel = 1,
                ArmorLevel = 1,
                LeftTrinketId = "",
                RightTrinketId = "",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("warrior_of_light"),
                    new QuirkInfo("kleptomaniac"),
                    new QuirkInfo("god_fearing"),
                },
            },
            #endregion
            #region Hero 2
            new SaveHeroData()
            {
                RosterId = 2,
                Name = "Dismas",
                HeroClass = "highwayman",
                ResolveLevel = 0,
                ResolveXP = 0,
                StressLevel = 10,
                WeaponLevel = 1,
                ArmorLevel = 1,
                LeftTrinketId = "",
                RightTrinketId = "",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("hard_noggin"),
                    new QuirkInfo("known_cheat"),
                    new QuirkInfo("quick_reflexes"),
                },
            },
            #endregion
        };
        #endregion

        #region StageCoach Heroes
        saveData.StageCoachHeroes = new List<SaveHeroData>();
        #endregion

        #region Initial Hero Purchases
        saveData.InstancedPurchases.Clear();
        for (int i = 0; i < saveData.RosterHeroes.Count; i++)
        {
            var newHeroPurchases = new Dictionary<string, UpgradePurchases>();
            saveData.InstancedPurchases.Add(saveData.RosterHeroes[i].RosterId, newHeroPurchases);
            newHeroPurchases.Add(saveData.RosterHeroes[i].HeroClass + ".weapon",
                new UpgradePurchases(saveData.RosterHeroes[i].HeroClass + ".weapon", new string[0]));
            newHeroPurchases.Add(saveData.RosterHeroes[i].HeroClass + ".armour",
                new UpgradePurchases(saveData.RosterHeroes[i].HeroClass + ".armour", new string[0]));
            var heroClass = DarkestDungeonManager.Data.HeroClasses[saveData.RosterHeroes[i].HeroClass];
            for (int j = 0; j < heroClass.CombatSkills.Count; j++)
                newHeroPurchases.Add(saveData.RosterHeroes[i].HeroClass + "." + heroClass.CombatSkills[j].Id, new UpgradePurchases(
                    saveData.RosterHeroes[i].HeroClass + "." + heroClass.CombatSkills[j].Id, new string[0]));
            for (int j = 0; j < heroClass.CampingSkills.Count; j++)
                newHeroPurchases.Add(heroClass.CampingSkills[j].Id, new UpgradePurchases(
                    heroClass.CampingSkills[j].Id, new string[0]));
        }
        saveData.InstancedPurchases[1]["crusader.smite"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["crusader.zealous_accusation"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["crusader.stunning_blow"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["crusader.bulwark_of_faith"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(0);
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(1);
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(2);
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(3);
        saveData.InstancedPurchases[1]["encourage"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["stand_tall"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["zealous_speech"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[0].SelectedCampingSkillIndexes.Add(0);
        saveData.RosterHeroes[0].SelectedCampingSkillIndexes.Add(4);
        saveData.RosterHeroes[0].SelectedCampingSkillIndexes.Add(5);

        saveData.InstancedPurchases[2]["highwayman.opened_vein"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["highwayman.pistol_shot"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["highwayman.grape_shot_blast"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["highwayman.take_aim"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(6);
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(1);
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(3);
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(4);
        saveData.InstancedPurchases[2]["first_aid"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["clean_guns"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["bandits_sense"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[1].SelectedCampingSkillIndexes.Add(1);
        saveData.RosterHeroes[1].SelectedCampingSkillIndexes.Add(5);
        saveData.RosterHeroes[1].SelectedCampingSkillIndexes.Add(6);
        #endregion

        #region Activity Log
        saveData.ActivityLog = new List<WeekActivityLog>();
        saveData.CompletedPlot = new List<string>();
        saveData.GeneratedQuests = new List<Quest>();
        #endregion

        #region Estate Misc
        saveData.InventoryTrinkets.Clear();

        saveData.WagonTrinkets.Clear();

        saveData.DungeonProgress.Clear();
        saveData.DungeonProgress.Add("crypts", new DungeonProgress("crypts", 0, 0, true, false));
        saveData.DungeonProgress.Add("warrens", new DungeonProgress("warrens", 0, 0, true, false));
        saveData.DungeonProgress.Add("weald", new DungeonProgress("weald", 0, 0, true, false));
        saveData.DungeonProgress.Add("cove", new DungeonProgress("cove", 0, 0, true, false));
        saveData.DungeonProgress.Add("darkestdungeon", new DungeonProgress("darkestdungeon", 1, 0, true, false));
        saveData.DungeonProgress.Add("town", new DungeonProgress("town", 1, 0, true, true));

        saveData.DeathRecords = new List<DeathRecord>();

        saveData.BuildingUpgrades.Clear();

        saveData.BuildingUpgrades.Add("abbey.meditation", new UpgradePurchases("abbey.meditation", new string[0]));
        saveData.BuildingUpgrades.Add("abbey.prayer", new UpgradePurchases("abbey.prayer", new string[0]));
        saveData.BuildingUpgrades.Add("abbey.flagellation", new UpgradePurchases("abbey.flagellation", new string[0]));
        saveData.BuildingUpgrades.Add("tavern.bar", new UpgradePurchases("tavern.bar", new string[0]));
        saveData.BuildingUpgrades.Add("tavern.gambling", new UpgradePurchases("tavern.gambling", new string[0]));
        saveData.BuildingUpgrades.Add("tavern.brothel", new UpgradePurchases("tavern.brothel", new string[0]));
        saveData.BuildingUpgrades.Add("sanitarium.cost", new UpgradePurchases("sanitarium.cost", new string[0]));
        saveData.BuildingUpgrades.Add("sanitarium.disease_quirk_cost", new UpgradePurchases("sanitarium.disease_quirk_cost", new string[0]));
        saveData.BuildingUpgrades.Add("sanitarium.slots", new UpgradePurchases("sanitarium.slots", new string[0]));
        saveData.BuildingUpgrades.Add("blacksmith.weapon", new UpgradePurchases("blacksmith.weapon", new string[0]));
        saveData.BuildingUpgrades.Add("blacksmith.armour", new UpgradePurchases("blacksmith.armour", new string[0]));
        saveData.BuildingUpgrades.Add("blacksmith.cost", new UpgradePurchases("blacksmith.cost", new string[0]));
        saveData.BuildingUpgrades.Add("guild.skill_levels", new UpgradePurchases("guild.skill_levels", new string[0]));
        saveData.BuildingUpgrades.Add("guild.cost", new UpgradePurchases("guild.cost", new string[0]));
        saveData.BuildingUpgrades.Add("camping_trainer.cost", new UpgradePurchases("camping_trainer.cost", new string[0]));
        saveData.BuildingUpgrades.Add("nomad_wagon.numitems", new UpgradePurchases("nomad_wagon.numitems", new string[0]));
        saveData.BuildingUpgrades.Add("nomad_wagon.cost", new UpgradePurchases("nomad_wagon.cost", new string[0]));
        saveData.BuildingUpgrades.Add("stage_coach.numrecruits", new UpgradePurchases("stage_coach.numrecruits", new string[0]));
        saveData.BuildingUpgrades.Add("stage_coach.rostersize", new UpgradePurchases("stage_coach.rostersize", new string[0]));
        saveData.BuildingUpgrades.Add("stage_coach.upgraded_recruits", new UpgradePurchases("stage_coach.upgraded_recruits", new string[0]));
        #endregion

        #region ActivitySlots
        saveData.AbbeyActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        saveData.TavernActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        saveData.SanitariumActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        #endregion

        saveData.InRaid = true;

        #region Quest
        saveData.QuestCompleted = false;
        saveData.Quest = new PlotQuest()
        {
            IsPlotQuest = true,
            Id = "tutorial",
            Difficulty = 1,
            Type = "tutorial_room",
            Dungeon = "weald",
            DungeonLevel = 1,
            Goal = DarkestDungeonManager.Data.QuestDatabase.QuestGoals["tutorial_final_room"],
            Length = 1,
            PlotTrinket = new PlotTrinketReward() { Amount = 0, Rarity = "very_common" },
            Reward = new CompletionReward()
            {
                ResolveXP = 2,
                ItemDefinitions = new List<ItemDefinition>()
                 {
                     new ItemDefinition("gold", "", 5000),
                 }
            },

            CanRetreat = false,
            CompletionDungeonXp = false,
        };
        #endregion

        #region Dungeon
        saveData.Dungeon = new Dungeon();
        saveData.Dungeon.Name = "weald";
        saveData.Dungeon.GridSizeX = 9;
        saveData.Dungeon.GridSizeY = 1;
        saveData.Dungeon.StartingRoomId = "room1_1";

        DungeonRoom room = new DungeonRoom("room1_1", 1, 1)
        {
            Knowledge = Knowledge.Completed,
            Type = AreaType.Entrance,
            MashId = 1,
            Prop = null,
            BattleEncounter = null,
            Doors = new List<Door>(),
            TextureId = "effigy_0",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);

        room = new DungeonRoom("room2_1", 8, 1)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.BattleTresure,
            MashId = 1,
            Prop = DarkestDungeonManager.Data.Curios["bandits_trapped_chest"],
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["weald"].BattleMashes.
                    Find(mash => mash.MashId == 1).NamedEncounters["tutorial_2"][0].MonsterSet),
            Doors = new List<Door>(),
            TextureId = "effigy_1",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);

        Hallway hallway = new Hallway("hallroom2_1_room1_1");
        hallway.RoomA = saveData.Dungeon.Rooms["room2_1"];
        hallway.RoomB = saveData.Dungeon.Rooms["room1_1"];
        hallway.RoomA.Doors.Add(new Door("room2_1", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room1_1", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", 7, 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", 6, 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "7",
                Type = AreaType.Curio,
                Prop = DarkestDungeonManager.Data.Curios["open_grave"],
            },
            new HallSector("2", 5, 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "8",
                Type = AreaType.Empty,
            },
            new HallSector("3", 4, 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "2",
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["weald"].BattleMashes.
                    Find(mash => mash.MashId == 1).NamedEncounters["tutorial_1"][0].MonsterSet),
            },
            new HallSector("4", 3, 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = "1",
                Type = AreaType.Empty,
            },
            new HallSector("5", 2, 1, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion

        #region Party
        saveData.RaidParty = new RaidPartySaveData()
        {
            IsMovingLeft = false,
            HeroInfo = new List<RaidPartyHeroInfoSaveData>()
            {
                new RaidPartyHeroInfoSaveData()
                {
                Factor = DeathFactor.AttackMonster,
                Killer = "",
                IsAlive = true,
                HeroRosterId = 1,
                },
                new RaidPartyHeroInfoSaveData()
                {
                Factor = DeathFactor.AttackMonster,
                Killer = "",
                IsAlive = true,
                HeroRosterId = 2,
                },
            }
        };
        #endregion

        #region Data
        saveData.ExploredRoomCount = 1;
        saveData.CurrentLocation = "room1_1";
        saveData.LastRoom = "room1_1";
        saveData.PreviousLastSector = "";
        saveData.LastSector = "";
        saveData.KilledMonsters = new List<string>();
        saveData.InvestigatedCurios = new List<string>();

        saveData.TorchAmount = 100;
        saveData.MaxTorchAmount = 100;
        saveData.ModifiedMinTorch = -1;
        saveData.ModifiedMaxTorch = -1;
        #endregion

        #region Formation
        saveData.HeroFormationData = new BattleFormationSaveData();
        saveData.HeroFormationData.unitData.Add(new FormationUnitSaveData()
        {
            IsHero = true,
            RosterId = 1,
            Rank = 1,
            CombatInfo = new FormationUnitInfo()
            {
                CombatId = 1,
            }
        });
        saveData.HeroFormationData.unitData.Add(new FormationUnitSaveData()
        {
            IsHero = true,
            RosterId = 2,
            Rank = 2,
            CombatInfo = new FormationUnitInfo()
            {
                CombatId = 2,
            }
        });
        #endregion

        #region Inventory
        saveData.InventoryItems = new List<InventorySlotData>();
        #endregion

        WriteSave(saveData);
        return saveData;
    }

    public static SaveCampaignData WriteTownInvasionSave(SaveCampaignData saveData)
    {
        saveData.IsFirstStart = true;
        saveData.LocationName = "Raid";
        saveData.QuestsCompleted = 0;
        saveData.CurrentWeek = 1;

        saveData.GoldAmount = 1000;
        saveData.BustsAmount = 10;
        saveData.DeedsAmount = 10;
        saveData.PortraitsAmount = 10;
        saveData.CrestsAmount = 10;

        #region Initial Heroes
        saveData.RosterHeroes = new List<SaveHeroData>
        {
            #region Hero 1
            new SaveHeroData()
            {
                RosterId = 1,
                Name = "Reynald",
                HeroClass = "crusader",
                ResolveLevel = 0,
                ResolveXP = 0,
                StressLevel = 10,
                WeaponLevel = 1,
                ArmorLevel = 1,
                LeftTrinketId = "",
                RightTrinketId = "dd_trinket",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("warrior_of_light"),
                    new QuirkInfo("kleptomaniac"),
                    new QuirkInfo("god_fearing"),
                },
            },
            #endregion
            #region Hero 2
            new SaveHeroData()
            {
                RosterId = 2,
                Name = "Dismas",
                HeroClass = "highwayman",
                ResolveLevel = 0,
                ResolveXP = 0,
                StressLevel = 10,
                WeaponLevel = 1,
                ArmorLevel = 1,
                LeftTrinketId = "",
                RightTrinketId = "dd_trinket",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("hard_noggin"),
                    new QuirkInfo("known_cheat"),
                    new QuirkInfo("quick_reflexes"),
                },
            },
            #endregion
        };
        #endregion

        #region StageCoach Heroes
        saveData.StageCoachHeroes = new List<SaveHeroData>();
        #endregion

        #region Initial Hero Purchases
        saveData.InstancedPurchases.Clear();
        for (int i = 0; i < saveData.RosterHeroes.Count; i++)
        {
            var newHeroPurchases = new Dictionary<string, UpgradePurchases>();
            saveData.InstancedPurchases.Add(saveData.RosterHeroes[i].RosterId, newHeroPurchases);
            newHeroPurchases.Add(saveData.RosterHeroes[i].HeroClass + ".weapon",
                new UpgradePurchases(saveData.RosterHeroes[i].HeroClass + ".weapon", new string[0]));
            newHeroPurchases.Add(saveData.RosterHeroes[i].HeroClass + ".armour",
                new UpgradePurchases(saveData.RosterHeroes[i].HeroClass + ".armour", new string[0]));
            var heroClass = DarkestDungeonManager.Data.HeroClasses[saveData.RosterHeroes[i].HeroClass];
            for (int j = 0; j < heroClass.CombatSkills.Count; j++)
                newHeroPurchases.Add(saveData.RosterHeroes[i].HeroClass + "." + heroClass.CombatSkills[j].Id, new UpgradePurchases(
                    saveData.RosterHeroes[i].HeroClass + "." + heroClass.CombatSkills[j].Id, new string[0]));
            for (int j = 0; j < heroClass.CampingSkills.Count; j++)
                newHeroPurchases.Add(heroClass.CampingSkills[j].Id, new UpgradePurchases(
                    heroClass.CampingSkills[j].Id, new string[0]));
        }
        saveData.InstancedPurchases[1]["crusader.smite"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["crusader.zealous_accusation"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["crusader.stunning_blow"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["crusader.bulwark_of_faith"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(0);
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(1);
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(2);
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(3);
        saveData.InstancedPurchases[1]["encourage"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["stand_tall"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["zealous_speech"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[0].SelectedCampingSkillIndexes.Add(0);
        saveData.RosterHeroes[0].SelectedCampingSkillIndexes.Add(4);
        saveData.RosterHeroes[0].SelectedCampingSkillIndexes.Add(5);


        saveData.InstancedPurchases[2]["highwayman.opened_vein"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["highwayman.pistol_shot"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["highwayman.grape_shot_blast"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["highwayman.take_aim"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(6);
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(1);
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(3);
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(4);
        saveData.InstancedPurchases[2]["first_aid"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["clean_guns"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["bandits_sense"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[1].SelectedCampingSkillIndexes.Add(1);
        saveData.RosterHeroes[1].SelectedCampingSkillIndexes.Add(5);
        saveData.RosterHeroes[1].SelectedCampingSkillIndexes.Add(6);
        #endregion

        #region Activity Log
        saveData.ActivityLog = new List<WeekActivityLog>();
        saveData.CompletedPlot = new List<string>();
        saveData.GeneratedQuests = new List<Quest>();
        #endregion

        #region Estate Misc
        saveData.InventoryTrinkets.Clear();

        saveData.WagonTrinkets.Clear();

        saveData.DungeonProgress.Clear();
        saveData.DungeonProgress.Add("crypts", new DungeonProgress("crypts", 0, 0, true, false));
        saveData.DungeonProgress.Add("warrens", new DungeonProgress("warrens", 0, 0, true, false));
        saveData.DungeonProgress.Add("weald", new DungeonProgress("weald", 0, 0, true, false));
        saveData.DungeonProgress.Add("cove", new DungeonProgress("cove", 0, 0, true, false));
        saveData.DungeonProgress.Add("darkestdungeon", new DungeonProgress("darkestdungeon", 1, 0, true, false));
        saveData.DungeonProgress.Add("town", new DungeonProgress("town", 1, 0, true, true));

        saveData.DeathRecords = new List<DeathRecord>();

        saveData.BuildingUpgrades.Clear();

        saveData.BuildingUpgrades.Add("abbey.meditation", new UpgradePurchases("abbey.meditation", new string[0]));
        saveData.BuildingUpgrades.Add("abbey.prayer", new UpgradePurchases("abbey.prayer", new string[0]));
        saveData.BuildingUpgrades.Add("abbey.flagellation", new UpgradePurchases("abbey.flagellation", new string[0]));
        saveData.BuildingUpgrades.Add("tavern.bar", new UpgradePurchases("tavern.bar", new string[0]));
        saveData.BuildingUpgrades.Add("tavern.gambling", new UpgradePurchases("tavern.gambling", new string[0]));
        saveData.BuildingUpgrades.Add("tavern.brothel", new UpgradePurchases("tavern.brothel", new string[0]));
        saveData.BuildingUpgrades.Add("sanitarium.cost", new UpgradePurchases("sanitarium.cost", new string[0]));
        saveData.BuildingUpgrades.Add("sanitarium.disease_quirk_cost", new UpgradePurchases("sanitarium.disease_quirk_cost", new string[0]));
        saveData.BuildingUpgrades.Add("sanitarium.slots", new UpgradePurchases("sanitarium.slots", new string[0]));
        saveData.BuildingUpgrades.Add("blacksmith.weapon", new UpgradePurchases("blacksmith.weapon", new string[0]));
        saveData.BuildingUpgrades.Add("blacksmith.armour", new UpgradePurchases("blacksmith.armour", new string[0]));
        saveData.BuildingUpgrades.Add("blacksmith.cost", new UpgradePurchases("blacksmith.cost", new string[0]));
        saveData.BuildingUpgrades.Add("guild.skill_levels", new UpgradePurchases("guild.skill_levels", new string[0]));
        saveData.BuildingUpgrades.Add("guild.cost", new UpgradePurchases("guild.cost", new string[0]));
        saveData.BuildingUpgrades.Add("camping_trainer.cost", new UpgradePurchases("camping_trainer.cost", new string[0]));
        saveData.BuildingUpgrades.Add("nomad_wagon.numitems", new UpgradePurchases("nomad_wagon.numitems", new string[0]));
        saveData.BuildingUpgrades.Add("nomad_wagon.cost", new UpgradePurchases("nomad_wagon.cost", new string[0]));
        saveData.BuildingUpgrades.Add("stage_coach.numrecruits", new UpgradePurchases("stage_coach.numrecruits", new string[0]));
        saveData.BuildingUpgrades.Add("stage_coach.rostersize", new UpgradePurchases("stage_coach.rostersize", new string[0]));
        saveData.BuildingUpgrades.Add("stage_coach.upgraded_recruits", new UpgradePurchases("stage_coach.upgraded_recruits", new string[0]));
        #endregion

        #region ActivitySlots
        saveData.AbbeyActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        saveData.TavernActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        saveData.SanitariumActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        #endregion

        saveData.InRaid = true;

        #region Quest
        saveData.QuestCompleted = false;
        saveData.Quest = DarkestDungeonManager.Data.QuestDatabase.PlotQuests.Find(quest =>
            quest.Id == "plot_town_invasion_0").Copy();
        #endregion

        #region Dungeon
        saveData.Dungeon = new Dungeon();
        saveData.Dungeon.Name = saveData.Quest.Dungeon;
        saveData.Dungeon.GridSizeX = 33;
        saveData.Dungeon.GridSizeY = 30;
        saveData.Dungeon.StartingRoomId = "room:17/13";

        #region Rooms
        #region Room 17/13 Entrance
        DungeonRoom room = new DungeonRoom("room:17/13", 17, 13)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Entrance,
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "start",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 17/23 Vvulf
        room = new DungeonRoom("room:17/23", 17, 23)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Boss,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["town"].BattleMashes.
                    Find(mash => mash.MashId == 6).BossEncounters.Find(enc => enc.MonsterSet.Contains("brigand_sapper_D")).MonsterSet),
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "altar",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 12/18
        room = new DungeonRoom("room:12/18", 12, 18)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Battle,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["town"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["town_incursion_03"][0].MonsterSet),
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "square",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 22/18
        room = new DungeonRoom("room:22/18", 22, 18)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Battle,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["town"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["town_incursion_04"][0].MonsterSet),
            MashId = 6,
            Doors = new List<Door>(),
            TextureId = "square",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 7/18
        room = new DungeonRoom("room:7/18", 7, 18)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.BattleCurio,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["town"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["town_incursion_05"][0].MonsterSet),
            MashId = 6,
            Prop = DarkestDungeonManager.Data.Curios["ancestors_knapsack"],
            Doors = new List<Door>(),
            TextureId = "square",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 27/18
        room = new DungeonRoom("room:27/18", 27, 18)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.BattleCurio,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["town"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["town_incursion_06"][0].MonsterSet),
            MashId = 6,
            Prop = DarkestDungeonManager.Data.Curios["ancestors_knapsack"],
            Doors = new List<Door>(),
            TextureId = "square",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 17/18
        room = new DungeonRoom("room:17/18", 17, 18)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.BattleCurio,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["town"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["town_incursion_06"][0].MonsterSet),
            MashId = 6,
            Prop = DarkestDungeonManager.Data.Curios["ancestors_knapsack"],
            Doors = new List<Door>(),
            TextureId = "square",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion

        #endregion

        #region Hallways
        #region 17/13 to 12/18
        Hallway hallway = new Hallway("17/13_to_12/18");
        hallway.RoomA = saveData.Dungeon.Rooms["room:17/13"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:12/18"];
        hallway.RoomA.Doors.Add(new Door("room:17/13", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:12/18", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX - 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX - 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["town"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["town_incursion_weak_01"][0].MonsterSet),
            },
            new HallSector("2", hallway.RoomA.GridX - 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Obstacle,
                Prop = DarkestDungeonManager.Data.Obstacles["town_rubble"],
            },
            new HallSector("3", hallway.RoomA.GridX - 3, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX - 4, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["town"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["town_incursion_01"][0].MonsterSet),
            },
            new HallSector("5", hallway.RoomA.GridX - 4, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Curio,
                Prop = DarkestDungeonManager.Data.Curios["travellers_tent"]
            },
            new HallSector("6", hallway.RoomA.GridX - 5, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("7", hallway.RoomA.GridX - 5, hallway.RoomA.GridY + 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("8", hallway.RoomA.GridX - 5, hallway.RoomA.GridY + 4, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 17/13 to 22/18
        hallway = new Hallway("17/13_to_22/18");
        hallway.RoomA = saveData.Dungeon.Rooms["room:17/13"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:22/18"];
        hallway.RoomA.Doors.Add(new Door("room:17/13", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:22/18", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["town"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["town_incursion_weak_02"][0].MonsterSet),
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Obstacle,
                Prop = DarkestDungeonManager.Data.Obstacles["town_rubble"],
            },
            new HallSector("3", hallway.RoomA.GridX + 3, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 4, hallway.RoomA.GridY + 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["town"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["town_incursion_02"][0].MonsterSet),
            },
            new HallSector("5", hallway.RoomA.GridX + 4, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Curio,
                Prop = DarkestDungeonManager.Data.Curios["travellers_tent"]
            },
            new HallSector("6", hallway.RoomA.GridX + 5, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("7", hallway.RoomA.GridX + 5, hallway.RoomA.GridY + 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("8", hallway.RoomA.GridX + 5, hallway.RoomA.GridY + 4, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 17/23 to 12/18
        hallway = new Hallway("17/23_to_12/18");
        hallway.RoomA = saveData.Dungeon.Rooms["room:17/23"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:12/18"];
        hallway.RoomA.Doors.Add(new Door("room:17/23", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:12/18", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX - 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX - 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX - 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX - 3, hallway.RoomA.GridY - 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["town"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["town_incursion_07"][0].MonsterSet),
            },
            new HallSector("4", hallway.RoomA.GridX - 4, hallway.RoomA.GridY - 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Obstacle,
                Prop = DarkestDungeonManager.Data.Obstacles["town_rubble"],
            },
            new HallSector("5", hallway.RoomA.GridX - 4, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Curio,
                Prop = DarkestDungeonManager.Data.Curios["crate"]
            },
            new HallSector("6", hallway.RoomA.GridX - 5, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["town"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["town_incursion_weak_07"][0].MonsterSet),
                Type = AreaType.Battle,
            },
            new HallSector("7", hallway.RoomA.GridX - 5, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("8", hallway.RoomA.GridX - 5, hallway.RoomA.GridY - 4, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 17/23 to 22/18
        hallway = new Hallway("17/23_to_22/18");
        hallway.RoomA = saveData.Dungeon.Rooms["room:17/23"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:22/18"];
        hallway.RoomA.Doors.Add(new Door("room:17/23", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:22/18", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 3, hallway.RoomA.GridY - 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["town"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["town_incursion_08"][0].MonsterSet),
            },
            new HallSector("4", hallway.RoomA.GridX + 4, hallway.RoomA.GridY - 1, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Obstacle,
                Prop = DarkestDungeonManager.Data.Obstacles["town_rubble"],
            },
            new HallSector("5", hallway.RoomA.GridX + 4, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Curio,
                Prop = DarkestDungeonManager.Data.Curios["crate"]
            },
            new HallSector("6", hallway.RoomA.GridX + 5, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["town"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["town_incursion_weak_08"][0].MonsterSet),
                Type = AreaType.Battle,
            },
            new HallSector("7", hallway.RoomA.GridX + 5, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("8", hallway.RoomA.GridX + 5, hallway.RoomA.GridY - 4, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 12/18 to 7/18
        hallway = new Hallway("12/18_to_7/18");
        hallway.RoomA = saveData.Dungeon.Rooms["room:12/18"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:7/18"];
        hallway.RoomA.Doors.Add(new Door("room:12/18", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:7/18", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX - 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX - 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX - 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["town"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["town_incursion_weak_05"][0].MonsterSet),
            },
            new HallSector("3", hallway.RoomA.GridX - 4, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 22/18 to 27/18
        hallway = new Hallway("22/18_to_27/18");
        hallway.RoomA = saveData.Dungeon.Rooms["room:22/18"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:27/18"];
        hallway.RoomA.Doors.Add(new Door("room:22/18", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:27/18", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["town"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["town_incursion_weak_06"][0].MonsterSet),
            },
            new HallSector("3", hallway.RoomA.GridX + 4, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 17/23 to 17/18
        hallway = new Hallway("17/23_to_17/18");
        hallway.RoomA = saveData.Dungeon.Rooms["room:17/23"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:17/18"];
        hallway.RoomA.Doors.Add(new Door("room:17/23", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:17/18", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY - 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY - 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Obstacle,
                Prop = DarkestDungeonManager.Data.Obstacles["town_rubble"],
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY - 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["town"].BattleMashes.
                    Find(mash => mash.MashId == 6).NamedEncounters["town_incursion_weak_06"][0].MonsterSet),
            },
            new HallSector("3", hallway.RoomA.GridX, hallway.RoomA.GridY - 4, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #endregion
        #endregion

        #region Party
        saveData.RaidParty = new RaidPartySaveData()
        {
            IsMovingLeft = false,
            HeroInfo = new List<RaidPartyHeroInfoSaveData>()
            {
                new RaidPartyHeroInfoSaveData()
                {
                Factor = DeathFactor.AttackMonster,
                Killer = "",
                IsAlive = true,
                HeroRosterId = 1,
                },
                new RaidPartyHeroInfoSaveData()
                {
                Factor = DeathFactor.AttackMonster,
                Killer = "",
                IsAlive = true,
                HeroRosterId = 2,
                },
            }
        };
        #endregion

        #region Data
        saveData.ExploredRoomCount = 1;
        saveData.CurrentLocation = "room:17/13";
        saveData.LastRoom = "room:17/13";
        saveData.PreviousLastSector = "";
        saveData.LastSector = "";
        saveData.KilledMonsters = new List<string>();
        saveData.InvestigatedCurios = new List<string>();

        saveData.TorchAmount = 100;
        saveData.MaxTorchAmount = 100;
        saveData.ModifiedMinTorch = -1;
        saveData.ModifiedMaxTorch = -1;
        #endregion

        #region Formation
        saveData.HeroFormationData = new BattleFormationSaveData();
        saveData.HeroFormationData.unitData.Add(new FormationUnitSaveData()
        {
            IsHero = true,
            RosterId = 1,
            Rank = 1,
            CombatInfo = new FormationUnitInfo()
            {
                CombatId = 1,
            }
        });
        saveData.HeroFormationData.unitData.Add(new FormationUnitSaveData()
        {
            IsHero = true,
            RosterId = 2,
            Rank = 2,
            CombatInfo = new FormationUnitInfo()
            {
                CombatId = 2,
            }
        });
        #endregion

        #region Inventory
        saveData.InventoryItems = new List<InventorySlotData>();
        #endregion

        WriteSave(saveData);
        return saveData;
    }

    public static SaveCampaignData WriteTutorialCryptsSave(SaveCampaignData saveData)
    {
        saveData.IsFirstStart = true;
        saveData.LocationName = "Raid";
        saveData.QuestsCompleted = 0;
        saveData.CurrentWeek = 1;

        saveData.GoldAmount = 1000;
        saveData.BustsAmount = 10;
        saveData.DeedsAmount = 10;
        saveData.PortraitsAmount = 10;
        saveData.CrestsAmount = 10;

        #region Initial Heroes
        saveData.RosterHeroes = new List<SaveHeroData>
        {
            #region Hero 1
            new SaveHeroData()
            {
                RosterId = 1,
                Name = "Reynald",
                HeroClass = "crusader",
                ResolveLevel = 0,
                ResolveXP = 0,
                StressLevel = 10,
                WeaponLevel = 1,
                ArmorLevel = 1,
                LeftTrinketId = "",
                RightTrinketId = "dd_trinket",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("warrior_of_light"),
                    new QuirkInfo("kleptomaniac"),
                    new QuirkInfo("god_fearing"),
                },
            },
            #endregion
            #region Hero 2
            new SaveHeroData()
            {
                RosterId = 2,
                Name = "Dismas",
                HeroClass = "highwayman",
                ResolveLevel = 0,
                ResolveXP = 0,
                StressLevel = 10,
                WeaponLevel = 1,
                ArmorLevel = 1,
                LeftTrinketId = "",
                RightTrinketId = "dd_trinket",
                Quirks = new List<QuirkInfo>()
                {
                    new QuirkInfo("hard_noggin"),
                    new QuirkInfo("known_cheat"),
                    new QuirkInfo("quick_reflexes"),
                },
            },
            #endregion
        };
        #endregion

        #region StageCoach Heroes
        saveData.StageCoachHeroes = new List<SaveHeroData>();
        #endregion

        #region Initial Hero Purchases
        saveData.InstancedPurchases.Clear();
        for (int i = 0; i < saveData.RosterHeroes.Count; i++)
        {
            var newHeroPurchases = new Dictionary<string, UpgradePurchases>();
            saveData.InstancedPurchases.Add(saveData.RosterHeroes[i].RosterId, newHeroPurchases);
            newHeroPurchases.Add(saveData.RosterHeroes[i].HeroClass + ".weapon",
                new UpgradePurchases(saveData.RosterHeroes[i].HeroClass + ".weapon", new string[0]));
            newHeroPurchases.Add(saveData.RosterHeroes[i].HeroClass + ".armour",
                new UpgradePurchases(saveData.RosterHeroes[i].HeroClass + ".armour", new string[0]));
            var heroClass = DarkestDungeonManager.Data.HeroClasses[saveData.RosterHeroes[i].HeroClass];
            for (int j = 0; j < heroClass.CombatSkills.Count; j++)
                newHeroPurchases.Add(saveData.RosterHeroes[i].HeroClass + "." + heroClass.CombatSkills[j].Id, new UpgradePurchases(
                    saveData.RosterHeroes[i].HeroClass + "." + heroClass.CombatSkills[j].Id, new string[0]));
            for (int j = 0; j < heroClass.CampingSkills.Count; j++)
                newHeroPurchases.Add(heroClass.CampingSkills[j].Id, new UpgradePurchases(
                    heroClass.CampingSkills[j].Id, new string[0]));
        }
        saveData.InstancedPurchases[1]["crusader.smite"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["crusader.zealous_accusation"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["crusader.stunning_blow"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["crusader.bulwark_of_faith"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(0);
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(1);
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(2);
        saveData.RosterHeroes[0].SelectedCombatSkillIndexes.Add(3);
        saveData.InstancedPurchases[1]["encourage"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["stand_tall"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[1]["zealous_speech"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[0].SelectedCampingSkillIndexes.Add(0);
        saveData.RosterHeroes[0].SelectedCampingSkillIndexes.Add(4);
        saveData.RosterHeroes[0].SelectedCampingSkillIndexes.Add(5);


        saveData.InstancedPurchases[2]["highwayman.opened_vein"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["highwayman.pistol_shot"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["highwayman.grape_shot_blast"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["highwayman.take_aim"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(6);
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(1);
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(3);
        saveData.RosterHeroes[1].SelectedCombatSkillIndexes.Add(4);
        saveData.InstancedPurchases[2]["first_aid"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["clean_guns"].PurchasedUpgrades.Add("0");
        saveData.InstancedPurchases[2]["bandits_sense"].PurchasedUpgrades.Add("0");
        saveData.RosterHeroes[1].SelectedCampingSkillIndexes.Add(1);
        saveData.RosterHeroes[1].SelectedCampingSkillIndexes.Add(5);
        saveData.RosterHeroes[1].SelectedCampingSkillIndexes.Add(6);
        #endregion

        #region Activity Log
        saveData.ActivityLog = new List<WeekActivityLog>();
        saveData.CompletedPlot = new List<string>();
        saveData.GeneratedQuests = new List<Quest>();
        #endregion

        #region Estate Misc
        saveData.InventoryTrinkets.Clear();

        saveData.WagonTrinkets.Clear();

        saveData.DungeonProgress.Clear();
        saveData.DungeonProgress.Add("crypts", new DungeonProgress("crypts", 0, 0, true, false));
        saveData.DungeonProgress.Add("warrens", new DungeonProgress("warrens", 0, 0, true, false));
        saveData.DungeonProgress.Add("weald", new DungeonProgress("weald", 0, 0, true, false));
        saveData.DungeonProgress.Add("cove", new DungeonProgress("cove", 0, 0, true, false));
        saveData.DungeonProgress.Add("darkestdungeon", new DungeonProgress("darkestdungeon", 1, 0, true, false));
        saveData.DungeonProgress.Add("town", new DungeonProgress("town", 1, 0, true, true));

        saveData.DeathRecords = new List<DeathRecord>();

        saveData.BuildingUpgrades.Clear();

        saveData.BuildingUpgrades.Add("abbey.meditation", new UpgradePurchases("abbey.meditation", new string[0]));
        saveData.BuildingUpgrades.Add("abbey.prayer", new UpgradePurchases("abbey.prayer", new string[0]));
        saveData.BuildingUpgrades.Add("abbey.flagellation", new UpgradePurchases("abbey.flagellation", new string[0]));
        saveData.BuildingUpgrades.Add("tavern.bar", new UpgradePurchases("tavern.bar", new string[0]));
        saveData.BuildingUpgrades.Add("tavern.gambling", new UpgradePurchases("tavern.gambling", new string[0]));
        saveData.BuildingUpgrades.Add("tavern.brothel", new UpgradePurchases("tavern.brothel", new string[0]));
        saveData.BuildingUpgrades.Add("sanitarium.cost", new UpgradePurchases("sanitarium.cost", new string[0]));
        saveData.BuildingUpgrades.Add("sanitarium.disease_quirk_cost", new UpgradePurchases("sanitarium.disease_quirk_cost", new string[0]));
        saveData.BuildingUpgrades.Add("sanitarium.slots", new UpgradePurchases("sanitarium.slots", new string[0]));
        saveData.BuildingUpgrades.Add("blacksmith.weapon", new UpgradePurchases("blacksmith.weapon", new string[0]));
        saveData.BuildingUpgrades.Add("blacksmith.armour", new UpgradePurchases("blacksmith.armour", new string[0]));
        saveData.BuildingUpgrades.Add("blacksmith.cost", new UpgradePurchases("blacksmith.cost", new string[0]));
        saveData.BuildingUpgrades.Add("guild.skill_levels", new UpgradePurchases("guild.skill_levels", new string[0]));
        saveData.BuildingUpgrades.Add("guild.cost", new UpgradePurchases("guild.cost", new string[0]));
        saveData.BuildingUpgrades.Add("camping_trainer.cost", new UpgradePurchases("camping_trainer.cost", new string[0]));
        saveData.BuildingUpgrades.Add("nomad_wagon.numitems", new UpgradePurchases("nomad_wagon.numitems", new string[0]));
        saveData.BuildingUpgrades.Add("nomad_wagon.cost", new UpgradePurchases("nomad_wagon.cost", new string[0]));
        saveData.BuildingUpgrades.Add("stage_coach.numrecruits", new UpgradePurchases("stage_coach.numrecruits", new string[0]));
        saveData.BuildingUpgrades.Add("stage_coach.rostersize", new UpgradePurchases("stage_coach.rostersize", new string[0]));
        saveData.BuildingUpgrades.Add("stage_coach.upgraded_recruits", new UpgradePurchases("stage_coach.upgraded_recruits", new string[0]));
        #endregion

        #region ActivitySlots
        saveData.AbbeyActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        saveData.TavernActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        saveData.SanitariumActivitySlots = new List<List<SaveActivitySlot>>()
        {
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
            new List<SaveActivitySlot>()
            {
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
                 new SaveActivitySlot(),
            },
        };
        #endregion

        saveData.InRaid = true;

        #region Quest
        saveData.QuestCompleted = false;
        saveData.Quest = DarkestDungeonManager.Data.QuestDatabase.PlotQuests.Find(quest =>
            quest.Id == "plot_tutorial_crypts").Copy();
        #endregion

        #region Dungeon
        saveData.Dungeon = new Dungeon();
        saveData.Dungeon.Name = saveData.Quest.Dungeon;
        saveData.Dungeon.GridSizeX = 30;
        saveData.Dungeon.GridSizeY = 25;
        saveData.Dungeon.StartingRoomId = "room:14/4";

        #region Rooms
        #region Room 14/4 Entrance
        DungeonRoom room = new DungeonRoom("room:14/4", 14, 4)
        {
            Knowledge = Knowledge.Scouted,
            Type = AreaType.Entrance,
            MashId = 1,
            Doors = new List<Door>(),
            TextureId = "entrance",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 14/11
        room = new DungeonRoom("room:14/11", 14, 11)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.BattleTresure,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["crypts"].BattleMashes.
                    Find(mash => mash.MashId == 1).NamedEncounters["tutorial_mash_01"][0].MonsterSet),
            MashId = 1,
            Prop = DarkestDungeonManager.Data.Curios["tutorial_shovel"],
            Doors = new List<Door>(),
            TextureId = "altar",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 14/18
        room = new DungeonRoom("room:14/18", 14, 18)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 1,
            Doors = new List<Door>(),
            TextureId = "empty",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 14/25
        room = new DungeonRoom("room:14/25", 14, 25)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.BattleTresure,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["crypts"].BattleMashes.
                    Find(mash => mash.MashId == 1).NamedEncounters["tutorial_mash_03"][0].MonsterSet),
            MashId = 1,
            Prop = DarkestDungeonManager.Data.Curios["heirloom_chest"],
            Doors = new List<Door>(),
            TextureId = "drain",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 21/25
        room = new DungeonRoom("room:21/25", 21, 25)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 1,
            Doors = new List<Door>(),
            TextureId = "barrels",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 28/25
        room = new DungeonRoom("room:28/25", 28, 25)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.Empty,
            MashId = 1,
            Doors = new List<Door>(),
            TextureId = "library",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 28/18
        room = new DungeonRoom("room:28/18", 28, 18)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.BattleTresure,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["crypts"].BattleMashes.
                    Find(mash => mash.MashId == 1).NamedEncounters["tutorial_mash_05"][0].MonsterSet),
            Prop = DarkestDungeonManager.Data.Curios["heirloom_chest"],
            MashId = 1,
            Doors = new List<Door>(),
            TextureId = "torture",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #region Room 21/18
        room = new DungeonRoom("room:21/18", 21, 18)
        {
            Knowledge = Knowledge.Hidden,
            Type = AreaType.BattleCurio,
            BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["crypts"].BattleMashes.
                    Find(mash => mash.MashId == 1).NamedEncounters["tutorial_mash_04"][0].MonsterSet),
            Prop = DarkestDungeonManager.Data.Curios["altar_of_light"],
            MashId = 1,
            Doors = new List<Door>(),
            TextureId = "empty",
        };
        saveData.Dungeon.Rooms.Add(room.Id, room);
        #endregion
        #endregion

        #region Hallways
        #region 14/4 to 14/11
        Hallway hallway = new Hallway("14/4_to_14/11");
        hallway.RoomA = saveData.Dungeon.Rooms["room:14/4"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:14/11"];
        hallway.RoomA.Doors.Add(new Door("room:14/4", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:14/11", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY + 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 8).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY + 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 8).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX, hallway.RoomA.GridY + 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Curio,
                Prop = DarkestDungeonManager.Data.Curios["sconce"],
            },
            new HallSector("4", hallway.RoomA.GridX, hallway.RoomA.GridY + 5, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 8).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX, hallway.RoomA.GridY + 6, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 14/4 to 14/18
        hallway = new Hallway("14/11_to_14/18");
        hallway.RoomA = saveData.Dungeon.Rooms["room:14/11"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:14/18"];
        hallway.RoomA.Doors.Add(new Door("room:14/11", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:14/18", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY + 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 8).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY + 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 8).ToString(),
                Type = AreaType.Obstacle,
                Prop = DarkestDungeonManager.Data.Obstacles["rubble"],
            },
            new HallSector("3", hallway.RoomA.GridX, hallway.RoomA.GridY + 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                
            },
            new HallSector("4", hallway.RoomA.GridX, hallway.RoomA.GridY + 5, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 8).ToString(),
                Type = AreaType.Battle,
                BattleEncounter = new BattleEncounter(DarkestDungeonManager.Data.DungeonEnviromentData["crypts"].BattleMashes.
                    Find(mash => mash.MashId == 1).NamedEncounters["tutorial_mash_02"][0].MonsterSet),
            },
            new HallSector("5", hallway.RoomA.GridX, hallway.RoomA.GridY + 6, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 14/18 to 14/25
        hallway = new Hallway("14/18_to_14/25");
        hallway.RoomA = saveData.Dungeon.Rooms["room:14/18"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:14/25"];
        hallway.RoomA.Doors.Add(new Door("room:14/18", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:14/25", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY + 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 8).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY + 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 8).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX, hallway.RoomA.GridY + 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Curio,
                Prop = DarkestDungeonManager.Data.Curios["tutorial_key"],
            },
            new HallSector("4", hallway.RoomA.GridX, hallway.RoomA.GridY + 5, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 8).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX, hallway.RoomA.GridY + 6, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 14/25 to 21/25
        hallway = new Hallway("14/25_to_21/25");
        hallway.RoomA = saveData.Dungeon.Rooms["room:14/25"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:21/25"];
        hallway.RoomA.Doors.Add(new Door("room:14/25", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:21/25", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 8).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 8).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Trap,
                Prop = DarkestDungeonManager.Data.Traps["spikes"],
            },
            new HallSector("4", hallway.RoomA.GridX + 5, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 8).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX + 6, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 21/25 to 28/25
        hallway = new Hallway("21/25_to_28/25");
        hallway.RoomA = saveData.Dungeon.Rooms["room:21/25"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:28/25"];
        hallway.RoomA.Doors.Add(new Door("room:21/25", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:28/25", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 8).ToString(),
                Type = AreaType.Curio,
                Prop = DarkestDungeonManager.Data.Curios["tutorial_key"],
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 8).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX + 5, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 8).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX + 6, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 14/18 to 21/18
        hallway = new Hallway("14/18_to_21/18");
        hallway.RoomA = saveData.Dungeon.Rooms["room:14/18"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:21/18"];
        hallway.RoomA.Doors.Add(new Door("room:14/18", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:21/18", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX + 1, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX + 2, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 8).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX + 3, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 8).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX + 4, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Curio,
                Prop = DarkestDungeonManager.Data.Curios["tutorial_holy"],
            },
            new HallSector("4", hallway.RoomA.GridX + 5, hallway.RoomA.GridY, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 8).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX + 6, hallway.RoomA.GridY, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 21/18 to 21/25
        hallway = new Hallway("21/18_to_21/25");
        hallway.RoomA = saveData.Dungeon.Rooms["room:21/18"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:21/25"];
        hallway.RoomA.Doors.Add(new Door("room:21/18", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:21/25", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY + 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 8).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY + 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 8).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX, hallway.RoomA.GridY + 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Trap,
                Prop = DarkestDungeonManager.Data.Traps["spikes"],
            },
            new HallSector("4", hallway.RoomA.GridX, hallway.RoomA.GridY + 5, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 8).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX, hallway.RoomA.GridY + 6, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #region 28/18 to 28/25
        hallway = new Hallway("28/18_to_28/25");
        hallway.RoomA = saveData.Dungeon.Rooms["room:28/18"];
        hallway.RoomB = saveData.Dungeon.Rooms["room:28/25"];
        hallway.RoomA.Doors.Add(new Door("room:28/18", hallway.Id, Direction.Left));
        hallway.RoomB.Doors.Add(new Door("room:28/25", hallway.Id, Direction.Right));
        hallway.Halls = new List<HallSector>()
        {
            new HallSector("0", hallway.RoomA.GridX, hallway.RoomA.GridY + 1, hallway, new Door(hallway.Id, hallway.RoomA.Id, Direction.Left)),
            new HallSector("1", hallway.RoomA.GridX, hallway.RoomA.GridY + 2, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 8).ToString(),
                Type = AreaType.Obstacle,
                Prop = DarkestDungeonManager.Data.Obstacles["rubble"],
            },
            new HallSector("2", hallway.RoomA.GridX, hallway.RoomA.GridY + 3, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 8).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("3", hallway.RoomA.GridX, hallway.RoomA.GridY + 4, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 6).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("4", hallway.RoomA.GridX, hallway.RoomA.GridY + 5, hallway)
            {
                Knowledge = Knowledge.Hidden,
                TextureId = Random.Range(1, 8).ToString(),
                Type = AreaType.Empty,
            },
            new HallSector("5", hallway.RoomA.GridX, hallway.RoomA.GridY + 6, hallway, new Door(hallway.Id, hallway.RoomB.Id, Direction.Right)),
        };
        saveData.Dungeon.Hallways.Add(hallway.Id, hallway);
        #endregion
        #endregion
        #endregion

        #region Party
        saveData.RaidParty = new RaidPartySaveData()
        {
            IsMovingLeft = false,
            HeroInfo = new List<RaidPartyHeroInfoSaveData>()
            {
                new RaidPartyHeroInfoSaveData()
                {
                Factor = DeathFactor.AttackMonster,
                Killer = "",
                IsAlive = true,
                HeroRosterId = 1,
                },
                new RaidPartyHeroInfoSaveData()
                {
                Factor = DeathFactor.AttackMonster,
                Killer = "",
                IsAlive = true,
                HeroRosterId = 2,
                },
            }
        };
        #endregion

        #region Data
        saveData.ExploredRoomCount = 1;
        saveData.CurrentLocation = "room:14/4";
        saveData.LastRoom = "room:14/4";
        saveData.PreviousLastSector = "";
        saveData.LastSector = "";
        saveData.KilledMonsters = new List<string>();
        saveData.InvestigatedCurios = new List<string>();

        saveData.TorchAmount = 100;
        saveData.MaxTorchAmount = 100;
        saveData.ModifiedMinTorch = -1;
        saveData.ModifiedMaxTorch = -1;
        #endregion

        #region Formation
        saveData.HeroFormationData = new BattleFormationSaveData();
        saveData.HeroFormationData.unitData.Add(new FormationUnitSaveData()
        {
            IsHero = true,
            RosterId = 1,
            Rank = 1,
            CombatInfo = new FormationUnitInfo()
            {
                CombatId = 1,
            }
        });
        saveData.HeroFormationData.unitData.Add(new FormationUnitSaveData()
        {
            IsHero = true,
            RosterId = 2,
            Rank = 2,
            CombatInfo = new FormationUnitInfo()
            {
                CombatId = 2,
            }
        });
        #endregion

        #region Inventory
        saveData.InventoryItems = new List<InventorySlotData>();
        #endregion

        WriteSave(saveData);
        return saveData;
    }


    private static void RecreateMapDirectory()
    {
        if (!Directory.Exists(Application.persistentDataPath + Path.AltDirectorySeparatorChar + "Maps"))
            Directory.CreateDirectory(Application.persistentDataPath + Path.AltDirectorySeparatorChar + "Maps");
    }

    private static void RecreateSaveDirectory()
    {
        if (!Directory.Exists(Application.persistentDataPath + Path.AltDirectorySeparatorChar + "Saves"))
            Directory.CreateDirectory(Application.persistentDataPath + Path.AltDirectorySeparatorChar + "Saves");
    }

    private static string GenerateSaveFileName(int slotId)
    {
        return Application.persistentDataPath + Path.AltDirectorySeparatorChar + "Saves" +
               Path.AltDirectorySeparatorChar + "DarkestSave" + slotId + ".darkestsave";
    }

    private static string GenerateMapFileName(string mapName)
    {
        return Application.persistentDataPath + "\\Maps\\" + mapName + ".darkestmap";
    }
}