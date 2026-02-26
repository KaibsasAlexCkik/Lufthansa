using System;
using System.Collections.Generic;
using System.IO;

namespace Lufthansa {
    public struct csomag {
        public string azon;
        public double suly;
        public double terfogat;
    }

    public struct csaladicsomag {
        public string csaladazon;
        public double osszsuly;
        public double osszterfogat;
        public List<csomag> csomagok;
    }

    public struct kontener {
        public int azon; 
        public int erokar;
        public double suly;
        public double terfogat;
        public List<csaladicsomag> csomagok;
    }

    internal class Program {
        static Dictionary<string, csaladicsomag> beolvas(string fajlnev) {
            Dictionary<string, csaladicsomag> csaladicsomagok = new Dictionary<string, csaladicsomag>();
            StreamReader sr = new StreamReader(fajlnev); 
            while (!sr.EndOfStream) {
                string sor = sr.ReadLine();
                string[] adatok = sor.Split(';');
                double besuly = Convert.ToDouble(adatok[2]);
                double beterfogat = Convert.ToDouble(adatok[3]);
                if (csaladicsomagok.ContainsKey(adatok[1])) {
                    csaladicsomag cs = csaladicsomagok[adatok[1]];
                    cs.osszsuly += besuly;
                    cs.osszterfogat += beterfogat;
                    cs.csomagok.Add(new csomag {
                        azon = adatok[0],
                        suly = besuly,
                        terfogat = beterfogat
                    }); 
                    csaladicsomagok[adatok[1]] = cs; 
                } else {
                    csaladicsomag cs = new csaladicsomag();
                    cs.csaladazon = adatok[1];
                    cs.osszsuly = besuly;
                    cs.osszterfogat = beterfogat;
                    cs.csomagok = new List<csomag>(); 
                    cs.csomagok.Add(new csomag {
                        azon = adatok[0],
                        suly = besuly,
                        terfogat = beterfogat
                    }); 
                    csaladicsomagok.Add(adatok[1], cs);
                }
            } 
            sr.Close();
            return csaladicsomagok;
        }

        static kontener[] initkontener() {
            kontener[] k = new kontener[5]; 
            for (int i = 0; i < 5; i++) {
                k[i] = new kontener();
                k[i].azon = i + 1;
                k[i].erokar = i - 2;
                k[i].suly = 0;
                k[i].terfogat = 0;
                k[i].csomagok = new List<csaladicsomag>();
            }
            return k;
        }

        static bool belefer(kontener k, csaladicsomag cs) {
            return k.suly + cs.osszsuly <= 1500 && k.terfogat + cs.osszterfogat <= 6.0;
        }

        static string legnehezebbk(Dictionary<string, csaladicsomag> cs, int k) {
            List<csaladicsomag> lista = new List<csaladicsomag>();
            foreach (var elem in cs) {
                lista.Add(elem.Value);
            }
            for (int i = 0; i < lista.Count - 1; i++) {
                for (int j = i + 1; j < lista.Count; j++) {
                    if (lista[i].osszsuly < lista[j].osszsuly) {
                        csaladicsomag temp = lista[i];
                        lista[i] = lista[j];
                        lista[j] = temp;
                    }
                }
            }

            return lista[k].csaladazon;
        }

        static double sulypont(kontener[] k) {
            double szamlalo = 0;
            double nevezo = 0; 
            for (int i = 0; i < k.Length; i++) {
                szamlalo += k[i].suly * k[i].erokar;
                nevezo += k[i].suly;
            }
            if (nevezo == 0) {
                return 0;
            }
            return szamlalo / nevezo;
        }

        static int legkevesbeelter(kontener[] k, csaladicsomag cs) {
            int legjobbindex = -1;
            double legjobb = 1000000;
            for (int i = 0; i < 5; i++) {
                if (belefer(k[i], cs)) {
                    kontener[] seged = new kontener[5];
                    k.CopyTo(seged, 0);
                    seged[i].suly += cs.osszsuly;
                    seged[i].terfogat += cs.osszterfogat;
                    double ujcg = sulypont(seged);
                    if (ujcg < 0) {
                        ujcg = -ujcg;
                    }
                    if (ujcg < legjobb) {
                        legjobb = ujcg;
                        legjobbindex = i;
                    }
                }
            }
            return legjobbindex;
        }

        static kontener[] szimulacio() {
            kontener[] k = initkontener();
            Dictionary<string, csaladicsomag> csaladicsomagok = beolvas("../../csomagok.csv");
            for (int i = 0; i < csaladicsomagok.Count; i++) {
                string csaladid = legnehezebbk(csaladicsomagok, i);
                csaladicsomag cs = csaladicsomagok[csaladid];
                int index = legkevesbeelter(k, cs);
                if (index != -1) {
                    k[index].suly += cs.osszsuly;
                    k[index].terfogat += cs.osszterfogat;
                    k[index].csomagok.Add(cs);
                }
            }
            return k;
        }

        static void Main(string[] args) {
            kontener[] k = szimulacio();
            Console.WriteLine("--- LUFTHANSA JÁRAT RAKODÁSI TERV ---");
            for (int i = 0; i < k.Length; i++) {
                Console.WriteLine("Konténer " + k[i].azon +" (Erőkar: " + k[i].erokar + "): " + Convert.ToString(k[i].suly) + " kg / " + Convert.ToString(k[i].terfogat) + " m^3 - " + k[i].csomagok.Count + " család" );
            }
            double vegso = sulypont(k);
            Console.WriteLine();
            Console.WriteLine("Végső CG: " + Convert.ToString(vegso));
            if (vegso < 0.5 && vegso > -0.5) {
                Console.WriteLine("A gép tökéletes egyensúlyban van. Felszállás engedélyezve!");
            }
            else {
                Console.WriteLine("A gép nincs jó egyensúlyban. A felszállás megtagadva!");
            }

            Console.ReadKey();
        }
    }
}