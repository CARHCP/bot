using System.Collections.Generic;
using System;
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
            public Pirate defenderr { get; set; }
            public Pirate pirateToDefend { get; set; }

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

        List<List<Location>> enemyPirateHistoryLocations = new List<List<Location>>();

        public void DoTurn(IPirateGame game)
        {

            BoardStatus status = GetBoardStatus(game);
            List<PirateTactics> tactics = AssignTargets(game, status);
            TakeAction(game, tactics);
            SavePirateLocation(game);
        }

        private Location GetPreviousLocation(Pirate pirate)
        {
            List<Location> list = enemyPirateHistoryLocations[enemyPirateHistoryLocations.Count - 1];
            return list[pirate.Id];
        }

        private void SavePirateLocation(IPirateGame game)
        {
            // add enemymoves
            List<Location> list = new List<Location>();
            foreach (Pirate enemyPirate in game.AllEnemyPirates())
            {
                list.Add(enemyPirate.Location);
            }
            enemyPirateHistoryLocations.Add(list);
        }
        private BoardStatus GetBoardStatus(IPirateGame game)
        {
            int distance = 0;
            Pirate pirateToDefend = null;
            Pirate enemy = null;
            Pirate shooter = null;
            Pirate defenderrr = null;

            Pirate searcher = null;
            List<Pirate> piratewithT = new List<Pirate>();

            Treasure nearestTreasure = null;
            int min = 1000;

            
            //prevent crash when enemy pirat stand on initaillocatoin
            foreach (Pirate pirate in game.MyPiratesWithTreasures())
            {
                bool move = true;
                int distance1 = game.Distance(pirate, pirate.InitialLocation);
                if (distance1 == 1)
                {
                    if (game.IsOccupied(pirate.InitialLocation))
                    {
                        move = false;
                    }

                    foreach (Pirate enemyPirate in game.EnemyPiratesWithoutTreasures())
                    {
                        if (enemyPirate.IsLost || enemyPirate.TurnsToSober > 0)
                        {
                            continue;
                        }
                        if (game.Distance(enemyPirate, pirate.InitialLocation) <= game.GetActionsPerTurn())
                        {
                            move = false;
                        }
                    }
                }
                if (move)
                {
                    game.Debug("pirateT. " + pirate.Id);
                    
                    piratewithT.Add(pirate);
                }
            }


            foreach (Pirate enmpirate in game.EnemyPiratesWithTreasures())
            {

                foreach (Pirate pirate in game.MyPiratesWithoutTreasures())
                {

                    if (pirate.TurnsToSober == 0 && !pirate.IsLost)
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

            if (shooter == null)
            {

                // find threat on my with treasure with teasure 
                min = 1000;
                Pirate myShipWithT = null;
                foreach (Pirate pirate in game.MyPiratesWithTreasures())
                {

                    distance = game.Distance(pirate, pirate.InitialLocation);
                    if (distance < min)
                    {
                        myShipWithT = pirate;
                        min = distance;
                    }


                }

                if (myShipWithT != null)
                {
                    min = 1000;
                    Pirate defender = null;
                    foreach (Pirate pirate in game.MyPiratesWithoutTreasures())
                    {
                        if (pirate.TurnsToSober == 0 && !pirate.IsLost && pirate != shooter)
                        {
                            distance = game.Distance(pirate, myShipWithT);
                            if (distance < min)
                            {
                                min = distance;
                                defender = pirate;
                            }
                        }
                    }

                    if (defender != null)
                    {
                        defenderrr = defender;
                        pirateToDefend = myShipWithT;
                    }
                }
            }


            //}
            //else
            //{
            min = 1000;
            foreach (Pirate pirate in game.MyPiratesWithoutTreasures())
            {
                if (pirate.TurnsToSober == 0 && pirate != shooter && pirate != defenderrr && !pirate.IsLost)
                {
                    int maxTreasureValue = Bigtreasure(game);
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

            if (shooter != null)
            {
                game.Debug("shooter " + shooter.Id);
                game.Debug("shooter attack" + enemy.Id);
            }
            if (searcher != null)
            {
                game.Debug("searcher " + searcher.Id);
            }
            if (defenderrr != null)
            {
                game.Debug("defeder " + defenderrr.Id);
            }
            return new BoardStatus()
            {
                shooter = shooter,
                Pirate = searcher,
                Treasure = nearestTreasure,
                defenderr = defenderrr,
                piratewithT = piratewithT,
                enemywith = enemy,
                pirateToDefend = pirateToDefend

            };
        }

        private int Bigtreasure(IPirateGame game)
        {
            int max = 1;

            foreach (Treasure treasure in game.Treasures())
            {

                if (treasure.Value > max)
                {
                    max = treasure.Value;

                }
            }
            return max;

        }
        private bool isPirateInTactisList(List<PirateTactics> list, Pirate pirate)
        {
            foreach (PirateTactics tactics in list)
            {
                if (pirate == tactics.Pirate)
                    return true;
            }
            return false;
        }
        private List<PirateTactics> AssignTargets(IPirateGame game, BoardStatus status)
        {
            List<PirateTactics> list = new List<PirateTactics>();

            list.AddRange(defendfromthreat(game, status));


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

            if (status.piratewithT != null)
            {
                foreach (Pirate pirate in status.piratewithT)
                {
                    if (remainingMoves <= 0)
                    {
                        break;
                    }

                    if (isPirateInTactisList(list, pirate))
                    {
                        continue;
                    }


                    PirateTactics tactics1 = new PirateTactics() { Pirate = pirate };

                    tactics1.FinalDestination = pirate.InitialLocation;
              
                    setTempLocation(game, occupiedlocation, tactics1, ref remainingMoves);
                    list.Add(tactics1);
                }
            }

            if (status.shooter != null && remainingMoves > 0 && !isPirateInTactisList(list, status.shooter))
            {
                int j = 0;
                PirateTactics tactics2 = new PirateTactics() { Pirate = status.shooter };
                if (game.InRange(tactics2.Pirate, status.enemywith))
                {
                    game.Debug("shooter " + status.enemywith.DefenseExpirationTurns + "," + status.enemywith.DefenseReloadTurns + "," + status.shooter.ReloadTurns + "}");
                    if (status.shooter.ReloadTurns > 0) { 
                    
                        game.Debug("shooter craching with " + status.enemywith.Id);
                        crachWithEnemyHasT(game, occupiedlocation, status.enemywith, tactics2, ref remainingMoves);

                    }
                    else
                    {
                        tactics2.Target = status.enemywith;

                    }
                }
                else {

                    tactics2.FinalDestination = status.enemywith.Location;

                    setTempLocation(game, occupiedlocation, tactics2, ref remainingMoves);

                }
                list.Add(tactics2);
            }

            if (status.defenderr != null && remainingMoves > 0 && !isPirateInTactisList(list, status.defenderr))
            {
                


                Pirate enemyToAttach = null;

                if (game.IsOccupied(status.pirateToDefend.InitialLocation))
                {
                    enemyToAttach = game.GetPirateOn(status.pirateToDefend.InitialLocation);
                }

                if (enemyToAttach == null)
                {
                    int min = 1000;
                    foreach (Pirate enemy in game.EnemyPiratesWithoutTreasures())
                    {
                        if (enemy.IsLost || enemy.TurnsToSober>0)                        {
                            continue;
                        }
                        //enemy.AttackRadius && enemy.ReloadTurns == 0 && enemy.TurnsToSober == 0
                        if (game.Distance(status.pirateToDefend, enemy) < min)
                        {
                           
                                enemyToAttach = enemy;
                                min = game.Distance(enemy, status.pirateToDefend);
                        }
                    }
                }

                PirateTactics tactics2 = new PirateTactics() { Pirate = status.defenderr };
                if (enemyToAttach != null)
                {

                    game.Debug("enemy to attack " + enemyToAttach.Id);

                    if (game.InRange(status.defenderr, enemyToAttach))
                    {
                        game.Debug("defnder{" + enemyToAttach.DefenseExpirationTurns + "," + enemyToAttach.DefenseReloadTurns + "," + status.defenderr.ReloadTurns + "}");
                        if (enemyToAttach.DefenseExpirationTurns > 0 || enemyToAttach.DefenseReloadTurns > 0 || status.defenderr.ReloadTurns > 0)
                        {
                            game.Debug("defender craching with" + enemyToAttach.Id);
                            crachWithEnemyAttackT(game, occupiedlocation, enemyToAttach, status, tactics2, ref remainingMoves);
                        }
                        else
                        {
                            tactics2.Target = enemyToAttach;

                        }
                    }

                    else {
                        List<Location> enemyToAttachPossibleLocation = game.GetSailOptions(enemyToAttach, status.pirateToDefend.Location, game.GetActionsPerTurn());
                        foreach (Location location in enemyToAttachPossibleLocation)
                        {
                            if (location.Equals(status.pirateToDefend.Location))
                            {
                                continue;
                            }
                            else
                            {
                                tactics2.FinalDestination = location;
                                break;
                            }
                        }
                        if (tactics2.FinalDestination == null)
                        {
                            tactics2.FinalDestination = enemyToAttach.Location;
                        }
                        game.Debug("defender. FinalDestination" + tactics2.FinalDestination);
                        setTempLocation(game, occupiedlocation, tactics2, ref remainingMoves);

                    }
                    list.Add(tactics2);

                }
                else
                {
                    if (game.Distance(status.defenderr, status.pirateToDefend.InitialLocation) < game.Distance(status.pirateToDefend, status.pirateToDefend.InitialLocation))
                    {
                        tactics2.FinalDestination = status.pirateToDefend.Location;
                        setTempLocation(game, occupiedlocation, tactics2, ref remainingMoves);
                        list.Add(tactics2);
                    }
                }


            }


            // isPirateInTactisList(list, status.piratewithT);



            if (status.Pirate != null && remainingMoves > 0 && !isPirateInTactisList(list, status.Pirate))
            {
                PirateTactics tactics = new PirateTactics() { Pirate = status.Pirate };
                game.Debug("searcher is " + status.Pirate.Id);


                tactics.FinalDestination = status.Treasure.Location;

                setTempLocation(game, occupiedlocation, tactics, ref remainingMoves);

                list.Add(tactics);



            }
            return list;
        }
        private bool isLocationHasTreause(IPirateGame game, Location location)
        {
            foreach (Treasure treasure in game.Treasures())
            {
                if (treasure.Location.Equals(location))
                    return true;
            }
            return false;

        }

        private void setTempLocation(IPirateGame game, HashSet<Location> occupiedlocation, PirateTactics tactics, ref int remainingMoves)
        {
            
            int maxMoves = remainingMoves;
            if (tactics.Pirate.HasTreasure)
            {
                maxMoves = 1;
            }
           

            List<Location> possibleLocations = game.GetSailOptions(tactics.Pirate, tactics.FinalDestination, maxMoves);
            Location tempDestination = null;
            List<Location> safePossibleLocation = new List<Location>();
            while (tempDestination == null && maxMoves > 0)
            {
                foreach (Location location in possibleLocations)
                {
                    if (!location.Equals(tactics.FinalDestination) && isLocationHasTreause(game, location) )
                    {
                        continue;
                    }
                    if (occupiedlocation.Contains(location))
                    {
                        continue;
                    }
                    else
                    {
                        safePossibleLocation.Add(location);
                       
                    }
                }


                if (safePossibleLocation.Count > 0)
                {
                    Random rnd = new Random();
                    int index = rnd.Next(safePossibleLocation.Count);

                    tempDestination = safePossibleLocation[index];

                    game.Debug("tempDist." + tactics.Pirate.Id + "=" + tempDestination);

                    break;
                }

                if (tempDestination == null)
                {
                    maxMoves--;
                }
                
                
                
            }

            if (tempDestination != null)
            {
                
                tactics.TempDestination = tempDestination;
                tactics.Moves = game.Distance(tactics.Pirate, tempDestination);
                remainingMoves -= tactics.Moves;
                occupiedlocation.Add(tempDestination);
            }

        }


        private Pirate getThreat(IPirateGame game, Pirate mypirate)
        {
            Pirate threat = null;
            foreach (Pirate enpirateaa in game.EnemyPiratesWithoutTreasures())
            {
                if (game.InRange(mypirate, enpirateaa) && enpirateaa.TurnsToSober == 0)
                {
                    threat = enpirateaa;
                }


            }
            return threat;
        }

        void crachWithEnemyHasT(IPirateGame game, HashSet<Location> occupiedlocation, Pirate enemyPirateWithTreasure, PirateTactics tactics, ref int remainingMoves)
        {
            game.Debug("enemyT." + enemyPirateWithTreasure.Id + " " + GetPreviousLocation(enemyPirateWithTreasure));
            if (GetPreviousLocation(enemyPirateWithTreasure).Equals(enemyPirateWithTreasure.Location)) // no moves
            {
                tactics.FinalDestination = enemyPirateWithTreasure.Location;
                

            }
            else
            {
                List<Location> enemyToAttachPossibleLocation = game.GetSailOptions(enemyPirateWithTreasure, enemyPirateWithTreasure.InitialLocation, 1);
                tactics.FinalDestination = enemyToAttachPossibleLocation[0];
            }

            setTempLocation(game, occupiedlocation, tactics, ref remainingMoves);

            
        }

        

        void crachWithEnemyAttackT(IPirateGame game, HashSet<Location> occupiedlocation, Pirate enemyToAttack, BoardStatus status, PirateTactics tactics, ref int remainingMoves)
        {

            // find where my shi with t go
            List<Location> myPirateWithTPossibleLocation = game.GetSailOptions(status.pirateToDefend, status.pirateToDefend.InitialLocation, 1);

            foreach (Location location in myPirateWithTPossibleLocation)
            {
                Location myPirateWithT = myPirateWithTPossibleLocation[0];
                game.Debug("pirateToDefend.distinatio=" + myPirateWithT);

                List<Location> enemyToAttachPossibleLocation = game.GetSailOptions(enemyToAttack, myPirateWithT, game.GetActionsPerTurn());
                foreach (Location location1 in enemyToAttachPossibleLocation)
                {
                    if (!occupiedlocation.Contains(location1))
                    {
                        tactics.FinalDestination = location1;
                        break;
                    }
                }
                if (tactics.FinalDestination != null)
                {
                    break;
                }
            }
            if (tactics.FinalDestination == null)
            {
                tactics.FinalDestination = status.pirateToDefend.InitialLocation;
            }
            
            game.Debug("enemyToAttac.finaldistinatio=" + tactics.FinalDestination);

            setTempLocation(game, occupiedlocation, tactics, ref remainingMoves);

            

           // game.Debug("crachWithEnemyAttackT.TempDestination=" + tactics.TempDestination);


        }

        private List<PirateTactics> defendfromthreat(IPirateGame game, BoardStatus status)
        {
            List<PirateTactics> list = new List<PirateTactics>();
            foreach (Pirate pirate in game.MySoberPirates())
            {

                // shooter should not do defence
                if (pirate == status.shooter || pirate==status.defenderr)
                {
                    continue;
                }
                Pirate theart = getThreat(game, pirate);

                if (theart != null)
                {
                    
                    PirateTactics tactics5 = null;
                    if (pirate.HasTreasure)
                    {
                        if (pirate.DefenseReloadTurns == 0 && theart.ReloadTurns == 0)
                        {
                            tactics5 = new PirateTactics() { Pirate = pirate };
                            tactics5.defender = pirate;
                            game.Debug("perate " + pirate.Id + " make defence from theart " + theart.Id);
                        }

                    }
                    else
                    {
                        // game.Debug("thread " + theart.Id + " DefenseExpirationTurns=" + theart.DefenseExpirationTurns + ",DefenseReloadTurns=" + theart.DefenseReloadTurns);

                        if (theart.ReloadTurns == 0 && pirate.DefenseReloadTurns == 0)
                        {
                            tactics5 = new PirateTactics() { Pirate = pirate };
                            tactics5.defender = pirate;
                            game.Debug("perate " + pirate.Id + " make defence from threat" + theart.Id);
                        }
                        else if (theart.DefenseReloadTurns > 0 && theart.DefenseExpirationTurns == 0)
                        {
                            game.Debug("perate " + pirate.Id + " make attack from threat" + theart.Id);
                            tactics5 = new PirateTactics() { Pirate = pirate };
                            tactics5.Target = theart;
                        }
                    }
                    if (tactics5 != null)
                    {
                        list.Add(tactics5);
                    }


                }
            }


            return list;
        }
        private void TakeAction(IPirateGame game, List<PirateTactics> listTactis)
        {
            game.Debug("tactics number=" + listTactis.Count);

            foreach (PirateTactics tactic in listTactis)
            {
                if (tactic.Target != null)
                {
                    game.Debug("Perate " + tactic.Pirate.Id + " attack " + tactic.Target.Id);
                    game.Attack(tactic.Pirate, tactic.Target);

                }
                else if (tactic.defender != null)
                {
                    game.Debug("Perate " + tactic.defender.Id + " make defend ");
                    game.Defend(tactic.defender);
                }
                else if (tactic.TempDestination != null)
                {
                    game.Debug("Perate " + tactic.Pirate.Id + " sail to  " + tactic.TempDestination.ToString());
                    game.SetSail(tactic.Pirate, tactic.TempDestination);
                }

            }

        }


    }
}