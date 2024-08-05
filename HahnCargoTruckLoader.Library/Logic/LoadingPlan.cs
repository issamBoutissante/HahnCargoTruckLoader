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
        private readonly List<Crate> crates;
        private readonly Truck truck;
        public LoadingPlan(Truck truck, List<Crate> crates)
        {
            instructions = new Dictionary<int, LoadingInstruction>();
            this.crates = crates;
            this.truck = truck;
        }

        public Dictionary<int, LoadingInstruction> GetLoadingInstructions()
        {
            // Use CrateLoadingService to calculate loading instructions
            var service = new CrateLoadingService();
            var instructions = service.CalculateLoadingPlan(truck, crates);
            return instructions;
        }

        public (Dictionary<int, LoadingInstruction> instructions, List<Crate> placed, List<Crate> unplaced) GetLoadingResults()
        {
            var service = new CrateLoadingService();
            var instructions = service.CalculateLoadingPlan(truck, crates);

            // Extract placed and unplaced crates
            var placedCrates = instructions.Keys.Select(id => crates.First(c => c.CrateID == id)).ToList();
            var unplacedCrates = crates.Except(placedCrates).ToList();

            return (instructions, placedCrates, unplacedCrates);
        }
    }
}
