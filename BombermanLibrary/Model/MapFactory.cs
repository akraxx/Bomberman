using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Bomberman.Model
{
    public static class MapFactory
    {
        private static Random rand = new Random();

        public static Map Build(Status.Modes mode, int level)
        {
            if (mode == Status.Modes.Cooperation)
            {
                switch (level)
                {
                    case 1:
                        return buildFirstMap();
                    case 2:
                        return buildSecondMap();
                    case 3:
                        return buildThirdMap();
                    default:
                        return null;
                }
            }
            else
            {
                switch (level)
                {
                    case 1:
                        return buildFirstPvpMap();
                    case 2:
                        return buildSecondPvpMap();
                    default:
                        return null;
                }
            }
        }

        private static Object rollItem(double bonusChance, double powerUpChance, Point position)
        {
            double roll = rand.NextDouble();
            double bonusThreshold = bonusChance;
            double powerUpThreshold = bonusThreshold + powerUpChance;
            if (roll < bonusThreshold)
            {
                return new Bonus(Bonus.GetRandomSprite(rand), Bonus.DefaultTimer, position);
            }
            else if (roll < powerUpThreshold)
            {
                return new PowerUp(PowerUp.GetRandomType(rand), PowerUp.DefaultTimer, position);
            }
            else
            {
                return null;
            }
        }

        private static void placeCornerSpawnPoints(Map map, int distanceFromEdge)
        {
            map.Add(new SpawnPoint(new Point(distanceFromEdge, distanceFromEdge)));
            map.Add(new SpawnPoint(new Point(16 - distanceFromEdge, distanceFromEdge)));
            map.Add(new SpawnPoint(new Point(distanceFromEdge, 10 - distanceFromEdge)));
            map.Add(new SpawnPoint(new Point(16 - distanceFromEdge, 10 - distanceFromEdge)));
        }

        private static void buildRegularGrid(Map map, Point start, Point end, bool destroyable)
        {
            for (int x = start.X; x <= end.X; x += 2)
            {
                for (int y = start.Y; y <= end.Y; y += 2)
                {
                    Point p = new Point(x, y);
                    map.Add(new Wall(destroyable, destroyable ? rollItem(0.2, 0.4, p) : null, p));
                }
            }
        }

        private static void buildQuarter(Map map, int x, int y, int coefX, int coefY)
        {
            map.Add(new SpawnPoint(new Point(x + coefX * 1, y)));

            map.Add(new Wall(false, null, new Point(x + coefX * 2, y)));
            map.Add(new Wall(false, null, new Point(x + coefX * 1, y + coefY * 1)));
            map.Add(new Wall(false, null, new Point(x + coefX * 5, y + coefY * 4)));

            map.Add(new Wall(false, null, new Point(x + coefX * 1, y + coefY * 3)));
            map.Add(new Wall(false, null, new Point(x, y + coefY * 4)));
            map.Add(new Wall(false, null, new Point(x + coefX * 1, y + coefY * 4)));

            map.Add(new Wall(false, null, new Point(x + coefX * 3, y + coefY * 2)));
            map.Add(new Wall(false, null, new Point(x + coefX * 3, y + coefY * 3)));
            map.Add(new Wall(false, null, new Point(x + coefX * 4, y + coefY * 3)));

            map.Add(new Wall(false, null, new Point(x + coefX * 5, y + coefY * 1)));
            map.Add(new Wall(false, null, new Point(x + coefX * 6, y + coefY * 1)));
            map.Add(new Wall(false, null, new Point(x + coefX * 7, y + coefY * 2)));

            map.Add(new Wall(true, null, new Point(x + coefX * 1, y + coefY * 2)));
            map.Add(new Wall(true, null, new Point(x + coefX * 2, y + coefY * 3)));
            map.Add(new Wall(true, null, new Point(x + coefX * 4, y)));

            map.Add(new Bonus(Bonus.GetRandomSprite(rand), Bonus.DefaultTimer, new Point(x + coefX * 3, y + coefY * 4)));
            map.Add(new Bonus(Bonus.GetRandomSprite(rand), Bonus.DefaultTimer, new Point(x + coefX * 4, y + coefY * 4)));
            map.Add(new PowerUp(PowerUp.Types.FullSpeed, PowerUp.DefaultTimer, new Point(x + coefX * 7, y + coefY * 1)));
        }

        private static Map buildFirstMap()
        {
            Map map = new Map(17, 11, 0);

            buildRegularGrid(map, new Point(1, 1), new Point(15, 10), false);

            for (int x = 6; x < 11; x += 2)
            {
                int yMin = 3;
                int yMax = 7;
                map.Add(new Wall(true, rollItem(0.4, 0.2, new Point(x, yMin)), new Point(x, yMin)));
                map.Add(new Wall(true, rollItem(0.4, 0.2, new Point(x, yMax)), new Point(x, yMax)));
            }
            for (int y = 4; y < 7; y += 2)
            {
                int xMin = 5;
                int xMax = 11;
                map.Add(new Wall(true, rollItem(0.4, 0.2, new Point(xMin, y)), new Point(xMin, y)));
                map.Add(new Wall(true, rollItem(0.4, 0.2, new Point(xMax, y)), new Point(xMax, y)));
            }

            map.Add(new PowerUp(PowerUp.Types.RemoteBomb, PowerUp.DefaultTimer, new Point(8, 5)));

            map.Add(new Monster(MonsterType.Zombie, map, new Vector2(6, 4)));
            map.Add(new Monster(MonsterType.Zombie, map, new Vector2(10, 4)));
            map.Add(new Monster(MonsterType.Zombie, map, new Vector2(6, 6)));
            map.Add(new Monster(MonsterType.Zombie, map, new Vector2(10, 6)));

            placeCornerSpawnPoints(map, 0);

            return map;
        }

        private static Map buildSecondMap()
        {
            Map map = new Map(17, 11, 0);

            int[] wallsX = { 1, 3, 5, 7, 9, 11, 13, 15, 0, 2, 4, 6, 8, 10, 12, 14, 0, 2, 4, 6, 8, 10, 12, 14, 16, 2, 3, 4, 6, 7, 8, 9, 10, 11, 13, 14, 15, 1, 3, 5, 7, 9, 11, 13, 15, 1, 3, 5, 7, 9, 11, 13, 15, 0, 2, 4, 6, 8, 10, 12, 14, 16 };
            int[] wallsY = { 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 4, 4, 4, 4, 4, 4, 4, 4, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 6, 6, 6, 6, 6, 6, 6, 6, 8, 8, 8, 8, 8, 8, 8, 8, 9, 9, 9, 9, 9, 9, 9, 9, 10 };

            for (int i = 0; i < wallsX.Length && i < wallsY.Length; i++)
            {
                map.Add(new Wall(false, null, new Point(wallsX[i], wallsY[i])));
            }

            map.Add(new Wall(true, rollItem(0.4, 0.2, new Point(8, 1)), new Point(8, 1)));
            map.Add(new Wall(true, rollItem(0.4, 0.2, new Point(10, 1)), new Point(10, 1)));
            map.Add(new Wall(true, rollItem(0.4, 0.2, new Point(12, 1)), new Point(12, 1)));
            map.Add(new Wall(true, rollItem(0.4, 0.2, new Point(14, 1)), new Point(14, 1)));

            map.Add(new Wall(true, null, new Point(3, 2)));
            map.Add(new Wall(true, null, new Point(2, 3)));
            map.Add(new Wall(true, null, new Point(3, 4)));

            map.Add(new Wall(true, null, new Point(9, 2)));
            map.Add(new Wall(true, null, new Point(8, 3)));
            map.Add(new Wall(true, null, new Point(9, 4)));

            map.Add(new Wall(true, null, new Point(8, 6)));
            map.Add(new Wall(true, null, new Point(9, 7)));
            map.Add(new Wall(true, null, new Point(8, 8)));

            map.Add(new Wall(true, null, new Point(14, 6)));
            map.Add(new Wall(true, null, new Point(15, 7)));
            map.Add(new Wall(true, null, new Point(14, 8)));

            map.Add(new PowerUp(PowerUp.Types.Speed, PowerUp.DefaultTimer.Add(PowerUp.DefaultTimer), new Point(5, 5)));
            map.Add(new PowerUp(PowerUp.Types.Speed, PowerUp.DefaultTimer.Add(PowerUp.DefaultTimer), new Point(12, 5)));

            map.Add(new Monster(MonsterType.Mummy, map, new Vector2(15, 2)));
            map.Add(new Monster(MonsterType.Mummy, map, new Vector2(3, 3)));
            map.Add(new Monster(MonsterType.Mummy, map, new Vector2(14, 7)));

            map.Add(new Monster(MonsterType.Zombie, map, new Vector2(2, 6)));
            map.Add(new Monster(MonsterType.Zombie, map, new Vector2(4, 8)));
            map.Add(new Monster(MonsterType.Zombie, map, new Vector2(6, 6)));

            map.Add(new SpawnPoint(new Point(0, 0)));
            map.Add(new SpawnPoint(new Point(0, 1)));
            map.Add(new SpawnPoint(new Point(0, 10)));
            map.Add(new SpawnPoint(new Point(1, 9)));

            return map;
        }

        private static Map buildThirdMap()
        {
            Map map = new Map(17, 11, 0);

            buildRegularGrid(map, new Point(1, 0), new Point(3, 10), false);
            buildRegularGrid(map, new Point(5, 1), new Point(7, 9), false);
            buildRegularGrid(map, new Point(9, 0), new Point(11, 10), false);
            buildRegularGrid(map, new Point(13, 1), new Point(15, 9), false);

            map.Add(new Monster(MonsterType.Chicken, map, new Vector2(8, 5)));

            placeCornerSpawnPoints(map, 0);

            return map;
        }

        private static Map buildFirstPvpMap()
        {
            Map map = new Map(17, 11, 0);

            for (int x = 1; x < 17; x += 2)
            {
                for (int y = 1; y < 11; y += 2)
                {
                    map.Add(new Wall(false, null, new Point(x, y)));
                }
            }

            for (int x = 0; x < 17; x += 2)
            {
                for (int y = 1; y < 11; y += 2)
                {
                    if ((x == 0 && y == 1) || (x == 16 && y == 1) || (x == 0 && y == 9) || (x == 16 && y == 9)) { }
                    else
                    {
                        map.Add(new Wall(true, null, new Point(x, y)));
                    }
                }
            }

            for (int x = 1; x < 17; x += 2)
            {
                for (int y = 0; y < 11; y += 2)
                {
                    if ((x == 1 && y == 0) || (x == 1 && y == 10) || (x == 15 && y == 0) || (x == 15 && y == 10)) { }
                    else
                    {
                        map.Add(new Wall(true, null, new Point(x, y)));
                    }
                }
            }

            for (int x = 2; x <= 14; x += 2)
            {
                for (int y = 2; y <= 8; y += 2)
                {
                    map.Add(new PowerUp(PowerUp.GetRandomType(rand), PowerUp.DefaultTimer.Add(PowerUp.DefaultTimer.Add(PowerUp.DefaultTimer)), new Point(x, y)));
                }
            }

            map.Add(new SpawnPoint(new Point(0, 0)));
            map.Add(new SpawnPoint(new Point(16, 0)));
            map.Add(new SpawnPoint(new Point(0, 10)));
            map.Add(new SpawnPoint(new Point(16, 10)));

            return map;
        }

        private static Map buildSecondPvpMap()
        {
            Map map = new Map(17, 11, 0);

            buildQuarter(map, 0, 0, 1, 1);
            buildQuarter(map, 16, 0, -1, 1);
            buildQuarter(map, 0, 10, 1, -1);
            buildQuarter(map, 16, 10, -1, -1);

            for (int x = 0; x < 17; x++)
            {
                if (x < 6 || x > 10)
                {
                    map.Add(new Wall(false, null, new Point(x, 5)));
                }
            }
            for (int y = 0; y < 11; y++)
            {
                if (y < 3 || y > 7)
                {
                    map.Add(new Wall(false, null, new Point(8, y)));
                }
            }

            map.Add(new PowerUp(PowerUp.Types.Invulnerability, PowerUp.ShortTimer, new Point(8, 5)));

            return map;
        }
    }
}
