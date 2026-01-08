using System;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Threading;
using Wisej.Web;
//using static System.Net.Mime.MediaTypeNames;


namespace HighchartsMaps2
{
    public partial class Page1 : Page
    {
        public int[][] continentCoordinatesArray = new int[7][];//has 4 elements, each of which is a single-dimensional array of integers:
       
public int index = 0;
        public Page1()
        {
            InitializeComponent();
            continentCoordinatesArray[0] = new int[] { -21, 0 }; //africa
            continentCoordinatesArray[1] = new int[] { 5, -43 };//europe
            continentCoordinatesArray[2] = new int[] { -94, -27 };//asia
            continentCoordinatesArray[3] = new int[] { -108, 21 }; //australia
            continentCoordinatesArray[4] = new int[] { 50, -96 }; //arctic
            continentCoordinatesArray[5] = new int[] { 110, -33 };//north america
            continentCoordinatesArray[6] = new int[] { 57, 2 }; //south america

            //set the color textbox so it is the same color as the initial continents color (green)
            textBox2.Text = "#31784B";//max color dark green
            textBox3.Text = "#BFCFAD";//min color light green
            //note that this causes the textBox2_TextChanged event and textBox3_TextChanged to be fired when the page loads
            //but that was gonna be fired anyways- default color is black, so it was setting the continents color to black on page load.
        }

        private void Page1_Load(object sender, EventArgs e)
        {
            //add all the continent names to the combobox
            this.comboBox1.Items.AddRange(new[] { "Africa", "Arctic", "Asia", "Australia", "Europe", "North America", "South America" });

            this.widget1.Options.chart = new
            {
                map = getTopology()
            };
            this.widget1.Options.title = new
            {
                text = "Airport density per country",
                floating=true,
                align="left",
                style = new
                {
                    textOutline = "2px white"
                }
            };
            this.widget1.Options.subtitle = new
            {
                text = "Source: <a href=\"http://www.citypopulation.de/en/world/bymap/airports/\">citypopulation.de</a><br>" + "Click and drag to rotate globe<br>",
                floating = true,
                y=34,
                align="left"
            };
            this.widget1.Options.legend = new
            {
                enabled = false
            };
            

            this.widget1.Options.mapNavigation = new
            {
                enabled = true,
                enableDoubleClickZoomTo = true,
                buttonOptions = new
                {
                    verticalAlign= "bottom"
                }

            };
            this.widget1.Options.mapView = new
            {
                maxZoom = 30,
                projection = new
                {
                    name= "Orthographic",
                    rotation= new[] {60, -30}
            }
            };
            this.widget1.Options.colorAxis = new
            {
                tickPixelInterval = 100,
                minColor = "#BFCFAD",
                maxColor = "#31784B",
                max = 1000
            };
            this.widget1.Options.series = new dynamic[]
            {
                new
                {
                    name = "Graticule",
                    id = "graticule",
                    type = "mapline",
                    data = new dynamic[] { },
                    nullColor = "rgba(11,127,171,0.5)",
                    accessibility = new
                    {
                        enabled = false
                    },
                    enableMouseTracking= false
                },
                new
                {
                    data = getData(),
                    joinBy="name",
                    name="Airports per million km^2",
                    states = new
                    {
                        hover = new
                        {
                            color ="#a4edba", //green
                            borderColor="#000000" //black
                        }
                    },
                    dataLabels = new
                    {
                        enabled = false,
                        format = "{point.name}"
                    }
                }

            };
        }

        private object getTopology()
        {
            var json = File.ReadAllText(Application.MapPath("world.topo.json"));
            return JSON.Parse(json);
        }

        private object getData()
        {
            var json = File.ReadAllText(Application.MapPath("airportdata.json"));
            return JSON.Parse(json).airportData; //gotta parse it into an object, then we can access airportData from it
        }

        private void button4_Click(object sender, EventArgs e)
        {

            this.widget1.Options.title = new
            {
                text = textBox1.Text,
                floating = true,
                align = "left",
                style = new
                {
                    textOutline = "2px white"
                }
            };
        }

        public void SwitchContinent()
        {

            int[] coords = continentCoordinatesArray[index];//coordinates of the current continent to show
            index += 1;
            if(index > continentCoordinatesArray.Length -1)
            {
                index = 0;//reset the loop
            }

            //move the map to the coords of current continent
            this.widget1.Call("rotateMap", coords);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            //north america
            int[] rotation = new[] { 110, -33 };
            this.widget1.Call("rotateMap", rotation);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            //maximum color
            setContinentColor();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            //minimum color
            setContinentColor();
        }

        public void setContinentColor()
        {
            //change continent color
            this.widget1.Options.colorAxis = new
            {
                tickPixelInterval = 100,
                minColor = textBox3.Text,
                maxColor = textBox2.Text,
                max = 1000
            };
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            //change hover color

            //note: this is also reloading the graticule data (latitude and longitude lines)
            //and the map data (data about each country, ie number of airports
            this.widget1.Options.series = new dynamic[]
            {
                new
                {
                    name = "Graticule",
                    id = "graticule",
                    type = "mapline",
                    data = new dynamic[] { },
                    nullColor = "rgba(11,127,171,0.5",
                    accessibility = new
                    {
                        enabled = false
                    },
                    enableMouseTracking= false
                },
                new
                {
                    data = getData(),
                    joinBy="name",
                    name="Airports per million km^2",
                    states = new
                    {
                        hover = new
                        {
                            color = textBox4.Text,
                            borderColor="#000000" //black
                        }
                    },
                    dataLabels = new
                    {
                        enabled = false,
                        format = "{point.name}"
                    }
                }

            };
        }

        private bool autoRotate = false;
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //toggle autorotate on/off
            autoRotate = !autoRotate;

            if(autoRotate)
            {
                Application.StartTask(() =>
                {

                    while (autoRotate)
                    {

                        SwitchContinent(); //auto switch continents

                        // We are running out-of-bound and we need to push the updates
                        // to the client using Application.Update(). It can be called at any time
                        // and interval. It simply flushes the changed back to the client.
                        Application.Update(this);

                        Thread.Sleep(2000); //wait 2 seconds
                    }
                });
            }
        }

        private void comboBox1_SelectedItemChanged(object sender, EventArgs e)
        {

            //set to north america rotation
            int[] rotation = new[] { 110, -33 };


            switch (comboBox1.Text)
            {
                case "Africa":
                    rotation = new int[] { -21, 0 }; //africa
                    break;
                case "Arctic":
                    rotation = new int[] { 50, -96 }; //arctic
                    break;
                case "Asia":
                    rotation = new int[] { -94, -27 };//asia
                    break;
                case "Australia":
                    rotation = new int[] { -108, 21 }; //australia
                    break;
                case "Europe":
                    rotation = new int[] { 5, -43 };//europe
                    break;
                case "North America":
                    rotation = new int[] { 110, -33 };//north america
                    break;
                case "South America":
                    rotation = new int[] { 57, 2 }; //south america
                    break;
                default://set to north america by default
                    rotation = new[] { 110, -33 };
                    break;
            }
            this.widget1.Call("rotateMap", rotation);
        }
    }
}
