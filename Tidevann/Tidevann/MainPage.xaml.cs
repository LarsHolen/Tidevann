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

namespace Tidevann
{
    public partial class MainPage : ContentPage
    {
        
        readonly HttpClient client = new HttpClient();
       
        //private string lokasjon = "Lokasjon";

        readonly List<Location> locList = new List<Location>();


        private List<TidevannModel> myTidevannModelList = new List<TidevannModel>();
        private List<GeoStedModel> myGeoStedList = new List<GeoStedModel>();
       

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            // Loads predefined names/coords for the pickerlist
            LoadLocs();

            // Adding select handler for the picker list
            LocationPicker.SelectedIndexChanged += SelectedLocChange;
        }

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

        private void LoadLocs()
        {
            // Location names and coordiantes for the picker list
            locList.Add(new Location("Vadsø", 70.08, 29.73));
            locList.Add(new Location("Honningsvåg", 70.98, 25.97));
            locList.Add(new Location("Tromsø", 69.69, 18.98));
            locList.Add(new Location("Andøya", 69.08, 15.76));
            locList.Add(new Location("Narvik", 68.44, 17.48));
            locList.Add(new Location("Røst", 67.52, 12.11));
            locList.Add(new Location("Bodø", 67.27, 14.44));
            locList.Add(new Location("Sandnessjøen", 66.02, 12.62));
            locList.Add(new Location("Brønnøysund", 65.48, 12.19));
            locList.Add(new Location("Namsos", 64.46, 11.51));
            locList.Add(new Location("Hitra", 63.56, 8.73));
            locList.Add(new Location("Verdal", 63.79, 11.44));
            locList.Add(new Location("Kristiansund", 63.11, 7.73));
            locList.Add(new Location("Sunndalsøra", 62.68, 8.55));
            locList.Add(new Location("Ålesund", 62.48, 6.14));
            locList.Add(new Location("Måløy", 61.93, 5.10));
            locList.Add(new Location("Florø", 61.59, 5.04));
            locList.Add(new Location("Høyanger", 61.21, 6.07));
            locList.Add(new Location("Gulen", 60.98, 5.08));
            locList.Add(new Location("Bergen", 60.40, 5.31));
            locList.Add(new Location("Bømlo", 59.80, 5.24));
            locList.Add(new Location("Haugesund", 59.40, 5.27));
            locList.Add(new Location("Stavanger", 58.97, 5.74));

            //FJERN FLEKKEFJORD
            locList.Add(new Location("Flekkefjord", 58.29, 6.66));


            locList.Add(new Location("Lindesnes", 57.98, 7.05));
            locList.Add(new Location("Kristiansand", 58.15, 8.03));
            locList.Add(new Location("Arendal", 58.46, 8.78));
            locList.Add(new Location("Kragerø", 58.86, 9.41));
            locList.Add(new Location("Sandefjord", 59.13, 10.22));
            locList.Add(new Location("Horten", 59.41, 10.48));
            locList.Add(new Location("Oslo", 59.91, 10.73));
            locList.Add(new Location("Fredrikstad", 59.21, 10.91));
           

            LocationPicker.ItemDisplayBinding = new Binding("Name");
            LocationPicker.ItemsSource = locList;
          
        }

       
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

        private void GpsButtonOn(bool v)
        {
            if(v)
            {
                act.IsRunning = false;
                act.IsVisible = false;
                gpsBtn.IsVisible = true;
            } else
            {
                gpsBtn.IsVisible = false;
                act.IsRunning = true;
                act.IsVisible = true;
            }
            

        }

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

        private async void TestApi(double lan, double lon)
        {
            // Loads tide data from coordinates
            try
            {
                // adding an custom item to the list that show "loading" text in the list
                myTidevannModelList = new List<TidevannModel>();
                myListView.ItemsSource = myTidevannModelList;
                myListView.Header = MyHeader("Laster inn tidevanns data");

                // Getting time for yesterday, and in 7 days, to retrive data for one week
                DateTime now = DateTime.Now.Date.AddDays(-0.5);
                DateTime oneWeek = DateTime.Now.AddDays(6);
                string connectionString = "https://api.sehavniva.no/tideapi.php?lat=" + lan.ToString() + "&lon=" + lon.ToString() + "&fromtime=" + now.ToString("u") + "&totime=" + oneWeek.ToString("u") +"&datatype=tab&refcode=msl&place=&file=&lang=nn&interval=10&dst=0&tzone=&tide_request=locationdata";
                
                var respones = await client.GetAsync(connectionString);
                

                respones.EnsureSuccessStatusCode();

                if (respones.Content != null)
                {
                    string responseContent = await respones.Content.ReadAsStringAsync();
                    // if responseContent contain the word "Beklager", we did not get any tide data.  Same with "Position outside area"
                    if(responseContent.Contains("Beklager"))
                    {
                        LocationPicker.Title = "Velg sted!";
                        responseContent = "";
                        myTidevannModelList = new List<TidevannModel>();
                        myListView.ItemsSource = myTidevannModelList;
                        myListView.Header = MyHeader("Det finnes ikke data for dette området, eller det mangler koordinater.");
                        return;
                    } else if(responseContent.Contains("Position outside area"))
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
                    XmlNodeList tide  = xDoc.GetElementsByTagName("waterlevel");
                    XmlNode docNode = xDoc.DocumentElement;
                    if(docNode.OuterXml == "<error>Position outside area</error>")
                    {
                        myListView.Header = MyHeader("Position utenfor APIens område/norskekysten, tap her for å velge fra liste.");
                        return;
                    }
                    myTidevannModelList = new List<TidevannModel>();
                    foreach (XmlElement ti in tide)
                    {
                        Debug.WriteLine("*******Høy/lav: " + ti.GetAttribute("flag") + "        Time: " + ti.GetAttribute("time") + "                 Vannnivå i forhold til middel:" + ti.GetAttribute("value"));
                        TidevannModel t = new TidevannModel()
                        {
                            Flag = ti.GetAttribute("flag"),
                            Verdi = ti.GetAttribute("value"),
                            Tid = ti.GetAttribute("time")
                        };
                        myTidevannModelList.Add(t);

                    }
                    myListView.ItemsSource = myTidevannModelList;
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
                HorizontalOptions = LayoutOptions.CenterAndExpand
            };
            f.Content = new Label()
            {
                Text =  s,
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.Fill,
                FontSize = 12,
                TextColor = Color.Black,
                FontAttributes = FontAttributes.Bold,
                BackgroundColor = Color.GhostWhite,
                Padding = 5
            };
            return f;
        }

        private void TideTapped(object sender, ItemTappedEventArgs e)
        {
            if(e.Item is TidevannModel t)
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
            if(!string.IsNullOrEmpty(stedsNavn)) GetLocNameFromGeodata(stedsNavn);
        }

        private async void GetLocNameFromGeodata(string stedsNavn)
        {
            // Finner en liste med steder med stringens navn fra kartverket
            var bak = this.Content;
            myGeoStedList = new List<GeoStedModel>();
            try
            {
                string connectionString = "https://ws.geonorge.no/SKWS3Index/ssr/json/sok?navn=" + stedsNavn + "&maxAnt=10&eksakteForst=true&epsgKode=4258";
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
                    responseContent = responseContent.Replace("},{", "}|{");
                    int pFrom = responseContent.IndexOf("[")+1;
                    int pTo = responseContent.LastIndexOf("]");
                    string result = responseContent.Substring(pFrom, pTo - pFrom);
                    string[] resultList = result.Split(separator: '|');
                    Debug.WriteLine("resultlist 0: " + resultList[0]);
                    foreach (string sted in resultList)
                    {
                        GeoStedModel geoSted = JsonConvert.DeserializeObject<GeoStedModel>(sted);
                        geoSted.SetPickerText();
                        myGeoStedList.Add(geoSted);
                    }
                    //
                    if (myGeoStedList.Count != 0)
                    {
                        Picker picker = new Picker
                        {
                            Title = "Velg sted(navn,kommune,fylke):",
                            VerticalOptions = LayoutOptions.CenterAndExpand
                        };
                        picker.ItemDisplayBinding = new Binding("PickerText");
                        picker.ItemsSource = myGeoStedList;
                        picker.SelectedIndexChanged += (sender, args) =>
                        {
                            Debug.WriteLine("Selected: " + args.ToString());
                            Debug.WriteLine("Selected: " + myGeoStedList[picker.SelectedIndex].Stedsnavn);
                            Debug.WriteLine("Resonse: " + myGeoStedList[picker.SelectedIndex].Lan + "      " + myGeoStedList[picker.SelectedIndex].Lon);
                            Debug.WriteLine("Resonse Doubles: " + double.Parse(myGeoStedList[picker.SelectedIndex].Lan, CultureInfo.InvariantCulture).ToString() + "      " + double.Parse(myGeoStedList[picker.SelectedIndex].Lon, CultureInfo.InvariantCulture).ToString());
                            //Debug.WriteLine("Resonse: " + Convert.ToDouble(myGeoStedList[picker.SelectedIndex].Lan) + "      " + Convert.ToDouble(myGeoStedList[picker.SelectedIndex].Lon));
                            TestApi(double.Parse(myGeoStedList[picker.SelectedIndex].Lan, CultureInfo.InvariantCulture), double.Parse(myGeoStedList[picker.SelectedIndex].Lon, CultureInfo.InvariantCulture));
                            //picker.Unfocus();
                            //picker = null;
                            this.Content = bak;
                        };

                        this.Content = picker;
                        picker.Focus();
                        Debug.WriteLine("Resonse: " + responseContent);
                    }
                    else 
                    {
                        
                    }

                   
                }

            } catch(Exception ex)
            {
                Debug.WriteLine("Catching error from steds henting" + ex.Message);
                await DisplayAlert("Info:", "Sted ikke funnet", "Ok");
            }
        }

        private async void GpsClick(object sender, EventArgs e)
        {
            await TestGPSAsync();
        }

        
    }
}
