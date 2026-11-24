//A ADAPTER

// declaration de l objet et le constructeur 
using System;  // Nécessaire pour utiliser l'attribut [Serializable]

// Namespace partagé entre le client et le serveur (projet "Interfaces" / DLL commune)
namespace RemotingInterfaces
{
    // [Serializable] : indique que les objets de cette classe peuvent être
    // convertis en flux d'octets (sérialisation) pour être envoyés à travers
    // le réseau, puis reconstruits de l'autre côté (désérialisation).
    // C'est INDISPENSABLE en Remoting pour transmettre un objet "par valeur".
    [Serializable]
    public class etudiant
    {
        // Propriétés (get/set) : représentent les données d'un étudiant
        public int matricule { get; set; }   // Identifiant unique de l'étudiant
        public string nom { get; set; }      // Nom de famille
        public string prenom { get; set; }   // Prénom

        // Constructeur SANS paramètres (vide)
        // OBLIGATOIRE pour la sérialisation : le framework en a besoin
        // pour recréer l'objet lors de la désérialisation.
        public etudiant() { }

        // Constructeur AVEC paramètres : permet de créer et d'initialiser
        // un étudiant en une seule ligne, ex: new etudiant(1, "Ali", "Ahmed")
        public etudiant(int matricule, string nom, string prenom)
        {
            // "this" désigne l'objet courant : il différencie l'attribut
            // de la classe (this.nom) du paramètre du constructeur (nom)
            this.matricule = matricule;
            this.nom = nom;
            this.prenom = prenom;
        }
    }
}
