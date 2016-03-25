using System.Collections.Generic;
using Pirates;
namespace MyBot
{
    public class MyBot : Pirates.IPirateBot
    {
        internal class BoardStatus
        {
            public Pirate Pirate { get; set; }
            public Treasure Treasure { get; set; }
            public List<Pirate> piratewithT { get; set; }
            public Pirate shooter { get; set; }
            public Pirate enemywith { get; set; }
            public Pirate enemywithoutT { get; set; }
            public Pirate defenderr { get; set; }
        }
        internal class PirateTactics
        {
            public Pirate Pirate { get; set; }
            public Location FinalDestination { get; set; }
            public Location TempDestination { get; set; }
            public int Moves { get; set; }
            public Pirate Target { get; set; }
            public Pirate eattaker { get; set; }
            public Pirate defender { get; set; }
        }
        public void DoTurn(IPirateGame game)
        {
            BoardStatus status = GetBoardStatus(game);
            List<PirateTactics> tactics = AssignTargets(game, status);
            TakeAction(game, tactics);
        }
        private BoardStatus GetBoardStatus(IPirateGame game)
        {
            int distance = 0;
            Pirate enemywithoutt = null;
            Pirate enemy = null;
            Pirate shooter = null;
            Pirate defenderrr = null;
            Pirate searcher = null;
            List<Pirate> piratewithT = game.MyPiratesWithTreasures();
            Treasure nearestTreasure = null;
            int min = 1000;
            int minn = 1000;
            if (game.MyPirates().Count < 2)
            {
                if (game.EnemyPiratesWithTreasures()[0].TurnsToSober > 0)
                {
                    searcher = game.MyPirates()[0];
                    foreach (Treasure treasure1 in game.Treasures())
                    {
                        nearestTreasure = treasure1;

                    }

                }
                else
                {
                    shooter = game.MyPirates()[0];
                    enemy = game.EnemyPirates()[0];
                }

                return new BoardStatus()

                {

                    shooter = shooter,

                    Pirate = searcher,

                    Treasure = nearestTreasure,

                    piratewithT = piratewithT,

                    enemywith = enemy
                  

                };
            }
            foreach (Pirate enmpirate in game.EnemyPiratesWithTreasures())
            {
                foreach (Pirate pirate in game.MyPiratesWithoutTreasures())
                {
                    if (pirate.TurnsToSober == 0 && pirate.ReloadTurns == 0)
                    {
                        distance = game.Distance(pirate, enmpirate);
                        if (distance < min)
                        {
                            enemy = enmpirate;
                            shooter = pirate;
                            min = distance;
                        }
                    }
                }
            }
            foreach (Pirate enmpiratea in game.EnemyPiratesWithoutTreasures())
            {
                foreach (Pirate piratewithtt in game.MyPiratesWithTreasures())
                {
                    if (piratewithtt.DefenseExpirationTurns == 0 && piratewithtt.DefenseReloadTurns == 0)
                    {
                        distance = game.Distance(piratewithtt, enmpiratea);
                        if (distance < min)
                        {
                            enemywithoutt = enmpiratea;
                            defenderrr = piratewithtt;
                            minn = distance;
                        }
                    }
                }
            }
            //}
            //else
            //{
            foreach (Pirate pirate in game.MyPiratesWithoutTreasures())
            {
                if (pirate.TurnsToSober == 0)
                {
                    foreach (Treasure treasure in game.Treasures())
                    {
                        distance = game.Distance(pirate.InitialLocation, treasure.Location);
                        if (distance < min)
                        {
                            searcher = pirate;
                            nearestTreasure = treasure;
                            min = distance;
                        }
                    }
                }
            }
            //  }
            return new BoardStatus()
            {
                shooter = shooter,
                Pirate = searcher,
                Treasure = nearestTreasure,
                defenderr = defenderrr,
                piratewithT = piratewithT,
                enemywith = enemy,
                enemywithoutT = enemywithoutt
            };
        }
        private List<PirateTactics> AssignTargets(IPirateGame game, BoardStatus status)
        {
            Location location1 = null;
            HashSet<Location> occupiedlocation = new HashSet<Location>();
            foreach (Pirate enpirate in game.EnemyDrunkPirates())
            {
                location1 = enpirate.Location;
                occupiedlocation.Add(location1);
            }
            foreach (Pirate pirate in game.MyDrunkPirates())
            {
                location1 = pirate.Location;
                occupiedlocation.Add(location1);
            }
            int remainingMoves = game.GetActionsPerTurn();
            List<PirateTactics> list = new List<PirateTactics>();
            if (status.shooter != null && remainingMoves > 0)
            {
                int j = 0;
                
                PirateTactics tactics2 = new PirateTactics() { Pirate = status.shooter };
                tactics2.Target = status.enemywith;
                tactics2.FinalDestination = status.enemywith.Location;
                tactics2.Moves = remainingMoves - 1; // keep one move for pirate with teasure
                remainingMoves -= tactics2.Moves;
                foreach(Pirate mypirate in game.AllMyPirates())
                    foreach (Pirate enemypirate in game.AllEnemyPirates())
                    {
                        if (enemypirate.Location == mypirate.InitialLocation)
                            tactics2.FinalDestination = mypirate.InitialLocation;
                    }
                List<Location> possibleLocations2 = game.GetSailOptions(tactics2.Pirate, tactics2.FinalDestination, tactics2.Moves);
                tactics2.TempDestination = tactics2.Pirate.Location;

                foreach (Location location3 in possibleLocations2)
                {
                    if (occupiedlocation.Contains(location3))
                        j++;
                    else
                    {
                        tactics2.TempDestination = location3;
                        occupiedlocation.Add(location3);
                        break;
                    }
                }
                list.Add(tactics2);


            }
            if (status.piratewithT != null)
            {
                int i = 0;
                foreach (Pirate pirate in status.piratewithT)
                {
                    if (remainingMoves <= 0)
                    {
                        break;
                    }
                    PirateTactics tactics1 = new PirateTactics() { Pirate = pirate };
                    if (tactics1.Pirate == status.defenderr)
                    {
                        tactics1.eattaker = status.enemywithoutT;
                    }
                    tactics1.FinalDestination = pirate.InitialLocation;
                    tactics1.Moves = 1;
                    remainingMoves -= tactics1.Moves;
                    List<Location> possibleLocations1 = game.GetSailOptions(tactics1.Pirate, tactics1.FinalDestination, tactics1.Moves);
                    tactics1.TempDestination = tactics1.Pirate.Location;
                    foreach (Location location in possibleLocations1)
                    {
                        if (occupiedlocation.Contains(location))
                            i++;
                        else
                        {
                            tactics1.TempDestination = location;
                            occupiedlocation.Add(location);
                            break;
                        }
                    }

                    list.Add(tactics1);
                }
            }
            if (status.Pirate != null && remainingMoves > 0)
            {
                PirateTactics tactics = new PirateTactics() { Pirate = status.Pirate };
                tactics.FinalDestination = status.Treasure.Location;
                tactics.Moves = remainingMoves;
                remainingMoves -= tactics.Moves;
                List<Location> possibleLocations = game.GetSailOptions(tactics.Pirate, tactics.FinalDestination, tactics.Moves);
                tactics.TempDestination = possibleLocations[0];
                list.Add(tactics);
            }
            return list;
        }
        private void TakeAction(IPirateGame game, List<PirateTactics> listTactis)
        {
            foreach (PirateTactics tactic in listTactis)
            {
                if (tactic.Target != null)
                {
                    if (game.InRange(tactic.Pirate, tactic.Target))
                    {
                        game.Attack(tactic.Pirate, tactic.Target);
                        continue;
                    }
                }
                if (tactic.Pirate != null)
                {
                    Pirate eattakerrr = null;
                    foreach (Pirate enpirateaa in game.EnemyPiratesWithoutTreasures())
                    {


                        if (game.InRange(tactic.Pirate, enpirateaa) && enpirateaa.TurnsToSober == 0 && enpirateaa.ReloadTurns == 0)
                        {
                            eattakerrr = enpirateaa;
                        }


                    }
                    if (eattakerrr != null && tactic.Pirate.DefenseReloadTurns == 0)
                    {
                        game.Defend(tactic.Pirate);
                        continue;
                    }
                    else
                        game.SetSail(tactic.Pirate, tactic.TempDestination);
                }



            }
        }
    }
}