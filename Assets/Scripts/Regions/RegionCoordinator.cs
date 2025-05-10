using UnityEngine;
using Core;
using Components.Camera;
using System.Collections.Generic;
namespace Regions
{
    public static class RegionCoordinator
    {
        private static Vector3 PlayerPosition
        {
            get
            {
                if (Player.Instance == null || Player.Instance.CarGameObjectIsNull) 
                    return Vector3.zero;
                
                return Player.Instance.PlayerCarGameObject.transform.position;
            }
        }

        public static void FindCurrentPlayerLocation()
        {
            if(Player.Instance == null || Player.Instance.CarGameObjectIsNull)
                return;
            
            RegionManager.UpdateVisibleLocations(GetVisibleLocations());
        }

        public static List<Location> GetLocationsFromPosition(Vector3 position, List<Sector> sectors = null)
        {
            if(sectors==null) sectors = GetSectorsFromPosition(position);
            
            var locations = new List<Location>();
            foreach (var sector in sectors)
            {
                foreach (var location in sector.Locations)
                {
                    if (location.IsInsideBounds(position))
                    {
                        locations.Add(location);
                    }
                }
            }
            return locations;
        }

        public static List<Sector> GetSectorsFromPosition(Vector3 position, Region region = null)
        {
            var sectors = new List<Sector>();
            if (region == null) region = GetRegionFromPosition(position);
            if (region == null) return sectors;

            foreach (var sectorInRegion in region.Sectors)
            {
                if (sectorInRegion.IsInsideBounds(position))
                {
                    sectors.Add(sectorInRegion);
                }
            }
            return sectors;
        }

        public static Region GetRegionFromPosition(Vector3 position)
        {
            foreach (var region in RegionManager.Regions)
            {
                if (region.IsInsideBounds(position))
                {
                    return region;
                }
            }
            return null;
        }

        private static List<Location> GetVisibleLocations()
        {
            var visibleLocation = new List<Location>();
            foreach (var region in RegionManager.Regions)
            {
                if (!CameraManager.IsBoundsInCameraView(region.Bounds))
                    continue;

                visibleLocation.Add(region);

                foreach (var sector in region.Sectors)
                {
                    if (!CameraManager.IsBoundsInCameraView(sector.Bounds))
                        continue;

                    visibleLocation.Add(sector);

                    foreach (var location in sector.Locations)
                    {
                        if (!CameraManager.IsBoundsInCameraView(location.Bounds))
                            continue;
                        visibleLocation.Add(location);
                    }
                }

            }
            return visibleLocation;
        }
    }
}


