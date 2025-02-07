using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Runtime.Projectiles;
using PlasticPipe.PlasticProtocol.Messages;
using UnityEngine;
using Utilities;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        public override string TargetUnitName => "Cobra Commando";
        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;
        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;
        private List<Vector2Int> _targetsNotInRange = new List<Vector2Int>(); 
        
        
        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            float overheatTemperature = OverheatTemperature;
            ///////////////////////////////////////
            // Homework 1.3 (1st block, 3rd module)
            ///////////////////////////////////////           

            if (GetTemperature() >= overheatTemperature)
            {
                return;
            }

            for (int i = 0; i <= GetTemperature(); i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
            IncreaseTemperature();
            ///////////////////////////////////////
        }

        public override Vector2Int GetNextStep()
        {
            Vector2Int position = unit.Pos;

            if (_targetsNotInRange.Any())
            {
                Vector2Int nextPosition = _targetsNotInRange[0];
                position = position.CalcNextStepTowards(nextPosition);
            }
            return position;
        }

        private List<Vector2Int> SelectNearestTargets(List<Vector2Int> targets)
        {
            if (!targets.Any())
            {
                return targets;
            }

            Vector2Int nearestTarget = targets[0];
            float minDistance = DistanceToOwnBase(nearestTarget);

            foreach (var target in targets)
            {
                float distance = DistanceToOwnBase(target);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestTarget = target;
                }
            }

            targets.Clear();
            targets.Add(nearestTarget);

            return targets;
        }

        protected override List<Vector2Int> SelectTargets()
        {
            ///////////////////////////////////////
            // Homework 2.2 (2 block, 2 module)
            ///////////////////////////////////////
            List<Vector2Int> allTargets = new List<Vector2Int>();

            foreach (var target in GetAllTargets())
            {
                allTargets.Add(target);
            }

            List<Vector2Int> result = new List<Vector2Int>();
            
            _targetsNotInRange.Clear();
            if (allTargets.Any())
            {
                Vector2Int nearestTarget = SelectNearestTargets(allTargets)[0];
                
                if (IsTargetInRange(nearestTarget))
                    result.Add(nearestTarget);
                else
                    _targetsNotInRange.Add(nearestTarget);
            }
            else
            {
                int enemyId = IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerId;
                Vector2Int enemyBasePos = runtimeModel.RoMap.Bases[enemyId];
                result.Add(enemyBasePos);
            }

            return result;
            ///////////////////////////////////////
        }

        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {              
                _cooldownTime += Time.deltaTime;
                float t = _cooldownTime / (OverheatCooldown/10);
                _temperature = Mathf.Lerp(OverheatTemperature, 0, t);
                if (t >= 1)
                {
                    _cooldownTime = 0;
                    _overheated = false;
                }
            }
        }

        private int GetTemperature()
        {
            if(_overheated) return (int) OverheatTemperature;
            else return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;
            if (_temperature >= OverheatTemperature) _overheated = true;
        }
    }
}