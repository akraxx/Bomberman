using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Bomberman.Model;
using Bomberman.Network;

namespace Bomberman.Server
{
    /// <summary>
    /// Class that animates entities on the map until the round is finished.
    /// </summary>
    public sealed class ServerAnimator
    {
        private ServerController controller;

        private const int deathFrames = 25;
        private const int hitFrames = 25;
        private const float collisionThreshold = 0.5f;
        private static readonly TimeSpan minTimeLimit = TimeSpan.FromSeconds(60.9);

        private static readonly TimeSpan endTimerMax = TimeSpan.FromSeconds(3.0);
        private TimeSpan endTimer;

        private CreatureMover mover;
        private BlastMapper blastMapper;

        private List<Model.Object> spawnObjects = new List<Model.Object>();
        private List<Creature> spawnCreatures = new List<Creature>();
        private List<Model.Object> removeObjects = new List<Model.Object>();
        private List<Creature> removeCreatures = new List<Creature>();

        private Dictionary<Monster, ServerMonsterRoutine> routines = new Dictionary<Monster, ServerMonsterRoutine>();

        private Random random = new Random();

        private void _endRound()
        {
            Status status = controller.Game.Status;
            if (status.Mode == Status.Modes.Versus)
            {
                State = States.NextLevel;

                // Grant a win to whoever is still alive
                foreach (Player p in status.Players)
                {
                    Model.Bomberman b = p.Bomberman;
                    if (b != null && !b.Dead)
                    {
                        p.Wins++;
                        controller.SendPlayerState(p.ID);
                    }
                }
            }
            else
            {
                State = status.Players.Count(p => p.Playing && !p.GameOver) > 0 ? States.NextLevel : States.Wipeout;
            }
        }

        private void _checkEndCondition()
        {
            Map map = controller.Game.Map;
            Status status = controller.Game.Status;
            bool ended = false;
            if (status.Mode == Status.Modes.Versus)
            {
                ended = (map.Bombermen.Count(b => !b.Dead) <= 1);
            }
            else
            {
                ended = (status.Players.Count(p => p.Playing && !p.GameOver) == 0 || (map.Monsters.Count(m => !m.Dead) == 0));
            }
            if (ended)
            {
                endTimer = endTimerMax;

                status.TimeLimit = TimeSpan.FromSeconds(-1);
                controller.Send(new MessageEvent(MessageEvent.Types.SetTimeLimit, status.TimeLimit));
            }
        }

        private void _spawnObjects()
        {
            foreach (Model.Object o in spawnObjects)
            {
                controller.Game.Map.Add(o);
                controller.Send(new MessageEvent(MessageEvent.Types.MapEvent, new MapEvent(MapEvent.Actions.Spawn, o)));
            }
            spawnObjects.Clear();
        }

        private void _spawnCreatures()
        {
            foreach (Creature c in spawnCreatures)
            {
                controller.Game.Map.Add(c);
                controller.Send(new MessageEvent(MessageEvent.Types.MapEvent, new MapEvent(MapEvent.Actions.Spawn, c)));
            }
            spawnCreatures.Clear();
        }

        private void _cleanupObjects()
        {
            foreach (Model.Object o in removeObjects)
            {
                controller.RemoveObject(o);
            }
            removeObjects.Clear();
        }

        private void _cleanupCreatures()
        {
            foreach (Creature c in removeCreatures)
            {
                controller.RemoveCreature(c);
            }
            removeCreatures.Clear();
        }

        private void _playerGameOver(byte id)
        {
            Status status = controller.Game.Status;
            Player player = status.Players[id];
            player.Waiting = status.Continuable;
            player.GameOver = true;
            controller.SendPlayerState(player.ID);
        }

        private void _respawnPlayer(Player player)
        {
            if (player.Stock > 0)
            {
                player.Stock--;
                controller.SendPlayerState(player.ID);

                player.Bomberman = new Model.Bomberman(player.ID, controller.Game.Map, player.Bomberman.Position);
                spawnCreatures.Add(player.Bomberman);

                Status status = controller.Game.Status;
                if (status.TimeLimit >= TimeSpan.Zero && status.TimeLimit < minTimeLimit)
                {
                    status.TimeLimit = minTimeLimit;
                    controller.Send(new MessageEvent(MessageEvent.Types.SetTimeLimit, status.TimeLimit));
                }
            }
            else
            {
                _playerGameOver(player.ID);
            }
        }

        private void _spawnPlayers()
        {
            Model.Game game = controller.Game;
            Map map = game.Map;
            List<SpawnPoint> spawnPoints = new List<SpawnPoint>(game.Level.Map.SpawnPoints);

            foreach (Player p in game.Status.Players)
            {
                if (p.Playing && !p.GameOver)
                {
                    if (spawnPoints.Count > 0)
                    {
                        int index = random.Next(spawnPoints.Count);
                        Point spawnPoint = spawnPoints[index].Position;
                        spawnPoints.RemoveAt(index);

                        p.Waiting = false;
                        p.Bomberman = new Model.Bomberman(p.ID, map, new Vector2(spawnPoint.X, spawnPoint.Y));
                        controller.SendPlayerState(p.ID);
                        spawnCreatures.Add(p.Bomberman);
                    }
                    else
                    {
                        throw new InvalidOperationException("No more spawn points");
                    }
                }
            }
            _spawnCreatures();
        }

        private void _kaboom(Bomb initialBomb)
        {
            Model.Game game = controller.Game;
            Status status = game.Status;
            Map map = game.Map;
            TimeSpan expired = TimeSpan.FromTicks(-1);

            if (initialBomb != null)
            {
                initialBomb.Timer = expired;
                removeObjects.Add(initialBomb);
                blastMapper.Start(initialBomb);
                controller.Send(new MessageEvent(MessageEvent.Types.MapEvent, new MapEvent(MapEvent.Actions.Destroy, initialBomb)));
            }
            else
            {
                blastMapper.Nuke();
            }

            // Create the bomb chain-reaction, then apply effects on other objects
            bool ok = true;
            while (ok)
            {
                ok = false;
                foreach (Bomb bomb in map.Bombs)
                {
                    if (blastMapper.IsHit(bomb.Position) && bomb.Timer >= TimeSpan.Zero)
                    {
                        ok = true;

                        bomb.Timer = expired;
                        removeObjects.Add(bomb);
                        blastMapper.Add(bomb);
                    }
                }
            }
            foreach (PowerUp powerup in map.PowerUps)
            {
                if (blastMapper.IsHit(powerup.Position))
                {
                    removeObjects.Add(powerup);
                    controller.Send(new MessageEvent(MessageEvent.Types.MapEvent, new MapEvent(MapEvent.Actions.Destroy, powerup)));
                }
            }
            foreach (Bonus bonus in map.Bonuses)
            {
                if (blastMapper.IsHit(bonus.Position))
                {
                    removeObjects.Add(bonus);
                    controller.Send(new MessageEvent(MessageEvent.Types.MapEvent, new MapEvent(MapEvent.Actions.Destroy, bonus)));
                }
            }
            foreach (Wall wall in map.Walls)
            {
                if (blastMapper.IsHit(wall.Position) && wall.Destructible && !wall.Destroying)
                {
                    wall.Destroy(Wall.DefaultTimer);
                    controller.Send(new MessageEvent(MessageEvent.Types.MapEvent, new MapEvent(MapEvent.Actions.Destroy, wall)));
                }
            }
            _cleanupObjects();

            // Apply blasts
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    Point p = new Point(x, y);
                    if (blastMapper.IsHit(p))
                    {
                        if (map.GetObject(p) == null)
                        {
                            spawnObjects.Add(blastMapper.MakeBlast(Blast.DefaultTimer, p));
                        }
                    }
                }
            }
            _spawnObjects();

            blastMapper.Clear();
        }

        private void _damageCreature(Creature c, Blast cause, int amount)
        {
            c.Damage(amount);
            controller.SendCreatureState(c);

            if (c.Dead)
            {
                c.Velocity = Vector2.Zero;
                controller.SendCreaturePosition(c);

                c.SpriteState.Begin(Actions.Burn);
                controller.SendCreatureAnimation(c);

                if (c is Monster && cause != null)
                {
                    // Credit points to the killer
                    Player player = controller.Game.Status.Players.FirstOrDefault(p => p.Bomberman != null && p.Bomberman.ID == cause.Owner);
                    if (player != null)
                    {
                        player.Score += ((Monster)c).Type.Value;
                        controller.SendPlayerState(player.ID);
                    }
                }
            }
            else if (c is Monster)
            {
                c.Velocity = Vector2.Zero;
                controller.SendCreaturePosition(c);

                c.SpriteState.Begin(Actions.Hit);
                controller.SendCreatureAnimation(c);
            }
        }

        private void _checkPickup(Model.Bomberman b, Model.Object o)
        {
            if (o is Bonus || o is PowerUp)
            {
                Player player = controller.Game.Status.Players.FirstOrDefault(p => p.ID == b.Player);
                if (player != null)
                {
                    if (o is Bonus)
                    {
                        player.Score += ((Bonus)o).Value;
                        controller.SendPlayerState(player.ID);
                    }
                    else
                    {
                        PowerUp powerUp = (PowerUp)o;
                        if (powerUp.Type == PowerUp.Types.Bomb)
                        {
                            b.AddBomb();
                        }
                        else if (powerUp.Type == PowerUp.Types.FullBomb)
                        {
                            for (int i = 0; i < 3; i++) b.AddBomb();
                        }
                        else if (powerUp.Type == PowerUp.Types.Power)
                        {
                            b.AddPower();
                        }
                        else if (powerUp.Type == PowerUp.Types.FullPower)
                        {
                            for (int i = 0; i < 3; i++) b.AddPower();
                        }
                        else if (powerUp.Type == PowerUp.Types.RemoteBomb)
                        {
                            b.RefillBombType(Bomb.Types.Remote, 5);
                        }
                        else if (powerUp.Type == PowerUp.Types.SplitBomb)
                        {
                            b.RefillBombType(Bomb.Types.Split, 5);
                        }
                        else if (powerUp.Type == PowerUp.Types.Speed)
                        {
                            b.AddBoots();
                        }
                        else if (powerUp.Type == PowerUp.Types.FullSpeed)
                        {
                            for (int i = 0; i < 2; i++) b.AddBoots();
                        }
                        else if (powerUp.Type == PowerUp.Types.ExtraLife)
                        {
                            player.Stock++;
                        }
                        else if (powerUp.Type == PowerUp.Types.Invulnerability)
                        {
                            b.Invulnerability = TimeSpan.FromSeconds(10);
                        }
                        else if (powerUp.Type == PowerUp.Types.Weaken)
                        {
                            b.Nerf();
                        }
                    }
                    controller.SendPlayerState(player.ID);
                    controller.SendCreatureState(b);
                }
                removeObjects.Add(o);
                controller.Send(new MessageEvent(MessageEvent.Types.MapEvent, new MapEvent(MapEvent.Actions.Picked, o)));
            }
        }

        private void _checkHostileCollision(Model.Bomberman b)
        {
            Map map = controller.Game.Map;
            foreach (Monster m in map.Monsters)
            {
                if (!m.Dead && !b.Invulnerable)
                {
                    if ((b.Position - m.Position).Length() < collisionThreshold)
                    {
                        _damageCreature(b, null, 1);
                    }
                }
            }
        }

        private void _moveMonster(Monster m, TimeSpan elapsed)
        {
            if (m.SpriteState.Action == Actions.Hit)
            {
                if (m.SpriteState.Frame >= hitFrames)
                {
                    m.SpriteState.Begin(Actions.Idle);
                    controller.SendCreatureAnimation(m);
                }
            }
            else
            {
                if (routines.ContainsKey(m))
                {
                    ServerMonsterRoutine routine = routines[m];
                    routine.Execute(elapsed);
                }
                else
                {
                    ServerMonsterRoutine routine = Routines.ServerRoutineFactory.Build(controller, m);
                    if (routine != null)
                    {
                        routines.Add(m, routine);
                        routine.Execute(elapsed);
                    }
                }
            }
        }

        /// <summary>
        /// Possible states of the animator.
        /// </summary>
        public enum States
        {
            /// <summary>
            /// State of the animator while the level is playing.
            /// </summary>
            Playing,

            /// <summary>
            /// State of the animator when the server should start the next level.
            /// </summary>
            NextLevel,

            /// <summary>
            /// State of the animator when the server should show the continue or game over screen after all players have lost.
            /// </summary>
            Wipeout,
        };

        /// <summary>
        /// The current state of the animator.
        /// </summary>
        public States State { get; private set; }

        /// <summary>
        /// Begin the animation of the game round.
        /// </summary>
        public void Begin()
        {
            if (State != States.Playing)
            {
                State = States.Playing;

                endTimer = TimeSpan.Zero;

                routines.Clear();

                _spawnPlayers();
            }
        }

        /// <summary>
        /// Animate the game round.
        /// </summary>
        public void Animate(TimeSpan elapsed)
        {
            if (State == States.Playing)
            {
                Model.Game game = controller.Game;
                Status status = game.Status;
                Map map = game.Map;
                bool allowCriticalActions = (endTimer == TimeSpan.Zero);

                // Update timed objects
                foreach (Model.Object o in map.Objects)
                {
                    if (o is TimedObject)
                    {
                        TimedObject obj = (TimedObject)o;
                        if (obj.Timer > TimeSpan.Zero)
                        {
                            obj.Timer -= elapsed;
                        }
                        else
                        {
                            obj.Timer = TimeSpan.Zero;
                            if (obj is Bonus || obj is PowerUp || obj is Blast)
                            {
                                removeObjects.Add(obj);
                            }
                            else if (obj is Wall)
                            {
                                Wall wall = (Wall)obj;
                                if (wall.Destroying)
                                {
                                    removeObjects.Add(wall);
                                    if (wall.HiddenItem != null) spawnObjects.Add(wall.HiddenItem);
                                }
                            }
                        }
                    }
                }
                _cleanupObjects();
                _spawnObjects();

                if (allowCriticalActions)
                {
                    // Time limit
                    if (status.TimeLimit > TimeSpan.Zero)
                    {
                        status.TimeLimit -= elapsed;

                        if (status.TimeLimit <= TimeSpan.Zero)
                        {
                            // TIME UP!
                            status.TimeLimit = TimeSpan.Zero;
                            controller.Send(new MessageEvent(MessageEvent.Types.TimeUp, null));

                            // Remove all active invulnerability from all bomberman
                            foreach (Model.Bomberman b in map.Bombermen)
                            {
                                if (b.Invulnerable)
                                {
                                    b.Invulnerability = TimeSpan.Zero;
                                    controller.SendCreatureState(b);
                                }
                            }

                            // Nuke the whole map
                            _kaboom(null);
                        }
                    }

                    // Check bomb destruction
                    Bomb bomb;
                    while ((bomb = map.Bombs.FirstOrDefault(b => b.Timer == TimeSpan.Zero)) != null)
                    {
                        _kaboom(bomb);
                    }
                }

                // Update creatures
                foreach (Creature c in map.SortedCreatures)
                {
                    c.SpriteState.Tick();

                    c.Invulnerability -= elapsed;
                    if (c.Invulnerability < TimeSpan.Zero) c.Invulnerability = TimeSpan.Zero;

                    if (c.Dead)
                    {
                        if (c.SpriteState.Frame >= deathFrames)
                        {
                            removeCreatures.Add(c);

                            // Check for a respawn
                            Player player = status.Players.FirstOrDefault(p => p.Bomberman == c);
                            if (player != null && status.Mode == Status.Modes.Cooperation)
                            {
                                _respawnPlayer(player);
                            }
                        }
                    }
                    else
                    {
                        Point p = Tools.Vector2Point(CreatureMover.AlignPosition(c.Position, true));
                        Model.Object o = map.GetObject(p);

                        if (c is Model.Bomberman)
                        {
                            _checkPickup((Model.Bomberman)c, o);

                            if (allowCriticalActions)
                            {
                                _checkHostileCollision((Model.Bomberman)c);
                            }
                        }
                        else if (c is Monster)
                        {
                            _moveMonster((Monster)c, elapsed);
                        }

                        if (o is Blast && !c.Invulnerable && allowCriticalActions)
                        {
                            _damageCreature(c, (Blast)o, 1);
                        }
                    }
                }
                _cleanupObjects();
                _cleanupCreatures();
                _spawnCreatures();

                if (endTimer > TimeSpan.Zero)
                {
                    // Delay before stopping the animator
                    endTimer -= elapsed;
                    if (endTimer <= TimeSpan.Zero)
                    {
                        endTimer = TimeSpan.Zero;
                        _endRound();
                    }
                }
                else
                {
                    // Check end condition
                    _checkEndCondition();
                }
            }
            else
            {
                throw new InvalidOperationException("Not in 'Playing' state");
            }
        }

        /// <summary>
        /// Should be called when a player wants to move his bomberman.
        /// </summary>
        public void MovePlayer(Player player, PositionPayload positionPayload)
        {
            if (State == States.Playing && player != null)
            {
                Model.Bomberman bomberman = player.Bomberman;
                if (bomberman != null && !bomberman.Dead)
                {
                    Vector2 velocity = positionPayload.DecodeVelocity();
                    if (velocity != Vector2.Zero)
                    {
                        velocity.Normalize();
                        velocity *= bomberman.Speed;
                    }
                    bomberman.Velocity = velocity;

                    mover.SetActive(bomberman);
                    mover.MoveTo(positionPayload.DecodePosition());
                    if (mover.Legal)
                    {
                        controller.SendCreaturePosition(bomberman);
                    }
                }
            }
        }

        /// <summary>
        /// Should be called when a player wants to place a bomb.
        /// </summary>
        public void PlaceBomb(Player player, Bomb.Types type)
        {
            if (State == States.Playing && player != null)
            {
                Model.Bomberman bomberman = player.Bomberman;
                if (bomberman != null && !bomberman.Dead)
                {
                    mover.SetActive(bomberman);
                    Vector2 align = CreatureMover.AlignPosition(bomberman.Position, true);
                    Point point = Tools.Vector2Point(align);
                    Map map = controller.Game.Map;
                    if (map.GetObject(point) == null && bomberman.GetBombType(type) > 0)
                    {
                        int count = map.Bombs.Count(b => b.Owner == bomberman.ID);
                        if (count < bomberman.Bombs)
                        {
                            TimeSpan timer = (type == Bomb.Types.Remote ? TimeSpan.FromSeconds(300) : Bomb.DefaultTimer);

                            spawnObjects.Add(new Bomb(type, bomberman.ID, bomberman.Power, timer, point));
                            _spawnObjects();

                            bomberman.UseBombType(type);
                            controller.SendCreatureState(bomberman);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Should be called when a player wants to detonate a bomb.
        /// </summary>
        public void DetonateBomb(Player player, ushort id)
        {
            if (State == States.Playing && player != null)
            {
                Model.Bomberman bomberman = player.Bomberman;
                if (bomberman != null && !bomberman.Dead)
                {
                    Map map = controller.Game.Map;
                    Bomb bomb = map.Bombs.FirstOrDefault(b => b.ID == id);
                    if (bomb.Owner == bomberman.ID)
                    {
                        bomb.Timer = TimeSpan.Zero;
                    }
                }
            }
        }

        public ServerAnimator(ServerController controller)
        {
            if (controller != null)
            {
                this.controller = controller;

                mover = new CreatureMover(controller.Game.Map);
                blastMapper = new BlastMapper(controller.Game.Map);

                State = States.NextLevel;
            }
            else
            {
                throw new ArgumentNullException("controller");
            }
        }
    }
}