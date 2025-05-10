using Core.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Components.Camera;
using Core;
using System.Linq;


public static class VisibleActorsManager
{
    private static Dictionary<IMoving, MovingState> movingActorsDictionary = new();

    private static Coroutine updateVisibleObjects;
    private static float currentTime;
    private static float defaultTime = 10;
    private static Vector2 rangeOfTimer = new Vector2(-2, 0);

    public static void StartUpdatingVisibleObjects()
    {
        if (updateVisibleObjects == null)
            updateVisibleObjects = Player.Instance.StartCoroutine(TimerOfUpdatingVisibleActors());
        else
            Debug.Log("Trying to create already existing coroutine");
    }

    private static IEnumerator TimerOfUpdatingVisibleActors()
    {
        while (true)
        {
            currentTime -= Time.deltaTime;

            if (currentTime <= 0)
            {
                currentTime = defaultTime + Random.Range(rangeOfTimer.x, rangeOfTimer.y);
                VisibleObjectsTick();

            }

            yield return null;
        }
    }

    public static void VisibleObjectsTick()
    {
        currentTime = defaultTime + Random.Range(rangeOfTimer.x, rangeOfTimer.y);
        UpdateVisibleObjects(GetVisibleObjects());
    }

    // How this system works
    // Everything happens during ticks, with an interval of defaultTime + rangeOfTimer
    // During a tick, actors are distributed into three categories: InPlayerView (actors within the player's view or in their loaded sector), OutsidePlayerView (actors outside the player's view), and ActorsToUnload (actors that need to be unloaded)

    // 1. All actors in OutsidePlayerView become ActorsToUnload so they can be unloaded in the next tick.
    // 2. All InPlayerView actors are checked to see if the player can see them. If not, the actor is marked as not visible (IsVisible = false). If the actor is outside the sector and the sector is not loaded, it moves to OutsidePlayerView.
    // 3. Actors from the newVisibleObjects list are marked as InPlayerView because we can see them (IsVisible = true).
    // 4. All ActorsToUnload are unloaded.

    // The IsVisible setting ensures that objects we do not see get unloaded.

    public static void UpdateVisibleObjects(HashSet<IMoving> newVisibleObjects)
    {
        for (int index = 0; index < movingActorsDictionary.Keys.Count; index++)
        {
            IMoving movingActor = movingActorsDictionary.ElementAt(index).Key;

            if (movingActorsDictionary[movingActor] == MovingState.OutsidePlayerView)
                movingActorsDictionary[movingActor] = MovingState.ActorsToUnload;

            if (!newVisibleObjects.Contains(movingActor) && movingActorsDictionary[movingActor] == MovingState.InPlayerView)
            {
                movingActor.IsVisible = false;

                if (movingActor.IsOutOfSector && !movingActor.IsSectorLoaded)
                    movingActorsDictionary[movingActor] = MovingState.OutsidePlayerView;
            }
        }

        foreach (var movingObject in newVisibleObjects)
        {
            movingObject.IsVisible = true;

            movingActorsDictionary[movingObject] = MovingState.InPlayerView;
        }

        RemoveUnvisibleObjects();
    }


    public static void SearchVisibleObjects()
    {
        foreach (var movingObject in GetVisibleObjects())
        {
            movingObject.IsVisible = true;

            movingActorsDictionary[movingObject] = MovingState.InPlayerView;
        }
    }
    public static void RemoveUnvisibleObjects()
    {
        List<IMoving> unloadList = new();

        for (int index = 0; index < movingActorsDictionary.Keys.Count; index++)
        {
            IMoving unloadMovingObject = movingActorsDictionary.ElementAt(index).Key;

            if (movingActorsDictionary[unloadMovingObject] == MovingState.ActorsToUnload)
                unloadList.Add(unloadMovingObject);
        }

        foreach (var item in unloadList)
            item.UnloadIfOutOfBounds();

    }

    public static void AddActingObjects(List<IMoving> movings)
    {
        foreach (var item in movings)
        {
            if (!movingActorsDictionary.ContainsKey(item))
                movingActorsDictionary.Add(item, MovingState.InPlayerView);
        }
    }

    public static void AddActingObject(IMoving movingObject)
    {
        if (!movingActorsDictionary.ContainsKey(movingObject))
            movingActorsDictionary.Add(movingObject, MovingState.InPlayerView);
    }

    public static void RemoveActingObject(IMoving moving) =>
        movingActorsDictionary.Remove(moving);

    private static HashSet<IMoving> GetVisibleObjects()
    {
        var visibleObjects = new HashSet<IMoving>();

        foreach (var actor in movingActorsDictionary.Keys)
        {
            if (!CameraManager.IsBoundsInCameraView(actor.GetBounds()))
                continue;

            visibleObjects.Add(actor);
        }

        return visibleObjects;
    }

    public enum MovingState
    {
        InPlayerView,
        ActorsToUnload,
        OutsidePlayerView
    }
}
