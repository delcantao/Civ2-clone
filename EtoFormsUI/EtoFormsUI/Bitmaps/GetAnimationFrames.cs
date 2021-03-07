﻿using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using Civ2engine;
using Civ2engine.Units;
using Civ2engine.Events;
using Civ2engine.Enums;

namespace EtoFormsUI
{
    public class GetAnimationFrames : BaseInstance
    {
        // Get animation frames for waiting unit
        public static List<Bitmap> UnitWaiting(IUnit activeUnit)
        {
            var animationFrames = new List<Bitmap>();

            // Get coords of central tile & which squares are to be drawn
            int[] activeUnitXY = new int[] { activeUnit.X, activeUnit.Y };
            var coordsOffsetsToBeDrawn = new List<int[]>
            {
                new int[] {0, -2},
                new int[] {-1, -1},
                new int[] {1, -1},
                new int[] {0, 0},
                new int[] {-1, 1},
                new int[] {1, 1},
                new int[] {0, 2}
            };

            // Get 2 frames (one with and other without the active unit/moving piece)
            int[] coordsOffsetsPx;
            int x, y;
            for (int frame = 0; frame < 2; frame++)
            {
                var _bitmap = new Bitmap(2 * Game.Xpx, 3 * Game.Ypx, PixelFormat.Format32bppRgba);
                using (var g = new Graphics(_bitmap))
                {
                    // Fill bitmap with black (necessary for correct drawing if image is on upper map edge)
                    g.FillRectangle(Brushes.Black, new Rectangle(0, 0, 2 * Game.Xpx, 3 * Game.Ypx));

                    foreach (int[] coordsOffsets in coordsOffsetsToBeDrawn)
                    {
                        // Change coords of central offset
                        x = activeUnitXY[0] + coordsOffsets[0];
                        y = activeUnitXY[1] + coordsOffsets[1];
                        coordsOffsetsPx = new int[] { coordsOffsets[0] * Game.Xpx, coordsOffsets[1] * Game.Ypx };

                        if (x < 0 || y < 0 || x >= 2 * Map.Xdim || y >= Map.Ydim) break;    // Make sure you're not drawing tiles outside map bounds

                        // Tiles
                        Draw.Tile(g, x, y, Game.Zoom, new Point(coordsOffsetsPx[0], coordsOffsetsPx[1] + Game.Ypx));

                        // Units
                        List<IUnit> unitsHere = Game.GetUnits.Where(u => u.X == x && u.Y == y).ToList();
                        if (unitsHere.Count > 0)
                        {
                            IUnit unit;
                            // If this is not tile with active unit or viewing piece, draw last unit on stack
                            if (x != activeUnitXY[0] || y != activeUnitXY[1])
                            {
                                unit = unitsHere.Last();
                                if (!unit.IsInCity) Draw.Unit(g, unit, unit.IsInStack, Game.Zoom, new Point(coordsOffsetsPx[0], coordsOffsetsPx[1]));
                            }
                        }

                        // City
                        City city = Game.GetCities.Find(c => c.X == x && c.Y == y);
                        if (city != null) Draw.City(g, city, true, Game.Zoom, new Point(coordsOffsetsPx[0], coordsOffsetsPx[1]));

                        // Draw active unit if it's not moving
                        if (unitsHere.Count > 0)
                        {
                            // This tile has active unit/viewing piece
                            if (x == activeUnitXY[0] && y == activeUnitXY[1])
                            {
                                // For first frame draw unit, for second not
                                if (frame == 0) Draw.Unit(g, Game.ActiveUnit, Game.ActiveUnit.IsInStack, Game.Zoom, new Point(coordsOffsetsPx[0], coordsOffsetsPx[1]));
                            }
                        }
                    }

                    // Gridlines
                    if (Game.Options.Grid)
                    {
                        foreach (int[] coordsOffsets in coordsOffsetsToBeDrawn)
                        {
                            coordsOffsetsPx = new int[] { coordsOffsets[0] * Game.Xpx, coordsOffsets[1] * Game.Ypx + Game.Ypx };
                            Draw.Grid(g, Game.Zoom, new Point(coordsOffsetsPx[0], coordsOffsetsPx[1]));
                        }
                    }

                    // City names
                    foreach (int[] coordsOffsets in coordsOffsetsToBeDrawn)
                    {
                        // Change coords of central offset
                        x = activeUnitXY[0] + coordsOffsets[0];
                        y = activeUnitXY[1] + coordsOffsets[1];

                        if (x >= 0 && y >= 0 && x < 2 * Map.Xdim && y < Map.Ydim) break;   // Make sure you're not drawing tiles outside map bounds

                        City city = Game.GetCities.Find(c => c.X == x && c.Y == y);
                        if (city != null)
                        {
                            Draw.CityName(g, city, Game.Zoom, new int[] { x, y });
                            //Bitmap cityNameBitmap = Draw.CityName(city, Game.Zoom);
                            //g.DrawImage(cityNameBitmap,
                            //    Game.Xpx * (coordsOffsets[0] + 1) - cityNameBitmap.Width / 2,
                            //    Game.Ypx * coordsOffsets[1] + 5 * 2 / Game.Ypx + Game.Ypx);
                        }
                    }

                    ////Viewing piece (is drawn on top of everything)
                    //if (MapPanel.ViewingPiecesMode)
                    //{
                    //    if (frame == 0)
                    //    {
                    //        g.DrawImage(Draw.ViewPiece, 0, 16);
                    //    }
                    //}
                }
                animationFrames.Add(_bitmap);
            }

            return animationFrames;
        }

        // Get animation frames for moving unit
        public static List<Bitmap> UnitMoving(IUnit activeUnit)
        {
            List<Bitmap> animationFrames = new List<Bitmap>();

            // Get coords of central tile & which squares are to be drawn
            int[] activeUnitPrevXY = activeUnit.PrevXY;
            List<int[]> coordsOffsetsToBeDrawn = new List<int[]>
            {
                new int[] {-2, -4},
                new int[] {0, -4},
                new int[] {2, -4},
                new int[] {-3, -3},
                new int[] {-1, -3},
                new int[] {1, -3},
                new int[] {3, -3},
                new int[] {-2, -2},
                new int[] {0, -2},
                new int[] {2, -2},
                new int[] {-3, -1},
                new int[] {-1, -1},
                new int[] {1, -1},
                new int[] {3, -1},
                new int[] {-2, 0},
                new int[] {0, 0},
                new int[] {2, 0},
                new int[] {-3, 1},
                new int[] {-1, 1},
                new int[] {1, 1},
                new int[] {3, 1},
                new int[] {-2, 2},
                new int[] {0, 2},
                new int[] {2, 2},
                new int[] {-3, 3},
                new int[] {-1, 3},
                new int[] {1, 3},
                new int[] {3, 3},
                new int[] {-2, 4},
                new int[] {0, 4},
                new int[] {2, 4}
            };

            // First draw main bitmap with everything except the moving unit
            int[] coordsOffsetsPx;
            var _mainBitmap = new Bitmap(6 * Game.Xpx, 7 * Game.Ypx, PixelFormat.Format32bppRgba);
            using (var g = new Graphics(_mainBitmap))
            {
                g.FillRectangle(Brushes.Black, new Rectangle(0, 0, 6 * Game.Xpx, 7 * Game.Ypx));    // Fill bitmap with black (necessary for correct drawing if image is on upper map edge)

                foreach (int[] coordsOffsets in coordsOffsetsToBeDrawn)
                {
                    // Change coords of central offset
                    int x = activeUnitPrevXY[0] + coordsOffsets[0];
                    int y = activeUnitPrevXY[1] + coordsOffsets[1];
                    coordsOffsetsPx = new int[] { (coordsOffsets[0] + 2) * Game.Xpx, (coordsOffsets[1] + 3) * Game.Ypx };

                    if (x < 0 || y < 0 || x >= 2 * Map.Xdim || y >= Map.Ydim) break;   // Make sure you're not drawing tiles outside map bounds

                    // Tiles
                    int civId = Game.WhichCivsMapShown;
                    if ((civId < 8 && Map.IsTileVisibleC2(x, y, civId)) || civId == 8)
                    {
                        Draw.Tile(g, x, y, Game.Zoom, new Point(coordsOffsetsPx[0], coordsOffsetsPx[1]));

                        // Implement dithering in all 4 directions if necessary
                        if (civId != 8)
                        {
                            for (int tileX = 0; tileX < 2; tileX++)
                            {
                                for (int tileY = 0; tileY < 2; tileY++)
                                {
                                    int[] offset = new int[] { -1, 1 };
                                    int xNew = x + offset[tileX];
                                    int yNew = y + offset[tileY];
                                    if (xNew >= 0 && xNew < 2 * Map.Xdim && yNew >= 0 && yNew < Map.Ydim)   // Don't observe outside map limits
                                        if (!Map.IsTileVisibleC2(xNew, yNew, civId))   // Surrounding tile is not visible -> dither
                                            Draw.Dither(g, tileX, tileY, Game.Zoom, new Point(coordsOffsetsPx[0] + Game.Xpx * tileX, coordsOffsetsPx[1] + Game.Ypx * tileY));
                                }
                            }
                        }
                    }

                    // Units
                    List<IUnit> unitsHere = Game.GetUnits.Where(u => u.X == x && u.Y == y).ToList();
                    // If active unit is in this list-- > remove it
                    if (unitsHere.Contains(activeUnit)) unitsHere.Remove(activeUnit);
                    
                    if (unitsHere.Count > 0)
                    {
                        IUnit unit;
                        // If this is not tile with active unit or viewing piece, draw last unit on stack
                        if (!unitsHere.Contains(activeUnit))
                        {
                            unit = unitsHere.Last();
                            if (!unit.IsInCity)
                            {
                                Draw.Unit(g, unit, unitsHere.Count > 1, Game.Zoom, new Point(coordsOffsetsPx[0], coordsOffsetsPx[1] - Game.Ypx));
                            }
                        }
                    }

                    // Cities
                    City city = Game.GetCities.Find(c => c.X == x && c.Y == y);
                    if (city != null)
                    {
                        Draw.City(g, city, true, Game.Zoom, new Point(coordsOffsetsPx[0], coordsOffsetsPx[1] - Game.Ypx));
                    }
                }

                // City names
                // Add additional coords for drawing city names
                coordsOffsetsToBeDrawn.Add(new int[] { -3, -5 });
                coordsOffsetsToBeDrawn.Add(new int[] { -1, -5 });
                coordsOffsetsToBeDrawn.Add(new int[] { 1, -5 });
                coordsOffsetsToBeDrawn.Add(new int[] { 3, -5 });
                coordsOffsetsToBeDrawn.Add(new int[] { -4, -2 });
                coordsOffsetsToBeDrawn.Add(new int[] { 4, -2 });
                coordsOffsetsToBeDrawn.Add(new int[] { -4, 0 });
                coordsOffsetsToBeDrawn.Add(new int[] { 4, 0 });
                coordsOffsetsToBeDrawn.Add(new int[] { -4, 2 });
                coordsOffsetsToBeDrawn.Add(new int[] { 4, 2 });
                foreach (int[] coordsOffsets in coordsOffsetsToBeDrawn)
                {
                    // Change coords of central offset
                    int x = activeUnitPrevXY[0] + coordsOffsets[0];
                    int y = activeUnitPrevXY[1] + coordsOffsets[1];

                    if (x >= 0 && y >= 0 && x < 2 * Map.Xdim && y < Map.Ydim)    // Make sure you're not drawing tiles outside map bounds
                    {
                        City city = Game.GetCities.Find(c => c.X == x && c.Y == y);
                        if (city != null)
                        {
                            Draw.CityName(g, city, Game.Zoom, new int[] { coordsOffsets[0] + 2, coordsOffsets[1] + 3 });
                        }
                    }
                }
            }

            // Now draw the moving unit on top of main bitmap
            int noFramesForOneMove = 8;
            for (int frame = 0; frame < noFramesForOneMove; frame++)
            {
                // Make a clone of the main bitmap in order to draw frames with unit on it
                var _bitmapWithMovingUnit = new Bitmap(_mainBitmap);
                using (var g = new Graphics(_bitmapWithMovingUnit))
                {
                    // Draw active unit on top of everything
                    //int[] activeUnitDrawOffset = new int[] { 0, 0 };
                    //switch (Game.ActiveUnit.PrevXY)
                    //{
                    //    case 0: activeUnitDrawOffset = new int[] { 1, -1 }; break;
                    //    case 1: activeUnitDrawOffset = new int[] { 2, 0 }; break;
                    //    case 2: activeUnitDrawOffset = new int[] { 1, 1 }; break;
                    //    case 3: activeUnitDrawOffset = new int[] { 0, 2 }; break;
                    //    case 4: activeUnitDrawOffset = new int[] { -1, 1 }; break;
                    //    case 5: activeUnitDrawOffset = new int[] { -2, 0 }; break;
                    //    case 6: activeUnitDrawOffset = new int[] { -1, -1 }; break;
                    //    case 7: activeUnitDrawOffset = new int[] { 0, -2 }; break;
                    //}
                    int[] unitDrawOffset = new int[] { activeUnit.X - activeUnit.PrevXY[0], activeUnit.Y - activeUnit.PrevXY[1] };
                    unitDrawOffset[0] *= Game.Xpx / noFramesForOneMove * (frame + 1);
                    unitDrawOffset[1] *= Game.Ypx / noFramesForOneMove * (frame + 1);

                    Draw.Unit(g, activeUnit, false, Game.Zoom, new Point(2 * Game.Xpx + unitDrawOffset[0], 2 * Game.Ypx + unitDrawOffset[1]));
                }
                animationFrames.Add(_bitmapWithMovingUnit);
            }

            return animationFrames;
        }

        //Get animation frames for view piece
        //public static List<Bitmap> ViewPiece()
        //{
        //    List<Bitmap> animationFrames = new List<Bitmap>();
        //    for (int frame = 0; frame < 2; frame++)
        //    {
        //        Bitmap _mainBitmap = new Bitmap(64, 32);
        //        using (Graphics g = Graphics.FromImage(_mainBitmap))
        //        {
        //            g.DrawImage(Game.WholeMap, 0, 0, 32 * MapPanel.ActiveXY[0], 16 * MapPanel.ActiveXY[0]);
        //            if (frame == 0)
        //                g.DrawImage(Draw.ViewPiece, 0, 0, 32 * MapPanel.ActiveXY[0], 16 * MapPanel.ActiveXY[0]);
        //        }
        //        animationFrames.Add(_mainBitmap);
        //    }
        //    return animationFrames;
        //}

        // Get attack animation frames
        //public static List<Bitmap> UnitAttack(IUnit attacker, IUnit defender, List<bool> combatRoundsAttackerWins)
        public static List<Bitmap> UnitAttack(UnitEventArgs e)
        {
            List<Bitmap> animationFrames = new List<Bitmap>();

            // Which squares are to be drawn
            List<int[]> coordsOffsetsToBeDrawn = new List<int[]>
            {
                new int[] {-2, -4},
                new int[] {0, -4},
                new int[] {2, -4},
                new int[] {-3, -3},
                new int[] {-1, -3},
                new int[] {1, -3},
                new int[] {3, -3},
                new int[] {-2, -2},
                new int[] {0, -2},
                new int[] {2, -2},
                new int[] {-3, -1},
                new int[] {-1, -1},
                new int[] {1, -1},
                new int[] {3, -1},
                new int[] {-2, 0},
                new int[] {0, 0},
                new int[] {2, 0},
                new int[] {-3, 1},
                new int[] {-1, 1},
                new int[] {1, 1},
                new int[] {3, 1},
                new int[] {-2, 2},
                new int[] {0, 2},
                new int[] {2, 2},
                new int[] {-3, 3},
                new int[] {-1, 3},
                new int[] {1, 3},
                new int[] {3, 3},
                new int[] {-2, 4},
                new int[] {0, 4},
                new int[] {2, 4}
            };

            // First draw main bitmap with everything except the moving unit
            int[] coordsOffsetsPx;
            var _mainBitmap = new Bitmap(6 * Game.Xpx, 7 * Game.Ypx, PixelFormat.Format32bppRgba);
            using (var g = new Graphics(_mainBitmap))
            {
                g.FillRectangle(Brushes.Black, new Rectangle(0, 0, 6 * Game.Xpx, 7 * Game.Ypx));    // Fill bitmap with black (necessary for correct drawing if image is on upper map edge)

                foreach (int[] coordsOffsets in coordsOffsetsToBeDrawn)
                {
                    // Change coords of central offset
                    int x = e.Attacker.X + coordsOffsets[0];
                    int y = e.Attacker.Y + coordsOffsets[1];
                    coordsOffsetsPx = new int[] { (coordsOffsets[0] + 2) * Game.Xpx, (coordsOffsets[1] + 3) * Game.Ypx };

                    if (x < 0 || y < 0 || x >= 2 * Map.Xdim || y >= Map.Ydim) break;   // Make sure you're not drawing tiles outside map bounds

                    // Tiles
                    int civId = Game.WhichCivsMapShown;
                    if ((civId < 8 && Map.IsTileVisibleC2(x, y, civId)) || civId == 8)
                    {
                        Draw.Tile(g, x, y, Game.Zoom, new Point(coordsOffsetsPx[0], coordsOffsetsPx[1]));

                        // Implement dithering in all 4 directions if necessary
                        if (civId != 8)
                        {
                            for (int tileX = 0; tileX < 2; tileX++)
                            {
                                for (int tileY = 0; tileY < 2; tileY++)
                                {
                                    int[] offset = new int[] { -1, 1 };
                                    int xNew = x + offset[tileX];
                                    int yNew = y + offset[tileY];
                                    if (xNew >= 0 && xNew < 2 * Map.Xdim && yNew >= 0 && yNew < Map.Ydim)   // Don't observe outside map limits
                                        if (!Map.IsTileVisibleC2(xNew, yNew, civId))   // Surrounding tile is not visible -> dither
                                            Draw.Dither(g, tileX, tileY, Game.Zoom, new Point(coordsOffsetsPx[0] + Game.Xpx * tileX, coordsOffsetsPx[1] + Game.Ypx * tileY));
                                }
                            }
                        }
                    }

                    // Units
                    // If tile is with attacking & defending unit, draw these two first
                    // TODO: this won't draw correctly if unit is in city
                    if (x == e.Attacker.X && y == e.Attacker.Y)
                    {
                        Draw.Unit(g, e.Attacker, e.Attacker.IsInStack, Game.Zoom, new Point(coordsOffsetsPx[0], coordsOffsetsPx[1] - Game.Ypx));
                    }
                    else if (x == e.Defender.X && y == e.Defender.Y)
                    {
                        Draw.Unit(g, e.Defender, e.Defender.IsInStack, Game.Zoom, new Point(coordsOffsetsPx[0], coordsOffsetsPx[1] - Game.Ypx));
                    }
                    else    // Other units
                    {
                        var units = Game.GetUnits.Where(u => u.X == x && u.Y == y).ToList();
                        if (units.Count > 0) 
                        {
                            var unit = units.Last();
                            if (!unit.IsInCity)
                            {
                                Draw.Unit(g, unit, unit.IsInStack, Game.Zoom, new Point(coordsOffsetsPx[0], coordsOffsetsPx[1] - Game.Ypx));
                            }
                        } 
                    }

                    // Cities
                    City city = Game.GetCities.Find(c => c.X == x && c.Y == y);
                    if (city != null)
                    {
                        Draw.City(g, city, true, Game.Zoom, new Point(coordsOffsetsPx[0], coordsOffsetsPx[1] - Game.Ypx));
                    }
                }

                // City names
                // Add additional coords for drawing city names
                coordsOffsetsToBeDrawn.Add(new int[] { -3, -5 });
                coordsOffsetsToBeDrawn.Add(new int[] { -1, -5 });
                coordsOffsetsToBeDrawn.Add(new int[] { 1, -5 });
                coordsOffsetsToBeDrawn.Add(new int[] { 3, -5 });
                coordsOffsetsToBeDrawn.Add(new int[] { -4, -2 });
                coordsOffsetsToBeDrawn.Add(new int[] { 4, -2 });
                coordsOffsetsToBeDrawn.Add(new int[] { -4, 0 });
                coordsOffsetsToBeDrawn.Add(new int[] { 4, 0 });
                coordsOffsetsToBeDrawn.Add(new int[] { -4, 2 });
                coordsOffsetsToBeDrawn.Add(new int[] { 4, 2 });
                foreach (int[] coordsOffsets in coordsOffsetsToBeDrawn)
                {
                    // Change coords of central offset
                    int x = e.Attacker.X + coordsOffsets[0];
                    int y = e.Attacker.Y + coordsOffsets[1];

                    if (x >= 0 && y >= 0 && x < 2 * Map.Xdim && y < Map.Ydim)    // Make sure you're not drawing tiles outside map bounds
                    {
                        City city = Game.GetCities.Find(c => c.X == x && c.Y == y);
                        if (city != null)
                        {
                            Draw.CityName(g, city, Game.Zoom, new int[] { coordsOffsets[0] + 2, coordsOffsets[1] + 3 });
                        }
                    }
                }
            }

            // Now draw the battle animation on top of attacking & defending unit
            // Number of battle rounds / 5 determines number of explosions (must be at least one). Each explosion has 8 frames.
            Point point;
            for (int explosion = 0; explosion < e.CombatRoundsAttackerWins.Count / 5; explosion++)
            {
                for (int frame = 0; frame < 8; frame++)
                {
                    // Make a clone of the main bitmap in order to draw frames with unit on it
                    var _bitmapWithExplosions = new Bitmap(_mainBitmap);
                    using (var g = new Graphics(_bitmapWithExplosions))
                    {
                        // Draw chaning HP of both units
                        foreach (int[] coordsOffsets in coordsOffsetsToBeDrawn)
                        {
                            int x = e.Attacker.X + coordsOffsets[0];
                            int y = e.Attacker.Y + coordsOffsets[1];

                            if (x == e.Attacker.X && y == e.Attacker.Y)
                            {
                                Draw.UnitShield(g, e.Attacker.Type, e.Attacker.Owner.Id, e.Attacker.Order, e.Attacker.IsInStack, e.AttackerHitpoints[explosion * 5], e.Attacker.HitpointsBase, Game.Zoom, new Point(2 * Game.Xpx, 2 * Game.Ypx));
                                Draw.UnitSprite(g, e.Attacker.Type, false, false, Game.Zoom, new Point(2 * Game.Xpx, 2 * Game.Ypx));
                            }
                            else if (x == e.Defender.X && y == e.Defender.Y)
                            {
                                Draw.UnitShield(g, e.Defender.Type, e.Defender.Owner.Id, e.Defender.Order, e.Defender.IsInStack, e.DefenderHitpoints[explosion * 5], e.Defender.HitpointsBase, Game.Zoom, new Point((2 + coordsOffsets[0]) * Game.Xpx, (2 + coordsOffsets[1]) * Game.Ypx));
                                Draw.UnitSprite(g, e.Defender.Type, e.Defender.Order == OrderType.Sleep, e.Defender.Order == OrderType.Fortified, Game.Zoom, new Point((2 + coordsOffsets[0]) * Game.Xpx, (2 + coordsOffsets[1]) * Game.Ypx));
                            }
                        }

                        // Draw explosion
                        if (e.CombatRoundsAttackerWins[explosion]) point = new Point((int)(Game.Xpx * (2.5 + e.Defender.X - e.Attacker.X)), Game.Ypx * (3 + (e.Defender.Y - e.Attacker.Y))); // Explosion on defender
                        else point = new Point((int)(Game.Xpx * 2.5), Game.Ypx * 3); // Explosion on attacker

                        //g.DrawImage(Images.BattleAnim[frame], point);
                        Draw.BattleAnim(g, frame, Game.Zoom, point);
                    }
                    animationFrames.Add(_bitmapWithExplosions);
                }
            }
            return animationFrames;
        }
    }
}
