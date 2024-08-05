using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HahnCargoTruckLoader.Library.Model;

namespace HahnCargoTruckLoader.Library
{
    public class CrateLoadingService
    {
        public Dictionary<int, LoadingInstruction> CalculateLoadingPlan(Truck truck, List<Crate> crates)
        {
            var instructions = new Dictionary<int, LoadingInstruction>();
            var placedCrates = new List<CratePlacement>();

            foreach (var crate in crates)
            {
                bool placed = false;
                for (int turnHorizontal = 0; turnHorizontal <= 1 && !placed; turnHorizontal++)
                {
                    for (int turnVertical = 0; turnVertical <= 1 && !placed; turnVertical++)
                    {
                        crate.Turn(new LoadingInstruction { TurnHorizontal = turnHorizontal == 1, TurnVertical = turnVertical == 1 });

                        for (int x = 0; x < truck.Width - crate.Width + 1; x++)
                        {
                            for (int y = 0; y < truck.Height - crate.Height + 1; y++)
                            {
                                for (int z = 0; z < truck.Length - crate.Length + 1; z++)  // Correct iteration variable here
                                {
                                    if (IsPositionAvailable(x, y, z, crate, truck, placedCrates))
                                    {
                                        if (!instructions.ContainsKey(crate.CrateID))
                                        {
                                            instructions[crate.CrateID] = new LoadingInstruction
                                            {
                                                CrateId = crate.CrateID,
                                                TopLeftX = x,
                                                TopLeftY = y,
                                                TurnHorizontal = turnHorizontal == 1,
                                                TurnVertical = turnVertical == 1
                                            };

                                            placedCrates.Add(new CratePlacement(crate, x, y, z));
                                            placed = true;
                                            break;
                                        }
                                        else
                                        {
                                            // Handle duplicate CrateID if necessary
                                            // This situation should not occur if CrateID is unique
                                            Console.WriteLine($"Duplicate CrateID: {crate.CrateID}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return instructions;
        }

        private bool IsPositionAvailable(int x, int y, int z, Crate crate, Truck truck, List<CratePlacement> placedCrates)
        {
            if (x + crate.Width > truck.Width || y + crate.Height > truck.Height || z + crate.Length > truck.Length)
            {
                return false;
            }

            return placedCrates.All(placed => !DoCratesOverlap(placed, new CratePlacement(crate, x, y, z)));
        }

        private bool DoCratesOverlap(CratePlacement a, CratePlacement b)
        {
            return a.X < b.X + b.Crate.Width &&
                   a.X + a.Crate.Width > b.X &&
                   a.Y < b.Y + b.Crate.Height &&
                   a.Y + a.Crate.Height > b.Y &&
                   a.Z < b.Z + b.Crate.Length &&
                   a.Z + a.Crate.Length > b.Z;
        }

        private class CratePlacement
        {
            public Crate Crate { get; }
            public int X { get; }
            public int Y { get; }
            public int Z { get; }

            public CratePlacement(Crate crate, int x, int y, int z)
            {
                Crate = crate;
                X = x;
                Y = y;
                Z = z;
            }
        }
    }
}
