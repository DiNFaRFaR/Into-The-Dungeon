using UnityEngine;

public enum DoorDirection
{
    Up, Down, Left, Right
}

public static class DoorDirectionExtensions
{
    public static DoorDirection Opposite(this DoorDirection d)
    {
        return d switch
        {
            DoorDirection.Up => DoorDirection.Down,
            DoorDirection.Down => DoorDirection.Up,
            DoorDirection.Left => DoorDirection.Right,
            DoorDirection.Right => DoorDirection.Left,
            _ => DoorDirection.Up
        };
    }
}