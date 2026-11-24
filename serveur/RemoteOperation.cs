//A ADAPTER

//les methodes du serveur qui sont appelées à distance par le client via .NET Remoting
using System;
using System.Collections.Generic;          // Pour List<T>
using Oracle.ManagedDataAccess.Client;     // Pilote ADO.NET pour Oracle (OracleConnection, OracleCommand...)
using RemotingInterfaces;                  // La DLL partagée : interface IRemoteOperation + classe etudiant

namespace Serveur
{
    // MarshalByRefObject : rend l'objet "appelable à distance".
    //   Le client ne reçoit pas une copie, mais un PROXY : les méthodes
    //   s'exécutent sur le SERVEUR (là où se trouve la base Oracle).
    // IRemoteOperation : la classe respecte le contrat et implémente
    //   toutes les méthodes déclarées dans l'interface.
    public class RemoteOperation : MarshalByRefObject, IRemoteOperation
    {
        // Chaîne de connexion Oracle :
      
        // "const" : valeur fixe, non modifiable pendant l'exécution.
        private const string ORADB =
           "Data Source=localhost:1521/XE; User Id=oracle; Password=manager;";

        // Méthode utilitaire privée : évite de répéter le code d'ouverture
        // de connexion dans chaque méthode (principe DRY).
        private OracleConnection OuvrirConnexion()
        {
            OracleConnection conn = new OracleConnection(ORADB); // Crée l'objet connexion
            conn.Open();                                         // Ouvre réellement la connexion
            return conn;                                         // Retourne la connexion ouverte
        }

        // ------------------------------------------------------------
        // 1) Tester la connexion
        // ------------------------------------------------------------
        public string Connect()
        {
            try
            {
                // "using" : ferme et libère AUTOMATIQUEMENT la connexion
                // à la fin du bloc, même en cas d'erreur.
                using (OracleConnection conn = OuvrirConnexion())
                {
                    return conn.State.ToString();   // Retourne "Open" si la connexion fonctionne
                }
            }
            catch (OracleException e)
            {
                return e.Message;                    // Erreur Oracle (mauvais mot de passe, service arrêté...)
            }
        }

        // ------------------------------------------------------------
        // 2) Ajouter un étudiant (INSERT)
        // ------------------------------------------------------------
        public bool AjouterEtudiant(etudiant e)
        {
            try
            {
                using (OracleConnection conn = OuvrirConnexion())
                {
                    // Requête paramétrée : ":mat", ":nom", ":prenom" sont des
                    // PARAMÈTRES (espaces réservés), remplis plus bas.
                    string sql = "INSERT INTO Etudiant (matricule, nom, prenom) " +
                                 "VALUES (:mat, :nom, :prenom)";

                    // OracleCommand = la requête SQL + la connexion à utiliser
                    using (OracleCommand cmd = new OracleCommand(sql, conn))
                    {
                        // Par défaut, Oracle associe les paramètres par POSITION.
                        // BindByName = true → il les associe par NOM (plus sûr).
                        cmd.BindByName = true;

                        // Remplissage des paramètres avec les valeurs de l'objet etudiant
                        cmd.Parameters.Add("mat", e.matricule);
                        cmd.Parameters.Add("nom", e.nom);
                        cmd.Parameters.Add("prenom", e.prenom);

                        // ExecuteNonQuery : pour INSERT / UPDATE / DELETE.
                        // Retourne le nombre de lignes affectées.
                        // > 0 signifie que l'insertion a réussi → true.
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Affiche l'erreur côté serveur (ex : matricule déjà existant)
                Console.WriteLine("Erreur AjouterEtudiant : " + ex.Message);
                return false;   // Le client reçoit false = échec
            }
        }

        // ------------------------------------------------------------
        // 3) Modifier un étudiant (UPDATE) : on cherche par le MATRICULE
        // ------------------------------------------------------------
        public bool ModifierEtudiant(etudiant e)
        {
            try
            {
                using (OracleConnection conn = OuvrirConnexion())
                {
                    // On modifie nom et prénom de l'étudiant dont le matricule correspond.
                    // Le matricule est la clé : on ne le modifie pas, on s'en sert pour CHERCHER.
                    string sql = "UPDATE Etudiant SET nom = :nom, prenom = :prenom " +
                                 "WHERE matricule = :mat";

                    using (OracleCommand cmd = new OracleCommand(sql, conn))
                    {
                        cmd.BindByName = true;   // Association par nom (l'ordre n'a donc plus d'importance)
                        cmd.Parameters.Add("nom", e.nom);
                        cmd.Parameters.Add("prenom", e.prenom);
                        cmd.Parameters.Add("mat", e.matricule);

                        // > 0 : au moins une ligne modifiée.
                        // Si le matricule n'existe pas → 0 ligne → false.
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur ModifierEtudiant : " + ex.Message);
                return false;
            }
        }

        // ------------------------------------------------------------
        // 4) Supprimer un étudiant (DELETE)
        // ------------------------------------------------------------
        public bool SupprimerEtudiant(int matricule)
        {
            try
            {
                using (OracleConnection conn = OuvrirConnexion())
                {
                    // Suppression de la ligne ayant ce matricule
                    string sql = "DELETE FROM Etudiant WHERE matricule = :mat";

                    using (OracleCommand cmd = new OracleCommand(sql, conn))
                    {
                        cmd.BindByName = true;
                        cmd.Parameters.Add("mat", matricule);
                        return cmd.ExecuteNonQuery() > 0;   // true si une ligne a été supprimée
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur SupprimerEtudiant : " + ex.Message);
                return false;
            }
        }

        // ------------------------------------------------------------
        // 5) Afficher tous les étudiants (SELECT)
        // ------------------------------------------------------------
        public List<etudiant> AfficherEtudiants()
        {
            // Liste vide créée au départ : si une erreur survient,
            // on retourne quand même une liste (vide) et non null.
            List<etudiant> liste = new List<etudiant>();
            try
            {
                using (OracleConnection conn = OuvrirConnexion())
                {
                    // ORDER BY : trie les résultats par matricule croissant
                    string sql = "SELECT matricule, nom, prenom FROM Etudiant ORDER BY matricule";

                    using (OracleCommand cmd = new OracleCommand(sql, conn))
                    // ExecuteReader : pour un SELECT. Retourne un DataReader
                    // qui lit les résultats ligne par ligne (lecture seule, vers l'avant).
                    using (OracleDataReader dr = cmd.ExecuteReader())
                    {
                        // dr.Read() avance à la ligne suivante, et retourne false à la fin
                        while (dr.Read())
                        {
                            etudiant etu = new etudiant();
                            // Convert.ToInt32 : Oracle renvoie un type NUMBER → conversion en int
                            etu.matricule = Convert.ToInt32(dr["matricule"]);
                            etu.nom = dr["nom"].ToString();
                            etu.prenom = dr["prenom"].ToString();
                            liste.Add(etu);   // Ajoute l'étudiant à la liste
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur AfficherEtudiants : " + ex.Message);
            }
            // La liste est renvoyée au client (sérialisée, car etudiant est [Serializable])
            return liste;
        }
    }
}