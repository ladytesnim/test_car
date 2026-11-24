//A ADAPTER

//signature des methodes
using System.Collections.Generic;  // Nécessaire pour utiliser List<T>

// Même namespace que la classe etudiant : cette interface fait partie
// de la DLL partagée entre le client et le serveur
namespace RemotingInterfaces
{
    // Interface = "contrat" : elle liste les méthodes que le serveur
    // s'engage à fournir, SANS donner leur code (pas d'implémentation).
    // Le serveur l'implémente, le client l'utilise.
    public interface IRemoteOperation
    {
       

        // ---------- Gestion des étudiants (opérations CRUD) ----------

        // CREATE : ajoute un étudiant dans la base.
        // Retourne true si l'insertion a réussi, false sinon.
        bool AjouterEtudiant(etudiant e);

        // UPDATE : modifie les informations d'un étudiant existant
        // (identifié par son matricule). Retourne true/false.
        bool ModifierEtudiant(etudiant e);

        // DELETE : supprime l'étudiant qui a ce matricule.
        // On passe seulement le matricule (clé unique), pas tout l'objet.
        bool SupprimerEtudiant(int matricule);

        // READ : retourne la liste de tous les étudiants de la base.
        List<etudiant> AfficherEtudiants();
    }
}