﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;

namespace GeneticCars
{
    class comparer : IComparer<Element>
    {
        #region IComparer Members

        public int Compare(Element x, Element y)
        {
            if ((x is Element) && (y is Element))
            {
                Element e1 = (Element)x;
                Element e2 = (Element)y;

                if (e1.Cost == e2.Cost) return 0;
                else return e1.Cost > e2.Cost ? 1 : -1;
            }
            else return 0;
        }

        #endregion
    }

    class GenetskiAlgoritmi
    {
        static Bitmap BackgroundImage;
        const int velikost_populacije = 20;
        
        public double BestCost = double.MinValue;
        public bool end = false;
        public int generacija = 0;

        Element NajbolsiElement = null;
        public Element Best { get { return NajbolsiElement; } }

        List<Element> populacija = new List<Element>();

        public GenetskiAlgoritmi()
        {
            BackgroundImage = PlayingGround.field;
        }

        public List<Element> Inicializiraj()
        {
            for (int i = 0; i < velikost_populacije; i++)
            {
                Element p = new Element();
                p.Mutate();
                populacija.Add(p);
            }

            return populacija;
        }

        public List<Element> Reset()
        {
            populacija.Clear();
            populacija = Inicializiraj();

            return populacija;
        }

        public List<Element> Load(string FileName, int count = int.MaxValue)
        {
            List<Element> arr = new List<Element>();

            using (StreamReader sr = new StreamReader(File.OpenRead(FileName)))
            {
                string read = sr.ReadToEnd();

                string[] networks = read.Split(new string[] { "Network" }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < (networks.Length < count ? networks.Length : count); i++)
                {
                    string str = networks[i];

                    Element e = new Element();
                    e.network = new NeuralNetwork.Network(str.Trim());
                    arr.Add(e);
                }
            }

            return arr;
        }

        public void Write(string FileName)
        {
            List<Element> lst = new List<Element>();
            lst.AddRange(populacija);

            lst.Sort(new comparer());

            using (StreamWriter sw = new StreamWriter(File.OpenWrite(FileName)))
            {
                for (int i = lst.Count - 1; i >= 0; i--)
                {
                    Element e = lst[i];
                    e.network.Write(sw);
                }
            }
        }

        void MarkAsBest(Element el)
        {
            if (NajbolsiElement != null)
            {
                NajbolsiElement.setColor(Color.Red, false);
                NajbolsiElement.SetRisiCrte(false);
            }
            
            NajbolsiElement = el;

            NajbolsiElement.setColor(Color.Yellow, true);
            NajbolsiElement.SetRisiCrte(true);

            if (el.Cost > BestCost) BestCost = el.Cost;
        }

        public List<Element> PripraviTekmovalce()
        {
            //pobijemo slabe

            List<Element> lst = new List<Element>();
            lst.AddRange(populacija);

            lst.Sort(new comparer());
            while (populacija.Count > velikost_populacije)
            {
                populacija.Remove(lst[0]);
                lst.RemoveAt(0);
            }

            MarkAsBest((Element)lst[lst.Count - 1]);
            lst = null;

            List<Element> novaPopulacija = new List<Element>();
            
            //Elitest survives
            novaPopulacija.Add(NajbolsiElement);

            for (int k = 0; k < populacija.Count; k++)
            {
                Element trenutni = (Element)populacija[k];

                //Mating:
                //Advanced mating algorithm:
                int st_poskusov = 5;

                Element bestPartner = null;
                for (int i = 0; ((i < st_poskusov) || (bestPartner == null)); i++)
                {
                    Element partner = (Element)populacija[Functions.rand.Next(populacija.Count)];
                    if (((bestPartner == null) || (partner.Cost > bestPartner.Cost)) && (partner != trenutni) && (partner.StRazmnozevanj < Element.MaxRazmnozevanj)) bestPartner = partner;
                }

                int stOtrok = 2 + Functions.rand.Next(5);
                for (int dodaj = 0; dodaj < stOtrok; dodaj++)
                    novaPopulacija.Add(Element.Mate(trenutni, bestPartner));

                bestPartner.StRazmnozevanj++;
            }

            //Resetiramo
            foreach (Element element in novaPopulacija)
            {
                element.Reset();
            }

            populacija = novaPopulacija;

            generacija++;

            return populacija;
        }
    }
}