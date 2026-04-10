using System;
using SeasonalBastion.Contracts;
using UnityEngine;

namespace SeasonalBastion
{
    public sealed class WorldOps : IWorldOps
    {
        private readonly IWorldState _world;
        private readonly IEventBus _bus;
        private readonly IDataRegistry _data;
        private readonly IWorldIndex _index;
        private readonly IJobBoard _jobs;

        public WorldOps(IWorldState world, IEventBus bus, IDataRegistry data = null, IWorldIndex index = null, IJobBoard jobs = null)
        {
            _world = world;
            _bus = bus;
            _data = data;
            _index = index;
            _jobs = jobs;
        }

        public BuildingId CreateBuilding(string buildingDefId, CellPos anchor, Dir4 rotation)
        {
            int level = 1;
            int maxHp = 1;
            int ammo = 0;
            if (_data != null && _data.TryGetBuilding(buildingDefId, out var def) && def != null)
            {
                level = Math.Max(1, def.BaseLevel);
                maxHp = Math.Max(1, def.MaxHp);
                if (def.IsTower && _data.TryGetTower(buildingDefId, out var towerDef) && towerDef != null)
                    ammo = Math.Max(0, towerDef.AmmoMax);
            }

            BuildingState st = new()
            {
                DefId = buildingDefId,
                Anchor = anchor,
                Rotation = rotation,
                Level = level,
                IsConstructed = true,
                HP = maxHp,
                MaxHP = maxHp,
                Ammo = ammo
            };

            BuildingId id = _world.Buildings.Create(st);
            st.Id = id;
            _world.Buildings.Set(id, st);
            TryCreateTowerState(st);
            NotifyBuildingCreated(buildingDefId, id);
            return id;
        }

        public void DestroyBuilding(BuildingId id)
        {
            if (!_world.Buildings.Exists(id)) return;
            BuildingState st = _world.Buildings.Get(id);
            string defId = st.DefId;
            ClearNpcWorkplaceReferences(id);
            CancelQueuedJobsForWorkplace(id);
            DestroyTowerStateForBuilding(st);
            _world.Buildings.Destroy(id);
            try { _index?.OnBuildingDestroyed(id); } catch (Exception ex) { Debug.LogError($"[WorldOps] Failed to update WorldIndex after destroying building {id.Value}: {ex}"); }
            _bus?.Publish(new BuildingDestroyedEvent(defId, id));
            _bus?.Publish(new WorldStateChangedEvent("Building", id.Value));
            _bus?.Publish(new RoadsDirtyEvent());
        }

        public NpcId CreateNpc(string npcDefId, CellPos spawn)
        {
            NpcState st = new() { DefId = npcDefId, Cell = spawn, Workplace = default, CurrentJob = default, IsIdle = true };
            NpcId id = _world.Npcs.Create(st);
            st.Id = id;
            _world.Npcs.Set(id, st);
            _bus?.Publish(new WorldStateChangedEvent("Npc", id.Value));
            return id;
        }

        public void DestroyNpc(NpcId id)
        {
            if (!_world.Npcs.Exists(id)) return;
            _world.Npcs.Destroy(id);
            _bus?.Publish(new WorldStateChangedEvent("Npc", id.Value));
        }

        public EnemyId CreateEnemy(string enemyDefId, CellPos spawn, int lane)
        {
            int hp = 1;
            if (_data != null && _data.TryGetEnemy(enemyDefId, out var def) && def != null)
                hp = Math.Max(1, def.MaxHp);

            EnemyState st = new() { DefId = enemyDefId, Cell = spawn, Lane = lane, Hp = hp };
            EnemyId id = _world.Enemies.Create(st);
            st.Id = id;
            _world.Enemies.Set(id, st);
            _bus?.Publish(new WorldStateChangedEvent("Enemy", id.Value));
            return id;
        }

        public void DestroyEnemy(EnemyId id)
        {
            if (!_world.Enemies.Exists(id)) return;
            _world.Enemies.Destroy(id);
            _bus?.Publish(new WorldStateChangedEvent("Enemy", id.Value));
        }

        public SiteId CreateBuildSite(string buildingDefId, CellPos anchor, Dir4 rotation)
        {
            BuildSiteState st = new() { BuildingDefId = buildingDefId, Anchor = anchor, Rotation = rotation };
            SiteId id = _world.Sites.Create(st);
            st.Id = id;
            _world.Sites.Set(id, st);
            _bus?.Publish(new WorldStateChangedEvent("BuildSite", id.Value));
            return id;
        }

        public void DestroyBuildSite(SiteId id)
        {
            if (!_world.Sites.Exists(id)) return;
            _world.Sites.Destroy(id);
            _bus?.Publish(new WorldStateChangedEvent("BuildSite", id.Value));
        }

        private void NotifyBuildingCreated(string buildingDefId, BuildingId id)
        {
            try { _index?.OnBuildingCreated(id); } catch (Exception ex) { Debug.LogError($"[WorldOps] Failed to update WorldIndex after creating building {id.Value} ({buildingDefId}): {ex}"); }
            _bus?.Publish(new BuildingPlacedEvent(buildingDefId, id));
            _bus?.Publish(new WorldStateChangedEvent("Building", id.Value));
            _bus?.Publish(new RoadsDirtyEvent());
        }

        private void TryCreateTowerState(in BuildingState building)
        {
            if (_data == null) return;
            if (!_data.TryGetBuilding(building.DefId, out var def) || def == null || !def.IsTower) return;

            int hpMax = Math.Max(1, def.MaxHp);
            int ammoMax = 0;
            if (_data.TryGetTower(building.DefId, out var towerDef) && towerDef != null)
            {
                hpMax = Math.Max(1, towerDef.MaxHp);
                ammoMax = Math.Max(0, towerDef.AmmoMax);
            }

            int w = Math.Max(1, def.SizeX);
            int h = Math.Max(1, def.SizeY);
            CellPos towerCell = new(building.Anchor.X + (w / 2), building.Anchor.Y + (h / 2));
            foreach (TowerId tid in _world.Towers.Ids)
            {
                if (!_world.Towers.Exists(tid)) continue;
                TowerState existing = _world.Towers.Get(tid);
                if (existing.Cell.X == towerCell.X && existing.Cell.Y == towerCell.Y)
                    return;
            }

            TowerState tower = new() { Cell = towerCell, Hp = hpMax, HpMax = hpMax, Ammo = ammoMax, AmmoCap = ammoMax };
            TowerId towerId = _world.Towers.Create(tower);
            tower.Id = towerId;
            _world.Towers.Set(towerId, tower);
            _bus?.Publish(new WorldStateChangedEvent("Tower", towerId.Value));
        }

        private void DestroyTowerStateForBuilding(in BuildingState building)
        {
            if (_data == null) return;
            if (!_data.TryGetBuilding(building.DefId, out var def) || def == null || !def.IsTower) return;

            int w = Math.Max(1, def.SizeX);
            int h = Math.Max(1, def.SizeY);
            CellPos towerCell = new(building.Anchor.X + (w / 2), building.Anchor.Y + (h / 2));
            foreach (TowerId tid in _world.Towers.Ids)
            {
                if (!_world.Towers.Exists(tid)) continue;
                TowerState existing = _world.Towers.Get(tid);
                if (existing.Cell.X == towerCell.X && existing.Cell.Y == towerCell.Y)
                {
                    _world.Towers.Destroy(tid);
                    _bus?.Publish(new WorldStateChangedEvent("Tower", tid.Value));
                    return;
                }
            }
        }

        private void ClearNpcWorkplaceReferences(BuildingId buildingId)
        {
            foreach (NpcId npcId in _world.Npcs.Ids)
            {
                if (!_world.Npcs.Exists(npcId)) continue;
                NpcState npc = _world.Npcs.Get(npcId);
                if (npc.Workplace.Value != buildingId.Value) continue;
                if (npc.CurrentJob.Value != 0) _jobs?.Cancel(npc.CurrentJob);
                npc.Workplace = default;
                npc.CurrentJob = default;
                npc.IsIdle = true;
                _world.Npcs.Set(npcId, npc);
            }
        }

        private void CancelQueuedJobsForWorkplace(BuildingId workplace)
        {
            if (_jobs == null) return;
            while (_jobs.TryPeekForWorkplace(workplace, out var job))
                _jobs.Cancel(job.Id);
        }
    }
}
