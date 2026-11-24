//A NE PAS TOUCHER

// Projet SERVEUR : programme principal qui démarre le serveur .NET Remoting et expose l'objet distant RemoteOperation aux clients. contient la méthode Main() qui est le point d'entrée de l'application.
using System;
using System.Runtime.Remoting;                // Classes de configuration Remoting (RemotingConfiguration, WellKnownObjectMode)
using System.Runtime.Remoting.Channels;       // ChannelServices : gestion des canaux de communication
using System.Runtime.Remoting.Channels.Tcp;   // TcpChannel : canal basé sur le protocole TCP

// Projet SERVEUR : c'est lui qui héberge l'objet distant et attend les clients
namespace Serveur
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // 1) Création d'un canal TCP qui écoute sur le port 1069.
                // C'est la "porte d'entrée" par laquelle les clients se connectent.
                TcpChannel channel = new TcpChannel(1069);

                // 2) Enregistrement du canal auprès du système Remoting.
                // Le 2e paramètre (true) active la sécurité (ensureSecurity).
                ChannelServices.RegisterChannel(channel, true);

                // 3) Enregistrement de la classe RemoteOperation comme service distant
                // "well-known" (connu à l'avance par une URL).
                RemotingConfiguration.RegisterWellKnownServiceType(
                    typeof(RemoteOperation),          // Type de l'objet distant à exposer
                    "RemoteOperation",                // URI : nom public de l'objet (partie finale de l'URL)
                    WellKnownObjectMode.Singleton);   // Mode d'activation : UNE seule instance partagée par tous les clients

                // Affiche l'URL que les clients devront utiliser
                Console.WriteLine("Serveur démarré  ...");

                // 4) Création d'une instance LOCALE (indépendante de celle de Remoting)
                // uniquement pour tester la connexion à Oracle au démarrage.
                RemoteOperation op = new RemoteOperation();
                Console.WriteLine("État connexion Oracle : " + op.Connect());

                // 5) Garde le serveur en vie : sans ReadLine(), le programme
                // se terminerait immédiatement et le canal serait fermé.
                Console.WriteLine("Appuyez sur Entrée pour arrêter le serveur.");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                // Affiche l'erreur (ex : port déjà utilisé) et attend avant de fermer
                Console.WriteLine("Erreur serveur : " + ex.Message);
                Console.ReadLine();
            }
        }
    }
}