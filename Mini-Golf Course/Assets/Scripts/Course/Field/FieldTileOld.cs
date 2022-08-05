using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Course.Field
{
    // TODO: REMOVE THIS CLASS
    // Represents the tile in a field
    // public class FieldTile
    // {
    //     public TileDirection tileDirection;
    //     public Vector3Int tilePosition;
    //     public List<TileModifier> tileModifiers = new List<TileModifier>(); // List of all the modifiers on the tile
    //     public Playfield playfield;
    //     public FieldTile parentTile;
    //     public List<FieldTile> childrenTiles = new List<FieldTile>();
    //
    //     public FieldTile()
    //     {
    //         
    //     }
    //
    //     public FieldTile(Playfield playfield, Vector3Int tilePosition, TileDirection tileDirection)
    //     {
    //         this.playfield = playfield;
    //         this.tilePosition = tilePosition;
    //         this.tileDirection = tileDirection;
    //     }
    //
    //     // Connects a child field tile to this field tile
    //     public void Connect(FieldTile child)
    //     {
    //         // Set the parent
    //         child.parentTile = this;
    //         
    //         // Store the child if its not actually the parent
    //         if (parentTile != null && child.tilePosition != parentTile.tilePosition)
    //         {
    //             childrenTiles.Add(child);
    //         }
    //     }
    //     
    //     // Disconnects a child field tile from this field tile
    //     public void Disconnect(FieldTile child)
    //     {
    //         // Remove the child if its actually the parent
    //         if (child.parentTile == this)
    //         {
    //             childrenTiles.Remove(child);
    //         }
    //         
    //         // Remove the parent
    //         child.parentTile = null;
    //     }
    //     
    //     // Adds a modifier
    //     public void AddModifier(TileModifier modifier)
    //     {
    //         tileModifiers.Add(modifier);
    //     }
    //     
    //     // Is the tile straight
    //     public bool IsStraight()
    //     {
    //         return tileDirection == TileDirection.Horizontal || tileDirection == TileDirection.Vertical;
    //     }
    //     
    //     // Can the tile be modified (Straight and Corners)
    //     public bool IsModable()
    //     {
    //         return IsStraight() || tileDirection == TileDirection.CornerUpLeft || tileDirection == TileDirection.CornerUpRight || tileDirection == TileDirection.CornerDownLeft || tileDirection == TileDirection.CornerDownRight;
    //     }
    //     
    //     // Does the tile have a slope as a child
    //     public bool IsSlope()
    //     {
    //         return tileDirection == TileDirection.HorizontalSlopeLeft || tileDirection == TileDirection.HorizontalSlopeRight || tileDirection == TileDirection.VerticalSlopeUp || tileDirection == TileDirection.VerticalSlopeDown;
    //     }
    //     
    //     // Is the grandparent tile aligned with the current tile
    //     public bool GrandParentIsAligned()
    //     {
    //         // Is the parent empty
    //         if (parentTile != null && parentTile.tileDirection == TileDirection.Empty)
    //         {
    //             // Debug.Log("Parent is empty...");
    //             // Does the grandparent exist
    //             if (parentTile.parentTile != null)
    //             {
    //                 // Debug.Log("Grandparent exists...");
    //                 FieldTile grandparent = parentTile.parentTile;
    //                 Vector3Int grandparentDirection = grandparent.tilePosition - parentTile.tilePosition;
    //                 Debug.Log("Tile at: " + tilePosition.ToString() + " has a grandparent at: " + grandparent.tilePosition.ToString() + " with a direction of: " + grandparentDirection.ToString());
    //                 
    //                 
    //                 if (grandparentDirection.x != 0 && (tileDirection == TileDirection.HorizontalSlopeLeft || tileDirection == TileDirection.HorizontalSlopeRight))
    //                 {
    //                     // Jagged
    //                     return false;
    //                 }
    //                 if (grandparentDirection.y != 0 && (tileDirection == TileDirection.VerticalSlopeUp || tileDirection == TileDirection.VerticalSlopeDown))
    //                 {
    //                     // Jagged
    //                     return false;
    //                 }
    //             }
    //         }
    //
    //         return true;
    //     }
    // }
}
