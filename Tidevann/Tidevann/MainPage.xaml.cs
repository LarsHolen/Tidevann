using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Xamarin.Forms;
using Xamarin.Essentials;
using Tidevann.Models;
using Location = Tidevann.Models.Location;
using Newtonsoft.Json;
using System.Globalization;
using System.Threading;
using Tidevann.Data;

namespace Tidevann
{
    public partial class MainPage : ContentPage
    {
        // Using this httpclient for all api calls
        readonly HttpClient client = new HttpClient();


        // List of Tides/times that will be shown
        private List<TidevannModel> myTidevannModelList = new List<TidevannModel>();

        // List of places when searching for a geografic location 
        private List<GeoStedModel> myGeoStedList = new List<GeoStedModel>();


        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;

            // Setting CultureInfo to en-US because of long and lat coordinates from API
            CultureInfo.CurrentCulture = new CultureInfo("en-US");

            // Setting binding for the picker
            LocationPicker.ItemDisplayBinding = new Binding("Name");

            // Loads predefined names/coords for the pickerlist
            LocationPicker.ItemsSource = StaticLocationRepository.LocList;

            // Adding select handler for the picker list
            LocationPicker.SelectedIndexChanged += SelectedLocChange;
        }


        /// <summary>
        /// Handler for when one select a location from the static list of locations
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectedLocChange(object sender, EventArgs e)
        {
            // Select location from pickerlist, and loads data from TestApi
            Picker picker = sender as Picker;
            if (picker.SelectedItem == null) return;
            Location l = picker.SelectedItem as Location;
            // picker.Title = l.Name + ", tap for å velge annet fra liste.";
            double lan = l.Lan;
            double lon = l.Lon;
            TestApi(lan, lon);
            picker.SelectedItem = null;
        }

        /// <summary>
        /// Handler for when GPS is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GpsClick(object sender, EventArgs e)
        {
            await TestGPSAsync();
        }

        /// <summary>
        /// Trying to get a position from the GPS
        /// </summary>
        /// <returns></returns>
        private async Task TestGPSAsync()
        {
            // Trying to get a cached position

            // Disabel the button, hide it and show activityindicator
            GpsButtonOn(false);

            // trying to get cached gps position, since its a lot faster than getting a new
            try
            {
                var location = await Geolocation.GetLastKnownLocationAsync();

                if (location != null)
                {
                    // Got an location, start to load the tide info and reset the button 
                    Debug.WriteLine("Positioning: " + location.Latitude.ToString() + " - "  + location.Longitude.ToString() + "...");
                    TestApi(location.Latitude, location.Longitude);
                    GpsButtonOn(true);
                }
                else
                {
                    Debug.WriteLine("No pos found");
                    // No location found.  Trying to get a new pos, and catch possible errors in that method
                    GetNewPos();
                }
            }
            catch (Exception ex)
            {
                // Unable to get location, trying to get a new one
                Debug.WriteLine("GPS cached error(loading new): " + ex.Message);
                GetNewPos();
            }

        }

        /// <summary>
        /// If getting a cached GPS pos fails, this func will try to activate and get a new pos from the GPS
        /// Om en får feil, vises en feilmelding med DisplayAlert
        /// </summary>
        private async void GetNewPos()
        {
            Debug.WriteLine("Trying to get gps");
            // Trying to get a new GPS position, which can be slow.  Setting max time to 30 sec
            CancellationTokenSource cts;
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Lowest, TimeSpan.FromSeconds(30));
                cts = new CancellationTokenSource();
                var geoLocation = await Geolocation.GetLocationAsync(request, cts.Token);
                if (geoLocation != null)
                {
                    // Got a location, loading tide data
                    Debug.WriteLine("Position: ", geoLocation.Latitude, geoLocation.Longitude);
                    TestApi(geoLocation.Latitude, geoLocation.Longitude);
                    GpsButtonOn(true);
                }
                else
                {
                    Debug.WriteLine("Position zero");
                    await DisplayAlert("Feil", "GPS data ikke funnet.", "Ok");
                    GpsButtonOn(true);
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                Debug.WriteLine("**fns***" + fnsEx.Message);
                await DisplayAlert("Feil", "GPS ikke funnet.", "Ok");
                GpsButtonOn(true);
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
                Debug.WriteLine("**fne***" + fneEx.Message);
                await DisplayAlert("Feil", "GPS ikke på.", "Ok");
                GpsButtonOn(true);
            }
            catch (PermissionException pEx)
            {
                Debug.WriteLine("*pex****" + pEx.Message);
                await DisplayAlert("Feil", "Mangler tillatelse for å bruke GPS.", "Ok");
                GpsButtonOn(true);
            }
            catch (Exception ex)
            {
                // Unable to get location
                Debug.WriteLine("**ex***" + ex.Message);
                await DisplayAlert("Feil", "Greier ikke finne GPS data.", "Ok");
                GpsButtonOn(true);
            }
        }

        /// <summary>
        /// Toggeling GPS button on off and set the activity indicator on/off
        /// </summary>
        /// <param name="v"></param>
        private void GpsButtonOn(bool v)
        {
            if (v)
            {
                act.IsRunning = false;
                act.IsVisible = false;
                gpsBtn.IsVisible = true;
            }
            else
            {
                gpsBtn.IsVisible = false;
                act.IsRunning = true;
                act.IsVisible = true;
            }
        }

        /// <summary>
        /// Loading tide data from the API.
        /// </summary>
        /// <param name="lan"> breddegrader </param>
        /// <param name="lon"> lengdegrader </param>
        private async void TestApi(double lan, double lon)
        {
            // Loads tide data from coordinates
            try
            {
                // adding an custom item to the list that show "loading" text in the list
                myTidevannModelList.Clear();
                myListView.ItemsSource = myTidevannModelList;
                myListView.Header = MyHeader("Laster inn tidevanns data");

                // Getting time for yesterday, and in 7 days, to retrive data for one week + yesterday
                DateTime now = DateTime.Now.AddHours(-12);
                DateTime oneWeek = DateTime.Now.AddDays(7);
                string connectionString = "https://api.sehavniva.no/tideapi.php?lat=" + lan.ToString() + "&lon=" + lon.ToString() + "&fromtime=" + now.ToString("u") + "&totime=" + oneWeek.ToString("u") + "&datatype=tab&refcode=msl&place=&file=&lang=nn&interval=10&dst=0&tzone=&tide_request=locationdata";

                var respones = await client.GetAsync(connectionString);


                respones.EnsureSuccessStatusCode();

                if (respones.Content != null)
                {
                    string responseContent = await respones.Content.ReadAsStringAsync();
                    // if responseContent contain the word "Beklager", we did not get any tide data.  Same with "Position outside area"
                    if (responseContent.Contains("Beklager"))
                    {
                        LocationPicker.Title = "Velg sted!";
                        responseContent = "";
                        myTidevannModelList = new List<TidevannModel>();
                        myListView.ItemsSource = myTidevannModelList;
                        myListView.Header = MyHeader("Det finnes ikke data for dette området, eller det mangler koordinater.");
                        return;
                    }
                    else if (responseContent.Contains("Position outside area"))
                    {
                        LocationPicker.Title = "Velg sted!";
                        myListView.Header = MyHeader("GPS er utenfor kartverkets dekning");
                        responseContent = "";
                        myTidevannModelList = new List<TidevannModel>();
                        myListView.ItemsSource = myTidevannModelList;
                        //myListView.Header = MyHeader("GPS er utenf\nor kartverkets dekning" + respones.StatusCode + " kake\n");
                        return;
                    }

                    // load data into  xmlDocument
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.LoadXml(responseContent);

                    // Get the "location" elements.
                    XmlNodeList lcList = xDoc.GetElementsByTagName("location");
                    // picking the first element.  Should only be one, since we request tide data for only one location
                    XmlNode locEle = lcList.Item(0);

                    myListView.Header = MyHeader("Kartverkets tidevanns data for 7 dager");
                    XmlNodeList tide = xDoc.GetElementsByTagName("waterlevel");
                    XmlNode docNode = xDoc.DocumentElement;
                    if (docNode.OuterXml == "<error>Position outside area</error>")
                    {
                        myListView.Header = MyHeader("Position utenfor APIens område/norskekysten, tap her for å velge fra liste.");
                        return;
                    }
                    myTidevannModelList = new List<TidevannModel>();
                    foreach (XmlElement ti in tide)
                    {
                        //Debug.WriteLine("*******Høy/lav: " + ti.GetAttribute("flag") + "        Time: " + ti.GetAttribute("time") + "                 Vannnivå i forhold til middel:" + ti.GetAttribute("value"));
                        TidevannModel t = new TidevannModel()
                        {
                            Flag = ti.GetAttribute("flag"),
                            Verdi = ti.GetAttribute("value"),
                            Tid = ti.GetAttribute("time")
                        };
                        myTidevannModelList.Add(t);

                    }
                    myListView.ItemsSource = myTidevannModelList;

                    myListView.ScrollTo(myListView.Header, ScrollToPosition.MakeVisible, false);

                    myListView.ItemTapped += TideTapped;
                    
                }
                else
                {
                    myListView.Header = MyHeader("Problemer med å laste ned data, tap her for å velge fra liste for å prøve igjen.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CONTENT exeption:: " + ex.Message);
            }

        }

        public Frame MyHeader(string s)
        {
            Frame f = new Frame()
            {
                CornerRadius = 5,
                HasShadow = true,
                BackgroundColor = Color.GhostWhite,
                BorderColor = Color.DarkGray,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Content = new Label()
                {
                    Text = s,
                    VerticalOptions = LayoutOptions.StartAndExpand,
                    HorizontalOptions = LayoutOptions.Fill,
                    FontSize = 12,
                    TextColor = Color.Black,
                    FontAttributes = FontAttributes.Bold,
                    BackgroundColor = Color.GhostWhite,
                    Padding = 5
                }
            };
            
            return f;
        }

        private void TideTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is TidevannModel t)
            {
                Debug.WriteLine("TidevannsModel item: " + t.Dag + " " + t.Flag + " " + t.Klokke + " " + t.Verdi);
            }

        }




        private async void HelpClick(object sender, EventArgs e)
        {
            // add help info
            await DisplayAlert("Info:", "GPS vil prøve å finne data for din cachede posisjon, før den prøver å skaffe en ny GPS posisjon(som tar lengre tid)." +
                "  Søk lar deg søke etter steder fra kartverkets database.  " +
                "Og du kan velge fra noen utvalgte steder.  " +
                "Du kan få feil om stedet er for langt borte fra hav eller i området rundt Egersund(ingen data, lite tidevann der).", "OK");
        }

        private async void SokClick(object sender, EventArgs e)
        {
            string stedsNavn = await DisplayPromptAsync("Søk", "Stedsnavn:");
            if (!string.IsNullOrEmpty(stedsNavn)) GetLocNameFromGeodata(stedsNavn);
        }

        private async void GetLocNameFromGeodata(string stedsNavn)
        {
            // Finner en liste med steder med stringens navn fra kartverket
            var bak = this.Content;
            myGeoStedList = new List<GeoStedModel>();
            try
            {
                string connectionString = "https://ws.geonorge.no/stedsnavn/v1/navn?sok=" + stedsNavn + "&fuzzy=true&utkoordsys=4258&treffPerSide=10&side=1&filtrer=navn.skrivem%C3%A5te%2Cnavn.navneobjekttype%2Cnavn.fylker.fylkesnavn%2Cnavn.representasjonspunkt.%C3%B8st%2Cnavn.representasjonspunkt.nord";
                var respones = await client.GetAsync(connectionString);
                respones.EnsureSuccessStatusCode();

                if (respones.Content != null)
                {
                    string responseContent = await respones.Content.ReadAsStringAsync();
                    Debug.WriteLine(responseContent.Contains("Beklager").ToString() + "CONTENT:: " + responseContent);
                    Debug.WriteLine("Res: " + responseContent);
                    Debug.WriteLine(responseContent.GetType());
                    // If nothing is found, return
                    if (responseContent.Contains("Beklager"))
                    {
                        return;
                    }
                    else if (responseContent.Contains("Position outside area"))
                    {
                        return;
                    }
                    
                    Rootobject root = JsonConvert.DeserializeObject<Rootobject>(responseContent);
      
                    foreach(var item in root.navn)
                    {
                        GeoStedModel geoSted = new GeoStedModel()
                        {
                            Stedsnavn = item.Skrivemåte,
                            Fylkesnavn = item.Fylker[0].Fylkesnavn,
                            Lan = item.Representasjonspunkt.Øst.ToString(),
                            Lon = item.Representasjonspunkt.Nord.ToString(),
                            Navneobjekttype = item.Navneobjekttype
                        };
                        geoSted.SetPickerText();
                        myGeoStedList.Add(geoSted);
                    }

                    if (myGeoStedList.Count != 0)
                    {
                        Picker picker = new Picker
                        {
                            Title = "Velg sted(navn,kommune,fylke):",
                            VerticalOptions = LayoutOptions.CenterAndExpand,
                            ItemsSource = myGeoStedList,
                            ItemDisplayBinding = new Binding("PickerText")
                        };

                        picker.SelectedIndexChanged += (sender, args) =>
                        {
                            TestApi(double.Parse(myGeoStedList[picker.SelectedIndex].Lon, CultureInfo.InvariantCulture), double.Parse(myGeoStedList[picker.SelectedIndex].Lan, CultureInfo.InvariantCulture));
                            this.Content = bak;
                        };
                        picker.Unfocused += (sender, args) =>
                        {
                            this.Content = bak;
                        };

                        this.Content = picker;
                        picker.Focus();
                    }
                    else
                    {
                        await DisplayAlert("Info:", "Sted ikke funnet", "Ok");
                    }


                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Catching error from steds henting" + ex.Message);
                await DisplayAlert("Info:", "Sted ikke funnet", "Ok");
            }
        }




    }


    

    

    

    

}
