using HahnCargoTruckLoader.Library.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HahnCargoTruckLoader.Library.Logic
{
    public class LoadingPlan
    {
        private readonly Dictionary<int, LoadingInstruction> instructions;
        public readonly List<Crate> crates;
        public readonly Truck truck;

        public LoadingPlan(Truck truck, List<Crate> crates)
        {
            instructions = [];
            this.crates = crates;
            this.truck = truck;
        }

        public Dictionary<int, LoadingInstruction> GetLoadingInstructions()
        {
            var placedCrates = new List<CratePlacement>();
            int stepNumber = 1;

            foreach (var crate in crates)
                TryPlaceCrate(crate, placedCrates, ref stepNumber);

            return instructions;
        }

        private bool TryPlaceCrate(Crate crate, List<CratePlacement> placedCrates, ref int stepNumber)
        {
            var orientations = GetPossibleOrientations(crate);

            foreach (var orientation in orientations)
            {
                crate.Turn(orientation);
                if (TryPlaceCrateInOrientation(crate, placedCrates, ref stepNumber))
                {
                    return true;
                }
            }
            return false;
        }

        private bool TryPlaceCrateInOrientation(Crate crate, List<CratePlacement> placedCrates, ref int stepNumber)
        {
            for (int x = 0; x < truck.Width - crate.Width + 1; x++)
            {
                for (int y = 0; y < truck.Height - crate.Height + 1; y++)
                {
                    for (int z = 0; z < truck.Length - crate.Length + 1; z++)
                    {
                        if (IsPositionAvailable(x, y, z, crate, placedCrates))
                        {
                            AddLoadingInstruction(crate, x, y, z, ref stepNumber);
                            placedCrates.Add(new CratePlacement(crate, x, y, z));
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void AddLoadingInstruction(Crate crate, int x, int y, int z, ref int stepNumber)
        {
            instructions[crate.CrateID] = new LoadingInstruction
            {
                LoadingStepNumber = stepNumber++, // Assign and increment step number
                CrateId = crate.CrateID,
                TopLeftX = x,
                TopLeftY = y,
                TopLeftZ = z,
                TurnHorizontal = crate.Instruction?.TurnHorizontal ?? false, // Use values directly if available
                TurnVertical = crate.Instruction?.TurnVertical ?? false     // Use values directly if available
            };

            // Set the Instruction property for the crate
            crate.Instruction = instructions[crate.CrateID];
        }

        private bool IsPositionAvailable(int x, int y, int z, Crate crate, List<CratePlacement> placedCrates)
        {
            // Check if the crate exceeds the truck's boundaries
            if (x + crate.Width > truck.Width || y + crate.Height > truck.Height || z + crate.Length > truck.Length)
            {
                // If any dimension of the crate exceeds the truck's size, the position is not available
                return false;
            }

            // Check if the crate overlaps with any already placed crates
            return placedCrates.All(placed => !DoCratesOverlap(placed, new CratePlacement(crate, x, y, z)));
        }


        /// <summary>
        /// Checks if two crates overlap.
        /// Condition for no overlap:
        /// - a.X >= b.X + b.Crate.Width  (a is to the right of b)
        /// - a.X + a.Crate.Width <= b.X  (a is to the left of b)
        /// - a.Y >= b.Y + b.Crate.Height (a is above b)
        /// - a.Y + a.Crate.Height <= b.Y (a is below b)
        /// - a.Z >= b.Z + b.Crate.Length (a is behind b)
        /// - a.Z + a.Crate.Length <= b.Z (a is in front of b)
        /// </summary>
        private bool DoCratesOverlap(CratePlacement a, CratePlacement b)
        {
            return a.X < b.X + b.Crate.Width &&
                   a.X + a.Crate.Width > b.X &&
                   a.Y < b.Y + b.Crate.Height &&
                   a.Y + a.Crate.Height > b.Y &&
                   a.Z < b.Z + b.Crate.Length &&
                   a.Z + a.Crate.Length > b.Z;
        }

        private IEnumerable<LoadingInstruction> GetPossibleOrientations(Crate crate)
        {
            bool isCubic = crate.Width == crate.Height && crate.Height == crate.Length;
            bool isFlat = crate.Width == crate.Height || crate.Width == crate.Length || crate.Height == crate.Length;

            if (isCubic)
            {
                // Only one unique orientation for cubic crates
                return new List<LoadingInstruction> { new() { TurnHorizontal = false, TurnVertical = false } };
            }
            else if (isFlat)
            {
                // For crates with two equal dimensions
                return new List<LoadingInstruction>
                {
                    new() { TurnHorizontal = false, TurnVertical = false },
                    new() { TurnHorizontal = false, TurnVertical = true }
                };
            }
            else
            {
                // General case with three distinct dimensions
                return new List<LoadingInstruction>
                {
                    new() { TurnHorizontal = false, TurnVertical = false },
                    new() { TurnHorizontal = true, TurnVertical = false },
                    new() { TurnHorizontal = false, TurnVertical = true },
                    new() { TurnHorizontal = true, TurnVertical = true }
                };
            }
        }
    }
}
