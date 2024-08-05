using HahnCargoTruckLoader.Library.Model;

namespace HahnCargoTruckLoader.Library.Model
{
    public class CratePlacement
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
