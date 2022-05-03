using System;
using System.Collections.Generic;
using System.Linq;
using Civ2engine.Enums;
using Civ2engine.Improvements;
using Civ2engine.MapObjects;
using Civ2engine.Units;

namespace Civ2engine
{
    public static class CityExtensions
    {
        public static void CalculateOutput(this City city, GovernmentType government, Game game)
        {
            var lowOrganisation = government <= GovernmentType.Despotism;
            var orgLevel = city.OrganizationLevel;
            // var hasSuperMarket = city.ImprovementExists(ImprovementType.Supermarket);
            // var hasSuperhighways = city.ImprovementExists(ImprovementType.Superhighways);
            
            var totalFood = 0;
            var totalSheilds = 0;
            var totalTrade = 0;
            
            city.WorkedTiles.ForEach(t =>
            {
                totalFood += t.GetFood(lowOrganisation);
                totalSheilds += t.GetShields(lowOrganisation);
                totalTrade += t.GetTrade(orgLevel);
            });
            

            city.Support = city.SupportedUnits.Count(u => u.NeedsSupport);
            
            
            var distance = ComputeDistanceFactor(city, government, game);
            if (distance == 0)
            {
                city.Waste = 0;
                city.Corruption = 0;
            }
            else
            {
                distance *= game.CurrentMap.ScaleFactor;
                var gov = (int)( city.WeLoveKingDay ? government + 1 : government); 

                var corruptionTenTenFactor = 15d / (4 + gov);
                var wasteTenTenFactor = 15d / (4 + gov * 4);

                // https://apolyton.net/forum/miscellaneous/archives/civ2-strategy-archive/62524-corruption-and-waste
                var corruption = totalTrade * Math.Min(32, distance) * corruptionTenTenFactor / 100;
                
                var waste = (totalSheilds - city.Support) * Math.Min(16, distance) * wasteTenTenFactor / 100;

                if (city.ImprovementExists(ImprovementType.Courthouse))
                {
                    waste *= 0.5d;
                    corruption *= 0.5d;
                }
                
                //TODO: Trade route to capital
                
                
                city.Waste = (int)Math.Floor(waste);
                city.Corruption = (int)Math.Floor(corruption);
            }

            city.TotalProduction = totalSheilds;
            city.Trade = totalTrade - city.Corruption;
            city.Production = totalSheilds - city.Support - city.Waste;
            city.FoodConsumption = city.Size * game.Rules.Cosmic.FoodEatenPerTurn +
                                   city.SupportedUnits.Count(u => u.AIrole == AIroleType.Settle) *
                                   (government <= GovernmentType.Monarchy
                                       ? game.Rules.Cosmic.SettlersEatTillMonarchy
                                       : game.Rules.Cosmic.SettlersEatFromCommunism);
            city.FoodProduction = totalFood;
            city.SurplusHunger = totalFood - city.FoodConsumption;
        }

        public static void SetUnitSupport(this City city, CosmicRules constants)
        {
            var isFun = city.Owner.Government == GovernmentType.Fundamentalism;
            var freeSupport = city.FreeSupport(city.Owner.Government, constants);
            city.SupportedUnits.ForEach(unit =>
            {
                unit.NeedsSupport = !unit.FreeSupport(isFun) && freeSupport > 0;
                freeSupport--;
            });
        }

        private static int FreeSupport(this City city, GovernmentType government, CosmicRules constants)
        {
            return government switch
            {
                GovernmentType.Anarchy => city.Size // Only units above city size cost 1 shield
                ,
                GovernmentType.Despotism => city.Size // Only units above city size cost 1 shield
                ,
                GovernmentType.Communism => constants.CommunismPaysSupport // First 3 units have no shield cost
                ,
                GovernmentType.Monarchy => constants.MonarchyPaysSupport // First 3 units have no shield cost
                ,
                GovernmentType.Fundamentalism => constants.FundamentalismPaysSupport // First 10 units have no shield cost
                ,
                GovernmentType.Republic => 0 // Each unit costs 1 shield per turn
                ,
                GovernmentType.Democracy => 0 // Each unit costs 1 shield per turn
                ,
                _ => 0
            };
        }

        private static double ComputeDistanceFactor(City city, GovernmentType government, Game game)
        {
            double distance;
            if (government == GovernmentType.Communism)
            {
                distance = game.Rules.Cosmic.CommunismEquivalentPalaceDistance;
            }
            else
            {
                if (government is GovernmentType.Democracy or GovernmentType.Fundamentalism 
                    || city.ImprovementExists(ImprovementEffect.Capital))
                {
                    return 0;
                }

                distance =
                    city.Owner.Cities.Where(c => c.ImprovementExists(ImprovementEffect.Capital))
                        .Select(c => Utilities.DistanceTo(c, city.Location, game.Options.FlatEarth)).OrderBy(v => v).FirstOrDefault();
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (distance == default)
                {
                    distance = game.MaxDistance;
                }
                if (government < GovernmentType.Monarchy)
                {
                    distance += (int)game.DifficultyLevel;
                }
            }
            return distance;
        }

        public static bool SellImprovement(this City city, Improvement improvement)
        {
            if (city.ImprovementSold)
            {
                return false;
            }
            //TODO: Remove effects

            city._improvements.Remove(improvement.Type);
            city.ImprovementSold = true;
            return true;
        }
        
        public static void AddImprovement(this City city,Improvement improvement) => city._improvements.Add(improvement.Type,improvement);
        public static bool ImprovementExists(this City city, ImprovementType improvement) => city._improvements.ContainsKey(improvement);
        
        private static bool ImprovementExists(this City city, ImprovementEffect improvement) => city._improvements.Values.Any(i => i.Effects.ContainsKey(improvement));

        public static void ShrinkCity(this City city, Game game)
        {
            city.Size -= 1;
            city.AutoRemoveWorkersDistribution();
            city.CalculateOutput(city.Owner.Government, game);
            
            game.TriggerMapEvent(MapEventType.UpdateMap, new List<Tile> {city.Location});
        }
        
        public static void GrowCity(this City city, Game game)
        {
            city.Size += 1;

            city.AutoAddDistributionWorkers(); // Automatically add a workers on a tile
            city.CalculateOutput(city.Owner.Government, game);
            
            game.TriggerMapEvent(MapEventType.UpdateMap, new List<Tile> {city.Location});
        }

        public static void ResetFoodStorage(this City city, int foodRows)
        {
            city.FoodInStorage = 0;

            var storageBuildings = city.Improvements
                .Where(i => i.Effects.ContainsKey(ImprovementEffect.FoodStorage))
                .Select(b => b.Effects[ImprovementEffect.FoodStorage]).ToList();

            if (storageBuildings.Count <= 0) return;
            
            var totalStorage = storageBuildings.Sum();
            if (totalStorage is > 100 or < 0)
            {
                totalStorage = storageBuildings.Where(v => v is >= 0 and <= 100).Max();
            }

            if (totalStorage == 0) return;
            
            var maxFood = (city.Size + 1) * foodRows;
            city.FoodInStorage += maxFood * totalStorage / 100;
        }

        public static void AutoRemoveWorkersDistribution(this City city)
        {
            //TODO: remove scuentists & taxmen first
            var tiles = city.WorkedTiles.Where(t => t != city.Location);
            
            var organization = city.OrganizationLevel;
            // var hasSupermarket = city.ImprovementExists(ImprovementType.Supermarket);
            // var hasSuperhighways = city.ImprovementExists(ImprovementType.Superhighways);

            var unworked = tiles.OrderBy(t =>
                t.GetFood(organization == 0) + t.GetShields(organization == 0) +
                t.GetTrade(organization)).First();

            city.WorkedTiles.Remove(unworked);
        }

        public static void AutoAddDistributionWorkers(this City city)
        {
            // First determine how many workers are to be added
            int workersToBeAdded = city.Size + 1 - city.WorkedTiles.Count;

            var organization = city.OrganizationLevel;
            // var hasSupermarket = city.ImprovementExists(ImprovementType.Supermarket);
            // var hasSuperhighways = city.ImprovementExists(ImprovementType.Superhighways);
            var lowOrganization = organization == 0;
            
            // Make a list of tiles where you can add workers
            var tilesToAddWorkersTo = new List<Tile>();
            
            var tileValue = new List<double>();
            foreach (var tile in city.Location.CityRadius().Where(t =>
                         t.WorkedBy == null && t.Visibility[city.OwnerId] &&
                         !t.UnitsHere.Any<Unit>(u => u.Owner != city.Owner && u.AttackBase > 0) && t.CityHere == null))
            {
                var food = tile.GetFood(lowOrganization) * 1.5 ;
                var shields = tile.GetShields(lowOrganization);
                var trade = tile.GetTrade(organization) * 0.5;

                var total = food + shields + trade;
                var insertionIndex = tilesToAddWorkersTo.Count;
                for (; insertionIndex > 0; insertionIndex--)
                {
                    if (tileValue[insertionIndex-1] >= total)
                    {
                        break;
                    }
                }

                if (insertionIndex == tilesToAddWorkersTo.Count)
                {
                    if (insertionIndex >= workersToBeAdded) continue;
                    
                    tilesToAddWorkersTo.Add(tile);
                    tileValue.Add(total);
                }
                else
                {
                    tilesToAddWorkersTo.Insert(insertionIndex, tile);
                    tileValue.Insert(insertionIndex, total);
                }
            }

            foreach (var tile in tilesToAddWorkersTo.Take(workersToBeAdded))
            {
                tile.WorkedBy = city;
            }
        }
    }
}