using System;

namespace ExclusionMutuelle
{
    //Implémentation de la classe décrivant les messages nécessaires pour le fonctionnement de l'algorithme de Lamport
    public class Message
    {
        //Le type du message
            // 0 : Req
            // 1 : Rel
            // 2 : Acq
            // 3 : Absent
            // 4 : Rentrée
            // 5 : Hello [utilisés par le détecteur]
        public int Type { get; set; }

        //L'horloge logique scalaire
        public int Horloge { get; set; }

        //Le numéro du site
        public int Site { get; set; }

        //Constructeur de messages
        public Message(int t, int h, int s)
        {
            Type = t;
            Horloge = h;
            Site = s;
        }

        //Constructeur de messages
        public Message(string s)
        {
            Type = Convert.ToInt32(s.Split(';')[0]);
            Horloge = Convert.ToInt32(s.Split(';')[1]);
            Site = Convert.ToInt32(s.Split(';')[2]);
        }

        //Conversion d'un message en une chaine de caractère
        public override string ToString()
        {
            return Convert.ToString(Type) + ";" + Convert.ToString(Horloge) + ";" + Convert.ToString(Site);
        }
    }
}
