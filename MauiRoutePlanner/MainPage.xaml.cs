using Microsoft.Maui.Controls.Maps;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;

namespace MauiRoutePlanner
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            InitializeMap();
        }

        // set initial map details
        private void InitializeMap()
        {
            // set starting coordinates and zoom mileage
            MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(44.52471473008515, -89.58544206749154), Distance.FromMiles(10)));
        }

        // click event for 'Calculate Route' button
        private async void OnCalculateRouteClicked(object sender, EventArgs e)
        {
            // get starting point and destination from user input
            string startingPoint = StartingPointEntry.Text;
            string destination = DestinationEntry.Text;

            // check for empty input
            if (string.IsNullOrEmpty(startingPoint) || string.IsNullOrEmpty(destination))
            {
                await DisplayAlert("Error", "Please enter both starting point and destination.", "OK");
                return;
            }

            try
            {
                // geocode the location inputs
                var startLocation = await GeocodeAddressAsync(startingPoint);
                var endLocation = await GeocodeAddressAsync(destination);

                // check for empty locations
                if (startLocation == null || endLocation == null)
                {
                    await DisplayAlert("Error", "Unable to find locations. Please check the input addresses.", "OK");
                    return;
                }

                // calculate and display valid route
                DisplayRoute(startLocation, endLocation);
                DisplayRouteInfo(startLocation, endLocation);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred while calculating the route: {ex.Message}", "OK");
            }
        }

        // geocode the address input
        private async Task<Location> GeocodeAddressAsync(string address)
        {
            try
            {
                // get location from address
                var locations = await Geocoding.GetLocationsAsync(address);
                return locations?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred while geocoding the address '{address}': {ex.Message}", "OK");
                return null;
            }
        }

        // display route on map
        private void DisplayRoute(Location startLocation, Location endLocation)
        {
            // remove existing pins/lines
            // add pins/lines for new route
            MyMap.Pins.Clear();
            MyMap.Pins.Add(new Pin
            {
                Label = "Start",
                Address = "Start",
                Location = new Location(startLocation.Latitude, startLocation.Longitude)
            });
            MyMap.Pins.Add(new Pin
            {
                Label = "End",
                Address = "End",
                Location = new Location(endLocation.Latitude, endLocation.Longitude),
            });

            // create a polyline for the route
            var polyline = new Polyline
            {
                StrokeColor = Colors.HotPink,
                StrokeWidth = 5,
                Geopath =
                {
                    new Location(startLocation.Latitude, startLocation.Longitude),
                    new Location(endLocation.Latitude, endLocation.Longitude)
                }
            };

            // clear existing map elements and add new polyline
            MyMap.MapElements.Clear();
            MyMap.MapElements.Add(polyline);

            // calculate bounding box
            var minLat = Math.Min(startLocation.Latitude, endLocation.Latitude);
            var maxLat = Math.Max(startLocation.Latitude, endLocation.Latitude);
            var minLon = Math.Min(startLocation.Longitude, endLocation.Longitude);
            var maxLon = Math.Max(startLocation.Longitude, endLocation.Longitude);

            // add padding
            var padding = 0.01;
            minLat -= padding;
            maxLat += padding;
            minLon -= padding;
            maxLon += padding;

            // center map on route
            var centerLatitude = (minLat + maxLat) / 2;
            var centerLongitude = (minLon + maxLon) / 2;
            var latitudeDelta = maxLat - minLat;
            var longitudeDelta = maxLon - minLon;

            MyMap.MoveToRegion(new MapSpan(
                new Location(centerLatitude, centerLongitude),
                latitudeDelta, longitudeDelta));
        }

        // display route info
        private void DisplayRouteInfo(Location startLocation, Location endLocation)
        {
            // calculate route distance
            var distance = Location.CalculateDistance(startLocation, endLocation, DistanceUnits.Miles);

            // calculate estimated travel time
            var averageSpeedMph = 60.0;
            var travelTime = distance / averageSpeedMph;

            // display distance and travel time
            DistanceLabel.Text = $"Distance: {distance:F2} mi";
            TravelTimeLabel.Text = $"Estimated Travel Time: {travelTime:F2} hours";
        }
    }
}
