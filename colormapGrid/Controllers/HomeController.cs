using colormapGrid.Models;
using Microsoft.AspNetCore.Mvc;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.SkiaSharp;
using System.Diagnostics;

namespace colormapGrid.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var gridData = new List<dynamic>
            {
                new { X = 1, Y = 1, Count = 5 },
                new { X = 2, Y = 1, Count = 3 },
                new { X = 3, Y = 1, Count = 7 },
                new { X = 1, Y = 2, Count = 2 },
                new { X = 2, Y = 2, Count = 6 },
                new { X = 3, Y = 2, Count = 1 },
                // Add more data points as needed
            };

            return View(gridData);
        }

        public IActionResult Privacy()
        {
            // Sample data points (replace with your actual data)
            List<(int x, int y)> dataPoints = new List<(int x, int y)>
            {
                (10, 20), (60, 80), (120, 100), (200, 300), (400, 500),
                // Add more data points as needed
            };

            // Define the grid dimensions
            int gridWidth = 415;
            int gridHeight = 510;
            int cellSize = 50;

            // Calculate the number of cells
            int numCellsX = (int)Math.Ceiling((double)gridWidth / cellSize);
            int numCellsY = (int)Math.Ceiling((double)gridHeight / cellSize);

            // Initialize the count grid
            int[,] countGrid = new int[numCellsX, numCellsY];

            // Populate the count grid based on the data points
            foreach (var point in dataPoints)
            {
                int cellX = point.x / cellSize; // Determine cell index on x-axis
                int cellY = (gridHeight - point.y) / cellSize; // Determine cell index on y-axis

                // Check if the cell indices are within the grid bounds
                if (cellX < numCellsX && cellY < numCellsY && cellY >= 0)
                {
                    countGrid[cellX, cellY]++;
                }
            }

            // Prepare the count grid for view
            ViewBag.CountGrid = countGrid;
            ViewBag.NumCellsX = numCellsX;
            ViewBag.NumCellsY = numCellsY;

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult CreateAndSaveGridImage()
        {
            List<(int x, int y)> dataPoints = new List<(int x, int y)>
    {
        (10, 20),(15,25), (60, 80), (120, 100), (200, 300), (400, 500),
        // Add more data points as needed
    };

            // Define the cell size and grid dimensions
            int cellSize = 50;
            int numCellsX = 9;
            int numCellsY = 11;

            // Initialize the count grid
            int[,] countGrid = new int[numCellsX, numCellsY];

            // Populate the count grid based on the data points
            foreach (var point in dataPoints)
            {
                int cellX = point.x / cellSize; // Determine cell index on x-axis
                int cellY = point.y / cellSize; // Determine cell index on y-axis without inversion

                // Check if the cell indices are within the grid bounds
                if (cellX >= 0 && cellX < numCellsX && cellY >= 0 && cellY < numCellsY)
                {
                    countGrid[cellX, cellY]++;
                }
            }

            // Create the plot and save it as an image
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "grid_image.png");
            SaveGridWithCountsAsImage(countGrid, numCellsX, numCellsY, cellSize, imagePath);

            return Ok("Grid image created and saved at: " + imagePath);
        }

        private void SaveGridWithCountsAsImage(int[,] countGrid, int numCellsX, int numCellsY, int cellSize, string imagePath)
        {
            try
            {
                var plotModel = new PlotModel
                {
                    Title = "Grid Count Visualization",
                    Background = OxyColors.White
                };

                // Add X and Y axes
                plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Minimum = 0, Maximum = numCellsX * cellSize, Title = "X-Axis", MajorGridlineStyle = LineStyle.None });
                plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Minimum = 0, Maximum = numCellsY * cellSize, Title = "Y-Axis", MajorGridlineStyle = LineStyle.None });

                // Find the max count to set the color gradient range
                int maxCount = 0;
                foreach (var count in countGrid)
                {
                    if (count > maxCount) maxCount = count;
                }

                // Helper function to determine color based on count
                //OxyColor GetColorForCount(int count)
                //{
                //    if (count == 0)
                //    {
                //        return OxyColors.Green; // Use green for zero count
                //    }
                //    else
                //    {
                //        double ratio = (double)count / maxCount;
                //        int red = (int)(255 * ratio);
                //        int green = (int)(255 * (1 - ratio));
                //        return OxyColor.FromRgb((byte)red, (byte)green, 0); // Gradient from green to yellow
                //    }
                //}

                OxyColor GetColorForCount(int count)
                {
                    if (count == 0)
                    {
                        return OxyColors.Green;
                    }
                    else
                    {
                        double ratio = (double)count / maxCount;

                        // Gradually increase the green intensity and adjust the red for mustard yellow
                        int red = (int)(255 * ratio); // Red increases with count
                        int green = (int)(255 * (1 - (ratio * 0.5))); // Decrease green intensity to reach mustard
                        int blue = 0; // Blue remains 0

                        return OxyColor.FromRgb((byte)red, (byte)green, (byte)blue); // Gradient from green to mustard yellow
                    }
                }

                //working code

                // Add text annotations for counts in each cell
                //for (int x = 0; x < numCellsX; x++)
                //{
                //    for (int y = 0; y < numCellsY; y++)
                //    {
                //        int count = countGrid[x, y];
                //        if (count > 0) // Only display cells with counts
                //        {
                //            plotModel.Annotations.Add(new TextAnnotation
                //            {
                //                Text = count.ToString(),
                //                TextPosition = new DataPoint((x + 0.5) * cellSize, (y + 0.5) * cellSize),
                //                Stroke = OxyColors.Transparent,
                //                TextHorizontalAlignment = HorizontalAlignment.Center,
                //                TextVerticalAlignment = VerticalAlignment.Middle,
                //                FontSize = 12,
                //                FontWeight = FontWeights.Bold
                //            });
                //        }
                //    }
                //}

                //exp - color
                // Add rectangles with color-coded backgrounds based on counts
                for (int x = 0; x < numCellsX; x++)
                {
                    for (int y = 0; y < numCellsY; y++)
                    {
                        int count = countGrid[x, y];
                        var color = GetColorForCount(count);

                        // Draw a colored rectangle for each cell
                        var rectangleAnnotation = new RectangleAnnotation
                        {
                            MinimumX = x * cellSize,
                            MaximumX = (x + 1) * cellSize,
                            MinimumY = y * cellSize,
                            MaximumY = (y + 1) * cellSize,
                            Fill = color
                        };
                        plotModel.Annotations.Add(rectangleAnnotation);

                        // Add count text annotation inside each cell
                        //if (count > 0)
                        {
                            plotModel.Annotations.Add(new TextAnnotation
                            {
                                Text = count.ToString(),
                                TextPosition = new DataPoint((x + 0.5) * cellSize, (y + 0.5) * cellSize),
                                Stroke = OxyColors.Transparent,
                                TextHorizontalAlignment = HorizontalAlignment.Center,
                                TextVerticalAlignment = VerticalAlignment.Middle,
                                FontSize = 12,
                                FontWeight = FontWeights.Bold,
                                TextColor = OxyColors.Black
                            });
                        }
                    }
                }

                // Manually draw grid lines for each cell
                for (int x = 0; x <= numCellsX; x++)
                {
                    plotModel.Annotations.Add(new LineAnnotation
                    {
                        Type = LineAnnotationType.Vertical,
                        X = x * cellSize,
                        Color = OxyColors.Black,
                        StrokeThickness = 2,
                        LineStyle = LineStyle.Solid,
                    });
                }

                for (int y = 0; y <= numCellsY; y++)
                {
                    plotModel.Annotations.Add(new LineAnnotation
                    {
                        Type = LineAnnotationType.Horizontal,
                        Y = y * cellSize,
                        Color = OxyColors.Black,
                        StrokeThickness = 2,
                        LineStyle = LineStyle.Solid,
                    });
                }

                // Save the plot as an image with specified dimensions
                using (var stream = new FileStream(imagePath, FileMode.Create, FileAccess.Write))
                {
                    var pngExporter = new PngExporter { Width = 415, Height = 510 };
                    pngExporter.Export(plotModel, stream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }
    }
}
