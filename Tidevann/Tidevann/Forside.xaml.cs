using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Tidevann
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Forside : ContentPage
    {
        HttpClient client = new HttpClient();
        public Forside()
        {
            InitializeComponent();
            BindingContext = this;
            TestApi();
        }

        private async void TestApi()
        {
            Debug.WriteLine("*******************************TestApi*******************");
            try
            {
                string connectionString = "http://api.sehavniva.no/tideapi.php?lat=58.974339&lon=5.730121&fromtime=2021-04-12T00%3A00&totime=2021-04-13T00%3A00&datatype=tab&refcode=msl&place=&file=&lang=nn&interval=10&dst=0&tzone=&tide_request=locationdata";
                var respones = await client.GetAsync(connectionString);
                respones.EnsureSuccessStatusCode();

                if (respones.Content != null)
                {
                    string responseContent = await respones.Content.ReadAsStringAsync();
                    Debug.WriteLine("CONTENT:: " + responseContent);
                } else
                {
                    Debug.WriteLine("Response = null");
                }
            } catch (Exception ex)
            {
                Debug.WriteLine("CONTENT:: " + ex.Message);
            }
            
        }
    }
}