using HahnCargoTruckLoader.Library.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HahnCargoTruckLoader.Library.Logic
{
    public class LoadingPlan
    {
        private readonly Dictionary<int, LoadingInstruction> instructions;
        public readonly List<Crate> crates;
        public readonly Truck truck;
        public LoadingPlan(Truck truck, List<Crate> crates)
        {
            instructions = new Dictionary<int, LoadingInstruction>();
            this.crates = crates;
            this.truck = truck;
        }

        public Dictionary<int, LoadingInstruction> GetLoadingInstructions()
        {
            var instructions = new Dictionary<int, LoadingInstruction>();
            var placedCrates = new List<CratePlacement>();
            int stepNumber = 1; // Initialize step number

            foreach (var crate in crates)
            {
                bool placed = PlaceCrate(crate, truck, instructions, placedCrates, ref stepNumber);
                if (!placed)
                {
                    // Handle the case where the crate cannot be placed
                    Console.WriteLine($"Crate {crate.CrateID} could not be placed.");
                }
            }

            return instructions;
        }

        private bool PlaceCrate(Crate crate, Truck truck, Dictionary<int, LoadingInstruction> instructions, List<CratePlacement> placedCrates, ref int stepNumber)
        {
            for (int turnHorizontal = 0; turnHorizontal <= 1; turnHorizontal++)
            {
                for (int turnVertical = 0; turnVertical <= 1; turnVertical++)
                {
                    crate.Turn(new LoadingInstruction { TurnHorizontal = turnHorizontal == 1, TurnVertical = turnVertical == 1 });

                    for (int x = 0; x < truck.Width - crate.Width + 1; x++)
                    {
                        for (int y = 0; y < truck.Height - crate.Height + 1; y++)
                        {
                            for (int z = 0; z < truck.Length - crate.Length + 1; z++)
                            {
                                if (IsPositionAvailable(x, y, z, crate, truck, placedCrates))
                                {
                                    instructions[crate.CrateID] = new LoadingInstruction
                                    {
                                        LoadingStepNumber = stepNumber++, // Assign and increment step number
                                        CrateId = crate.CrateID,
                                        TopLeftX = x,
                                        TopLeftY = y,
                                        TopLeftZ = z,
                                        TurnHorizontal = turnHorizontal == 1,
                                        TurnVertical = turnVertical == 1
                                    };

                                    placedCrates.Add(new CratePlacement(crate, x, y, z));
                                    crate.Instruction = instructions[crate.CrateID];
                                    return true; // Exit all loops and return success
                                }
                            }
                        }
                    }
                }
            }

            return false; // Return false if the crate could not be placed
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
