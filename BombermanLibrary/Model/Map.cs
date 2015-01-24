using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Bomberman.Model
{
    /// <summary>
    /// The game map, which contains the various characters and objects.
    /// </summary>
    public sealed class Map : IDGenerator
    {
        private ushort id;

        private int creatureCompare(Creature x, Creature y)
        {
            // TODO: pour éviter le clignotement de 2 sprites exactement sur la même ligne,
            // il faudrait aussi tenir compte de l'axe X dans le tri
            if (x.Position.Y < y.Position.Y)
            {
                return -1;
            }
            else if (x.Position.Y > y.Position.Y)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Absolute maximum width of maps.
        /// </summary>
        public const int MaxWidth = 256;

        /// <summary>
        /// Absolute maximum height of maps.
        /// </summary>
        public const int MaxHeight = 256; // MaxWidth * MaxHeight must be <= 65536.

        /// <summary>
        /// The width of the map, in playable tiles.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// The height of the map, in playable tiles.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// The numerical ID that identifies the theme of the map.
        /// The theme should be used to determinate the border, ground and wall graphics.
        /// </summary>
        public int Theme { get; private set; }

        // Part 1 - Objects

        private List<Blast> blasts;
        private List<Bomb> bombs;
        private List<Bonus> bonuses;
        private List<PowerUp> powerUps;
        private List<SpawnPoint> spawnPoints;
        private List<Wall> walls;

        private List<Object> objects;

        private Object[,] cache;

        /// <summary>
        /// Read-only list of blasts on the map.
        /// </summary>
        public IList<Blast> Blasts { get { return blasts.AsReadOnly(); } }

        /// <summary>
        /// Read-only list of bombs on the map.
        /// </summary>
        public IList<Bomb> Bombs { get { return bombs.AsReadOnly(); } }

        /// <summary>
        /// Read-only list of bonuses on the map.
        /// </summary>
        public IList<Bonus> Bonuses { get { return bonuses.AsReadOnly(); } }

        /// <summary>
        /// Read-only list of powerups on the map.
        /// </summary>
        public IList<PowerUp> PowerUps { get { return powerUps.AsReadOnly(); } }

        /// <summary>
        /// Read-only list of spawn points on the map.
        /// </summary>
        public IList<SpawnPoint> SpawnPoints { get { return spawnPoints.AsReadOnly(); } }

        /// <summary>
        /// Read-only list of walls on the map.
        /// </summary>
        public IList<Wall> Walls { get { return walls.AsReadOnly(); } }

        /// <summary>
        /// Read-only list of objects on the map.
        /// </summary>
        public IList<Object> Objects { get { return objects.AsReadOnly(); } }

        // Part 2 - Creatures

        private List<Bomberman> bombermen;
        private List<Monster> monsters;

        private List<Creature> sortedCreatures;

        /// <summary>
        /// Read-only list of bombermen on the map.
        /// </summary>
        public IList<Bomberman> Bombermen { get { return bombermen.AsReadOnly(); } }

        /// <summary>
        /// Read-only list of monsters on the map.
        /// </summary>
        public IList<Monster> Monsters { get { return monsters.AsReadOnly(); } }

        /// <summary>
        /// Read-only list of creatures on the map sorted by their Y position.
        /// The list is sorted as long as regular calls to SortCreatures are made.
        /// </summary>
        public IList<Creature> SortedCreatures { get { return sortedCreatures.AsReadOnly(); } }

        /// <summary>
        /// Add a game object to the map.
        /// The position of the object is cached to increase performances when retrieving it.
        /// </summary>
        public void Add(Object o)
        {
            Point p = o.Position;
            if (this.InBoundaries(p))
            {
                if (cache[p.Y, p.X] == null)
                {
                    if (o is Blast)
                    {
                        blasts.Add((Blast)o);
                    }
                    else if (o is Bomb)
                    {
                        bombs.Add((Bomb)o);
                    }
                    else if (o is Bonus)
                    {
                        bonuses.Add((Bonus)o);
                    }
                    else if (o is PowerUp)
                    {
                        powerUps.Add((PowerUp)o);
                    }
                    else if (o is SpawnPoint)
                    {
                        spawnPoints.Add((SpawnPoint)o);
                    }
                    else if (o is Wall)
                    {
                        walls.Add((Wall)o);
                    }
                    else
                    {
                        throw new InvalidOperationException("Unsupported object");
                    }

                    objects.Add(o);

                    // Now put the object in cache
                    cache[p.Y, p.X] = o;
                }
                else
                {
                    throw new InvalidOperationException("Tile already occupied by another game object");
                }
            }
            else
            {
                throw new InvalidOperationException("Object is out of map boundaries");
            }
        }

        /// <summary>
        /// Remove a game object from the map.
        /// </summary>
        public void Remove(Object o)
        {
            if (o is Blast)
            {
                blasts.Remove((Blast)o);
            }
            else if (o is Bomb)
            {
                bombs.Remove((Bomb)o);
            }
            else if (o is Bonus)
            {
                bonuses.Remove((Bonus)o);
            }
            else if (o is PowerUp)
            {
                powerUps.Remove((PowerUp)o);
            }
            else if (o is SpawnPoint)
            {
                spawnPoints.Remove((SpawnPoint)o);
            }
            else if (o is Wall)
            {
                walls.Remove((Wall)o);
            }
            else
            {
                throw new InvalidOperationException("Unsupported object");
            }

            objects.Remove(o);

            // Remove the object from cache
            Point p = o.Position;
            cache[p.Y, p.X] = null;
        }

        /// <summary>
        /// Add a game creature to the map.
        /// </summary>
        public void Add(Creature c)
        {
            if (c is Bomberman)
            {
                bombermen.Add((Bomberman)c);
                sortedCreatures.Add(c);
            }
            else if (c is Monster)
            {
                monsters.Add((Monster)c);
                sortedCreatures.Add(c);
            }
            else
            {
                throw new InvalidOperationException("Unsupported creature");
            }
        }

        /// <summary>
        /// Remove a game creature from the map.
        /// </summary>
        public void Remove(Creature c)
        {
            if (c is Bomberman)
            {
                bombermen.Remove((Bomberman)c);
                sortedCreatures.Remove(c);
            }
            else if (c is Monster)
            {
                monsters.Remove((Monster)c);
                sortedCreatures.Remove(c);
            }
            else
            {
                throw new InvalidOperationException("Unsupported creature");
            }
        }

        /// <summary>
        /// Remove all objects and creatures from the map.
        /// Will also reset the unique ID generator.
        /// </summary>
        public void RemoveAll()
        {
            id = 0; // Can start back to unique ID 0.

            blasts.Clear();
            bombs.Clear();
            bonuses.Clear();
            powerUps.Clear();
            spawnPoints.Clear();
            walls.Clear();

            objects.Clear();

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    cache[y, x] = null;
                }
            }

            bombermen.Clear();
            monsters.Clear();

            sortedCreatures.Clear();
        }

        /// <summary>
        /// Check if the provided position is within map boundaries.
        /// </summary>
        public bool InBoundaries(Point p)
        {
            return p.X >= 0 && p.Y >= 0 && p.X < Width && p.Y < Height;
        }

        /// <summary>
        /// Ask the map if there is a game object at the specified position.
        /// O(1) operation.
        /// </summary>
        public Object GetObject(Point p)
        {
            return this.InBoundaries(p) ? cache[p.Y, p.X] : null;
        }

        /// <summary>
        /// Ask the map to update its sorted list of creatures.
        /// Should be typically called after moving creatures at the end of each update cycle.
        /// This operation is O(n.log(n)), with n being the number of creatures.
        /// </summary>
        public void SortCreatures()
        {
            sortedCreatures.Sort(creatureCompare);
        }

        /// <summary>
        /// Generate the next unique ID.
        /// </summary>
        public ushort GenerateID()
        {
            id++;
            return id;
        }

        /// <summary>
        /// Copy the provided map to the current map in a way that is suited for gameplay.
        /// In particular, spawn points and bombermen objects are not copied.
        /// Should be used to transfer storage maps to the gameplay map.
        /// </summary>
        public void Transfer(Map source)
        {
            if (source != null)
            {
                this.RemoveAll();

                // Do transfer

                id = source.id;

                Width = source.Width;
                Height = source.Height;
                Theme = source.Theme;

                cache = new Object[Height, Width];

                foreach (Object o in source.Objects)
                {
                    if (!(o is SpawnPoint))
                    {
                        this.Add(o);
                    }
                }
                foreach (Monster m in source.Monsters)
                {
                    this.Add(m);
                }
            }
            else
            {
                throw new ArgumentNullException("source");
            }
        }

        public Map(int width, int height, int theme)
        {
            if (width >= 0 && width <= MaxWidth && height >= 0 && height <= MaxHeight)
            {
                Width = width;
                Height = height;
                Theme = theme;

                blasts = new List<Blast>();
                bombs = new List<Bomb>();
                bonuses = new List<Bonus>();
                powerUps = new List<PowerUp>();
                spawnPoints = new List<SpawnPoint>();
                walls = new List<Wall>();

                objects = new List<Object>();
                
                cache = new Object[height, width];

                bombermen = new List<Bomberman>();
                monsters = new List<Monster>();

                sortedCreatures = new List<Creature>();
            }
            else
            {
                throw new ArgumentOutOfRangeException("width or height");
            }
        }
    }
}