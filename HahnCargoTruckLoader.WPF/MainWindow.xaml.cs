using HelixToolkit.Wpf;
using HahnCargoTruckLoader.Library.Logic;
using HahnCargoTruckLoader.Library.Model;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System;

namespace HahnCargoTruckLoader.WPF
{
    public partial class MainWindow : Window
    {
        private LoadingPlan loadingPlan;

        public MainWindow()
        {
            InitializeComponent();
            InitializeScene();
            SetCameraDefaults();
        }

        private void InitializeScene()
        {
            var truck = Initialize.LoadTruck();
            var crates = Initialize.GetCrates();
            loadingPlan = new LoadingPlan(truck, crates);
            CratesListView.ItemsSource = crates;
            RedrawScene();
        }
        public (Dictionary<int, LoadingInstruction> instructions, List<Crate> placed, List<Crate> unplaced) GetLoadingResults()
        {
            var instructions = loadingPlan.GetLoadingInstructions();

            // Extract placed and unplaced crates
            var placedCrates = instructions.Keys.Select(id => loadingPlan.crates.First(c => c.CrateID == id)).ToList();
            var unplacedCrates = loadingPlan.crates.Except(placedCrates).ToList();

            return (instructions, placedCrates, unplacedCrates);
        }
        private void RedrawScene()
        {
            helixViewport.Children.Clear();
            unplacedViewport.Children.Clear();

            // Draw the truck with yellow borders
            Create3DBox(helixViewport, 0, 0, 0, loadingPlan.truck.Width, loadingPlan.truck.Height, loadingPlan.truck.Length, Colors.Yellow);
            this.TruckDimentions.Text = $"Width = {loadingPlan.truck.Width} - Length = {loadingPlan.truck.Length} - Height = {loadingPlan.truck.Height} - Volume = {loadingPlan.truck.Width * loadingPlan.truck.Length * loadingPlan.truck.Height}";

            var (instructions, placedCrates, unplacedCrates) = GetLoadingResults();

            // Draw placed crates with green borders
            foreach (var crate in placedCrates)
            {
                var instruction = instructions[crate.CrateID];
                crate.Turn(instruction);
                Create3DBox(helixViewport, instruction.TopLeftX, instruction.TopLeftY, crate.Instruction.TopLeftZ, crate.Width, crate.Height, crate.Length, Colors.Green);
            }

            // Draw unplaced crates with red borders in a separate area
            double offset = 0;
            foreach (var crate in unplacedCrates)
            {
                Create3DBox(unplacedViewport, offset, 0, 0, crate.Width, crate.Height, crate.Length, Colors.Red);
                offset += crate.Width + 1; // Add a small offset for spacing
            }

            helixViewport.Focus();
        }


        private void Create3DBox(HelixViewport3D viewport, double x, double y, double z, double width, double height, double length, Color borderColor)
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

            // Set edges to the specified border color
            LinesVisual3D edges = new LinesVisual3D
            {
                Color = borderColor,
                Thickness = 2
            };
            edges.Points = new Point3DCollection
            {
                corners[0], corners[1], corners[1], corners[2], corners[2], corners[3], corners[3], corners[0], // Front edges
                corners[4], corners[5], corners[5], corners[6], corners[6], corners[7], corners[7], corners[4], // Back edges
                corners[0], corners[4], corners[1], corners[5], corners[2], corners[6], corners[3], corners[7]  // Side edges
            };

            ModelVisual3D visual = new ModelVisual3D();
            viewport.Children.Add(visual);
            viewport.Children.Add(edges);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            int newId = loadingPlan.crates.Any() ? loadingPlan.crates.Max(c => c.CrateID) + 1 : 1;
            loadingPlan.crates.Add(new Crate { CrateID = newId, Height = 2, Width = 1, Length = 4 });
            CratesListView.ItemsSource = null;
            CratesListView.ItemsSource = loadingPlan.crates;
            RedrawScene();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int crateId)
            {
                var crateToRemove = loadingPlan.crates.FirstOrDefault(c => c.CrateID == crateId);
                if (crateToRemove != null)
                {
                    loadingPlan.crates.RemoveAll(c=>crateToRemove.CrateID==c.CrateID);
                    CratesListView.ItemsSource = null;
                    CratesListView.ItemsSource = loadingPlan.crates;
                    RedrawScene();
                }
            }
        }

        private void SetCameraDefaults()
        {
            helixViewport.Camera.Position = new Point3D(10, -15, 10);
            helixViewport.Camera.LookDirection = new Vector3D(-10, 15, -10);
            helixViewport.Camera.UpDirection = new Vector3D(0, 0, 1);
        }

        private void RotateScene(HelixViewport3D viewport, double angle, Vector3D axis)
        {
            var center = new Point3D(0, 0, 0); // Center point for rotation
            var rotateTransform = new RotateTransform3D(new AxisAngleRotation3D(axis, angle), center);

            foreach (var visual in viewport.Children)
            {
                if (visual is ModelVisual3D modelVisual)
                {
                    var transformGroup = modelVisual.Transform as Transform3DGroup ?? new Transform3DGroup();
                    transformGroup.Children.Add(rotateTransform);
                    modelVisual.Transform = transformGroup;
                }
            }
        }

        private void helixViewport_KeyDown(object sender, KeyEventArgs e)
        {
            var rotationAngle = 5; // degrees
            var axis = new Vector3D(0, 1, 0); // Default to Y-axis for horizontal rotation

            switch (e.Key)
            {
                case Key.Up:
                    axis = new Vector3D(1, 0, 0); // Rotate around X-axis for upward/downward view
                    RotateScene(helixViewport, rotationAngle, axis);
                    break;
                case Key.Down:
                    axis = new Vector3D(-1, 0, 0); // Inverse X-axis rotation
                    RotateScene(helixViewport, rotationAngle, axis);
                    break;
                case Key.Left:
                    axis = new Vector3D(0, 1, 0); // Rotate left around Y-axis
                    RotateScene(helixViewport, rotationAngle, axis);
                    break;
                case Key.Right:
                    axis = new Vector3D(0, -1, 0); // Rotate right around Y-axis
                    RotateScene(helixViewport, rotationAngle, axis);
                    break;
            }
        }
        private void unplacedViewport_KeyDown(object sender, KeyEventArgs e)
            {
                var rotationAngle = 5; // degrees
                var axis = new Vector3D(0, 1, 0); // Default to Y-axis for horizontal rotation

                switch (e.Key)
                {
                    case Key.Left:
                        axis = new Vector3D(0, 1, 0); // Rotate left around Y-axis
                        RotateScene(unplacedViewport, rotationAngle, axis);
                        break;
                    case Key.Right:
                        axis = new Vector3D(0, -1, 0); // Rotate right around Y-axis
                        RotateScene(unplacedViewport, rotationAngle, axis);
                        break;
                }
            }

            private void FocusHelixViewport(object sender, RoutedEventArgs e)
            {
                helixViewport.Focus();
            }

            private void FocusUnplacedViewport(object sender, RoutedEventArgs e)
            {
                unplacedViewport.Focus();
            }
        }
    }
