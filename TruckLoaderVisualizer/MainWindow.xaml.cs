using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TruckLoaderVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Initialize3DScene();
        }

        private void Initialize3DScene()
        {
            // Load truck and crates
            Truck truck = Initialize.LoadTruck();
            List<Crate> crates = Initialize.GetCrates();

            // Get loading instructions
            LoadingPlan loadingPlan = new LoadingPlan(truck, crates);
            Dictionary<int, LoadingInstruction> instructions = loadingPlan.GetLoadingInstructions();

            // Create the truck container
            Create3DBox(0, 0, 0, truck.Width, truck.Height, truck.Length, Colors.Gray);

            // Create crates according to loading instructions
            foreach (var instruction in instructions.Values)
            {
                Crate crate = crates.First(c => c.CrateID == instruction.CrateId);
                Create3DBox(instruction.TopLeftX, instruction.TopLeftY, 0, crate.Width, crate.Height, crate.Length, Colors.Red);
            }
        }

        private void Create3DBox(double x, double y, double z, double width, double height, double length, Color color)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            Point3DCollection corners = new Point3DCollection
            {
                new Point3D(x, y, z),
                new Point3D(x + width, y, z),
                new Point3D(x + width, y + height, z),
                new Point3D(x, y + height, z),
                new Point3D(x, y, z + length),
                new Point3D(x + width, y, z + length),
                new Point3D(x + width, y + height, z + length),
                new Point3D(x, y + height, z + length)
            };

            Int32Collection triangles = new Int32Collection
            {
                0,1,2, 0,2,3, // Front
                4,5,6, 4,6,7, // Back
                0,1,5, 0,5,4, // Bottom
                2,3,7, 2,7,6, // Top
                1,2,6, 1,6,5, // Right
                0,3,7, 0,7,4  // Left
            };

            mesh.Positions = corners;
            mesh.TriangleIndices = triangles;

            GeometryModel3D model = new GeometryModel3D
            {
                Geometry = mesh,
                Material = new DiffuseMaterial(new SolidColorBrush(color))
            };

            ModelVisual3D visual = new ModelVisual3D { Content = model };
            viewport.Children.Add(visual);
        }
    }
}