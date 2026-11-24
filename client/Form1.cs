//interface graphique (Windows Forms) du projet CLIENT, qui appelle le serveur à distance via .NET Remoting
using RemotingInterfaces;                       // DLL partagée : IRemoteOperation + classe etudiant
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Channels;         // ChannelServices
using System.Runtime.Remoting.Channels.Tcp;     // TcpChannel
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;                     // Composants Windows Forms (Form, MessageBox, boutons...)

// Projet CLIENT : application graphique (Windows Forms) qui appelle le serveur à distance
namespace client
{
    public partial class Form1 : Form
    {
        // Référence vers l'objet distant, typée par l'INTERFACE (pas par la classe RemoteOperation,
        // que le client ne connaît pas). En réalité, c'est un PROXY.
        private IRemoteOperation remoteOperation;

        public Form1()
        {
            InitializeComponent();   // Génère les composants du formulaire (boutons, textbox, DataGridView)

            try
            {
                // 1) Création d'un canal TCP CÔTÉ CLIENT.
                // Pas de numéro de port : le client n'écoute pas, il initie seulement la connexion.
                TcpChannel channel = new TcpChannel();

                // 2) Enregistrement du canal (true = sécurité, même valeur que sur le serveur)
                ChannelServices.RegisterChannel(channel, true);

                // 3) Récupération du proxy vers l'objet distant.
                // Activator.GetObject ne fait PAS d'appel réseau tout de suite :
                // il crée juste le proxy. La connexion réelle a lieu au 1er appel de méthode.
                remoteOperation = (IRemoteOperation)Activator.GetObject(
                    typeof(IRemoteOperation),                  // Type attendu (l'interface partagée)
                    "tcp://localhost:1069/RemoteOperation");   // URL publiée par le serveur
            }
            catch
            {
                MessageBox.Show("Erreur de connexion au serveur");
            }
        }

        // Méthode utilitaire : vide les trois zones de texte après une opération réussie
        private void ViderChamps()
        {
            txtMatricule.Clear();
            txtNom.Clear();
            txtPrenom.Clear();
        }

        // Méthode utilitaire : valide la saisie et construit un objet etudiant.
        // Retourne null si un champ est invalide.
        private etudiant LireChamps()
        {
            int matricule;

            // TryParse : tente de convertir le texte en int SANS lever d'exception.
            // Retourne false si ce n'est pas un nombre.
            // Trim() : supprime les espaces au début et à la fin.
            if (!int.TryParse(txtMatricule.Text, out matricule) ||
                txtNom.Text.Trim() == "" ||
                txtPrenom.Text.Trim() == "")
            {
                MessageBox.Show("Veuillez remplir correctement tous les champs.");
                return null;
            }

            // Utilise le constructeur avec paramètres de la classe etudiant
            return new etudiant(matricule, txtNom.Text.Trim(), txtPrenom.Text.Trim());
        }

        // ------------------------------------------------------------
        // Bouton Ajouter
        // ------------------------------------------------------------
        private void btnAjouter_Click(object sender, EventArgs e)
        {
            etudiant etu = LireChamps();      // Lecture + validation des champs
            if (etu == null) return;          // Saisie invalide → on arrête

            // Appel DISTANT : l'objet etudiant est sérialisé et envoyé au serveur,
            // qui exécute l'INSERT dans Oracle et renvoie true/false.
            if (remoteOperation.AjouterEtudiant(etu))
            {
                MessageBox.Show("Étudiant ajouté.");
                ViderChamps();
            }
            else
                MessageBox.Show("Échec de l'ajout (matricule déjà existant ?).");
        }

        // ------------------------------------------------------------
        // Bouton Modifier
        // ------------------------------------------------------------
        private void btnModifier_Click(object sender, EventArgs e)
        {
            etudiant etu = LireChamps();
            if (etu == null) return;

            // Le serveur fait un UPDATE en cherchant l'étudiant par son matricule
            if (remoteOperation.ModifierEtudiant(etu))
            {
                MessageBox.Show("Étudiant modifié.");
                ViderChamps();
            }
            else
                MessageBox.Show("Échec de la modification (matricule introuvable ?).");
        }

        // ------------------------------------------------------------
        // Bouton Supprimer
        // ------------------------------------------------------------
        private void btnSupprimer_Click(object sender, EventArgs e)
        {
            int matricule;

            // Pour supprimer, seul le matricule est nécessaire (pas nom/prénom)
            if (!int.TryParse(txtMatricule.Text, out matricule))
            {
                MessageBox.Show("Sélectionnez un étudiant ou saisissez son matricule.");
                return;
            }

            // Boîte de confirmation Oui/Non avant l'opération (sécurité pour l'utilisateur)
            if (MessageBox.Show("Supprimer l'étudiant " + matricule + " ?", "Confirmation",
                MessageBoxButtons.YesNo) != DialogResult.Yes) return;

            if (remoteOperation.SupprimerEtudiant(matricule))
            {
                MessageBox.Show("Étudiant supprimé.");
                ViderChamps();
            }
            else
                MessageBox.Show("Échec de la suppression.");
        }

        // ------------------------------------------------------------
        // Bouton Afficher
        // ------------------------------------------------------------
        private void btnAfficher_Click(object sender, EventArgs e)
        {
            // On met d'abord DataSource à null pour forcer le rafraîchissement de la grille
            dgvEtudiants.DataSource = null;

            // Appel distant : le serveur renvoie List<etudiant> (sérialisée),
            // qui est directement liée au DataGridView (colonnes = propriétés de etudiant).
            dgvEtudiants.DataSource = remoteOperation.AfficherEtudiants();
        }
    }
}