using PredmetniZadatak2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace PredmetniZadatak2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Dictionary<long, Substation> stanice = new Dictionary<long, Substation>();
        List<LineEntity> linije = new List<LineEntity>();
        Dictionary<long, Node> cvorovi = new Dictionary<long, Node>();
        Dictionary<long, Switch> prekidaci = new Dictionary<long, Switch>();

        double maxX = double.MinValue, maxY = double.MinValue, minX = double.MaxValue, minY = double.MaxValue;
   
        private static int[] row = { -1,  0, 0, 1 };
        private static int[] col = {  0, -1, 1, 0 };
        private static string putanja = string.Empty;
        private static List<Tuple<long, long>> aleksandar = new List<Tuple<long, long>>();

        private static bool prviProlaz = true;

        private static string StarToolTip = string.Empty;
        private static string EndToolTip = string.Empty;
        private static Podeok[,] podeoci;

        
        public MainWindow()
        {
            InitializeComponent();
            podeoci = new Podeok[200, 200];
            InitPodeoci();
            LoadDataFromXml();
            CrtajCvorove();
            NacrtajVodove();
            DataContext = this.DataContext;
        }

        public void NacrtajVodove()
        {
            
            List<LineEntity> indexiZaBrisanjeLinija = new List<LineEntity>();
            LineEntity najkraca = linije.Find(linija => linija.StartNodeID.Equals(37349) && linija.EndNodeID.Equals(37403));
            if (najkraca != null)
            {
                if (NadjiPutanjuIspisi(najkraca))
                {
                    linije.Remove(najkraca);
                }
            }

            for (int i = 0; i < linije.Count; i++)
            {
                if (NadjiPutanjuIspisi(linije[i]))
                {
                    indexiZaBrisanjeLinija.Add(linije[i]);
                }
            }

            foreach (LineEntity li in indexiZaBrisanjeLinija)
            {
                linije.Remove(li);
            }

            prviProlaz = false;

            indexiZaBrisanjeLinija.Clear();
            for (int i = 0; i < linije.Count; i++)
            {
                if (NadjiPutanjuIspisi(linije[i]))
                {
                    indexiZaBrisanjeLinija.Add(linije[i]);
                }
            }

            foreach (LineEntity li in indexiZaBrisanjeLinija)
            {
                linije.Remove(li);
            }

        }

        private void InitPodeoci()
        {
            for (int i = 0; i < podeoci.GetLength(0); i++)
            {
                for (int j = 0; j < podeoci.GetLength(1); j++)
                {
                    podeoci[i, j] = new Podeok();
                    
                }
            }
        }

        private void CrtajCvorove()
        {
            int MatricaX, MatricaY;
            int KanvasX, KanvasY;
            foreach (Node sub in cvorovi.Values)
            {
                Ellipse point = new Ellipse();
                point.Height = 3;
                point.Width = 5;
                point.Fill = Brushes.Lime;
                point.Stroke = Brushes.Lime/*Chocolate*/;
                point.ToolTip = "Node ID:" + sub.ID + " Name:" + sub.Name;

                int x = (int)(IzracunajX(sub.X, maxX, minX) / 5);
                int y = (int)(IzracunajY(sub.Y, maxY, minY) / 3);
                if (x == podeoci.GetLength(0))
                {
                    x--;
                }
                if (y == podeoci.GetLength(1))
                {
                    y--;
                }
                PronadjiMestoCvoru(x, y, sub.ID, out KanvasX, out KanvasY, out MatricaX, out MatricaY);

                kanvas.Children.Add(point);
                sub.X = MatricaX; //zbog crtanja vodova
                sub.Y = MatricaY; //zbog crtanja vodova

                Canvas.SetLeft(point, KanvasX);
                Canvas.SetBottom(point, KanvasY);

            }

            foreach (Switch sub in prekidaci.Values)
            {
                Ellipse point = new Ellipse();
                point.Height = 3;
                point.Width = 5;
                point.Fill = Brushes.Blue;
                point.Stroke = Brushes.Blue/*PaleVioletRed*/;
                point.ToolTip = "Switch ID:" + sub.ID + " Name:" + sub.Name;

                int x = (int)(IzracunajX(sub.X, maxX, minX) / 5);
                int y = (int)(IzracunajY(sub.Y, maxY, minY) / 3);
                if (x == podeoci.GetLength(0))
                {
                    x--;
                }
                if (y == podeoci.GetLength(1))
                {
                    y--;
                }
                PronadjiMestoCvoru(x, y, sub.ID, out KanvasX, out KanvasY, out MatricaX, out MatricaY);

                kanvas.Children.Add(point);
                sub.X = MatricaX; //zbog crtanja vodova
                sub.Y = MatricaY; //zbog crtanja vodova

                Canvas.SetLeft(point, KanvasX);
                Canvas.SetBottom(point, KanvasY);


            }

            foreach (Substation sub in stanice.Values)
            {
                Ellipse point = new Ellipse();

                point.Height = 3;
                point.Width = 5;
                point.Fill = Brushes.Red;
                point.Stroke = Brushes.Red;
                point.ToolTip = "Substation ID:" + sub.ID + " Name:" + sub.Name;/* + " Broj:" + kanvas.Children.Count; */

                int x = (int)(IzracunajX(sub.X, maxX, minX) / 5);
                int y = (int)(IzracunajY(sub.Y, maxY, minY) / 3);
                if (x == podeoci.GetLength(0))
                {
                    x--;
                }
                if (y == podeoci.GetLength(1))
                {
                    y--;
                }
                PronadjiMestoCvoru(x, y, sub.ID, out KanvasX, out KanvasY, out MatricaX, out MatricaY);

                kanvas.Children.Add(point);
                sub.X = MatricaX; //zbog crtanja vodova
                sub.Y = MatricaY; //zbog crtanja vodova

                Canvas.SetLeft(point, KanvasX);
                Canvas.SetBottom(point, KanvasY);

            }
        }

        private void PronadjiMestoCvoru(int x, int y, long cvorID, out int KanvasX, out int KanvasY, out int MatricaX, out int MatricaY)
        {
            KanvasX = -1; MatricaX = -1;
            KanvasY = -1; MatricaY = -1;
            if (!podeoci[x, y].Zauzet)
            {
                podeoci[x, y].CvorID = cvorID;
                podeoci[x, y].Zauzet = true;

                KanvasX = x * 5;
                KanvasY = y * 3;
                MatricaX = x;
                MatricaY = y;

            }
            else
            {
                bool upisan = false;
                int pocetnoX = x;
                int pocetnoY = y;
                int korak = 1;

                while (!upisan)
                {
                    List<Tuple<int,int>> dobijeniIndeksi = DobaviSusedne(pocetnoX, pocetnoY, korak);

                    foreach (Tuple<int,int> koordinate in dobijeniIndeksi)
                    {
                        if (koordinate.Item1 >= 0 && koordinate.Item1 < podeoci.GetLength(0) && koordinate.Item2 >= 0 && koordinate.Item2 < podeoci.GetLength(1))
                        {
                            if (!podeoci[koordinate.Item1, koordinate.Item2].Zauzet)
                            {
                                upisan = true;
                                podeoci[koordinate.Item1, koordinate.Item2].CvorID = cvorID;
                                podeoci[koordinate.Item1, koordinate.Item2].Zauzet = true;
                                
                                KanvasX = koordinate.Item1 * 5;
                                KanvasY = koordinate.Item2 * 3;
                                MatricaX = koordinate.Item1;
                                MatricaY = koordinate.Item2;
                                break;
                            }
                        }
                    }

                    korak++;
                }
            }
        }

        private List<Tuple<int,int>> DobaviSusedne(int pocetnoX, int pocetnoY, int korak)
        {
            List<Tuple<int, int>> dobijeniIndeksi = new List<Tuple<int, int>>();

            for (int i = korak - 1; i > -korak; i--)
            {
                dobijeniIndeksi.Add(new Tuple<int, int>(pocetnoX + korak, pocetnoY + i)); 
                dobijeniIndeksi.Add(new Tuple<int, int>(pocetnoX + i,     pocetnoY - korak));
                dobijeniIndeksi.Add(new Tuple<int, int>(pocetnoX - korak, pocetnoY + i));
                dobijeniIndeksi.Add(new Tuple<int, int>(pocetnoX + i,     pocetnoY + korak));
            }

            dobijeniIndeksi.Add(new Tuple<int, int>(pocetnoX + korak, pocetnoY + korak ));
            dobijeniIndeksi.Add(new Tuple<int, int>(pocetnoX + korak, pocetnoY - korak ));
            dobijeniIndeksi.Add(new Tuple<int, int>(pocetnoX - korak, pocetnoY + korak ));
            dobijeniIndeksi.Add(new Tuple<int, int>(pocetnoX - korak, pocetnoY - korak ));

            return dobijeniIndeksi;
        }

        private double IzracunajX(double x, double maxX, double minX)
        {
            double izlaz = 0;

            izlaz = kanvas.Width / ((maxX - minX) / (x - minX));

            return izlaz;
        }

        private double IzracunajY(double y, double maxY, double minY)
        {
            double izlaz = 0;

            izlaz = kanvas.Height / ((maxY - minY) / (y - minY));

            return izlaz;
        }

        public bool NacrtajVod(LineEntity aca)
        {
            if (aleksandar.Count > 0)
            {
                // Dodaj u matricu vod koji ce da se iscrta kako ne bi dolazilo do preseka vodova
                foreach (Tuple<long, long> koordinate in aleksandar)
                {
                    
                    //drugi prolaz je u pitanju
                    if (!prviProlaz)
                    {   //vec je iscratana linija na toj poziciji
                        if (podeoci[koordinate.Item1, koordinate.Item2].CvorID == 1)
                        {
                            //tacka preseka
                            Ellipse tacka = new Ellipse();
                            tacka.Width = 1;
                            tacka.Height = 1;
                            tacka.Fill = Brushes.White;
                            tacka.Stroke = Brushes.White;
                            kanvas.Children.Add(tacka);
                            Canvas.SetLeft(tacka, koordinate.Item1 * 5 + 2);
                            Canvas.SetBottom(tacka, koordinate.Item2 * 3 + 1);
                            Canvas.SetZIndex(tacka, 1);
                        }
                        else
                        {
                            //ako je zauzet na tom mestu je cvor, nemoj upisivati 1 na mesto gde se nalazi CvorID jer onda BFS nece moci da nadje
                            // putanje za sve Vodove
                            if(!podeoci[koordinate.Item1,koordinate.Item2].Zauzet)
                                podeoci[koordinate.Item1, koordinate.Item2].CvorID = 1;
                        }
                    }
                    else
                    {
                        //ako je zauzet na tom mestu je cvor, nemoj upisivati 1 na mesto gde se nalazi CvorID jer onda BFS nece moci da nadje
                        // putanje za sve Vodove
                        if (!podeoci[koordinate.Item1, koordinate.Item2].Zauzet)
                            podeoci[koordinate.Item1, koordinate.Item2].CvorID = 1;
                    }
                } 

                var prvi = aleksandar[0];
                aleksandar = aleksandar.Skip(1).ToList();
                List<Tuple<long, long>> temp = new List<Tuple<long, long>>();
                while (aleksandar.Count != 0)
                {


                    temp.Clear();
                    Line linija = new Line();

                    linija.X1 = prvi.Item1 * 5 + 2.5;
                    linija.Y1 = (podeoci.GetLength(1) - prvi.Item2) * 3 - 1.5; //podeoci.GetLength(1) zato sto si cvorove u Canvas dodavao kao Canvas.SetBottom(element,y)

                    linija.Fill = Brushes.Black;
                    linija.Stroke = Brushes.Black;

                    linija.StrokeThickness = 1;
                    linija.HorizontalAlignment = HorizontalAlignment.Left;
                    linija.VerticalAlignment = VerticalAlignment.Center;

                    temp.AddRange(aleksandar.TakeWhile(tuple => tuple.Item1.Equals(prvi.Item1) && !tuple.Item2.Equals(prvi.Item2)));

                    if (temp.Count == 0)
                    {
                        temp.AddRange(aleksandar.TakeWhile(tuple => tuple.Item2.Equals(prvi.Item2) && !tuple.Item1.Equals(prvi.Item1)));

                        if (temp.Count == 0)
                        {
                            //nije ni po x ni po y jednako
                            // moraju 2 linije, jer se onda tacke nalaze dijagonalno
                            Tuple<long, long> drugi = aleksandar[0];
                            linija.X2 = prvi.Item1;
                            linija.Y2 = drugi.Item2;
                            linija.ToolTip = "ID: " + aca.ID + " Name: " + aca.Name  /*+ " |" + aca.StartNodeID + "-->" + aca.EndNodeID*/;
                            linija.Uid = aca.StartNodeID.ToString() + ";" + aca.EndNodeID.ToString();

                            Line linija2 = new Line();
                            linija.Fill = Brushes.Black;
                            linija.Stroke = Brushes.Black;
                            linija.StrokeThickness = 1;
                            linija.HorizontalAlignment = HorizontalAlignment.Left;
                            linija.VerticalAlignment = VerticalAlignment.Center;
                            linija2.X1 = prvi.Item1;
                            linija2.Y1 = drugi.Item2;
                            linija2.X2 = drugi.Item1 /*+ 2.5*/;
                            linija2.Y2 = drugi.Item2 /*+ 2.5*/;
                            linija2.ToolTip = "ID: " + aca.ID + " Name: " + aca.Name  /*+ " |" + aca.StartNodeID + "-->" + aca.EndNodeID*/;
                            linija2.Uid = aca.StartNodeID.ToString() + ";" + aca.EndNodeID.ToString();

                            kanvas.Children.Add(linija);
                            kanvas.Children.Add(linija2);
                            //prolaz kroz matricu kako bi popunili ove linije da ne bi dolazilo do preseka
                            //prolaz na pocetku metode nece dodati 1 u matricu ako je putanja dijagonalna


                            for (long j = Math.Min(prvi.Item2, drugi.Item2); j <= Math.Max(prvi.Item2, drugi.Item2); j++)
                            {
                                //prvi.item1 jer crtamo po njegovom X-u vertiaklu
                                podeoci[prvi.Item1, j].CvorID = 1;
                            }

                            for (long i = Math.Min(prvi.Item1, drugi.Item1); i <= Math.Max(prvi.Item1, drugi.Item1); i++)
                            {
                                //drugi.item2 jer crtamo po njegovom Y-u horizontalu
                                podeoci[i, drugi.Item2].CvorID = 1; //bilo je 2 ko zna zasto, verovatno greska;
                            }



                            prvi = drugi;
                            aleksandar.Remove(drugi);
                            continue;
                        }
                    }


                    linija.X2 = temp.Last().Item1 * 5 + 2.5;
                    linija.Y2 = (podeoci.GetLength(1) - temp.Last().Item2) * 3 - 1.5; //podeoci.GetLength(1) zato sto si cvorove u Canvas dodavao kao Canvas.SetBottom(element,y)

                    linija.ToolTip = "ID: " + aca.ID + " Name: " + aca.Name + " |" + aca.StartNodeID + "-->" + aca.EndNodeID;
                    linija.Uid = aca.StartNodeID.ToString() + ";" + aca.EndNodeID.ToString();

                    linija.MouseRightButtonDown += OnMouseRightButtonDown;


                    kanvas.Children.Add(linija);

                    prvi = temp.Last();

                    aleksandar.RemoveAll(item => temp.Contains(item));

                }

                return true;
            }
            return false;
        }

        public bool NadjiPutanjuIspisi(LineEntity aca)
        {
            aleksandar.Clear();
            Cvor aleksa = null;
            if (stanice.Keys.Contains(aca.StartNodeID))
            {
                aleksa = FindPath((long)stanice[aca.StartNodeID].X, (long)stanice[aca.StartNodeID].Y, aca.EndNodeID);
            }
            else if (cvorovi.Keys.Contains(aca.StartNodeID))
            {
                aleksa = FindPath((long)cvorovi[aca.StartNodeID].X, (long)cvorovi[aca.StartNodeID].Y, aca.EndNodeID);
            }
            else if (prekidaci.Keys.Contains(aca.StartNodeID))
            {
                aleksa = FindPath((long)prekidaci[aca.StartNodeID].X, (long)prekidaci[aca.StartNodeID].Y, aca.EndNodeID);
            }

            if (aleksa != null)
            {
                PrintPath(aleksa);
                if (NacrtajVod(aca)) return true;
            }

            return false;



        }

        private void LoadDataFromXml()
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load("Geographic.xml");

            XmlNodeList xmlNodeList;

            xmlNodeList = xmlDocument.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity");

         
            double noviX, noviY;

            foreach (XmlNode xmlNode in xmlNodeList)
            {
                Substation substation = new Substation();
                substation.ID = long.Parse(xmlNode.SelectSingleNode("Id").InnerText);
                substation.Name = xmlNode.SelectSingleNode("Name").InnerText;
                substation.X = double.Parse(xmlNode.SelectSingleNode("X").InnerText);
                substation.Y = double.Parse(xmlNode.SelectSingleNode("Y").InnerText);

                //ToLatLon(substation.X, substation.Y, 34, out noviX, out noviY);
                //substation.X = noviX;
                //substation.Y = noviY;
                noviX = substation.X;
                noviY = substation.Y;


                if (maxX < noviX) maxX = noviX;
                if (maxY < noviY) maxY = noviY;

                if (minX > noviX) minX = noviX;
                if (minY > noviY) minY = noviY;

                stanice.Add(substation.ID, substation);
               



            }

            xmlNodeList = xmlDocument.DocumentElement.SelectNodes("/NetworkModel/Nodes/NodeEntity");
           
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                Node node = new Node();
                node.ID = long.Parse(xmlNode.SelectSingleNode("Id").InnerText);
                node.Name = xmlNode.SelectSingleNode("Name").InnerText;
                node.X = double.Parse(xmlNode.SelectSingleNode("X").InnerText);
                node.Y = double.Parse(xmlNode.SelectSingleNode("Y").InnerText);

                //ToLatLon(node.X, node.Y, 34, out noviX, out noviY);
                //node.X = noviX;
                //node.Y = noviY;
                noviX = node.X;
                noviY = node.Y;
                if (maxX < noviX) maxX = noviX;
                if (maxY < noviY) maxY = noviY;

                if (minX > noviX) minX = noviX;
                if (minY > noviY) minY = noviY;



                cvorovi.Add(node.ID, node);

            }

            

            xmlNodeList = xmlDocument.DocumentElement.SelectNodes("/NetworkModel/Switches/SwitchEntity");

            foreach (XmlNode xmlNode in xmlNodeList)
            {
                Switch prekidac = new Switch();
                prekidac.ID = long.Parse(xmlNode.SelectSingleNode("Id").InnerText);
                prekidac.Name = xmlNode.SelectSingleNode("Name").InnerText;
                prekidac.X = double.Parse(xmlNode.SelectSingleNode("X").InnerText);
                prekidac.Y = double.Parse(xmlNode.SelectSingleNode("Y").InnerText);

                //ToLatLon(prekidac.X, prekidac.Y, 34, out noviX, out noviY);
                //prekidac.X = noviX;
                //prekidac.Y = noviY;
                noviX = prekidac.X;
                noviY = prekidac.Y;
                if (maxX < noviX) maxX = noviX;
                if (maxY < noviY) maxY = noviY;

                if (minX > noviX) minX = noviX;
                if (minY > noviY) minY = noviY;

                prekidaci.Add(prekidac.ID, prekidac);

            }


            xmlNodeList = xmlDocument.DocumentElement.SelectNodes("/NetworkModel/Lines/LineEntity");

            foreach (XmlNode xmlNode in xmlNodeList)
            {
                LineEntity vod = new LineEntity();
                vod.ID = long.Parse(xmlNode.SelectSingleNode("Id").InnerText);
                vod.Name = xmlNode.SelectSingleNode("Name").InnerText;
                vod.StartNodeID = long.Parse(xmlNode.SelectSingleNode("FirstEnd").InnerText);
                vod.EndNodeID = long.Parse(xmlNode.SelectSingleNode("SecondEnd").InnerText);

                if ((prekidaci.Keys.Contains(vod.StartNodeID) || stanice.Keys.Contains(vod.StartNodeID) || cvorovi.Keys.Contains(vod.StartNodeID)) &&
                    (prekidaci.Keys.Contains(vod.EndNodeID) || stanice.Keys.Contains(vod.EndNodeID) || cvorovi.Keys.Contains(vod.EndNodeID)))
                {
                    if (!linije.Any(item => item.StartNodeID.Equals(vod.EndNodeID) && item.EndNodeID.Equals(vod.StartNodeID)))
                    {
                        linije.Add(vod);
                    }

                }

            }

        }
        
        //From UTM to Latitude and longitude in decimal
        public static void ToLatLon(double utmX, double utmY, int zoneUTM, out double latitude, out double longitude)
        {
            bool isNorthHemisphere = true;

            var diflat = -0.00066286966871111111111111111111111111;
            var diflon = -0.0003868060578;

            var zone = zoneUTM;
            var c_sa = 6378137.000000;
            var c_sb = 6356752.314245;
            var e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
            var e2cuadrada = Math.Pow(e2, 2);
            var c = Math.Pow(c_sa, 2) / c_sb;
            var x = utmX - 500000;
            var y = isNorthHemisphere ? utmY : utmY - 10000000;

            var s = ((zone * 6.0) - 183.0);
            var lat = y / (c_sa * 0.9996);
            var v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
            var a = x / v;
            var a1 = Math.Sin(2 * lat);
            var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
            var j2 = lat + (a1 / 2.0);
            var j4 = ((3 * j2) + a2) / 4.0;
            var j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
            var alfa = (3.0 / 4.0) * e2cuadrada;
            var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
            var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
            var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
            var b = (y - bm) / v;
            var epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
            var eps = a * (1 - (epsi / 3.0));
            var nab = (b * (1 - epsi)) + lat;
            var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
            var delt = Math.Atan(senoheps / (Math.Cos(nab)));
            var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

            longitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
            latitude = ((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
        }
        
        public Cvor FindPath(long x, long y, long destID)
        {
            Queue<Cvor> q = new Queue<Cvor>();
            Cvor source = new Cvor(x, y, null);
            q.Enqueue(source);

            string key = source.X + ", " + source.Y;
            HashSet<string> visited = new HashSet<string>();
            visited.Add(key);

            while (!(q.Count == 0))
            {
                Cvor current = q.Dequeue();
                long i = current.X, j = current.Y;
                //nasao odredisni cvor
                if (podeoci[i, j].CvorID == destID)
                {
                    return current;
                }


                
                for (int k = 0; k < 4; k++)
                {
                    x = i + row[k];
                    y = j + col[k];


                    if (IsValid(x, y))
                    {
                        Cvor sledeci = new Cvor(x, y, current);
                        key = sledeci.X + ", " + sledeci.Y;
                        //ako nije posecen
                        if (!visited.Contains(key))
                        {
                            q.Enqueue(sledeci);
                            visited.Add(key);
                        }
                    }
                }


            }
            return null;
        }

        private static void PrintPath(Cvor cvor)
        {
            if (cvor == null)
            {
                return;
            }

            PrintPath(cvor.Roditelj);
            aleksandar.Add(new Tuple<long, long>(cvor.X, cvor.Y));
            //return putanja += cvor.ToString() + " ";
            return;
        }

        private static bool IsValid(long x, long y)
        {
            if (prviProlaz)
                return (x >= 0 && x < podeoci.GetLength(0)) && (y >= 0 && y < podeoci.GetLength(1)) && podeoci[x, y].CvorID != 1;
            else
                return (x >= 0 && x < podeoci.GetLength(0)) && (y >= 0 && y < podeoci.GetLength(1) /*&& podeoci[x, y].CvorID != 1*/);
        }

        private void OnMouseRightButtonDown(object sender, RoutedEventArgs e)
        {
            Line linija = sender as Line;
            string[] delovi = linija.Uid.Split(';');
            long startID = long.Parse(delovi[0]);
            long endID = long.Parse(delovi[1]);
            string start = "";
            string end = "";

            bool startNasao = false, endNasao = false;

            if (StarToolTip != string.Empty && EndToolTip != string.Empty)
            {
                foreach (var item in kanvas.Children.OfType<Ellipse>())
                {
                    if (item.ToolTip.Equals(StarToolTip))
                    {
                        if (item.ToolTip.ToString().Contains("Substation"))
                        {
                            item.Fill = Brushes.Red;
                            item.Stroke = Brushes.Red;
                        }
                        else if (item.ToolTip.ToString().Contains("Node"))
                        {
                            item.Fill = Brushes.Lime;
                            item.Stroke = Brushes.Lime;
                        }
                        else if (item.ToolTip.ToString().Contains("Switch"))
                        {
                            item.Fill = Brushes.Blue;
                            item.Stroke = Brushes.Blue;
                        }
                        startNasao = true;
                    }

                    if (item.ToolTip.Equals(EndToolTip))
                    {
                        if (item.ToolTip.ToString().Contains("Substation"))
                        {
                            item.Fill = Brushes.Red;
                            item.Stroke = Brushes.Red;
                        }
                        else if (item.ToolTip.ToString().Contains("Node"))
                        {
                            item.Fill = Brushes.Lime;
                            item.Stroke = Brushes.Lime;
                        }
                        else if (item.ToolTip.ToString().Contains("Switch"))
                        {
                            item.Fill = Brushes.Blue;
                            item.Stroke = Brushes.Blue;
                        }
                        endNasao = true;
                    }

                    if (startNasao && endNasao)
                    {
                        StarToolTip = string.Empty;
                        EndToolTip = string.Empty;
                        break;
                    }
                }
            }

            if (prekidaci.ContainsKey(startID))
            {
                start = "Switch ID:" + prekidaci[startID].ID + " Name:" + prekidaci[startID].Name;
            }
            else if (cvorovi.ContainsKey(startID))
            {
                start = "Node ID:" + cvorovi[startID].ID + " Name:" + cvorovi[startID].Name;
            }
            else if (stanice.ContainsKey(startID))
            {
                start = "Substation ID:" + stanice[startID].ID + " Name:" + stanice[startID].Name;
            }

            if (prekidaci.ContainsKey(endID))
            {
                end = "Switch ID:" + prekidaci[endID].ID + " Name:" + prekidaci[endID].Name;
            }
            else if (cvorovi.ContainsKey(endID))
            {
                end = "Node ID:" + cvorovi[endID].ID + " Name:" + cvorovi[endID].Name;
            }
            else if (stanice.ContainsKey(endID))
            {
                end = "Substation ID:" + stanice[endID].ID + " Name:" + stanice[endID].Name;
            }
            startNasao = false; endNasao = false;
            foreach (var item in kanvas.Children.OfType<Ellipse>())
            {
                if (item.ToolTip.ToString().Equals(start))
                {
                    item.Fill = Brushes.Chocolate;
                    item.Stroke = Brushes.Chocolate;
                    startNasao = true;
                    StarToolTip = item.ToolTip.ToString();
                }
                else if (item.ToolTip.ToString().Equals(end))
                {
                    item.Fill = Brushes.Chocolate;
                    item.Stroke = Brushes.Chocolate;
                    endNasao = true;
                    EndToolTip = item.ToolTip.ToString();
                }

                if (startNasao && endNasao)
                {
                    break;
                }
            }

        }
    }
}
